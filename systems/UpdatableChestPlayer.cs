using Terraria;
using Terraria.ModLoader;
using jarkos.tiles;
using jarkos.items.materials;
using jarkos.cradle_biome.system;
using Terraria.Utilities;
using Jarkos.items.materials;
using jarkos.cradle_biome;
using Terraria.ID;
namespace jarkos.systems
{
    public class UpdatableChestPlayer : ModPlayer
    {
        private int previousChest = -1;
        public override void PostUpdate()
        {
            if (previousChest != -1 && Player.chest == -1)
            {
                ChestClosed(previousChest);
            }
            previousChest = Player.chest;
        }
        private void ChestClosed(int chestIndex)
        {
            if (chestIndex < 0 || chestIndex >= Main.maxChests) 
                return;
            Chest chest = Main.chest[chestIndex];

            if (chest == null || chest.item == null) 
                return;
            if (chest.x < 10 || chest.y < 10 || chest.x >= Main.maxTilesX - 10 || chest.y >= Main.maxTilesY - 10) 
                return;

            Tile tile = Main.tile[chest.x, chest.y];
            if (tile.TileType != ModContent.TileType<CradleUpdatableChest>()) 
                return;

            bool hasCore = false;
            int coreIndex = -1;

            for (int i = 0; i < Chest.maxItems; i++)
            {
                Item item = chest.item[i];

                if (item == null || item.IsAir) continue;
                if (item.type == ModContent.ItemType<DeliciousBait>())
                {
                    if (hasCore || item.stack > 1)
                    {
                        return;
                    }
                    hasCore = true;
                    coreIndex = i;
                }
                else
                {
                    return;
                }
            }
            if (hasCore)
            {
                chest.item[coreIndex].TurnToAir();
                RestockChest(chest, chestIndex);
            }
        }
        private void RestockChest(Chest chest, int chestIndex)
        {
            UnifiedRandom rand = new UnifiedRandom();
            PetrifiedAbyssSystem.FillAbyssChest(chest, rand, PetrifiedAbyssSystem.LootTier.Heart);
            for (int i = 0; i < 15; i++)
            {
                Dust.NewDust(new Microsoft.Xna.Framework.Vector2(chest.x * 16, chest.y * 16), 32, 32, DustID.MagicMirror);
            }
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                for (int i = 0; i < Chest.maxItems; i++)
                {
                    NetMessage.SendData(MessageID.SyncChestItem, -1, -1, null, chestIndex, i);
                }
            }
        }
    }
}
