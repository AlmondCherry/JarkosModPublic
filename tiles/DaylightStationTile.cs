using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using jarkos.UI;
using System;

namespace jarkos.tiles
{
	public class DaylightStationTile : ModTile
	{
		public override string Texture => "jarkos/tiles/DaylightStationTile";

		public override void SetStaticDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = false;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
			TileObjectData.newTile.Width = 6;
			TileObjectData.newTile.Height = 4;
			TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16, 16 };
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.Origin = new Point16(0, 3);
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<DaylightStationTileEntity>().Hook_AfterPlacement, -1, 0, false);
			TileObjectData.addTile(Type);
			AddMapEntry(new Color(255, 180, 0), CreateMapEntryName());
		}

		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;

		public override bool CanPlace(int i, int j)
		{
			if (Main.netMode == NetmodeID.Server) return true;
			int count = TileEntity.ByID.Values.OfType<DaylightStationTileEntity>().Count();
			if (count >= 1)
			{
				Main.NewText("Only one station!", Color.Red);
				return false;
			}
			return true;
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			int left = i - frameX / 18;
			int top = j - frameY / 18;
			ModContent.GetInstance<DaylightStationTileEntity>().Kill(left, top);
		}

		public override bool RightClick(int i, int j)
		{
			Tile tile = Main.tile[i, j];
			int frameX = tile.TileFrameX / 18;
			int frameY = tile.TileFrameY / 18;
			int left = i - frameX;
			int top = j - frameY;
			Point16 tePosition = new Point16(left, top);

			if (TileEntity.ByPosition.TryGetValue(tePosition, out TileEntity te))
			{
				if (te is DaylightStationTileEntity gte)
				{
					DaylightStationUISystem.Instance.OpenUI(gte);
				}
			}
			else
			{
				TryPlaceMissingTE(left, top);
			}
			return true;
		}

		private bool TryPlaceMissingTE(int left, int top)
		{
			Tile tile = Main.tile[left, top];
			if (tile.HasTile && tile.TileType == Type)
			{
				int id = ModContent.GetInstance<DaylightStationTileEntity>().Place(left, top);
				if (id != -1)
				{
					if (TileEntity.ByPosition.TryGetValue(new Point16(left, top), out TileEntity te) && te is DaylightStationTileEntity gte)
					{
						DaylightStationUISystem.Instance.OpenUI(gte);
						return true;
					}
				}
			}
			return false;
		}

		public override void MouseOver(int i, int j)
		{
			Player player = Main.LocalPlayer;
			player.noThrow = 2;
			
			
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
		{
			Tile tile = Main.tile[i, j];
			if (tile.TileFrameX == 0 && tile.TileFrameY == 0)
			{
				if (TileEntity.ByPosition.TryGetValue(new Point16(i, j), out TileEntity te) && te is DaylightStationTileEntity gte)
				{
					if (gte.GetTotalEssences() >= DaylightStationTileEntity.MaxCapacity)
					{
						if (ModContent.HasAsset("jarkos/UI/WarningIcon"))
						{
							var warningTex = ModContent.Request<Texture2D>("jarkos/UI/WarningIcon").Value;
							float offset = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3f) * 4f;
							Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange, Main.offScreenRange);
							Vector2 pos = new Vector2(i * 16, j * 16) - Main.screenPosition + zero;
							Vector2 drawPos = pos + new Vector2(48, -30 + offset);
							spriteBatch.Draw(warningTex, drawPos, null, Color.White, 0f, warningTex.Size() / 2f, 1f, SpriteEffects.None, 0f);
						}
					}
				}
			}
		}
	}
}
