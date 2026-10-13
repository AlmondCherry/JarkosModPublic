using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.buffs.summonbuff
{
	public class IceMarkDebuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.velocity *= 0.97f;
            if (Main.rand.NextBool(18))
                Dust.NewDust(npc.position, npc.width, npc.height, DustID.IceTorch, 0, -0.5f, 150, new Color(150, 230, 255), 0.7f);
        }
    }
}
