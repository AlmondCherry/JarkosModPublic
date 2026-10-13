using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace jarkos.projectiles.bullet
{
    internal class Mechanical_explosion : ModProjectile
    {
		public override void SetStaticDefaults()
		{
			Main.projFrames[Projectile.type] = 6;
		}
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.MeteorShot);
			AIType = ProjectileID.MeteorShot;
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.ownerHitCheck = true;
            Projectile.light = 1f;
            Projectile.extraUpdates = 1;
            Projectile.timeLeft = 60;
            Projectile.scale = 1f;
        }
        public override void AI()
        {
            Projectile.velocity *= 0f;
            Player player = Main.player[Projectile.owner];

            Projectile.direction = player.direction;
            Projectile.rotation = Projectile.velocity.ToRotation();

            ++Projectile.frameCounter;
            if (Projectile.frameCounter == 10)
            {
                Projectile.frameCounter = 0;
                ++Projectile.frame;
            }
            
            if (Projectile.frame >= 6)
            {
                Projectile.Kill();
            }
        }
    }
}
