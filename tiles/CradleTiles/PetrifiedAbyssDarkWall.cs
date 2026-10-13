using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.tiles.CradleTiles
{
    public class PetrifiedAbyssDarkWall : ModWall
    {
        public override string Texture => "jarkos/items/usetiles/PetrifiedAbyssWall";
        public override void SetStaticDefaults()
        {
            Main.wallHouse[Type] = false;
            DustType = DustID.Stone;
            AddMapEntry(new Color(30, 20, 40));
        }
        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }
    }
}
