using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.buffs.summonbuff
{
	public class LightningMarkDebuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }
        public override void Update(NPC npc, ref int buffIndex)
        {
            if (Main.rand.NextBool(12))
                Dust.NewDust(npc.position, npc.width, npc.height, DustID.Electric, Main.rand.NextFloat(-1f, 1f), -0.5f, 150, new Color(255, 255, 80), 1.0f);
        }
    }
}
