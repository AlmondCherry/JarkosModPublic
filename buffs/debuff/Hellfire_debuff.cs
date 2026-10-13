using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.buffs.debuff
{
  public class Hellfire_debuff : ModBuff
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
      npc.lifeRegen -= 25;
      
      if (Main.rand.NextBool(2)) 
      {
				int dust = Dust.NewDust(npc.position, npc.width, npc.height, DustID.OrangeTorch);
				Main.dust[dust].scale = 1.5f;
				Main.dust[dust].noGravity = true;
			}
	  }
  }
}
