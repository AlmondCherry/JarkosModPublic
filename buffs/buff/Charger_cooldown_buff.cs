using Terraria;
using Terraria.ModLoader;

namespace jarkos.buffs.buff
{
  public class Charger_cooldown_buff : ModBuff
  {
    public override void SetStaticDefaults()
    {
      Main.buffNoTimeDisplay[Type] = false;
      Main.debuff[Type] = true;
      Main.pvpBuff[Type] = false;
      Main.buffNoSave[Type] = false;
    }
  }
}
