using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Microsoft.Xna.Framework;
namespace jarkos.tiles.CradleTiles
{
    public class PetrifiedAbyssPot : ModTile
    {
        public override string Texture => "jarkos/items/usetiles/PetrifiedAbyssWall";
        public override void SetStaticDefaults() {
            Main.tileFrameImportant[Type] = true;
            Main.tileCut[Type] = true;
            Main.tileSolid[Type] = false;
            Main.tileMergeDirt[Type] = false;
            Main.tileWaterDeath[Type] = false;
            Main.tileLavaDeath[Type] = false;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.addTile(Type);
            DustType = DustID.Stone;
            HitSound = SoundID.Shatter;
        }
        public override System.Collections.Generic.IEnumerable<Item> GetItemDrops(int i, int j) {
            int dropType = ItemID.CopperCoin;
            int amount = 1;
            int rand = Main.rand.Next(100);
            if (rand < 20) {
                dropType = ItemID.SilverCoin;
                amount = Main.rand.Next(2, 6);
            } else if (rand < 40) {
                dropType = ItemID.Heart;
                amount = 1;
            } else if (rand < 60) {
                dropType = ItemID.Torch;
                amount = Main.rand.Next(3, 8);
            } else if (rand < 70) {
                dropType = ItemID.LesserHealingPotion;
                amount = 1;
            } else if (rand < 80) {
                dropType = ItemID.GoldCoin;
                amount = 1;
            } else {
                dropType = ItemID.CopperCoin;
                amount = Main.rand.Next(10, 30);
            }
            yield return new Item(dropType, amount);
        }
    }
}
