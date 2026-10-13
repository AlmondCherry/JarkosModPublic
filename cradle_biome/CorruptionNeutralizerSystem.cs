using System.Collections.Generic;
using jarkos.tiles.CradleTiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace jarkos.cradle_biome
{
	public class CorruptionNeutralizerSystem : ModSystem
	{
		private static readonly List<Point16> activePositions = new();
		public override void PostUpdateEverything()
		{
			activePositions.Clear();
			foreach (var pair in TileEntity.ByPosition)
			{
				if (pair.Value is CorruptionNeutralizerTileEntity entity && entity.IsActive)
				{
					Tile tile = Framing.GetTileSafely(entity.Position.X, entity.Position.Y);
					if (!tile.HasTile || tile.TileType != ModContent.TileType<CorruptionNeutralizerTile>())
					{
						continue;
					}
					activePositions.Add(entity.Position);
				}
			}
		}
		public static bool IsActiveWithin(Vector2 worldPos, float radiusTiles)
		{
			if (activePositions.Count == 0)
			{
				return false;
			}

			float radiusPx = radiusTiles * 16f;
			float radiusSq = radiusPx * radiusPx;

			foreach (Point16 pos in activePositions) 
			{
				Vector2 centre = new(pos.X * 16 + 16, pos.Y * 16 + 24);

				float dx = worldPos.X - centre.X;
				float dy = worldPos.Y - centre.Y;

				if (dx * dx + dy * dy <= radiusSq)
				{
					return true;
				}
			}
			return false;
		}
		public static bool AnyActive()
		{
			return activePositions.Count > 0;
		}
	}
}
