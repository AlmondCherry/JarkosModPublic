using Terraria;
using Terraria.ModLoader;

namespace jarkos.buffs.buff
{
	public class Fallen_star_moonshine_buff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoTimeDisplay[Type] = false;
			Main.debuff[Type] = false;
			Main.buffNoSave[Type] = false;
		}
		public override void Update(Player player, ref int buffIndex)
		{
			player.endurance += 0.15f;
			player.luck += 0.15f;
			player.fishingSkill += 50;
			player.lifeRegen += 5;
			player.manaRegen += 5;
		}
	}
}
