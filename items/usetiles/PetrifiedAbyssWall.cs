using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.usetiles
{
	public class PetrifiedAbyssWall : ModWall
	{
		public override void SetStaticDefaults()
		{
			Main.wallHouse[Type] = false;
			DustType = DustID.Stone;
			AddMapEntry(new Color(66, 58, 61), CreateMapEntryName());
		}
		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			if ((i + j) % 9 == 0)
			{
				r = 0.05f;
				g = 0.01f;
				b = 0.08f;
			}
		}
	}
}
