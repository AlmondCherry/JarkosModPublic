using Terraria;
using Terraria.ModLoader;

namespace jarkos.buffs.buff
{
  public class Superemerald_buff : ModBuff
  {
    public override void SetStaticDefaults()
    {
      Main.buffNoTimeDisplay[Type] = true;
      Main.debuff[Type] = false;
      Main.pvpBuff[Type] = true;
      Main.buffNoSave[Type] = true;
    }
    public override void Update(Player player, ref int buffIndex)
    {
      player.GetDamage(DamageClass.Generic) += 0.15f;
      player.GetAttackSpeed(DamageClass.Generic) += 0.10f;
      player.statLifeMax2 += 50;
      player.statManaMax2 += 100;
      player.lifeRegen += 5;
      player.manaRegen += 15;
    }
  }
}
