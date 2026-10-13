using Terraria;
using Terraria.ModLoader;
using jarkos.items.accs.melee;
using Terraria.ID;
using Terraria.Localization;

namespace jarkos.buffs.buff
{
	public class DisteryanBlessing : ModBuff
	{
		public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = true;
            Main.buffNoSave[Type] = true;
            Main.debuff[Type] = false;
        }
        public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
        {
            var modPlayer = Main.LocalPlayer.GetModPlayer<NatureGodPlayer>();
            float trust = modPlayer.natureTrust;

            tip = Language.GetTextValue("Mods.jarkos.Buffs.DisteryanBlessing.Description", trust);
            rare = ItemRarityID.Lime;
        }
	}
}
