using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using jarkos.items.accs.summon;

namespace jarkos.buffs.debuff
{
    public class AutodafeDebuff : ModBuff
    {
        public override string Texture => "jarkos/items/accs/summon/Autodafe";

        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
        public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
        {
            NPC npc = null;
            int stacks = 1;
            if (npc != null && npc.active)
            {
                var globalNPC = npc.GetGlobalNPC<AutodafeWeaknessNPC>();
                if (globalNPC != null && globalNPC.Stacks > 0)
                {
                    stacks = globalNPC.Stacks;
                }
            }

            float damagePercent = (float)Math.Round(stacks * AutodafeWeaknessNPC.DamageBonusPerStack * 100f, 1);
            float slowPercent = (float)Math.Round(MathHelper.Clamp(stacks * AutodafeWeaknessNPC.SlowPerStack, 0f, 0.95f) * 100f, 1);

            tip = Language.GetTextValue("Mods.jarkos.Buffs.AutodafeDebuff.Description", damagePercent, slowPercent);
        }
    }
}
