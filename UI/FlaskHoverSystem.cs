using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using jarkos.tiles;
using jarkos.core.Data;
using Terraria.DataStructures;
using Terraria.UI.Chat;
using Terraria.GameContent;

namespace jarkos.UI
{
    public class FlaskHoverSystem : ModSystem
    {
        public override void PostDrawInterface(SpriteBatch spriteBatch)
        {
            if (Main.HoveringOverAnNPC || Main.LocalPlayer.mouseInterface) return;
            Point tileCoords = Main.MouseWorld.ToTileCoordinates();
            
            if (tileCoords.X >= 0 && tileCoords.X < Main.maxTilesX && tileCoords.Y >= 0 && tileCoords.Y < Main.maxTilesY)
            {
                Tile tile = Main.tile[tileCoords.X, tileCoords.Y];
                if (tile.HasTile)
                {
                    if (tile.TileType == ModContent.TileType<EssenceFlaskTile>())
                    {
                        if (TileEntity.ByPosition.TryGetValue(new Point16(tileCoords.X, tileCoords.Y), out TileEntity te) && te is EssenceFlaskTileEntity flaskTe)
                        {
                            DrawFlaskHover(spriteBatch, flaskTe);
                        }
                    }
                    else if (tile.TileType == ModContent.TileType<LargeEssenceFlaskTile>())
                    {
                        int topX = tileCoords.X - tile.TileFrameX / 18;
                        int topY = tileCoords.Y - tile.TileFrameY / 18;
                        if (TileEntity.ByPosition.TryGetValue(new Point16(topX, topY), out TileEntity te) && te is LargeEssenceFlaskTileEntity largeFlaskTe)
                        {
                            DrawLargeFlaskHover(spriteBatch, largeFlaskTe);
                        }
                    }
                }
            }
        }
        private void DrawFlaskHover(SpriteBatch spriteBatch, EssenceFlaskTileEntity flaskTe)
        {
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
            player.cursorItemIconEnabled = false;

            if ((int)flaskTe.storedType == -1)
            {
                ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, "Empty (0 / 1000)", Main.MouseScreen + new Vector2(24, 24), Color.White, 0f, Vector2.Zero, Vector2.One);
                return;
            }
            string text = $"{EssencePalette.DisplayName(flaskTe.storedType)} Essence: {flaskTe.storedAmount} / 1000";
            Vector2 drawPos = Main.MouseScreen + new Vector2(24, 24);
            Texture2D icon = EssencePalette.GetTexture(flaskTe.storedType);

            if (icon != null)
            {
                float scale = 24f / MathHelper.Max(icon.Width, icon.Height);
                spriteBatch.Draw(icon, drawPos + new Vector2(12, 12), null, Color.White, 0f, new Vector2(icon.Width / 2f, icon.Height / 2f), scale, SpriteEffects.None, 0f);
            }
            Vector2 textPos = drawPos + new Vector2(28, 0);
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, text, textPos, Color.White, 0f, Vector2.Zero, Vector2.One);
        }
        private void DrawLargeFlaskHover(SpriteBatch spriteBatch, LargeEssenceFlaskTileEntity flaskTe)
        {
            Player player = Main.LocalPlayer;
            player.noThrow = 2;
            player.cursorItemIconEnabled = false;

            if (flaskTe.TotalEssences == 0)
            {
                ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, $"Empty (0 / {LargeEssenceFlaskTileEntity.MaxCapacity})", Main.MouseScreen + new Vector2(24, 24), Color.White, 0f, Vector2.Zero, Vector2.One);
                return;
            }
            Vector2 drawPos = Main.MouseScreen + new Vector2(24, 24);
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, $"Stored ({flaskTe.TotalEssences} / {LargeEssenceFlaskTileEntity.MaxCapacity}):", drawPos, Color.White, 0f, Vector2.Zero, Vector2.One);
            drawPos.Y += 24;

            foreach (var kvp in flaskTe.StoredEssences)
            {
                if (kvp.Value > 0)
                {
                    Texture2D icon = EssencePalette.GetTexture(kvp.Key);
                    if (icon != null)
                    {
                        float scale = 20f / MathHelper.Max(icon.Width, icon.Height);
                        spriteBatch.Draw(icon, drawPos + new Vector2(10, 10), null, Color.White, 0f, new Vector2(icon.Width / 2f, icon.Height / 2f), scale, SpriteEffects.None, 0f);
                    }
                    string text = $"{EssencePalette.DisplayName(kvp.Key)}: {kvp.Value}";
                    Vector2 textPos = drawPos + new Vector2(24, 0);
                    ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, text, textPos, Color.White, 0f, Vector2.Zero, Vector2.One);
                    drawPos.Y += 22;
                }
            }
        }
    }
}
