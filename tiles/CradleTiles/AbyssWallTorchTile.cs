using System;
using jarkos.items.usetiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
namespace jarkos.tiles.CradleTiles
{
	public class AbyssWallTorchTile : ModTile
	{
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileSolid[Type] = false;
			Main.tileSolidTop[Type] = false;
			Main.tileNoAttach[Type] = true;
			Main.tileLighted[Type] = true;
			Main.tileLavaDeath[Type] = true;
			Main.tileWaterDeath[Type] = false;
			TileID.Sets.DisableSmartCursor[Type] = true;
			TileID.Sets.FramesOnKillWall[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
			TileObjectData.newTile.AnchorWall = true;
			TileObjectData.newTile.AnchorTop = AnchorData.Empty;
			TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
			TileObjectData.newTile.AnchorLeft = AnchorData.Empty;
			TileObjectData.newTile.AnchorRight = AnchorData.Empty;
			TileObjectData.newTile.UsesCustomCanPlace = true;
			TileObjectData.newTile.LavaDeath = true;
			TileObjectData.addTile(Type);
			RegisterItemDrop(ModContent.ItemType<AbyssWallTorchItem>());
			AddMapEntry(new Color(200, 140, 180), CreateMapEntryName());
			DustType = DustID.Stone;
			AdjTiles = new int[] { TileID.Torches };
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			float flicker = 0.85f + 0.15f * (float)Math.Sin((Main.GameUpdateCount + (i * 13 + j * 7)) * 0.18f);
			r = 0.55f * flicker;
			g = 0.20f * flicker;
			b = 0.40f * flicker;
		}
		public override void AnimateTile(ref int frame, ref int frameCounter) {
			frameCounter++;
			if (frameCounter >= 6) {
				frameCounter = 0;
				frame = (frame + 1) % 3;
			}
		}
		public override void NumDust(int i, int j, bool fail, ref int num) {
			num = fail ? 1 : 3;
		}
	}
}
