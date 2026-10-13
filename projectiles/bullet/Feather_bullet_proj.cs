using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using jarkos.particles;

namespace jarkos.projectiles.bullet
{
	public class Feather_bullet_proj : ModProjectile
	{
		public override void SetStaticDefaults()
        {
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
		}

		public override void SetDefaults()
        {
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.ignoreWater = true;
			Projectile.light = 0.6f;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 600;
			Projectile.penetrate = 3;
			Projectile.extraUpdates = 1;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

			if (Main.rand.NextBool(3))
			{
				Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.WhiteTorch, 0f, 0f, 100, default, 1.1f);
				d.noGravity = true;
				d.velocity *= 0.2f;
			}
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			if (target.velocity.LengthSquared() > 0.25f)
			{
				modifiers.SourceDamage *= 1.10f;
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (Main.rand.NextFloat() < 0.05f)
			{
				Vector2 pushVel = Projectile.velocity.SafeNormalize(Vector2.UnitX) * 5f;
				target.velocity += pushVel;

				SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.4f, Pitch = 0.4f }, target.Center);
			}

			Vector2 coneDir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
			int sparkCount = Main.rand.Next(6, 11);

			for (int i = 0; i < sparkCount; i++)
			{
				float angleOffset = Main.rand.NextFloat(-MathHelper.ToRadians(35f), MathHelper.ToRadians(35f));
				float speed = Main.rand.NextFloat(3f, 8f);
				Vector2 sparkVel = coneDir.RotatedBy(angleOffset) * speed;

				new Spark(target.Center, sparkVel, Color.White * 0.95f, Main.rand.Next(15, 30), Main.rand.NextFloat(0.4f, 0.85f), 1f, Main.rand.NextFloat(-0.1f, 0.1f)).Spawn();
			}

			for (int i = 0; i < 3; i++)
			{
				Vector2 speed = Main.rand.NextVector2CircularEdge(1.5f, 1.5f);
				Dust d = Dust.NewDustPerfect(Projectile.position, DustID.WhiteTorch, speed, Scale: 1.5f);
				d.noGravity = true;
			}
		}

		public override void OnKill(int timeLeft)
		{
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
		}
	}
}
