using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.npc.godofdeath
{
	public class God_of_d_slimeproj : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			Main.projFrames[Projectile.type] = 2;
		}
		public override void SetDefaults()
		{
			Projectile.netImportant = true;
			Projectile.CloneDefaults(ProjectileID.MeteorShot);
			AIType = ProjectileID.MeteorShot;
			Projectile.width = 30;
			Projectile.height = 30;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.ignoreWater = true;
			Projectile.light = 0.5f;
			Projectile.tileCollide = true;
			Projectile.timeLeft = 300;
			Projectile.penetrate = 1;
		}
        public override void AI()
        {
			Projectile.velocity.Y += 0.22f;
			if (Projectile.velocity.Length() > 0.1f)
			{
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			}
			if (Main.rand.NextBool(2))
			{
				Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Wraith, 0f, 0f, 100, default, 1.1f);
				d.velocity *= 0.1f;
				d.noGravity = true;
			}
        }
		public override void OnKill(int timeLeft)
		{
			SoundEngine.PlaySound(SoundID.NPCDeath1 with { Pitch = -0.2f, Volume = 0.8f }, Projectile.position);
			
			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				int numDroplets = Main.rand.Next(4, 7);
				for (int i = 0; i < numDroplets; i++)
				{					Vector2 vel = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-6f, -3f));

					int proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, vel, ModContent.ProjectileType<Minislimeproj>(), Projectile.damage / 2, 1f, Main.myPlayer);

					if (proj >= 0 && proj < Main.maxProjectiles)
					{
						Main.projectile[proj].netUpdate = true;
					}
				}
			}
			for (int i = 0; i < 15; i++)
			{
				Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Wraith, 0f, 0f, 100, default, 1.3f);
				d.velocity = Main.rand.NextVector2Circular(4f, 4f);
				d.noGravity = true;
			}
		}
    }
}
