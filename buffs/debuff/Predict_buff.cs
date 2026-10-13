using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.buffs.debuff
{
  public class Predict_buff : ModBuff
  {
    public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }
        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.lifeRegen -= 5;
            npc.velocity *= 0.95f;

            if (Main.rand.NextBool(5))
            {
                Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.Blood);
                dust.velocity = new Vector2(Main.rand.NextFloat(-1, 1), -2f);
                dust.scale = 1.2f;
                dust.noGravity = true;
            }

            Lighting.AddLight(npc.Center, 0.5f, 0.1f, 0.1f);
        }
	}
}
