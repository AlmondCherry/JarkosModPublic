using Terraria;
using Terraria.ModLoader;
namespace jarkos.buffs.buff
{
  public class Infinite_wingtime : ModBuff
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
      player.wingTime = 99999;
	  }
  }
}
