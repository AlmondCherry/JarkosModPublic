using Terraria;
using Terraria.ModLoader;

namespace jarkos.buffs.debuff
{
    public class Necrotization : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
            Main.pvpBuff[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<NecrotizationGlobalNPC>().necrotizationDebuff = true;
        }
    }
}
