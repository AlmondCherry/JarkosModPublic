using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using ReLogic.Graphics;
using jarkos.items.usable;
using jarkos.systems;
using Terraria.Audio;

namespace jarkos.tiles
{
	public class PunisherStatueTile : ModTile
	{
		public override string Texture => "jarkos/tiles/PunisherStatue";

		public override void SetStaticDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = false;
			Main.tileLighted[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.Width = 2;
			TileObjectData.newTile.Height = 4;
			TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16, 18 };
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.Origin = new Point16(0, 3);
			TileObjectData.addTile(Type);

			AddMapEntry(new Color(130, 90, 160), CreateMapEntryName());

			MinPick = 10000;
			DustType = DustID.Stone;
		}

		public override bool CanKillTile(int i, int j, ref bool blockDamaged) => false;
		public override bool CanExplode(int i, int j) => false;

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			r = 0.35f;
			g = 0.15f;
			b = 0.45f;
		}

		public override void MouseOver(int i, int j)
		{
			Player player = Main.LocalPlayer;
			player.cursorItemIconEnabled = false;
		}

		public override bool RightClick(int i, int j)
		{
			Player player = Main.LocalPlayer;
			if (player == null || !player.active || Main.myPlayer != player.whoAmI) return false;

			int phase = GetStatuePhase();
			if (phase == 3)
			{
				int tantoType = ModContent.ItemType<RitualTanto>();
				player.QuickSpawnItem(player.GetSource_TileInteraction(i, j), tantoType);
				SoundEngine.PlaySound(SoundID.Item4 with { Volume = 0.8f, Pitch = -0.2f }, player.Center);
			}
			else
			{
				SoundEngine.PlaySound(SoundID.MenuTick with { Volume = 0.5f }, player.Center);
			}

			return true;
		}

		public static int GetStatuePhase()
		{
			if (DownedBossSystem.downedPunisher || DownedBossSystem.killedPunisher)
				return 4;
			if (NPC.downedPlantBoss)
				return 3;
			if (Main.hardMode)
				return 2;
			return 1;
		}

		public static string GetPhaseDialogue(int phase)
		{
			return phase switch
			{
				1 => "...",
				2 => "It seems you're moving further and further...",
				3 => "Take a risk.",
				4 => "...",
				_ => "..."
			};
		}
	}

	public class PunisherStatueGroundProtection : GlobalTile
	{
		public override bool CanKillTile(int i, int j, int type, ref bool blockDamaged)
		{
			int statueType = ModContent.TileType<PunisherStatueTile>();
			for (int dy = 1; dy <= 4; dy++)
			{
				Tile tileAbove = Framing.GetTileSafely(i, j - dy);
				if (tileAbove.HasTile && tileAbove.TileType == statueType)
				{
					return false;
				}
			}
			return true;
		}

		public override bool CanExplode(int i, int j, int type)
		{
			int statueType = ModContent.TileType<PunisherStatueTile>();
			for (int dy = 1; dy <= 4; dy++)
			{
				Tile tileAbove = Framing.GetTileSafely(i, j - dy);
				if (tileAbove.HasTile && tileAbove.TileType == statueType)
				{
					return false;
				}
			}
			return true;
		}
	}

	public class PunisherStatueDrawingSystem : ModSystem
	{
		public override void PostDrawTiles()
		{
			Player player = Main.LocalPlayer;
			if (player == null || !player.active) return;

			int tileType = ModContent.TileType<PunisherStatueTile>();
			Vector2 screenPos = Main.screenPosition;

			int startX = (int)(screenPos.X / 16f) - 5;
			int endX = (int)((screenPos.X + Main.screenWidth) / 16f) + 5;
			int startY = (int)(screenPos.Y / 16f) - 5;
			int endY = (int)((screenPos.Y + Main.screenHeight) / 16f) + 5;

			startX = Math.Max(0, startX);
			endX = Math.Min(Main.maxTilesX, endX);
			startY = Math.Max(0, startY);
			endY = Math.Min(Main.maxTilesY, endY);

			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

			for (int x = startX; x < endX; x++)
			{
				for (int y = startY; y < endY; y++)
				{
					Tile tile = Main.tile[x, y];
					if (tile.HasTile && tile.TileType == tileType && tile.TileFrameX == 0 && tile.TileFrameY == 0)
					{
						Vector2 tileWorldPos = new Vector2(x * 16, y * 16);
						float dist = Vector2.Distance(player.Center, tileWorldPos + new Vector2(16, 36));

						if (dist < 340f)
						{
							int phase = PunisherStatueTile.GetStatuePhase();
							string dialogue = PunisherStatueTile.GetPhaseDialogue(phase);

							Vector2 drawPos = tileWorldPos + new Vector2(16, -28) - Main.screenPosition;
							float textAlpha = MathHelper.Clamp((340f - dist) / 100f, 0f, 1f);

							DrawWavyText(Main.spriteBatch, dialogue, drawPos, textAlpha);
						}
					}
				}
			}

			Main.spriteBatch.End();
		}

		private static void DrawWavyText(SpriteBatch spriteBatch, string text, Vector2 centerPos, float alpha)
		{
			DynamicSpriteFont font = FontAssets.MouseText.Value;
			Vector2 textSize = font.MeasureString(text);
			Vector2 startPos = centerPos - new Vector2(textSize.X / 2f, textSize.Y / 2f);

			float time = Main.GlobalTimeWrappedHourly;
			float curX = startPos.X;

			for (int i = 0; i < text.Length; i++)
			{
				string letterStr = text[i].ToString();
				Vector2 letterSize = font.MeasureString(letterStr);

				
				float wave = MathF.Sin(time * 5f + i * 0.45f) * 4f;
				Vector2 letterPos = new Vector2(curX, startPos.Y + wave);

				Color shadowColor = Color.Black * alpha * 0.85f;
				Color letterColor = new Color(245, 210, 255) * alpha;

				
				Terraria.Utils.DrawBorderStringFourWay(spriteBatch, font, letterStr, letterPos.X, letterPos.Y, letterColor, shadowColor, Vector2.Zero, 1f);

				curX += letterSize.X;
			}
		}
	}
}
