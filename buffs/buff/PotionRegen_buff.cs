using Terraria;
using Terraria.ModLoader;

namespace jarkos.buffs.buff
{
	public class PotionRegen_buff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoTimeDisplay[Type] = false;
			Main.debuff[Type] = false;
			Main.buffNoSave[Type] = false;
		}
		public override void Update(Player player, ref int buffIndex)
		{
			player.lifeRegen += 8;
			player.statLifeMax2 -= player.statLifeMax2 * 40 / 100;
		}
	}
}
