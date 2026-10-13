using jarkos.tiles.CradleTiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using System.Collections.Generic;
using jarkos.particles;
using System.Reflection;
namespace jarkos.cradle_biome.system
{
	public class PetrifiedAbyssOverlaySystem : ModSystem
	{
		public static List<StarParticle> AmbientStars = new List<StarParticle>();
		public override void PostUpdatePlayers()
		{
			if (Main.dedServ) return;
			if (Main.LocalPlayer != null && Main.LocalPlayer.active && Main.LocalPlayer.InModBiome<PetrifiedAbyssBiome>())
			{
				if (Main.rand.NextBool(10))
				{
					Vector2 spawnPos = Main.LocalPlayer.Center + new Vector2(Main.rand.Next(-1600, 1600), Main.rand.Next(-1000, 1000));
					float scale = Main.rand.NextFloat(0.05f, 0.45f);
					float speedX = 0.5f + (1.5f - scale) * 2f;
					Vector2 velocity = new Vector2(speedX, 0f);
					Color[] colors = new Color[] { Color.Violet, Color.Purple, Color.Pink, Color.MediumOrchid, Color.Fuchsia };
					Color color = colors[Main.rand.Next(colors.Length)];
					int lifetime = Main.rand.Next(300, 600);
					float opacity = Main.rand.NextFloat(0.4f, 0.8f);
					StarParticle star = new StarParticle(spawnPos, velocity, color, lifetime, scale, opacity, rotationSpeed: 0f, ambient: true);
					star.GetType().GetProperty("Texture", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
						?.SetValue(star, ModContent.Request<Texture2D>("jarkos/particles/StarParticle").Value);
					AmbientStars.Add(star);
				}
			}
			for (int i = AmbientStars.Count - 1; i >= 0; i--)
			{
				AmbientStars[i].Time++;
				AmbientStars[i].Update();
				AmbientStars[i].Position += AmbientStars[i].Velocity;
				if (AmbientStars[i].Time >= AmbientStars[i].Lifetime)
				{
					AmbientStars.RemoveAt(i);
				}
			}
		}
		public override void PostDrawInterface(SpriteBatch spriteBatch) {
			if (!Main.dedServ && AmbientStars.Count > 0)
			{
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
				foreach (var star in AmbientStars)
				{
					star.Draw();
				}
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
			}
			if (Main.gameMenu || Main.LocalPlayer is null || !Main.LocalPlayer.InModBiome<PetrifiedAbyssBiome>()) {
				return;
			}
			if (CorruptionNeutralizerSystem.IsActiveWithin(Main.LocalPlayer.Center, CorruptionNeutralizerTileEntity.RadiusTiles)) {
				return;
			}
			Rectangle screen = new(0, 0, Main.screenWidth, Main.screenHeight);
			spriteBatch.Draw(TextureAssets.MagicPixel.Value, screen, new Color(95, 86, 82) * 0.08f);
		}
	}
}
