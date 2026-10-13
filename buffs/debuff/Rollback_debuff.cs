using Terraria;
using Terraria.ModLoader;

namespace jarkos.buffs.debuff
{
  public class Rollback_debuff : ModBuff
  {
    public override void SetStaticDefaults()
    {
      Main.buffNoTimeDisplay[Type] = true;
      Main.debuff[Type] = false;
      Main.pvpBuff[Type] = true;
      Main.buffNoSave[Type] = true;
    }
    public override void Update(NPC npc, ref int buffIndex)
	  {
      npc.lifeRegen -= 5;
	  }
  }
}
