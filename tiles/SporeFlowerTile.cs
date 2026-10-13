using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using System;
namespace jarkos.tiles
{
    public class SporeFlowerTile : ModTile
    {
        public override string Texture => "jarkos/tiles/SporeFlower";
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16, 16 };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.Origin = new Point16(1, 3);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<SporeFlowerTileEntity>().Hook_AfterPlacement, -1, 0, false);
            TileObjectData.addTile(Type);
            AddMapEntry(new Color(100, 200, 100), CreateMapEntryName());
        }
        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Point16 origin = GetTileOrigin(i, j);
            ModContent.GetInstance<SporeFlowerTileEntity>().Kill(origin.X, origin.Y);
        }
        public static Point16 GetTileOrigin(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            if (tile == null || !tile.HasTile) return new Point16(i, j);
            int partX = tile.TileFrameX / 18;
            int partY = tile.TileFrameY / 18;
            return new Point16(i - partX, j - partY);
        }
        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            return false;
        }
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];
            if (tile.TileFrameX == 0 && tile.TileFrameY == 0)
            {
                if (TileEntity.ByPosition.TryGetValue(new Point16(i, j), out TileEntity te) && te is SporeFlowerTileEntity flowerTe)
                {
                    Vector2 zero = new Vector2(Main.offScreenRange, Main.offScreenRange);
                    if (Main.drawToScreen) zero = Vector2.Zero;
                    Vector2 drawPos = new Vector2(i * 16, j * 16) - Main.screenPosition + zero;
                    Texture2D baseTexture = ModContent.Request<Texture2D>("jarkos/tiles/SporeFlower").Value;
                    Texture2D headTexture = ModContent.Request<Texture2D>("jarkos/tiles/SporeFlowerHead").Value;
                    Vector2 bottomCenter = drawPos + new Vector2(24, 64);
                    Vector2 basePos = bottomCenter - new Vector2(22, 6);
                    spriteBatch.Draw(baseTexture, basePos, new Rectangle(0, 0, 44, 6), Lighting.GetColor(i + 1, j + 3));
                    Vector2 headOrigin = new Vector2(22, 46);
                    Vector2 headPos = bottomCenter - new Vector2(0, 6);
                    Rectangle headFrameRect = new Rectangle(0, flowerTe.HeadFrame * 46, 44, 46);
                    float scaleY = flowerTe.ScaleY;
                    Color headColor = Lighting.GetColor(i + 1, j + 1);
                    float windSway = 0f;
                    windSway = Main.windSpeedCurrent * 0.15f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 1.5f + i * 0.1f);
                    spriteBatch.Draw(headTexture, headPos, headFrameRect, headColor, windSway, headOrigin, new Vector2(1f, scaleY), SpriteEffects.None, 0f);
                }
            }
        }
    }
}
