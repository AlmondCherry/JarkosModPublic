using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.GameplayChanges
{
	public class VeinMinerPlayer : ModPlayer
	{
		public bool veinMinerEquipped;
		public bool minerAccessoryEquipped;
		public Queue<Point16> miningQueue = new Queue<Point16>();
		public HashSet<Point16> queuedTiles = new HashSet<Point16>();
		public int targetOreType = -1;
		public int miningTimer = 0;
		public static bool IsChainMining = false;

		public override void ResetEffects()
		{
			veinMinerEquipped = false;
			minerAccessoryEquipped = false;
		}

		public override void PostUpdate()
		{
			if (!veinMinerEquipped && !minerAccessoryEquipped)
			{
				miningQueue.Clear();
				queuedTiles.Clear();
				targetOreType = -1;
				miningTimer = 0;
				return;
			}

			if (miningQueue.Count == 0)
			{
				targetOreType = -1;
				miningTimer = 0;
				return;
			}

			
			int delayTicks = minerAccessoryEquipped ? 1 : 12;

			miningTimer++;
			if (miningTimer >= delayTicks)
			{
				miningTimer = 0;
				ProcessNextTile();
			}
		}

		private void ProcessNextTile()
		{
			while (miningQueue.Count > 0)
			{
				Point16 p = miningQueue.Dequeue();
				queuedTiles.Remove(p);

				if (p.X < 10 || p.X >= Main.maxTilesX - 10 || p.Y < 10 || p.Y >= Main.maxTilesY - 10)
					continue;

				Tile tile = Main.tile[p.X, p.Y];
				if (tile != null && tile.HasTile && tile.TileType == targetOreType)
				{
					IsChainMining = true;
					try
					{
						WorldGen.KillTile(p.X, p.Y, fail: false, effectOnly: false, noItem: false);
						if (Main.netMode == NetmodeID.MultiplayerClient)
						{
							NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 20, p.X, p.Y);
						}
					}
					finally
					{
						IsChainMining = false;
					}
					break; 
				}
			}
		}

		public void StartVeinMining(int startI, int startJ, int oreType)
		{
			if (miningQueue.Count >= 1000) return;

			targetOreType = oreType;

			HashSet<Point16> visited = new HashSet<Point16>();
			Queue<Point16> bfsQueue = new Queue<Point16>();

			Point16 startPoint = new Point16(startI, startJ);
			visited.Add(startPoint);
			bfsQueue.Enqueue(startPoint);

			int maxBlocksInVein = 500;
			int addedCount = 0;

			int[] dx = { 1, -1, 0, 0, 1, 1, -1, -1 };
			int[] dy = { 0, 0, 1, -1, 1, -1, 1, -1 };

			while (bfsQueue.Count > 0 && addedCount < maxBlocksInVein && miningQueue.Count < 1000)
			{
				Point16 current = bfsQueue.Dequeue();

				if ((current.X != startI || current.Y != startJ) && !queuedTiles.Contains(current))
				{
					miningQueue.Enqueue(current);
					queuedTiles.Add(current);
					addedCount++;
				}

				for (int d = 0; d < 8; d++)
				{
					int nx = current.X + dx[d];
					int ny = current.Y + dy[d];

					if (nx < 10 || nx >= Main.maxTilesX - 10 || ny < 10 || ny >= Main.maxTilesY - 10)
						continue;

					Point16 nextPoint = new Point16(nx, ny);
					if (visited.Contains(nextPoint)) continue;

					Tile neighbor = Main.tile[nx, ny];
					if (neighbor != null && neighbor.HasTile && neighbor.TileType == oreType)
					{
						visited.Add(nextPoint);
						bfsQueue.Enqueue(nextPoint);
					}
				}
			}
		}
	}

	public class VeinMinerGlobalTile : GlobalTile
	{
		public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
		{
			if (fail || effectOnly || VeinMinerPlayer.IsChainMining) return;

			if (!IsOre(type)) return;

			Player player = Main.LocalPlayer;
			if (player == null || !player.active || player.dead) return;

			var veinPlayer = player.GetModPlayer<VeinMinerPlayer>();
			if (veinPlayer != null && (veinPlayer.veinMinerEquipped || veinPlayer.minerAccessoryEquipped))
			{
				veinPlayer.StartVeinMining(i, j, type);
			}
		}

		public static bool IsOre(int type)
		{
			if (type <= 0) return false;
			if (type < TileID.Sets.Ore.Length && TileID.Sets.Ore[type]) return true;

			switch (type)
			{
				case TileID.Copper:
				case TileID.Tin:
				case TileID.Iron:
				case TileID.Lead:
				case TileID.Silver:
				case TileID.Tungsten:
				case TileID.Gold:
				case TileID.Platinum:
				case TileID.Demonite:
				case TileID.Crimtane:
				case TileID.Meteorite:
				case TileID.Obsidian:
				case TileID.Hellstone:
				case TileID.Cobalt:
				case TileID.Palladium:
				case TileID.Mythril:
				case TileID.Orichalcum:
				case TileID.Adamantite:
				case TileID.Titanium:
				case TileID.Chlorophyte:
				case TileID.LunarOre:
				case TileID.Amethyst:
				case TileID.Topaz:
				case TileID.Sapphire:
				case TileID.Emerald:
				case TileID.Ruby:
				case TileID.Diamond:
				case TileID.ExposedGems:
					return true;
				default:
					return false;
			}
		}
	}
}
