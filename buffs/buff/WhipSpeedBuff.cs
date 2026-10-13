using Terraria;
using Terraria.ModLoader;

namespace jarkos.buffs.buff
{
    public class WhipSpeedBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = false;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.whipRangeMultiplier += 0f;
            player.GetAttackSpeed(DamageClass.SummonMeleeSpeed) += 0.25f;
        }
    }
}
