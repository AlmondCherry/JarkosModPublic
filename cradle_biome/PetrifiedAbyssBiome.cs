using jarkos.cradle_biome.system;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Graphics.Capture;
using Terraria.ModLoader;

namespace jarkos.cradle_biome
{
	public class PetrifiedAbyssBiome : ModBiome
	{
		public override int Music => MusicLoader.GetMusicSlot(Mod, "Music/TheCradle");
		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;
		public override CaptureBiome.TileColorStyle TileColorStyle => CaptureBiome.TileColorStyle.Crimson;
		public override string BestiaryIcon => "jarkos/cradle_biome/PetrifiedAbyssBiome_Icon";
		public override string BackgroundPath => "jarkos/cradle_biome/PetrifiedAbyssBiome_Background";
		public override string MapBackground => "jarkos/cradle_biome/PetrifiedAbyssBiome_Background";
		public override bool IsBiomeActive(Player player)
		{
			Point tilePosition = player.Center.ToTileCoordinates();
			bool nearBuriedCenter = Math.Abs(tilePosition.X - Main.maxTilesX / 2) < Main.maxTilesX / 5 && tilePosition.Y > Main.rockLayer;

			return nearBuriedCenter && ModContent.GetInstance<PetrifiedAbyssTileCount>().petrifiedDepthstoneCount >= 75;
		}
		public override void OnEnter(Player player)
		{
		}
		public override void OnInBiome(Player player)
		{
			player.ZoneRockLayerHeight = true;
		}
	}
}
