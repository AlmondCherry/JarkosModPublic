using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.buffs.summonbuff
{
	public class ManaEssence_buff : ModBuff
	{
		public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.manaRegenBonus += 40;
            if (Main.rand.NextBool(8))
            {
                Dust dust = Dust.NewDustDirect(player.position, player.width, player.height, DustID.BlueTorch, 0, -1f, 100, new Color(100, 160, 255), 0.6f);
                dust.noGravity = true;
                dust.velocity.Y = -0.5f;
            }
            if (Main.rand.NextBool(15))
            {
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(player.width * 0.6f, player.height * 0.6f);
                Dust Spark = Dust.NewDustDirect(pos, 0, 0, DustID.MagicMirror, 0, -0.3f, 0, new Color(150, 200, 255), 0.5f);
                Spark.noGravity = true;
            }
        }
    }
}
