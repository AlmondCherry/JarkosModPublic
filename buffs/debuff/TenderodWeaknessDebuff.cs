using Terraria;
using Terraria.ModLoader;
namespace jarkos.buffs.debuff
{
    public class TenderodWeaknessDebuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetDamage(DamageClass.Generic) *= 0.25f;
        }
    }
}
