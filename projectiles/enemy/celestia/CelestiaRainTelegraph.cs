using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using jarkos.systems;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.enemy.celestia
{
	public class CelestiaRainTelegraph : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Starfury;
		public override void SetDefaults()
		{
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.hostile = false;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 70;
			Projectile.alpha = 255;
		}
		public override void AI()
		{
			Projectile.velocity = Vector2.Zero;
			float age = 70f - Projectile.timeLeft;
			float laneWidth = Projectile.ai[0] <= 0f ? 140f : Projectile.ai[0];
			int arrows = Projectile.ai[1] <= 0f ? 4 : (int)Projectile.ai[1];
			float progress = age / 70f;
			if (!Main.dedServ && progress > 0.15f)
			{
				int dustCount = Main.rand.NextBool(3) ? 1 : 0;
				dustCount += progress > 0.7f ? 1 : 0;
				for (int i = 0; i < dustCount; i++)
				{
					float edgeBias = Main.rand.NextBool() ? -0.5f : 0.5f;
					float widthScale = MathHelper.SmoothStep(0f, 1f, progress);
					float x = laneWidth * edgeBias * widthScale + Main.rand.NextFloat(-6f, 6f);
					Vector2 position = new Vector2(Projectile.Center.X + x, Projectile.Center.Y + Main.rand.NextFloat(-560f, 420f));
					Color tint = Color.Lerp(new Color(255, 90, 130), new Color(220, 30, 50), progress);
					Dust dust = Dust.NewDustPerfect(position, DustID.LifeDrain, new Vector2(0f, 1.5f + progress * 4f), 0, tint, (0.6f + progress * 1.0f));
					dust.noGravity = true;
					dust.fadeIn = 0.25f;
				}
				if (progress > 0.3f && Main.rand.NextBool(4))
				{
					float sparkX = Main.rand.NextFloat(-laneWidth * 0.45f, laneWidth * 0.45f);
					Vector2 sparkPos = new Vector2(Projectile.Center.X + sparkX, Projectile.Center.Y + Main.rand.NextFloat(-300f, 380f));
					Color sparkTint = Color.Lerp(new Color(255, 100, 110), new Color(220, 20, 40), progress);
					Dust Spark = Dust.NewDustPerfect(sparkPos, DustID.RedTorch, new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), -1.5f - progress * 2.5f), 0, sparkTint, (0.55f + progress * 0.7f));
					Spark.noGravity = true;
					Spark.fadeIn = 0.4f;
				}
			}
			if (Projectile.timeLeft == 25 && Main.netMode != NetmodeID.MultiplayerClient)
			{
				for (int i = 0; i < arrows; i++)
				{
					float xOffset = arrows == 1 ? 0f : MathHelper.Lerp(-laneWidth * 0.42f, laneWidth * 0.42f, i / (float)(arrows - 1));
					Vector2 spawn = new Vector2(Projectile.Center.X + xOffset, Projectile.Center.Y - 660f - i * 28f);
					Vector2 velocity = new Vector2(Main.rand.NextFloat(-0.6f, 0.6f), 17.5f);
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawn, velocity, ModContent.ProjectileType<CelestiaMagicRainArrow>(), Projectile.damage, 0f, Main.myPlayer);
				}
				SoundEngine.PlaySound(SoundID.Item72 with { Volume = 0.75f, Pitch = 0.35f }, Projectile.Center);
			}
		}
		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D pixel = TextureAssets.MagicPixel.Value;
			float laneWidth = Projectile.ai[0] <= 0f ? 140f : Projectile.ai[0];
			float progress = 1f - Projectile.timeLeft / 70f;
			float widthScale = MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp(progress * 1.05f, 0f, 1f));
			float pulse = (float)Math.Sin(Main.GameUpdateCount * (0.18f + progress * 0.65f)) * 0.5f + 0.5f;
			Color pinkColor = new Color(220, 70, 100);
			Color redColor = new Color(255, 30, 45);
			Color baseColor = Color.Lerp(pinkColor, redColor, MathHelper.Clamp(progress * 1.15f + pulse * 0.15f, 0f, 1f));
			Rectangle source = new Rectangle(0, 0, 1, 1);
			Vector2 screenCenter = new Vector2(Projectile.Center.X - Main.screenPosition.X, Main.screenHeight / 2f);
			Vector2 scale = new Vector2(laneWidth * widthScale, Main.screenHeight + 240f);
			DrawShaderOverlay(pixel, screenCenter, source, scale, progress, baseColor);
			for (int i = 0; i < 2; i++)
			{
				float edgeX = Projectile.Center.X + (i == 0 ? -laneWidth * 0.5f * widthScale : laneWidth * 0.5f * widthScale) - Main.screenPosition.X;
				Color edgeColor = Color.Lerp(pinkColor, redColor, MathHelper.Clamp(progress * 1.2f, 0f, 1f));
				float edgeAlpha = (0.20f + progress * 0.50f + pulse * 0.10f * progress);
				Main.EntitySpriteDraw(pixel, new Vector2(edgeX, Main.screenHeight / 2f), source, edgeColor * edgeAlpha * 0.45f, 0f, new Vector2(0.5f, 0.5f), new Vector2(7f + progress * 6f, Main.screenHeight + 240f), SpriteEffects.None, 0);
				Main.EntitySpriteDraw(pixel, new Vector2(edgeX, Main.screenHeight / 2f), source, edgeColor * edgeAlpha, 0f, new Vector2(0.5f, 0.5f), new Vector2(2f + progress * 2.5f, Main.screenHeight + 240f), SpriteEffects.None, 0);
			}
			return false;
		}
		private void DrawShaderOverlay(Texture2D pixel, Vector2 screenCenter, Rectangle source, Vector2 scale, float progress, Color baseColor)
		{
			Effect effect = CelestiaShaderSystem.ArrowRainTelegraphShader?.Value;
			if (effect == null)
			{
				float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.45f) * 0.5f + 0.5f;
				Color fallbackColor = baseColor * (0.04f + progress * 0.16f + pulse * 0.04f * progress);
				Main.EntitySpriteDraw(pixel, screenCenter, source, fallbackColor, 0f, new Vector2(0.5f, 0.5f), scale, SpriteEffects.None, 0);
				return;
			}
			effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
			effect.Parameters["uProgress"]?.SetValue(progress);
			effect.Parameters["uOpacity"]?.SetValue(0.30f);
			effect.Parameters["uPixelMix"]?.SetValue(0.22f);
			effect.Parameters["uBaseColor"]?.SetValue(new Vector3(0.85f, 0.20f, 0.28f));
			effect.Parameters["uAccentColor"]?.SetValue(new Vector3(1f, 0.10f, 0.18f));
			effect.Parameters["uGlowColor"]?.SetValue(new Vector3(1f, 0.55f, 0.65f));
			effect.CurrentTechnique = effect.Techniques["Technique1"];
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, effect, Main.GameViewMatrix.TransformationMatrix);
			Main.spriteBatch.Draw(pixel, screenCenter, source, Color.White, 0f, new Vector2(0.5f, 0.5f), scale, SpriteEffects.None, 0f);
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
		}
		private void DrawLine(Texture2D pixel, Vector2 start, Vector2 end, Color color, float width)
		{
			Vector2 edge = end - start;
			Main.EntitySpriteDraw(pixel, start, new Rectangle(0, 0, 1, 1), color, edge.ToRotation(), new Vector2(0f, 0.5f), new Vector2(edge.Length(), width), SpriteEffects.None, 0);
		}
	}
}
