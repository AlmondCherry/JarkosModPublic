using Terraria;
using Terraria.ModLoader;
using Terraria.Localization;

namespace jarkos.buffs.buff
{
    public class DiffusalDefenseBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            var modPl = player.GetModPlayer<Jarkos_player>();
            player.statDefense += modPl.diffusalOrangeStacks;
            player.lifeRegen += modPl.diffusalOrangeStacks;
        }
        public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
        {
            Player player = Main.LocalPlayer;
            if (player == null || !player.active)
                return;

            var modPl = player.GetModPlayer<Jarkos_player>();
            int stacks = modPl.diffusalOrangeStacks;
            int defenseBonus = stacks;
            float regenBonus = stacks * 0.5f;

            tip = Language.GetTextValue("Mods.jarkos.Buffs.DiffusalDefenseBuff.Description", defenseBonus, regenBonus);
        }
    }
}
