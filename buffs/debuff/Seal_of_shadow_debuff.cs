using jarkos.particles.storm;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace jarkos.buffs.debuff
{
  public class Seal_of_shadow_debuff : ModBuff
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
      npc.GetGlobalNPC<Jarkos_npc>().Seal_of_shadow_debuff = true;
      Vector2 vel = Main.rand.NextVector2Circular(1f, 1f);

      if (Main.rand.NextBool(5))
      {
        new Fire_particle(npc.Center, vel * 4f, Color.MediumPurple, 120, Main.rand.NextFloat(0.45f, 0.95f), 2f, Main.rand.NextFloat(0.1f, 1f)).Spawn();
      }
	  }
  }
}
