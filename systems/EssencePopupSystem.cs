using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.GameContent;
using Terraria.ModLoader;
using jarkos.core.Data;
using Terraria.UI.Chat;

namespace jarkos.systems
{
    public class EssencePopup
    {
        public EssenceType Type;
        public int TotalAmount;
        public float Lifetime;
        public float MaxLifetime = 120f;
        public float CurrentYOffset;
        public float TargetYOffset;
        public int PlayerIndex;

        public EssencePopup(int playerIndex, EssenceType type, int totalAmount, float startYOffset)
        {
            PlayerIndex = playerIndex;
            Type = type;
            TotalAmount = totalAmount;
            Lifetime = 0f;
            CurrentYOffset = startYOffset;
            TargetYOffset = startYOffset;
        }
    }

    public class EssencePopupSystem : ModSystem
    {
        private static readonly List<EssencePopup> activePopups = new();

        public static void AddPopup(Player player, EssenceType type, int totalAmount)
        {
            if (Main.netMode == NetmodeID.Server || player == null || !player.active || !(ModContent.GetInstance<Jarkos_config>()?.ShowEssencePickupText ?? true))
                return;

            float spacing = 26f;
            foreach (var popup in activePopups)
            {
                if (popup.PlayerIndex == player.whoAmI)
                {
                    popup.TargetYOffset -= spacing;
                }
            }

            var newPopup = new EssencePopup(player.whoAmI, type, totalAmount, -40f);
            activePopups.Add(newPopup);
        }

        public override void PostUpdateEverything()
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            for (int i = activePopups.Count - 1; i >= 0; i--)
            {
                var popup = activePopups[i];
                popup.Lifetime++;


                popup.CurrentYOffset = MathHelper.Lerp(popup.CurrentYOffset, popup.TargetYOffset, 0.25f);


                popup.TargetYOffset -= 0.3f;

                if (popup.Lifetime >= popup.MaxLifetime)
                {
                    activePopups.RemoveAt(i);
                }
            }
        }

        public override void PostDrawInterface(SpriteBatch spriteBatch)
        {
            if (Main.netMode == NetmodeID.Server || activePopups.Count == 0 || !(ModContent.GetInstance<Jarkos_config>()?.ShowEssencePickupText ?? true))
                return;

            var font = FontAssets.MouseText.Value;
            if (font == null) return;

            foreach (var popup in activePopups)
            {
                if (popup.PlayerIndex < 0 || popup.PlayerIndex >= Main.maxPlayers)
                    continue;

                Player player = Main.player[popup.PlayerIndex];
                if (!player.active) continue;

                Vector2 worldPos = player.Center + new Vector2(0f, popup.CurrentYOffset);
                Vector2 screenPos = worldPos - Main.screenPosition;


                float spawnProgress = MathHelper.Clamp(popup.Lifetime / 10f, 0f, 1f);
                float scale = MathHelper.Lerp(0.6f, 1.0f, spawnProgress);

                float opacity = 1f;
                if (popup.Lifetime < 10f)
                    opacity = popup.Lifetime / 10f;
                else if (popup.Lifetime > 90f)
                    opacity = 1f - (popup.Lifetime - 90f) / 30f;
                opacity = MathHelper.Clamp(opacity, 0f, 1f);

                if (opacity <= 0f) continue;

                string text = $"{EssencePalette.DisplayName(popup.Type)} x{popup.TotalAmount}";
                Color essenceColor = EssencePalette.ColorOf(popup.Type) * opacity;

                Texture2D iconTex = EssencePalette.GetTexture(popup.Type);
                Vector2 textSize = font.MeasureString(text) * scale;
                float iconSize = 20f * scale;
                float gap = 6f * scale;

                float totalWidth = (iconTex != null ? iconSize + gap : 0f) + textSize.X;
                Vector2 startPos = screenPos - new Vector2(totalWidth / 2f, textSize.Y / 2f);


                if (iconTex != null)
                {
                    Vector2 iconCenter = startPos + new Vector2(iconSize / 2f, textSize.Y / 2f);
                    float iconScale = iconSize / Math.Max(iconTex.Width, iconTex.Height);
                    spriteBatch.Draw(iconTex, iconCenter, null, Color.White * opacity, 0f, iconTex.Size() / 2f, iconScale, SpriteEffects.None, 0f);
                    startPos.X += iconSize + gap;
                }


                ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, text, startPos, essenceColor, 0f, Vector2.Zero, new Vector2(scale));
            }
        }
    }
}
