using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
namespace jarkos.projectiles.melee.other_melee
{
    internal class Frozen_explode : ModProjectile
    {
		public override void SetStaticDefaults()
		{
			Main.projFrames[Projectile.type] = 4;
		}
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.MeteorShot);
			AIType = ProjectileID.MeteorShot;
            Projectile.width = 38;
            Projectile.height = 36;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.ownerHitCheck = true;
            Projectile.extraUpdates = 1;
            Projectile.timeLeft = 60;
            Projectile.scale = 1f;
        }
        public override void AI()
        {
        Projectile.velocity *= 0f;
        Terraria.Player player = Main.player[this.Projectile.owner];
        this.Projectile.direction = player.direction;
        this.Projectile.rotation = this.Projectile.velocity.ToRotation();
         ++this.Projectile.frameCounter;
        if (this.Projectile.frameCounter == 10 || this.Projectile.frameCounter == 20 || this.Projectile.frameCounter == 30 || this.Projectile.frameCounter == 40)
        {
            ++this.Projectile.frame;
        }
        if (this.Projectile.frame >= 4) {
            this.Projectile.Kill();
        }
      }
    }
    }
