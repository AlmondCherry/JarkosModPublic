using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
namespace jarkos.projectiles.magic.other
{
	public class Meteorite_explosion : ModProjectile
	{
		public override void SetStaticDefaults() 
        {
			Main.projFrames[Projectile.type] = 7;
		}
		public override void SetDefaults() 
        {
			Projectile.CloneDefaults(ProjectileID.Bullet);
            AIType = ProjectileID.Bullet;
            Projectile.tileCollide = false;
            Projectile.scale = 1.25f;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.damage = 30;
            Projectile.timeLeft = 58;
            Projectile.light = 0.5f;
		}
        public override void AI()
        {
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;
                if (!Main.dedServ)
                {
                    for(int i = 0; i < 10; i++)
                    {
                        Vector2 vct = Main.rand.NextVector2CircularEdge(8f, 8f);
                        float size = Main.rand.NextFloat(0.2f, 0.5f);
                        new Dust1_particle(Projectile.Center, vct, Color.DarkOrange, 60, size, 1f, 0f).Spawn();
                        new Circular_glow(Projectile.Center, vct, Color.Orange, 80, 0.6f, 1f, 0f).Spawn();
                    }
                    SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/short_burst_sound"), Projectile.position);
                }
            }

            ++Projectile.frameCounter;
            
            if (Projectile.frameCounter == 8 || Projectile.frameCounter == 16 || Projectile.frameCounter == 24 || Projectile.frameCounter == 32 || Projectile.frameCounter == 40 || Projectile.frameCounter == 48 || Projectile.frameCounter == 56)
            {
                ++Projectile.frame;
            }

            if (Projectile.frameCounter >= 56) 
            {
                Projectile.frameCounter = 0;
                Projectile.frame = 0;
            }
        }
    }
}
