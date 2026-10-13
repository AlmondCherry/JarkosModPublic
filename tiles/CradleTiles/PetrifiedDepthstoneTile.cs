using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.tiles.CradleTiles
{
	public class PetrifiedDepthstoneTile : ModTile
	{
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileMergeDirt[Type] = false;
			Main.tileStone[Type] = true;
			AddMapEntry(new Color(84, 40, 92), CreateMapEntryName());
			DustType = DustID.Stone;
			HitSound = SoundID.Tink;
			MineResist = 1.8f;
			MinPick = 55;
		}
		public override void NumDust(int i, int j, bool fail, ref int num) {
			num = fail ? 1 : 3;
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) {
			if ((i + j) % 7 == 0) {
				r = 0.08f;
				g = 0.01f;
				b = 0.12f;
			}
		}
	}
}
