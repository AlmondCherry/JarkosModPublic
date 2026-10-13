using Terraria;
using Terraria.ModLoader;
using Terraria.Localization;
using jarkos.items.accs.summon;

namespace jarkos.buffs.buff
{
	public class SummonPowerBoostBuff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoTimeDisplay[Type] = false;
			Main.debuff[Type] = false;
			Main.pvpBuff[Type] = true;
			Main.buffNoSave[Type] = true;
		}

		public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
		{
			Player player = Main.LocalPlayer;
			int stacks = player.GetModPlayer<GalaxyPearlPlayer>().hitStacks;
			if (stacks < 1) stacks = 1;

			tip = Language.GetTextValue("Mods.jarkos.Buffs.SummonPowerBoostBuff.Description", stacks);
		}
	}
}
