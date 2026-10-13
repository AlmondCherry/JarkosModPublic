using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using System.Linq;
using jarkos.UI;
using Terraria.GameContent.ObjectInteractions;
using jarkos.core.PlayerInteract;
using Microsoft.Xna.Framework.Graphics;
using System;
namespace jarkos.tiles
{
    public class DistillerTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = false;
            TileID.Sets.HasOutlines[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.Width = 2;
            TileObjectData.newTile.Height = 4;
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16, 22 };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 0;
            TileObjectData.newTile.Origin = new Point16(0, 3);
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<DistillerTileEntity>().Hook_AfterPlacement, -2, 0, false);
            TileObjectData.addTile(Type);
            AddMapEntry(new Color(200, 200, 200), CreateMapEntryName());
        }

        public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;

        public override bool CanPlace(int i, int j)
        {
            if (Main.netMode == NetmodeID.Server) return true;
            int limit = 1;
            try
            {
                var plr = Main.LocalPlayer.GetModPlayer<ResearchPlayer>();
                if (plr != null && plr.IsCompleted("jarkos:AquaNode8"))
                {
                    limit = 2;
                }
            }
            catch { }
            int count = TileEntity.ByID.Values.OfType<DistillerTileEntity>().Count();
            if (count >= limit)
            {
                if (limit == 1)
                    Main.NewText("You can only place one Distiller in the world!", Color.Red);
                else
                    Main.NewText("You can only place two Distiller in the world!", Color.Red);
                return false;
            }
            return true;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            int left = i - frameX / 16;
            int top = j - frameY / 16;
            ModContent.GetInstance<DistillerTileEntity>().Kill(left, top);
        }

        public override bool RightClick(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            int frameX = tile.TileFrameX / 16;
            int frameY = tile.TileFrameY / 16;
            int left = i - frameX;
            int top = j - frameY;
            Point16 tePosition = new Point16(left, top);
            if (TileEntity.ByPosition.TryGetValue(tePosition, out TileEntity te))
            {
                if (te is DistillerTileEntity dte)
                {
                    DistillerUISystem.Instance.OpenUI(dte);
                }
            }
            else
            {
                if (TryPlaceMissingTE(left, top))
                {
                }
            }
            return true;
        }

        private bool TryPlaceMissingTE(int left, int top)
        {
            Tile tile = Main.tile[left, top];
            if (tile.HasTile && tile.TileType == Type)
            {
                int id = ModContent.GetInstance<DistillerTileEntity>().Place(left, top);
                if (id != -1)
                {
                    if (TileEntity.ByPosition.TryGetValue(new Point16(left, top), out TileEntity te) && te is DistillerTileEntity dte)
                    {
                        DistillerUISystem.Instance.OpenUI(dte);
                        return true;
                    }
                }
            }
            return false;
        }

        public override void MouseOver(int i, int j)
        {
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];
            if (tile.TileFrameX == 0 && tile.TileFrameY == 0)
            {
                if (TileEntity.ByPosition.TryGetValue(new Point16(i, j), out TileEntity te) && te is DistillerTileEntity dte)
                {
                    if (dte.GetTotalEssences() >= DistillerTileEntity.MaxCapacity)
                    {
                        if (ModContent.HasAsset("jarkos/UI/WarningIcon"))
                        {
                            var warningTex = ModContent.Request<Texture2D>("jarkos/UI/WarningIcon").Value;
                            float offset = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 3f) * 4f;
                            Vector2 zero = new Vector2(Main.offScreenRange, Main.offScreenRange);

                            if (Main.drawToScreen) zero = Vector2.Zero;

                            Vector2 pos = new Vector2(i * 16, j * 16) - Main.screenPosition + zero;
                            Vector2 drawPos = pos + new Vector2(16, -30 + offset);
                            
                            spriteBatch.Draw(warningTex, drawPos, null, Color.White, 0f, warningTex.Size() / 2f, 1f, SpriteEffects.None, 0f);
                        }
                    }
                }
            }
        }
    }
}
