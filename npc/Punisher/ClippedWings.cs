using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using jarkos.particles;

namespace jarkos.npc.Punisher
{
    public class ClippedWings : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_36";
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.wingsLogic = 0;
            player.wingTime = 0;

            if (player.mount.Active)
            {
                player.mount.Dismount(player);
            }

            player.maxFallSpeed = 20f;
            player.gravity += 0.2f;
            
            if (player.lifeRegen > 0)
            {
                player.lifeRegen = 0;
            }

            player.lifeRegenTime = 0;
            player.lifeRegen -= 4;

            if (Main.rand.NextBool(4))
            {
                new Spark(player.Center, Main.rand.NextVector2Circular(3f, 3f), Color.DarkRed, 30, Main.rand.NextFloat(0.5f, 1.5f), 1f).Spawn();
            }
            
            if (!NPC.AnyNPCs(ModContent.NPCType<Punisher>()))
            {
                player.ClearBuff(Type);
            }
        }
    }
}
