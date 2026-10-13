using jarkos.particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.buffs.debuff
{
  public class Burning_mark_debuff : ModBuff
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
      Vector2 new_vector = new Vector2(npc.velocity.X / 2, npc.velocity.Y / 2);
      npc.lifeRegen -= 5;
      npc.velocity = new_vector;

      if (Main.rand.NextBool(2)) 
      {
        new Solid_particle(npc.Center, Main.rand.NextVector2Circular(5f, 5f), Color.Red, 120, 0.25f, 2f, 0f).Spawn();
				int dust = Dust.NewDust(npc.position, npc.width, npc.height, DustID.RedTorch);
				Main.dust[dust].scale = 0.5f;
				Main.dust[dust].noGravity = true;
			}
	  }
  }
}
