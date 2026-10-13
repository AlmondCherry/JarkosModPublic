using System;
using jarkos.tiles.CradleTiles;
using Terraria.ModLoader;

namespace jarkos.cradle_biome.system
{
	public class PetrifiedAbyssTileCount : ModSystem
	{
		public int petrifiedDepthstoneCount;
		public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
		{
			petrifiedDepthstoneCount = tileCounts[ModContent.TileType<PetrifiedDepthstoneTile>()];
		}
	}
}
