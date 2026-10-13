using jarkos.items.armor.bloodmantle;
using Terraria;
using Terraria.ModLoader;

namespace jarkos.buffs.buff
{
    public class BloodRageBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = false;
            Main.pvpBuff[Type] = false;
            Main.buffNoSave[Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            var modPlayer = player.GetModPlayer<BloodMantlePlayer>();
            if (modPlayer.BloodRageStacks > 0)
            {
                modPlayer.isBloodRageActive = true;
            }
        }
    }
}
