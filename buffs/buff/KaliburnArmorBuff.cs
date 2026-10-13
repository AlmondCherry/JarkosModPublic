using Terraria;
using Terraria.ModLoader;
using jarkos.projectiles.melee.true_melee;
using Terraria.Localization;

namespace jarkos.buffs.buff
{
    public class KaliburnArmorBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_14";

        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = false;
        }

        public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
        {
            Player player = Main.LocalPlayer;
            var modPlayer = player.GetModPlayer<KaliburnPlayer>();
            
            tip = Language.GetTextValue("Mods.jarkos.Buffs.KaliburnArmorBuff.Description", modPlayer.kaliburnArmorStacks);
        }
    }
}
