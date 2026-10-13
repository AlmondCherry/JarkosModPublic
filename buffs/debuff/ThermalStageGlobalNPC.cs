using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.buffs.debuff
{
    public class ThermalStageGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        
        public bool thermalStageDebuff = false;
        public int thermalStageTimer = 0;

        public override void ResetEffects(NPC npc)
        {
            thermalStageDebuff = false;
        }

        public override void PostAI(NPC npc)
        {
            if (thermalStageDebuff && !npc.boss && !Jarkos_npc.IsBossOrBossPart(npc))
            {
                thermalStageTimer++;
                if (thermalStageTimer >= 60)
                {
                    thermalStageTimer = 0;

                    int damageToDeal = Math.Max(1, (int)(npc.lifeMax * 0.02f));

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        npc.SimpleStrikeNPC(damageToDeal, 0, false, 0f, null, false, 0, true);
                    }
                }

                if (Main.rand.NextBool(3))
                {
                    Dust d = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Firework_Red, 0f, -1f, 100, default, 1.1f);
                    d.noGravity = true;
                }
                if (Main.rand.NextBool(4))
                {
                    Dust d2 = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Torch, 0f, -0.5f, 0, default, 1.2f);
                    d2.noGravity = true;
                }
            }
            else
            {
                thermalStageTimer = 0;
            }
        }
    }
}
