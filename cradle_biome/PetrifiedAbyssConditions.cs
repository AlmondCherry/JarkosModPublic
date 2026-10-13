using jarkos.cradle_biome.system;
using jarkos.tiles.CradleTiles;
using Terraria;

namespace jarkos.cradle_biome
{
	public static class PetrifiedAbyssConditions
	{
		public static Condition InPetrifiedAbyss = new("Mods.PetrifiedAbyss.Conditions.InPetrifiedAbyss", () => Main.LocalPlayer.InModBiome<PetrifiedAbyssBiome>());
		public static Condition PetrifiedAbyssGenerated = new("Mods.PetrifiedAbyss.Conditions.PetrifiedAbyssGenerated", () => PetrifiedAbyssSystem.generated);
		public static Condition NeutralizerActive = new("Mods.PetrifiedAbyss.Conditions.NeutralizerActive", () => Main.LocalPlayer is not null 
		&& CorruptionNeutralizerSystem.IsActiveWithin(Main.LocalPlayer.Center, CorruptionNeutralizerTileEntity.RadiusTiles));
	}
}
