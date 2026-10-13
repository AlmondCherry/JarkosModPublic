using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using jarkos.items.accs.melee;
using Terraria.Localization;

namespace jarkos.buffs.buff
{
	public class DisteryanCurse : ModBuff
	{
		public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.buffNoSave[Type] = true;
            Main.debuff[Type] = true;
        }
        public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
        {
            var modPlayer = Main.LocalPlayer.GetModPlayer<NatureGodPlayer>();
            float trust = modPlayer.natureTrust;

            tip = Language.GetTextValue("Mods.jarkos.Buffs.DisteryanCurse.Description", trust);
            rare = ItemRarityID.Gray;
        }
	}
}
