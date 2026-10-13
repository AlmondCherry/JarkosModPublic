using Terraria.ObjectData;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
namespace jarkos.tiles
{
	public class Decompositor : ModTile
	{
		public override void SetStaticDefaults() {
			Main.tileLighted[(int) this.Type] = true;
			Main.tileTable[Type] = false;
			Main.tileSolidTop[Type] = false;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = false;
			Main.tileFrameImportant[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;
			TileID.Sets.IgnoredByNpcStepUp[Type] = true;
			DustType = 63;
			AdjTiles = new int[] { TileID.Tables };
			TileObjectData.newTile.CopyFrom(TileObjectData.Style5x4);
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.CoordinateHeights = new int[4]      		{
        		16, 16, 16, 16
      		};
			TileObjectData.addTile(Type);
			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);
			AddMapEntry(new Color(200, 200, 200), Language.GetText("Decompositor"));
		}
        public override void NumDust(int x, int y, bool fail, ref int num) {
			num = fail ? 1 : 3;
		}
		 public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    	{
      		r = (float) Main.DiscoR / (float) byte.MaxValue;
      		g = (float) Main.DiscoG / (float) byte.MaxValue;
      		b = (float) Main.DiscoB / (float) byte.MaxValue;
    	}
		public override bool RightClick(int i, int j)
		{
			var tile = Main.tile[i, j];
			Player player = Main.LocalPlayer;
			 return false;
		}
	}
}
