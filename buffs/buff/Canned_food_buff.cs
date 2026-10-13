using Terraria;
using Terraria.ModLoader;

namespace jarkos.buffs.buff
{
	public class Canned_food_buff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoTimeDisplay[Type] = false;
			Main.debuff[Type] = false;
			Main.buffNoSave[Type] = false;
		}
		public override void Update(Player player, ref int buffIndex)
		{
			player.statDefense += 2;
			player.endurance += 0.02f;
			player.GetDamage(DamageClass.Generic) += 0.04f;
			player.lifeRegen += 1;
		}
	}
}
