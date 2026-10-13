using Terraria;
using Terraria.ModLoader;

namespace jarkos.buffs.buff
{
	public class Amnesia_buff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoTimeDisplay[Type] = false;
			Main.debuff[Type] = false;
			Main.buffNoSave[Type] = false;
		}
		public override void Update(Player player, ref int buffIndex)
		{
			player.GetDamage(DamageClass.Generic) += 0.30f;
			player.statDefense *= 0.6f;
		}
	}
}
