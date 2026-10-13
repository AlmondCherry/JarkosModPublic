using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.melee.true_melee
{
    public class ElectroStun : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_144";

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
    }
    public class ElectroStunGlobalNPC : GlobalNPC
    {
        public override bool PreAI(NPC npc)
        {
            if (npc.HasBuff<ElectroStun>())
            {
                if (npc.boss)
                {
                    npc.velocity *= 0.85f;
                    return true;
                }

                npc.velocity *= 0.1f;
                if (Main.rand.NextBool(3))
                {
                    Dust d = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Electric, 0f, 0f, 0, default, 0.8f);
                    d.noGravity = true;
                    d.velocity *= 0.3f;
                }
                return false;
            }
            return true;
        }
    }
}
