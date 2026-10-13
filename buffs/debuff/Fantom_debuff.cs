using jarkos.particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.buffs.debuff
{
  public class Fantom_debuff : ModBuff
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
      npc.GetGlobalNPC<Jarkos_npc>().frag_weakness_debuff = true;
      npc.lifeRegen -= npc.GetGlobalNPC<Jarkos_npc>().damage_done_global;
      
      if (Main.rand.NextBool(2))
      {
        new Solid_particle(npc.Center, Main.rand.NextVector2Circular(5f, 5f), Color.DeepPink, 120, 0.25f, 2f, 0f).Spawn();
        int dust = Dust.NewDust(npc.position, npc.width, npc.height, DustID.PinkTorch);
        Main.dust[dust].scale = 0.5f;
        Main.dust[dust].noGravity = true;
      }
	  }
  }
}
