using jarkos.tiles.CradleTiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
namespace jarkos.cradle_biome.system
{
	public class PetrifiedAbyssShaderSystem : ModSystem
	{
		public const string FilterKey = "Jarkos:Filter";
		public const string GlitchKey = "Jarkos:Glitch";
		private const string FilterAssetPath = "Jarkos/Shaders/PetrifiedAbyssFilter";
		private const string GlitchAssetPath = "Jarkos/Shaders/PetrifiedAbyssGlitch";
		private bool filterRegistered;
		private bool glitchRegistered;
		private static readonly Vector3 FilterPrimary = new(0.62f, 0.30f, 0.95f);
		private static readonly Vector3 FilterSecondary = new(0.38f, 0.16f, 0.66f);
		private static readonly Vector3 GlitchPrimary = new(0.82f, 0.45f, 1.00f);
		private static readonly Vector3 GlitchSecondary = new(0.22f, 0.06f, 0.42f);
		public override void Load() {
			if (Main.dedServ) {
				return;
			}
			filterRegistered = TryRegister(FilterKey, FilterAssetPath, EffectPriority.High);
			glitchRegistered = TryRegister(GlitchKey, GlitchAssetPath, EffectPriority.VeryHigh);
		}
		public override void Unload() {
			filterRegistered = false;
			glitchRegistered = false;
		}
		public override void PostUpdateEverything() {
			if (Main.dedServ || Main.gameMenu) {
				return;
			}
			Player player = Main.LocalPlayer;
			if (player is null || !player.active) {
				ForceDeactivate(FilterKey, filterRegistered);
				ForceDeactivate(GlitchKey, glitchRegistered);
				return;
			}
			bool inBiome = player.InModBiome<PetrifiedAbyssBiome>();
			bool neutralizerNearby = CorruptionNeutralizerSystem.IsActiveWithin(player.Center, CorruptionNeutralizerTileEntity.RadiusTiles);
			bool shouldBeActive = inBiome && !neutralizerNearby;
			Vector2 anchor = player.Center;
			UpdateFilter(FilterKey, filterRegistered, shouldBeActive, anchor, FilterPrimary, FilterSecondary, opacity: 0.55f, intensity: 0.85f);
			UpdateFilter(GlitchKey, glitchRegistered, shouldBeActive, anchor, GlitchPrimary, GlitchSecondary, opacity: 0.35f, intensity: 0.85f);
		}
		private bool TryRegister(string key, string assetPath, EffectPriority priority) {
			string stripped = StripModPrefix(assetPath);
			if (!Mod.HasAsset(stripped)) {
				return false;
			}
			Asset<Effect> effect = Mod.Assets.Request<Effect>(stripped, AssetRequestMode.ImmediateLoad);
			if (effect?.Value is null) {
				return false;
			}
			Filters.Scene[key] = new Filter(new ScreenShaderData(effect, "Pass1"), priority);
			Filters.Scene[key].Load();
			return true;
		}
		private static string StripModPrefix(string assetPath) {
			const string prefix = "Jarkos/";
			return assetPath.StartsWith(prefix) ? assetPath[prefix.Length..] : assetPath;
		}
		private static void UpdateFilter(string key, bool registered, bool shouldBeActive, Vector2 anchor, Vector3 primary, Vector3 secondary, float opacity, float intensity) {
			if (!registered) {
				return;
			}
			Filter filter = Filters.Scene[key];
			if (filter is null) {
				return;
			}
			if (shouldBeActive) {
				if (!filter.IsActive()) {
					Filters.Scene.Activate(key, anchor);
				}
				filter.GetShader()
					.UseColor(primary)
					.UseSecondaryColor(secondary)
					.UseOpacity(opacity)
					.UseIntensity(intensity)
					.UseProgress(1f);
			}
			else if (filter.IsActive()) {
				Filters.Scene[key].Deactivate();
			}
		}
		private static void ForceDeactivate(string key, bool registered) {
			if (!registered) {
				return;
			}
			Filter filter = Filters.Scene[key];
			if (filter is { } && filter.IsActive()) {
				filter.Deactivate();
			}
		}
	}
}
