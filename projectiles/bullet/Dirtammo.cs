using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace jarkos.projectiles.bullet
{
	public class Dirtammo : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.CloneDefaults(ProjectileID.Bullet);
			AIType = ProjectileID.Bullet;
			Projectile.width = 6;
			Projectile.height = 6;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.timeLeft = 600;
			Projectile.penetrate = 1;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			SpawnDirtPieces(target);
		}

		private void SpawnDirtPieces(NPC target)
		{
			if (Projectile.owner != Main.myPlayer) return;

			int piecesCount = Main.rand.Next(2, 4);
			int pieceDamage = (int)(Projectile.damage * 0.5f);
			if (pieceDamage < 1) pieceDamage = 1;

			for (int i = 0; i < piecesCount; i++)
			{
				Vector2 vel = Main.rand.NextVector2Circular(3.5f, 3.5f) + new Vector2(0f, -2.5f);
				Projectile.NewProjectile(Projectile.GetSource_OnHit(target), Projectile.Center, vel, ModContent.ProjectileType<Dirtammo02>(), pieceDamage, Projectile.knockBack * 0.5f, Projectile.owner);
			}
		}

		public override void OnKill(int timeLeft)
		{
			for (int i = 0; i < 5; i++) 
			{
				Vector2 speed = Main.rand.NextVector2Circular(4f, 4f);
				Dust.NewDustPerfect(Projectile.Center, DustID.Dirt, speed, Scale: 1f);
			}

			Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
		}
	}
}
