using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.buffs.summonbuff
{
	public class FireMarkDebuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
        public override void Update(NPC npc, ref int buffIndex)
        {
            if (Main.rand.NextBool(15))
                Dust.NewDust(npc.position, npc.width, npc.height, DustID.Torch, 0, -1f, 150, new Color(255, 100, 30), 0.8f);
        }
    }
}
