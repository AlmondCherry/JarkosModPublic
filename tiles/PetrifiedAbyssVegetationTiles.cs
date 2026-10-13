using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
namespace jarkos.tiles
{
    public class AbyssWeedTile : ModTile
    {
        public override string Texture => "jarkos/tiles/AbyssWeedTile";
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileCut[Type] = true;
            Main.tileSolid[Type] = false;
            Main.tileMergeDirt[Type] = false;
            Main.tileWaterDeath[Type] = true;
            Main.tileLavaDeath[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.AnchorValidTiles = new int[] { ModContent.TileType<CradleTiles.PetrifiedDepthstoneTile>(), ModContent.TileType<CradleTiles.PetrifiedSandTile>() };
            TileObjectData.newTile.CoordinateHeights = new int[] { 18 };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.RandomStyleRange = 3;
            TileObjectData.addTile(Type);
            HitSound = SoundID.Grass;
            DustType = DustID.Stone;
            AddMapEntry(new Color(100, 150, 100));
        }
        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
    }
    public class AbyssTallGrassTile : ModTile
    {
        public override string Texture => "jarkos/tiles/AbyssTallGrassTile";
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileCut[Type] = true;
            Main.tileSolid[Type] = false;
            Main.tileMergeDirt[Type] = false;
            Main.tileWaterDeath[Type] = true;
            Main.tileLavaDeath[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
            TileObjectData.newTile.AnchorValidTiles = new int[] { ModContent.TileType<CradleTiles.PetrifiedDepthstoneTile>(), ModContent.TileType<CradleTiles.PetrifiedSandTile>() };
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 18 };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.RandomStyleRange = 3;
            TileObjectData.addTile(Type);
            HitSound = SoundID.Grass;
            DustType = DustID.Stone;
            AddMapEntry(new Color(100, 180, 100));
        }
        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
    }
    public class GlowsporeBushTile : ModTile
    {
        public override string Texture => "jarkos/tiles/GlowsporeBushTile";
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileCut[Type] = true;
            Main.tileSolid[Type] = false;
            Main.tileMergeDirt[Type] = false;
            Main.tileWaterDeath[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileLighted[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.AnchorValidTiles = new int[] { ModContent.TileType<CradleTiles.PetrifiedDepthstoneTile>(), ModContent.TileType<CradleTiles.PetrifiedSandTile>() };
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 18 };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.addTile(Type);
            HitSound = SoundID.Grass;
            DustType = DustID.GlowingMushroom;
            AddMapEntry(new Color(150, 100, 200));
        }
        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.6f;
            g = 0.3f;
            b = 0.8f;
        }
    }
    public class PetrifiedRootTile : ModTile
    {
        public override string Texture => "jarkos/tiles/PetrifiedRootTile";
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileCut[Type] = true;
            Main.tileSolid[Type] = false;
            Main.tileMergeDirt[Type] = false;
            Main.tileWaterDeath[Type] = true;
            Main.tileLavaDeath[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidBottom, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
            TileObjectData.newTile.AnchorValidTiles = new int[] { ModContent.TileType<CradleTiles.PetrifiedDepthstoneTile>(), ModContent.TileType<CradleTiles.PetrifiedSandTile>() };
            TileObjectData.newTile.CoordinateHeights = new int[] { 18 };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.RandomStyleRange = 3;
            TileObjectData.addTile(Type);
            HitSound = SoundID.Grass;
            DustType = DustID.Stone;
            AddMapEntry(new Color(80, 80, 80));
        }
        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
    }
}
