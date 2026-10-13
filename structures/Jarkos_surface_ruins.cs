using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.IO;
using Terraria.WorldBuilding;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.items.accs.amulets;
using jarkos.items.accs.bloodmage;
using jarkos.items.accs.fishing;
using jarkos.items.accs.general;
namespace jarkos.structures
{
    public class Surface_ruins_System : ModSystem
    {
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            int shiniesIndex = tasks.FindIndex(genpass => genpass.Name.Equals("Shinies"));
            if (shiniesIndex != -1)
            {
                tasks.Insert(shiniesIndex + 1, new Jarkos_surface_ruins("Surface Ruins Generation", 250f));
            }
        }
    }
    public class Jarkos_surface_ruins : GenPass
    {
        public Jarkos_surface_ruins(string name, float loadWeight) : base(name, loadWeight)
        {
        }
        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "Generating Ancient Encampments...";
            SpawnEncampments();
        }
        private static int FindSurfaceY(int x)
        {
            int y = 50;
            while (y < Main.maxTilesY)
            {
                Tile tile = Main.tile[x, y];
                if (tile.HasTile && Main.tileSolid[tile.TileType])
                {
                    return y;
                }
                y++;
            }
            return -1;
        }
        private static bool IsValidLocation(int x, int y, int width, int height, ushort[] allowedGrassTypes)
        {
            if (x < 100 || x + width > Main.maxTilesX - 100 || y < 100 || y + height > Main.maxTilesY - 100)
            {
                return false;
            }
            if (Math.Abs(x - Main.spawnTileX) < 250)
            {
                return false;
            }
            int expectedSurfaceY = y + height;
            Tile groundTile = Main.tile[x + width / 2, expectedSurfaceY];
            if (!groundTile.HasTile)
            {
                return false;
            }
            bool typeAllowed = false;
            foreach (ushort t in allowedGrassTypes)
            {
                if (groundTile.TileType == t)
                {
                    typeAllowed = true;
                    break;
                }
            }
            if (!typeAllowed)
            {
                return false;
            }
            for (int dx = 0; dx < width; dx++)
            {
                int sY = FindSurfaceY(x + dx);
                if (sY == -1 || Math.Abs(sY - expectedSurfaceY) > 5)
                {
                    return false;
                }
            }
            return true;
        }
        private static bool PlaceRuinedChest(int x, int y, ushort tileType, int style)
        {
            for (int dx = 0; dx < 2; dx++)
            {
                for (int dy = 0; dy < 2; dy++)
                {
                    Tile tile = Main.tile[x + dx, y + dy];
                    tile.ClearTile();
                    tile.HasTile = false;
                }
            }
            for (int dx = 0; dx < 2; dx++)
            {
                Tile tile = Main.tile[x + dx, y + 2];
                tile.HasTile = true;
                tile.TileType = TileID.GrayBrick;
                tile.Slope = SlopeType.Solid;
                tile.IsHalfBlock = false;
                WorldGen.SquareTileFrame(x + dx, y + 2);
            }
            int chestIndex = WorldGen.PlaceChest(x, y, tileType, false, style);
            if (chestIndex < 0 || chestIndex >= Main.chest.Length)
            {
                chestIndex = Chest.CreateChest(x, y);
                if (chestIndex >= 0)
                {
                    for (int dx = 0; dx < 2; dx++)
                    {
                        for (int dy = 0; dy < 2; dy++)
                        {
                            Tile tile = Main.tile[x + dx, y + dy];
                            tile.HasTile = true;
                            tile.TileType = tileType;
                            tile.TileFrameX = (short)((dx + style * 2) * 18);
                            tile.TileFrameY = (short)(dy * 18);
                        }
                    }
                }
            }
            if (chestIndex >= 0 && chestIndex < Main.chest.Length)
            {
                Chest chest = Main.chest[chestIndex];
                if (chest != null)
                {
                    if (chest.item == null)
                    {
                        chest.item = new Item[Chest.maxItems];
                    }
                    for (int i = 0; i < chest.item.Length; i++)
                    {
                        if (chest.item[i] == null)
                        {
                            chest.item[i] = new Item();
                        }
                    }


                    int accessory = WorldGen.genRand.NextBool() ? ItemID.Aglet : ItemID.AnkletoftheWind;
                    chest.item[0].SetDefaults(accessory);


                    int oreType = WorldGen.genRand.NextBool() ? ItemID.IronOre : ItemID.LeadOre;
                    chest.item[1].SetDefaults(oreType);
                    chest.item[1].stack = WorldGen.genRand.Next(30, 61);


                    chest.item[2].SetDefaults(ItemID.Rope);
                    chest.item[2].stack = WorldGen.genRand.Next(10, 51);


                    chest.item[3].SetDefaults(ItemID.Torch);
                    chest.item[3].stack = WorldGen.genRand.Next(20, 41);


                    chest.item[4].SetDefaults(ItemID.SilverCoin);
                    chest.item[4].stack = WorldGen.genRand.Next(10, 100);


                    chest.item[5].SetDefaults(ItemID.CopperCoin);
                    chest.item[5].stack = WorldGen.genRand.Next(50, 100);

                    return true;
                }
            }
            return false;
        }
        private static void SpawnEncampments()
        {
            ushort[] allowedGrass = { TileID.Grass, TileID.CorruptGrass, TileID.CrimsonGrass, TileID.HallowedGrass };
            int count = 0;
            int maxSpawns = WorldGen.genRand.Next(8, 15);
            for (int i = 0; i < 10000 && count < maxSpawns; i++)
            {
                int x = WorldGen.genRand.Next(150, Main.maxTilesX - 150);
                int y = FindSurfaceY(x);
                if (y != -1 && IsValidLocation(x, y - 10, 20, 12, allowedGrass))
                {
                    GenerateEncampment(x, y - 10);
                    count++;
                }
            }
        }
        private static void GenerateEncampment(int x, int y)
        {
            int width = 20;
            int height = 12;
            for (int dx = -2; dx < width + 2; dx++)
            {
                for (int dy = -2; dy < height; dy++)
                {
                    Tile tile = Main.tile[x + dx, y + dy];
                    tile.ClearTile();
                    tile.HasTile = false;
                    tile.WallType = 0;
                }
            }
            for (int dx = -2; dx < width + 2; dx++)
            {
                Tile tile = Main.tile[x + dx, y + height];
                tile.HasTile = true;
                tile.TileType = TileID.GrayBrick;
                tile.Slope = SlopeType.Solid;
                tile.IsHalfBlock = false;
                WorldGen.SquareTileFrame(x + dx, y + height);
                int sY = FindSurfaceY(x + dx);
                if (sY != -1 && sY > y + height)
                {
                    for(int fillY = y + height + 1; fillY <= sY + 2; fillY++)
                    {
                        Tile fTile = Main.tile[x + dx, fillY];
                        if (!fTile.HasTile)
                        {
                            fTile.HasTile = true;
                            fTile.TileType = TileID.Dirt;
                            WorldGen.SquareTileFrame(x + dx, fillY);
                        }
                    }
                }
            }
            for (int dy = height - 1; dy >= height - 5; dy--)
            {
                int w = (dy - (height - 5));
                for (int dx = 2 - w/2; dx <= 2 + w/2; dx++)
                {
                    if (dx >= 0 && dx < width)
                    {
                        Main.tile[x + dx, y + dy].WallType = WallID.GrayBrick;
                    }
                }
            }
            for (int dy = height - 1; dy >= height - 7; dy--)
            {
                int w = (dy - (height - 7));
                for (int dx = 18 - w/2; dx <= 18 + w/2; dx++)
                {
                    if (dx >= 0 && dx < width)
                    {
                        Main.tile[x + dx, y + dy].WallType = WallID.GrayBrick;
                    }
                }
            }
            int peakX = 10;
            int peakY = -1;
            int bottomY = 7;
            for (int i = 0; i <= 8; i++)
            {
                for (int dy = peakY + i; dy <= bottomY; dy++)
                {
                    Tile leftTile = Main.tile[x + peakX - i, y + dy];
                    leftTile.HasTile = true;
                    leftTile.TileType = TileID.RedDynastyShingles;
                    leftTile.Slope = SlopeType.Solid;
                    leftTile.IsHalfBlock = false;
                    WorldGen.SquareTileFrame(x + peakX - i, y + dy);
                    if (i != 0)
                    {
                        Tile rightTile = Main.tile[x + peakX + i, y + dy];
                        rightTile.HasTile = true;
                        rightTile.TileType = TileID.RedDynastyShingles;
                        rightTile.Slope = SlopeType.Solid;
                        rightTile.IsHalfBlock = false;
                        WorldGen.SquareTileFrame(x + peakX + i, y + dy);
                    }
                }
            }
            for (int dy = height - 3; dy >= bottomY + 1; dy--)
            {
                Tile t1 = Main.tile[x + 5, y + dy];
                t1.HasTile = true;
                t1.TileType = TileID.WoodenBeam;
                t1.Slope = SlopeType.Solid;
                t1.IsHalfBlock = false;
                WorldGen.SquareTileFrame(x + 5, y + dy);
                Tile t2 = Main.tile[x + 15, y + dy];
                t2.HasTile = true;
                t2.TileType = TileID.WoodenBeam;
                t2.Slope = SlopeType.Solid;
                t2.IsHalfBlock = false;
                WorldGen.SquareTileFrame(x + 15, y + dy);
            }
            WorldGen.PlaceTile(x + 2, y + height - 3, TileID.Platforms, true, false, -1, 0);
            WorldGen.PlaceTile(x + 3, y + height - 3, TileID.Platforms, true, false, -1, 0);
            WorldGen.PlaceTile(x + 2, y + height - 4, TileID.Books, true, false, -1, 0);
            WorldGen.PlaceTile(x + 17, y + height - 5, TileID.Platforms, true, false, -1, 0);
            WorldGen.PlaceTile(x + 18, y + height - 5, TileID.Platforms, true, false, -1, 0);
            WorldGen.PlaceTile(x + 17, y + height - 6, TileID.Books, true, false, -1, 0);
            WorldGen.PlaceTile(x + 18, y + height - 6, TileID.Books, true, false, -1, 0);
            WorldGen.PlaceTile(x + 6, y + height - 1, TileID.Furnaces, true, false, -1, 0);
            WorldGen.PlaceTile(x + 13, y + height - 1, TileID.WorkBenches, true, false, -1, 0);
            WorldGen.PlaceTile(x + 11, y + height - 1, TileID.CopperCoinPile, true, false, -1, 0);
            WorldGen.PlaceTile(x + 12, y + height - 1, TileID.SilverCoinPile, true, false, -1, 0);
            PlaceRuinedChest(x + 9, y + height - 2, TileID.Containers, 0);
        }
    }
}
