using jarkos.items.usetiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria.GameInput;
namespace Jarkos.tiles
{
	public class Crimson_eden_bigger_flower : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileLighted[(int)Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = true;
			Main.tileFrameImportant[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;
			TileID.Sets.IgnoredByNpcStepUp[Type] = true;
			DustType = DustID.WhiteTorch;
			AdjTiles = new int[] { TileID.Tables };
			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.CoordinateHeights = new int[4]			  {
				16, 16, 16, 16
			  };
			TileObjectData.addTile(Type);
			AddMapEntry(new Color(200, 200, 200), Language.GetText("Crimson big flower"));
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    	{
      		r = 1;
      		g = 1;
      		b = 1;
    	}
	}
}
