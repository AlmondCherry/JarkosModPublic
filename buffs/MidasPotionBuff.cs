using Terraria;
using Terraria.ModLoader;

namespace jarkos.buffs
{
	public class MidasPotionBuff : ModBuff
	{
		public override string Texture => "jarkos/buffs/buff/Strength_Essence_Buff";

		public override void SetStaticDefaults()
		{
			Main.buffNoSave[Type] = false;
			Main.debuff[Type] = false;
			Main.pvpBuff[Type] = true;
			Main.buffNoTimeDisplay[Type] = false;
		}
	}
}
