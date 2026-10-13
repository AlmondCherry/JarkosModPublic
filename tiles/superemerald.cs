using Terraria.ObjectData;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using jarkos.buffs.buff;
using jarkos.items.usetiles;
namespace jarkos.tiles
{
	public class superemerald : ModTile
	{
		public override void SetStaticDefaults() {
			Main.tileLighted[(int) this.Type] = true;
			Main.tileTable[Type] = true;
			Main.tileSolidTop[Type] = true;
			Main.tileNoAttach[Type] = true;
			Main.tileLavaDeath[Type] = true;
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
			AddMapEntry(new Color(200, 200, 200), Language.GetText("MapObject.The Gloomy Altar"));
		}
		public override void NearbyEffects(int i, int j, bool closer)
    	{
      	Player localPlayer = Main.LocalPlayer;
      	if (localPlayer == null || localPlayer.dead || !((Entity) localPlayer).active)
        	return;
      	localPlayer.AddBuff(ModContent.BuffType<Superemerald_buff>(), 20, true);
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
	}
}
