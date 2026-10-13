using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.buffs.debuff
{
    public class BloodFieldGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public bool bloodField = false;
        public override void ResetEffects(NPC npc)
        {
            bloodField = false;
        }
        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (bloodField)
            {
                if (npc.lifeRegen > 0)
                    npc.lifeRegen = 0;
                npc.lifeRegen -= 80;
                if (damage < 10)
                    damage = 10;
            }
        }
        public override void PostAI(NPC npc)
        {
            if (bloodField)
            {
                if (!npc.boss && npc.velocity.Length() > 0)
                {
                    npc.position -= npc.velocity * 0.45f;
                }
                if (Main.rand.NextBool(3))
                {
                    Dust d = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Blood, 0f, 2f, 100, default, 1.2f);
                    d.noGravity = false;
                }
            }
        }
    }
}
