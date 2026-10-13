using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace jarkos.projectiles.magic.scrolls
{
    internal class Fire_ball01 : ModProjectile
    {

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.MeteorShot);
			      AIType = ProjectileID.MeteorShot;
            Projectile.width = 28;
            Projectile.height = 26;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.tileCollide = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.ownerHitCheck = true;
            Projectile.extraUpdates = 1;
            Projectile.timeLeft = 800;
            Projectile.scale = 1;
        }
        public override void OnKill(int timeLeft)
			  {
            if (Projectile.owner == Main.myPlayer)
            {
		          Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.position.X, Projectile.position.Y, 0, 0, ModContent.ProjectileType<Fire_ball_explosion>(), (int)(Projectile.damage * 0.25), 0f, Projectile.owner, 0f, 0f);
            }
			  }
        public override void AI()
        {
        Projectile.velocity *= 1f;
        Player player = Main.player[Projectile.owner];
        Projectile.direction = player.direction;
        Projectile.rotation = Projectile.velocity.ToRotation();
      }
    }
  }
