using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.systems
{
	public class MainMenuJarkos : ModMenu
	{
		private struct AshParticle
		{
			public Vector2 Position;
			public Vector2 Velocity;
			public float Size;
			public float Life;
			public float MaxLife;
			public Color Color;
			public float Rotation;
			public float RotSpeed;
			public int Type;
		}
		private readonly List<AshParticle> _particles = new();
		private float _time;
		private float _smoothMouseX;
		private float _smoothMouseY;
		private Texture2D _circleTexture;
		private Texture2D _softGlowTexture;
		private bool _wasMouseDown;
		public override string DisplayName => "Jarkos";
		public override int Music => MusicLoader.GetMusicSlot(Mod, "Music/MeniMusic");
		private void EnsureTextures(GraphicsDevice device)
		{
			if (_circleTexture == null || _circleTexture.IsDisposed)
			{
				int size = 64;
				_circleTexture = new Texture2D(device, size, size);

				Color[] data = new Color[size * size];
				float center = size * 0.5f;

				for (int y = 0; y < size; y++)
				{
					for (int x = 0; x < size; x++)
					{
						float dx = x - center + 0.5f;
						float dy = y - center + 0.5f;
						float dist = (float)Math.Sqrt(dx * dx + dy * dy) / center;
						float alpha = MathHelper.Clamp(1f - dist, 0f, 1f);

						alpha = alpha * alpha;
						data[y * size + x] = new Color(255, 255, 255, (byte)(alpha * 255));
					}
				}
				_circleTexture.SetData(data);
			}
			if (_softGlowTexture == null || _softGlowTexture.IsDisposed)
			{
				int size = 128;
				_softGlowTexture = new Texture2D(device, size, size);
				Color[] data = new Color[size * size];
				float center = size * 0.5f;

				for (int y = 0; y < size; y++)
				{
					for (int x = 0; x < size; x++)
					{
						float dx = x - center + 0.5f;
						float dy = y - center + 0.5f;
						float dist = (float)Math.Sqrt(dx * dx + dy * dy) / center;
						float alpha = MathHelper.Clamp(1f - dist, 0f, 1f);

						alpha = alpha * alpha * alpha;
						data[y * size + x] = new Color(alpha, alpha, alpha, alpha);
					}
				}
				_softGlowTexture.SetData(data);
			}
		}
		public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor)
		{
			_time += 1f / 60f;
			EnsureTextures(Main.graphics.GraphicsDevice);

			Texture2D bg = ModContent.Request<Texture2D>("jarkos/textures/MenuBackground", AssetRequestMode.ImmediateLoad).Value;
			Texture2D px = TextureAssets.MagicPixel.Value;

			int sw = Main.screenWidth;
			int sh = Main.screenHeight;
			float lerpSpeed = 0.05f;

			_smoothMouseX = MathHelper.Lerp(_smoothMouseX, Main.mouseX, lerpSpeed);
			_smoothMouseY = MathHelper.Lerp(_smoothMouseY, Main.mouseY, lerpSpeed);

			float parallaxStr = 30f;
			float mx = (_smoothMouseX - sw * 0.5f) / (sw * 0.5f);
			float my = (_smoothMouseY - sh * 0.5f) / (sh * 0.5f);
			float scale = Math.Max((sw + parallaxStr * 2f) / bg.Width, (sh + parallaxStr * 2f) / bg.Height);

			Vector2 bgPos = new Vector2((sw - bg.Width * scale) * 0.5f - mx * parallaxStr, (sh - bg.Height * scale) * 0.5f - my * parallaxStr);
			spriteBatch.Draw(bg, bgPos, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

			UpdateParticles(sw, sh);
			DrawParticles(spriteBatch);

			int bands = 25;
			int vH = sh / 4;
			int vW = sw / 6;

			for (int i = 0; i < bands; i++)
			{
				float t = (float)i / bands;
				float a = (1f - t) * (1f - t);
				int bh = vH / bands + 1;

				spriteBatch.Draw(px, new Rectangle(0, i * bh, sw, bh), Color.Black * (a * 0.55f));
				spriteBatch.Draw(px, new Rectangle(0, sh - (i + 1) * bh, sw, bh), Color.Black * (a * 0.55f));

				int bw = vW / bands + 1;
				spriteBatch.Draw(px, new Rectangle(i * bw, 0, bw, sh), Color.Black * (a * 0.35f));
				spriteBatch.Draw(px, new Rectangle(sw - (i + 1) * bw, 0, bw, sh), Color.Black * (a * 0.35f));
			}
			float pulse = (float)(Math.Sin(_time * 1.2) * 0.5 + 0.5);
			spriteBatch.Draw(px, new Rectangle(0, 0, sw, sh), new Color(100, 0, 0) * (0.02f + pulse * 0.035f));

			DrawLogo(spriteBatch, sw, sh);
			DrawVersionAndSocials(spriteBatch, sw, sh);
			return false;
		}
		private void DrawParticles(SpriteBatch spriteBatch)
		{
			Vector2 circleOrigin = new Vector2(_circleTexture.Width * 0.5f, _circleTexture.Height * 0.5f);
			Vector2 glowOrigin = new Vector2(_softGlowTexture.Width * 0.5f, _softGlowTexture.Height * 0.5f);
			foreach (var p in _particles)
			{
				float lr = p.Life / p.MaxLife;
				float fade = lr > 0.85f ? (1f - lr) / 0.15f : (lr < 0.25f ? lr / 0.25f : 1f);
				fade = fade * fade;
				float glowScale = p.Size * 3f / _softGlowTexture.Width;

				spriteBatch.Draw(_softGlowTexture, p.Position, null, p.Color * (fade * 0.2f), 0f, glowOrigin, glowScale, SpriteEffects.None, 0f);
				float coreScale = p.Size / _circleTexture.Width;

				spriteBatch.Draw(_circleTexture, p.Position, null, p.Color * (fade * 0.9f), p.Rotation, circleOrigin, coreScale, SpriteEffects.None, 0f);
			}
		}
		private void DrawLogo(SpriteBatch spriteBatch, int sw, int sh)
		{
			Texture2D logo = ModContent.Request<Texture2D>("jarkos/textures/logo", AssetRequestMode.ImmediateLoad).Value;

			float logoScale = 0.8f;
			Vector2 logoSize = new Vector2(logo.Width, logo.Height) * logoScale;
			Vector2 logoPos = new Vector2((sw - logoSize.X) * 0.5f, sh * 0.08f);
			Vector2 logoCenter = logoPos + logoSize * 0.5f;

			float floatY = (float)Math.Sin(_time * 1.5) * 3f;
			logoPos.Y += floatY;
			logoCenter.Y += floatY;

			float dist = Vector2.Distance(new Vector2(Main.mouseX, Main.mouseY), logoCenter);
			float prox = MathHelper.Clamp(1f - dist / 250f, 0f, 1f);
			prox *= prox;
			float pulse = (float)(Math.Sin(_time * 2.0) * 0.5 + 0.5);
			Vector2 origin = new Vector2(logo.Width * 0.5f, logo.Height * 0.5f);

			float breathe = (float)(Math.Sin(_time * 1.8) * 0.5 + 0.5);
			float bdAlpha = 0.4f + breathe * 0.15f;
			Vector2 gc = new Vector2(_softGlowTexture.Width * 0.5f, _softGlowTexture.Height * 0.5f);
			float sX = (logoSize.X + 2000f) / _softGlowTexture.Width;
			float sY = (logoSize.Y + 200f) / _softGlowTexture.Height;

			spriteBatch.Draw(_softGlowTexture, logoCenter, null, Color.Black * bdAlpha, 0f, gc, new Vector2(sX, sY), SpriteEffects.None, 0f);
			spriteBatch.Draw(_softGlowTexture, logoCenter, null, Color.Black * bdAlpha, 0f, gc, new Vector2(sX * 0.8f, sY * 0.8f), SpriteEffects.None, 0f);
			spriteBatch.Draw(_softGlowTexture, logoCenter, null, Color.Black * (bdAlpha * 0.7f), 0f, gc, new Vector2(sX * 0.5f, sY * 0.5f), SpriteEffects.None, 0f);

			float glowStr = 0.15f + prox * 0.5f + pulse * 0.1f;

			for (int i = 0; i < 4; i++)
			{
				float angle = i * MathHelper.PiOver2;
				float offs = 3f + prox * 2f;
				Vector2 glowOff = new Vector2((float)Math.Cos(angle) * offs, (float)Math.Sin(angle) * offs);

				spriteBatch.Draw(logo, logoCenter + glowOff, null, new Color(200, 40, 30) * glowStr, 0f, origin, logoScale, SpriteEffects.None, 0f);
			}

			float softScale = logoScale * (1.3f + prox * 0.2f);

			spriteBatch.Draw(logo, logoCenter, null, new Color(180, 30, 20) * (glowStr * 0.3f), 0f, origin, softScale, SpriteEffects.None, 0f);
			float logoAlpha = 0.85f + prox * 0.15f;

			spriteBatch.Draw(logo, logoCenter, null, Color.White * logoAlpha, 0f, origin, logoScale, SpriteEffects.None, 0f);
		}
		private void DrawVersionAndSocials(SpriteBatch spriteBatch, int sw, int sh)
		{
			float glow = (float)(Math.Sin(_time * 2.0) * 0.5 + 0.5);
			string version = "jarkos 1.3";

			var font = FontAssets.MouseText.Value;
			float textScale = 1.4f;
			Vector2 textSize = font.MeasureString(version) * textScale;
			Vector2 versionPos = new Vector2(sw - textSize.X - 20, 18);
			Vector2 vCenter = versionPos + textSize * 0.5f;

			float vDist = Vector2.Distance(new Vector2(Main.mouseX, Main.mouseY), vCenter);
			float vProx = MathHelper.Clamp(1f - vDist / 200f, 0f, 1f);
			vProx *= vProx;
			float vAlpha = MathHelper.Lerp(0.15f, 0.9f, vProx) + glow * 0.08f;
			Color vGlowColor = new Color(180, 30, 20) * (vProx * 0.3f + glow * 0.1f);
			
			Terraria.Utils.DrawBorderString(spriteBatch, version, versionPos + new Vector2(-1, -1), vGlowColor, textScale);
			Terraria.Utils.DrawBorderString(spriteBatch, version, versionPos + new Vector2(1, 1), vGlowColor, textScale);
			Color textColor = Color.Lerp(new Color(200, 60, 50), new Color(255, 120, 80), glow);
			Terraria.Utils.DrawBorderString(spriteBatch, version, versionPos, textColor * vAlpha, textScale);

			float iconSize = 28f;
			float iconGap = 10f;
			float iconsBaseX = sw - iconSize - 14;
			float iconsBaseY = versionPos.Y + textSize.Y + 16f;
			bool clicked = Main.mouseLeft && !_wasMouseDown;
			_wasMouseDown = Main.mouseLeft;

			Vector2 discordBase = new Vector2(iconsBaseX, iconsBaseY);
			Vector2 youtubeBase = new Vector2(iconsBaseX, iconsBaseY + iconSize + iconGap);
			Texture2D dsIcon = ModContent.Request<Texture2D>("jarkos/textures/ds", AssetRequestMode.ImmediateLoad).Value;
			DrawSocialIconIndependent(spriteBatch, null, dsIcon, discordBase, iconSize, new Color(114, 137, 218), glow, clicked, "https://discord.gg/kaxucWRzvg");
			Texture2D ytIcon = ModContent.Request<Texture2D>("jarkos/textures/yt", AssetRequestMode.ImmediateLoad).Value;
			DrawSocialIconIndependent(spriteBatch, null, ytIcon, youtubeBase, iconSize, new Color(255, 50, 50), glow, clicked, "https://www.youtube.com/@JarkosOfficial");
		}
		private void DrawSocialIconIndependent(SpriteBatch spriteBatch, string label, Texture2D icon, Vector2 basePos, float size, Color brandColor, float glow, bool clicked, string url)
		{
			var font = FontAssets.MouseText.Value;

			Vector2 baseCenter = basePos + new Vector2(size * 0.5f);
			float dist = Vector2.Distance(new Vector2(Main.mouseX, Main.mouseY), baseCenter);
			float prox = MathHelper.Clamp(1f - dist / 150f, 0f, 1f);
			prox *= prox;
			float alpha = MathHelper.Lerp(0.15f, 1f, prox) + glow * 0.06f;
			Vector2 toMouse = new Vector2(Main.mouseX, Main.mouseY) - baseCenter;
			float maxFollow = 5f;

			if (toMouse.Length() > 0.1f)
				toMouse = Vector2.Normalize(toMouse) * Math.Min(toMouse.Length() * 0.03f, maxFollow) * prox;
			else
				toMouse = Vector2.Zero;

			Vector2 center = baseCenter + toMouse;
			bool hovered = dist < size * 0.9f;
			float hoverScale = hovered ? 1.2f : 1f;
			float hoverAlpha = hovered ? 1f : alpha;
			float gScale = size * 2f * hoverScale / _softGlowTexture.Width;
			Vector2 glowOrigin = new Vector2(_softGlowTexture.Width * 0.5f);

			spriteBatch.Draw(_softGlowTexture, center, null, brandColor * (hoverAlpha * 0.35f), 0f, glowOrigin, gScale, SpriteEffects.None, 0f);

			if (icon != null)
			{
				float iconScale = size * hoverScale / Math.Max(icon.Width, icon.Height);
				Vector2 iconOrigin = new Vector2(icon.Width * 0.5f, icon.Height * 0.5f);
				spriteBatch.Draw(icon, center, null, Color.White * hoverAlpha, 0f, iconOrigin, iconScale, SpriteEffects.None, 0f);
			}
			else
			{
				Vector2 circleOrigin = new Vector2(_circleTexture.Width * 0.5f);
				float cScale = size * hoverScale / _circleTexture.Width;
				Color circleColor = hovered ? brandColor : Color.Lerp(Color.DarkGray, brandColor, prox);

				spriteBatch.Draw(_circleTexture, center, null, circleColor * (hoverAlpha * 0.7f), 0f, circleOrigin, cScale, SpriteEffects.None, 0f);
				float labelScale = 0.6f * hoverScale;
				Vector2 labelSize = font.MeasureString(label) * labelScale;
				Vector2 labelPos = center - labelSize * 0.5f;

				Terraria.Utils.DrawBorderString(spriteBatch, label, labelPos, Color.White * hoverAlpha, labelScale);
			}
			if (hovered && clicked)
			{
				SoundEngine.PlaySound(SoundID.MenuOpen);
				try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); } 
				catch { }
			}
		}
		private void UpdateParticles(int sw, int sh)
		{
			int maxParticles = 120; 

			if (_particles.Count < maxParticles)
			{
				if (Main.rand.NextFloat() < 0.4f)
					_particles.Add(CreateParticle(sw, sh));
			}

			for (int i = _particles.Count - 1; i >= 0; i--)
			{
				var p = _particles[i];

				p.Position += p.Velocity;
				p.Life -= 1f / 60f;
				p.Rotation += p.RotSpeed;

				switch (p.Type)
				{
					case 0:
						p.Velocity.Y -= 0.003f;
						p.Velocity.X += (float)Math.Sin(_time * 1.5 + i * 1.3) * 0.008f;
						p.Velocity *= 0.998f;
						break;
					case 1:
						p.Velocity.Y -= 0.008f;
						p.Velocity.X += (float)Math.Sin(_time * 2.0 + i * 0.9) * 0.015f;
						p.Velocity *= 0.997f;
						break;
					case 2:
						p.Velocity.Y -= 0.02f;
						p.Velocity.X += (float)Math.Cos(_time * 3.0 + i * 2.1) * 0.01f;
						p.Velocity *= 0.99f;
						break;
				}

				_particles[i] = p;

				if (p.Life <= 0 || p.Position.Y < -80 || p.Position.Y > sh + 80 || p.Position.X < -80 || p.Position.X > sw + 80)
				{
					_particles.RemoveAt(i);
				}
			}
		}
		private AshParticle CreateParticle(int sw, int sh)
		{
			AshParticle p;

			float roll = Main.rand.NextFloat();

			if (roll < 0.45f)
				p.Type = 0;
			else if (roll < 0.8f)
				p.Type = 1;
			else
				p.Type = 2;

			switch (p.Type)
			{
				case 0:
					p.Position = new Vector2(Main.rand.NextFloat() * sw, Main.rand.NextFloat() * sh * 0.5f + sh * 0.4f);
					p.Velocity = new Vector2(Main.rand.NextFloat() * 0.4f - 0.2f, -(Main.rand.NextFloat() * 0.4f + 0.1f));
					p.Size = Main.rand.NextFloat() * 4f + 2f;
					p.MaxLife = Main.rand.NextFloat() * 5f + 4f;

					int grey = Main.rand.Next(40, 90);
					p.Color = new Color(grey + Main.rand.Next(20, 50), grey, grey);
					p.RotSpeed = Main.rand.NextFloat() * 0.04f - 0.02f;

					break;
				case 1:
					p.Position = new Vector2(Main.rand.NextFloat() * sw, Main.rand.NextFloat() * sh * 0.4f + sh * 0.55f);
					p.Velocity = new Vector2(Main.rand.NextFloat() * 0.6f - 0.3f, -(Main.rand.NextFloat() * 0.8f + 0.3f));
					p.Size = Main.rand.NextFloat() * 3f + 1.5f;
					p.MaxLife = Main.rand.NextFloat() * 3f + 2.5f;
					p.Color = new Color(Main.rand.Next(200, 255), Main.rand.Next(40, 100), 0);
					p.RotSpeed = Main.rand.NextFloat() * 0.02f - 0.01f;

					break;
				default:
					p.Position = new Vector2(Main.rand.NextFloat() * sw, Main.rand.NextFloat() * sh * 0.3f + sh * 0.6f);
					p.Velocity = new Vector2(Main.rand.NextFloat() * 1.0f - 0.5f, -(Main.rand.NextFloat() * 1.5f + 0.5f));
					p.Size = Main.rand.NextFloat() * 1.5f + 0.8f;
					p.MaxLife = Main.rand.NextFloat() * 1.5f + 0.8f;
					p.Color = new Color(255, Main.rand.Next(150, 230), Main.rand.Next(50, 120));
					p.RotSpeed = 0f;

					break;
			}
			p.Life = p.MaxLife;
			p.Rotation = Main.rand.NextFloat() * MathHelper.TwoPi;
			
			return p;
		}
	}
}
