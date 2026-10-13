using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
namespace jarkos
{
    public class Jarkos_gen : ModSystem
    {
    #region Worm Cave Generator
    public class WormCaveGenerator
    {
        private readonly Noises.PerlinNoise noise;
        private readonly Noises.PerlinNoise noiseWidth;
        private readonly Random random;
        public WormCaveGenerator(int seed)
        {
            noise = new Noises.PerlinNoise(seed);
            noiseWidth = new Noises.PerlinNoise(seed + 1000);
            random = new Random(seed);
        }
        public class WormParams
        {
            public int StartX { get; set; }
            public int StartY { get; set; }
            public int Length { get; set; } = 200;
            public double BaseRadius { get; set; } = 4;
            public double RadiusVariation { get; set; } = 2;
            public double Curviness { get; set; } = 0.1;
            public double NoiseScale { get; set; } = 0.02;
            public double DirectionBias { get; set; } = 0;
            public bool CanBranch { get; set; } = true;
            public double BranchChance { get; set; } = 0.02;
            public int MaxBranchDepth { get; set; } = 3;
        }
        public void GenerateCaveSystem(int startX, int startY, int numWorms = 5)
        {
            for (int i = 0; i < numWorms; i++)
            {
                var param = new WormParams
                {
                    StartX = startX + random.Next(-100, 100), StartY = startY + random.Next(-50, 50), Length = random.Next(150, 400), BaseRadius = random.Next(3, 7), Curviness = 0.05 + random.NextDouble() * 0.15, DirectionBias = random.NextDouble() * Math.PI * 2
                };
                GenerateWormCave(param, 0);
            }
        }
        public List<Vector2> GenerateWormCave(WormParams param, int branchDepth = 0)
        {
            var path = new List<Vector2>();
            double x = param.StartX;
            double y = param.StartY;
            double angle = param.DirectionBias;
            double stepSize = 2.0;
            for (int step = 0; step < param.Length; step++)
            {
                double noiseValue = noise.FractalNoise(x * param.NoiseScale, y * param.NoiseScale, 3);
                double turbulence = noise.Turbulence(x * param.NoiseScale * 2, y * param.NoiseScale * 2, 2) * 0.3;
                angle += noiseValue * param.Curviness + turbulence * 0.1;
                angle += Math.Sin(angle - Math.PI / 2) * 0.01;
                x += Math.Cos(angle) * stepSize;
                y += Math.Sin(angle) * stepSize;
                if (x < 50 || x > Main.maxTilesX - 50 || y < Main.worldSurface || y > Main.maxTilesY - 200)
                    break;
                path.Add(new Vector2((float)x, (float)y));
                double radiusNoise = noiseWidth.FractalNoise(x * param.NoiseScale * 3, y * param.NoiseScale * 3, 2);
                double radius = param.BaseRadius + radiusNoise * param.RadiusVariation;
                radius = Math.Max(2, radius);
                CarveOrganicCircle((int)x, (int)y, radius);
                if (param.CanBranch && branchDepth < param.MaxBranchDepth)
                {
                    if (random.NextDouble() < param.BranchChance)
                    {
                        var branchParams = new WormParams
                        {
                            StartX = (int)x, StartY = (int)y, Length = param.Length / 2, BaseRadius = param.BaseRadius * 0.7, RadiusVariation = param.RadiusVariation * 0.7, Curviness = param.Curviness * 1.5, DirectionBias = angle + (random.NextDouble() - 0.5) * Math.PI, CanBranch = true, BranchChance = param.BranchChance * 0.5, MaxBranchDepth = param.MaxBranchDepth
                        };
                        GenerateWormCave(branchParams, branchDepth + 1);
                    }
                }
                if (random.NextDouble() < 0.03)
                {
                    CarveOrganicBlob((int)x, (int)y, radius * 2, radius * 1.5);
                }
            }
            return path;
        }
        private void CarveOrganicCircle(int centerX, int centerY, double radius)
        {
            int intRadius = (int)Math.Ceiling(radius) + 2;
            for (int dx = -intRadius; dx <= intRadius; dx++)
            {
                for (int dy = -intRadius; dy <= intRadius; dy++)
                {
                    int tileX = centerX + dx;
                    int tileY = centerY + dy;
                    if (!WorldGen.InWorld(tileX, tileY, 10))
                        continue;
                    double distance = Math.Sqrt(dx * dx + dy * dy);
                    double edgeNoise = noise.FractalNoise(tileX * 0.2, tileY * 0.2, 2) * 1.5;
                    double adjustedRadius = radius + edgeNoise;
                    if (distance < adjustedRadius)
                    {
                        Tile tile = Main.tile[tileX, tileY];
                        if (IsProtectedTile(tile))
                            continue;
                        tile.HasTile = false;
                        tile.TileType = TileID.Dirt;
                        if (distance > adjustedRadius - 1)
                        {
                            if (random.NextDouble() < 0.3)
                            {
                            }
                        }
                    }
                }
            }
        }
        private void CarveOrganicBlob(int centerX, int centerY, double radiusX, double radiusY)
        {
            int maxRadius = (int)Math.Max(radiusX, radiusY) + 3;
            for (int dx = -maxRadius; dx <= maxRadius; dx++)
            {
                for (int dy = -maxRadius; dy <= maxRadius; dy++)
                {
                    int tileX = centerX + dx;
                    int tileY = centerY + dy;
                    if (!WorldGen.InWorld(tileX, tileY, 10))
                        continue;
                    double normalizedDist = Math.Sqrt((dx * dx) / (radiusX * radiusX) + (dy * dy) / (radiusY * radiusY));
                    double warpedNoise = noise.WarpedNoise(tileX * 0.1, tileY * 0.1, 0.8);
                    normalizedDist += warpedNoise * 0.3;
                    if (normalizedDist < 1.0)
                    {
                        Tile tile = Main.tile[tileX, tileY];
                        if (!IsProtectedTile(tile))
                        {
                            tile.HasTile = false;
                            tile.TileType = TileID.Dirt;
                        }
                    }
                }
            }
        }
        private bool IsProtectedTile(Tile tile)
        {
            if (!tile.HasTile) return false;
            return tile.TileType == TileID.LihzahrdBrick || tile.TileType == TileID.BlueDungeonBrick || tile.TileType == TileID.GreenDungeonBrick || tile.TileType == TileID.PinkDungeonBrick || tile.TileType == TileID.Containers || tile.TileType == TileID.Containers2;
        }
        }
    }
    #endregion
    #region Organic Cave Rooms
    public class OrganicRoomGenerator
    {
        private readonly Noises.PerlinNoise noise;
        private readonly Random random;
        public OrganicRoomGenerator(int seed)
        {
            noise = new Noises.PerlinNoise(seed);
            random = new Random(seed);
        }
        public enum RoomType
        {
            Cave, Grotto, Hollow, Crevice, Chamber
        }
        public void GenerateRoom(int centerX, int centerY, RoomType type, int sizeMin = 15, int sizeMax = 30)
        {
            int width = random.Next(sizeMin, sizeMax);
            int height = random.Next(sizeMin, sizeMax);
            switch (type)
            {
                case RoomType.Cave:
                    GenerateCaveRoom(centerX, centerY, width, height);
                    break;
                case RoomType.Grotto:
                    GenerateGrottoRoom(centerX, centerY, width, height);
                    break;
                case RoomType.Hollow:
                    GenerateHollowRoom(centerX, centerY, Math.Min(width, height));
                    break;
                case RoomType.Crevice:
                    GenerateCrevice(centerX, centerY, width, height);
                    break;
                case RoomType.Chamber:
                    GenerateChamberWithPillars(centerX, centerY, width, height);
                    break;
            }
        }
        private void GenerateCaveRoom(int centerX, int centerY, int width, int height)
        {
            double noiseScale = 0.08 + random.NextDouble() * 0.04;
            double noiseOffset = random.NextDouble() * 1000;
            int maxDist = Math.Max(width, height);
            for (int dx = -maxDist; dx <= maxDist; dx++)
            {
                for (int dy = -maxDist; dy <= maxDist; dy++)
                {
                    int tileX = centerX + dx;
                    int tileY = centerY + dy;
                    if (!WorldGen.InWorld(tileX, tileY, 10))
                        continue;
                    double normalizedX = (double)dx / width;
                    double normalizedY = (double)dy / height;
                    double ellipticDist = Math.Sqrt(normalizedX * normalizedX + normalizedY * normalizedY);
                    double noiseValue = noise.FractalNoise((tileX + noiseOffset) * noiseScale, (tileY + noiseOffset) * noiseScale, 4, 0.5, 2.0);
                    double warp = noise.WarpedNoise(tileX * noiseScale * 0.5, tileY * noiseScale * 0.5, 0.6) * 0.2;
                    double threshold = 0.8 + noiseValue * 0.4 + warp;
                    if (ellipticDist < threshold)
                    {
                        CarveTile(tileX, tileY);
                        if (ellipticDist > threshold - 0.1)
                        {
                            AddEdgeDetails(tileX, tileY);
                        }
                    }
                }
            }
            AddCaveFormations(centerX, centerY, width, height);
        }
        private void GenerateGrottoRoom(int centerX, int centerY, int width, int height)
        {
            GenerateCaveRoom(centerX, centerY, width, height);
            int waterLevel = centerY + (int)(height * 0.3);
            for (int dx = -width; dx <= width; dx++)
            {
                for (int dy = 0; dy <= height; dy++)
                {
                    int tileX = centerX + dx;
                    int tileY = waterLevel + dy / 2;
                    if (!WorldGen.InWorld(tileX, tileY, 10))
                        continue;
                    Tile tile = Main.tile[tileX, tileY];
                    if (!tile.HasTile)
                    {
                        tile.LiquidAmount = 255;
                        tile.LiquidType = LiquidID.Water;
                    }
                }
            }
            AddGlowingPlants(centerX, centerY, width, height);
        }
        private void GenerateHollowRoom(int centerX, int centerY, int radius)
        {
            double noiseScale = 0.12;
            for (int dx = -radius - 5; dx <= radius + 5; dx++)
            {
                for (int dy = -radius - 5; dy <= radius + 5; dy++)
                {
                    int tileX = centerX + dx;
                    int tileY = centerY + dy;
                    if (!WorldGen.InWorld(tileX, tileY, 10))
                        continue;
                    double distance = Math.Sqrt(dx * dx + dy * dy);
                    double surfaceNoise = noise.FractalNoise(tileX * noiseScale, tileY * noiseScale, 3) * (radius * 0.3);
                    double adjustedRadius = radius + surfaceNoise;
                    if (distance < adjustedRadius)
                    {
                        CarveTile(tileX, tileY);
                    }
                }
            }
            int numIslands = random.Next(2, 5);
            for (int i = 0; i < numIslands; i++)
            {
                double angle = random.NextDouble() * Math.PI * 2;
                double dist = random.NextDouble() * radius * 0.5;
                int islandX = centerX + (int)(Math.Cos(angle) * dist);
                int islandY = centerY + (int)(Math.Sin(angle) * dist);
                int islandSize = random.Next(3, 7);
                GenerateFloatingIsland(islandX, islandY, islandSize);
            }
        }
        private void GenerateCrevice(int centerX, int centerY, int width, int depth)
        {
            double noiseScale = 0.1;
            int halfWidth = width / 2;
            for (int dy = 0; dy < depth; dy++)
            {
                int tileY = centerY + dy;
                double depthFactor = 1.0 - (double)dy / depth;
                double baseWidth = halfWidth * (0.5 + depthFactor * 0.5);
                double widthNoise = noise.FractalNoise(centerX * noiseScale, tileY * noiseScale, 3) * halfWidth * 0.5;
                double currentWidth = baseWidth + widthNoise;
                currentWidth = Math.Max(2, currentWidth);
                double offsetNoise = noise.Noise(tileY * 0.05, 0) * halfWidth * 0.3;
                int currentCenterX = centerX + (int)offsetNoise;
                for (int dx = -(int)currentWidth; dx <= (int)currentWidth; dx++)
                {
                    int tileX = currentCenterX + dx;
                    if (!WorldGen.InWorld(tileX, tileY, 10))
                        continue;
                    double edgeNoise = noise.Noise(tileX * 0.2, tileY * 0.2);
                    double adjustedWidth = currentWidth + edgeNoise * 2;
                    if (Math.Abs(dx) < adjustedWidth)
                    {
                        CarveTile(tileX, tileY);
                    }
                }
            }
        }
        private void GenerateChamberWithPillars(int centerX, int centerY, int width, int height)
        {
            GenerateCaveRoom(centerX, centerY, width, height);
            var pillarPositions = new List<Vector2>();
            double pillarNoiseScale = 0.15;
            for (int dx = -width + 3; dx <= width - 3; dx += 4)
            {
                for (int dy = -height + 3; dy <= height - 3; dy += 4)
                {
                    int tileX = centerX + dx;
                    int tileY = centerY + dy;
                    double pillarNoise = noise.FractalNoise(tileX * pillarNoiseScale, tileY * pillarNoiseScale, 2);
                    if (pillarNoise > 0.3)
                    {
                        pillarPositions.Add(new Vector2(tileX, tileY));
                    }
                }
            }
            foreach (var pos in pillarPositions)
            {
                GenerateOrganicPillar((int)pos.X, (int)pos.Y, random.Next(2, 4));
            }
        }
        private void GenerateOrganicPillar(int x, int y, int radius)
        {
            int ceiling = y;
            int floor = y;
            while (ceiling > 10 && !Main.tile[x, ceiling].HasTile)
                ceiling--;
            while (floor < Main.maxTilesY - 10 && !Main.tile[x, floor].HasTile)
                floor++;
            for (int ty = ceiling; ty <= floor; ty++)
            {
                double heightRatio = (double)(ty - ceiling) / (floor - ceiling);
                double bulge = Math.Sin(heightRatio * Math.PI);
                int currentRadius = (int)(radius * (0.5 + bulge * 0.5));
                double radiusNoise = noise.Noise(x * 0.3, ty * 0.3) * 1.5;
                currentRadius = Math.Max(1, currentRadius + (int)radiusNoise);
                for (int dx = -currentRadius; dx <= currentRadius; dx++)
                {
                    int tileX = x + dx;
                    if (!WorldGen.InWorld(tileX, ty, 5))
                        continue;
                    Tile tile = Main.tile[tileX, ty];
                    tile.HasTile = true;
                    tile.TileType = TileID.Stone;
                }
            }
        }
        private void GenerateFloatingIsland(int centerX, int centerY, int radius)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius / 2; dy <= radius / 2; dy++)
                {
                    int tileX = centerX + dx;
                    int tileY = centerY + dy;
                    if (!WorldGen.InWorld(tileX, tileY, 5))
                        continue;
                    double dist = Math.Sqrt(dx * dx / ((double)radius * radius) + dy * dy / ((double)(radius / 2) * (radius / 2)));
                    double noiseVal = noise.Noise(tileX * 0.2, tileY * 0.2) * 0.3;
                    if (dist + noiseVal < 1.0)
                    {
                        Tile tile = Main.tile[tileX, tileY];
                        tile.HasTile = true;
                        tile.TileType = TileID.Stone;
                    }
                }
            }
        }
        private void CarveTile(int x, int y)
        {
            if (!WorldGen.InWorld(x, y, 5)) return;
            Tile tile = Main.tile[x, y];
            if (tile.TileType == TileID.LihzahrdBrick || tile.TileType == TileID.BlueDungeonBrick)
                return;
            tile.HasTile = false;
            tile.TileType = TileID.Dirt;
        }
        private void AddEdgeDetails(int x, int y)
        {
            if (random.NextDouble() < 0.1)
            {
            }
        }
        private void AddCaveFormations(int centerX, int centerY, int width, int height)
        {
            for (int dx = -width; dx <= width; dx++)
            {
                if (random.NextDouble() > 0.15) continue;
                int x = centerX + dx;
                int y = centerY - height;
                while (y < centerY && !Main.tile[x, y].HasTile)
                    y++;
                if (y >= centerY) continue;
                int length = random.Next(2, 6);
                for (int i = 0; i < length; i++)
                {
                    if (!WorldGen.InWorld(x, y + i, 5)) break;
                    Tile tile = Main.tile[x, y + i];
                    tile.HasTile = true;
                    tile.TileType = TileID.Stone;
                }
            }
        }
        private void AddGlowingPlants(int centerX, int centerY, int width, int height)
        {
            for (int dx = -width; dx <= width; dx++)
            {
                if (random.NextDouble() > 0.2) continue;
                int x = centerX + dx;
                for (int y = centerY; y < centerY + height; y++)
                {
                    if (!WorldGen.InWorld(x, y, 5)) break;
                    Tile below = Main.tile[x, y + 1];
                    Tile current = Main.tile[x, y];
                    if (!current.HasTile && below.HasTile)
                    {
                        WorldGen.PlaceTile(x, y, TileID.MushroomPlants);
                        break;
                    }
                }
            }
        }
    }
    #endregion
    #region Ore Vein Generator
    public class OreVeinGenerator
    {
        private readonly Noises.PerlinNoise noise;
        private readonly Noises.PerlinNoise directionNoise;
        private readonly Random random;
        public OreVeinGenerator(int seed)
        {
            noise = new Noises.PerlinNoise(seed);
            directionNoise = new Noises.PerlinNoise(seed + 500);
            random = new Random(seed);
        }
        public class VeinConfig
        {
            public ushort OreType { get; set; } = TileID.Iron;
            public int MinDepth { get; set; } = 0;
            public int MaxDepth { get; set; } = 1000;
            public double Rarity { get; set; } = 0.02;
            public double NoiseThreshold { get; set; } = 0.6;
            public double NoiseScale { get; set; } = 0.05;
            public int MinVeinSize { get; set; } = 3;
            public int MaxVeinSize { get; set; } = 12;
            public bool FollowsStrata { get; set; } = true;
        }
        public void GenerateAllOres()
        {
            var oreConfigs = new List<VeinConfig>
            {
                new VeinConfig
                {
                    OreType = TileID.Copper, MinDepth = 0, MaxDepth = (int)(Main.maxTilesY * 0.5), Rarity = 0.03, NoiseThreshold = 0.5, NoiseScale = 0.06
                }, new VeinConfig
                {
                    OreType = TileID.Iron, MinDepth = (int)(Main.worldSurface + 50), MaxDepth = (int)(Main.maxTilesY * 0.7), Rarity = 0.025, NoiseThreshold = 0.55, NoiseScale = 0.05
                }, new VeinConfig
                {
                    OreType = TileID.Silver, MinDepth = (int)(Main.worldSurface + 150), MaxDepth = (int)(Main.maxTilesY * 0.8), Rarity = 0.02, NoiseThreshold = 0.6, NoiseScale = 0.04
                }, new VeinConfig
                {
                    OreType = TileID.Gold, MinDepth = (int)(Main.rockLayer), MaxDepth = (int)(Main.maxTilesY * 0.85), Rarity = 0.015, NoiseThreshold = 0.65, NoiseScale = 0.035, MaxVeinSize = 8
                }, new VeinConfig
                {
                    OreType = TileID.Hellstone, MinDepth = Main.maxTilesY - 200, MaxDepth = Main.maxTilesY - 50, Rarity = 0.04, NoiseThreshold = 0.45, NoiseScale = 0.08, FollowsStrata = false
                }
            };
            foreach (var config in oreConfigs)
            {
                GenerateOreWithNoise(config);
            }
        }
        public void GenerateOreWithNoise(VeinConfig config)
        {
            double offsetX = random.NextDouble() * 10000;
            double offsetY = random.NextDouble() * 10000;
            for (int x = 50; x < Main.maxTilesX - 50; x++)
            {
                for (int y = config.MinDepth; y < config.MaxDepth; y++)
                {
                    if (!WorldGen.InWorld(x, y, 5))
                        continue;
                    double baseNoise = noise.FractalNoise((x + offsetX) * config.NoiseScale, (y + offsetY) * config.NoiseScale, 4, 0.5, 2.0);
                    double ridgedNoise = noise.RidgedNoise((x + offsetX) * config.NoiseScale * 2, (y + offsetY) * config.NoiseScale * 2, 3);
                    double combinedNoise = baseNoise * 0.4 + ridgedNoise * 0.6;
                    if (config.FollowsStrata)
                    {
                        double strataPattern = Math.Sin(y * 0.05 + noise.Noise(x * 0.01, 0) * 10) * 0.2;
                        combinedNoise += strataPattern;
                    }
                    if (combinedNoise > config.NoiseThreshold)
                    {
                        if (random.NextDouble() < config.Rarity)
                        {
                            int veinSize = random.Next(config.MinVeinSize, config.MaxVeinSize);
                            GenerateSingleVein(x, y, config.OreType, veinSize);
                        }
                    }
                }
            }
        }
        private void GenerateSingleVein(int startX, int startY, ushort oreType, int size)
        {
            var placedTiles = new HashSet<(int x, int y)>();
            var frontier = new Queue<(int x, int y, int depth)>();
            frontier.Enqueue((startX, startY, 0));
            double veinAngle = directionNoise.Noise(startX * 0.1, startY * 0.1) * Math.PI;
            Vector2 veinDirection = new Vector2((float)Math.Cos(veinAngle), (float)Math.Sin(veinAngle));
            while (frontier.Count > 0 && placedTiles.Count < size)
            {
                var (x, y, depth) = frontier.Dequeue();
                if (placedTiles.Contains((x, y)))
                    continue;
                if (!WorldGen.InWorld(x, y, 5))
                    continue;
                Tile tile = Main.tile[x, y];
                if (!tile.HasTile || !IsReplaceableTile(tile.TileType))
                    continue;
                double distanceFactor = 1.0 - (double)depth / size;
                double placementNoise = noise.Noise(x * 0.5, y * 0.5);
                if (random.NextDouble() < distanceFactor + placementNoise * 0.3)
                {
                    tile.TileType = oreType;
                    placedTiles.Add((x, y));
                    var neighbors = GetWeightedNeighbors(x, y, veinDirection);
                    foreach (var (nx, ny) in neighbors)
                    {
                        if (!placedTiles.Contains((nx, ny)))
                        {
                            frontier.Enqueue((nx, ny, depth + 1));
                        }
                    }
                }
            }
        }
        private List<(int x, int y)> GetWeightedNeighbors(int x, int y, Vector2 direction)
        {
            var neighbors = new List<(int x, int y, double weight)>
            {
                (x - 1, y, 1.0), (x + 1, y, 1.0), (x, y - 1, 1.0), (x, y + 1, 1.0), (x - 1, y - 1, 0.7), (x + 1, y - 1, 0.7), (x - 1, y + 1, 0.7), (x + 1, y + 1, 0.7)
            };
            var result = new List<(int x, int y)>();
            foreach (var (nx, ny, baseWeight) in neighbors)
            {
                Vector2 toNeighbor = new Vector2(nx - x, ny - y);
                toNeighbor.Normalize();
                double directionBonus = Vector2.Dot(toNeighbor, direction) * 0.5 + 0.5;
                double finalWeight = baseWeight * (0.5 + directionBonus);
                if (random.NextDouble() < finalWeight)
                {
                    result.Add((nx, ny));
                }
            }
            return result;
        }
        public void GenerateRiverVein(int startX, int startY, ushort oreType, int length, int thickness)
        {
            double x = startX;
            double y = startY;
            double angle = random.NextDouble() * Math.PI * 2;
            for (int step = 0; step < length; step++)
            {
                double angleChange = noise.Noise(x * 0.05, y * 0.05) * 0.5;
                angle += angleChange;
                angle += (random.NextDouble() - 0.5) * 0.2;
                x += Math.Cos(angle) * 1.5;
                y += Math.Sin(angle) * 1.5;
                int tileX = (int)x;
                int tileY = (int)y;
                if (!WorldGen.InWorld(tileX, tileY, 10))
                    break;
                PlaceOreBlob(tileX, tileY, oreType, thickness);
            }
        }
        private void PlaceOreBlob(int centerX, int centerY, ushort oreType, int radius)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    int x = centerX + dx;
                    int y = centerY + dy;
                    if (!WorldGen.InWorld(x, y, 5))
                        continue;
                    double distance = Math.Sqrt(dx * dx + dy * dy);
                    double edgeNoise = noise.Noise(x * 0.3, y * 0.3) * radius * 0.5;
                    if (distance < radius + edgeNoise)
                    {
                        Tile tile = Main.tile[x, y];
                        if (tile.HasTile && IsReplaceableTile(tile.TileType))
                        {
                            tile.TileType = oreType;
                        }
                    }
                }
            }
        }
        private bool IsReplaceableTile(ushort tileType)
        {
            return tileType == TileID.Stone || tileType == TileID.Dirt || tileType == TileID.Mud || tileType == TileID.ClayBlock || tileType == TileID.Ebonstone || tileType == TileID.Crimstone || tileType == TileID.Pearlstone || tileType == TileID.Sand || tileType == TileID.Ash;
        }
    }
    #endregion
    public class OrganicCavesPass : GenPass
    {
        public OrganicCavesPass(string name, float weight) : base(name, weight) { }
        protected override void ApplyPass(GenerationProgress progress, GameConfiguration config)
        {
            progress.Message = "Creating organic caves...";
            int seed = WorldGen._genRandSeed;
            var caveGen = new Jarkos_gen.WormCaveGenerator(seed);
            int numSystems = Main.maxTilesX / 400;
            for (int i = 0; i < numSystems; i++)
            {
                progress.Set((double)i / numSystems);
                int startX = 200 + (i * (Main.maxTilesX - 400) / numSystems);
                int startY = (int)(Main.worldSurface + 100 + WorldGen.genRand.Next(200));
                caveGen.GenerateCaveSystem(startX, startY, WorldGen.genRand.Next(3, 8));
            }
        }
    public class OrganicOrePass : GenPass
    {
        public OrganicOrePass(string name, float weight) : base(name, weight) { }
        protected override void ApplyPass(GenerationProgress progress, GameConfiguration config)
        {
            progress.Message = "Generate ore vein...";
            int seed = WorldGen._genRandSeed;
            var oreGen = new OreVeinGenerator(seed);
            oreGen.GenerateAllOres();
            progress.Set(1.0);
        }
    }
    public class OrganicRoomsPass : GenPass
    {
        public OrganicRoomsPass(string name, float weight) : base(name, weight) { }
        protected override void ApplyPass(GenerationProgress progress, GameConfiguration config)
        {
            progress.Message = "Creating underworld caves...";
            int seed = WorldGen._genRandSeed;
            var roomGen = new OrganicRoomGenerator(seed);
            int numRooms = Main.maxTilesX / 200;
            var roomTypes = new[] {
                OrganicRoomGenerator.RoomType.Cave, OrganicRoomGenerator.RoomType.Grotto, OrganicRoomGenerator.RoomType.Hollow, OrganicRoomGenerator.RoomType.Crevice, OrganicRoomGenerator.RoomType.Chamber
            };
            for (int i = 0; i < numRooms; i++)
            {
                progress.Set((double)i / numRooms);
                int x = WorldGen.genRand.Next(100, Main.maxTilesX - 100);
                int y = (int)(Main.rockLayer + WorldGen.genRand.Next(0, Main.maxTilesY - (int)Main.rockLayer - 300));
                var roomType = roomTypes[WorldGen.genRand.Next(roomTypes.Length)];
                roomGen.GenerateRoom(x, y, roomType, 15, 35);
            }
        }
    }
    #region OrganicWorldGen
    public class OrganicWorldGen : ModSystem
    {
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            int caveIndex = tasks.FindIndex(pass => pass.Name.Contains("Caves"));
            if (caveIndex != -1)
            {
                tasks.Insert(caveIndex + 1, new OrganicCavesPass("Organic Caves", 200f));
            }
            int oreIndex = tasks.FindIndex(pass => pass.Name.Contains("Shinies"));
            if (oreIndex != -1)
            {
                tasks.Insert(oreIndex + 1, new OrganicOrePass("Organic Ore Veins", 150f));
            }
        }
    }
    }
    #endregion
    #region Rooms
    public class VortexWorldGen : ModSystem
    {
        public static int MinVortexCount = 8;
        public static int MaxVortexCount = 15;
        public static int MinVortexRadius = 25;
        public static int MaxVortexRadius = 50;
        public static int MinArms = 3;
        public static int MaxArms = 6;
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
        }
    }
     public enum VortexType
    {
        Stone, Ice, Jungle, Hell
    }
    public struct VortexMaterials
    {
        public VortexType Type;
        public ushort BrickType;
        public ushort WallType;
        public ushort PlatformType;
        public ushort PillarType;
        public ushort ChestType;
        public int ChestStyle;
        public ushort TorchType;
    }
    #endregion
}
