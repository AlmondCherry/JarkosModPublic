using jarkos.items.weapons.summon.staff.LunarDisaster;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.buffs.summonbuff
{
	public class LevitatingBlade_buff : ModBuff
	{
		public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = false;
            Main.debuff[Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<BladeEmpowermentPlayer>().bladeEmpowerment = true;
        }
    }
    public class BladeEmpowermentPlayer : ModPlayer
    {
        public bool bladeEmpowerment = false;
        public override void ResetEffects()
        {
            bladeEmpowerment = false;
        }
        public override void PostUpdateMiscEffects()
        {
            if (bladeEmpowerment)
            {
                Player.GetAttackSpeed(DamageClass.SummonMeleeSpeed) += 0.25f;
                Player.GetAttackSpeed(DamageClass.Summon) += 0.25f;
            }
        }
    }
}
