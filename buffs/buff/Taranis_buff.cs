using jarkos.particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace jarkos.buffs.buff
{
  public class Taranis_buff : ModBuff
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
      npc.defense = 0;
      Vector2 new_vector = new Vector2(npc.velocity.X / 1.25f, npc.velocity.Y / 1.25f);

      if(!npc.boss)
      {
        npc.velocity = new_vector;
      }
      if(Main.rand.NextBool(12))
      {
        new StarParticleHorizontal(npc.Center, Main.rand.NextVector2Circular(4f, 4f), Color.Cyan, 90, 0.1f, 2f, 0f).Spawn();
				new StarParticleVertical(npc.Center, Main.rand.NextVector2Circular(4f, 4f), Color.Cyan, 90, 0.1f, 2f, 0f).Spawn();
      }
	  }
  }
}
