using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ObjectData;
using jarkos.UI;
using Terraria.ID;
using Terraria.GameContent.ObjectInteractions;
using jarkos.items.placeable;
using Microsoft.Xna.Framework.Graphics;
namespace jarkos.tiles
{
    public class RuneAltarTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;
            TileID.Sets.HasOutlines[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 14 };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 0;
            TileObjectData.addTile(Type);
            AddMapEntry(new Color(100, 100, 255), Terraria.Localization.Language.GetText("Rune Altar"));
        }
        public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;
        public override bool RightClick(int i, int j)
        {
            RuneAltarUISystem.OpenUI(new Point(i, j));
            return true;
        }

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
            player.cursorItemIconID = ModContent.ItemType<RuneAltarItem>();
        }
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];
            if (tile.TileFrameX == 0 && tile.TileFrameY == 0)
            {
                if (ModContent.HasAsset("jarkos/tiles/RuneAltarTile_Fill"))
                {
                    var fillTex = ModContent.Request<Texture2D>("jarkos/tiles/RuneAltarTile_Fill").Value;
                    Vector2 zero = new Vector2(Main.offScreenRange, Main.offScreenRange);
                    if (Main.drawToScreen) zero = Vector2.Zero;
                    Vector2 pos = new Vector2(i * 16, j * 16) - Main.screenPosition + zero;
                    Color color = Main.DiscoColor;
                    spriteBatch.Draw(fillTex, pos, null, color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                }
            }
        }
    }
}
