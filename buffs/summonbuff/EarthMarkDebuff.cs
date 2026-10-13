using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.buffs.summonbuff
{
	public class EarthMarkDebuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
        public override void Update(NPC npc, ref int buffIndex)
        {
            if (Main.rand.NextBool(18))
                Dust.NewDust(npc.position, npc.width, npc.height, DustID.Dirt, 0, -0.5f, 150, new Color(139, 90, 43), 0.7f);
        }
    }
}
