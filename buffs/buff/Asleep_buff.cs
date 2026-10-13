using Terraria;
using Terraria.ModLoader;

namespace jarkos.buffs.buff
{
	public class Asleep_buff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoTimeDisplay[Type] = false;
			Main.debuff[Type] = false;
			Main.buffNoSave[Type] = false;
		}
		public override void Update(Player player, ref int buffIndex)
		{
			player.statManaMax2 += player.statManaMax2 * 5 / 100;
			
			if(Main.dayTime)
			{
				player.manaRegen += 4;
			}
			else
			{
				player.manaRegen += 10;
			}
		}
	}
}
