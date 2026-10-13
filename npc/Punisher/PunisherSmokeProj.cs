using Terraria;
using Terraria.ModLoader;

namespace jarkos.npc.Punisher
{
    public class PunisherSmokeProj : ModProjectile
    {
        public override string Texture => "jarkos/npc/Punisher/smoke_burst";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 5;
        }
        public override void SetDefaults()
        {
            Projectile.width = 140;
            Projectile.height = 140;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 60;
            Projectile.alpha = 50;
        }
        public override void AI()
        {
            Projectile.velocity *= 0.95f;
            if (++Projectile.frameCounter >= 5)
            {
                Projectile.frameCounter = 0;
                if (++Projectile.frame >= Main.projFrames[Projectile.type])
                {
                    Projectile.frame = Main.projFrames[Projectile.type] - 1;
                }
            }
            if (Projectile.timeLeft < 20)
            {
                Projectile.alpha += 10;
            }
        }
    }
}
