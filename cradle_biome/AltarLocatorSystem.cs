using System;
using System.Collections.Generic;
using jarkos.tiles.CradleTiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;

namespace jarkos.cradle_biome
{
	public class AltarLocatorSystem : ModSystem
	{
		private static int locatorHeldFrame = -1000;
		private static int lastScanTick = -10000;
		private static int lastFullscanTick = -10000;
		private static Point cachedTarget = new(-1, -1);
		private const int RescanIntervalTicks = 30;
		private const int FullscanIntervalTicks = 600;
		private static Asset<Texture2D> arrowTexture;
		public override void Load() 
		{
			if (Main.dedServ) 
			{
				return;
			}
			arrowTexture = ModContent.Request<Texture2D>("Jarkos/UI/arrow_down", AssetRequestMode.ImmediateLoad);
		}
		public override void Unload() 
		{
			arrowTexture = null;
		}
		public static void MarkLocatorHeld() 
		{
			locatorHeldFrame = (int)Main.GameUpdateCount;
		}
		public override void PostUpdateEverything() 
		{
			if (Main.netMode == NetmodeID.Server || Main.LocalPlayer is null) 
			{
				return;
			}
			if (!IsLocatorActive()) {
				return;
			}
			if ((int)Main.GameUpdateCount - lastScanTick < RescanIntervalTicks) 
			{
				return;
			}
			cachedTarget = FindNearestAltar(Main.LocalPlayer);
			lastScanTick = (int)Main.GameUpdateCount;
		}
		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) 
		{
			int mouseLayerIdx = layers.FindIndex(l => l.Name == "Vanilla: Cursor");
			if (mouseLayerIdx < 0) 
			{
				mouseLayerIdx = layers.Count;
			}
			layers.Insert(mouseLayerIdx, new LegacyGameInterfaceLayer("PetrifiedAbyss: Altar Locator", () => 
			{
				DrawLocatorArrow();
				return true;
			}, InterfaceScaleType.UI));
		}
		private static void DrawLocatorArrow() 
		{
			if (Main.gameMenu || !IsLocatorActive() || Main.LocalPlayer is null) return;
			if (cachedTarget.X < 0 || arrowTexture?.Value is null) return;

			Player player = Main.LocalPlayer;
			Vector2 playerWorld = player.Center;
			Vector2 targetWorld = new(cachedTarget.X * 16f + 24f, cachedTarget.Y * 16f + 16f);
			Vector2 toTarget = targetWorld - playerWorld;

			float distance = toTarget.Length();
			if (distance < 1f) return;

			Vector2 direction = toTarget / distance;
			float angle = (float)Math.Atan2(direction.Y, direction.X) + MathHelper.PiOver2 * 3;
			float screenW = Main.screenWidth;
			float screenH = Main.screenHeight;

			float padX = 64f;
			float padY = 64f;
			
			Vector2 screenCentre = new(screenW * 0.5f, screenH * 0.5f);

			float halfW = screenW * 0.5f - padX;
			float halfH = screenH * 0.5f - padY;

			Vector2 arrowPos = ProjectToBoxBoundary(screenCentre, direction, halfW, halfH);
			Vector2 targetScreen = targetWorld - Main.screenPosition;

			bool altarVisible = targetScreen.X >= padX && targetScreen.X <= screenW - padX && targetScreen.Y >= padY && targetScreen.Y <= screenH - padY;
			if (altarVisible) 
			{
				float ringRadius = Math.Min(halfW, halfH) * 0.6f;
				arrowPos = screenCentre + direction * ringRadius;
			}

			SpriteBatch sb = Main.spriteBatch;
			Texture2D tex = arrowTexture.Value;
			Vector2 origin = new(tex.Width * 0.5f, tex.Height * 0.5f);

			float pulse = 0.85f + 0.15f * (float)Math.Sin(Main.GameUpdateCount * 0.18f);
			Color tint = Color.White * pulse;
			sb.Draw(tex, arrowPos, null, tint, angle, origin, 1f, SpriteEffects.None, 0f);
			int distanceTiles = (int)(distance / 16f);
			string label = distanceTiles + "t";

			DrawCenteredTextBeneath(sb, label, arrowPos, angle);
		}
		private static Vector2 ProjectToBoxBoundary(Vector2 centre, Vector2 direction, float halfW, float halfH) 
		{
			float ax = Math.Abs(direction.X);
			float ay = Math.Abs(direction.Y);
			float t;

			if (ax * halfH > ay * halfW) 
			{
				t = halfW / Math.Max(ax, 1e-6f);
			}
			else 
			{
				t = halfH / Math.Max(ay, 1e-6f);
			}
			return centre + direction * t;
		}
		private static void DrawCenteredTextBeneath(SpriteBatch sb, string text, Vector2 anchor, float angle)
		{
			Vector2 backDir = new((float)Math.Cos(angle + Math.PI), (float)Math.Sin(angle + Math.PI));
			Vector2 textPos = anchor + backDir * 32f;
			Vector2 size = FontAssets.MouseText.Value.MeasureString(text);

			textPos -= size * 0.5f;
			ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, text, textPos, new Color(255, 220, 180), 0f, Vector2.Zero, Vector2.One);
		}
		private static bool IsLocatorActive()
		{
			return (int)Main.GameUpdateCount - locatorHeldFrame <= 4;
		}
		private static Point FindNearestAltar(Player player) 
		{
			int altarTileType = ModContent.TileType<PetrifiedAltarTile>();
			int playerTileX = (int)(player.Center.X / 16f);
			int playerTileY = (int)(player.Center.Y / 16f);
			
			Point best = new(-1, -1);
			long bestSqDist = long.MaxValue;

			foreach (var entry in TileEntity.ByPosition) 
			{
				if (entry.Value is not PetrifiedAltarTileEntity entity) 
				{
					continue;
				}

				Tile tile = Framing.GetTileSafely(entity.Position.X, entity.Position.Y);
				if (!tile.HasTile || tile.TileType != altarTileType) 
				{
					continue;
				}

				long dx = entity.Position.X - playerTileX;
				long dy = entity.Position.Y - playerTileY;
				long sq = dx * dx + dy * dy;

				if (sq < bestSqDist) 
				{
					bestSqDist = sq;
					best = new Point(entity.Position.X, entity.Position.Y);
				}
			}
			if (best.X >= 0) 
			{
				return best;
			}
			if ((int)Main.GameUpdateCount - lastFullscanTick < FullscanIntervalTicks) 
			{
				return best;
			}

			lastFullscanTick = (int)Main.GameUpdateCount;
			int scanRadius = 1000;
			int sx0 = Math.Max(40, playerTileX - scanRadius);
			int sx1 = Math.Min(Main.maxTilesX - 40, playerTileX + scanRadius);
			int sy0 = Math.Max(40, playerTileY - scanRadius);
			int sy1 = Math.Min(Main.maxTilesY - 40, playerTileY + scanRadius);

			for (int x = sx0; x < sx1; x++) 
			{
				for (int y = sy0; y < sy1; y++) 
				{
					Tile t = Framing.GetTileSafely(x, y);
					if (!t.HasTile || t.TileType != altarTileType) 
					{
						continue;
					}
					if (t.TileFrameX != 0 || t.TileFrameY != 0) 
					{
						continue;
					}
					long dx = x - playerTileX;
					long dy = y - playerTileY;
					long sq = dx * dx + dy * dy;
					if (sq < bestSqDist) 
					{
						bestSqDist = sq;
						best = new Point(x, y);
					}
				}
			}
			return best;
		}
	}
}
