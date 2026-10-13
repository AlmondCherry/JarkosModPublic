using jarkos.items.usetiles;
using jarkos.tiles;
using jarkos.tiles.CradleTiles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;

namespace jarkos.cradle_biome.system
{
	public class PetrifiedAbyssSystem : ModSystem
	{
	public static bool generated;
	public static bool announcedGeneration;
	public static int noiseSeedA;
	public static int noiseSeedB;
	private static bool wavyReady;
	private static int wavyCenterX;
	private static int wavyCenterY;
	private static int wavyRadiusX;
	private static int wavyRadiusY;
	public override void ClearWorld()
	{
		generated = false;
		announcedGeneration = false;
		wavyReady = false;
	}
	public override void SaveWorldData(TagCompound tag)
	{
		if (generated)
		{
			tag["generated"] = true;
			tag["announcedGeneration"] = announcedGeneration;
		}
	}
	public override void LoadWorldData(TagCompound tag)
	{
		generated = tag.GetBool("generated");
		announcedGeneration = tag.GetBool("announcedGeneration");
	}
	public override void PostWorldLoad()
	{
		if (Main.netMode == NetmodeID.MultiplayerClient || generated || !NPC.downedBoss2)
		{
			return;
		}

		GeneratePetrifiedAbyss();
		generated = true;
		announcedGeneration = true;
	}
	public override void NetSend(BinaryWriter writer)
	{
		writer.Write(generated);
		writer.Write(announcedGeneration);
	}
	public override void NetReceive(BinaryReader reader)
	{
		generated = reader.ReadBoolean();
		announcedGeneration = reader.ReadBoolean();
	}
	public override void PostUpdateWorld() {
		if (Main.netMode == NetmodeID.MultiplayerClient || !NPC.downedBoss2)
		{
			return;
		}
		if (!generated)
		{
			GeneratePetrifiedAbyss();
			generated = true;
		}
		if (announcedGeneration)
		{
			return;
		}
		announcedGeneration = true;
		Color messageColor = new(180, 120, 220);
		if (Main.netMode == NetmodeID.Server)
		{
			ChatHelper.BroadcastChatMessage(NetworkText.FromKey("The Subterranean Cradle awakened in underground"), messageColor);
			NetMessage.SendData(MessageID.WorldData);
		}
		else
		{
			Main.NewText(Language.GetTextValue("The Subterranean Cradle awakened in underground"), messageColor);
		}
	}
	public static void GeneratePetrifiedAbyss()
	{
		UnifiedRandom random = Main.rand;
		noiseSeedA = random.Next(100000);
		noiseSeedB = random.Next(100000);

		int depthstoneType = ModContent.TileType<PetrifiedDepthstoneTile>();
		ushort abyssWall = (ushort)ModContent.WallType<PetrifiedAbyssWall>();

		int centerX = Main.maxTilesX / 2;
		int underworldTop = Main.UnderworldLayer;
		int radiusX = Math.Clamp(Main.maxTilesX / 8, 460, 900);
		int radiusY = Math.Clamp(Main.maxTilesY / 11, 150, 250);

		int minCenterY = (int)Main.rockLayer + radiusY + 80;
		int maxCenterY = underworldTop - radiusY - 40;
		int centerY = minCenterY <= maxCenterY ? Math.Clamp(underworldTop - radiusY - 100, minCenterY, maxCenterY) : (minCenterY + maxCenterY) / 2;

		wavyCenterX = centerX;
		wavyCenterY = centerY;
		wavyRadiusX = radiusX;
		wavyRadiusY = radiusY;
		wavyReady = true;

		SculptLivingCaverns(centerX, centerY, radiusX, radiusY, depthstoneType, abyssWall, random);

		PlacedChamber heart = new(centerX, centerY, Math.Max(40, radiusX / 6), Math.Max(28, radiusY / 4), true);
		CarveHeartChamber(heart, abyssWall, random);

		List<PlacedChamber> chambers = PlaceRingChambers(centerX, centerY, radiusX, radiusY, abyssWall, random);

		foreach (PlacedChamber chamber in chambers)
		{
			CarveWormTunnel(heart.X, heart.Y, chamber.X, chamber.Y, abyssWall, random);
		}

		for (int i = 0; i < chambers.Count; i++)
		{
			PlacedChamber a = chambers[i];
			PlacedChamber b = chambers[(i + 1) % chambers.Count];

			if (random.NextBool(3)) continue;
			CarveWormTunnel(a.X, a.Y, b.X, b.Y, abyssWall, random);
		}

		CarveExitTunnels(centerX, centerY, radiusX, radiusY, abyssWall, random);
		GrowEvilVeins(heart, radiusX, radiusY, random);
		ScatterEvilOre(centerX, centerY, radiusX, radiusY, depthstoneType, random);
		ScatterPrehardmodeOres(centerX, centerY, radiusX, radiusY, depthstoneType, random);
		CleanupLooseBlocks(centerX, centerY, radiusX, radiusY);
		FillAbyssWalls(centerX, centerY, radiusX, radiusY, abyssWall);
		CarveWavyWallPattern(centerX, centerY, radiusX, radiusY);
		BuildHeartInterior(heart, random);

		foreach (PlacedChamber chamber in chambers)
		{
			BuildChamberInterior(chamber, random);
		}

		FillPuddles(chambers, random);
		FillDepressionPools(centerX, centerY, radiusX, radiusY, random);

		List<PlacedChamber> allRooms = new(chambers) { heart };

		PlaceAllChests(heart, chambers, centerX, centerY, radiusX, radiusY, depthstoneType, abyssWall, random);
		BuildOuterVines(centerX, centerY, radiusX, radiusY, random);
		FlattenSlopes(centerX - radiusX - 20, centerY - radiusY - 20, radiusX * 2 + 40, radiusY * 2 + 40);
		PlantAbyssVegetation(centerX, centerY, radiusX, radiusY, random);
		PlaceAbyssWallTorches(centerX, centerY, radiusX, radiusY, random);
		FrameArea(centerX - radiusX - 20, centerY - radiusY - 20, radiusX * 2 + 40, radiusY * 2 + 40);

		foreach (PlacedChamber chamber in allRooms)
		{
			PopulateChamberWallDecor(chamber, random);
		}

		if (Main.netMode == NetmodeID.Server)
		{
			SendAbyssTiles(centerX, centerY, radiusX, radiusY);
		}
	}
	public static void SculptLivingCaverns(int centerX, int centerY, int radiusX, int radiusY, int depthstoneType, ushort abyssWall, UnifiedRandom random)
	{
		for (int x = centerX - radiusX; x <= centerX + radiusX; x++)
		{
			for (int y = centerY - radiusY; y <= centerY + radiusY; y++)
			{
				if (!InWorld(x, y)) continue;

				double normalized = EllipseValue(x, y, centerX, centerY, radiusX, radiusY);
				double angle = Math.Atan2((y - centerY) / (double)radiusY, (x - centerX) / (double)radiusX);
				double edge = OuterEdgeFactor(angle);

				if (normalized > edge + 0.15) continue;
				if (normalized > edge - 0.1) {
					double fade = (edge + 0.15 - normalized) / 0.25;
					if (random.NextDouble() > fade) continue;
				}

				Tile tile = Framing.GetTileSafely(x, y);
				double cave = CaveNoise(x, y);
				double depthFactor = 1.0 - normalized / Math.Max(0.01, edge);
				double threshold = -0.15 + depthFactor * 0.45;

				if (cave < threshold)
				{
					ClearTile(tile);
				}
				else
				{
					SetTile(tile, cave > threshold + 0.55 && random.NextBool(6) ? ModContent.TileType<PetrifiedSandTile>() : depthstoneType);
				}
				tile.WallType = abyssWall;
			}
		}
	}
	public static double CaveNoise(int x, int y)
	{
		double a = noiseSeedA * 0.001;
		double b = noiseSeedB * 0.001;
		double n = 0.0;
		n += Math.Sin(x * 0.045 + a) * Math.Cos(y * 0.052 + b) * 0.9;
		n += Math.Sin(x * 0.019 + y * 0.023 + a * 2.0) * 0.7;
		n += Math.Sin(x * 0.090 - y * 0.075 + b * 3.0) * 0.35;
		n += Math.Cos(x * 0.011 - a) * Math.Sin(y * 0.013 + b) * 0.55;
		return n;
	}
	public static void CarveHeartChamber(PlacedChamber heart, ushort abyssWall, UnifiedRandom random)
	{
		int depthstoneType = ModContent.TileType<PetrifiedDepthstoneTile>();
		for (int x = heart.X - heart.RadiusX - 6; x <= heart.X + heart.RadiusX + 6; x++)
		{
			for (int y = heart.Y - heart.RadiusY - 6; y <= heart.Y + heart.RadiusY + 6; y++)
			{
				if (!InWorld(x, y)) continue;
				double v = EllipseValue(x, y, heart.X, heart.Y, heart.RadiusX, heart.RadiusY);
				Tile tile = Framing.GetTileSafely(x, y);
				if (v <= 0.82)
				{
					ClearTile(tile);
					tile.WallType = abyssWall;
				}
				else if (v <= 1.08)
				{
					SetTile(tile, random.NextBool(4) ? TileID.Ebonstone : (random.NextBool(7) ? TileID.Crimstone : depthstoneType));
					tile.WallType = abyssWall;
				}
			}
		}
		int coreR = Math.Max(5, heart.RadiusY / 5);
		for (int dx = -coreR; dx <= coreR; dx++)
		{
			for (int dy = -coreR; dy <= coreR; dy++)
			{
				if (dx * dx + dy * dy > coreR * coreR) continue;

				int tx = heart.X + dx;
				int ty = heart.Y - heart.RadiusY / 2 + dy;
				if (!InWorld(tx, ty)) continue;

				Tile tile = Framing.GetTileSafely(tx, ty);
				SetTile(tile, dx * dx + dy * dy > (coreR - 2) * (coreR - 2) ? TileID.Ebonstone : TileID.Demonite);
			}
		}
	}
	public static List<PlacedChamber> PlaceRingChambers(int centerX, int centerY, int radiusX, int radiusY, ushort abyssWall, UnifiedRandom random)
	{
		var chambers = new List<PlacedChamber>();
		int count = random.Next(5, 8);
		double startAngle = random.NextDouble() * MathHelper.TwoPi;

		for (int i = 0; i < count; i++)
		{
			double angle = startAngle + MathHelper.TwoPi * i / count + random.NextDouble() * 0.35 - 0.175;
			double ring = 0.48 + random.NextDouble() * 0.22;

			int cx = centerX + (int)(Math.Cos(angle) * radiusX * ring);
			int cy = centerY + (int)(Math.Sin(angle) * radiusY * ring);
			int rx = random.Next(28, 48);
			int ry = random.Next(18, 30);

			PlacedChamber chamber = new(cx, cy, rx, ry, false);
			chambers.Add(chamber);

			CarveBlobChamber(chamber, abyssWall);
		}
		return chambers;
	}
	public static void CarveBlobChamber(PlacedChamber chamber, ushort abyssWall)
	{
		for (int x = chamber.X - chamber.RadiusX - 4; x <= chamber.X + chamber.RadiusX + 4; x++)
		{
			for (int y = chamber.Y - chamber.RadiusY - 4; y <= chamber.Y + chamber.RadiusY + 4; y++)
			{
				if (!InWorld(x, y)) continue;
				double v = EllipseValue(x, y, chamber.X, chamber.Y, chamber.RadiusX, chamber.RadiusY);

				v += CaveNoise(x * 2, y * 2) * 0.06;

				if (v > 0.95) continue;

				Tile tile = Framing.GetTileSafely(x, y);
				ClearTile(tile);
				tile.WallType = abyssWall;
			}
		}
	}
	public static void CarveWormTunnel(int fromX, int fromY, int toX, int toY, ushort abyssWall, UnifiedRandom random)
	{
		Vector2 pos = new(fromX, fromY);
		Vector2 target = new(toX, toY);

		int radius = random.Next(4, 7);
		int safety = 4000;

		while (Vector2.Distance(pos, target) > 6f && safety-- > 0)
		{
			Vector2 dir = Vector2.Normalize(target - pos);
			float wobble = (float)(CaveNoise((int)pos.X, (int)pos.Y)) * 0.6f;

			dir = dir.RotatedBy(wobble * 0.5f + random.NextFloat(-0.25f, 0.25f));
			pos += dir * 2f;

			for (int dx = -radius; dx <= radius; dx++)
			{
				for (int dy = -radius; dy <= radius; dy++)
				{
					if (dx * dx + dy * dy > radius * radius) continue;

					int tx = (int)pos.X + dx;
					int ty = (int)pos.Y + dy;

					if (!InWorld(tx, ty)) continue;

					Tile tile = Framing.GetTileSafely(tx, ty);
					ClearTile(tile);

					tile.WallType = abyssWall;
				}
			}
			if (random.NextBool(6))
			{
				radius = Math.Clamp(radius + random.Next(-1, 2), 3, 8);
			}
		}
	}
	public static void CarveExitTunnels(int centerX, int centerY, int radiusX, int radiusY, ushort abyssWall, UnifiedRandom random)
	{
		int exits = random.Next(3, 5);

		for (int i = 0; i < exits; i++)
		{
			double angle = MathHelper.TwoPi * i / exits + random.NextDouble() * 0.6;
			int endX = centerX + (int)(Math.Cos(angle) * radiusX * 1.25);
			int endY = centerY + (int)(Math.Sin(angle) * radiusY * 1.25);
			int startX = centerX + (int)(Math.Cos(angle) * radiusX * 0.4);
			int startY = centerY + (int)(Math.Sin(angle) * radiusY * 0.4);

			CarveWormTunnel(startX, startY, endX, endY, abyssWall, random);
		}
	}
	public static void GrowEvilVeins(PlacedChamber heart, int radiusX, int radiusY, UnifiedRandom random)
	{
		GrowVeinSet(heart, radiusX, radiusY, TileID.Ebonstone, -1, random);
		GrowVeinSet(heart, radiusX, radiusY, TileID.Crimstone, +1, random);
	}
	public static void GrowVeinSet(PlacedChamber heart, int radiusX, int radiusY, int stoneType, int sideSign, UnifiedRandom random)
	{
		int veins = random.Next(5, 9);
		for (int i = 0; i < veins; i++)
		{
			double angle = sideSign > 0 ? random.NextDouble() * Math.PI - Math.PI / 2 : random.NextDouble() * Math.PI + Math.PI / 2;

			double px = heart.X;
			double py = heart.Y;

			int length = random.Next(radiusX / 3, radiusX * 3 / 4);
			int thickness = random.Next(2, 5);

			for (int s = 0; s < length; s++)
			{
				angle += random.NextDouble() * 0.4 - 0.2;
				px += Math.Cos(angle) * 1.5;
				py += Math.Sin(angle) * 1.0;
				int cx = (int)px, cy = (int)py;

				for (int dx = -thickness; dx <= thickness; dx++)
				{
					for (int dy = -thickness; dy <= thickness; dy++)
					{
						if (dx * dx + dy * dy > thickness * thickness) continue;
						int tx = cx + dx, ty = cy + dy;
						if (!InWorld(tx, ty)) continue;

						Tile tile = Framing.GetTileSafely(tx, ty);
						if (tile.HasTile && Main.tileSolid[tile.TileType] && tile.TileType != TileID.Demonite && tile.TileType != TileID.GrayBrick)
						{
							SetTile(tile, stoneType);
						}
					}
				}
				if (random.NextBool(10)) thickness = Math.Clamp(thickness + random.Next(-1, 2), 1, 5);
			}
		}
	}
	public static void ScatterEvilOre(int centerX, int centerY, int radiusX, int radiusY, int depthstoneType, UnifiedRandom random)
	{
		int deposits = random.Next(14, 22);
		for (int i = 0; i < deposits; i++) {
			int x = random.Next(centerX - radiusX + 20, centerX + radiusX - 20);
			int y = random.Next(centerY - radiusY + 20, centerY + radiusY - 20);

			if (!InWorld(x, y)) continue;
			Tile probe = Framing.GetTileSafely(x, y);
			if (!probe.HasTile) continue;
			int oreType = x < centerX ? TileID.Demonite : TileID.Crimtane;
			int size = random.Next(3, 7);

			for (int dx = -size; dx <= size; dx++)
			{
				for (int dy = -size; dy <= size; dy++)
				{
					if (dx * dx + dy * dy > size * size || random.NextBool(3)) continue;
					int tx = x + dx, ty = y + dy;
					if (!InWorld(tx, ty)) continue;

					Tile tile = Framing.GetTileSafely(tx, ty);
					if (tile.HasTile && (tile.TileType == depthstoneType || tile.TileType == TileID.Ebonstone || tile.TileType == TileID.Crimstone))
					{
						SetTile(tile, oreType);
					}
				}
			}
		}
	}
	public static int[] GetPrehardmodeOres()
	{
		var ores = new List<int>();

		int copper = WorldGen.SavedOreTiers.Copper;
		int iron = WorldGen.SavedOreTiers.Iron;
		int silver = WorldGen.SavedOreTiers.Silver;
		int gold = WorldGen.SavedOreTiers.Gold;
		
		if (copper > 0) ores.Add(copper); else { ores.Add(TileID.Copper); ores.Add(TileID.Tin); }
		if (iron > 0) ores.Add(iron); else { ores.Add(TileID.Iron); ores.Add(TileID.Lead); }
		if (silver > 0) ores.Add(silver); else { ores.Add(TileID.Silver); ores.Add(TileID.Tungsten); }
		if (gold > 0) ores.Add(gold); else { ores.Add(TileID.Gold); ores.Add(TileID.Platinum); }

		return ores.ToArray();
	}
	public static void ScatterPrehardmodeOres(int centerX, int centerY, int radiusX, int radiusY, int depthstoneType, UnifiedRandom random)
	{
		int sandType = ModContent.TileType<PetrifiedSandTile>();
		int[] ores = GetPrehardmodeOres();

		if (ores.Length == 0) return;

		int targetDeposits = random.Next(70, 101);
		int placed = 0;
		int attempts = 0;

		while (placed < targetDeposits && attempts++ < targetDeposits * 40)
		{
			int x = random.Next(centerX - radiusX + 12, centerX + radiusX - 12);
			int y = random.Next(centerY - radiusY + 12, centerY + radiusY - 12);

			if (!InWorld(x, y)) continue;
			bool dense = true;
			for (int dx = -3; dx <= 3 && dense; dx++)
			{
				for (int dy = -3; dy <= 3 && dense; dy++)
				{
					Tile t = Framing.GetTileSafely(x + dx, y + dy);
					if (!t.HasTile || !Main.tileSolid[t.TileType]) dense = false;
				}
			}

			if (!dense) continue;
			int oreType = ores[random.Next(ores.Length)];
			int blobs = random.Next(2, 5);
			double dirAngle = random.NextDouble() * MathHelper.TwoPi;
			double bx = x, by = y;

			for (int blob = 0; blob < blobs; blob++)
			{
				int size = random.Next(4, 9);
				for (int dx = -size; dx <= size; dx++)
				{
					for (int dy = -size; dy <= size; dy++)
					{
						if (dx * dx + dy * dy > size * size || random.NextBool(6)) continue;
						int tx = (int)bx + dx, ty = (int)by + dy;

						if (!InWorld(tx, ty)) continue;

						Tile tile = Framing.GetTileSafely(tx, ty);
						if (tile.HasTile && (tile.TileType == depthstoneType || tile.TileType == sandType || tile.TileType == TileID.Ebonstone || tile.TileType == TileID.Crimstone))
						{
							SetTile(tile, oreType);
						}
					}
				}

				dirAngle += random.NextDouble() * 0.7 - 0.35;
				double step = size * (0.9 + random.NextDouble() * 0.5);

				bx += Math.Cos(dirAngle) * step;
				by += Math.Sin(dirAngle) * step * 0.7;
			}
			placed++;
		}
	}
	public static bool IsInWavyBand(int x, int y)
	{
		if (!wavyReady) return false;

		if (x < wavyCenterX - wavyRadiusX || x > wavyCenterX + wavyRadiusX) return false;
		if (y < wavyCenterY - wavyRadiusY || y > wavyCenterY + wavyRadiusY) return false;

		const int bands = 6;

		for (int band = 0; band < bands; band++)
		{
			double baseY = wavyCenterY - wavyRadiusY + (band + 1) * (wavyRadiusY * 2.0) / (bands + 1);
			double phase = noiseSeedA * 0.01 + band * 2.399;
			double amp = wavyRadiusY * (0.06 + 0.05 * Math.Sin(noiseSeedB * 0.013 + band));

			double waveY = baseY + Math.Sin(x * 0.035 + phase) * amp + Math.Sin(x * 0.011 + phase * 1.7) * amp * 0.6;

			int thickness = 1 + (int)(1.5 + 1.5 * Math.Sin(x * 0.02 + phase * 3.1));

			if (Math.Abs(y - (int)waveY) <= thickness + 1) return true;
		}
		return false;
	}
	public static void CarveWavyWallPattern(int centerX, int centerY, int radiusX, int radiusY)
	{
		ushort abyssWall = (ushort)ModContent.WallType<PetrifiedAbyssWall>();
		ushort abyssDarkWall = (ushort)ModContent.WallType<PetrifiedAbyssDarkWall>();
		int bands = 6;

		for (int band = 0; band < bands; band++)
		{
			double baseY = centerY - radiusY + (band + 1) * (radiusY * 2.0) / (bands + 1);
			double phase = noiseSeedA * 0.01 + band * 2.399;
			double amp = radiusY * (0.06 + 0.05 * Math.Sin(noiseSeedB * 0.013 + band));

			for (int x = centerX - radiusX; x <= centerX + radiusX; x++)
			{
				double waveY = baseY + Math.Sin(x * 0.035 + phase) * amp + Math.Sin(x * 0.011 + phase * 1.7) * amp * 0.6;
				int thickness = 1 + (int)(1.5 + 1.5 * Math.Sin(x * 0.02 + phase * 3.1));

				for (int dy = -thickness; dy <= thickness; dy++)
				{
					int ty = (int)waveY + dy;
					if (!InWorld(x, ty)) continue;

					Tile tile = Framing.GetTileSafely(x, ty);

					if (tile.HasTile) continue;
					if (tile.WallType != abyssWall && tile.WallType != abyssDarkWall) continue;
					tile.WallType = WallID.None;
				}
			}
		}
	}
	public static void PlaceAbyssWallTorches(int centerX, int centerY, int radiusX, int radiusY, UnifiedRandom random)
	{
		ushort abyssWall = (ushort)ModContent.WallType<PetrifiedAbyssWall>();
		ushort abyssDarkWall = (ushort)ModContent.WallType<PetrifiedAbyssDarkWall>();

		for (int gx = centerX - radiusX + 6; gx <= centerX + radiusX - 6; gx += 22)
		{
			for (int gy = centerY - radiusY + 6; gy <= centerY + radiusY - 6; gy += 18)
			{
				int x = gx + random.Next(-5, 6);
				int y = gy + random.Next(-4, 5);

				if (!InWorld(x, y)) continue;
				if (IsInWavyBand(x, y)) continue;

				Tile tile = Framing.GetTileSafely(x, y);
				if (tile.HasTile) continue;
				if (tile.WallType != abyssWall && tile.WallType != abyssDarkWall) continue;

				WorldGen.PlaceTile(x, y, TileID.Torches, mute: true, forced: false, plr: -1, style: TorchID.Purple);
			}
		}
	}
	public static void BuildHeartInterior(PlacedChamber heart, UnifiedRandom random)
	{
		int depthstoneType = ModContent.TileType<PetrifiedDepthstoneTile>();
		int platformSurfaceY = heart.Y + heart.RadiusY / 3;

		for (int x = heart.X - heart.RadiusX; x <= heart.X + heart.RadiusX; x++)
		{
			for (int y = heart.Y - heart.RadiusY; y <= heart.Y + heart.RadiusY + 2; y++)
			{
				if (!InWorld(x, y)) continue;
				double v = EllipseValue(x, y, heart.X, heart.Y, heart.RadiusX, heart.RadiusY);

				if (v > 0.85) continue;
				Tile tile = Framing.GetTileSafely(x, y);

				if (y >= platformSurfaceY)
				{
					SetTile(tile, TileID.GrayBrick);
				}
				else if (tile.HasTile && tile.TileType == TileID.GrayBrick)
				{
					ClearTile(tile);
				}
			}
		}
		PlaceCenterStatue(heart.X, platformSurfaceY - 1);
		TryPlaceTorch(heart.X - heart.RadiusX / 2, platformSurfaceY - 1);
		TryPlaceTorch(heart.X + heart.RadiusX / 2, platformSurfaceY - 1);

		int stalactites = random.Next(5, 9);
		for (int i = 0; i < stalactites; i++)
		{
			int x = heart.X - heart.RadiusX + (i + 1) * heart.RadiusX * 2 / (stalactites + 1) + random.Next(-3, 4);
			int ceiling = FindCeilingEdge(x, heart.Y, heart.RadiusY + 6);

			if (ceiling <= 0) continue;
			BuildStalactite(x, ceiling + 1, random.Next(4, 9), depthstoneType, random);
		}
	}
	public static void PlaceCenterStatue(int x, int bottomY)
	{
		try
		{
			int statueType = ModContent.TileType<PunisherStatueTile>();
			for (int dx = 0; dx < 2; dx++)
			{
				for (int dy = 0; dy < 4; dy++)
				{
					int tx = x + dx, ty = bottomY - dy;
					if (!InWorld(tx, ty)) return;

					ClearTile(Framing.GetTileSafely(tx, ty));
				}
			}
			for (int dx = 0; dx < 2; dx++)
			{
				Tile ground = Framing.GetTileSafely(x + dx, bottomY + 1);
				SetTile(ground, TileID.GrayBrick);
			}
			WorldGen.PlaceObject(x, bottomY, statueType, mute: true);
			Tile check = Framing.GetTileSafely(x, bottomY);

			if (check.HasTile && check.TileType == statueType) return;
			for (int dx = 0; dx < 2; dx++)
			{
				for (int dy = 0; dy < 4; dy++)
				{
					int tx = x + dx, ty = bottomY - 3 + dy;
					Tile tile = Framing.GetTileSafely(tx, ty);

					tile.ClearTile();
					tile.HasTile = true;

					tile.TileType = (ushort)statueType;
					tile.TileFrameX = (short)(dx * 18);
					tile.TileFrameY = (short)(dy * 18);
				}
			}
		}
		catch
		{
		}
	}
	public static void BuildChamberInterior(PlacedChamber chamber, UnifiedRandom random)
	{
		int depthstoneType = ModContent.TileType<PetrifiedDepthstoneTile>();
		int floorY = FindFloorEdge(chamber.X, chamber.Y, chamber.RadiusY + 6);

		if (floorY < 0) floorY = chamber.Y + (int)(chamber.RadiusY * 0.8);

		floorY -= 1;

		int interiorLeft = chamber.X - chamber.RadiusX + 4;
		int interiorRight = chamber.X + chamber.RadiusX - 4;

		BuildTerrainLedge(interiorLeft + 2, floorY, interiorRight - interiorLeft - 4, depthstoneType, random);
		int tiers = random.Next(1, 4);

		for (int i = 0; i < tiers; i++)
		{
			int y = floorY - (i + 1) * random.Next(7, 11);
			if (y <= chamber.Y - chamber.RadiusY + 4) break;

			int width = random.Next(8, Math.Max(10, chamber.RadiusX));
			int left = chamber.X - width / 2 + random.Next(-chamber.RadiusX / 4, chamber.RadiusX / 4 + 1);

			BuildTerrainLedge(left, y, width, depthstoneType, random);
		}
		int decor = random.Next(2, 5);
		for (int i = 0; i < decor; i++)
		{
			int x = random.Next(interiorLeft + 3, interiorRight - 3);
			TryPlacePot(x, floorY - 1);
		}

		int torches = 1;
		for (int i = 0; i < torches; i++)
		{
			int x = interiorLeft + (interiorRight - interiorLeft) / 2 + random.Next(-4, 5);
			TryPlaceTorch(x, floorY - 3);
		}

		int stal = random.Next(2, 5);
		for (int i = 0; i < stal; i++)
		{
			int x = random.Next(interiorLeft, interiorRight + 1);
			int ceiling = FindCeilingEdge(x, chamber.Y, chamber.RadiusY + 6);
			if (ceiling > 0) BuildStalactite(x, ceiling + 1, random.Next(3, 7), depthstoneType, random);
		}
	}
	public static void FillPuddles(List<PlacedChamber> chambers, UnifiedRandom random)
	{
		foreach (PlacedChamber chamber in chambers)
		{
			for (int x = chamber.X - chamber.RadiusX + 4; x <= chamber.X + chamber.RadiusX - 4; x++)
			{
				int floorY = FindFloorEdge(x, chamber.Y, chamber.RadiusY + 6);
				if (floorY > 0 && random.NextBool(2))
				{
					Tile t = Framing.GetTileSafely(x, floorY - 1);
					if (!t.HasTile)
					{
						t.LiquidAmount = 255;
						t.LiquidType = LiquidID.Water;
					}
				}
			}
		}
	}
	public static void FillDepressionPools(int centerX, int centerY, int radiusX, int radiusY, UnifiedRandom random)
	{
		int maxPools = random.Next(20, 31);
		int pools = 0;
		int attempts = 0;

		while (pools < maxPools && attempts++ < 2000)
		{
			int x = random.Next(centerX - radiusX + 15, centerX + radiusX - 15);
			int floorY = FindFloorEdge(x, centerY, radiusY);
			if (floorY <= 0) continue;

			int rowsFilled = 0;
			for (int row = 0; row < 4; row++)
			{
				int y = floorY - 1 - row;
				if (!TryFillSealedRow(x, y, out _)) break;
				rowsFilled++;
			}

			if (rowsFilled > 0) pools++;
		}
	}
	public static bool TryFillSealedRow(int x, int y, out int width)
	{
		width = 0;
		const int maxWidth = 34;

		if (!InWorld(x, y)) return false;
		Tile start = Framing.GetTileSafely(x, y);

		if (start.HasTile) return false;
		if (!HasPoolSupport(x, y)) return false;

		int left = x;

		while (true)
		{
			int nx = left - 1;
			if (!InWorld(nx, y)) return false;
			Tile t = Framing.GetTileSafely(nx, y);

			if (t.HasTile && Main.tileSolid[t.TileType] && !Main.tileSolidTop[t.TileType]) break;
			if (t.HasTile) return false;
			if (!HasPoolSupport(nx, y)) return false;

			left = nx;
			if (x - left > maxWidth) return false;
		}
		int right = x;
		while (true)
		{
			int nx = right + 1;
			if (!InWorld(nx, y)) return false;
			Tile t = Framing.GetTileSafely(nx, y);

			if (t.HasTile && Main.tileSolid[t.TileType] && !Main.tileSolidTop[t.TileType]) break;
			if (t.HasTile) return false;
			if (!HasPoolSupport(nx, y)) return false;

			right = nx;
			if (right - x > maxWidth) return false;
		}
		width = right - left + 1;

		if (width < 2) return false;
		for (int fx = left; fx <= right; fx++)
		{
			Tile t = Framing.GetTileSafely(fx, y);
			if (!t.HasTile)
			{
				t.LiquidAmount = 255;
				t.LiquidType = LiquidID.Water;
			}
		}
		return true;
	}
	public static bool HasPoolSupport(int x, int y)
	{
		if (!InWorld(x, y + 1)) return false;
		Tile below = Framing.GetTileSafely(x, y + 1);

		if (below.HasTile && Main.tileSolid[below.TileType] && !Main.tileSolidTop[below.TileType]) return true;
		if (!below.HasTile && below.LiquidAmount >= 255 && below.LiquidType == LiquidID.Water) return true;
		return false;
	}
	public static void PlaceAllChests(PlacedChamber heart, List<PlacedChamber> chambers, int centerX, int centerY, int radiusX, int radiusY, int depthstoneType, ushort abyssWall, UnifiedRandom random)
	{
		int platformSurfaceY = heart.Y + heart.RadiusY / 3;
		TryPlaceAbyssChestSafe(heart.X - heart.RadiusX / 2, platformSurfaceY - 3, random, LootTier.Heart);
		TryPlaceAbyssChestSafe(heart.X + heart.RadiusX / 2, platformSurfaceY - 3, random, LootTier.Heart);

		foreach (PlacedChamber chamber in chambers)
		{
			int count = random.Next(1, 3);
			for (int i = 0; i < count; i++)
			{
				int x = chamber.X + random.Next(-chamber.RadiusX / 2, chamber.RadiusX / 2 + 1);
				int floorY = FindFloorEdge(x, chamber.Y, chamber.RadiusY + 6);

				if (floorY > 0)
				{
					TryPlaceAbyssChestSafe(x, floorY - 3, random, LootTier.Chamber);
				}
			}
		}
		int hidden = random.Next(2, 5);
		int attempts = 0;

		while (hidden > 0 && attempts++ < 300)
		{
			int x = random.Next(centerX - radiusX + 25, centerX + radiusX - 25);
			int y = random.Next(centerY - radiusY + 25, centerY + radiusY - 25);

			if (!InWorld(x, y)) continue;
			bool solid = true;
			for (int dx = -6; dx <= 6 && solid; dx++)
			{
				for (int dy = -5; dy <= 5 && solid; dy++)
				{
					Tile t = Framing.GetTileSafely(x + dx, y + dy);
					if (!t.HasTile || !Main.tileSolid[t.TileType]) solid = false;
				}
			}

			if (!solid) continue;

			for (int dx = -5; dx <= 5; dx++)
			{
				for (int dy = -4; dy <= 4; dy++)
				{
					if (dx * dx / 25f + dy * dy / 16f > 1f) continue;
					int tx = x + dx, ty = y + dy;

					if (!InWorld(tx, ty)) continue;

					Tile tile = Framing.GetTileSafely(tx, ty);
					ClearTile(tile);
					tile.WallType = abyssWall;
				}
			}
			if (TryPlaceAbyssChestSafe(x, y + 1, random, LootTier.Hidden))
			{
				TryPlaceTorch(x - 3, y);
				hidden--;
			}
		}
	}
	public enum LootTier
	{
		Chamber, Hidden, Heart
	}
	public static bool TryPlaceAbyssChestSafe(int x, int y, UnifiedRandom random, LootTier tier)
	{
		try
		{
			if (!InWorld(x - 1, y - 1) || !InWorld(x + 2, y + 3))
			{
				return false;
			}

			int depthstoneType = ModContent.TileType<PetrifiedDepthstoneTile>();

			for (int dx = -1; dx <= 2; dx++)
			{
				for (int dy = 0; dy <= 1; dy++)
				{
					Tile tile = Framing.GetTileSafely(x + dx, y + dy);
					ClearTile(tile);
					tile.LiquidAmount = 0;
				}
			}

			for (int dx = -1; dx <= 2; dx++) 
			{
				Tile ground = Framing.GetTileSafely(x + dx, y + 2);
				ground.ClearTile();

				ground.HasTile = true;
				ground.TileType = (ushort)depthstoneType;
				ground.Slope = SlopeType.Solid;
				ground.IsHalfBlock = false;
				ground.LiquidAmount = 0;

				WorldGen.SquareTileFrame(x + dx, y + 2);
			}

			int chestType = ModContent.TileType<CradleUpdatableChest>();
			int chestIndex = WorldGen.PlaceChest(x, y + 1, (ushort)chestType, notNearOtherChests: false, style: 0);
			if (chestIndex < 0)
			{
				chestIndex = PlaceAbyssChestDirect(x, y, chestType);
			}
			if (chestIndex < 0 || chestIndex >= Main.chest.Length)
			{
				return false;
			}
			Chest chest = Main.chest[chestIndex];
			if (chest == null)
			{
				return false;
			}
			EnsureChestItems(chest);
			FillAbyssChest(chest, random, tier);
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}
	public static void EnsureChestItems(Chest chest)
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
	}
	public static int PlaceAbyssChestDirect(int x, int y, int chestType)
	{
		int chestIndex = Chest.CreateChest(x, y);
		if (chestIndex < 0)
		{
			chestIndex = Chest.FindChest(x, y);
			if (chestIndex < 0)
			{
				return -1;
			}
		}
		for (int dx = 0; dx < 2; dx++)
		{
			for (int dy = 0; dy < 2; dy++)
			{
				if (!InWorld(x + dx, y + dy)) return -1;
				Tile tile = Framing.GetTileSafely(x + dx, y + dy);
				tile.ClearTile();

				tile.HasTile = true;
				tile.TileType = (ushort)chestType;
				tile.TileFrameX = (short)(dx * 18);
				tile.TileFrameY = (short)(dy * 18);

				tile.LiquidAmount = 0;
			}
		}
		if (chestIndex < 0 || chestIndex >= Main.chest.Length)
		{
			return -1;
		}
		if (Main.chest[chestIndex] == null)
		{
			Main.chest[chestIndex] = new Chest();
		}

		Chest chest = Main.chest[chestIndex];
		EnsureChestItems(chest);

		chest.x = x;
		chest.y = y;
		return chestIndex;
	}
	public static void FillAbyssChest(Chest chest, UnifiedRandom random, LootTier tier)
	{
		if (chest == null) return;
		EnsureChestItems(chest);
		int slot = 0;
		switch (tier)
		{
			case LootTier.Heart:
				slot = SafeAdd(chest, slot, random.NextBool()
					? (random.NextBool() ? ItemID.BandofStarpower : ItemID.BallOHurt)
					: (random.NextBool() ? ItemID.PanicNecklace : ItemID.CrimsonRod), 1, 1, random);

				slot = SafeAdd(chest, slot, random.NextBool() ? ItemID.DemoniteBar : ItemID.CrimtaneBar, 5, 12, random);
				break;
				
			case LootTier.Hidden:
				slot = SafeAdd(chest, slot, random.NextBool()
					? ItemID.Vilethorn : ItemID.TheRottedFork, 1, 1, random);

				slot = SafeAdd(chest, slot, random.NextBool() ? ItemID.ShadowScale : ItemID.TissueSample, 8, 15, random);
				break;

			default:
				slot = SafeAdd(chest, slot, random.NextBool() ? ItemID.DemoniteOre : ItemID.CrimtaneOre, 10, 25, random);
				slot = SafeAdd(chest, slot, random.NextBool() ? ItemID.RottenChunk : ItemID.Vertebrae, 5, 12, random);
				break;
		}

		slot = SafeAdd(chest, slot, ItemID.PurpleTorch, 8, 20, random);

		if (random.NextBool(2)) slot = SafeAdd(chest, slot, ItemID.CorruptSeeds, 3, 8, random);
		if (random.NextBool(2)) slot = SafeAdd(chest, slot, ItemID.CrimsonSeeds, 3, 8, random);
		if (random.NextBool(3)) slot = SafeAdd(chest, slot, ItemID.UnholyArrow, 25, 60, random);
		if (random.NextBool(3)) slot = SafeAdd(chest, slot, ItemID.PurificationPowder, 5, 15, random);
		
		slot = SafeAdd(chest, slot, ItemID.HealingPotion, 3, 6, random);
		if (random.NextBool(2)) slot = SafeAdd(chest, slot, random.NextBool() ? ItemID.BattlePotion : ItemID.NightOwlPotion, 1, 3, random);

		slot = SafeAdd(chest, slot, ItemID.GoldCoin, 2, 8, random);
	}
	public static int SafeAdd(Chest chest, int slot, int itemType, int minStack, int maxStack, UnifiedRandom random)
	{
		if (chest == null || chest.item == null || slot < 0 || slot >= chest.item.Length)
		{
			return slot;
		}

		if (itemType <= ItemID.None || itemType >= ItemLoader.ItemCount)
		{
			return slot;
		}

		if (chest.item[slot] == null)
		{
			chest.item[slot] = new Item();
		}

		int low = Math.Max(1, Math.Min(minStack, maxStack));
		int high = Math.Max(low, maxStack);

		chest.item[slot].SetDefaults(itemType);
		chest.item[slot].stack = random.Next(low, high + 1);
		return slot + 1;
	}
	public static void PlantAbyssVegetation(int centerX, int centerY, int radiusX, int radiusY, UnifiedRandom random)
	{
		int abyssWall = ModContent.WallType<PetrifiedAbyssWall>();
		int depthstoneType = ModContent.TileType<PetrifiedDepthstoneTile>();
		int sandType = ModContent.TileType<PetrifiedSandTile>();
		int weedType = ModContent.TileType<AbyssWeedTile>();
		int tallGrassType = ModContent.TileType<AbyssTallGrassTile>();
		int bushType = ModContent.TileType<GlowsporeBushTile>();
		int rootType = ModContent.TileType<PetrifiedRootTile>();
		int sporeFlowerType = ModContent.TileType<SporeFlowerTile>();

		for (int x = centerX - radiusX - 10; x <= centerX + radiusX + 10; x++)
		{
			for (int y = centerY - radiusY - 10; y <= centerY + radiusY + 10; y++)
			{
				if (!InWorld(x, y)) continue;
				Tile tile = Framing.GetTileSafely(x, y);

				if (tile.WallType != abyssWall || tile.HasTile) continue;
				Tile below = Framing.GetTileSafely(x, y + 1);

				bool goodGround = below.HasTile
					&& (below.TileType == depthstoneType || below.TileType == sandType || below.TileType == TileID.Ebonstone || below.TileType == TileID.Crimstone)
					&& !below.IsHalfBlock && below.Slope == SlopeType.Solid;
				if (goodGround)
				{
					if (random.NextBool(40))
					{
						if (WorldGen.PlaceObject(x, y, sporeFlowerType, true))
						{
							ModContent.GetInstance<SporeFlowerTileEntity>()?.Place(x - 1, y - 3);
						}
					}
					else if (random.NextBool(25))
					{
						WorldGen.PlaceObject(x, y, bushType, true);
					}
					else if (random.NextBool(12))
					{
						WorldGen.PlaceObject(x, y, tallGrassType, true, random.Next(3));
					}
					else if (random.NextBool(5))
					{
						WorldGen.PlaceObject(x, y, weedType, true, random.Next(3));
					}
				}
				if (!Framing.GetTileSafely(x, y).HasTile)
				{
					Tile above = Framing.GetTileSafely(x, y - 1);

					if (above.HasTile && (above.TileType == depthstoneType || above.TileType == sandType) && !above.IsHalfBlock && above.Slope == SlopeType.Solid) {
						if (random.NextBool(8))
						{
							WorldGen.PlaceObject(x, y, rootType, true, random.Next(3));
						}
					}
				}
			}
		}
	}
	public static void BuildOuterVines(int centerX, int centerY, int radiusX, int radiusY, UnifiedRandom random)
	{
		int vines = random.Next(50, 90);
		for (int i = 0; i < vines; i++)
		{
			int x = random.Next(centerX - radiusX + 8, centerX + radiusX - 8);
			int ceilingY = FindCeilingEdge(x, centerY, radiusY);

			if (ceilingY <= 0) continue;
			BuildHangingVine(x, ceilingY + 1, random.Next(6, 24), random);
		}
	}
	public static void BuildHangingVine(int x, int startY, int length, UnifiedRandom random)
	{
		int drift = 0;
		for (int y = 0; y < length; y++)
		{
			if (random.NextBool(5)) drift += random.Next(-1, 2);
			int tx = x + Math.Clamp(drift, -2, 2);
			int ty = startY + y;

			if (!InWorld(tx, ty) || Framing.GetTileSafely(tx, ty).HasTile) break;
			SetTile(Framing.GetTileSafely(tx, ty), y % 3 == 0 ? TileID.JungleVines : TileID.Vines);
		}
	}
	public static void PopulateChamberWallDecor(PlacedChamber chamber, UnifiedRandom random)
	{
		int interiorLeft = chamber.X - chamber.RadiusX + 4;
		int interiorRight = chamber.X + chamber.RadiusX - 4;
		int interiorTop = chamber.Y - chamber.RadiusY + 4;
		int interiorBottom = chamber.Y + chamber.RadiusY - 4;

		int torchCount = Math.Clamp(chamber.RadiusX / 30, 1, 3);

		for (int attempt = 0, placed = 0; placed < torchCount && attempt < torchCount * 10; attempt++)
		{
			int x = random.Next(interiorLeft, interiorRight + 1);
			int y = random.Next(interiorTop, interiorBottom + 1);

			if (TryPlacePurpleTorchOnWall(x, y)) placed++;
		}
	}
	public static bool TryPlacePurpleTorchOnWall(int x, int y)
	{
		if (!InWorld(x, y)) return false;
		if (IsInWavyBand(x, y)) return false;

		Tile tile = Framing.GetTileSafely(x, y);
		if (tile.HasTile || tile.WallType == WallID.None) return false;

		WorldGen.PlaceTile(x, y, TileID.Torches, mute: true, forced: false, plr: -1, style: TorchID.Purple);
		Tile check = Framing.GetTileSafely(x, y);

		return check.HasTile && check.TileType == TileID.Torches;
	}
	public static void BuildStalactite(int x, int startY, int height, int tileType, UnifiedRandom random)
	{
		for (int y = 0; y < height; y++)
		{
			int halfWidth = Math.Max(0, (height - y) / 4);
			for (int dx = -halfWidth; dx <= halfWidth; dx++)
			{
				int tx = x + dx, ty = startY + y;
				if (!InWorld(tx, ty)) continue;
				Tile tile = Framing.GetTileSafely(tx, ty);

				if (tile.HasTile) continue;
				SetTile(tile, random.NextBool(5) ? TileID.Ebonstone : tileType);
			}
		}
	}
	public static void BuildTerrainLedge(int startX, int y, int width, int tileType, UnifiedRandom random)
	{
		int currentY = y;
		int potType = ModContent.TileType<PetrifiedAbyssPot>();

		for (int i = 0; i < width; i++)
		{
			int x = startX + i;

			if (random.NextBool(6)) currentY += random.NextBool() ? 1 : -1;
			if (random.NextBool(8)) continue;
			if (!InWorld(x, currentY)) continue;

			int depth = random.Next(1, 4);

			for (int dy = 0; dy < depth; dy++)
			{
				Tile tile = Framing.GetTileSafely(x, currentY + dy);
				if (!tile.HasTile) SetTile(tile, tileType);
			}

			if (random.NextBool(9) && i > 0 && i < width - 1 && InWorld(x + 1, currentY - 2))
			{
				Tile t1 = Framing.GetTileSafely(x, currentY - 1);
				Tile t2 = Framing.GetTileSafely(x + 1, currentY - 1);
				Tile t3 = Framing.GetTileSafely(x, currentY - 2);
				Tile t4 = Framing.GetTileSafely(x + 1, currentY - 2);

				if (!t1.HasTile && !t2.HasTile && !t3.HasTile && !t4.HasTile)
				{
					WorldGen.PlaceTile(x, currentY - 1, potType, true, false, -1, 0);
				}
			}
		}
	}
	public static void TryPlacePot(int x, int floorY)
	{
		if (InWorld(x, floorY) && !Framing.GetTileSafely(x, floorY).HasTile && WorldGen.SolidTile(x, floorY + 1))
		{
			WorldGen.PlaceTile(x, floorY, TileID.Pots, true, true, -1, Main.rand.Next(0, 22));
		}
	}
	public static bool TryPlaceTorch(int x, int y)
	{
		for (int attempt = 0; attempt < 8; attempt++) 
		{
			int ty = y + attempt;

			if (!InWorld(x, ty) || IsInWavyBand(x, ty)) continue;
			if (!WorldGen.SolidTile(x, ty + 1) || Framing.GetTileSafely(x, ty).HasTile) continue;

			WorldGen.PlaceTile(x, ty, TileID.Torches, true, true, -1, TorchID.Purple);

			return Framing.GetTileSafely(x, ty).HasTile;
		}
		return false;
	}
	public static double OuterEdgeFactor(double angle)
	{
		double edge = 1.0 + Math.Sin(angle * 3.0 + 0.55) * 0.055 + Math.Sin(angle * 6.0 - 1.20) * 0.025;
		edge -= 0.25 * Gaussian(angle + Math.PI / 2.0, 0.22);
		edge -= 0.18 * Gaussian(angle - Math.PI / 2.0, 0.30);
		edge += 0.12 * Gaussian(angle, 0.22);
		edge += 0.12 * Gaussian(WrapAngle(angle - Math.PI), 0.22);
		return edge;
	}
	public static double Gaussian(double offset, double sigma)
	{
		double n = offset / sigma;
		return Math.Exp(-n * n);
	}
	public static double WrapAngle(double angle)
	{
		while (angle > Math.PI) angle -= 2.0 * Math.PI;
		while (angle < -Math.PI) angle += 2.0 * Math.PI;
		return angle;
	}
	public static void FillAbyssWalls(int centerX, int centerY, int radiusX, int radiusY, ushort abyssWall)
	{
		ushort abyssDarkWall = (ushort)ModContent.WallType<PetrifiedAbyssDarkWall>();

		for (int x = centerX - radiusX; x <= centerX + radiusX; x++)
		{
			for (int y = centerY - radiusY; y <= centerY + radiusY; y++)
			{
				if (!InWorld(x, y)) continue;

				double normalized = EllipseValue(x, y, centerX, centerY, radiusX, radiusY);
				double angle = Math.Atan2((y - centerY) / (double)radiusY, (x - centerX) / (double)radiusX);

				if (normalized <= OuterEdgeFactor(angle))
				{
					double wallNoise = Math.Sin(x * 0.15) * Math.Cos(y * 0.15) + Math.Sin(x * 0.05 - y * 0.08);
					Framing.GetTileSafely(x, y).WallType = wallNoise > 0.3 ? abyssDarkWall : abyssWall;
				}
			}
		}
	}
	public static void CleanupLooseBlocks(int centerX, int centerY, int radiusX, int radiusY)
	{
		for (int pass = 0; pass < 3; pass++)
		{
			for (int x = centerX - radiusX; x <= centerX + radiusX; x++)
			{
				for (int y = centerY - radiusY; y <= centerY + radiusY; y++)
				{
					if (!InWorld(x, y) || !Framing.GetTileSafely(x, y).HasTile) continue;
					int neighbors = 0;

					if (Framing.GetTileSafely(x - 1, y).HasTile) neighbors++;
					if (Framing.GetTileSafely(x + 1, y).HasTile) neighbors++;
					if (Framing.GetTileSafely(x, y - 1).HasTile) neighbors++;
					if (Framing.GetTileSafely(x, y + 1).HasTile) neighbors++;

					if (neighbors <= 1)
					{
						ClearTile(Framing.GetTileSafely(x, y));
					}
				}
			}
		}
	}
	public static int FindCeilingEdge(int x, int centerY, int radiusY)
	{
		for (int y = centerY - radiusY; y < centerY; y++)
		{
			if (InWorld(x, y) && InWorld(x, y + 1) && Framing.GetTileSafely(x, y).HasTile && !Framing.GetTileSafely(x, y + 1).HasTile)
			{
				return y;
			}
		}
		return -1;
	}
	public static int FindFloorEdge(int x, int centerY, int radiusY)
	{
		for (int y = centerY; y < centerY + radiusY + 10; y++)
		{
			if (InWorld(x, y) && InWorld(x, y + 1) && !Framing.GetTileSafely(x, y).HasTile && Framing.GetTileSafely(x, y + 1).HasTile && Main.tileSolid[Framing.GetTileSafely(x, y + 1).TileType])
			{
				return y + 1;
			}
		}
		return -1;
	}
	public static void SetTile(Tile tile, int tileType)
	{
		tile.HasTile = true;
		tile.TileType = (ushort)tileType;
		tile.LiquidAmount = 0;
		tile.Slope = SlopeType.Solid;
		tile.IsHalfBlock = false;
	}
	public static void ClearTile(Tile tile)
	{
		tile.ClearTile();
		tile.LiquidAmount = 0;
		tile.Slope = SlopeType.Solid;
		tile.IsHalfBlock = false;
	}
	public static void FlattenSlopes(int startX, int startY, int width, int height)
	{
		for (int x = Math.Max(10, startX); x < Math.Min(Main.maxTilesX - 10, startX + width); x++)
		{
			for (int y = Math.Max(10, startY); y < Math.Min(Main.maxTilesY - 10, startY + height); y++)
			{
				Tile tile = Framing.GetTileSafely(x, y);
				if (!tile.HasTile) continue;
				tile.Slope = SlopeType.Solid;
				tile.IsHalfBlock = false;
			}
		}
	}
	public static void FrameArea(int startX, int startY, int width, int height)
	{
		for (int x = Math.Max(10, startX); x < Math.Min(Main.maxTilesX - 10, startX + width); x++)
		{
			for (int y = Math.Max(10, startY); y < Math.Min(Main.maxTilesY - 10, startY + height); y++)
			{
				WorldGen.SquareTileFrame(x, y);
				WorldGen.SquareWallFrame(x, y);
			}
		}
	}
	public static void SendAbyssTiles(int centerX, int centerY, int radiusX, int radiusY)
	{
		for (int x = centerX - radiusX; x <= centerX + radiusX; x += 90)
		{
			for (int y = centerY - radiusY; y <= centerY + radiusY; y += 90)
			{
				NetMessage.SendTileSquare(-1, x, y, 100);
			}
		}
	}
	public static double EllipseValue(int x, int y, int centerX, int centerY, int radiusX, int radiusY)
	{
		double nx = (x - centerX) / (double)radiusX;
		double ny = (y - centerY) / (double)radiusY;

		double baseVal = nx * nx + ny * ny;
		double noise = Math.Sin(x * 0.1) * Math.Cos(y * 0.1) * 0.04 + Math.Sin(x * 0.04 + y * 0.03) * 0.04;

		return baseVal + noise;
	}
	public static bool InWorld(int x, int y)
	{
		return x > 10 && y > 10 && x < Main.maxTilesX - 10 && y < Main.maxTilesY - 210 && y < Main.UnderworldLayer - 20;
	}
	public readonly struct PlacedChamber
	{
		public readonly int X;
		public readonly int Y;
		public readonly int RadiusX;
		public readonly int RadiusY;
		public readonly bool Central;
		public PlacedChamber(int x, int y, int radiusX, int radiusY, bool central)
		{
			X = x;
			Y = y;
			
			RadiusX = radiusX;
			RadiusY = radiusY;
			Central = central;
		}
	}
}
}
