using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.enemy.celestia
{
	public class CelestiaDashTelegraph : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Starfury;
		public override void SetDefaults()
		{
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.hostile = false;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 60;
			Projectile.alpha = 255;
		}
		public override void AI()
		{
			if (Projectile.localAI[0] == 0f)
			{
				Projectile.localAI[0] = Projectile.ai[1] <= 0f ? 50f : Projectile.ai[1];
				Projectile.timeLeft = (int)Projectile.localAI[0];
				Projectile.netUpdate = true;
			}
			Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitY);
			Lighting.AddLight(Projectile.Center, 0.45f, 0.28f, 0.95f);
			if (!Main.dedServ)
			{
				float progress = 1f - Projectile.timeLeft / Projectile.localAI[0];
				Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitY);
				Vector2 side = direction.RotatedBy(MathHelper.PiOver2);
				for (int i = 0; i < 3; i++)
				{
					float length = Projectile.ai[0] <= 0f ? 1100f : Projectile.ai[0];
					Vector2 position = Projectile.Center + direction * Main.rand.NextFloat(40f, length) + side * Main.rand.NextFloat(-18f, 18f);
					Dust dust = Dust.NewDustPerfect(position, DustID.WhiteTorch, -direction * (1f + progress * 4f), 0, default, 1.1f + progress * 0.9f);
					dust.noGravity = true;
				}
			}
		}
		public override bool? CanDamage()
		{
			return false;
		}
		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D pixel = TextureAssets.MagicPixel.Value;
			Texture2D star = TextureAssets.Projectile[Projectile.type].Value;
			Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.UnitY);

			Vector2 side = direction.RotatedBy(MathHelper.PiOver2);
			float length = Projectile.ai[0] <= 0f ? 1100f : Projectile.ai[0];
			float duration = Projectile.localAI[0] <= 0f ? 50f : Projectile.localAI[0];
			float progress = 1f - Projectile.timeLeft / duration;
			float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.42f) * 0.5f + 0.5f;
			
			Vector2 start = Projectile.Center - Main.screenPosition;
			Color coreColor = Color.Lerp(new Color(130, 70, 255, 0), Color.White, progress * 0.85f) * (0.38f + progress * 0.58f);
			DrawLine(pixel, start, start + direction * length, coreColor, 4f + progress * 8f);
			DrawLine(pixel, start + side * 18f, start + direction * length + side * 18f, new Color(120, 80, 255, 0) * (0.18f + progress * 0.35f), 2f + progress * 4f);
			DrawLine(pixel, start - side * 18f, start + direction * length - side * 18f, new Color(120, 80, 255, 0) * (0.18f + progress * 0.35f), 2f + progress * 4f);
			Vector2 origin = star.Size() / 2f;
			for (int i = 0; i < 7; i++)
			{
				float markerDistance = 120f + i * 150f + Main.GameUpdateCount * (2.5f + progress * 7f) % 150f;
				if (markerDistance < length)
				{
					Vector2 marker = start + direction * markerDistance;
					float markerScale = 0.45f + progress * 0.35f + pulse * 0.08f;
					Main.EntitySpriteDraw(star, marker, null, Color.White * (0.4f + progress * 0.45f), direction.ToRotation() + MathHelper.PiOver2, origin, markerScale, SpriteEffects.None, 0);
					DrawLine(pixel, marker - direction * 32f - side * 18f, marker, coreColor, 3f + progress * 3f);
					DrawLine(pixel, marker - direction * 32f + side * 18f, marker, coreColor, 3f + progress * 3f);
				}
			}
			return false;
		}
		private void DrawLine(Texture2D pixel, Vector2 start, Vector2 end, Color color, float width)
		{
			Vector2 edge = end - start;
			Main.EntitySpriteDraw(pixel, start, new Rectangle(0, 0, 1, 1), color, edge.ToRotation(), new Vector2(0f, 0.5f), new Vector2(edge.Length(), width), SpriteEffects.None, 0);
		}
	}
}
