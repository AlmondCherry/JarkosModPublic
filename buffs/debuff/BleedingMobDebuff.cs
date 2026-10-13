using jarkos.items.accs.summon;
using Terraria;
using Terraria.ModLoader;

namespace jarkos.buffs.debuff
{
	public class BleedingMobDebuff : ModBuff
	{
		public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.lifeRegen -= 5;
            npc.GetGlobalNPC<EvilGodsParchmentGlobalNPC>().BleedingParchDebuff = true;
        }
	}
}
