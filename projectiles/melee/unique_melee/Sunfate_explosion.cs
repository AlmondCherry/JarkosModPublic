using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
namespace jarkos.projectiles.melee.unique_melee
{
    internal class Sunfate_explosion : ModProjectile
    {
		public override void SetStaticDefaults()
		{
			Main.projFrames[Projectile.type] = 8;
		}
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.MeteorShot);
			AIType = ProjectileID.MeteorShot;
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.penetrate = 99;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.ownerHitCheck = true;
            Projectile.extraUpdates = 1;
            Projectile.timeLeft = 90;
            Projectile.scale = 1;
            Projectile.light = 1;
        }
        public override void AI()
        {
        Projectile.velocity *= 1f;
        Terraria.Player player = Main.player[Projectile.owner];
         ++Projectile.frameCounter;
        if (Projectile.frameCounter == 10 || Projectile.frameCounter == 20 || Projectile.frameCounter == 30 || Projectile.frameCounter == 40 || this.Projectile.frameCounter == 50 || Projectile.frameCounter == 60 || Projectile.frameCounter == 70 || Projectile.frameCounter == 80)
        {
            ++Projectile.frame;
        }
        if (Projectile.frame >= 8) {
            Projectile.Kill();
        }
      }
    }
    }
