using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
namespace jarkos.projectiles.ranged.firearm.beamrifle
{
    internal class Red_beam_burst : ModProjectile
    {
		public override void SetStaticDefaults()
		{
			Main.projFrames[base.Projectile.type] = 5;
		}
        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.MeteorShot);
			AIType = ProjectileID.MeteorShot;
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.friendly = true;
			Projectile.hostile = false;
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
        this.Projectile.direction = npc.direction;
        this.Projectile.rotation = this.Projectile.velocity.ToRotation();
         ++this.Projectile.frameCounter;
        if (this.Projectile.frameCounter == 10 || this.Projectile.frameCounter == 20 || this.Projectile.frameCounter == 30 || this.Projectile.frameCounter == 40 || this.Projectile.frameCounter == 50)
        {
            ++this.Projectile.frame;
        }
        if (this.Projectile.frame >= 5) {
            this.Projectile.Kill();
        }
      }
    }
    }
