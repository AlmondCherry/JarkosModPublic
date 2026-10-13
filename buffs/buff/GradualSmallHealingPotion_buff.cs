using Terraria;
using Terraria.ModLoader;

namespace jarkos.buffs.buff
{
	public class GradualSmallHealingPotion_buff : ModBuff
	{
        public static int HealAmount = 5;
        public static int HealInterval = 30;
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            var modPlayer = player.GetModPlayer<GradualHealingPlayer>();
            modPlayer.healTimer++;
            if (modPlayer.healTimer >= HealInterval)
            {
                modPlayer.healTimer = 0;
                int actualHeal = HealAmount;
                if (player.statLife + actualHeal > player.statLifeMax2)
                {
                    actualHeal = player.statLifeMax2 - player.statLife;
                }
                if (actualHeal > 0)
                {
                    player.statLife += actualHeal;
                    player.HealEffect(actualHeal, true);
                }
            }
        }
    }
    public class GradualHealingPlayer : ModPlayer
    {
        public int healTimer = 0;
        public override void ResetEffects()
        {
            if (!Player.HasBuff(ModContent.BuffType<GradualSmallHealingPotion_buff>()))
            {
                healTimer = 0;
            }
        }
    }
}
