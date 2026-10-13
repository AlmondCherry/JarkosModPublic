using Terraria.ObjectData;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using jarkos.buffs.buff;
namespace jarkos.tiles
{
	public class Lazatep_statue : ModTile
	{
		public override void SetStaticDefaults() {
			Main.tileTable[Type] = true;
			Main.tileSolidTop[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = true;
			Main.tileFrameImportant[Type] = true;
			TileID.Sets.DisableSmartCursor[Type] = true;
			TileID.Sets.IgnoredByNpcStepUp[Type] = true;
			Main.tileLighted[(int) this.Type] = true;
			DustType = 63;
			AdjTiles = new int[] { TileID.Tables };
			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
			TileObjectData.newTile.StyleHorizontal = true;
			TileObjectData.newTile.CoordinateHeights = new[] { 16, 18 };
			TileObjectData.addTile(Type);
			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);
			AddMapEntry(new Color(200, 200, 200), Language.GetText("MapObject.The Gloomy Altar"));
		}
		public override void NearbyEffects(int i, int j, bool closer)
    	{
      	Player localPlayer = Main.LocalPlayer;
      	if (localPlayer == null || localPlayer.dead || !((Entity) localPlayer).active)
        	return;
      	localPlayer.AddBuff(ModContent.BuffType<Lazatep_buff>(), 20, true);
    	}
		public override void NumDust(int x, int y, bool fail, ref int num) {
			num = fail ? 1 : 3;
		}
	}
}
