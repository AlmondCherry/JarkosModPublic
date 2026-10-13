using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using jarkos.particles;
namespace jarkos.projectiles.bullet
{
	public class Ar6gl_round : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
		}
		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.Bullet);
			AIType = ProjectileID.Bullet;
			Projectile.width = 2;
			Projectile.height = 2;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.ignoreWater = true;
			Projectile.light = 1f;
			Projectile.tileCollide = true;
			Projectile.timeLeft = 600;
			Projectile.penetrate = 1;
		}
		public override void AI()
		{
			Player player = Main.player[Projectile.owner];
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			for (int i = 0; i < 2; i++)
			{
				float multiplier_speed = Main.rand.NextFloat(1.5f, 2.25f);
				float multiplier_size = Main.rand.NextFloat(0.25f, 0.75f);
				float degrees = 15f;
				Vector2 vel = Projectile.velocity.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-degrees, degrees)));
				new Spark(Projectile.Center, vel * multiplier_speed, Color.Yellow, 300, multiplier_size, 2f, 0f).Spawn();
			}
			for (int a = 0; a < 5; a++)
			{
				float multiplier_speed = Main.rand.NextFloat(1.25f, 2f);
				float multiplier_size = Main.rand.NextFloat(0.75f, 1.5f);
				float degrees = 45f;
				Vector2 vel = Projectile.velocity.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-degrees, degrees)));
				Dust d = Dust.NewDustPerfect(Projectile.position, DustID.Blood, vel * multiplier_speed, Scale: multiplier_size);
				d.noGravity = false;
			}
		}
        public override void OnKill(int timeLeft)
        {
			for (int a = 0; a < 3; a++)
			{
				float multiplier_speed = Main.rand.NextFloat(1.5f, 2.25f);
				float degrees = 25f;
				Vector2 vel = Projectile.velocity.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-degrees, degrees)));
				Dust d = Dust.NewDustPerfect(Projectile.position, DustID.YellowTorch, vel * multiplier_speed, Scale: 0.5f);
				d.noGravity = false;
			}

			Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
            base.OnKill(timeLeft);
        }
    }
}
