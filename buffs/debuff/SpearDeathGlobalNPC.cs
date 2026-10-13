using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.buffs.debuff
{
    public class SpearDeathGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        
        public bool spearDeathDebuff = false;

        public override void ResetEffects(NPC npc)
        {
            spearDeathDebuff = false;
        }

        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (spearDeathDebuff)
            {
                if (npc.lifeRegen > 0)
                {
                    npc.lifeRegen = 0;
                }

                float speed = npc.velocity.Length();
                float speedFactor = MathHelper.Clamp(speed / 12f, 0f, 1f);
                int regenLoss = (int)MathF.Round(MathHelper.Lerp(1f, 10f, speedFactor));

                npc.lifeRegen -= regenLoss;

                if (damage < 1)
                {
                    damage = 1;
                }
            }
        }

        public override void PostAI(NPC npc)
        {
            if (spearDeathDebuff)
            {
                float speed = npc.velocity.Length();
                float chance = MathHelper.Lerp(0.08f, 0.5f, MathHelper.Clamp(speed / 12f, 0f, 1f));
                if (Main.rand.NextFloat() < chance)
                {
                    Dust d = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Blood, -npc.velocity.X * 0.15f, -npc.velocity.Y * 0.15f, 100, default, 1.1f);
                    d.noGravity = true;
                }
            }
        }
    }
}
