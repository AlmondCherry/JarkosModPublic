using Terraria;
using Terraria.ModLoader;

namespace jarkos.buffs.debuff
{
    public class BloodFieldDebuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
            Main.pvpBuff[Type] = true;
        }
        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.GetGlobalNPC<BloodFieldGlobalNPC>().bloodField = true;
        }
    }
}
