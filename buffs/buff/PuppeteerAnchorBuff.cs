using Terraria;
using Terraria.ModLoader;

namespace jarkos.buffs.buff
{
    public class PuppeteerAnchorBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetDamage(DamageClass.Summon) += 0.40f;
            player.GetAttackSpeed(DamageClass.Summon) += 0.25f;
        }
    }
}
