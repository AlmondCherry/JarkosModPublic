using jarkos.particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.buffs.buff
{
    public class HemoStabilizer_buff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = false;
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = false;
            Main.pvpBuff[Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.GetAttackSpeed(DamageClass.Generic) += 0.20f;
            if (Main.rand.NextBool(4))
			{
				Vector2 vec = new Vector2(Main.rand.NextFloat(-0.1f, 0.1f), Main.rand.NextFloat(-16.5f, -8.75f));
				Vector2 vec2 = new Vector2(Main.rand.NextFloat(-25f, 25f), 0);
				new Spark(player.Center + vec2, vec, Color.IndianRed, 90, 0.35f, 1.5f, 0f).Spawn();
				new Spark(player.Center + vec2, vec, Color.IndianRed, 90, 0.725f, 1.5f, 0f).Spawn();
			}
			if (Main.rand.NextBool(10))
			{
				new Circular_glow(player.Center, Vector2.Zero, Color.IndianRed, 100, 0.2f, 1.25f, 0f).Spawn();
			}
        }
        public override void ModifyBuffText(ref string buffName, ref string tip, ref int rare)
        {
            rare = ItemRarityID.Red;
        }
    }
}
