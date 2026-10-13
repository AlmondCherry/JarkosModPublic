using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.ModLoader;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.Audio;
using System;
using jarkos.tiles;
using Terraria.ModLoader.IO;
using Jarkos.UI;
using jarkos.systems;
using Jarkos.items.materials;
using Terraria.Localization;

namespace Jarkos.UI
{
    public class UIParticleArcane
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Color ParticleColor;
        public float Scale;
        public float Rotation;
        public float RotationSpeed;
        public float Alpha;
        public float FadeSpeed;
        public float Lifetime;
        public float MaxLifetime;
        public Texture2D CustomTexture;
        public bool UseMagicPixel;
        public bool UseGlow;
        public float Gravity;
        public bool IsDead => Lifetime >= MaxLifetime || Alpha <= 0;
        public void Update()
        {
            Position += Velocity;
            Velocity.Y += Gravity;
            Velocity *= 0.97f;
            Rotation += RotationSpeed;
            Lifetime++;
            float fadeStart = MaxLifetime * 0.6f;
            if (Lifetime > fadeStart)
            {
                Alpha -= FadeSpeed;
            }
            if (Lifetime > MaxLifetime * 0.5f)
            {
                Scale *= 0.98f;
            }
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            if (IsDead || Alpha <= 0) return;
            Color drawColor = ParticleColor * Math.Max(0, Alpha);
            if (UseMagicPixel)
            {
                Texture2D pixel = TextureAssets.MagicPixel.Value;
                Vector2 origin = new Vector2(0.5f, 0.5f);
                spriteBatch.Draw(pixel, Position, new Rectangle(0, 0, 1, 1), drawColor, Rotation, origin, Scale, SpriteEffects.None, 0f);
                if (UseGlow)
                {
                    Color glowColor = drawColor * 0.3f;
                    spriteBatch.Draw(pixel, Position, new Rectangle(0, 0, 1, 1), glowColor, Rotation, origin, Scale * 2f, SpriteEffects.None, 0f);
                }
            }
            else if (CustomTexture != null)
            {
                Vector2 origin = new Vector2(CustomTexture.Width / 2f, CustomTexture.Height / 2f);
                spriteBatch.Draw(CustomTexture, Position, null, drawColor, Rotation, origin, Scale, SpriteEffects.None, 0f);
                if (UseGlow)
                {
                    Color glowColor = drawColor * 0.4f;
                    spriteBatch.Draw(CustomTexture, Position, null, glowColor, Rotation, origin, Scale * 1.5f, SpriteEffects.None, 0f);
                }
            }
        }
    }
    public class FloatingText
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public string Text;
        public Color TextColor;
        public float Alpha;
        public float Scale;
        public float Lifetime;
        public float MaxLifetime;
        public float Deceleration;
        public bool UseGlow;
        public Color GlowColor;
        public bool IsDead => Lifetime >= MaxLifetime || Alpha <= 0;
        public FloatingText(string text, Vector2 position, Color color, float maxLifetime = 120f)
        {
            Text = text;
            Position = position;
            TextColor = color;
            GlowColor = color;
            Alpha = 1f;
            Scale = 1f;
            Lifetime = 0;
            MaxLifetime = maxLifetime;
            Velocity = new Vector2(0, -3f);
            Deceleration = 0.96f;
            UseGlow = true;
        }
        public void Update()
        {
            Position += Velocity;
            Velocity *= Deceleration;
            Lifetime++;
            float fadeStart = MaxLifetime * 0.4f;
            if (Lifetime > fadeStart)
            {
                float fadeProgress = (Lifetime - fadeStart) / (MaxLifetime - fadeStart);
                Alpha = 1f - fadeProgress;
            }
            if (Lifetime < 15)
            {
                Scale = 1f + Lifetime / 15f * 0.2f;
            }
            else if (Lifetime > MaxLifetime * 0.7f)
            {
                float shrinkProgress = (Lifetime - MaxLifetime * 0.7f) / (MaxLifetime * 0.3f);
                Scale = 1.2f - shrinkProgress * 0.4f;
            }
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            if (IsDead || Alpha <= 0) return;
            Vector2 textSize = FontAssets.MouseText.Value.MeasureString(Text) * Scale;
            Vector2 drawPos = Position - textSize / 2f;
            Color currentColor = TextColor * Alpha;
            if (UseGlow)
            {
                Color glowColor = GlowColor * Alpha * 0.3f;
                for (int i = 0; i < 8; i++)
                {
                    float angle = i * MathHelper.TwoPi / 8f;
                    Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 3f;
                    Terraria.Utils.DrawBorderString(spriteBatch, Text, drawPos + offset, glowColor * 0.5f, Scale);
                }
                for (int i = 0; i < 4; i++)
                {
                    float angle = i * MathHelper.TwoPi / 4f;
                    Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 1.5f;
                    Terraria.Utils.DrawBorderString(spriteBatch, Text, drawPos + offset, glowColor, Scale);
                }
            }
            Terraria.Utils.DrawBorderString(spriteBatch, Text, drawPos, currentColor, Scale);
        }
    }
    public class ArcaneForgeUI : UIState
    {
    private ArcaneUIPanel mainPanel;
    private SkyForgeTE tileEntity;
    private SkyForgeItemSlot materialSlot;
    private SkyForgeItemSlot weaponSlot;
    private SkyForgeItemSlot outputSlot;
    private UIStateBarAcrane damageBar;
    private UIStateBarAcrane speedBar;
    private UIStateBarAcrane critBar;
    private UIStateBarAcrane knockbackBar;
    private UITextPanel<string> calibrateButton;
    private CraftingProgressBar craftingProgressBar;
    private bool isCalibrating = false;
    private List<UIParticleArcane> particles = new List<UIParticleArcane>();
    private Texture2D starTexture;
    private Texture2D sparkTexture;
    private bool needsPositionUpdate = true;
    private Item pendingCalibratedWeapon;
    private int calibrationParticleTimer = 0;
    private List<FloatingText> floatingTexts = new List<FloatingText>();
    public void SetTileEntity(SkyForgeTE te)
    {
        tileEntity = te;
        needsPositionUpdate = true;
        if (te.BonusDamage == 0 && te.BonusSpeed == 0 && te.BonusCrit == 0 && te.BonusKnockback == 0)
        {
            te.RandomizeCalibration();
        }
        UpdateSlots();
        UpdateBars();
        craftingProgressBar?.Reset();
        isCalibrating = false;
        calibrationParticleTimer = 0;
        floatingTexts.Clear();
        Recalculate();
    }
    public override void OnInitialize()
    {
        starTexture = ModContent.Request<Texture2D>("jarkos/textures/StarTexture", AssetRequestMode.ImmediateLoad).Value;
        sparkTexture = ModContent.Request<Texture2D>("jarkos/textures/tooltip_spark", AssetRequestMode.ImmediateLoad).Value;
        mainPanel = new ArcaneUIPanel();
        mainPanel.Width.Set(400f, 0f);
        mainPanel.Height.Set(400f, 0f);
        mainPanel.HAlign = 0.5f;
        mainPanel.VAlign = 0.5f;
        mainPanel.BackgroundColor = new Color(25, 35, 70, 240);
        mainPanel.BorderColor = new Color(80, 130, 220);
        Append(mainPanel);
        UITextPanel<string> closeBtn = new UITextPanel<string>("X", 0.8f, false);
        closeBtn.Width.Set(30f, 0f);
        closeBtn.Height.Set(30f, 0f);
        closeBtn.Left.Set(-35f, 1f);
        closeBtn.Top.Set(5f, 0f);
        closeBtn.BackgroundColor = new Color(120, 40, 40);
        closeBtn.OnLeftClick += (evt, elem) =>
        {
            if (!isCalibrating)
                ModContent.GetInstance<SkyForgeUISystem>().CloseUI();
        };
        closeBtn.OnMouseOver += (evt, elem) => ((UITextPanel<string>)elem).BackgroundColor = new Color(180, 60, 60);
        closeBtn.OnMouseOut += (evt, elem) => ((UITextPanel<string>)elem).BackgroundColor = new Color(120, 40, 40);
        mainPanel.Append(closeBtn);
        UIText materialLabel = new UIText(LangSystem.ArcaneForgeMaterial);
        materialLabel.Left.Set(30f, 0f);
        materialLabel.Top.Set(65f, 0f);
        mainPanel.Append(materialLabel);
        materialSlot = new SkyForgeItemSlot(0);
        materialSlot.Left.Set(30f, 0f);
        materialSlot.Top.Set(90f, 0f);
        materialSlot.OnItemChanged += OnMaterialChanged;
        mainPanel.Append(materialSlot);
        UIText weaponLabel = new UIText(LangSystem.ArcaneForgeWeapon);
        weaponLabel.Left.Set(130f, 0f);
        weaponLabel.Top.Set(65f, 0f);
        mainPanel.Append(weaponLabel);
        weaponSlot = new SkyForgeItemSlot(1);
        weaponSlot.Left.Set(130f, 0f);
        weaponSlot.Top.Set(90f, 0f);
        weaponSlot.OnItemChanged += OnWeaponChanged;
        mainPanel.Append(weaponSlot);
        craftingProgressBar = new CraftingProgressBar();
        craftingProgressBar.Left.Set(195f, 0f);
        craftingProgressBar.Top.Set(100f, 0f);
        craftingProgressBar.OnProgressComplete += OnCraftingComplete;
        mainPanel.Append(craftingProgressBar);
        outputSlot = new SkyForgeItemSlot(2, true);
        outputSlot.Left.Set(280f, 0f);
        outputSlot.Top.Set(90f, 0f);
        outputSlot.OnItemChanged += OnOutputChanged;
        mainPanel.Append(outputSlot);
        UIText statsHeader = new UIText(LangSystem.ArcaneForgeParameters, 0.85f, false);
        statsHeader.HAlign = 0.5f;
        statsHeader.Top.Set(165f, 0f);
        statsHeader.TextColor = new Color(180, 220, 255);
        mainPanel.Append(statsHeader);
        float barY = 190f;
        float barSpacing = 40f;
        damageBar = new UIStateBarAcrane(LangSystem.ArcaneForgeDamage, new Color(255, 80, 80));
        damageBar.Left.Set(30f, 0f);
        damageBar.Top.Set(barY, 0f);
        mainPanel.Append(damageBar);
        speedBar = new UIStateBarAcrane(LangSystem.ArcaneForgeAttackSpeed, new Color(255, 220, 80));
        speedBar.Left.Set(30f, 0f);
        speedBar.Top.Set(barY + barSpacing, 0f);
        mainPanel.Append(speedBar);
        critBar = new UIStateBarAcrane(LangSystem.ArcaneForgeCritChance, new Color(255, 160, 50));
        critBar.Left.Set(30f, 0f);
        critBar.Top.Set(barY + barSpacing * 2, 0f);
        mainPanel.Append(critBar);
        knockbackBar = new UIStateBarAcrane(LangSystem.ArcaneForgeKnockback, new Color(80, 200, 255));
        knockbackBar.Left.Set(30f, 0f);
        knockbackBar.Top.Set(barY + barSpacing * 3, 0f);
        mainPanel.Append(knockbackBar);
        calibrateButton = new UITextPanel<string>(LangSystem.ArcaneForgeAdaptate.ToString(), 1f, false);
        calibrateButton.Width.Set(180f, 0f);
        calibrateButton.Height.Set(45f, 0f);
        calibrateButton.HAlign = 0.5f;
        calibrateButton.Top.Set(345f, 0f);
        calibrateButton.BackgroundColor = new Color(50, 90, 160);
        calibrateButton.BorderColor = new Color(100, 160, 255);
        calibrateButton.OnLeftClick += CalibrateWeapon;
        calibrateButton.OnMouseOver += OnButtonHover;
        calibrateButton.OnMouseOut += OnButtonLeave;
        mainPanel.Append(calibrateButton);
    }
    private void SpawnPrefixFloatingText(string prefix, Color prefixColor)
    {
        Vector2 outputSlotTop = outputSlot.GetDimensions().Position() + new Vector2(26f, -10f);
        FloatingText floatingText = new FloatingText(prefix, outputSlotTop, prefixColor, 90f)
        {
            Velocity = new Vector2(0, -2.5f), Deceleration = 0.94f, Scale = 1.1f, UseGlow = true, GlowColor = prefixColor
        };
        floatingTexts.Add(floatingText);
    }
    private void UpdateFloatingTexts()
    {
        for (int i = floatingTexts.Count - 1; i >= 0; i--)
        {
            floatingTexts[i].Update();
            if (floatingTexts[i].IsDead)
            {
                floatingTexts.RemoveAt(i);
            }
        }
    }
    private void DrawFloatingTexts(SpriteBatch spriteBatch)
    {
        foreach (var text in floatingTexts)
        {
            text.Draw(spriteBatch);
        }
    }
    private void SpawnCalibrationProcessParticles()
    {
        Vector2 outputSlotCenter = outputSlot.GetDimensions().Position() + new Vector2(26f, 26f);
        Color[] particleColors = new Color[]
        {
            new Color(100, 180, 255), new Color(150, 200, 255), new Color(200, 150, 255), new Color(255, 200, 100), new Color(180, 255, 200), };
        int particleCount = Main.rand.Next(2, 5);
        for (int i = 0; i < particleCount; i++)
        {
            float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
            float distance = Main.rand.NextFloat(80f, 150f);
            Vector2 spawnOffset = new Vector2((float)Math.Cos(angle) * distance, (float)Math.Sin(angle) * distance);
            Vector2 spawnPosition = outputSlotCenter + spawnOffset;
            Vector2 directionToCenter = Vector2.Normalize(outputSlotCenter - spawnPosition);
            float speed = Main.rand.NextFloat(3f, 7f);
            UIParticleArcane particle = new UIParticleArcane
            {
                Position = spawnPosition, Velocity = directionToCenter * speed, ParticleColor = particleColors[Main.rand.Next(particleColors.Length)], Scale = Main.rand.NextFloat(3f, 7f), Rotation = angle, RotationSpeed = Main.rand.NextFloat(-0.1f, 0.1f), Alpha = 1f, FadeSpeed = Main.rand.NextFloat(0.01f, 0.02f), Lifetime = 0, MaxLifetime = Main.rand.NextFloat(40f, 60f), UseMagicPixel = true, UseGlow = true, Gravity = 0f
            };
            particles.Add(particle);
        }
    }
    private void SpawnSpiralParticles()
    {
        Vector2 outputSlotCenter = outputSlot.GetDimensions().Position() + new Vector2(26f, 26f);
        Color[] particleColors = new Color[]
        {
            new Color(255, 220, 100), new Color(255, 180, 80), new Color(200, 150, 255), };
        float baseAngle = Main.GameUpdateCount * 0.1f;
        for (int i = 0; i < 2; i++)
        {
            float angle = baseAngle + i * MathHelper.Pi;
            float distance = Main.rand.NextFloat(60f, 100f);
            Vector2 spawnOffset = new Vector2((float)Math.Cos(angle) * distance, (float)Math.Sin(angle) * distance);
            Vector2 spawnPosition = outputSlotCenter + spawnOffset;
            Vector2 directionToCenter = Vector2.Normalize(outputSlotCenter - spawnPosition);
            Vector2 perpendicular = new Vector2(-directionToCenter.Y, directionToCenter.X);
            float speed = Main.rand.NextFloat(4f, 6f);
            float spiralStrength = 0.3f;
            Vector2 velocity = directionToCenter * speed + perpendicular * speed * spiralStrength;

            UIParticleArcane particle = new UIParticleArcane
            {
                Position = spawnPosition, Velocity = velocity, ParticleColor = particleColors[Main.rand.Next(particleColors.Length)], Scale = Main.rand.NextFloat(4f, 8f), Rotation = Main.rand.NextFloat() * MathHelper.TwoPi, RotationSpeed = Main.rand.NextFloat(0.1f, 0.2f), Alpha = 1f, FadeSpeed = 0.015f, Lifetime = 0, MaxLifetime = Main.rand.NextFloat(35f, 50f), UseMagicPixel = true, UseGlow = true, Gravity = 0f
            };
            particles.Add(particle);
        }
    }
    private void SpawnSlotToSlotParticles()
    {
        Vector2 outputSlotCenter = outputSlot.GetDimensions().Position() + new Vector2(26f, 26f);
        Vector2 materialSlotCenter = materialSlot.GetDimensions().Position() + new Vector2(26f, 26f);
        Vector2 weaponSlotCenter = weaponSlot.GetDimensions().Position() + new Vector2(26f, 26f);
        if (Main.rand.NextBool(2))
        {
            Vector2 spawnPos = materialSlotCenter + Main.rand.NextVector2Circular(15f, 15f);
            Vector2 direction = Vector2.Normalize(outputSlotCenter - spawnPos);
            float speed = Main.rand.NextFloat(4f, 8f);
            UIParticleArcane particle = new UIParticleArcane
            {
                Position = spawnPos, Velocity = direction * speed, ParticleColor = new Color(255, 200, 80), Scale = Main.rand.NextFloat(4f, 7f), Rotation = 0f, RotationSpeed = Main.rand.NextFloat(-0.1f, 0.1f), Alpha = 1f, FadeSpeed = 0.02f, Lifetime = 0, MaxLifetime = 40f, UseMagicPixel = true, UseGlow = true, Gravity = 0f
            };
            particles.Add(particle);
        }
        if (Main.rand.NextBool(2))
        {
            Vector2 spawnPos = weaponSlotCenter + Main.rand.NextVector2Circular(15f, 15f);
            Vector2 direction = Vector2.Normalize(outputSlotCenter - spawnPos);
            float speed = Main.rand.NextFloat(4f, 8f);
            UIParticleArcane particle = new UIParticleArcane
            {
                Position = spawnPos, Velocity = direction * speed, ParticleColor = new Color(100, 180, 255), Scale = Main.rand.NextFloat(4f, 7f), Rotation = 0f, RotationSpeed = Main.rand.NextFloat(-0.1f, 0.1f), Alpha = 1f, FadeSpeed = 0.02f, Lifetime = 0, MaxLifetime = 40f, UseMagicPixel = true, UseGlow = true, Gravity = 0f
            };
            particles.Add(particle);
        }
    }
    private void OnCraftingComplete()
    {
        if (pendingCalibratedWeapon == null || tileEntity == null)
        {
            isCalibrating = false;
            return;
        }
        outputSlot.StoredItem = pendingCalibratedWeapon;
        weaponSlot.StoredItem.TurnToAir();
        tileEntity.MaterialSlot = materialSlot.StoredItem.Clone();
        tileEntity.WeaponSlot = new Item();
        tileEntity.OutputSlot = outputSlot.StoredItem.Clone();
        SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/CraftAdaptate") with { Pitch = Main.rand.NextFloat(-0.2f, 0.2f)});
        SpawnCalibrationParticles();
        float totalBonus = tileEntity.BonusDamage + tileEntity.BonusSpeed + tileEntity.BonusCrit + tileEntity.BonusKnockback;
        string prefix = GetCalibrationPrefix(totalBonus);
        Color prefixColor = GetPrefixColor(totalBonus);
        SpawnPrefixFloatingText(prefix, prefixColor);
        tileEntity.RandomizeCalibration();
        UpdateBars();
        pendingCalibratedWeapon = null;
        isCalibrating = false;
        calibrationParticleTimer = 0;
        calibrateButton.SetText(LangSystem.ArcaneForgeAdaptate.ToString());
        calibrateButton.BackgroundColor = new Color(50, 90, 160);
    }
    private void CalibrateWeapon(UIMouseEvent evt, UIElement listeningElement)
    {
        if (tileEntity == null) return;
        if (isCalibrating)
        {
            return;
        }
        if (materialSlot.StoredItem.IsAir || materialSlot.StoredItem.type != ModContent.ItemType<CosmicFlame>())
        {
            SoundEngine.PlaySound(SoundID.MenuClose);
            return;
        }
        if (weaponSlot.StoredItem.IsAir || !IsValidWeapon(weaponSlot.StoredItem))
        {
            SoundEngine.PlaySound(SoundID.MenuClose);
            return;
        }
        if (!outputSlot.StoredItem.IsAir)
        {
            SoundEngine.PlaySound(SoundID.MenuClose);
            return;
        }
        materialSlot.StoredItem.stack--;
        if (materialSlot.StoredItem.stack <= 0)
            materialSlot.StoredItem.TurnToAir();
        pendingCalibratedWeapon = weaponSlot.StoredItem.Clone();
        CalibratedWeaponGlobalItem globalItem = pendingCalibratedWeapon.GetGlobalItem<CalibratedWeaponGlobalItem>();
        globalItem.IsCalibrated = true;
        globalItem.CalibrationDamage = tileEntity.BonusDamage;
        globalItem.CalibrationSpeed = tileEntity.BonusSpeed;
        globalItem.CalibrationCrit = tileEntity.BonusCrit;
        globalItem.CalibrationKnockback = tileEntity.BonusKnockback;
        float totalBonus = tileEntity.BonusDamage + tileEntity.BonusSpeed + tileEntity.BonusCrit + tileEntity.BonusKnockback;
        string prefix = GetCalibrationPrefix(totalBonus);
        globalItem.CalibrationPrefix = prefix;
        isCalibrating = true;
        calibrationParticleTimer = 0;
        craftingProgressBar.StartProgress(2.5f);
        calibrateButton.SetText(LangSystem.ArcaneForgeAdaptate.ToString());
        calibrateButton.BackgroundColor = new Color(80, 80, 80);
        SoundEngine.PlaySound(SoundID.Item4);
    }
    private void SpawnCalibrationParticles()
    {
        Vector2 outputSlotCenter = outputSlot.GetDimensions().Position() + new Vector2(26f, 26f);
        Color[] particleColors = new Color[]        {
            new Color(100, 180, 255), new Color(150, 200, 255), new Color(200, 150, 255), new Color(255, 200, 100), new Color(180, 255, 200), new Color(255, 255, 255), };
        SpawnStarParticles(outputSlotCenter, particleColors, 12);
        SpawnMagicPixelParticles(outputSlotCenter, particleColors, 30);
        SpawnRingParticles(outputSlotCenter, particleColors, 16);
        SpawnSparkParticles(outputSlotCenter, particleColors, 20);
    }
    private void SpawnStarParticles(Vector2 center, Color[] colors, int count)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
            float speed = Main.rand.NextFloat(1f, 4f);
            float distance = Main.rand.NextFloat(0f, 15f);
            Vector2 offset = new Vector2((float)Math.Cos(angle) * distance, (float)Math.Sin(angle) * distance);
            UIParticleArcane particle = new UIParticleArcane
            {
                Position = center + offset, Velocity = new Vector2((float)Math.Cos(angle) * speed, (float)Math.Sin(angle) * speed), ParticleColor = colors[Main.rand.Next(colors.Length)], Scale = Main.rand.NextFloat(0.03f, 0.3f), Rotation = Main.rand.NextFloat() * MathHelper.TwoPi, RotationSpeed = Main.rand.NextFloat(-0.15f, 0.15f), Alpha = 1f, FadeSpeed = Main.rand.NextFloat(0.015f, 0.03f), Lifetime = 0, MaxLifetime = Main.rand.NextFloat(40f, 70f), CustomTexture = starTexture, UseMagicPixel = false, UseGlow = true, Gravity = Main.rand.NextFloat(-0.02f, 0.02f)
            };
            particles.Add(particle);
        }
    }
    private void SpawnMagicPixelParticles(Vector2 center, Color[] colors, int count)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
            float speed = Main.rand.NextFloat(2f, 8f);
            UIParticleArcane particle = new UIParticleArcane
            {
                Position = center + Main.rand.NextVector2Circular(5f, 5f), Velocity = new Vector2((float)Math.Cos(angle) * speed, (float)Math.Sin(angle) * speed), ParticleColor = colors[Main.rand.Next(colors.Length)], Scale = Main.rand.NextFloat(4f, 8f), Rotation = Main.rand.NextFloat() * MathHelper.TwoPi, RotationSpeed = Main.rand.NextFloat(-0.2f, 0.2f), Alpha = 1f, FadeSpeed = Main.rand.NextFloat(0.02f, 0.04f), Lifetime = 0, MaxLifetime = Main.rand.NextFloat(30f, 60f), UseMagicPixel = true, UseGlow = true, Gravity = Main.rand.NextFloat(0.05f, 0.15f)
            };
            particles.Add(particle);
        }
    }
    private void SpawnRingParticles(Vector2 center, Color[] colors, int count)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = (float)i / count * MathHelper.TwoPi;
            float speed = Main.rand.NextFloat(3f, 5f);
            UIParticleArcane particle = new UIParticleArcane
            {
                Position = center, Velocity = new Vector2((float)Math.Cos(angle) * speed, (float)Math.Sin(angle) * speed), ParticleColor = colors[Main.rand.Next(colors.Length)], Scale = Main.rand.NextFloat(4f, 8f), Rotation = angle, RotationSpeed = 0f, Alpha = 1f, FadeSpeed = 0.025f, Lifetime = 0, MaxLifetime = 50f, UseMagicPixel = true, UseGlow = true, Gravity = 0f
            };
            particles.Add(particle);
        }
    }
    private void SpawnSparkParticles(Vector2 center, Color[] colors, int count)
    {
        for (int i = 0; i < count; i++)
        {
            UIParticleArcane particle = new UIParticleArcane
            {
                Position = center + Main.rand.NextVector2Circular(20f, 10f), Velocity = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-5f, -2f)), ParticleColor = new Color(255, Main.rand.Next(150, 255), Main.rand.Next(50, 150)), Scale = Main.rand.NextFloat(4f, 8f), Rotation = 0f, RotationSpeed = Main.rand.NextFloat(-0.1f, 0.1f), Alpha = 1f, FadeSpeed = Main.rand.NextFloat(0.03f, 0.05f), Lifetime = 0, MaxLifetime = Main.rand.NextFloat(25f, 45f), UseMagicPixel = true, UseGlow = false, Gravity = -0.05f
            };
            particles.Add(particle);
        }
    }
    private void UpdateParticles()
    {
        for (int i = particles.Count - 1; i >= 0; i--)
        {
            particles[i].Update();
            if (particles[i].IsDead)
            {
                particles.RemoveAt(i);
            }
        }
    }
    private void DrawParticles(SpriteBatch spriteBatch)
    {
        foreach (var particle in particles)
        {
            particle.Draw(spriteBatch);
        }
    }
    private void OnButtonHover(UIMouseEvent evt, UIElement listeningElement)
    {
        if (isCalibrating) return;
        UITextPanel<string> button = (UITextPanel<string>)listeningElement;
        button.BackgroundColor = new Color(70, 120, 200);
        button.BorderColor = new Color(150, 200, 255);
        SoundEngine.PlaySound(SoundID.MenuTick);
    }
    private void OnButtonLeave(UIMouseEvent evt, UIElement listeningElement)
    {
        if (isCalibrating) return;
        UITextPanel<string> button = (UITextPanel<string>)listeningElement;
        button.BackgroundColor = new Color(50, 90, 160);
        button.BorderColor = new Color(100, 160, 255);
    }
    private void OnMaterialChanged()
    {
        if (tileEntity != null && !isCalibrating)
        {
            tileEntity.MaterialSlot = materialSlot.StoredItem.Clone();
        }
    }
    private void OnWeaponChanged()
    {
        if (tileEntity != null && !isCalibrating)
        {
            tileEntity.WeaponSlot = weaponSlot.StoredItem.Clone();
        }
    }
    private void OnOutputChanged()
    {
        if (tileEntity != null)
        {
            tileEntity.OutputSlot = outputSlot.StoredItem.Clone();
            if (outputSlot.StoredItem.IsAir)
            {
                craftingProgressBar.Reset();
            }
        }
    }
    private bool IsValidWeapon(Item item)
    {
        return item.damage > 0 && (item.CountsAsClass(DamageClass.Melee) || item.CountsAsClass(DamageClass.Ranged) || item.CountsAsClass(DamageClass.Magic) || item.CountsAsClass(DamageClass.Summon) || item.CountsAsClass(DamageClass.Throwing));
    }
    private string GetCalibrationPrefix(float totalBonus)
    {
        if (totalBonus >= 0.6f) return LangSystem.ArcaneForgePrefixFog.ToString();
        if (totalBonus >= 0.45f) return LangSystem.ArcaneForgePrefixEther.ToString();
        if (totalBonus >= 0.3f) return LangSystem.ArcaneForgePrefixStar.ToString();
        if (totalBonus >= 0.15f) return LangSystem.ArcaneForgePrefixHeavenly.ToString();
        return LangSystem.ArcaneForgePrefixGodly.ToString();
    }
    private Color GetPrefixColor(float totalBonus)
    {
        if (totalBonus >= 0.6f) return new Color(255, 215, 0);
        if (totalBonus >= 0.45f) return new Color(150, 200, 255);
        if (totalBonus >= 0.3f) return new Color(200, 150, 255);
        if (totalBonus >= 0.15f) return new Color(180, 220, 255);
        return new Color(150, 150, 180);
    }
    private void UpdateSlots()
    {
        if (tileEntity == null) return;
        materialSlot.StoredItem = tileEntity.MaterialSlot?.Clone() ?? new Item();
        weaponSlot.StoredItem = tileEntity.WeaponSlot?.Clone() ?? new Item();
        outputSlot.StoredItem = tileEntity.OutputSlot?.Clone() ?? new Item();
    }
    private void UpdateBars()
    {
        if (tileEntity == null) return;
        damageBar.SetValue(tileEntity.BonusDamage);
        speedBar.SetValue(tileEntity.BonusSpeed);
        critBar.SetValue(tileEntity.BonusCrit);
        knockbackBar.SetValue(tileEntity.BonusKnockback);
    }
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        UpdateParticles();
        UpdateFloatingTexts();
        if (isCalibrating && craftingProgressBar.IsAnimating)
        {
            calibrationParticleTimer++;
            int spawnInterval = Math.Max(2, 8 - (int)(craftingProgressBar.Progress * 6));
            if (calibrationParticleTimer % spawnInterval == 0)
            {
                SpawnCalibrationProcessParticles();
            }
            if (calibrationParticleTimer % 10 == 0)
            {
                SpawnSpiralParticles();
            }
            if (calibrationParticleTimer % 6 == 0)
            {
                SpawnSlotToSlotParticles();
            }
        }
        if (tileEntity != null)
        {
            Vector2 tePos = new Vector2(tileEntity.Position.X * 16 + 24, tileEntity.Position.Y * 16 + 16);
            if (Vector2.Distance(Main.LocalPlayer.Center, tePos) > 200f)
            {
                if (isCalibrating)
                {
                    isCalibrating = false;
                    craftingProgressBar.Reset();
                    calibrateButton.SetText("Calibrate");
                    calibrateButton.BackgroundColor = new Color(50, 90, 160);
                    calibrationParticleTimer = 0;
                    if (materialSlot.StoredItem.IsAir)
                    {
                        materialSlot.StoredItem = new Item(ModContent.ItemType<CosmicFlame>(), 1);
                    }
                    else
                    {
                        materialSlot.StoredItem.stack++;
                    }
                    pendingCalibratedWeapon = null;
                }
                SkyForgeUISystem.Instance.CloseUI();
            }
        }
    }
    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
        DrawParticles(spriteBatch);
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
        DrawFloatingTexts(spriteBatch);
    }
    public void SaveSlotsToTE()
    {
        if (tileEntity == null) return;
        tileEntity.MaterialSlot = materialSlot.StoredItem.Clone();
        tileEntity.WeaponSlot = weaponSlot.StoredItem.Clone();
        tileEntity.OutputSlot = outputSlot.StoredItem.Clone();
    }
}
    public class SkyForgeItemSlot : UIElement
    {
        public Item StoredItem = new Item();
        private int slotType;
        private bool isOutputOnly;
        public event Action OnItemChanged;
        private int animationFrame = 0;
        private int animationTimer = 0;
        private const int ANIMATION_SPEED = 6;
        private const int TOTAL_FRAMES = 4;
        public SkyForgeItemSlot(int type, bool outputOnly = false)
        {
            slotType = type;
            isOutputOnly = outputOnly;
            Width.Set(52f, 0f);
            Height.Set(52f, 0f);
        }
        private bool IsAnimatedItem(Texture2D texture)
        {
            return texture.Height > texture.Width * 3;
        }
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (slotType == 0 && !StoredItem.IsAir)
        {
            animationTimer++;
            if (animationTimer >= ANIMATION_SPEED)
            {
                animationTimer = 0;
                animationFrame++;
                if (animationFrame >= TOTAL_FRAMES)
                {
                    animationFrame = 0;
                }
            }
        }
    }
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        Vector2 position = GetDimensions().Position();
        Texture2D slotTexture = TextureAssets.InventoryBack.Value;
        Color slotColor = slotType switch
        {
            0 => new Color(255, 180, 80), 1 => new Color(80, 180, 255), 2 => new Color(80, 255, 120), _ => Color.White
        };
        spriteBatch.Draw(slotTexture, position, null, slotColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        if (!StoredItem.IsAir)
        {
            Main.instance.LoadItem(StoredItem.type);
            Texture2D itemTexture = TextureAssets.Item[StoredItem.type].Value;
            Vector2 itemPos = position + new Vector2(26f, 26f);
            if (slotType == 0 && IsAnimatedItem(itemTexture))
            {
                DrawAnimatedItem(spriteBatch, itemTexture, itemPos);
            }
            else
            {
                DrawRegularItem(spriteBatch, itemTexture, itemPos);
            }
            if (StoredItem.stack > 1)
            {
                Terraria.Utils.DrawBorderString(spriteBatch, StoredItem.stack.ToString(), position + new Vector2(10f, 36f), Color.White, 0.8f);
            }
        }
    }
    private void DrawAnimatedItem(SpriteBatch spriteBatch, Texture2D itemTexture, Vector2 itemPos)
    {
        int frameWidth = itemTexture.Width;
        int frameHeight = itemTexture.Height / TOTAL_FRAMES;
        Rectangle sourceRect = new Rectangle(0, animationFrame * frameHeight, frameWidth, frameHeight);
        float scale = 1f;
        int maxSize = Math.Max(frameWidth, frameHeight);
        if (maxSize > 32)
        {
            scale = 32f / maxSize;
        }
        Vector2 origin = new Vector2(frameWidth / 2f, frameHeight / 2f);
        spriteBatch.Draw(itemTexture, itemPos, sourceRect, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
    }
    private void DrawRegularItem(SpriteBatch spriteBatch, Texture2D itemTexture, Vector2 itemPos)
    {
        float scale = 1f;
        if (itemTexture.Width > 32 || itemTexture.Height > 32)
        {
            scale = 32f / Math.Max(itemTexture.Width, itemTexture.Height);
        }
        spriteBatch.Draw(itemTexture, itemPos, null, Color.White, 0f, new Vector2(itemTexture.Width / 2f, itemTexture.Height / 2f), scale, SpriteEffects.None, 0f);
    }
    public override void LeftClick(UIMouseEvent evt)
    {
        Player player = Main.LocalPlayer;
        Item mouseItem = Main.mouseItem;
        if (isOutputOnly)
        {
            if (!StoredItem.IsAir && mouseItem.IsAir)
            {
                Main.mouseItem = StoredItem.Clone();
                StoredItem.TurnToAir();
                SoundEngine.PlaySound(SoundID.Grab);
                OnItemChanged?.Invoke();
            }
            return;
        }
        if (!mouseItem.IsAir && !IsValidForSlot(mouseItem))
        {
            return;
        }
        Item temp = StoredItem.Clone();
        StoredItem = mouseItem.Clone();
        Main.mouseItem = temp;
        SoundEngine.PlaySound(SoundID.Grab);
        OnItemChanged?.Invoke();
    }
    public override void RightClick(UIMouseEvent evt)
    {
        if (isOutputOnly)
        {
            if (!StoredItem.IsAir && Main.mouseItem.IsAir)
            {
                Main.mouseItem = StoredItem.Clone();
                StoredItem.TurnToAir();
                SoundEngine.PlaySound(SoundID.Grab);
                OnItemChanged?.Invoke();
            }
            return;
        }
        if (!StoredItem.IsAir && Main.mouseItem.IsAir)
        {
            int half = (StoredItem.stack + 1) / 2;
            Main.mouseItem = StoredItem.Clone();
            Main.mouseItem.stack = half;
            StoredItem.stack -= half;
            if (StoredItem.stack <= 0)
                StoredItem.TurnToAir();
            SoundEngine.PlaySound(SoundID.Grab);
            OnItemChanged?.Invoke();
        }
        else if (!Main.mouseItem.IsAir && IsValidForSlot(Main.mouseItem))
        {
            if (StoredItem.IsAir)
            {
                StoredItem = Main.mouseItem.Clone();
                StoredItem.stack = 1;
                Main.mouseItem.stack--;
                if (Main.mouseItem.stack <= 0)
                    Main.mouseItem.TurnToAir();
            }
            else if (StoredItem.type == Main.mouseItem.type && StoredItem.stack < StoredItem.maxStack)
            {
                StoredItem.stack++;
                Main.mouseItem.stack--;
                if (Main.mouseItem.stack <= 0)
                    Main.mouseItem.TurnToAir();
            }
            SoundEngine.PlaySound(SoundID.Grab);
            OnItemChanged?.Invoke();
        }
    }
    private bool IsValidForSlot(Item item)
    {
        return slotType switch
        {
            0 => item.type == ModContent.ItemType<CosmicFlame>(), 1 => item.damage > 0 && (item.CountsAsClass(DamageClass.Melee) || item.CountsAsClass(DamageClass.Ranged) || item.CountsAsClass(DamageClass.Magic) || item.CountsAsClass(DamageClass.Summon) || item.CountsAsClass(DamageClass.Throwing)), _ => false
        };
    }
    public override void MouseOver(UIMouseEvent evt)
    {
        base.MouseOver(evt);
        if (!StoredItem.IsAir)
        {
            Main.hoverItemName = StoredItem.Name;
            Main.HoverItem = StoredItem.Clone();
        }
        else
        {
            string hint = slotType switch
            {
                0 => "Heavenly flame", 1 => "Any weapon", 2 => "Result", _ => ""
            };
            Main.hoverItemName = $"[Slot: {hint}]";
        }
    }
}
    public static class UIDrawHelper
    {
        public static void DrawRect(SpriteBatch spriteBatch, Rectangle rect, Color color)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, 1), color);
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y + rect.Height - 1, rect.Width, 1), color);
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, 1, rect.Height), color);
            spriteBatch.Draw(pixel, new Rectangle(rect.X + rect.Width - 1, rect.Y, 1, rect.Height), color);
        }
    }
    }
    public static class Utils
    {
        public static void DrawRect(SpriteBatch spriteBatch, Rectangle rect, Color color)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, 1), color);
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y + rect.Height - 1, rect.Width, 1), color);
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, 1, rect.Height), color);
            spriteBatch.Draw(pixel, new Rectangle(rect.X + rect.Width - 1, rect.Y, 1, rect.Height), color);
        }
    }
    public class SkyForgeUISystem : ModSystem
    {
        public static SkyForgeUISystem Instance { get; private set; }
        private UserInterface skyForgeInterface;
        private ArcaneForgeUI skyForgeState;
        private bool isUIOpen = false;
        public override void Load()
        {
            Instance = this;
            if (!Main.dedServ)
            {
                skyForgeInterface = new UserInterface();
                skyForgeState = new ArcaneForgeUI();
                skyForgeState.Activate();
            }
        }
        public override void Unload()
        {
            Instance = null;
            skyForgeState = null;
            skyForgeInterface = null;
        }
        public void ToggleUI(SkyForgeTE te)
        {
            if (isUIOpen)
            {
                CloseUI();
            }
            else
            {
                OpenUI(te);
            }
        }
        public void OpenUI(SkyForgeTE te)
        {
            if (skyForgeInterface == null || skyForgeState == null)
            {
                return;
            }
            skyForgeState.SetTileEntity(te);
            skyForgeInterface.SetState(skyForgeState);
            isUIOpen = true;
            Main.playerInventory = true;
        }
        public void CloseUI()
        {
            if (skyForgeState != null)
            {
                skyForgeState.SaveSlotsToTE();
            }
            if (skyForgeInterface != null)
            {
                skyForgeInterface.SetState(null);
            }
            isUIOpen = false;
        }
        public override void UpdateUI(GameTime gameTime)
        {
            if (isUIOpen && skyForgeInterface?.CurrentState != null)
            {
                skyForgeInterface.Update(gameTime);
            }
            if (isUIOpen && !Main.playerInventory)
            {
                CloseUI();
            }
        }
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
            if (inventoryIndex != -1)
            {
                layers.Insert(inventoryIndex + 1, new LegacyGameInterfaceLayer("SkyForgeMod: SkyForge UI", delegate
                    {
                        if (isUIOpen && skyForgeInterface?.CurrentState != null)
                        {
                            skyForgeInterface.Draw(Main.spriteBatch, new GameTime());
                        }
                        return true;
                    }, InterfaceScaleType.UI));
            }
        }
    }
    #region CraftingBar
    public class CraftingProgressBar : UIElement
    {
    private float progress = 0f;
    private bool isAnimating = false;
    private float animationSpeed;
    private float shakeOffset = 0f;
    private float shakeTimer = 0f;
    public bool IsComplete => progress >= 1f;
    public bool IsAnimating => isAnimating;
    public float Progress => progress;
    public event Action OnProgressComplete;
    public CraftingProgressBar()
    {
        Width.Set(70f, 0f);
        Height.Set(24f, 0f);
    }
    public void StartProgress(float durationInSeconds = 0.35f)
    {
        SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/CraftAdaptateWorking") with { Pitch = Main.rand.NextFloat(-0.2f, 0.2f)});
        progress = 0f;
        isAnimating = true;
        animationSpeed = 1f / (durationInSeconds * 60f);
        shakeTimer = 0f;
    }
    public void Reset()
    {
        progress = 0f;
        isAnimating = false;
        shakeOffset = 0f;
        shakeTimer = 0f;
    }
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (isAnimating)
        {
            progress += animationSpeed * 2.5f;
            shakeTimer += 0.15f;
            float shakeIntensity = progress * 2f;
            shakeOffset = (float)Math.Sin(shakeTimer * 8f) * shakeIntensity;
            shakeOffset += Main.rand.NextFloat(-shakeIntensity * 0.5f, shakeIntensity * 0.5f);
            if (progress >= 1f)
            {
                progress = 1f;
                isAnimating = false;
                shakeOffset = 0f;
                OnProgressComplete?.Invoke();
            }
        }
    }
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        Vector2 position = GetDimensions().Position();
        float width = GetDimensions().Width;
        float height = GetDimensions().Height;
        Vector2 drawPos = position;
        drawPos.X += shakeOffset * 0.3f;
        drawPos.Y += shakeOffset;

        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Rectangle bgRect = new Rectangle((int)drawPos.X, (int)drawPos.Y, (int)width, (int)height);
        spriteBatch.Draw(pixel, bgRect, new Color(20, 25, 45, 230));
        Rectangle innerRect = new Rectangle((int)drawPos.X + 3, (int)drawPos.Y + 3, (int)width - 6, (int)height - 6);
        spriteBatch.Draw(pixel, innerRect, new Color(10, 15, 30, 200));

        if (progress > 0)
        {
            int fillWidth = (int)((width - 6) * progress);
            Color fillColor;
            if (progress < 0.5f)
            {
                fillColor = Color.Lerp(new Color(80, 150, 255), new Color(80, 255, 180), progress * 2f);
            }
            else
            {
                fillColor = Color.Lerp(new Color(80, 255, 180), new Color(255, 220, 100), (progress - 0.5f) * 2f);
            }
            if (isAnimating)
            {
                float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.4f) * 0.15f + 0.85f;
                fillColor = new Color((int)(fillColor.R * pulse), (int)(fillColor.G * pulse), (int)(fillColor.B * pulse), fillColor.A);
            }
            Rectangle fillRect = new Rectangle((int)drawPos.X + 3, (int)drawPos.Y + 3, fillWidth, (int)height - 6);
            spriteBatch.Draw(pixel, fillRect, fillColor);
            Rectangle glowRect = new Rectangle((int)drawPos.X + 3, (int)drawPos.Y + 3, fillWidth, ((int)height - 6) / 3);
            spriteBatch.Draw(pixel, glowRect, fillColor * 0.5f);
        }
        DrawBorder(spriteBatch, drawPos, width, height, isAnimating ? new Color(150, 200, 255) : new Color(80, 130, 220));
    }
    private void DrawBorder(SpriteBatch spriteBatch, Vector2 pos, float width, float height, Color color)
    {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        int w = (int)width;
        int h = (int)height;
        int x = (int)pos.X;
        int y = (int)pos.Y;
        int thickness = 2;

        spriteBatch.Draw(pixel, new Rectangle(x, y, w, thickness), color);
        spriteBatch.Draw(pixel, new Rectangle(x, y + h - thickness, w, thickness), color);
        spriteBatch.Draw(pixel, new Rectangle(x, y, thickness, h), color);
        spriteBatch.Draw(pixel, new Rectangle(x + w - thickness, y, thickness, h), color);
    }
    }
    #endregion
    #region BarFilling
    public class UIStateBarAcrane : UIElement
    {
    private LocalizedText label;
    private Color barColor;
    private float targetValue = 0f;
    private float displayValue = 0f;
    private float animationSpeed = 8f;
    private Asset<Texture2D> barFillTexture;
    private Asset<Texture2D> barFrameTexture;
    private bool useCustomTextures = false;
    private const int BAR_WIDTH = 160;
    private const int BAR_HEIGHT = 20;
    public UIStateBarAcrane(LocalizedText labelText, Color color)
    {
        label = labelText;
        barColor = color;
        Width.Set(340f, 0f);
        Height.Set(35f, 0f);
        if (ModContent.HasAsset("jarkos/UI/ArcaneBarFill"))
        {
            barFillTexture = ModContent.Request<Texture2D>("jarkos/UI/ArcaneBarFill");
            useCustomTextures = true;
        }
        if (ModContent.HasAsset("jarkos/UI/ArcaneBar"))
        {
            barFrameTexture = ModContent.Request<Texture2D>("jarkos/UI/ArcaneBar");
        }
    }
    public void SetValue(float val)
    {
        targetValue = MathHelper.Clamp(val, 0f, 0.2f);
    }
    public void SetValueInstant(float val)
    {
        targetValue = MathHelper.Clamp(val, 0f, 0.2f);
        displayValue = targetValue;
    }
    public void SetAnimationSpeed(float speed)
    {
        animationSpeed = MathHelper.Clamp(speed, 1f, 50f);
    }
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (Math.Abs(displayValue - targetValue) > 0.0001f)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float lerpFactor = 1f - (float)Math.Pow(0.5f, deltaTime * animationSpeed);
            displayValue = MathHelper.Lerp(displayValue, targetValue, lerpFactor);
            if (Math.Abs(displayValue - targetValue) < 0.0001f)
            {
                displayValue = targetValue;
            }
        }
    }
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        Vector2 position = GetDimensions().Position();
        Terraria.Utils.DrawBorderString(spriteBatch, label.ToString(), position, Color.White, 0.85f);
        Vector2 barPos = position + new Vector2(120f, 2f);
        Rectangle barBg = new Rectangle((int)barPos.X, (int)barPos.Y, BAR_WIDTH, BAR_HEIGHT);

        spriteBatch.Draw(TextureAssets.MagicPixel.Value, barBg, new Color(20, 20, 30, 220));
        float fillPercent = displayValue / 0.2f;
        int fillWidth = (int)(BAR_WIDTH * fillPercent);

        if (barFrameTexture != null && barFrameTexture.IsLoaded)
        {
            Texture2D frameTex = barFrameTexture.Value;
            Rectangle frameRect = new Rectangle((int)barPos.X - 2, (int)barPos.Y - 2, BAR_WIDTH + 4, BAR_HEIGHT + 4);
            spriteBatch.Draw(frameTex, frameRect, Color.White);
        }
        else
        {
            DrawFrame(spriteBatch, barPos);
        }
        if (fillWidth > 0)
        {
            if (useCustomTextures && barFillTexture != null && barFillTexture.IsLoaded)
            {
                Texture2D fillTex = barFillTexture.Value;
                int sourceWidth = (int)(fillTex.Width * fillPercent);
                Rectangle sourceRect = new Rectangle(0, 0, sourceWidth, fillTex.Height);
                Rectangle destRect = new Rectangle((int)barPos.X, (int)barPos.Y, sourceWidth, BAR_HEIGHT);
                if (fillTex.Height != BAR_HEIGHT)
                {
                    destRect = new Rectangle((int)barPos.X, (int)barPos.Y, sourceWidth, BAR_HEIGHT);
                }
                spriteBatch.Draw(fillTex, destRect, sourceRect, barColor * 0.9f);
            }
            else
            {
                DrawGradientFill(spriteBatch, barPos, fillWidth);
            }
            Rectangle glowRect = new Rectangle((int)barPos.X, (int)barPos.Y, fillWidth, 3);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, glowRect, Color.White * 0.3f);
        }
        string percentText = $"+{displayValue * 100f:F1}%";
        Color textColor = displayValue > 0.15f ? Color.Gold : (displayValue > 0.08f ? Color.LightGreen : Color.White);
        Terraria.Utils.DrawBorderString(spriteBatch, percentText, barPos + new Vector2(170f, 0f), textColor, 0.85f);
    }
    private void DrawGradientFill(SpriteBatch spriteBatch, Vector2 barPos, int fillWidth)
    {
        Color startColor = barColor * 0.6f;
        Color endColor = barColor;
        for (int i = 0; i < fillWidth; i++)
        {
            float t = (float)i / fillWidth;
            Color gradientColor = Color.Lerp(startColor, endColor, t);
            Rectangle pixelRect = new Rectangle((int)barPos.X + i, (int)barPos.Y, 1, BAR_HEIGHT);
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, pixelRect, gradientColor);
        }
    }
    private void DrawFrame(SpriteBatch spriteBatch, Vector2 barPos)
    {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Color frameColor = new Color(100, 150, 255);
        Color innerFrameColor = new Color(50, 80, 150);
        
        spriteBatch.Draw(pixel, new Rectangle((int)barPos.X - 2, (int)barPos.Y - 2, BAR_WIDTH + 4, 2), frameColor);
        spriteBatch.Draw(pixel, new Rectangle((int)barPos.X - 2, (int)barPos.Y + BAR_HEIGHT, BAR_WIDTH + 4, 2), frameColor);
        spriteBatch.Draw(pixel, new Rectangle((int)barPos.X - 2, (int)barPos.Y - 2, 2, BAR_HEIGHT + 4), frameColor);
        spriteBatch.Draw(pixel, new Rectangle((int)barPos.X + BAR_WIDTH, (int)barPos.Y - 2, 2, BAR_HEIGHT + 4), frameColor);
        spriteBatch.Draw(pixel, new Rectangle((int)barPos.X, (int)barPos.Y + BAR_HEIGHT - 2, BAR_WIDTH, 2), innerFrameColor * 0.5f);
    }
    #endregion
    }
    #region Draggable
    public class ArcaneUIPanel : UIPanel
    {
    private Asset<Texture2D> backgroundTexture;
    private bool useCustomBackground = false;
    public ArcaneUIPanel()
    {
        if (ModContent.HasAsset("jarkos/UI/ArcaneForgeUI_BG"))
        {
            backgroundTexture = ModContent.Request<Texture2D>("jarkos/UI/ArcaneForgeUI_BG");
            useCustomBackground = true;
        }
    }
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (ContainsPoint(Main.MouseScreen))
        {
            Main.LocalPlayer.mouseInterface = true;
        }
    }
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
        if (useCustomBackground && backgroundTexture != null && backgroundTexture.IsLoaded)
        {
            CalculatedStyle dimensions = GetDimensions();
            spriteBatch.Draw(backgroundTexture.Value, new Rectangle((int)dimensions.X, (int)dimensions.Y, (int)dimensions.Width, (int)dimensions.Height), new Color(255, 255, 255, 155));
        }
        else
        {
            base.DrawSelf(spriteBatch);
        }
    }
}
    #endregion
    #region GlobalItem
    public class CalibratedWeaponGlobalItem : GlobalItem
    {
        public bool IsCalibrated = false;
        public float CalibrationDamage = 0f;
        public float CalibrationSpeed = 0f;
        public float CalibrationCrit = 0f;
        public float CalibrationKnockback = 0f;
        public string CalibrationPrefix = "";
        public override bool InstancePerEntity => true;
        public override GlobalItem Clone(Item from, Item to)
        {
            CalibratedWeaponGlobalItem clone = (CalibratedWeaponGlobalItem)base.Clone(from, to);
            clone.IsCalibrated = IsCalibrated;
            clone.CalibrationDamage = CalibrationDamage;
            clone.CalibrationSpeed = CalibrationSpeed;
            clone.CalibrationCrit = CalibrationCrit;
            clone.CalibrationKnockback = CalibrationKnockback;
            clone.CalibrationPrefix = CalibrationPrefix;
            return clone;
        }
        public override void SaveData(Item item, TagCompound tag)
        {
            if (IsCalibrated)
            {
                tag["calibrated"] = true;
                tag["calDamage"] = CalibrationDamage;
                tag["calSpeed"] = CalibrationSpeed;
                tag["calCrit"] = CalibrationCrit;
                tag["calKnockback"] = CalibrationKnockback;
                tag["calPrefix"] = CalibrationPrefix;
            }
        }
        public override void LoadData(Item item, TagCompound tag)
        {
            IsCalibrated = tag.GetBool("calibrated");
            if (IsCalibrated)
            {
                CalibrationDamage = tag.GetFloat("calDamage");
                CalibrationSpeed = tag.GetFloat("calSpeed");
                CalibrationCrit = tag.GetFloat("calCrit");
                CalibrationKnockback = tag.GetFloat("calKnockback");
                CalibrationPrefix = tag.GetString("calPrefix");
            }
        }
        public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage)
        {
            if (IsCalibrated)
            {
                damage += CalibrationDamage;
            }
        }
        public override float UseSpeedMultiplier(Item item, Player player)
        {
            if (IsCalibrated)
            {
                return 1f + CalibrationSpeed;
            }
            return 1f;
        }
        public override void ModifyWeaponCrit(Item item, Player player, ref float crit)
        {
            if (IsCalibrated)
            {
                crit += CalibrationCrit * 100f;
            }
        }
        public override void ModifyWeaponKnockback(Item item, Player player, ref StatModifier knockback)
        {
            if (IsCalibrated)
            {
                knockback += CalibrationKnockback;
            }
        }
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (!IsCalibrated) return;
            int nameIndex = tooltips.FindIndex(t => t.Name == "ItemName");
            if (nameIndex != -1)
            {
                tooltips[nameIndex].Text = $"[{CalibrationPrefix}] {tooltips[nameIndex].Text}";
                tooltips[nameIndex].OverrideColor = GetPrefixColor();
            }
            tooltips.Add(new TooltipLine(Mod, "CalibrationHeader", LangSystem.ArcaneForgeHeader.ToString())
            {
                OverrideColor = new Color(100, 200, 255)
            });
            if (CalibrationDamage > 0)
            {
                tooltips.Add(new TooltipLine(Mod, "CalDamage", $"  +{CalibrationDamage * 100f:F1}% " + LangSystem.ArcaneForgeDamageDesc.ToString())
                {
                    OverrideColor = Color.Red
                });
            }
            if (CalibrationSpeed > 0)
            {
                tooltips.Add(new TooltipLine(Mod, "CalSpeed", $"  +{CalibrationSpeed * 100f:F1}% " + LangSystem.ArcaneForgeAttackSpeedDesc.ToString())
                {
                    OverrideColor = Color.Yellow
                });
            }
            if (CalibrationCrit > 0)
            {
                tooltips.Add(new TooltipLine(Mod, "CalCrit", $"  +{CalibrationCrit * 100f:F1}% " + LangSystem.ArcaneForgeCritChanceDesc.ToString())
                {
                    OverrideColor = Color.Orange
                });
            }
            if (CalibrationKnockback > 0)
            {
                tooltips.Add(new TooltipLine(Mod, "CalKnockback", $"  +{CalibrationKnockback * 100f:F1}% " + LangSystem.ArcaneForgeKnockbackDesc.ToString())
                {
                    OverrideColor = Color.Cyan
                });
            }
        }
        private Color GetPrefixColor()
        {
            float total = CalibrationDamage + CalibrationSpeed + CalibrationCrit + CalibrationKnockback;
            if (total >= 0.6f) return new Color(255, 215, 0);
            if (total >= 0.45f) return new Color(150, 200, 255);
            if (total >= 0.3f) return new Color(200, 150, 255);
            if (total >= 0.15f) return new Color(180, 220, 255);
            return new Color(150, 150, 180);
        }
    #endregion
}
