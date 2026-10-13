using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace jarkos.projectiles.bullet
{
	public class Dirtammo02 : ModProjectile
	{
		public override void SetStaticDefaults() 
		{
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
		}

		public override void SetDefaults() {
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.timeLeft = 240;
			Projectile.penetrate = 3;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}

		public override void AI()
		{
			Projectile.velocity.Y += 0.25f;
			if (Projectile.velocity.Y > 12f)
			{
				Projectile.velocity.Y = 12f;
			}

			Projectile.rotation += Projectile.velocity.X * 0.08f;

			if (Main.rand.NextBool(5))
			{
				Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Dirt, 0f, 0f, 100, default, 0.8f);
				d.velocity *= 0.2f;
			}
		}

		public override void OnKill(int timeLeft)
		{
			for (int i = 0; i < 5; i++) 
			{
				Vector2 speed = Main.rand.NextVector2Circular(3f, 3f);
				Dust.NewDustPerfect(Projectile.Center, DustID.Dirt, speed, Scale: 1f);
			}

			Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Dig with { Volume = 0.5f }, Projectile.position);
		}
	}
}
