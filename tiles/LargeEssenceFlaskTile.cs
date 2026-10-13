using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria.ObjectData;
using jarkos.core.Data;
using jarkos.items.usetiles;
using Terraria.GameContent;
using Terraria.ID;
using Microsoft.Xna.Framework.Graphics;
using System;
namespace jarkos.tiles
{
    public class LargeEssenceFlaskTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16 };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 0;
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<LargeEssenceFlaskTileEntity>().Hook_AfterPlacement, -1, 0, false);
            TileObjectData.addTile(Type);
            HitSound = SoundID.Shatter;
            DustType = DustID.Glass;
            AddMapEntry(new Color(120, 180, 255), CreateMapEntryName());
        }
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];
            if (tile.TileFrameX != 0 || tile.TileFrameY != 0) return;
            if (TileEntity.ByPosition.TryGetValue(new Point16(i, j), out TileEntity te) && te is LargeEssenceFlaskTileEntity flaskTe)
            {
                if (flaskTe.TotalEssences > 0)
                {
                    Vector2 zero = new Vector2(Main.offScreenRange, Main.offScreenRange);
                    if (Main.drawToScreen) zero = Vector2.Zero;
                    Vector2 drawPos = new Vector2(i * 16, j * 16) - Main.screenPosition + zero;
                    int totalAmount = flaskTe.TotalEssences;
                    int maxWidth = 42;
                    int maxHeight = 40;
                    float currentY = drawPos.Y + 44;
                    float startX = drawPos.X + 3;
                    var essences = flaskTe.StoredEssences.Where(kv => kv.Value > 0).ToList();
                    foreach (var kvp in essences)
                    {
                        float ratio = (float)kvp.Value / LargeEssenceFlaskTileEntity.MaxCapacity;
                        int rectHeight = Math.Max(1, (int)(ratio * maxHeight));
                        if (rectHeight > 0)
                        {
                            currentY -= rectHeight;
                            Rectangle rect = new Rectangle((int)startX, (int)currentY, maxWidth, rectHeight);
                            Color color = EssencePalette.ColorOf(kvp.Key);
                            spriteBatch.Draw(TextureAssets.MagicPixel.Value, rect, color * 0.8f);
                        }
                    }
                }
            }
        }
        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            if (TileEntity.ByPosition.TryGetValue(new Point16(i, j), out TileEntity te) && te is LargeEssenceFlaskTileEntity flaskTe)
            {
                int itemIndex = Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 48, 48, ModContent.ItemType<LargeEssenceFlaskItem>());
                if (Main.item[itemIndex].ModItem is LargeEssenceFlaskItem flaskItem)
                {
                    foreach (var kvp in flaskTe.StoredEssences)
                    {
                        flaskItem.StoredEssences[kvp.Key] = kvp.Value;
                    }
                }
                ModContent.GetInstance<LargeEssenceFlaskTileEntity>().Kill(i, j);
            }
        }
        public override System.Collections.Generic.IEnumerable<Item> GetItemDrops(int i, int j)
        {
            yield break;
        }
    }
}
