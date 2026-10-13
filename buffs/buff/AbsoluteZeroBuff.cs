using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.buffs.buff
{
    public class AbsoluteZeroBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = false;
            Main.buffNoSave[Type] = true;
        }
        public override void Update(NPC npc, ref int buffIndex)
        {
            if (!npc.boss)
            {
                npc.velocity = Vector2.Zero;
                npc.frameCounter = 0;
            }
            else
            {
                npc.velocity *= 0.1f;
            }
            npc.color = Color.Cyan;
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Ice);
                d.velocity *= 0.1f;
                d.noGravity = true;
            }
        }
    }
}
