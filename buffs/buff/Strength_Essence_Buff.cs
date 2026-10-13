using Terraria;
using Terraria.ModLoader;

namespace jarkos.buffs.buff
{
	public class Strength_Essence_Buff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoTimeDisplay[Type] = false;
			Main.debuff[Type] = false;
			Main.buffNoSave[Type] = false;
		}
		public override void Update(Player player, ref int buffIndex)
		{
			player.GetDamage(DamageClass.Generic) += 0.12f;
			player.GetCritChance(DamageClass.Generic) += 0.12f;
		}
	}
}
