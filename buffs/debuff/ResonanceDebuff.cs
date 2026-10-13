using jarkos.particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.buffs.debuff
{
  public class ResonanceDebuff : ModBuff
  {
    public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = false;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
            BuffID.Sets.IsATagBuff[Type] = false;
        }
        public override void Update(NPC npc, ref int buffIndex)
        {
            Color[] color_rand = new Color[] {Color.Yellow, Color.LightYellow, Color.Orange};
            if (Main.rand.NextBool(4))
            {
                new Spark(npc.Center, Main.rand.NextVector2Circular(4f, 4f), color_rand[Main.rand.Next(0, 3)], 250, 1f, 1.5f, 0f).Spawn();
            }
            if (Main.rand.NextBool(8))
            {
                Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.GoldCoin, 0, 0, 180, default, 0.5f);
                dust.noGravity = true;
                dust.velocity = Vector2.Zero;
            }
            Lighting.AddLight(npc.Center, 0.3f, 0.15f, 0.4f);
        }
  }
}
