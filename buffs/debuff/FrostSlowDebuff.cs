using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.buffs.debuff
{
  public class FrostSlowDebuff : ModBuff
  {
    public override void SetStaticDefaults()
    {
        Main.debuff[Type] = true;
        Main.pvpBuff[Type] = true;
        Main.buffNoSave[Type] = true;
        BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
    }
    public override void Update(NPC npc, ref int buffIndex)
    {
        if (npc.velocity.X > 0.5f)
            npc.velocity.X *= 0.5f;
        else if (npc.velocity.X < -0.5f)
            npc.velocity.X *= 0.5f;
            
        if (npc.velocity.Y > 0.5f && npc.velocity.Y < npc.gravity)
            npc.velocity.Y *= 0.7f;
        if (Main.rand.NextBool(5))
        {
            Dust ice = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.IceTorch, 0, -1, 100, default, 0.8f);
            ice.noGravity = true;
        }
    }
  }
}
