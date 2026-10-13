using Terraria;
using Terraria.ModLoader;
using Terraria.Localization;

namespace jarkos.buffs.buff
{
    public class DiffusalAttackSpeedBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetAttackSpeed(DamageClass.Generic) += player.GetModPlayer<Jarkos_player>().diffusalBlueStacks * 0.03f;
        }
        public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
        {
            Player player = Main.LocalPlayer;
            if (player == null || !player.active)
                return;
                
            var modPl = player.GetModPlayer<Jarkos_player>();
            int stacks = modPl.diffusalBlueStacks;
            int speedBonus = stacks * 3;

            tip = Language.GetTextValue("Mods.jarkos.Buffs.DiffusalAttackSpeedBuff.Description", speedBonus, stacks);
        }
    }
}
