using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.true_melee
{
    public class TemporalFissure_anomaly : ModProjectile
    {
        public override void SetStaticDefaults()
        {
          Main.projFrames[Projectile.type] = 12;
        }
        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.scale = 1f;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.light = 1f;
        }
        public override void AI()
        {
            Projectile.ai[0]++;
            ++Projectile.frameCounter;
            if (Projectile.frameCounter == 3)
            {
            ++Projectile.frame;
            Projectile.frameCounter = 0;
            }
            if (Projectile.frame >= 12)
            {
                Projectile.Kill();
            }
            for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    float distance = 600f;
                    if (npc.active && !npc.friendly && !npc.boss && npc.type != NPCID.TargetDummy && npc.Distance(Projectile.Center) < distance)
                    {
                        Vector2 pullDirection = Projectile.Center - npc.Center;
                        npc.velocity += pullDirection * 0.005f;
                        if (Projectile.timeLeft % 15 == 0)
                        {
                            npc.AddBuff(BuffID.Slow, 180);
                            npc.StrikeNPC(new NPC.HitInfo { Damage = Projectile.damage / 3, Knockback = 0f }, true);
                        }
                    }
                }
        }
	}
}
