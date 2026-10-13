using Terraria;
using Terraria.ModLoader;
using jarkos.buffs.debuff;

namespace jarkos.buffs.buff
{
	public class Iodine_buff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoTimeDisplay[Type] = false;
			Main.debuff[Type] = false;
			Main.buffNoSave[Type] = false;
		}
		public override void Update(Player player, ref int buffIndex)
		{
			player.ClearBuff(ModContent.BuffType<Uranium_debuff>());
		}
	}
}
