using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using jarkos.UI;

namespace jarkos.tiles
{
    public class GoldenTrophyTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;
            TileID.Sets.HasOutlines[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16 };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(255, 215, 0), Terraria.Localization.Language.GetText("Golden Trophy"));
        }

        public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;

        public override bool RightClick(int i, int j)
        {
            GoldenTrophyUISystem.OpenUI(new Point(i, j));
            return true;
        }

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
            player.cursorItemIconID = ModContent.ItemType<items.placeable.GoldenTrophyItem>();
        }

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
        {
            Tile tile = Main.tile[i, j];
            if (tile.TileFrameX == 0 && tile.TileFrameY == 0)
            {

                Lighting.AddLight(new Vector2(i * 16 + 24, j * 16 + 24), new Vector3(0.9f, 0.75f, 0.2f));


                if (!Main.gamePaused && Main.instance.IsActive && Main.rand.NextBool(4))
                {
                    Vector2 spawnPos = new Vector2(i * 16 + Main.rand.NextFloat(4, 44), j * 16 + Main.rand.NextFloat(12, 44));
                    Vector2 speed = new Vector2(Main.rand.NextFloat(-0.25f, 0.25f), Main.rand.NextFloat(-1.2f, -0.4f));
                    Dust dust = Dust.NewDustPerfect(spawnPos, DustID.GoldFlame, speed, 80, default, Main.rand.NextFloat(0.5f, 0.8f));
                    dust.noGravity = true;
                    dust.fadeIn = 0.2f;
                }
            }
        }
    }
}
