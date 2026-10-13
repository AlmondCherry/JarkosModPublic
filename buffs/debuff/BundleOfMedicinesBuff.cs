using Terraria;
using Terraria.ModLoader;
namespace jarkos.buffs.debuff
{
    public class BundleOfMedicinesBuff : ModBuff
    {
        public override void SetStaticDefaults() 
        {
            Main.buffNoSave[Type] = false;
            Main.debuff[Type] = false;
        }
        public override void Update(Player player, ref int buffIndex) 
        {
            player.buffImmune[ModContent.BuffType<VulnerabilityBuff>()] = true;
            player.buffImmune[ModContent.BuffType<PetrifiedSicknessBuff>()] = true;
        }
    }
}
