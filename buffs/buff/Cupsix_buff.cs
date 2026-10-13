using Terraria;
using Terraria.ModLoader;

namespace jarkos.buffs.buff
{
	public class Cupsix_buff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoTimeDisplay[Type] = false;
			Main.debuff[Type] = false;
			Main.buffNoSave[Type] = false;
		}
	}
}
