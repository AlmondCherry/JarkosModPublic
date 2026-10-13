using Terraria;
using Terraria.ModLoader;

namespace jarkos.buffs.debuff
{
    public class Eternal_reign_cd : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = false;
        }
    }
}
