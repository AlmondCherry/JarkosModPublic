using jarkos.UI.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace jarkos.npc.Punisher.UI
{
	public class PunisherChoiceUI : UIState
	{
		public static bool Visible = false;

		private float killScale = 1.0f;
		private float killAlpha = 0.55f;
		private bool killHovered = false;

		private float spareScale = 1.0f;
		private float spareAlpha = 0.55f;
		private bool spareHovered = false;

		private bool isDismissing = false;
		private float dismissTimer = 0f;
		private const float DismissDuration = 34f;

		private bool prevVisible = false;
		private bool wasLeftMouseDown = false;

		private struct ButtonParticle
		{
			public Vector2 Position;
			public Vector2 Velocity;
			public float Size;
			public float Alpha;
			public float MaxLife;
			public float Life;
			public Color Color;
		}

		private readonly List<ButtonParticle> killParticles = new List<ButtonParticle>();
		private readonly List<ButtonParticle> spareParticles = new List<ButtonParticle>();

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			if (!Visible)
			{
				prevVisible = false;
				killParticles.Clear();
				spareParticles.Clear();
				return;
			}

			if (!prevVisible)
			{
				isDismissing = false;
				dismissTimer = 0f;
				killScale = 1.0f;
				spareScale = 1.0f;
				killAlpha = 0.55f;
				spareAlpha = 0.55f;
				killHovered = false;
				spareHovered = false;
				wasLeftMouseDown = true;
				killParticles.Clear();
				spareParticles.Clear();
			}
			prevVisible = true;

			Vector2 mousePos = new Vector2(Main.mouseX, Main.mouseY);
			Vector2 screenCenter = new Vector2(Main.screenWidth * 0.5f, Main.screenHeight * 0.62f);

			float baseOffset = 180f;
			float buttonWidth = 200f;
			float buttonHeight = 54f;

			float leftX = screenCenter.X - baseOffset;
			float rightX = screenCenter.X + baseOffset;

			if (isDismissing)
			{
				dismissTimer++;

				float t = MathHelper.Clamp(dismissTimer / DismissDuration, 0f, 1f);
				float easeOut = 1f - MathF.Pow(1f - t, 3f);
				leftX -= easeOut * 700f;
				rightX += easeOut * 700f;

				if (dismissTimer >= DismissDuration)
				{
					Visible = false;
					isDismissing = false;
					dismissTimer = 0f;
					return;
				}
			}
			else
			{
				Rectangle leftRect = new Rectangle((int)(leftX - buttonWidth * killScale / 2f), (int)(screenCenter.Y - buttonHeight * killScale / 2f), (int)(buttonWidth * killScale), (int)(buttonHeight * killScale));
				Rectangle rightRect = new Rectangle((int)(rightX - buttonWidth * spareScale / 2f), (int)(screenCenter.Y - buttonHeight * spareScale / 2f), (int)(buttonWidth * spareScale), (int)(buttonHeight * spareScale));

				bool nowKillHover = leftRect.Contains(mousePos.ToPoint());
				bool nowSpareHover = rightRect.Contains(mousePos.ToPoint());

				if (nowKillHover && !killHovered)
				{
					SoundEngine.PlaySound(SoundID.MenuTick);
				}
				if (nowSpareHover && !spareHovered)
				{
					SoundEngine.PlaySound(SoundID.MenuTick);
				}

				killHovered = nowKillHover;
				spareHovered = nowSpareHover;

				float targetKillScale = killHovered ? 1.14f : 1.00f;
				float targetKillAlpha = killHovered ? 1.00f : 0.55f;
				killScale = MathHelper.Lerp(killScale, targetKillScale, 0.20f);
				killAlpha = MathHelper.Lerp(killAlpha, targetKillAlpha, 0.20f);

				float targetSpareScale = spareHovered ? 1.14f : 1.00f;
				float targetSpareAlpha = spareHovered ? 1.00f : 0.55f;
				spareScale = MathHelper.Lerp(spareScale, targetSpareScale, 0.20f);
				spareAlpha = MathHelper.Lerp(spareAlpha, targetSpareAlpha, 0.20f);

				bool isLeftMouseDown = Main.mouseLeft || Mouse.GetState().LeftButton == ButtonState.Pressed;
				bool justClicked = isLeftMouseDown && !wasLeftMouseDown;
				wasLeftMouseDown = isLeftMouseDown;

				if (justClicked)
				{
					if (killHovered)
					{
						TriggerKillAction();
					}
					else if (spareHovered)
					{
						TriggerSpareAction();
					}
				}
			}

			UpdateParticles(killParticles, new Vector2(leftX, screenCenter.Y), buttonWidth * killScale, buttonHeight * killScale, killAlpha, true);
			UpdateParticles(spareParticles, new Vector2(rightX, screenCenter.Y), buttonWidth * spareScale, buttonHeight * spareScale, spareAlpha, false);
		}

		private void UpdateParticles(List<ButtonParticle> particles, Vector2 center, float width, float height, float currentAlpha, bool isKill)
		{
			if (currentAlpha > 0.05f && Main.rand.NextBool(2))
			{
				float spawnX = center.X + Main.rand.NextFloat(-width * 0.45f, width * 0.45f);
				float spawnY = center.Y + Main.rand.NextFloat(-height * 0.2f, height * 0.45f);

				Color particleColor = isKill ? (Main.rand.NextBool() ? new Color(70, 130, 255) : new Color(130, 180, 255))
					: (Main.rand.NextBool() ? new Color(255, 80, 220) : new Color(255, 150, 240));

				particles.Add(new ButtonParticle
				{
					Position = new Vector2(spawnX, spawnY),
					Velocity = new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), -Main.rand.NextFloat(0.8f, 2.2f)),
					Size = Main.rand.NextFloat(3f, 6f),
					Alpha = currentAlpha * Main.rand.NextFloat(0.6f, 0.95f),
					Life = 0f,
					MaxLife = Main.rand.NextFloat(22f, 45f),
					Color = particleColor
				});
			}

			for (int i = particles.Count - 1; i >= 0; i--)
			{
				ButtonParticle p = particles[i];
				p.Life++;
				p.Position += p.Velocity;
				p.Velocity.X *= 0.98f;

				if (p.Life >= p.MaxLife)
				{
					particles.RemoveAt(i);
				}
				else
				{
					particles[i] = p;
				}
			}
		}

		private void TriggerKillAction()
		{
			SoundEngine.PlaySound(SoundID.Roar, Main.LocalPlayer.Center);
			isDismissing = true;
			dismissTimer = 0f;

			int bossType = ModContent.NPCType<Punisher>();
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (npc.active && npc.type == bossType)
				{
					npc.ai[0] = 17f;
					npc.ai[1] = 0f;
					npc.ai[2] = 0f;
					npc.netUpdate = true;
				}
			}
		}

		private void TriggerSpareAction()
		{
			SoundEngine.PlaySound(SoundID.Item4, Main.LocalPlayer.Center);
			isDismissing = true;
			dismissTimer = 0f;

			int bossType = ModContent.NPCType<Punisher>();
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (npc.active && npc.type == bossType)
				{
					npc.ai[0] = 19f;
					npc.ai[1] = 0f;
					npc.ai[2] = 0f;
					npc.netUpdate = true;
				}
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (!Visible) return;

			Vector2 screenCenter = new Vector2(Main.screenWidth * 0.5f, Main.screenHeight * 0.62f);
			float baseOffset = 180f;
			float buttonWidth = 200f;
			float buttonHeight = 54f;

			float leftX = screenCenter.X - baseOffset;
			float rightX = screenCenter.X + baseOffset;

			float curKillAlpha = killAlpha;
			float curSpareAlpha = spareAlpha;

			if (isDismissing)
			{
				float t = MathHelper.Clamp(dismissTimer / DismissDuration, 0f, 1f);
				float easeOut = 1f - MathF.Pow(1f - t, 3f);

				leftX -= easeOut * 700f;
				rightX += easeOut * 700f;

				float fade = (1f - easeOut);
				curKillAlpha *= fade;
				curSpareAlpha *= fade;
			}

			Texture2D pixel = TextureAssets.MagicPixel.Value;

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

			DrawParticleList(spriteBatch, pixel, killParticles);
			DrawParticleList(spriteBatch, pixel, spareParticles);

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

			DrawSingleButton(spriteBatch, "Kill", new Vector2(leftX, screenCenter.Y), buttonWidth * killScale, buttonHeight * killScale, curKillAlpha, new Color(18, 18, 55), new Color(60, 80, 240), killHovered);

			DrawSingleButton(spriteBatch, "Spare", new Vector2(rightX, screenCenter.Y), buttonWidth * spareScale, buttonHeight * spareScale, curSpareAlpha, new Color(55, 18, 55), new Color(240, 60, 220), spareHovered);
		}

		private void DrawParticleList(SpriteBatch spriteBatch, Texture2D pixel, List<ButtonParticle> particles)
		{
			for (int i = 0; i < particles.Count; i++)
			{
				ButtonParticle p = particles[i];
				float lifeRatio = p.Life / p.MaxLife;
				float fadeAlpha = p.Alpha * (1f - lifeRatio);
				if (fadeAlpha <= 0.01f) continue;

				Color drawCol = p.Color * fadeAlpha;
				Rectangle drawRect = new Rectangle((int)(p.Position.X - p.Size / 2f), (int)(p.Position.Y - p.Size / 2f), (int)p.Size, (int)p.Size);

				spriteBatch.Draw(pixel, drawRect, drawCol);
			}
		}

		private void DrawSingleButton(SpriteBatch spriteBatch, string label, Vector2 center, float width, float height, float alpha, Color bgCol, Color borderCol, bool isHovered)
		{
			if (alpha <= 0.01f) return;

			Rectangle rect = new Rectangle((int)(center.X - width / 2f), (int)(center.Y - height / 2f), (int)width, (int)height);

			Color fill = bgCol * alpha;
			Color border = borderCol * alpha;

			if (isHovered)
			{
				fill = Color.Lerp(fill, Color.White * alpha, 0.15f);
				border = Color.Lerp(border, Color.White * alpha, 0.35f);
			}

			Panel.Rounded(spriteBatch, rect, 12, fill, border, 3);

			var font = FontAssets.MouseText.Value;
			Vector2 textSize = font.MeasureString(label);
			Vector2 textPos = center - textSize / 2f + new Vector2(0f, 2f);

			Color textColor = Color.White * alpha;
			Color shadowColor = Color.Black * alpha * 0.8f;

			Terraria.Utils.DrawBorderStringFourWay(spriteBatch, font, label, textPos.X, textPos.Y, textColor, shadowColor, Vector2.Zero, 1.3f);
		}
	}
}
