using Terraria;
using Terraria.ModLoader;
using jarkos.buffs.buff;

namespace jarkos.projectiles.melee.true_melee
{
    public class KaliburnPlayer : ModPlayer
    {
        public int kaliburnArmorStacks = 0;

        public void AddKaliburnStack()
        {
            if (!Player.HasBuff(ModContent.BuffType<KaliburnArmorBuff>()))
            {
                kaliburnArmorStacks = 1;
            }
            else if (kaliburnArmorStacks < 5)
            {
                kaliburnArmorStacks++;
            }
            Player.AddBuff(ModContent.BuffType<KaliburnArmorBuff>(), 180); 
        }

        public override void ResetEffects()
        {
            if (!Player.HasBuff(ModContent.BuffType<KaliburnArmorBuff>()))
            {
                kaliburnArmorStacks = 0;
            }
            else
            {
                Player.statDefense += 2 * kaliburnArmorStacks;
            }
        }
    }
}
