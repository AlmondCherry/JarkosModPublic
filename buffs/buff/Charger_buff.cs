using Terraria;
using Terraria.ModLoader;

namespace jarkos.buffs.buff
{
  public class Charger_buff : ModBuff
  {
    public override void SetStaticDefaults()
    {
      Main.buffNoTimeDisplay[Type] = true;
      Main.debuff[Type] = false;
      Main.pvpBuff[Type] = true;
      Main.buffNoSave[Type] = true;
    }
  }
}
