using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.ranged.firearm.beamrifle
{
    public class Green_beam_spore_smoke : ModProjectile
    {
        public override void SetStaticDefaults()
        {
        }
        public override void SetDefaults()
        {
            Projectile.tileCollide = false;
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.ignoreWater = true;
			Projectile.light = 1f;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 225;
			Projectile.penetrate = 1;
			Projectile.alpha = 0;
        }
        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 0f, 1f, 0.1f);
            Projectile.velocity *= 0.98f;
            Projectile.rotation += 0.030f * Projectile.velocity.Length();
            Projectile.spriteDirection = Projectile.direction = (Projectile.velocity.X > 0).ToDirectionInt();
            if(Projectile.timeLeft < 60)
            {
                Projectile.alpha += 20;
                if(Projectile.alpha > 250)
                {
                    Projectile.Kill();
                }
            }
        }
        public override void OnKill(int timeLeft)
        {
             for (int i = 0; i < 15; i++) {
			Vector2 speed = Main.rand.NextVector2Circular(2f, 2f);
			Dust d = Dust.NewDustPerfect(Projectile.position, 39, speed * 2.5f, Scale: 1f);
			d.noGravity = true;
        	}
        }
    }
}
