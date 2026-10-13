using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
namespace Jarkos.projectiles.mecha
{
    internal class Pierc_alt_aura : ModProjectile
    {
		public override void SetStaticDefaults()
		{
			Main.projFrames[Projectile.type] = 3;
		}
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.MeteorShot);
			AIType = ProjectileID.MeteorShot;
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Default;
            Projectile.ownerHitCheck = true;
            Projectile.extraUpdates = 1;
            Projectile.timeLeft = 40;
            Projectile.scale = 1.5f;
            Projectile.light = 1f;
        }
        public override void AI()
        {
        Projectile.velocity *= 2f;
        Player player = Main.player[Projectile.owner];
        Projectile.direction = player.direction;
        Projectile.rotation = Projectile.velocity.ToRotation();
         ++Projectile.frameCounter;
        if (Projectile.frameCounter == 15 || Projectile.frameCounter == 30 || Projectile.frameCounter == 45)
        {
            ++Projectile.frame;
        }
        if (Projectile.frame >= 3) {
            Projectile.Kill();
        }
      }
    }
    }
