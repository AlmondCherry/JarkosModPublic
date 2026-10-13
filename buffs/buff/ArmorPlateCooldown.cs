using Terraria;
using Terraria.ModLoader;

namespace jarkos.buffs.buff
{
	public class ArmorPlateCooldown : ModBuff
	{
		public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.buffNoSave[Type] = true;
            Main.debuff[Type] = true;
        }
	}
}
