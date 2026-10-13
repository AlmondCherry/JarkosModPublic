using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI.Chat;
using jarkos.tiles;
using jarkos.core.PlayerInteract;
using jarkos.core.Data;
using jarkos.UI;

namespace jarkos.systems
{
    public class ResearchTableDrawSystem : ModSystem
    {
        private static float _panelProgress = 0f;
        private static Point16 _activeTablePos = new Point16(-1, -1);

        public override void PostDrawInterface(SpriteBatch spriteBatch)
        {
            if (Main.dedServ || Main.gameMenu) return;
            if (ResearchScreen.Instance != null && ResearchScreen.Instance.Active) return;

            if (Main.playerInventory || Main.inFancyUI || Main.ingameOptionsWindow || Main.npcChatText != "" || Main.editChest || Main.mapFullscreen)
            {
                _panelProgress = 0f;
                return;
            }

            DrawOwnerNames(spriteBatch);

            if (Jarkos_config.HoverEssencePanelEnabled)
            {
                DrawHoverEssencePanel(spriteBatch);
            }
            else
            {
                _panelProgress = 0f;
            }
        }

        private void DrawOwnerNames(SpriteBatch spriteBatch)
        {
            foreach (var kvp in TileEntity.ByPosition)
            {
                if (kvp.Value is ResearchTableTileEntity te && !string.IsNullOrEmpty(te.OwnerName))
                {
                    Point16 pos = te.Position;
                    int i = pos.X;
                    int j = pos.Y;

                    float floatOffset = (float)Math.Sin((Main.GameUpdateCount + i * 10 + j * 5) * 0.05f) * 4f;
                    Vector2 worldPos = new Vector2(i * 16 + 32, j * 16 + 56 + floatOffset);
                    Vector2 screenPos = worldPos - Main.screenPosition;
                    Vector2 stringSize = FontAssets.MouseText.Value.MeasureString(te.OwnerName);

                    if (screenPos.X + stringSize.X / 2f > -100 && screenPos.X - stringSize.X / 2f < Main.screenWidth + 100 && screenPos.Y > -100 && screenPos.Y < Main.screenHeight + 100)
                    {
                        ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, te.OwnerName, screenPos, Color.Cyan, 0f, stringSize / 2f, new Vector2(1f));
                    }
                }
            }
        }

        private void DrawHoverEssencePanel(SpriteBatch sb)
        {
            if (Main.playerInventory || Main.inFancyUI || Main.ingameOptionsWindow || Main.npcChatText != "" || Main.editChest || Main.mapFullscreen)
            {
                _panelProgress = 0f;
                return;
            }

            Point tileCoords = Main.MouseWorld.ToTileCoordinates();
            bool hoveringTable = false;
            Point16 currentOrigin = new Point16(-1, -1);

            if (WorldGen.InWorld(tileCoords.X, tileCoords.Y, 10))
            {
                Tile tile = Main.tile[tileCoords.X, tileCoords.Y];
                if (tile != null && tile.HasTile && tile.TileType == ModContent.TileType<ResearchTableTile>())
                {
                    int originX = tileCoords.X - (tile.TileFrameX / 18);
                    int originY = tileCoords.Y - (tile.TileFrameY / 18);
                    currentOrigin = new Point16(originX, originY);
                    hoveringTable = true;
                }
            }

            if (hoveringTable)
            {
                _activeTablePos = currentOrigin;
            }

            if (_activeTablePos.X < 0) return;

            Player player = Main.LocalPlayer;
            if (player == null || !player.active) return;

            var rPlayer = player.GetModPlayer<ResearchPlayer>();
            if (rPlayer == null || rPlayer.Essences == null) return;

            List<KeyValuePair<EssenceType, int>> ownedEssences = new List<KeyValuePair<EssenceType, int>>();
            foreach (var kvp in rPlayer.Essences)
            {
                if (kvp.Value > 0) ownedEssences.Add(kvp);
            }

            float floatOffset = (float)Math.Sin(Main.GameUpdateCount * 0.07f) * 4f;
            Vector2 tableTopWorld = new Vector2(_activeTablePos.X * 16 + 32, _activeTablePos.Y * 16 + floatOffset);
            Vector2 screenPos = tableTopWorld - Main.screenPosition;

            int cols = 5;
            int slotSize = 42;
            int padding = 6;
            int rows = ownedEssences.Count > 0 ? (int)Math.Ceiling(ownedEssences.Count / (double)cols) : 1;

            int headerHeight = 26;
            int panelWidth = cols * slotSize + (cols + 1) * padding + 12;
            int panelHeight = headerHeight + rows * slotSize + (rows + 1) * padding + 8;

            float eased = MathSystem.EaseOutCubic(_panelProgress);
            float currentScale = MathHelper.Lerp(0.15f, 1.0f, eased);
            float panelAlpha = eased;

            Vector2 startCenter = new Vector2(screenPos.X, screenPos.Y);
            Vector2 targetTopCenter = new Vector2(screenPos.X, screenPos.Y - panelHeight - 10f);
            Vector2 targetCenter = new Vector2(screenPos.X, targetTopCenter.Y + panelHeight / 2f);
            Vector2 currentCenter = Vector2.Lerp(startCenter, targetCenter, eased);

            Rectangle panelRect = new Rectangle((int)(targetTopCenter.X - panelWidth / 2f), (int)targetTopCenter.Y, panelWidth, panelHeight);

            Matrix animMatrix = Matrix.CreateTranslation(new Vector3(-targetCenter.X, -targetCenter.Y, 0f)) * Matrix.CreateScale(currentScale, currentScale, 1f) * Matrix.CreateTranslation(new Vector3(currentCenter.X, currentCenter.Y, 0f)) * Main.UIScaleMatrix;

            Vector2 transformedMouse = Vector2.Transform(Main.MouseScreen, Matrix.Invert(animMatrix));

            bool hoveringPanel = _panelProgress > 0.05f && panelRect.Contains(transformedMouse.ToPoint());
            bool keepOpen = hoveringTable || hoveringPanel;

            if (keepOpen)
            {
                _panelProgress = Math.Min(1f, _panelProgress + 0.06f);
            }
            else
            {
                _panelProgress = Math.Max(0f, _panelProgress - 0.06f);
            }

            if (_panelProgress <= 0.001f) return;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, animMatrix);

            Terraria.Utils.DrawInvBG(sb, panelRect, new Color(10, 40, 75, (int)(135 * panelAlpha)));

            Color borderColor = new Color(60, 200, 255) * (0.9f * panelAlpha);
            sb.Draw(TextureAssets.MagicPixel.Value, new Rectangle(panelRect.X, panelRect.Y, panelRect.Width, 2), borderColor);
            sb.Draw(TextureAssets.MagicPixel.Value, new Rectangle(panelRect.X, panelRect.Bottom - 2, panelRect.Width, 2), borderColor);
            sb.Draw(TextureAssets.MagicPixel.Value, new Rectangle(panelRect.X, panelRect.Y, 2, panelRect.Height), borderColor);
            sb.Draw(TextureAssets.MagicPixel.Value, new Rectangle(panelRect.Right - 2, panelRect.Y, 2, panelRect.Height), borderColor);

            string title = "Your Essences";
            Vector2 titleSize = FontAssets.MouseText.Value.MeasureString(title) * 0.85f;

            ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, title, new Vector2(panelRect.X + (panelWidth - titleSize.X) / 2f, panelRect.Y + 6), new Color(120, 230, 255) * panelAlpha, 0f, Vector2.Zero, new Vector2(0.85f));

            if (ownedEssences.Count == 0)
            {
                string emptyText = "You have no essences";
                Vector2 emptySize = FontAssets.MouseText.Value.MeasureString(emptyText) * 0.85f;

                ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, emptyText, new Vector2(panelRect.X + (panelWidth - emptySize.X) / 2f, panelRect.Y + headerHeight + 12), Color.LightCyan * (0.7f * panelAlpha), 0f, Vector2.Zero, new Vector2(0.85f));
            }
            else
            {
                string hoveredEssenceName = null;
                int hoveredCount = 0;
                EssenceType hoveredType = (EssenceType)(-1);

                for (int index = 0; index < ownedEssences.Count; index++)
                {
                    var kvp = ownedEssences[index];
                    int r = index / cols;
                    int c = index % cols;

                    int slotX = panelRect.X + padding + 6 + c * (slotSize + padding);
                    int slotY = panelRect.Y + headerHeight + padding + r * (slotSize + padding);
                    Rectangle slotRect = new Rectangle(slotX, slotY, slotSize, slotSize);

                    bool hoverSlot = slotRect.Contains(transformedMouse.ToPoint());
                    if (hoverSlot)
                    {
                        hoveredEssenceName = EssencePalette.DisplayName(kvp.Key);
                        hoveredCount = kvp.Value;
                        hoveredType = kvp.Key;
                    }

                    Terraria.Utils.DrawInvBG(sb, slotRect, hoverSlot ? new Color(30, 90, 150, (int)(185 * panelAlpha)) : new Color(15, 45, 80, (int)(140 * panelAlpha)));

                    Texture2D eTex = EssencePalette.GetTexture(kvp.Key);
                    if (eTex != null)
                    {
                        sb.Draw(eTex, slotRect.Center.ToVector2(), null, Color.White * panelAlpha, 0f, eTex.Size() / 2f, 0.95f, SpriteEffects.None, 0f);
                    }

                    string countStr = kvp.Value.ToString();
                    Vector2 countSize = FontAssets.MouseText.Value.MeasureString(countStr) * 0.75f;
                    ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, countStr, new Vector2(slotRect.Right - countSize.X - 3, slotRect.Bottom - countSize.Y - 1), Color.White * panelAlpha, 0f, Vector2.Zero, new Vector2(0.75f));
                }

                if (!string.IsNullOrEmpty(hoveredEssenceName))
                {
                    string pcsText = "pcs";
                    string tooltip = $"{hoveredEssenceName}: {hoveredCount} {pcsText}";

                    Vector2 ttSize = FontAssets.MouseText.Value.MeasureString(tooltip) * 0.85f;
                    Vector2 ttPos = new Vector2(transformedMouse.X + 16, transformedMouse.Y + 16);
                    
                    if (ttPos.X + ttSize.X > Main.screenWidth) ttPos.X = transformedMouse.X - ttSize.X - 16;
                    if (ttPos.Y + ttSize.Y > Main.screenHeight) ttPos.Y = Main.screenHeight - ttSize.Y - 16;

                    Color ttColor = EssencePalette.ColorOf(hoveredType);
                    ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, tooltip, ttPos, ttColor * panelAlpha, 0f, Vector2.Zero, new Vector2(0.85f));
                }
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
        }
    }
}
