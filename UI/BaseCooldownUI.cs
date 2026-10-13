using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.UI;
using Terraria.ModLoader;
using System.Collections.Generic;
using System;
using jarkos.items.accs.amulets;
using jarkos.items.accs.melee;
using System.Linq;
using jarkos.items.accs.emblems;
using jarkos.items.accs.bloodmage;
using jarkos;

namespace Jarkos.UI
{
    public class UISizeConfig
    {
        public float BarWidth { get; set; }
        public float BarHeight { get; set; }
        public float IconSize { get; set; }
        public float FontScale { get; set; }
        public bool ShowText { get; set; }
        public bool ShowShimmer { get; set; }
        public static UISizeConfig GetConfig(int activeCount)
        {
            switch (activeCount)
            {
                case 1:
                    return new UISizeConfig
                    {
                        BarWidth = 180f, BarHeight = 12f, IconSize = 24f, FontScale = 0.75f, ShowText = true, ShowShimmer = true
                    };
                case 2:
                    return new UISizeConfig
                    {
                        BarWidth = 150f, BarHeight = 10f, IconSize = 20f, FontScale = 0.65f, ShowText = true, ShowShimmer = true
                    };
                case 3:
                    return new UISizeConfig
                    {
                        BarWidth = 120f, BarHeight = 8f, IconSize = 16f, FontScale = 0.55f, ShowText = true, ShowShimmer = false
                    };
                case 4:
                    return new UISizeConfig
                    {
                        BarWidth = 100f, BarHeight = 7f, IconSize = 14f, FontScale = 0.5f, ShowText = true, ShowShimmer = false
                    };
                case 5:
                default:
                    return new UISizeConfig
                    {
                        BarWidth = 80f, BarHeight = 6f, IconSize = 12f, FontScale = 0.45f, ShowText = false, ShowShimmer = false
                    };
            }
        }
    }
    public interface ICooldownProvider
    {
        bool IsEquipped();
        int GetCurrentCooldown();
        int GetMaxCooldown();
        Texture2D GetIconTexture();
        string GetDisplayName();
        Color GetCustomColor();
    }
    public abstract class BaseCooldownUI : UIElement
    {
        public float BarWidth = 180f;
        public float BarHeight = 12f;
        public float IconSize = 24f;
        public float Padding => IconSize * 0.25f;
        public float FontScale = 0.75f;
        public bool ShowText = true;
        public bool ShowShimmer = true;
        public virtual float TotalWidth => IconSize + Padding + BarWidth + (ShowText ? 40f : 10f);
        public virtual float TotalHeight => Math.Max(IconSize, BarHeight) + (IconSize * 0.5f);
        public float currentAlpha = 0f;
        public float targetAlpha = 0f;
        public const float FadeInSpeed = 0.08f;
        public const float FadeOutSpeed = 0.03f;
        public int fadeOutDelay = 0;
        public const int FadeOutDelayMax = 90;
        public float readyPulse = 0f;
        public bool wasOnCooldown = false;
        public Vector2 currentPosition;
        public Vector2 targetPosition;
        public float currentScale = 1f;
        public float targetScale = 1f;
        public const float PositionLerpSpeed = 0.12f;
        public const float ScaleLerpSpeed = 0.1f;
        public Color barBackgroundColor = new Color(20, 10, 30, 200);
        public Color barBorderColor = new Color(140, 80, 200, 220);
        public Color barFillColorStart = new Color(180, 100, 255);
        public Color barFillColorEnd = new Color(255, 200, 80);
        public Color barReadyColor = new Color(100, 255, 120);
        public Color textColor = new Color(230, 220, 255);
        public ICooldownProvider provider;
        public UISizeConfig currentConfig;
        private Vector2 customOffset = Vector2.Zero;
        private float customVerticalOffset = 0f;
        public BaseCooldownUI(ICooldownProvider provider)
        {
            this.provider = provider;
            ApplyConfig(UISizeConfig.GetConfig(1));
        }
        public void ApplyConfig(UISizeConfig config)
        {
            currentConfig = config;
            BarWidth = config.BarWidth;
            BarHeight = config.BarHeight;
            IconSize = config.IconSize;
            FontScale = config.FontScale;
            ShowText = config.ShowText;
            ShowShimmer = config.ShowShimmer;
            Color customColor = provider.GetCustomColor();
            if (customColor != default)
            {
                barFillColorStart = Color.Lerp(customColor, Color.White, 0.3f);
                barFillColorEnd = customColor;
                barBorderColor = customColor * 0.8f;
                barReadyColor = Color.Lerp(customColor, Color.Lime, 0.5f);
            }
            targetScale = 1f;
            Width.Set(TotalWidth + 20f, 0f);
            Height.Set(TotalHeight + 10f, 0f);
        }
        public void SetPositionOffset(Vector2 offset, float verticalOffset = 0f)
        {
            customOffset = offset;
            customVerticalOffset = verticalOffset;
        }
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            bool isEquipped = provider.IsEquipped();
            int cooldown = provider.GetCurrentCooldown();
            UpdateVisibility(isEquipped, cooldown);
            UpdateAlpha();
            UpdatePulse(isEquipped, cooldown);
            UpdateScale();
            UpdatePosition();
        }
        protected virtual void UpdateVisibility(bool isEquipped, int cooldown)
        {
            if (!isEquipped)
            {
                targetAlpha = 0f;
                fadeOutDelay = 0;
            }
            else if (cooldown > 0)
            {
                targetAlpha = 1f;
                fadeOutDelay = FadeOutDelayMax;
                wasOnCooldown = true;
            }
            else if (wasOnCooldown)
            {
                if (fadeOutDelay > 0)
                {
                    fadeOutDelay--;
                    targetAlpha = 1f;
                }
                else
                {
                    targetAlpha = 0f;
                    if (currentAlpha <= 0.01f)
                        wasOnCooldown = false;
                }
            }
            else
            {
                targetAlpha = 0f;
            }
        }
        protected virtual void UpdateAlpha()
        {
            if (currentAlpha < targetAlpha)
            {
                currentAlpha += FadeInSpeed;
                if (currentAlpha > targetAlpha) currentAlpha = targetAlpha;
            }
            else if (currentAlpha > targetAlpha)
            {
                currentAlpha -= FadeOutSpeed;
                if (currentAlpha < 0f) currentAlpha = 0f;
            }
        }
        protected virtual void UpdatePulse(bool isEquipped, int cooldown)
        {
            if (isEquipped && cooldown <= 0 && wasOnCooldown && fadeOutDelay > 0)
                readyPulse += 0.08f;
            else
                readyPulse = 0f;
        }
        protected virtual void UpdateScale()
        {
            currentScale = currentScale * (1f - ScaleLerpSpeed) + targetScale * ScaleLerpSpeed;
        }
        protected virtual void UpdatePosition()
        {
            float screenWidth = Main.screenWidth;
            targetPosition = new Vector2((screenWidth / 2f) - (TotalWidth / 2f) + customOffset.X, 20f + customVerticalOffset + customOffset.Y);
            targetPosition.X = MathHelper.Clamp(targetPosition.X, 10f, screenWidth - TotalWidth - 10f);
            if (currentPosition == Vector2.Zero)
                currentPosition = targetPosition;
            else
                currentPosition = Vector2.Lerp(currentPosition, targetPosition, PositionLerpSpeed);
            Left.Set(currentPosition.X, 0f);
            Top.Set(currentPosition.Y, 0f);
        }
        public bool IsVisible() => currentAlpha > 0.01f;
        public bool ShouldBeActive() => provider.IsEquipped() || currentAlpha > 0.01f;
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            if (currentAlpha <= 0.01f)
                return;
            if (!provider.IsEquipped() && currentAlpha <= 0.01f)
                return;

            CalculatedStyle dims = GetDimensions();

            float posX = dims.X;
            float posY = dims.Y;
            int cooldown = provider.GetCurrentCooldown();
            int maxCooldown = provider.GetMaxCooldown();

            float cooldownProgress;
            float remainingSeconds;
            bool isReady;

            if (cooldown > 0)
            {
                cooldownProgress = 1f - ((float)cooldown / maxCooldown);
                remainingSeconds = cooldown / 60f;
                isReady = false;
            }
            else
            {
                cooldownProgress = 1f;
                remainingSeconds = 0f;
                isReady = true;
            }
            DrawAdaptiveBackground(spriteBatch, posX, posY);
            Vector2 iconPos = new Vector2(posX + 4, posY + (TotalHeight - IconSize) / 2f);

            DrawIcon(spriteBatch, iconPos, isReady);
            Vector2 barPos = new Vector2(posX + 4 + IconSize + Padding, posY + (TotalHeight - BarHeight) / 2f);

            DrawCooldownBar(spriteBatch, barPos, cooldownProgress, isReady);
            if (ShowText)
            {
                Vector2 textPos = new Vector2(barPos.X + BarWidth + 4, barPos.Y - 2);
                DrawTimerText(spriteBatch, textPos, remainingSeconds, isReady, maxCooldown);
            }
        }
        protected virtual void DrawAdaptiveBackground(SpriteBatch spriteBatch, float posX, float posY)
        {
            int alpha = (int)(180 * currentAlpha);

            if (currentConfig.IconSize <= 14)
                alpha = (int)(140 * currentAlpha);

            DrawRoundedRect(spriteBatch, new Vector2(posX - 2, posY - 2), TotalWidth + 4, TotalHeight + 4, new Color(10, 5, 20, alpha));
        }
        protected virtual void DrawIcon(SpriteBatch spriteBatch, Vector2 pos, bool isReady)
        {
            Texture2D texture = provider.GetIconTexture();
            if (texture == null) return;

            float scale = IconSize / Math.Max(texture.Width, texture.Height) * currentScale;
            float pulseScale = 1f;
            Color iconColor = Color.White * currentAlpha;

            if (isReady && readyPulse > 0)
            {
                float pulse = (float)Math.Sin(readyPulse) * 0.15f;
                pulseScale = 1f + pulse;
                iconColor = Color.Lerp(Color.White, barReadyColor, 0.3f + pulse) * currentAlpha;

                if (IconSize >= 16)
                {
                    float glowPulse = (float)Math.Sin(readyPulse) * 0.5f + 0.5f;
                    Color glowColor = barReadyColor * (0.3f * glowPulse * currentAlpha);

                    spriteBatch.Draw(texture, pos + new Vector2(IconSize / 2f, IconSize / 2f), null, glowColor, 0f, new Vector2(texture.Width / 2f, texture.Height / 2f), scale * pulseScale * 1.4f, SpriteEffects.None, 0f);
                }
            }
            spriteBatch.Draw(texture, pos + new Vector2(IconSize / 2f, IconSize / 2f), null, iconColor, 0f, new Vector2(texture.Width / 2f, texture.Height / 2f), scale * pulseScale, SpriteEffects.None, 0f);
        }
        protected virtual void DrawCooldownBar(SpriteBatch spriteBatch, Vector2 pos, float progress, bool isReady)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;

            float alpha = currentAlpha;
            int borderThickness = BarHeight >= 10 ? 1 : 0;

            if (borderThickness > 0)
            {
                DrawRectBorder(spriteBatch, pixel, pos, BarWidth, BarHeight, barBorderColor * alpha);
            }
            spriteBatch.Draw(pixel, new Rectangle((int)pos.X, (int)pos.Y, (int)BarWidth, (int)BarHeight), barBackgroundColor * alpha);

            float fillWidth = BarWidth * progress;
            if (fillWidth > 0)
            {
                Color fillColor;
                if (isReady)
                {
                    float pulse = (float)Math.Sin(readyPulse) * 0.2f + 0.8f;
                    fillColor = barReadyColor * pulse * alpha;
                }
                else
                {
                    fillColor = Color.Lerp(barFillColorStart, barFillColorEnd, progress) * alpha;
                }
                spriteBatch.Draw(pixel, new Rectangle((int)pos.X, (int)pos.Y, (int)fillWidth, (int)BarHeight), fillColor);
                if (BarHeight >= 8)
                {
                    int highlightHeight = Math.Max(1, (int)(BarHeight * 0.2f));
                    spriteBatch.Draw(pixel, new Rectangle((int)pos.X, (int)pos.Y, (int)fillWidth, highlightHeight), Color.White * 0.15f * alpha);
                }
                if (ShowShimmer && !isReady)
                {
                    DrawShimmerEffects(spriteBatch, pixel, pos, fillWidth);
                }
            }
        }
        protected void DrawRectBorder(SpriteBatch spriteBatch, Texture2D pixel, Vector2 pos, float width, float height, Color color)
        {
            spriteBatch.Draw(pixel, new Rectangle((int)pos.X - 1, (int)pos.Y - 1, (int)width + 2, 1), color);
            spriteBatch.Draw(pixel, new Rectangle((int)pos.X - 1, (int)(pos.Y + height), (int)width + 2, 1), color);
            spriteBatch.Draw(pixel, new Rectangle((int)pos.X - 1, (int)pos.Y - 1, 1, (int)height + 2), color);
            spriteBatch.Draw(pixel, new Rectangle((int)(pos.X + width), (int)pos.Y - 1, 1, (int)height + 2), color);
        }
        protected void DrawShimmerEffects(SpriteBatch spriteBatch, Texture2D pixel, Vector2 pos, float fillWidth)
        {
            int shimmerCount = BarWidth > 120 ? 3 : (BarWidth > 80 ? 2 : 1);
            for (int i = 0; i < shimmerCount; i++)
            {
                float speed = 0.8f + i * 0.4f;
                float offset = (Main.GameUpdateCount * speed + i * 60f) % fillWidth;

                if (offset < fillWidth - 2 && offset > 2)
                {
                    float pAlpha = (float)Math.Sin(offset / fillWidth * Math.PI) * 0.3f;
                    int shimmerWidth = BarHeight >= 8 ? 3 : 2;
                    int shimmerHeight = Math.Max(1, (int)BarHeight - 2);

                    spriteBatch.Draw(pixel, new Rectangle((int)(pos.X + offset), (int)pos.Y + 1, shimmerWidth, shimmerHeight), Color.White * pAlpha * currentAlpha);
                }
            }
        }
        protected virtual void DrawTimerText(SpriteBatch spriteBatch, Vector2 pos, float seconds, bool isReady, int maxCooldown)
        {
            string text;
            Color color;
            if (isReady)
            {
                text = IconSize >= 16 ? "OK" : "ok";
                float pulse = (float)Math.Sin(readyPulse) * 0.3f + 0.7f;
                color = barReadyColor * pulse * currentAlpha;
            }
            else
            {
                if (IconSize >= 20)
                    text = seconds >= 10f ? $"{seconds:F0}s" : $"{seconds:F1}s";
                else if (IconSize >= 14)
                    text = seconds >= 10f ? $"{seconds:F0}" : $"{seconds:F1}";
                else
                    text = $"{seconds:F0}";
                float progress = 1f - (seconds / (maxCooldown / 60f));
                color = Color.Lerp(textColor, barFillColorEnd, progress * 0.5f) * currentAlpha;
            }
            float textScale = FontScale * currentScale;
            if (IconSize >= 14)
            {
                Terraria.Utils.DrawBorderString(spriteBatch, text, pos + new Vector2(1, 1), Color.Black * currentAlpha * 0.5f, textScale);
            }
            Terraria.Utils.DrawBorderString(spriteBatch, text, pos, color, textScale);
        }
        protected void DrawRoundedRect(SpriteBatch spriteBatch, Vector2 pos, float width, float height, Color color)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            int cornerSize = Math.Min(2, (int)(width * 0.05f));
            if (cornerSize > 0)
            {
                spriteBatch.Draw(pixel, new Rectangle((int)(pos.X + cornerSize), (int)pos.Y, (int)(width - cornerSize * 2), (int)height), color);
                spriteBatch.Draw(pixel, new Rectangle((int)pos.X, (int)(pos.Y + cornerSize), cornerSize, (int)(height - cornerSize * 2)), color);
                spriteBatch.Draw(pixel, new Rectangle((int)(pos.X + width - cornerSize), (int)(pos.Y + cornerSize), cornerSize, (int)(height - cornerSize * 2)), color);
            }
            else
            {
                spriteBatch.Draw(pixel, new Rectangle((int)pos.X, (int)pos.Y, (int)width, (int)height), color);
            }
        }
    }
    public class ResonanceBellProvider : ICooldownProvider
    {
        public bool IsEquipped() => Main.LocalPlayer.GetModPlayer<ResonanceBellPlayer>().hasResonanceBell;
        public int GetCurrentCooldown() => Main.LocalPlayer.GetModPlayer<ResonanceBellPlayer>().bellCooldown;
        public int GetMaxCooldown() => ResonanceBellPlayer.BellCooldownMax;
        public Texture2D GetIconTexture() => TextureAssets.Item[ModContent.ItemType<ResonanceBell>()].Value;
        public string GetDisplayName() => "Resonance Bell";
        public Color GetCustomColor() => new Color(180, 100, 255);
    }
    public class HemoStabilizerProvider : ICooldownProvider
    {
        public bool IsEquipped() => Main.LocalPlayer.GetModPlayer<HemostabilizerPlayer>().hemostabilizerEquipped;
        public int GetCurrentCooldown() => Main.LocalPlayer.GetModPlayer<HemostabilizerPlayer>().hemostabilizerCooldown;
        public int GetMaxCooldown() => HemostabilizerPlayer.CooldownTime;
        public Texture2D GetIconTexture() => TextureAssets.Item[ModContent.ItemType<HemoStabilizer>()].Value;
        public string GetDisplayName() => "Hemo Stabilizer";
        public Color GetCustomColor() => new Color(255, 100, 100);
    }
    public class NuminousVoicesProvider : ICooldownProvider
    {
        public bool IsEquipped() => Main.LocalPlayer.GetModPlayer<NuminousVoicesPlayer>().hasNuminousVoices;
        public int GetCurrentCooldown() => Main.LocalPlayer.GetModPlayer<NuminousVoicesPlayer>().voiceCooldown;
        public int GetMaxCooldown() => NuminousVoicesPlayer.MaxCooldown;
        public Texture2D GetIconTexture() => TextureAssets.Item[ModContent.ItemType<FireOfMadness>()].Value;
        public string GetDisplayName() => "Numinous Voices";
        public Color GetCustomColor() => new Color(255, 100, 100);
    }
    public class FacetedCrystalProvider : ICooldownProvider
    {
        public bool IsEquipped() => Main.LocalPlayer.GetModPlayer<FacetedCrystalPlayer>().hasCrystalAccessory;
        public int GetCurrentCooldown() => Main.LocalPlayer.GetModPlayer<FacetedCrystalPlayer>().crystalCooldown;
        public int GetMaxCooldown() => FacetedCrystalPlayer.CooldownTime;
        public Texture2D GetIconTexture() => TextureAssets.Item[ModContent.ItemType<CrystalOfConcetration>()].Value;
        public string GetDisplayName() => "Crystal Of Concetration";
        public Color GetCustomColor() => new Color(255, 100, 100);
    }
    public class SmolderingLotusProvider : ICooldownProvider
    {
        public bool IsEquipped() => Main.LocalPlayer.GetModPlayer<SmolderingLotusPlayer>().hasLotusAccessory;
        public int GetCurrentCooldown() => Main.LocalPlayer.GetModPlayer<SmolderingLotusPlayer>().LotusCooldown;
        public int GetMaxCooldown() => SmolderingLotusPlayer.CooldownTime;
        public Texture2D GetIconTexture() => TextureAssets.Item[ModContent.ItemType<SmolderingLotus>()].Value;
        public string GetDisplayName() => "Smoldering Lotus";
        public Color GetCustomColor() => new Color(255, 100, 100);
    }
    public class BloodImpulseProvider : ICooldownProvider
    {
        public bool IsEquipped() => Main.LocalPlayer.GetModPlayer<CrimsonPulsePlayer>().hasCrimsonPulse;
        public int GetCurrentCooldown() => Main.LocalPlayer.GetModPlayer<CrimsonPulsePlayer>().cooldown;
        public int GetMaxCooldown() => CrimsonPulsePlayer.cooldownMax;
        public Texture2D GetIconTexture() => TextureAssets.Item[ModContent.ItemType<BloodImpulse>()].Value;
        public string GetDisplayName() => "Blood Impulse";
        public Color GetCustomColor() => new Color(255, 100, 100);
    }
    public class ChargerHealthProvider : ICooldownProvider
    {
        public bool IsEquipped() => Main.LocalPlayer.GetModPlayer<jarkos.items.accs.special.ChargerHealthPlayer>().chargerHealthEquipped;
        public int GetCurrentCooldown() => Main.LocalPlayer.GetModPlayer<jarkos.items.accs.special.ChargerHealthPlayer>().chargerHealthCooldown;
        public int GetMaxCooldown() => jarkos.items.accs.special.ChargerHealthPlayer.CooldownTime;
        public Texture2D GetIconTexture() => TextureAssets.Item[ModContent.ItemType<jarkos.items.accs.special.ChargerHealth>()].Value;
        public string GetDisplayName() => "Charger Health";
        public Color GetCustomColor() => new Color(50, 180, 255);
    }
    public class CooldownUIManager : ModSystem
    {
        internal UserInterface cooldownInterface;
        internal CooldownUIState cooldownState;
        private GameTime lastGameTime;
        private List<BaseCooldownUI> uiElements = new List<BaseCooldownUI>();
        private List<ICooldownProvider> providers = new List<ICooldownProvider>();
        private const float BaseSpacing = 10f;
        private const int MaxElementsPerRow = 3;
        public override void Load()
        {
            if (!Main.dedServ)
            {
                providers.Add(new ResonanceBellProvider());
                providers.Add(new HemoStabilizerProvider());
                providers.Add(new NuminousVoicesProvider());
                providers.Add(new FacetedCrystalProvider());
                providers.Add(new SmolderingLotusProvider());
                providers.Add(new BloodImpulseProvider());
                providers.Add(new ChargerHealthProvider());
                cooldownState = new CooldownUIState();
                cooldownState.Activate();
                foreach (var provider in providers)
                {
                    var uiElement = new GenericCooldownUI(provider);
                    uiElements.Add(uiElement);
                    cooldownState.Append(uiElement);
                }
                cooldownInterface = new UserInterface();
                cooldownInterface.SetState(cooldownState);
            }
        }
        public override void Unload()
        {
            cooldownState = null;
            cooldownInterface = null;
            uiElements.Clear();
            providers.Clear();
        }
        public override void UpdateUI(GameTime gameTime)
        {
            lastGameTime = gameTime;
            if (ModContent.GetInstance<Jarkos_config>()?.EnableAccessoryCooldownUI == false)
                return;

            if (cooldownInterface?.CurrentState != null)
            {
                var activeElements = uiElements.Where(e => e.ShouldBeActive()).ToList();
                int activeCount = activeElements.Count;
                if (activeCount > 0)
                {
                    var config = UISizeConfig.GetConfig(activeCount);
                    foreach (var element in uiElements)
                    {
                        element.ApplyConfig(config);
                    }
                    CalculatePositions(activeElements);
                }
                cooldownInterface.Update(gameTime);
            }
        }
        private void CalculatePositions(List<BaseCooldownUI> activeElements)
        {
            int count = activeElements.Count;
            if (count == 1)
            {
                activeElements[0].SetPositionOffset(Vector2.Zero, 0);
            }
            else if (count <= MaxElementsPerRow)
            {
                ArrangeInRow(activeElements, 0);
            }
            else
            {
                int firstRowCount = (count + 1) / 2;
                int secondRowCount = count - firstRowCount;
                var firstRow = activeElements.Take(firstRowCount).ToList();
                var secondRow = activeElements.Skip(firstRowCount).Take(secondRowCount).ToList();
                ArrangeInRow(firstRow, -15f);
                ArrangeInRow(secondRow, 25f);
            }
        }
        private void ArrangeInRow(List<BaseCooldownUI> elements, float verticalOffset)
        {
            int count = elements.Count;
            float totalWidth = elements.Sum(e => e.TotalWidth) + (count - 1) * BaseSpacing;
            float startX = -totalWidth / 2f;
            float currentX = startX;
            foreach (var element in elements)
            {
                float elementWidth = element.TotalWidth;
                float centerOffset = currentX + elementWidth / 2f;
                element.SetPositionOffset(new Vector2(centerOffset, 0), verticalOffset);
                currentX += elementWidth + BaseSpacing;
            }
        }
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            if (ModContent.GetInstance<Jarkos_config>()?.EnableAccessoryCooldownUI == false)
                return;

            int resourceBarIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
            if (resourceBarIndex != -1)
            {
                layers.Insert(resourceBarIndex + 1, new LegacyGameInterfaceLayer("YourMod: Cooldown UI", delegate
                    {
                        if (lastGameTime != null && cooldownInterface?.CurrentState != null)
                        {
                            cooldownInterface.Draw(Main.spriteBatch, lastGameTime);
                        }
                        return true;
                    }, InterfaceScaleType.UI));
            }
        }
    }
    public class GenericCooldownUI : BaseCooldownUI
    {
        public GenericCooldownUI(ICooldownProvider provider) : base(provider) { }
    }
    public class CooldownUIState : UIState
    {
    }
}
