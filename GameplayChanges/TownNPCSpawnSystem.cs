using jarkos.npc.CradleMobs;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.systems
{
    public class TownNPCSpawnSystem : ModSystem
    {
        public override void PostUpdateWorld()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;

            float mult = Jarkos_config.Instance != null ? Jarkos_config.Instance.TownNPCSpawnMultiplier : 1f;
            if (mult <= 0f) return;

            if (WorldGen.prioritizedTownNPCType == 0)
            {
                if (ScavengerSystem.UnlockedScavenger && !NPC.AnyNPCs(ModContent.NPCType<ScavengerNPC>()))
                {
                    WorldGen.prioritizedTownNPCType = ModContent.NPCType<ScavengerNPC>();
                }
            }
        }
    }
}
