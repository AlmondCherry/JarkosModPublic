using Terraria;
using Terraria.ModLoader;

namespace jarkos.buffs.buff
{
	public class Cherry_smash_buff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoTimeDisplay[Type] = false;
			Main.debuff[Type] = false;
			Main.buffNoSave[Type] = false;
		}
		public override void Update(Player player, ref int buffIndex)
		{
			player.statDefense += 4;
			player.endurance += 0.04f;

			player.GetDamage(DamageClass.Generic) += 0.08f;
			player.GetArmorPenetration(DamageClass.Generic) += 5f;
			
			player.lifeRegen += 1;
			player.manaRegen += 1;
			player.lifeRegen += player.lifeRegen * 70 / 100;
		}
	}
}
