using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
namespace jarkos.projectiles.magic.scrolls
{
    public class Fire_ball_explosion : ModProjectile
    {
		public override void SetStaticDefaults()
		{
			Main.projFrames[Projectile.type] = 5;
		}
        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 100;
            Projectile.height = 100;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.localNPCHitCooldown = 10;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.extraUpdates = 1;
            Projectile.timeLeft = 60;
            Projectile.scale = 0.75f;
        }
        public override void AI()
        {
            Projectile.velocity *= 0f;
            Player player = Main.player[Projectile.owner];
            Projectile.direction = player.direction;
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 4)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
            }
            if (Projectile.frame >= 5) {
                Projectile.Kill();
            }
        }
    }
}
