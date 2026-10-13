using Terraria;
using Terraria.ModLoader;

namespace jarkos.buffs.buff
{
  public class Beer_buff_player : ModBuff
  {
    public override void SetStaticDefaults()
    {
      Main.buffNoTimeDisplay[Type] = false;
      Main.debuff[Type] = false;
      Main.pvpBuff[Type] = true;
      Main.buffNoSave[Type] = true;
    }
    public override void Update(Player player, ref int buffIndex)
    {
      player.statDefense += 2;
      player.GetAttackSpeed(DamageClass.Generic) += 0.05f;
	  }
  }
}
