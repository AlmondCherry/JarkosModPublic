using Terraria.ModLoader;
using jarkos.UI.Pedestal;

namespace jarkos
{
    public class Jarkos_world : ModSystem
    {
        public override void OnWorldLoad()
        {
            ModContent.GetInstance<Rune_pedestal_system>().HideMyUI();
        }
    }
    /*public class TheDepthsWorldGen : ModSystem
    {
    public class ChestsPass : GenPass
    {
        public ChestsPass(string name, float loadWeight) : base(name, loadWeight)
        {
        }
        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = EtherioniteSystem.ExampleOrePassMessage.Value;
            for (int chestID = 0; chestID < Main.maxChests; chestID++)
            {
                Chest chest = Main.chest[chestID];
                if (chest != null)
                {
                    for (int chestslot = 0; chestslot < Chest.maxItems; chestslot++)
                    {
                        switch (chest.item[chestslot].type)
                        {
                            case ItemID.LavaCharm:
                                chest.item[chestslot].TurnToAir(true);
                                chest.item[chestslot].SetDefaults(ModContent.ItemType<CE_ChestKey>());
                                break;
                            case ItemID.HellMinecart:
                                chest.item[chestslot] = new Item(ModContent.ItemType<CE_ChestKey>());
                                break;
                            case ItemID.HellfireArrow:
                                chest.item[chestslot] = new Item(ModContent.ItemType<CE_ChestKey>());
                                chest.item[chestslot].stack = WorldGen.genRand.Next(25) + 50;
                                break;
                            case ItemID.HellCake:
                                chest.item[chestslot] = new Item(ModContent.ItemType<CE_ChestKey>());
                                break;
                            case ItemID.TreasureMagnet:
                                chest.item[chestslot].TurnToAir(true);
                                chest.item[chestslot].SetDefaults(ModContent.ItemType<CE_ChestKey>());
                                break;
                            case ItemID.ObsidianSkinPotion:
                                chest.item[chestslot] = new Item(ModContent.ItemType<CE_ChestKey>());
                                chest.item[chestslot].stack = WorldGen.genRand.Next(1, 3);
                                break;
                            case ItemID.InfernoPotion:
                                chest.item[chestslot] = new Item(ModContent.ItemType<CE_ChestKey>());
                                chest.item[chestslot].stack = WorldGen.genRand.Next(1, 3);
                                break;
                            case ItemID.CobaltShield:
                                chest.item[chestslot].TurnToAir(true);
                                chest.item[chestslot].SetDefaults(ModContent.ItemType<CE_ChestKey>());
                                break;
                            case ItemID.GoldCoin:
                                chest.item[chestslot].TurnToAir(true);
                                chest.item[chestslot].SetDefaults(ModContent.ItemType<CE_ChestKey>());
                                break;
                        }
                    }
                }
            }
        }
    }
    public class CE_GenerationPass : GenPass
    {
        public CE_GenerationPass(string name, float loadWeight) : base(name, loadWeight)
        {
        }
        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = EtherioniteSystem.ExampleOrePassMessage.Value;
           for (int i = 0; i < 15; i++)
            {
                    int x = WorldGen.genRand.Next((int)GenVars.JungleX + 500, (int)GenVars.JungleX + 900);
                    int y = WorldGen.genRand.Next((int)GenVars.worldSurfaceLow + 200, (int)GenVars.worldSurfaceLow + 400);
                    if (x > WorldGen.genRand.Next((int)GenVars.JungleX + 800, (int)GenVars.JungleX + 900) || x < WorldGen.genRand.Next((int)GenVars.JungleX + 500, (int)GenVars.JungleX + 600))
                    {
                        WorldGen.TileRunner(x, y, WorldGen.genRand.Next(128, 264), WorldGen.genRand.Next(1, 2), ModContent.TileType<Crimson_soil_sand>());
                    }
                    if (y > WorldGen.genRand.Next((int)GenVars.worldSurfaceLow + 350, (int)GenVars.worldSurfaceLow + 400) || y < WorldGen.genRand.Next((int)GenVars.worldSurfaceLow + 200, (int)GenVars.worldSurfaceLow + 250))
                    {
                        WorldGen.TileRunner(x, y, WorldGen.genRand.Next(64, 89), WorldGen.genRand.Next(1, 2), ModContent.TileType<Crimson_eden_soil>());
                    }
            }
        }
    }
    public class Tunnel_GenerationPass : GenPass
    {
        private float float_01 = 0f;
        public Tunnel_GenerationPass(string name, float loadWeight) : base(name, loadWeight)
        {
        }
        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = EtherioniteSystem.ExampleOrePassMessage.Value;
        }
        public static void ClearTunnel(int x, int y, int width, int height, float? widenFactor, bool clearWalls)
        {
            {
                const float NoiseCutoff = -0.2f;
                 int useHeight = height / 2;
                for (int i = x; i < x + width; i++)
                {
                    float value = (i + 1) / 2;
                    if (widenFactor.HasValue)
                        value = MathHelper.Lerp(value, 1, widenFactor.Value);
                    for (int j = 0; j < useHeight * value; ++j)
                    {
                        int realX = i + j;
                        if (realX >= x + width)
                            realX -= width;
                        Tile tile = Main.tile[realX, y - j];
                        if (tile.HasTile && value > NoiseCutoff)
                        {
                            if (clearWalls)
                            {
                                tile.ClearEverything();
                                Main.tile[realX, y - j + 1].Clear(Terraria.DataStructures.TileDataType.Wall);
                                Main.tile[realX, y - j - 1].Clear(Terraria.DataStructures.TileDataType.Wall);
                            }
                            else
                            {
                                tile.Clear(Terraria.DataStructures.TileDataType.Tile);
                            }
                        }
                        realX = Math.Abs(i - j) % Main.maxTilesX;
                        if (realX < x)
                            realX += width;
                        if (Main.tile[realX, y + j].HasTile && value > NoiseCutoff)
                        {
                            Tile tile2 = Main.tile[realX, y + j];
                            if (clearWalls)
                            {
                                tile2.ClearEverything();
                                Main.tile[realX, y + j + 1].Clear(Terraria.DataStructures.TileDataType.Wall);
                                Main.tile[realX, y + j - 1].Clear(Terraria.DataStructures.TileDataType.Wall);
                            }
                            else
                            {
                                tile2.Clear(Terraria.DataStructures.TileDataType.Tile);
                            }
                        }
                    }
                }
            }
        }
    }*/
}
