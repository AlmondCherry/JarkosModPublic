using Terraria;
using Terraria.ModLoader;

namespace jarkos.buffs.buff
{
  public class Auriteh_buff : ModBuff
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
      player.GetCritChance(DamageClass.Generic) += 5;
      player.GetArmorPenetration(DamageClass.Generic) += 0.05f;
	  }
  }
}
