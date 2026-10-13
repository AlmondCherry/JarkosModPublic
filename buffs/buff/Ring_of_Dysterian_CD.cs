using Terraria;
using Terraria.ModLoader;

namespace jarkos.buffs.buff
{
  public class Ring_of_Dysterian_CD : ModBuff
  {
    public override void SetStaticDefaults()
    {
      Main.buffNoTimeDisplay[Type] = false;
      Main.debuff[Type] = true;
      Main.pvpBuff[Type] = true;
      Main.buffNoSave[Type] = true;
    }
  }
}
