using Terraria;
using Terraria.ModLoader;

namespace jarkos.buffs.buff
{
	public class Nasty_cocktail_buff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoTimeDisplay[Type] = false;
			Main.debuff[Type] = false;
			Main.buffNoSave[Type] = false;
		}
		public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
    	{
			Player player = Main.LocalPlayer;
			tip = $" {player.lifeRegen} " + "\n"
			+ $" {player.manaRegen} ";
    	}
		public override void Update(Player player, ref int buffIndex)
		{
			player.statDefense += 10;
			player.statDefense += player.statLifeMax2 / 50;
		}
	}
}
