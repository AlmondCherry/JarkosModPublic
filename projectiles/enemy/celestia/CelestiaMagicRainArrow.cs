using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.enemy.celestia
{
	public class CelestiaMagicRainArrow : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowRodBullet;
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}
		public override void SetDefaults()
		{
			Projectile.width = 14;
			Projectile.height = 34;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 240;
			Projectile.extraUpdates = 1;
		}
		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			Projectile.velocity.Y += 0.05f;
			Lighting.AddLight(Projectile.Center, 0.35f, 0.35f, 0.95f);
			if (!Main.dedServ && Main.rand.NextBool(3))
			{
				Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch, -Projectile.velocity * 0.08f, 0, default, 1.25f);
				dust.noGravity = true;
			}
		}
		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Vector2 origin = texture.Size() / 2f;
			for (int i = 0; i < Projectile.oldPos.Length; i++)
			{
				float progress = (float)(Projectile.oldPos.Length - i) / Projectile.oldPos.Length;
				Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
				Main.EntitySpriteDraw(texture, drawPos, null, new Color(100, 160, 255, 0) * progress * 0.55f, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
			}
			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
			return false;
		}
	}
}
