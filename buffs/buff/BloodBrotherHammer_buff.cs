using Terraria;
using Terraria.ModLoader;

namespace jarkos.buffs.buff
{
    public class BloodBrotherHammer_buff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = false;
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = false;
            Main.pvpBuff[Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.lifeRegen += 10;
            player.statDefense += 2;
        }
    }
}
