using Terraria;
using Terraria.ModLoader;

namespace jarkos.buffs.buff
{
    public class SunfateLaserCooldown : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_94";

        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.debuff[Type] = true;
            Main.buffNoTimeDisplay[Type] = false;
        }
    }
}
