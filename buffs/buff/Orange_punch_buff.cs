using Terraria;
using Terraria.ModLoader;

namespace jarkos.buffs.buff
{
	public class Orange_punch_buff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoTimeDisplay[Type] = false;
			Main.debuff[Type] = false;
			Main.buffNoSave[Type] = false;
		}
		public override void Update(Player player, ref int buffIndex)
		{
			player.statLifeMax2 += 200;
			player.statManaMax2 += 200;
			player.lifeRegen += 2;
			player.manaRegen += 2;
		}
	}
}
