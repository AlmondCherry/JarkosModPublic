using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using jarkos.particles;

namespace jarkos.npc.Punisher
{
    public class PunisherSwordProj : ModProjectile
    {
        public override string Texture => "jarkos/npc/Punisher/sword_fly";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 7;
        }
        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 48;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 300;
        }
        public override void AI()
        {
            if (++Projectile.frameCounter >= 4)
            {
                Projectile.frameCounter = 0;
                if (++Projectile.frame >= Main.projFrames[Projectile.type])
                {
                    Projectile.frame = 0;
                }
            }
            if (Main.rand.NextBool(3))
            {
                new Spark(Projectile.Center, Main.rand.NextVector2Circular(2f, 2f), Color.Purple, 20, Main.rand.NextFloat(0.5f, 1.5f), 1f).Spawn();
            }
            Projectile.ai[0]++;
            if (Projectile.ai[0] > 40)
            {
                NPC owner = Main.npc[(int)Projectile.ai[1]];
                if (owner.active && owner.type == ModContent.NPCType<Punisher>())
                {
                    Vector2 returnDir = owner.Center - Projectile.Center;
                    if (returnDir.Length() < 50f)
                    {
                        Projectile.Kill();
                    }
                    else
                    {
                        returnDir.Normalize();
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, returnDir * 24f, 0.08f);
                    }
                }
                else
                {
                    Projectile.Kill();
                }
            }
        }
    }
}
