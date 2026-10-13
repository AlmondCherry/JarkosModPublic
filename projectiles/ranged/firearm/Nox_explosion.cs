using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
namespace jarkos.projectiles.ranged.firearm
{
    internal class Nox_explosion : ModProjectile
    {
		public override void SetStaticDefaults()
		{
			Main.projFrames[base.Projectile.type] = 5;
		}
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.MeteorShot);
			AIType = ProjectileID.MeteorShot;
            Projectile.width = 100;
            Projectile.height = 100;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.damage = 100;
            Projectile.alpha = 125;
            Projectile.ownerHitCheck = true;
            Projectile.extraUpdates = 1;
            Projectile.timeLeft = 60;
            Projectile.scale = 1f;
        }
        public override void AI()
        {
            Projectile.alpha = 125;
            Projectile.velocity *= 0f;
        Terraria.Player player = Main.player[Projectile.owner];
        Projectile.direction = player.direction;
        Projectile.rotation = Projectile.velocity.ToRotation();
         ++Projectile.frameCounter;
        if (Projectile.frameCounter == 10 || Projectile.frameCounter == 20 || Projectile.frameCounter == 30 || Projectile.frameCounter == 40 || Projectile.frameCounter == 50)
        {
            ++Projectile.frame;
        }
        if (Projectile.frame >= 5) {
            Projectile.Kill();
        }
      }
    }
    }
