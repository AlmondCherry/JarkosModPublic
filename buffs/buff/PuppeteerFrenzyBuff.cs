using Terraria;
using Terraria.ModLoader;
namespace jarkos.buffs.buff
{
    public class PuppeteerFrenzyBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetDamage(DamageClass.Summon).Flat += 0f;
            player.GetDamage(DamageClass.Summon) *= 2.50f;
            player.GetKnockback(DamageClass.Summon) += 100f;
        }
    }
}
