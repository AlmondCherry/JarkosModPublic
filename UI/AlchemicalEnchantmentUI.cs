using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI.Chat;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.ModLoader;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.Audio;
using Jarkos.tiles;
using System;
using Terraria.DataStructures;
using jarkos.core.Data;
using jarkos.systems;
using Terraria.Localization;

namespace Jarkos.UI
{
	public class AlchemicalEnchantmentUIState : UIState
{
    private CustomUIPanel mainPanel;
    private Point16 tePosition = new Point16(-1, -1);
    public AlchemicalEnchantmentTE tileEntity => (tePosition.X != -1 && TileEntity.ByPosition.TryGetValue(tePosition, out TileEntity te) && te is AlchemicalEnchantmentTE ate) ? ate : null;
    private EssenceBarElement essenceBar;
    private UITextButton togglePullButton;
    private ItemSlotElement inputSlot1;
    private ItemSlotElement inputSlot2;
    private ItemSlotElement outputSlot;
    private RecipeIconElement recipeIcon;
    private UIImageButton arrowUp;
    private UIImageButton arrowDown;
    private UIText recipeName;
    private ProgressBarElement progressBar;
    private UITextButton craftButton;
    private UITextButton closeButton;
    private RecipeRequirementsElement recipeRequirements;
    private bool isDragging = false;
    private Vector2 dragOffset;
    private bool isCraftingEffect = false;
    private float craftEffectTimer = 0f;
    private const float CraftEffectDuration = 45f;
    private float glowPulse = 0f;
    private List<BubbleParticle> particles = new List<BubbleParticle>();
    private bool wasOutputEmpty = true;
    private int previousOutputType = 0;
    private int previousOutputStack = 0;
    private static string TexturePath => "jarkos/UI/";
    public override void OnInitialize()
    {
        mainPanel = new CustomUIPanel(ModContent.Request<Texture2D>(TexturePath + "AlchemicalEnchantmentUI_BG", AssetRequestMode.ImmediateLoad));
        mainPanel.Width.Set(580f, 0f);
        mainPanel.Height.Set(300f, 0f);
        mainPanel.HAlign = 0.5f;
        mainPanel.VAlign = 0.5f;
        Append(mainPanel);
        essenceBar = new EssenceBarElement();
        essenceBar.Width.Set(24f, 0f);
        essenceBar.Height.Set(160f, 0f);
        essenceBar.Left.Set(20f, 0f);
        essenceBar.Top.Set(50f, 0f);
        mainPanel.Append(essenceBar);
        togglePullButton = new UITextButton("V", 0.8f);
        togglePullButton.Width.Set(24f, 0f);
        togglePullButton.Height.Set(24f, 0f);
        togglePullButton.Left.Set(20f, 0f);
        togglePullButton.Top.Set(215f, 0f);
        togglePullButton.OnLeftClick += (evt, el) => {
            if (tileEntity != null) {
                tileEntity.PullEssenceMode = !tileEntity.PullEssenceMode;
                tileEntity.SendAction(0);
            }
        };
        mainPanel.Append(togglePullButton);
        UIText title = new UIText(LangSystem.AlchTitle, 1.2f);
        HAlign = 0.5f;
        title.Top.Set(12f, 0f);
        mainPanel.Append(title);
        closeButton = new UITextButton("X", 0.9f);
        closeButton.Width.Set(32f, 0f);
        closeButton.Height.Set(32f, 0f);
        closeButton.Left.Set(-42f, 1f);
        closeButton.Top.Set(8f, 0f);
        closeButton.SetColor(new Color(180, 50, 50));
        closeButton.SetHoverColor(new Color(220, 70, 70));
        closeButton.OnLeftClick += (evt, el) => AlchemicalEnchantmentUISystem.Instance.CloseUI();
        mainPanel.Append(closeButton);
        CustomUIPanel leftSection = new CustomUIPanel(ModContent.Request<Texture2D>(TexturePath + "AlchemicalEnchantmentUI_BG", AssetRequestMode.ImmediateLoad));
        leftSection.Width.Set(130f, 0f);
        leftSection.Height.Set(160f, 0f);
        leftSection.Left.Set(80f, 0f);
        leftSection.Top.Set(50f, 0f);
        mainPanel.Append(leftSection);
        CustomUIPanel requirementsPanel = new CustomUIPanel(ModContent.Request<Texture2D>(TexturePath + "AlchemicalEnchantmentUI_BG02", AssetRequestMode.ImmediateLoad));
        requirementsPanel.Width.Set(480f, 0f);
        requirementsPanel.Height.Set(35f, 0f);
        requirementsPanel.Left.Set(80f, 0f);
        requirementsPanel.Top.Set(215f, 0f);
        mainPanel.Append(requirementsPanel);
        recipeRequirements = new RecipeRequirementsElement();
        recipeRequirements.Width.Set(470f, 0f);
        recipeRequirements.Height.Set(30f, 0f);
        recipeRequirements.Left.Set(5f, 0f);
        recipeRequirements.Top.Set(2f, 0f);
        requirementsPanel.Append(recipeRequirements);
        UIText inputLabel = new UIText(jarkos.systems.LangSystem.AlchIngredients, 0.85f);
        HAlign = 0.5f;
        inputLabel.Top.Set(8f, 0f);
        leftSection.Append(inputLabel);
        inputSlot1 = new ItemSlotElement(0);
        inputSlot1.Width.Set(52f, 0f);
        inputSlot1.Height.Set(52f, 0f);
        inputSlot1.HAlign = 0.5f;
        inputSlot1.Top.Set(35f, 0f);
        leftSection.Append(inputSlot1);
        inputSlot2 = new ItemSlotElement(1);
        inputSlot2.Width.Set(52f, 0f);
        inputSlot2.Height.Set(52f, 0f);
        inputSlot2.HAlign = 0.5f;
        inputSlot2.Top.Set(98f, 0f);
        leftSection.Append(inputSlot2);
        CustomUIPanel centerSection = new CustomUIPanel(ModContent.Request<Texture2D>(TexturePath + "AlchemicalEnchantmentUI_BG", AssetRequestMode.ImmediateLoad));
        centerSection.Width.Set(170f, 0f);
        centerSection.Height.Set(160f, 0f);
        centerSection.Left.Set(230f, 0f);
        centerSection.Top.Set(50f, 0f);
        mainPanel.Append(centerSection);
        UIText recipeLabel = new UIText(jarkos.systems.LangSystem.AlchRecipe, 0.85f);
        HAlign = 0.5f;
        recipeLabel.Top.Set(8f, 0f);
        centerSection.Append(recipeLabel);
        arrowUp = new UIImageButton(ModContent.Request<Texture2D>(TexturePath + "arrow_up", AssetRequestMode.ImmediateLoad));
        arrowUp.Width.Set(32f, 0f);
        arrowUp.Height.Set(32f, 0f);
        arrowUp.HAlign = 0.5f;
        arrowUp.Top.Set(30f, 0f);
        arrowUp.OnLeftClick += ArrowUp_Click;
        centerSection.Append(arrowUp);
        recipeIcon = new RecipeIconElement();
        recipeIcon.Width.Set(56f, 0f);
        recipeIcon.Height.Set(56f, 0f);
        recipeIcon.HAlign = 0.5f;
        recipeIcon.Top.Set(65f, 0f);
        centerSection.Append(recipeIcon);
        arrowDown = new UIImageButton(ModContent.Request<Texture2D>(TexturePath + "arrow_down", AssetRequestMode.ImmediateLoad));
        arrowDown.Width.Set(32f, 0f);
        arrowDown.Height.Set(32f, 0f);
        arrowDown.HAlign = 0.5f;
        arrowDown.Top.Set(120f, 0f);
        arrowDown.OnLeftClick += ArrowDown_Click;
        centerSection.Append(arrowDown);
        CustomUIPanel rightSection = new CustomUIPanel(ModContent.Request<Texture2D>(TexturePath + "AlchemicalEnchantmentUI_BG", AssetRequestMode.ImmediateLoad));
        rightSection.Width.Set(130f, 0f);
        rightSection.Height.Set(160f, 0f);
        rightSection.Left.Set(420f, 0f);
        rightSection.Top.Set(50f, 0f);
        mainPanel.Append(rightSection);
        UIText outputLabel = new UIText(LangSystem.AlchResult, 0.85f);
        HAlign = 0.5f;
        outputLabel.Top.Set(8f, 0f);
        rightSection.Append(outputLabel);
        outputSlot = new ItemSlotElement(2, true);
        outputSlot.Width.Set(52f, 0f);
        outputSlot.Height.Set(52f, 0f);
        outputSlot.HAlign = 0.5f;
        outputSlot.Top.Set(55f, 0f);
        rightSection.Append(outputSlot);
        recipeName = new UIText("", 0.9f);
        recipeName.HAlign = 0.5f;
        recipeName.Top.Set(220f, 0f);
        mainPanel.Append(recipeName);
        progressBar = new ProgressBarElement(ModContent.Request<Texture2D>(TexturePath + "AlchemicalEnchantmentUI_BarFrame", AssetRequestMode.ImmediateLoad), ModContent.Request<Texture2D>(TexturePath + "AlchemicalEnchantmentUI_BarFill", AssetRequestMode.ImmediateLoad));
        progressBar.Width.Set(435f, 0f);
        progressBar.Height.Set(25f, 0f);
        progressBar.Left.Set(20f, 0f);
        progressBar.Top.Set(260f, 0f);
        mainPanel.Append(progressBar);
        craftButton = new UITextButton(LangSystem.AlchCraft, 0.9f);
        craftButton.Width.Set(100f, 0f);
        craftButton.Height.Set(35f, 0f);
        craftButton.Left.Set(460f, 0f);
        craftButton.Top.Set(255f, 0f);
        craftButton.OnLeftClick += CraftButton_Click;
        mainPanel.Append(craftButton);
        mainPanel.OnLeftMouseDown += MainPanel_OnLeftMouseDown;
        mainPanel.OnLeftMouseUp += MainPanel_OnLeftMouseUp;
    }
    public void TriggerCraftEffect()
    {
        isCraftingEffect = true;
        craftEffectTimer = CraftEffectDuration;
        glowPulse = 0f;
        SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/AlchemicalMoonCraft") with { Pitch = Main.rand.NextFloat(-0.3f, 0.3f)});
        SpawnBubbleExplosion();
    }
    private void SpawnBubbleExplosion()
    {
        if (outputSlot == null) return;
        var dims = outputSlot.GetDimensions();
        Vector2 center = new Vector2(dims.X + dims.Width / 2, dims.Y + dims.Height / 2);
        int count = Main.rand.Next(6, 11);
        for (int i = 0; i < count; i++)
        {
            float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
            float speed = Main.rand.NextFloat(2f, 6f);
            Vector2 velocity = new Vector2((float)Math.Cos(angle) * speed, (float)Math.Sin(angle) * speed);
            float size = Main.rand.NextFloat(0.1f, 0.9f);
            float lifetime = Main.rand.NextFloat(40f, 80f);
            Color color = Main.rand.Next(4)
            switch
            {
                0 => new Color(100, 200, 255, 200), 1 => new Color(150, 255, 150, 200), 2 => new Color(255, 200, 100, 200), _ => new Color(200, 150, 255, 200)
            };
            particles.Add(new BubbleParticle(center, velocity, size, lifetime, color));
        }
    }
    private void MainPanel_OnLeftMouseDown(UIMouseEvent evt, UIElement listeningElement)
    {
        if (evt.Target == mainPanel)
        {
            isDragging = true;
            dragOffset = new Vector2(evt.MousePosition.X - mainPanel.Left.Pixels, evt.MousePosition.Y - mainPanel.Top.Pixels);
        }
    }
    private void MainPanel_OnLeftMouseUp(UIMouseEvent evt, UIElement listeningElement)
    {
        isDragging = false;
    }
    private void ArrowUp_Click(UIMouseEvent evt, UIElement listeningElement)
    {
        if (tileEntity != null)
        {
            tileEntity.CycleRecipe(-1);
            tileEntity.SendAction(1, -1);
            SoundEngine.PlaySound(SoundID.MenuTick);
        }
    }
    private void ArrowDown_Click(UIMouseEvent evt, UIElement listeningElement)
    {
        if (tileEntity != null)
        {
            tileEntity.CycleRecipe(1);
            tileEntity.SendAction(1, 1);
            SoundEngine.PlaySound(SoundID.MenuTick);
        }
    }
    private void CraftButton_Click(UIMouseEvent evt, UIElement listeningElement)
    {
        if (tileEntity != null && tileEntity.CanCraft())
        {
            tileEntity.StartCrafting();
            tileEntity.SendAction(2);
            SoundEngine.PlaySound(SoundID.MenuTick);
        }
        else
        {
            SoundEngine.PlaySound(SoundID.MenuClose);
        }
    }
    public void SetTileEntity(AlchemicalEnchantmentTE te)
    {
        tePosition = te != null ? te.Position : new Point16(-1, -1);
        var activeTe = tileEntity;
        if (essenceBar != null) essenceBar.tileEntity = activeTe;
        if (inputSlot1 != null) inputSlot1.SetTileEntity(activeTe);
        if (inputSlot2 != null) inputSlot2.SetTileEntity(activeTe);
        if (outputSlot != null) outputSlot.SetTileEntity(activeTe);
        if (recipeIcon != null) recipeIcon.SetTileEntity(activeTe);
        if (recipeRequirements != null) recipeRequirements.SetTileEntity(activeTe);

        wasOutputEmpty = te?.OutputSlot?.IsAir ?? true;
        previousOutputType = te?.OutputSlot?.type ?? 0;
        previousOutputStack = te?.OutputSlot?.stack ?? 0;
    }
    public override void Update(GameTime gameTime)
    {
        var te = tileEntity;
        if (te == null)
        {
            AlchemicalEnchantmentUISystem.Instance.CloseUI();
            return;
        }
        if (essenceBar != null) essenceBar.tileEntity = te;
        if (inputSlot1 != null) inputSlot1.SetTileEntity(te);
        if (inputSlot2 != null) inputSlot2.SetTileEntity(te);
        if (outputSlot != null) outputSlot.SetTileEntity(te);
        if (recipeIcon != null) recipeIcon.SetTileEntity(te);
        if (recipeRequirements != null) recipeRequirements.SetTileEntity(te);

        base.Update(gameTime);
        if (te == null) return;
        if (isDragging)
        {
            mainPanel.Left.Set(Main.mouseX - dragOffset.X, 0f);
            mainPanel.Top.Set(Main.mouseY - dragOffset.Y, 0f);
            Recalculate();
        }
        if (te.IsCrafting && Main.netMode == NetmodeID.MultiplayerClient)
        {
            if (te.CraftingProgress < AlchemicalEnchantmentTE.MaxProgress)
            {
                te.CraftingProgress++;
            }
        }
        float progress = (float)te.CraftingProgress / AlchemicalEnchantmentTE.MaxProgress;
        progressBar.SetProgress(progress);
        if (togglePullButton != null)
        {
            if (te.PullEssenceMode)
            {
                togglePullButton.SetColor(new Color(150, 60, 200));
                togglePullButton.SetBorderColor(new Color(200, 100, 255));
                togglePullButton.SetHoverColor(new Color(180, 90, 230));
            }
            else
            {
                togglePullButton.SetColor(new Color(73, 94, 171));
                togglePullButton.SetBorderColor(new Color(40, 50, 100));
                togglePullButton.SetHoverColor(new Color(100, 120, 200));
            }
        }
        bool isOutputEmpty = te.OutputSlot.IsAir;
        int currentOutputType = te.OutputSlot.type;
        int currentOutputStack = te.OutputSlot.stack;
        if (!isOutputEmpty && (wasOutputEmpty || currentOutputType != previousOutputType || currentOutputStack > previousOutputStack))
        {
            TriggerCraftEffect();
        }
        wasOutputEmpty = isOutputEmpty;
        previousOutputType = currentOutputType;
        previousOutputStack = currentOutputStack;
        if (isCraftingEffect)
        {
            craftEffectTimer--;
            glowPulse += 0.15f;
            if (craftEffectTimer <= 0)
            {
                isCraftingEffect = false;
            }
        }
        for (int i = particles.Count - 1; i >= 0; i--)
        {
            particles[i].Update();
            if (particles[i].IsDead)
            {
                particles.RemoveAt(i);
            }
        }
        var recipe = te.GetCurrentRecipe();
        recipeName.SetText(recipe?.Name ?? "");
        bool canCraft = te.CanCraft();
        craftButton.SetColor(canCraft ? new Color(80, 180, 80) : new Color(100, 100, 100));
        craftButton.SetHoverColor(canCraft ? new Color(100, 220, 100) : new Color(120, 120, 120));
        if (mainPanel.ContainsPoint(Main.MouseScreen))
        {
            Main.LocalPlayer.mouseInterface = true;
        }
    }
    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);
        if (togglePullButton != null && togglePullButton.IsMouseHovering && tileEntity != null)
        {
            Main.hoverItemName = tileEntity.PullEssenceMode ? LangSystem.AlchSuckingMode.Value : LangSystem.AlchTransmissionMode.Value;
        }
        if (recipeIcon != null && recipeIcon.IsMouseHovering && tileEntity != null)
        {
            var recipe = tileEntity.GetCurrentRecipe();
            if (recipe != null)
            {
                recipeIcon.DrawCustomRecipeTooltip(spriteBatch, recipe);
            }
        }
        if (isCraftingEffect && outputSlot != null)
        {
            DrawCraftGlowEffect(spriteBatch);
        }
        foreach (var particle in particles)
        {
            particle.Draw(spriteBatch);
        }
    }
    private void DrawCraftGlowEffect(SpriteBatch spriteBatch)
    {
        var dims = outputSlot.GetDimensions();
        float alpha = craftEffectTimer / CraftEffectDuration;
        float pulse = (float)Math.Sin(glowPulse) * 0.3f + 0.7f;
        Vector2 center = new Vector2(dims.X + dims.Width / 2, dims.Y + dims.Height / 2);
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        for (int layer = 3; layer >= 0; layer--)
        {
            float layerScale = 1f + layer * 0.15f + (1f - alpha) * 0.3f;
            float layerAlpha = alpha * pulse * (1f - layer * 0.2f) * 0.4f;
            int size = (int)(dims.Width * layerScale);
            Rectangle glowRect = new Rectangle((int)(center.X - size / 2), (int)(center.Y - size / 2), size, size);
            Color glowColor = Color.Lerp(Color.Gold, Color.White, layer * 0.2f) * layerAlpha;
            spriteBatch.Draw(pixel, glowRect, glowColor);
        }
        if (alpha > 0.7f)
        {
            float flashAlpha = (alpha - 0.7f) / 0.3f;
            int flashSize = (int)(dims.Width * 0.8f);
            Rectangle flashRect = new Rectangle((int)(center.X - flashSize / 2), (int)(center.Y - flashSize / 2), flashSize, flashSize);
            spriteBatch.Draw(pixel, flashRect, Color.White * flashAlpha * 0.6f);
        }
    }
}
#region Bubble particle
public class BubbleParticle
{
    public Vector2 Position;
    public Vector2 Velocity;
    public float Scale;
    public float Lifetime;
    public float MaxLifetime;
    public Color Color;
    public bool IsDead => Lifetime <= 0;
    private static Texture2D bubbleTexture;
    private static bool textureLoadAttempted = false;
    private float rotation;
    private float rotationSpeed;
    private float wobble;
    private float wobbleSpeed;
    public BubbleParticle(Vector2 pos, Vector2 vel, float scale, float lifetime, Color color)
    {
        Position = pos;
        Velocity = vel;
        Scale = scale;
        Lifetime = lifetime;
        MaxLifetime = lifetime;
        Color = color;
        rotation = Main.rand.NextFloat() * MathHelper.TwoPi;
        rotationSpeed = Main.rand.NextFloat(-0.08f, 0.08f);
        wobble = Main.rand.NextFloat() * MathHelper.TwoPi;
        wobbleSpeed = Main.rand.NextFloat(0.1f, 0.2f);
    }
    public void Update()
    {
        Position += Velocity;
        Velocity *= 0.95f;
        Velocity.Y -= 0.08f;
        wobble += wobbleSpeed;
        Position.X += (float)Math.Sin(wobble) * 0.3f;
        rotation += rotationSpeed;
        Lifetime--;
    }
    public void Draw(SpriteBatch spriteBatch)
    {
        LoadTexture();
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
        float alpha = Lifetime / MaxLifetime;
        float scaleMultiplier;
        if (alpha > 0.8f)
        {
            scaleMultiplier = 1f - (alpha - 0.8f) * 2f;
        }
        else if (alpha < 0.2f)
        {
            scaleMultiplier = alpha / 0.2f;
        }
        else
        {
            scaleMultiplier = 1f;
        }
        float drawScale = Scale * scaleMultiplier;
        Color drawColor = Color * alpha;
        if (bubbleTexture != null)
        {
            Vector2 origin = new Vector2(bubbleTexture.Width / 2f, bubbleTexture.Height / 2f);
            spriteBatch.Draw(bubbleTexture, Position, null, drawColor, rotation, origin, drawScale, SpriteEffects.None, 0f);
        }
        else
        {
            DrawCircleFallback(spriteBatch, Position, drawScale * 8f, drawColor);
        }
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
    }
    private static void LoadTexture()
    {
        if (textureLoadAttempted) return;
        textureLoadAttempted = true;
        try
        {
            bubbleTexture = ModContent.Request<Texture2D>("jarkos/particles/storm/Bubble_particle", AssetRequestMode.ImmediateLoad).Value;
        }
        catch
        {
            bubbleTexture = null;
        }
    }
    private void DrawCircleFallback(SpriteBatch spriteBatch, Vector2 center, float radius, Color color)
    {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        int size = (int)(radius * 2);
        Rectangle rect = new Rectangle((int)(center.X - radius), (int)(center.Y - radius), size, size);
        spriteBatch.Draw(pixel, rect, color * 0.5f);
        
        int innerSize = (int)(size * 0.7f);
        Rectangle innerRect = new Rectangle((int)(center.X - innerSize / 2), (int)(center.Y - innerSize / 2), innerSize, innerSize);
        spriteBatch.Draw(pixel, innerRect, color * 0.8f);

        int highlightSize = (int)(size * 0.25f);
        Rectangle highlightRect = new Rectangle((int)(center.X - radius * 0.4f), (int)(center.Y - radius * 0.4f), highlightSize, highlightSize);
        spriteBatch.Draw(pixel, highlightRect, Color.White * 0.6f);
    }
}
#endregion
#region CustomUIPanel
    public class CustomUIPanel : UIElement
{
    private Asset<Texture2D> backgroundTexture;
    private Color drawColor = new Color(255, 255, 255, 120);
    private bool drawBorder = true;
    private Color borderColor = new Color(40, 50, 100);
    private int borderWidth = 2;
    public CustomUIPanel(Asset<Texture2D> texture)
    {
        backgroundTexture = texture;
    }
    public void SetDrawColor(Color color)
    {
        drawColor = color;
    }
    public void SetBorder(bool draw, Color? color = null, int width = 2)
    {
        drawBorder = draw;
        if (color.HasValue) borderColor = color.Value;
        borderWidth = width;
    }
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
        CalculatedStyle dimensions = GetDimensions();
        Rectangle drawRect = new Rectangle((int)dimensions.X, (int)dimensions.Y, (int)dimensions.Width, (int)dimensions.Height);
        if (backgroundTexture != null && backgroundTexture.IsLoaded)
        {
            spriteBatch.Draw(backgroundTexture.Value, drawRect, drawColor);
        }
        else
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            spriteBatch.Draw(pixel, drawRect, new Color(73, 94, 171, 200));
        }
    }
    }
    #region ProgressBarElement
    public class ProgressBarElement : UIElement
{
    private Asset<Texture2D> backgroundTexture;
    private Asset<Texture2D> fillTexture;
    private float progress = 0f;
    private int padding = 2;
    public ProgressBarElement(Asset<Texture2D> background, Asset<Texture2D> fill)
    {
        backgroundTexture = background;
        fillTexture = fill;
    }
    public void SetProgress(float value)
    {
        progress = MathHelper.Clamp(value, 0f, 1f);
    }
    public void SetPadding(int value)
    {
        padding = value;
    }
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        CalculatedStyle dimensions = GetDimensions();
        Rectangle backgroundRect = new Rectangle((int)dimensions.X, (int)dimensions.Y, (int)dimensions.Width, (int)dimensions.Height);
        if (backgroundTexture != null && backgroundTexture.IsLoaded)
        {
            spriteBatch.Draw(backgroundTexture.Value, backgroundRect, Color.White);
        }
        else
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            spriteBatch.Draw(pixel, backgroundRect, new Color(30, 30, 50, 200));
        }
        if (progress > 0f && fillTexture != null && fillTexture.IsLoaded)
        {
            Texture2D fill = fillTexture.Value;
            int fillAreaWidth = (int)dimensions.Width - padding * 2;
            int fillAreaHeight = (int)dimensions.Height - padding * 2;
            int visibleWidth = (int)(fillAreaWidth * progress);
            if (visibleWidth > 0)
            {
                Vector2 fillPosition = new Vector2(dimensions.X + padding, dimensions.Y + padding);
                int sourceWidth = (int)(fill.Width * progress);
                Rectangle sourceRect = new Rectangle(0, 0, sourceWidth, fill.Height);
                Rectangle destRect = new Rectangle((int)fillPosition.X, (int)fillPosition.Y, visibleWidth, fillAreaHeight);
                spriteBatch.Draw(fill, destRect, sourceRect, Color.White);
            }
        }
        else if (progress > 0f)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            int fillWidth = (int)((dimensions.Width - padding * 2) * progress);
            Rectangle fillRect = new Rectangle((int)dimensions.X + padding, (int)dimensions.Y + padding, fillWidth, (int)dimensions.Height - padding * 2);
            spriteBatch.Draw(pixel, fillRect, new Color(80, 200, 80, 255));
        }
        string percentText = $"{(int)(progress * 100)}%";
        Vector2 textSize = FontAssets.MouseText.Value.MeasureString(percentText) * 0.6f;
        Vector2 textPos = new Vector2(dimensions.X + dimensions.Width / 2 - textSize.X / 2, dimensions.Y + dimensions.Height / 2 - textSize.Y / 2);
        Terraria.Utils.DrawBorderString(spriteBatch, percentText, textPos, Color.White, 0.6f);
    }
    }
    #endregion
    #region RecipeRequirementsElement
    public class RecipeRequirementsElement : UIElement
{
    private AlchemicalEnchantmentTE tileEntity;
    public void SetTileEntity(AlchemicalEnchantmentTE te)
    {
        tileEntity = te;
    }
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        if (tileEntity == null) return;
        var recipe = tileEntity.GetCurrentRecipe();
        if (recipe == null) return;
        CalculatedStyle dims = GetDimensions();
        float x = dims.X;
        float y = dims.Y + 16;
        DrawIngredientRequirement(spriteBatch, ref x, y, recipe.Ingredient1, recipe.Ingredient1Count, tileEntity.HasEnoughIngredient(0));
        x += 10;
        Terraria.Utils.DrawBorderString(spriteBatch, "+", new Vector2(x, y - 2), Color.White, 0.9f);
        x += 20;
        DrawIngredientRequirement(spriteBatch, ref x, y, recipe.Ingredient2, recipe.Ingredient2Count, tileEntity.HasEnoughIngredient(1));
        x += 15;
        Terraria.Utils.DrawBorderString(spriteBatch, "→", new Vector2(x, y - 2), Color.Gold, 1f);
        x += 25;
        DrawResultItem(spriteBatch, ref x, y, recipe.Result, recipe.ResultCount);
    }
    private void DrawIngredientRequirement(SpriteBatch spriteBatch, ref float x, float y, int itemType, int count, bool hasEnough)
    {
        Main.instance.LoadItem(itemType);
        Texture2D tex = TextureAssets.Item[itemType].Value;
        Rectangle frame = Main.itemAnimations[itemType] != null
            ? Main.itemAnimations[itemType].GetFrame(tex)
            : tex.Bounds;
        float scale = 20f / Math.Max(tex.Width, tex.Height);
        Vector2 pos = new Vector2(x + 10, y);
        float itemScale = 1f;
        float maxSize = 32f;
        if (frame.Width > maxSize || frame.Height > maxSize)
        {
            itemScale = maxSize / Math.Max(frame.Width, frame.Height);
        }
        Vector2 itemOrigin = new Vector2(frame.Width / 2f, frame.Height / 2f);
        Color itemColor = hasEnough ? Color.White : Color.Gray;
        spriteBatch.Draw(tex, pos, frame, itemColor, 0f, itemOrigin, itemScale, SpriteEffects.None, 0f);
        x += 25;
        Color textColor = hasEnough ? Color.LimeGreen : Color.Red;
        string countText = count > 1 ? $"x{count}" : "";
        if (countText != "")
        {
            Terraria.Utils.DrawBorderString(spriteBatch, countText, new Vector2(x, y - 2), textColor, 0.7f);
            x += countText.Length * 8;
        }
    }
    private void DrawResultItem(SpriteBatch spriteBatch, ref float x, float y, int itemType, int count)
    {
        Main.instance.LoadItem(itemType);
        Texture2D tex = TextureAssets.Item[itemType].Value;
        float scale = 24f / Math.Max(tex.Width, tex.Height);
        Vector2 pos = new Vector2(x + 12, y);
        Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / 2f);
        spriteBatch.Draw(tex, pos, null, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
        x += 30;
        if (count > 1)
        {
            Terraria.Utils.DrawBorderString(spriteBatch, $"x{count}", new Vector2(x, y - 2), Color.Gold, 0.8f);
        }
    }
}
    #region RecipeIconElement
    public class RecipeIconElement : UIElement
{
    private AlchemicalEnchantmentTE tileEntity;
    private Texture2D[] recipeIcons;
    private Texture2D frameTexture;
    private bool texturesLoaded = false;
    private int previousRecipeIndex = -1;
    private int oldRecipeIndex = -1;
    private float transitionProgress = 1f;
    private int transitionDirection = 0;
    private const float TransitionSpeed = 0.08f;
    private bool isTransitioning = false;
    public RecipeIconElement()
    {
        Width.Set(56f, 0f);
        Height.Set(56f, 0f);
    }
    public void SetTileEntity(AlchemicalEnchantmentTE te)
    {
        tileEntity = te;
        if (te != null)
        {
            previousRecipeIndex = te.SelectedRecipeIndex;
        }
    }
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (tileEntity != null)
        {
            int currentIndex = tileEntity.SelectedRecipeIndex;
            if (previousRecipeIndex != -1 && previousRecipeIndex != currentIndex && !isTransitioning)
            {
                oldRecipeIndex = previousRecipeIndex;
                int recipeCount = AlchemicalEnchantmentTE.Recipes?.Count ?? 0;
                if (recipeCount > 0)
                {
                    if (previousRecipeIndex == recipeCount - 1 && currentIndex == 0)
                    {
                        transitionDirection = 1;
                    }
                    else if (previousRecipeIndex == 0 && currentIndex == recipeCount - 1)
                    {
                        transitionDirection = -1;
                    }
                    else
                    {
                        transitionDirection = previousRecipeIndex < currentIndex ? 1 : -1;
                    }
                }
                isTransitioning = true;
                transitionProgress = 0f;
            }
            previousRecipeIndex = currentIndex;
        }
        if (isTransitioning)
        {
            transitionProgress += TransitionSpeed;
            if (transitionProgress >= 1f)
            {
                transitionProgress = 1f;
                isTransitioning = false;
            }
        }
    }
    private void LoadTextures()
    {
        if (texturesLoaded) return;
        string modName = "jarkos";
        try
        {
            frameTexture = ModContent.Request<Texture2D>($"{modName}/UI/AmmoButtonHover", AssetRequestMode.ImmediateLoad).Value;
        }
        catch
        {
            frameTexture = null;
        }
        int recipeCount = AlchemicalEnchantmentTE.Recipes?.Count ?? 0;
        recipeIcons = new Texture2D[recipeCount];
        for (int i = 0; i < recipeCount; i++)
        {
            try
            {
                recipeIcons[i] = ModContent.Request<Texture2D>($"{modName}/UI/RecipeIcons/Recipe_{i}", AssetRequestMode.ImmediateLoad).Value;
            }
            catch
            {
                recipeIcons[i] = null;
            }
        }
        texturesLoaded = true;
    }
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        if (!texturesLoaded)
        {
            LoadTextures();
        }
        CalculatedStyle dimensions = GetDimensions();
        Rectangle destRect = new Rectangle((int)dimensions.X, (int)dimensions.Y, (int)dimensions.Width, (int)dimensions.Height);
        if (frameTexture != null)
        {
            spriteBatch.Draw(frameTexture, destRect, Color.White);
        }
        else
        {
            DrawSimpleFrame(spriteBatch, destRect);
        }
        int padding = 0;
        Rectangle iconRect = new Rectangle(destRect.X - 4, destRect.Y - 4, destRect.Width - padding * 2, destRect.Height - padding * 2);
        if (tileEntity == null)
        {
            DrawQuestionMark(spriteBatch, iconRect);
            return;
        }
        int currentIndex = tileEntity.SelectedRecipeIndex;
        var recipe = tileEntity.GetCurrentRecipe();
        if (recipe == null)
        {
            DrawQuestionMark(spriteBatch, iconRect);
            return;
        }
        if (isTransitioning && oldRecipeIndex >= 0)
        {
            float easedProgress = EaseOutBack(transitionProgress);
            float fadeProgress = EaseInOutQuad(transitionProgress);
            float oldOffset = transitionDirection * fadeProgress * iconRect.Height * 1.2f;
            float oldAlpha = 1f - fadeProgress;
            float oldScale = 1f - fadeProgress * 0.4f;

            DrawRecipeIcon(spriteBatch, iconRect, oldRecipeIndex, oldOffset, oldAlpha, oldScale);
            float newOffset = -transitionDirection * (1f - easedProgress) * iconRect.Height * 0.8f;
            float newAlpha = fadeProgress;
            float newScale = 0.6f + easedProgress * 0.4f;

            DrawRecipeIcon(spriteBatch, iconRect, currentIndex, newOffset, newAlpha, newScale);
        }
        else
        {
            DrawRecipeIcon(spriteBatch, iconRect, currentIndex, 0f, 1f, 1f);
        }
    }
    private float EaseInOutQuad(float t)
    {
        return t < 0.5f ? 2f * t * t : 1f - (float)Math.Pow(-2f * t + 2f, 2f) / 2f;
    }
    private float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * (float)Math.Pow(t - 1f, 3f) + c1 * (float)Math.Pow(t - 1f, 2f);
    }
    private void DrawRecipeIcon(SpriteBatch spriteBatch, Rectangle iconRect, int recipeIndex, float yOffset, float alpha, float scale)
    {
        if (recipeIndex < 0) return;
        var recipes = AlchemicalEnchantmentTE.Recipes;
        if (recipes == null || recipeIndex >= recipes.Count) return;
        var recipe = recipes[recipeIndex];
        if (recipe == null) return;
        Vector2 center = new Vector2(iconRect.X + iconRect.Width / 2f, iconRect.Y + iconRect.Height / 2f + yOffset);
        Color drawColor = Color.White * alpha;
        if (recipeIcons != null && recipeIndex >= 0 && recipeIndex < recipeIcons.Length && recipeIcons[recipeIndex] != null)
        {
            Texture2D icon = recipeIcons[recipeIndex];
            Vector2 origin = new Vector2(icon.Width / 2f, icon.Height / 2f);
            spriteBatch.Draw(icon, center, null, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
        }
        else
        {
            DrawItemIconAnimated(spriteBatch, recipe.Result, center, iconRect, scale, drawColor);
        }
    }
    private void DrawItemIconAnimated(SpriteBatch spriteBatch, int itemType, Vector2 center, Rectangle destRect, float scale, Color color)
    {
        Main.instance.LoadItem(itemType);
        Texture2D tex = TextureAssets.Item[itemType].Value;
        Rectangle frame = Main.itemAnimations[itemType] != null
            ? Main.itemAnimations[itemType].GetFrame(tex)
            : tex.Bounds;
        float baseScale = Math.Min((float)destRect.Width / frame.Width, (float)destRect.Height / frame.Height);
        if (baseScale > 1.2f) baseScale = 1.2f;
        baseScale *= scale;
        Vector2 origin = new Vector2(frame.Width / 2f, frame.Height / 2f);
        spriteBatch.Draw(tex, center, frame, color, 0f, origin, baseScale, SpriteEffects.None, 0f);
    }
    public void DrawCustomRecipeTooltip(SpriteBatch spriteBatch, PotionRecipe recipe)
    {
        if (recipe == null || tileEntity == null) return;
        Item ing1 = new Item();
        ing1.SetDefaults(recipe.Ingredient1);
        Item ing2 = new Item();
        ing2.SetDefaults(recipe.Ingredient2);
        Item result = new Item();
        result.SetDefaults(recipe.Result);
        List<string> lines = new List<string>();
        lines.Add(recipe.Name);
        lines.Add(LangSystem.AlchRequires.Value);
        lines.Add($"  {ing1.Name} x{recipe.Ingredient1Count}");
        lines.Add($"  {ing2.Name} x{recipe.Ingredient2Count}");
        int essenceStartLineIndex = lines.Count;
        var reqs = tileEntity.GetRequiredEssences(tileEntity.SelectedRecipeIndex);
        foreach (var req in reqs)
        {
            tileEntity.StoredEssences.TryGetValue(req.type, out int currentAmount);
            lines.Add($"{req.type} x{req.count} ({currentAmount}/{req.count})");
        }
        lines.Add($"{LangSystem.AlchResultText.Value} {result.Name} x{recipe.ResultCount}");
        int descLineIndex = -1;
        if (!string.IsNullOrEmpty(recipe.Description))
        {
            descLineIndex = lines.Count;
            lines.Add(recipe.Description);
        }
        var font = FontAssets.MouseText.Value;
        float maxWidth = 0f;
        float lineHeight = 24f;
        foreach (var line in lines)
        {
            Vector2 size = font.MeasureString(line);
            if (size.X > maxWidth) maxWidth = size.X;
        }
        float boxWidth = maxWidth + 30f;
        float boxHeight = lines.Count * lineHeight + 20f;
        float boxX = Main.mouseX + 12f;
        float boxY = Main.mouseY + 12f;
        if (boxX + boxWidth > Main.screenWidth) boxX = Main.screenWidth - boxWidth - 10f;
        if (boxY + boxHeight > Main.screenHeight) boxY = Main.screenHeight - boxHeight - 10f;
        Rectangle boxRect = new Rectangle((int)boxX, (int)boxY, (int)boxWidth, (int)boxHeight);
        Color bgColor = new Color(20, 20, 30, 240);
        Color borderColor = new Color(100, 100, 150, 245);
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, boxRect, bgColor);
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(boxRect.Left, boxRect.Top, boxRect.Width, 1), borderColor);
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(boxRect.Left, boxRect.Bottom - 1, boxRect.Width, 1), borderColor);
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(boxRect.Left, boxRect.Top, 1, boxRect.Height), borderColor);
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(boxRect.Right - 1, boxRect.Top, 1, boxRect.Height), borderColor);
        float currentY = boxY + 10f;
        for (int i = 0; i < lines.Count; i++)
        {
            string line = lines[i];
            Vector2 pos = new Vector2(boxX + 15f, currentY);
            Color textColor = Color.White;
            if (i == 0)
            {
                textColor = Color.Gold;
            }
            else if (i == 1 || i == lines.Count - (descLineIndex != -1 ? 2 : 1))
            {
                textColor = new Color(170, 170, 170);
            }
            else if (i >= essenceStartLineIndex && i < essenceStartLineIndex + reqs.Count)
            {
                int reqIndex = i - essenceStartLineIndex;
                var req = reqs[reqIndex];
                tileEntity.StoredEssences.TryGetValue(req.type, out int currentAmount);
                textColor = currentAmount >= req.count ? new Color(136, 255, 136) : new Color(255, 136, 136);
                var tex = EssencePalette.GetTexture(req.type);
                if (tex != null)
                {
                    Vector2 iconPos = new Vector2(boxX + 15f, currentY + 3f);
                    spriteBatch.Draw(tex, new Rectangle((int)iconPos.X, (int)iconPos.Y, 16, 16), Color.White);
                }
            }
            else if (i == descLineIndex)
            {
                textColor = new Color(136, 255, 136);
            }
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, line, pos, textColor, 0f, Vector2.Zero, Vector2.One);
            currentY += lineHeight;
        }
    }
    private void DrawSimpleFrame(SpriteBatch spriteBatch, Rectangle rect)
    {
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Color frameColor = new Color(100, 100, 150);
        Color bgColor = new Color(40, 40, 80, 200);
        spriteBatch.Draw(pixel, rect, bgColor);
        int b = 2;
        spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, b), frameColor);
        spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Bottom - b, rect.Width, b), frameColor);
        spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, b, rect.Height), frameColor);
        spriteBatch.Draw(pixel, new Rectangle(rect.Right - b, rect.Y, b, rect.Height), frameColor);
    }
    private void DrawQuestionMark(SpriteBatch spriteBatch, Rectangle rect)
    {
        Vector2 center = new Vector2(rect.X + rect.Width / 2f, rect.Y + rect.Height / 2f);
        Terraria.Utils.DrawBorderString(spriteBatch, "?", center, Color.Gray, 1.2f, 0.5f, 0.5f);
    }
}
    #region ItemSlotElement
    public class ItemSlotElement : UIElement
{
    private int slotIndex;
    private bool isOutput;
    private AlchemicalEnchantmentTE tileEntity;
    private float scale = 1f;
    private float hoverScale = 1f;
    private float glowIntensity = 0f;
    private float errorPulse = 0f;
    public ItemSlotElement(int index, bool output = false)
    {
        slotIndex = index;
        isOutput = output;
        Width.Set(52f, 0f);
        Height.Set(52f, 0f);
    }
    public void SetTileEntity(AlchemicalEnchantmentTE te)
    {
        tileEntity = te;
    }
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        float targetScale = IsMouseHovering ? 1.08f : 1f;
        hoverScale = MathHelper.Lerp(hoverScale, targetScale, 0.15f);
        errorPulse += 0.1f;
        if (isOutput && tileEntity != null && !tileEntity.OutputSlot.IsAir)
        {
            glowIntensity = MathHelper.Lerp(glowIntensity, 0.4f, 0.1f);
        }
        else if (!isOutput && tileEntity != null)
        {
            if (tileEntity.HasEnoughIngredient(slotIndex))
            {
                glowIntensity = MathHelper.Lerp(glowIntensity, 0.3f, 0.1f);
            }
            else
            {
                glowIntensity = MathHelper.Lerp(glowIntensity, 0f, 0.1f);
            }
        }
        else
        {
            glowIntensity = MathHelper.Lerp(glowIntensity, 0f, 0.1f);
        }
    }
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        if (tileEntity == null) return;
        CalculatedStyle dims = GetDimensions();
        Vector2 center = new Vector2(dims.X + dims.Width / 2, dims.Y + dims.Height / 2);
        Item item = isOutput ? tileEntity.OutputSlot : tileEntity.InputSlots[slotIndex];
        var recipe = tileEntity.GetCurrentRecipe();

        SlotState state = GetSlotState(item, recipe);
        DrawGlow(spriteBatch, center, dims, state);
        Texture2D slotTexture = TextureAssets.InventoryBack.Value;
        Color slotColor = GetSlotColor(state);

        if (IsMouseHovering)
        {
            slotColor = Color.Lerp(slotColor, Color.White, 0.3f);
        }
        float drawScale = scale * hoverScale * (dims.Width / slotTexture.Width);
        Vector2 slotOrigin = new Vector2(slotTexture.Width / 2f, slotTexture.Height / 2f);
        spriteBatch.Draw(slotTexture, center, null, slotColor, 0f, slotOrigin, drawScale, SpriteEffects.None, 0f);

        if (!isOutput && item.IsAir && recipe != null)
        {
            DrawGhostItem(spriteBatch, center, recipe, dims);
        }
        if (!item.IsAir)
        {
            DrawItem(spriteBatch, center, item, dims);
        }
        if (!isOutput && recipe != null)
        {
            DrawQuantityIndicator(spriteBatch, dims, item, recipe);
        }
        DrawTooltip(item, recipe);
    }
    private enum SlotState
    {
        Empty, Correct, WrongType, NotEnough, Output
    }
    private SlotState GetSlotState(Item item, PotionRecipe recipe)
    {
        if (isOutput) return SlotState.Output;
        if (item.IsAir) return SlotState.Empty;
        if (recipe == null) return SlotState.Empty;
        bool correctType = tileEntity.IsCorrectIngredientType(slotIndex);
        bool enoughCount = tileEntity.HasEnoughIngredient(slotIndex);

        if (correctType && enoughCount) return SlotState.Correct;
        if (correctType && !enoughCount) return SlotState.NotEnough;
        return SlotState.WrongType;
    }
    private Color GetSlotColor(SlotState state)
    {
        return state switch
        {
            SlotState.Correct => new Color(150, 255, 150), SlotState.WrongType => new Color(255, 150, 150), SlotState.NotEnough => new Color(255, 220, 100), SlotState.Output => new Color(255, 220, 100), _ => Color.White
        };
    }
    private void DrawGlow(SpriteBatch spriteBatch, Vector2 center, CalculatedStyle dims, SlotState state)
    {
        if (glowIntensity < 0.01f) return;
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        float glowSize = dims.Width * 1.4f;
        Rectangle glowRect = new Rectangle((int)(center.X - glowSize / 2), (int)(center.Y - glowSize / 2), (int)glowSize, (int)glowSize);
        Color glowColor = state switch
        {
            SlotState.Correct => Color.LimeGreen, SlotState.Output => Color.Gold, _ => Color.White
        };
        spriteBatch.Draw(pixel, glowRect, glowColor * glowIntensity * 0.5f);
    }
    private void DrawGhostItem(SpriteBatch spriteBatch, Vector2 center, PotionRecipe recipe, CalculatedStyle dims)
    {
        int requiredType = slotIndex == 0 ? recipe.Ingredient1 : recipe.Ingredient2;
        int requiredCount = slotIndex == 0 ? recipe.Ingredient1Count : recipe.Ingredient2Count;
        if (requiredType <= 0) return;
        Main.instance.LoadItem(requiredType);
        Texture2D itemTexture = TextureAssets.Item[requiredType].Value;
        float itemScale = 0.8f;
        if (itemTexture.Width > 28 || itemTexture.Height > 28)
        {
            itemScale = 28f / Math.Max(itemTexture.Width, itemTexture.Height);
        }
        Vector2 itemOrigin = new Vector2(itemTexture.Width / 2f, itemTexture.Height / 2f);
        float alpha = 0.3f + (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.1f;
        spriteBatch.Draw(itemTexture, center, null, Color.White * alpha, 0f, itemOrigin, itemScale, SpriteEffects.None, 0f);
        if (requiredCount > 1)
        {
            string countText = $"x{requiredCount}";
            Vector2 textPos = new Vector2(dims.X + dims.Width - 20, dims.Y + dims.Height - 16);
            Terraria.Utils.DrawBorderString(spriteBatch, countText, textPos, Color.Gray * 0.8f, 0.7f);
        }
    }
    private void DrawItem(SpriteBatch spriteBatch, Vector2 center, Item item, CalculatedStyle dims)
    {
        Main.instance.LoadItem(item.type);
        Texture2D itemTexture = TextureAssets.Item[item.type].Value;
        Rectangle frame = Main.itemAnimations[item.type] != null ? Main.itemAnimations[item.type].GetFrame(itemTexture) : itemTexture.Bounds;
        float itemScale = 1f;
        float maxSize = 32f;
        if (frame.Width > maxSize || frame.Height > maxSize)
        {
            itemScale = maxSize / Math.Max(frame.Width, frame.Height);
        }
        itemScale *= hoverScale;
        Vector2 itemOrigin = new Vector2(frame.Width / 2f, frame.Height / 2f);
        spriteBatch.Draw(itemTexture, center, frame, Color.White, 0f, itemOrigin, itemScale, SpriteEffects.None, 0f);
        if (item.stack > 1)
        {
            Vector2 stackPos = new Vector2(dims.X + 6f, dims.Y + dims.Height - 18f);
            Terraria.Utils.DrawBorderString(spriteBatch, item.stack.ToString(), stackPos, Color.White, 0.8f);
        }
    }
    private void DrawQuantityIndicator(SpriteBatch spriteBatch, CalculatedStyle dims, Item item, PotionRecipe recipe)
    {
        if (item.IsAir) return;

        int required = slotIndex == 0 ? recipe.Ingredient1Count : recipe.Ingredient2Count;
        int requiredType = slotIndex == 0 ? recipe.Ingredient1 : recipe.Ingredient2;
        if (item.type != requiredType) return;

        Color textColor = item.stack >= required ? Color.LimeGreen : Color.Yellow;
        string indicator = $"{item.stack}/{required}";
        Vector2 textPos = new Vector2(dims.X + dims.Width - 8, dims.Y + 4);

        Terraria.Utils.DrawBorderString(spriteBatch, indicator, textPos, textColor, 0.6f, 1f, 0f);
    }
    private void DrawTooltip(Item item, PotionRecipe recipe)
    {
        if (!IsMouseHovering) return;
        if (!item.IsAir)
        {
            Main.HoverItem = item.Clone();
            Main.hoverItemName = item.Name;
        }
        else if (!isOutput && recipe != null)
        {
            int requiredType = slotIndex == 0 ? recipe.Ingredient1 : recipe.Ingredient2;
            int requiredCount = slotIndex == 0 ? recipe.Ingredient1Count : recipe.Ingredient2Count;
            if (requiredType > 0)
            {
                Item tempItem = new Item();
                tempItem.SetDefaults(requiredType);
                string hint = $"{LangSystem.AlchRequires.Value} {tempItem.Name}";
                if (requiredCount > 1)
                    hint += $" x{requiredCount}";
                Main.hoverItemName = hint;
            }
        }
    }
    public override void LeftClick(UIMouseEvent evt)
    {
        if (tileEntity == null) return;
        Item playerItem = Main.mouseItem;
        Item slotItem;
        if (isOutput)
        {
            slotItem = tileEntity.OutputSlot;
            if (!slotItem.IsAir)
            {
                if (playerItem.IsAir)
                {
                    Main.mouseItem = slotItem.Clone();
                    tileEntity.OutputSlot = new Item();
                }
                else if (playerItem.type == slotItem.type && playerItem.stack < playerItem.maxStack)
                {
                    int canAdd = playerItem.maxStack - playerItem.stack;
                    int toAdd = Math.Min(canAdd, slotItem.stack);
                    playerItem.stack += toAdd;
                    slotItem.stack -= toAdd;
                    if (slotItem.stack <= 0)
                        tileEntity.OutputSlot = new Item();
                }
            }
            SoundEngine.PlaySound(SoundID.Grab);
        }
        else
        {
            slotItem = tileEntity.InputSlots[slotIndex];
            if (playerItem.IsAir && !slotItem.IsAir)
            {
                Main.mouseItem = slotItem.Clone();
                tileEntity.InputSlots[slotIndex] = new Item();
            }
            else if (!playerItem.IsAir)
            {
                if (slotItem.IsAir)
                {
                    tileEntity.InputSlots[slotIndex] = playerItem.Clone();
                    Main.mouseItem = new Item();
                }
                else if (playerItem.type == slotItem.type && slotItem.stack < slotItem.maxStack)
                {
                    int canAdd = slotItem.maxStack - slotItem.stack;
                    int toAdd = Math.Min(canAdd, playerItem.stack);
                    slotItem.stack += toAdd;
                    playerItem.stack -= toAdd;
                    if (playerItem.stack <= 0)
                        Main.mouseItem = new Item();
                }
                else
                {
                    Item temp = slotItem.Clone();
                    tileEntity.InputSlots[slotIndex] = playerItem.Clone();
                    Main.mouseItem = temp;
                }
            }
            SoundEngine.PlaySound(SoundID.Grab);
        }
        tileEntity.SendSlotSync(isOutput ? 2 : slotIndex);
    }
    public override void RightClick(UIMouseEvent evt)
    {
        if (tileEntity == null) return;
        Item slotItem = isOutput ? tileEntity.OutputSlot : tileEntity.InputSlots[slotIndex];
        if (!slotItem.IsAir && Main.mouseItem.IsAir)
        {
            int takeAmount = Math.Max(1, slotItem.stack / 2);
            Main.mouseItem = slotItem.Clone();
            Main.mouseItem.stack = takeAmount;
            slotItem.stack -= takeAmount;
            if (slotItem.stack <= 0)
            {
                if (isOutput)
                    tileEntity.OutputSlot = new Item();
                else
                    tileEntity.InputSlots[slotIndex] = new Item();
            }
            SoundEngine.PlaySound(SoundID.Grab);
        }
        else if (!isOutput && !Main.mouseItem.IsAir)
        {
            if (slotItem.IsAir || (slotItem.type == Main.mouseItem.type && slotItem.stack < slotItem.maxStack))
            {
                if (slotItem.IsAir)
                {
                    tileEntity.InputSlots[slotIndex] = Main.mouseItem.Clone();
                    tileEntity.InputSlots[slotIndex].stack = 1;
                }
                else
                {
                    slotItem.stack++;
                }
                Main.mouseItem.stack--;
                if (Main.mouseItem.stack <= 0)
                    Main.mouseItem = new Item();
                SoundEngine.PlaySound(SoundID.Grab);
            }
        }
        tileEntity.SendSlotSync(isOutput ? 2 : slotIndex);
    }
}
    #region UITextButton
    public class UITextButton : UIElement
{
    private UIText text;
    private Color normalColor = new Color(73, 94, 171);
    private Color hoverColor = new Color(100, 120, 200);
    private Color borderColor = new Color(40, 50, 100);
    public UITextButton(string label, float textScale = 1f)
    {
        text = new UIText(label, textScale);
        text.HAlign = 0.5f;
        text.VAlign = 0.5f;
        Append(text);
    }
    public UITextButton(LocalizedText label, float textScale = 1f)
    {
        text = new UIText(label, textScale);
        text.HAlign = 0.5f;
        text.VAlign = 0.5f;
        Append(text);
    }
    public void SetColor(Color color)
    {
        normalColor = color;
    }
    public void SetHoverColor(Color color)
    {
        hoverColor = color;
    }
    public void SetBorderColor(Color color)
    {
        borderColor = color;
    }
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        CalculatedStyle dimensions = GetDimensions();
        Rectangle rect = new Rectangle((int)dimensions.X, (int)dimensions.Y, (int)dimensions.Width, (int)dimensions.Height);
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Color bgColor = IsMouseHovering ? hoverColor : normalColor;

        spriteBatch.Draw(pixel, rect, bgColor);

        int borderWidth = 2;
        spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, borderWidth), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Bottom - borderWidth, rect.Width, borderWidth), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, borderWidth, rect.Height), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(rect.Right - borderWidth, rect.Y, borderWidth, rect.Height), borderColor);
    }
    public override void MouseOver(UIMouseEvent evt)
    {
        base.MouseOver(evt);
        SoundEngine.PlaySound(SoundID.MenuTick);
    }
}
    #region AlchemicalEnchantmentUISystem
    public class AlchemicalEnchantmentUISystem : ModSystem
    {
        public static AlchemicalEnchantmentUISystem Instance { get; private set; }
        private UserInterface userInterface;
        private AlchemicalEnchantmentUIState uiState;
        private bool isUIOpen = false;
        private AlchemicalEnchantmentTE currentTE;
        public override void Load()
        {
            Instance = this;
            if (!Main.dedServ)
            {
                userInterface = new UserInterface();
                uiState = new AlchemicalEnchantmentUIState();
                uiState.Activate();
            }
        }
        public override void Unload()
        {
            Instance = null;
            uiState = null;
            userInterface = null;
        }
        public void ToggleUI(AlchemicalEnchantmentTE te)
        {
            if (isUIOpen && currentTE == te)
            {
                CloseUI();
            }
            else
            {
                OpenUI(te);
            }
        }
        public void OpenUI(AlchemicalEnchantmentTE te)
        {
            currentTE = te;
            uiState.SetTileEntity(te);
            userInterface.SetState(uiState);
            isUIOpen = true;
            Main.playerInventory = true;
        }
        public void CloseUI()
        {
            userInterface.SetState(null);
            isUIOpen = false;
            currentTE = null;
        }
        public override void UpdateUI(GameTime gameTime)
        {
            if (isUIOpen)
            {
                userInterface?.Update(gameTime);
                if (currentTE != null)
                {
                    Vector2 tePos = new Vector2(currentTE.Position.X * 16 + 24, currentTE.Position.Y * 16 + 16);
                    if (Vector2.Distance(Main.LocalPlayer.Center, tePos) > 200f)
                    {
                        CloseUI();
                    }
                }
                if (Main.LocalPlayer.controlInv)
                {
                    CloseUI();
                }
            }
        }
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
            if (inventoryIndex != -1)
            {
                layers.Insert(inventoryIndex + 1, new LegacyGameInterfaceLayer("YourModName: Potion Crafter UI", delegate
                    {
                        if (isUIOpen)
                        {
                            userInterface.Draw(Main.spriteBatch, new GameTime());
                        }
                        return true;
                    }, InterfaceScaleType.UI));
            }
        }
    }
}
#endregion
#endregion
#endregion
#endregion
#endregion
#endregion
