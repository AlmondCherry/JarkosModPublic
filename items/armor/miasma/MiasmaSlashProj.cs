using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.armor.miasma
{
	public class MiasmaSlashProjectile : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.ShadowBeamFriendly;
		private const int LifeTime = 14;
		private float Rot => Projectile.ai[0];
		private float Seed => Projectile.ai[1];
		public override void SetDefaults()
		{
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Generic;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = LifeTime;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}
		private float Progress => 1f - Projectile.timeLeft / (float)LifeTime;
		private float LengthNow
		{
			get
			{
				float open = MathHelper.Clamp(Progress * 4.5f, 0f, 1f);
				open = 1f - (1f - open) * (1f - open);
				return 110f * open;
			}
		}
		private float Fade => MathHelper.Clamp((1f - Progress) * 2.2f, 0f, 1f);
		public override void AI()
		{
			Projectile.rotation = Rot;
			Lighting.AddLight(Projectile.Center, 0.5f * Fade, 0.2f * Fade, 0.9f * Fade);

			if (Main.rand.NextBool(3))
			{
				Vector2 along = Rot.ToRotationVector2() * Main.rand.NextFloat(-LengthNow, LengthNow) * 0.5f;
				Dust d = Dust.NewDustPerfect(Projectile.Center + along, DustID.PurpleTorch, Rot.ToRotationVector2().RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-1.5f, 1.5f), 0, default, 0.9f);
				d.noGravity = true;
			}
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float _ = 0f;
			Vector2 dir = Rot.ToRotationVector2() * LengthNow * 0.5f;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center - dir, Projectile.Center + dir, 8f, ref _);
		}
		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D pixel = TextureAssets.MagicPixel.Value;
			Vector2 pos = Projectile.Center - Main.screenPosition;
			
			Rectangle src = new(0, 0, 1, 1);
			Vector2 origin = new(0.5f, 0.5f);

			float len = LengthNow;
			float fade = Fade;
			
			float hue = (Main.GlobalTimeWrappedHourly * 0.35f + Seed) % 1f;
			Color cosmic = Main.hslToRgb(MathHelper.Lerp(0.68f, 0.85f, hue), 1f, 0.6f);
			Color deep = new Color(60, 10, 120);

			Main.EntitySpriteDraw(pixel, pos, src, deep * 0.55f * fade, Rot, origin, new Vector2(len * 1.15f, 7f), SpriteEffects.None);
			Main.EntitySpriteDraw(pixel, pos, src, cosmic * 0.85f * fade, Rot, origin, new Vector2(len, 3.2f), SpriteEffects.None);
			Main.EntitySpriteDraw(pixel, pos, src, Color.White * fade, Rot, origin, new Vector2(len * 0.92f, 1.3f), SpriteEffects.None);

			Vector2 tip = Rot.ToRotationVector2() * len * 0.5f;

			for (int i = -1; i <= 1; i++)
			{
				float s = (i == 0 ? 0.55f : 0.35f) * fade;
				Main.EntitySpriteDraw(pixel, pos + tip * i, null, cosmic * fade, Rot, pixel.Size() * 0.5f, s, SpriteEffects.None);
			}
			return false;
		}
	}
}
