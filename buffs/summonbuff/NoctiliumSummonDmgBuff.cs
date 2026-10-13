using jarkos.items.weapons.summon.staff.LunarDisaster;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.buffs.summonbuff
{
	public class NoctiliumSummonDmgBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetDamage(DamageClass.Summon) += 0.05f;
        }
    }
}
