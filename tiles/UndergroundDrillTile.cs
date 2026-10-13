using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ModLoader;
using Terraria.ObjectData;
using jarkos.UI;
using Terraria.GameContent;
using Terraria.UI.Chat;
using jarkos.items.usetiles;

namespace jarkos.tiles
{
    public class UndergroundDrillTile : ModTile
    {
        public override string Texture => "jarkos/tiles/Underground_drill_Tile";

        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = false;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
            TileObjectData.newTile.Width = 5;
            TileObjectData.newTile.Height = 5;
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16, 16, 16 };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.Origin = new Point16(2, 4);
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<UndergroundDrillTileEntity>().Hook_AfterPlacement, -1, 0, false);
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(130, 110, 160), CreateMapEntryName());
        }

        public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;

        public override bool CanDrop(int i, int j)
        {
            return false;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            int left = i - frameX / 18;
            int top = j - frameY / 18;
            Point16 pos = new Point16(left, top);

            Item dropItem = new Item(ModContent.ItemType<UndergroundDrillItem>());

            if (TileEntity.ByPosition.TryGetValue(pos, out TileEntity te) && te is UndergroundDrillTileEntity drillTe)
            {
                if (dropItem.ModItem is UndergroundDrillItem drillItem)
                {
                    for (int slot = 0; slot < 5; slot++)
                    {
                        if (drillTe.OutputSlots[slot] != null && !drillTe.OutputSlots[slot].IsAir)
                        {
                            drillItem.SavedOutputSlots[slot] = drillTe.OutputSlots[slot].Clone();
                        }
                    }
                }
            }

            Item.NewItem(new EntitySource_TileBreak(left, top), left * 16, top * 16, 80, 80, dropItem);
            ModContent.GetInstance<UndergroundDrillTileEntity>().Kill(left, top);
        }

        public override void PlaceInWorld(int i, int j, Item item)
        {
            Point16 tePosition = new Point16(i, j);
            if (TileEntity.ByPosition.TryGetValue(tePosition, out TileEntity te) && te is UndergroundDrillTileEntity drillTe)
            {
                if (item.ModItem is UndergroundDrillItem drillItem)
                {
                    for (int slot = 0; slot < 5; slot++)
                    {
                        if (drillItem.SavedOutputSlots[slot] != null && !drillItem.SavedOutputSlots[slot].IsAir)
                        {
                            drillTe.OutputSlots[slot] = drillItem.SavedOutputSlots[slot].Clone();
                        }
                    }
                    drillTe.SyncData();
                }
            }
        }

        public override bool RightClick(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            int frameX = tile.TileFrameX / 18;
            int frameY = tile.TileFrameY / 18;
            int left = i - frameX;
            int top = j - frameY;
            Point16 tePosition = new Point16(left, top);

            if (TileEntity.ByPosition.TryGetValue(tePosition, out TileEntity te))
            {
                if (te is UndergroundDrillTileEntity drillTe)
                {
                    UndergroundDrillUISystem.Instance.OpenUI(drillTe);
                }
            }
            return true;
        }

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
            player.cursorItemIconEnabled = true;
            player.cursorItemIconID = ModContent.ItemType<UndergroundDrillItem>();
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];
            if (tile.TileFrameX == 0 && tile.TileFrameY == 0)
            {
                Point16 tePosition = new Point16(i, j);
                if (TileEntity.ByPosition.TryGetValue(tePosition, out TileEntity te) && te is UndergroundDrillTileEntity drillTe)
                {
                    if (drillTe.IsFull)
                    {
                        float bounce = MathF.Sin((float)Main.timeForVisualEffects * 0.1f) * 4f;
                        Vector2 drawPos = new Vector2(i * 16 + 40 - Main.screenPosition.X, j * 16 - 22 + bounce - Main.screenPosition.Y);

                        ChatManager.DrawColorCodedStringWithShadow(
                            spriteBatch,
                            FontAssets.MouseText.Value,
                            "!",
                            drawPos,
                            Color.Yellow,
                            0f,
                            Vector2.Zero,
                            new Vector2(1.6f)
                        );
                    }
                }
            }
        }
    }
}
