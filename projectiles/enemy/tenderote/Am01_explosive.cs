using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
namespace jarkos.projectiles.enemy.tenderote
{
    internal class Am01_explosive : ModProjectile
    {
		public override void SetStaticDefaults()
		{
			Main.projFrames[Projectile.type] = 5;
		}
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.MeteorShot);
			AIType = ProjectileID.MeteorShot;
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ownerHitCheck = true;
            Projectile.extraUpdates = 1;
            Projectile.timeLeft = 60;
            Projectile.scale = 1f;
        }
        public override void AI()
        {
        Projectile.velocity *= 0f;
        NPC npc = Main.npc[Projectile.owner];
        Projectile.direction = npc.direction;
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
