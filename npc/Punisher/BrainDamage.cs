using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using jarkos.particles;

namespace jarkos.npc.Punisher
{
    public class BrainDamage : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_31";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = false;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.moveSpeed *= 0.2f;
            player.jumpSpeedBoost = -5f;
            player.accRunSpeed *= 0.2f;
            player.maxRunSpeed *= 0.2f;
            
            if (Main.rand.NextBool(5))
            {
                new Spark(player.Center, Main.rand.NextVector2Circular(2f, 2f), Color.DarkRed, 30, Main.rand.NextFloat(0.5f, 1.5f), 1f).Spawn();
            }
        }
    }
}
