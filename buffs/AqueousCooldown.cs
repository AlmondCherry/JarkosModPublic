using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.buffs
{
    public class AqueousCooldown : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_" + BuffID.ChaosState;
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = false;
        }
    }
}
