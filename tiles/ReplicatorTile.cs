using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using jarkos.UI;

namespace jarkos.tiles
{
	public class ReplicatorTile : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = false;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
			TileObjectData.newTile.Width = 4;
			TileObjectData.newTile.Height = 3;
			TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16 };
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.Origin = new Point16(1, 2);
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<ReplicatorTileEntity>().Hook_AfterPlacement, -1, 0, false);
			TileObjectData.addTile(Type);
			AddMapEntry(new Color(150, 90, 170), CreateMapEntryName());
		}

		public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			int left = i - frameX / 18;
			int top = j - frameY / 18;
			ModContent.GetInstance<ReplicatorTileEntity>().Kill(left, top);
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
				if (te is ReplicatorTileEntity rte)
				{
					ReplicatorUISystem.Instance.OpenUI(rte);
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
				int id = ModContent.GetInstance<ReplicatorTileEntity>().Place(left, top);
				if (id != -1)
				{
					if (TileEntity.ByPosition.TryGetValue(new Point16(left, top), out TileEntity te) && te is ReplicatorTileEntity rte)
					{
						ReplicatorUISystem.Instance.OpenUI(rte);
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
			player.cursorItemIconID = ModContent.ItemType<items.usetiles.ReplicatorItem>();
		}
	}
}
