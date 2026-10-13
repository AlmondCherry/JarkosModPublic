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
	public class DaylightGeneratorTile : ModTile
	{
		public override string Texture => "jarkos/tiles/DistillerTile";

		public override void SetStaticDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = false;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.Width = 2;
			TileObjectData.newTile.Height = 3;
			TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 18 };
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.Origin = new Point16(0, 2);
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<DaylightGeneratorTileEntity>().Hook_AfterPlacement, -1, 0, false);
			TileObjectData.addTile(Type);
			AddMapEntry(new Color(255, 215, 0), CreateMapEntryName());
		}

		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			int left = i - frameX / 18;
			int top = j - frameY / 18;
			ModContent.GetInstance<DaylightGeneratorTileEntity>().Kill(left, top);
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
				if (te is DaylightGeneratorTileEntity gte)
				{
					DaylightGeneratorUISystem.Instance.OpenUI(gte);
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
				int id = ModContent.GetInstance<DaylightGeneratorTileEntity>().Place(left, top);
				if (id != -1)
				{
					if (TileEntity.ByPosition.TryGetValue(new Point16(left, top), out TileEntity te) && te is DaylightGeneratorTileEntity gte)
					{
						DaylightGeneratorUISystem.Instance.OpenUI(gte);
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
			player.cursorItemIconEnabled = true;
			player.cursorItemIconID = ModContent.ItemType<items.usetiles.DaylightGeneratorItem>();
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
		{
			Tile tile = Main.tile[i, j];
			if (tile.TileFrameX == 0 && tile.TileFrameY == 0)
			{
				if (TileEntity.ByPosition.TryGetValue(new Point16(i, j), out TileEntity te) && te is DaylightGeneratorTileEntity gte)
				{
					if (gte.GetTotalEssences() >= DaylightGeneratorTileEntity.MaxCapacity)
					{
						if (ModContent.HasAsset("jarkos/UI/WarningIcon"))
						{
							var warningTex = ModContent.Request<Texture2D>("jarkos/UI/WarningIcon").Value;
							float offset = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3f) * 4f;
							Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange, Main.offScreenRange);
							Vector2 pos = new Vector2(i * 16, j * 16) - Main.screenPosition + zero;
							Vector2 drawPos = pos + new Vector2(16, -30 + offset);
							spriteBatch.Draw(warningTex, drawPos, null, Color.White, 0f, warningTex.Size() / 2f, 1f, SpriteEffects.None, 0f);
						}
					}
				}
			}
		}
	}
}
