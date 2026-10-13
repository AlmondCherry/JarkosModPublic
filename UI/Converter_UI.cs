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
using Jarkos.tiles;
using Terraria.DataStructures;
using System;
using Jarkos.items.materials;
using Microsoft.Xna.Framework.Input;

namespace Jarkos.UI
{
	public class Converter_UI : UIState
    {
    private TexturedUIPanel mainPanel;
    private EnergyConverterTE tileEntity;
    private Point16 tileEntityPosition;
    private ConverterItemSlot inputSlot;
    private ConverterItemSlot outputSlot;
    private TexturedConverterBar progressBar;
    private int previousOutputStack = 0;
    private int previousOutputType = 0;
    private float previousProgress = 0f;
    private int suckingParticleTimer = 0;
    private List<UIParticleConverter> particles = new List<UIParticleConverter>();
    public void SetTileEntity(EnergyConverterTE te)
    {
        tileEntity = te;
        tileEntityPosition = te?.Position ?? default;
        if (te != null && te.OutputSlot != null && !te.OutputSlot.IsAir)
        {
            previousOutputStack = te.OutputSlot.stack;
            previousOutputType = te.OutputSlot.type;
        }
        else
        {
            previousOutputStack = 0;
            previousOutputType = 0;
        }
        previousProgress = 0f;
        particles.Clear();
    }
    public EnergyConverterTE GetTileEntity()
    {
        return tileEntity;
    }
    public override void OnInitialize()
    {
        mainPanel = new TexturedUIPanel();
        mainPanel.Width.Set(320f, 0f);
        mainPanel.Height.Set(180f, 0f);
        mainPanel.HAlign = 0.5f;
        mainPanel.VAlign = 0.5f;
        Append(mainPanel);

        float groupStartX = 16f;
        float slotY = 60f;

        inputSlot = new ConverterItemSlot(this, SlotType.Input);
        inputSlot.Left.Set(groupStartX, 0f);
        inputSlot.Top.Set(slotY, 0f);
        mainPanel.Append(inputSlot);

        progressBar = new TexturedConverterBar(this);
        progressBar.Left.Set(groupStartX + 70f, 0f);
        progressBar.Top.Set(slotY + 13f, 0f);
        progressBar.Width.Set(160f, 0f);
        progressBar.Height.Set(20f, 0f);
        progressBar.FillPaddingLeft = 3;
        progressBar.FillPaddingRight = 3;
        progressBar.FillPaddingTop = 3;
        progressBar.FillPaddingBottom = 3;
        progressBar.ShowPercentText = true;
        mainPanel.Append(progressBar);

        outputSlot = new ConverterItemSlot(this, SlotType.Output);
        outputSlot.Left.Set(groupStartX + 240f, 0f);
        outputSlot.Top.Set(slotY, 0f);
        mainPanel.Append(outputSlot);

        UIText closeButton = new UIText("X", 0.9f);
        closeButton.Left.Set(-35f, 1f);
        closeButton.Top.Set(8f, 0f);
        closeButton.TextColor = new Color(255, 100, 100);
        closeButton.OnLeftClick += (evt, elem) => { ModContent.GetInstance<EnergyConverterUISystem>().CloseUI(); };
        closeButton.OnMouseOver += (evt, elem) => { ((UIText)elem).TextColor = Color.Red; };
        closeButton.OnMouseOut += (evt, elem) => { ((UIText)elem).TextColor = new Color(255, 100, 100); };
        mainPanel.Append(closeButton);

        UIText inputLabel = new UIText("Input", 0.7f);
        inputLabel.Left.Set(groupStartX + 10f, 0f);
        inputLabel.Top.Set(slotY + 55f, 0f);
        inputLabel.TextColor = new Color(150, 150, 255);
        mainPanel.Append(inputLabel);

        UIText outputLabel = new UIText("Output", 0.7f);
        outputLabel.Left.Set(groupStartX + 232f, 0f);
        outputLabel.Top.Set(slotY + 55f, 0f);
        outputLabel.TextColor = new Color(255, 150, 150);
        mainPanel.Append(outputLabel);
    }
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        if (tileEntityPosition != default && TileEntity.ByPosition.TryGetValue(tileEntityPosition, out TileEntity currentTE) && currentTE is EnergyConverterTE freshTE)
        {
            tileEntity = freshTE;
        }

        if (tileEntity == null)
        {
            ModContent.GetInstance<EnergyConverterUISystem>().CloseUI();
            return;
        }

        float progress = (float)tileEntity.ConversionProgress / EnergyConverterTE.MaxProgress;
        progressBar.SetProgress(progress);
        
        if (progress > 0f && progress < 1f)
        {
            SpawnSuckingParticles();
            SpawnProgressBarParticles();
        }
        previousProgress = progress;
        CheckOutputSlotForNewItem();
        UpdateParticles();

        Player player = Main.LocalPlayer;
        Vector2 teWorldPos = new Vector2(tileEntity.Position.X * 16 + 24, tileEntity.Position.Y * 16 + 32);
        float distance = Vector2.Distance(player.Center, teWorldPos);

        if (distance > 200f)
        {
            ModContent.GetInstance<EnergyConverterUISystem>().CloseUI();
            return;
        }

        if (Main.keyState.IsKeyDown(Keys.Escape))
        {
            ModContent.GetInstance<EnergyConverterUISystem>().CloseUI();
        }

        if (mainPanel.ContainsPoint(Main.MouseScreen))
        {
            Main.LocalPlayer.mouseInterface = true;
        }
    }
    private void SpawnSuckingParticles()
    {
        suckingParticleTimer++;

        if (suckingParticleTimer < 4) return;
        suckingParticleTimer = 0;

        if (outputSlot == null) return;

        CalculatedStyle dims = outputSlot.GetDimensions();

        Vector2 slotCenter = dims.Center();
        int count = Main.rand.Next(1, 3);

        for (int i = 0; i < count; i++)
        {
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            float distance = Main.rand.NextFloat(40f, 80f);

            Vector2 spawnPos = slotCenter + new Vector2((float)Math.Cos(angle) * distance, (float)Math.Sin(angle) * distance);

            UIParticleConverter particle = new UIParticleConverter
            {
                Position = spawnPos, 
                Velocity = Vector2.Zero, 
                Scale = Main.rand.NextFloat(4f, 16f), 
                Alpha = 0.8f, 
                Lifetime = 120, 
                CurrentLife = 0, 
                Color = Main.rand.Next(4) switch
                {
                    0 => new Color(255, 80, 80), 1 => new Color(255, 150, 50), 2 => new Color(200, 50, 50), _ => new Color(255, 200, 100)
                }, 
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi), 
                RotationSpeed = Main.rand.NextFloat(-0.1f, 0.1f), 
                IsSucking = true, 
                TargetPosition = slotCenter, 
                SuckStrength = Main.rand.NextFloat(0.15f, 0.3f)
            };
            particles.Add(particle);
        }
    }
    private void SpawnProgressBarParticles()
    {
        if (Main.rand.Next(6) != 0) return;
        if (progressBar == null) return;

        CalculatedStyle dims = progressBar.GetDimensions();
        float progress = progressBar.GetProgress();

        if (progress <= 0) return;

        float fillEdgeX = dims.X + dims.Width * progress;
        float centerY = dims.Y + dims.Height / 2f;
        int count = Main.rand.Next(1, 3);

        for (int i = 0; i < count; i++)
        {
            Vector2 spawnPos = new Vector2(fillEdgeX + Main.rand.NextFloat(-3f, 3f), centerY + Main.rand.NextFloat(-dims.Height / 3f, dims.Height / 3f));
            UIParticleConverter particle = new UIParticleConverter
            {
                Position = spawnPos, 
                Velocity = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-1f, -0.3f)), 
                Scale = Main.rand.NextFloat(1f, 2f), 
                Alpha = 0.7f,
                Lifetime = Main.rand.Next(15, 30),
                CurrentLife = 0,
                Color = Main.rand.Next(3) switch
                {
                    0 => new Color(255, 100, 100), 1 => new Color(255, 180, 80), _ => new Color(255, 220, 150)
                }, 
                Rotation = 0,
                RotationSpeed = Main.rand.NextFloat(-0.1f, 0.1f)
            };
            particles.Add(particle);
        }
    }
    private void CheckOutputSlotForNewItem()
    {
        if (tileEntity?.OutputSlot == null) return;
        Item output = tileEntity.OutputSlot;

        int currentStack = output.IsAir ? 0 : output.stack;
        int currentType = output.IsAir ? 0 : output.type;
        bool newItemCreated = false;

        if (currentType != 0 && currentStack > 0)
        {
            if (previousOutputType == 0 || previousOutputStack == 0)
            {
                newItemCreated = true;
            }
            else if (currentType == previousOutputType && currentStack > previousOutputStack)
            {
                newItemCreated = true;
            }
        }

        if (newItemCreated)
        {
            SpawnOutputParticles();
            SoundEngine.PlaySound(SoundID.Item4);
        }

        previousOutputStack = currentStack;
        previousOutputType = currentType;
    }
    private void SpawnOutputParticles()
    {
        if (outputSlot == null) return;
        CalculatedStyle dims = outputSlot.GetDimensions();
        Vector2 center = dims.Center();

        int explosionCount = 25;

        for (int i = 0; i < explosionCount; i++)
        {
            float angle = MathHelper.TwoPi * i / explosionCount + Main.rand.NextFloat(-0.3f, 0.3f);
            float speed = Main.rand.NextFloat(2f, 5f);

            UIParticleConverter particle = new UIParticleConverter
            {
                Position = center + new Vector2(Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f)), 
                Velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed, 
                Scale = Main.rand.NextFloat(8f, 16f), 
                Alpha = 1f, 
                Lifetime = Main.rand.Next(25, 50), 
                CurrentLife = 0, 
                Color = Main.rand.Next(5) switch
                {
                    0 => new Color(255, 50, 50), 1 => new Color(255, 200, 50), 2 => new Color(255, 255, 200), 3 => new Color(255, 100, 50), _ => new Color(255, 150, 100)
                }, 
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi), 
                RotationSpeed = Main.rand.NextFloat(-0.3f, 0.3f)
            };
            particles.Add(particle);
        }

        int sparkCount = 15;

        for (int i = 0; i < sparkCount; i++)
        {
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            float speed = Main.rand.NextFloat(4f, 8f);

            UIParticleConverter Spark = new UIParticleConverter
            {
                Position = center, 
                Velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed, 
                Scale = Main.rand.NextFloat(1f, 2f), 
                Alpha = 1f, 
                Lifetime = Main.rand.Next(10, 20), 
                CurrentLife = 0, 
                Color = new Color(255, 255, 220),
                Rotation = 0,
                RotationSpeed = 0
            };
            particles.Add(Spark);
        }

        for (int i = 0; i < 12; i++)
        {
            UIParticleConverter flash = new UIParticleConverter
            {
                Position = center + new Vector2(Main.rand.NextFloat(-8f, 8f), Main.rand.NextFloat(-8f, 8f)), 
                Velocity = Vector2.Zero, 
                Scale = Main.rand.NextFloat(8f, 15f), 
                Alpha = 0.9f, 
                Lifetime = 20, 
                CurrentLife = 0, 
                Color = Color.White,
                Rotation = 0,
                RotationSpeed = 0,
                IsFlash = true
            };

            particles.Add(flash);
        }

        for (int i = 0; i < 20; i++)
        {
            float angle = MathHelper.TwoPi * i / 20f;
            float dist = 5f;

            UIParticleConverter ring = new UIParticleConverter
            {
                Position = center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * dist,
                Velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 3f,
                Scale = 3f,
                Alpha = 1f,
                Lifetime = 25,
                CurrentLife = 0,
                Color = new Color(255, 220, 150),
                Rotation = 0,
                RotationSpeed = 0
            };
            particles.Add(ring);
        }
    }
    private void UpdateParticles()
    {
        for (int i = particles.Count - 1; i >= 0; i--)
        {
            if (!particles[i].Update())
            {
                particles.RemoveAt(i);
            }
        }
    }
    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);
        DrawParticles(spriteBatch);
    }
    private void DrawParticles(SpriteBatch spriteBatch)
    {
        if (particles.Count == 0) return;
        Texture2D pixel = TextureAssets.MagicPixel.Value;

        foreach (var particle in particles)
        {
            Color drawColor = particle.Color * particle.Alpha;
            if (particle.IsFlash)
            {
                float flashScale = particle.Scale * (1f - (float)particle.CurrentLife / particle.Lifetime);
                Rectangle rect = new Rectangle((int)(particle.Position.X - flashScale), (int)(particle.Position.Y - flashScale), (int)(flashScale * 2), (int)(flashScale * 2));

                spriteBatch.Draw(pixel, rect, drawColor * 0.4f);
            }
            else
            {
                spriteBatch.Draw(pixel, particle.Position, new Rectangle(0, 0, 1, 1), drawColor, particle.Rotation, new Vector2(0.5f, 0.5f), particle.Scale, SpriteEffects.None, 0f);
            }
        }
    }
    public ConverterItemSlot GetOutputSlot() => outputSlot;
    public void AddParticle(UIParticleConverter particle)
    {
        particles.Add(particle);
    }
}
public class TexturedUIPanel : UIElement
{
    public Color TintColor { get; set; } = Color.White;
    public Color FallbackBackgroundColor { get; set; } = new Color(40, 40, 80, 220);
    public Color FallbackBorderColor { get; set; } = new Color(80, 80, 120);
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

        CalculatedStyle dims = GetDimensions();
        Rectangle rect = dims.ToRectangle();

        if (UITextures.ConverterBackground != null && UITextures.ConverterBackground.IsLoaded)
        {
            Texture2D bgTexture = UITextures.ConverterBackground.Value;
            spriteBatch.Draw(bgTexture, rect, null, new Color(255, 255, 255, 155), 0f, Vector2.Zero, SpriteEffects.None, 0f);
        }
        else
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            spriteBatch.Draw(pixel, rect, FallbackBackgroundColor);

            int borderWidth = 2;

            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, rect.Width, borderWidth), FallbackBorderColor);
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Bottom - borderWidth, rect.Width, borderWidth), FallbackBorderColor);
            spriteBatch.Draw(pixel, new Rectangle(rect.X, rect.Y, borderWidth, rect.Height), FallbackBorderColor);
            spriteBatch.Draw(pixel, new Rectangle(rect.Right - borderWidth, rect.Y, borderWidth, rect.Height), FallbackBorderColor);
        }
    }
}
public class UIParticleConverter
{
    public Vector2 Position;
    public Vector2 Velocity;
    public float Scale;
    public float Alpha;
    public float Rotation;
    public float RotationSpeed;
    public Color Color;
    public int Lifetime;
    public int CurrentLife;
    public bool IsFlash = false;
    public bool IsSucking = false;
    public Vector2 TargetPosition;
    public float SuckStrength = 0.2f;
    public bool Update()
    {
        CurrentLife++;
        if (IsSucking)
        {
            Vector2 direction = TargetPosition - Position;
            float distance = direction.Length();

            if (distance > 3f)
            {
                direction.Normalize();
                Velocity += direction * SuckStrength;

                if (Velocity.Length() > 8f)
                {
                    Velocity.Normalize();
                    Velocity *= 8f;
                }
            }
            else
            {
                return false;
            }

            Scale = Main.rand.NextFloat(2f, 6.5f) + (float)Math.Sin(CurrentLife * 0.3f) * 1.5f;
            Alpha = 0.8f;
        }
        else
        {
            Velocity *= 0.92f;
            float lifeProgress = (float)CurrentLife / Lifetime;
            Alpha = 1f - lifeProgress;

            if (!IsFlash)
            {
                Scale *= 0.96f;
            }
        }

        Position += Velocity;
        Rotation += RotationSpeed;

        return CurrentLife < Lifetime;
    }
}
public enum SlotType { Input, Output }
public class ConverterItemSlot : UIElement
{
    private readonly Converter_UI parentState;
    private readonly SlotType slotType;
    private bool isHovered = false;
    public ConverterItemSlot(Converter_UI parent, SlotType type)
    {
        parentState = parent;
        slotType = type;

        Width.Set(52f, 0f);
        Height.Set(52f, 0f);
    }
    private Item GetItem()
    {
        var te = parentState.GetTileEntity();
        if (te == null) return null;
        
        return slotType == SlotType.Input ? te.InputSlot : te.OutputSlot;
    }
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        CalculatedStyle dims = GetDimensions();
        Rectangle rect = dims.ToRectangle();
        Color bgColor = slotType == SlotType.Input ? new Color(50, 50, 140, 180) : new Color(140, 50, 50, 180);
        if (isHovered)
            bgColor = Color.Lerp(bgColor, Color.White, 0.25f);

        spriteBatch.Draw(TextureAssets.MagicPixel.Value, rect, bgColor);

        DrawInnerBorder(spriteBatch, rect, new Color(100, 100, 100, 150), 1);
        DrawBorder(spriteBatch, rect, new Color(30, 30, 30), 2);
        Item item = GetItem();

        if (item != null && !item.IsAir)
        {
            Main.instance.LoadItem(item.type);
            Texture2D itemTexture = TextureAssets.Item[item.type].Value;
            Rectangle sourceRect = itemTexture.Bounds;

            if (Main.itemAnimations[item.type] != null)
                sourceRect = Main.itemAnimations[item.type].GetFrame(itemTexture);

            float scale = 1f;
            float maxSize = 36f;

            if (sourceRect.Width > maxSize || sourceRect.Height > maxSize)
                scale = maxSize / Math.Max(sourceRect.Width, sourceRect.Height);

            Vector2 pos = dims.Center();
            Vector2 origin = sourceRect.Size() / 2f;
            spriteBatch.Draw(itemTexture, pos, sourceRect, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);

            if (item.stack > 1)
            {
                string stackText = item.stack.ToString();
                Vector2 stackPos = new Vector2(rect.Right - 8, rect.Bottom - 18);

                Terraria.Utils.DrawBorderString(spriteBatch, stackText, stackPos, Color.White, 0.75f);
            }
        }
    }
    private void DrawBorder(SpriteBatch sb, Rectangle rect, Color color, int t)
    {
        Texture2D p = TextureAssets.MagicPixel.Value;

        sb.Draw(p, new Rectangle(rect.X, rect.Y, rect.Width, t), color);
        sb.Draw(p, new Rectangle(rect.X, rect.Bottom - t, rect.Width, t), color);
        sb.Draw(p, new Rectangle(rect.X, rect.Y, t, rect.Height), color);
        sb.Draw(p, new Rectangle(rect.Right - t, rect.Y, t, rect.Height), color);
    }
    private void DrawInnerBorder(SpriteBatch sb, Rectangle rect, Color color, int t)
    {
        Texture2D p = TextureAssets.MagicPixel.Value;

        sb.Draw(p, new Rectangle(rect.X + 2, rect.Y + 2, rect.Width - 4, t), color);
        sb.Draw(p, new Rectangle(rect.X + 2, rect.Bottom - 2 - t, rect.Width - 4, t), color);
        sb.Draw(p, new Rectangle(rect.X + 2, rect.Y + 2, t, rect.Height - 4), color);
        sb.Draw(p, new Rectangle(rect.Right - 2 - t, rect.Y + 2, t, rect.Height - 4), color);
    }
    public override void MouseOver(UIMouseEvent evt)
    {
        base.MouseOver(evt);
        isHovered = true;
        Item item = GetItem();
        if (item != null && !item.IsAir)
        {
            Main.hoverItemName = item.Name;
            Main.HoverItem = item.Clone();
        }
    }
    public override void MouseOut(UIMouseEvent evt)
    {
        base.MouseOut(evt);
        isHovered = false;
    }
    public override void LeftClick(UIMouseEvent evt)
    {
        base.LeftClick(evt);
        var te = parentState.GetTileEntity();
        Item slotItem = GetItem();
        
        if (slotItem == null || te == null) return;

        Item mouseItem = Main.mouseItem;
        if (slotType == SlotType.Input)
            HandleInputClick(slotItem, mouseItem);
        else
            HandleOutputClick(slotItem, mouseItem);

        te.SendSlotToServer(slotType == SlotType.Input ? 0 : 1);
    }
    private void HandleInputClick(Item slotItem, Item mouseItem)
    {
        if (mouseItem.IsAir && !slotItem.IsAir)
        {
            Main.mouseItem = slotItem.Clone();
            slotItem.TurnToAir();

            SoundEngine.PlaySound(SoundID.Grab);
        }
        else if (!mouseItem.IsAir && mouseItem.type == ModContent.ItemType<Battery>())
        {
            if (slotItem.IsAir)
            {
                slotItem.SetDefaults(mouseItem.type);
                slotItem.stack = mouseItem.stack;
                Main.mouseItem.TurnToAir();

                SoundEngine.PlaySound(SoundID.Grab);
            }
            else if (slotItem.type == mouseItem.type && slotItem.stack < slotItem.maxStack)
            {
                int space = slotItem.maxStack - slotItem.stack;
                int add = Math.Min(space, mouseItem.stack);

                slotItem.stack += add;
                mouseItem.stack -= add;

                if (mouseItem.stack <= 0)
                    Main.mouseItem.TurnToAir();

                SoundEngine.PlaySound(SoundID.Grab);
            }
        }
    }
    private void HandleOutputClick(Item slotItem, Item mouseItem)
    {
        if (!slotItem.IsAir)
        {
            if (mouseItem.IsAir)
            {
                Main.mouseItem = slotItem.Clone();
                slotItem.TurnToAir();

                SoundEngine.PlaySound(SoundID.Grab);
            }
            else if (mouseItem.type == slotItem.type && mouseItem.stack < mouseItem.maxStack)
            {
                int space = mouseItem.maxStack - mouseItem.stack;
                int add = Math.Min(space, slotItem.stack);

                Main.mouseItem.stack += add;
                slotItem.stack -= add;

                if (slotItem.stack <= 0)
                    slotItem.TurnToAir();

                SoundEngine.PlaySound(SoundID.Grab);
            }
        }
    }
    public override void RightClick(UIMouseEvent evt)
    {
        base.RightClick(evt);
        var te = parentState.GetTileEntity();
        Item slotItem = GetItem();

        if (slotItem == null || te == null) return;
        Item mouseItem = Main.mouseItem;

        if (slotType == SlotType.Input)
        {
            if (!mouseItem.IsAir && mouseItem.type == ModContent.ItemType<Battery>())
            {
                if (slotItem.IsAir)
                {
                    slotItem.SetDefaults(mouseItem.type);
                    slotItem.stack = 1;
                }
                else if (slotItem.type == mouseItem.type && slotItem.stack < slotItem.maxStack)
                {
                    slotItem.stack++;
                }
                else return;
                mouseItem.stack--;

                if (mouseItem.stack <= 0)
                    Main.mouseItem.TurnToAir();
                    
                SoundEngine.PlaySound(SoundID.Grab);
            }
            else if (mouseItem.IsAir && !slotItem.IsAir)
            {
                Main.mouseItem = slotItem.Clone();
                Main.mouseItem.stack = 1;
                slotItem.stack--;

                if (slotItem.stack <= 0)
                    slotItem.TurnToAir();

                SoundEngine.PlaySound(SoundID.Grab);
            }
        }
        else
        {
            if (mouseItem.IsAir && !slotItem.IsAir)
            {
                int take = Math.Max(1, slotItem.stack / 2);
                Main.mouseItem = slotItem.Clone();
                Main.mouseItem.stack = take;
                slotItem.stack -= take;
                
                if (slotItem.stack <= 0)
                    slotItem.TurnToAir();

                SoundEngine.PlaySound(SoundID.Grab);
            }
        }
        te.SendSlotToServer(slotType == SlotType.Input ? 0 : 1);
    }
}
    public class ConverterProgressBar : UIElement
    {
        private readonly Converter_UI parentState;
        public ConverterProgressBar(Converter_UI parent)
        {
            parentState = parent;
        }
        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            var te = parentState.GetTileEntity();
            float progress = 0f;
            if (te != null)
            {
                progress = (float)te.ConversionProgress / EnergyConverterTE.MaxProgress;
            }

            CalculatedStyle dims = GetDimensions();
            Rectangle rect = dims.ToRectangle();
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, rect, new Color(30, 30, 30, 220));
            
            if (progress > 0)
            {
                Rectangle fill = new Rectangle(rect.X + 3, rect.Y + 3, (int)((rect.Width - 6) * progress), rect.Height - 6);
                Color fillColor = Color.Lerp(new Color(150, 40, 40), new Color(255, 60, 60), progress);
                spriteBatch.Draw(TextureAssets.MagicPixel.Value, fill, fillColor);
            }

            DrawBorder(spriteBatch, rect, new Color(80, 80, 80), 2);
            string text = $"{(int)(progress * 100)}%";

            Vector2 textSize = FontAssets.MouseText.Value.MeasureString(text) * 0.8f;
            Vector2 textPos = dims.Center() - textSize / 2f;
            
            Terraria.Utils.DrawBorderString(spriteBatch, text, textPos, Color.White, 0.8f);
        }
        private void DrawBorder(SpriteBatch sb, Rectangle rect, Color color, int t)
        {
            Texture2D p = TextureAssets.MagicPixel.Value;
            
            sb.Draw(p, new Rectangle(rect.X, rect.Y, rect.Width, t), color);
            sb.Draw(p, new Rectangle(rect.X, rect.Bottom - t, rect.Width, t), color);
            sb.Draw(p, new Rectangle(rect.X, rect.Y, t, rect.Height), color);
            sb.Draw(p, new Rectangle(rect.Right - t, rect.Y, t, rect.Height), color);
        }
    }
    #region Textured Progress Bar
    public class TexturedConverterBar : UIState
	{
	private float progress = 0f;
    private Converter_UI parentUI;
    private Vector2 shakeOffset = Vector2.Zero;
    private float shakeIntensity = 0f;
    private int particleTimer = 0;
    public int FillPaddingLeft = 3;
    public int FillPaddingTop = 3;
    public int FillPaddingRight = 3;
    public int FillPaddingBottom = 3;
    public bool ShowPercentText = true;
    public TexturedConverterBar(Converter_UI parent = null)
    {
        parentUI = parent;
    }
    public void SetProgress(float value)
    {
        float oldProgress = progress;
        progress = MathHelper.Clamp(value, 0f, 1f);
        if (progress > 0f && progress < 1f)
        {
            shakeIntensity = 0.5f + progress * 0.25f;
        }
        else
        {
            shakeIntensity = 0f;
        }
    }
    public float GetProgress()
    {
        return progress;
    }
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (shakeIntensity > 0f)
        {
            shakeOffset = new Vector2(Main.rand.NextFloat(-shakeIntensity, shakeIntensity), Main.rand.NextFloat(-shakeIntensity, shakeIntensity));
            particleTimer++;
            if (particleTimer >= 4 && parentUI != null)
            {
                particleTimer = 0;
                SpawnBarParticle();
            }
        }
        else
        {
            shakeOffset = Vector2.Zero;
        }
    }
    private void SpawnBarParticle()
    {
        if (progress <= 0) return;
        CalculatedStyle dims = GetDimensions();

        float fillEdgeX = dims.X + FillPaddingLeft + (dims.Width - FillPaddingLeft - FillPaddingRight) * progress;
        float centerY = dims.Y + dims.Height / 2f;

        Vector2 spawnPos = new Vector2(fillEdgeX + Main.rand.NextFloat(-5f, 5f), centerY + Main.rand.NextFloat(-10f, 10f));

        UIParticleConverter particle = new UIParticleConverter
        {
            Position = spawnPos,
            Velocity = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), Main.rand.NextFloat(-1.2f, -0.2f)),
            Scale = Main.rand.NextFloat(1.5f, 3f),
            Alpha = 0.8f,
            Lifetime = Main.rand.Next(15, 35),
            CurrentLife = 0,
            Color = Main.rand.Next(4) switch
            {
                0 => new Color(255, 80, 80), 1 => new Color(255, 160, 60), 2 => new Color(255, 220, 100), _ => new Color(255, 120, 80)
            },
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi),
            RotationSpeed = Main.rand.NextFloat(-0.15f, 0.15f)
        };
        parentUI.AddParticle(particle);
    }
    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        CalculatedStyle dims = GetDimensions();
        Rectangle frameRect = new Rectangle((int)(dims.X + shakeOffset.X), (int)(dims.Y + shakeOffset.Y), (int)dims.Width, (int)dims.Height);

        if (UITextures.ProgressBarFrame != null && UITextures.ProgressBarFrame.IsLoaded)
        {
            Texture2D frameTexture = UITextures.ProgressBarFrame.Value;
            spriteBatch.Draw(frameTexture, frameRect, null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);
        }
        else
        {
            DrawBorder(spriteBatch, frameRect, new Color(80, 80, 80), 2);
        }

        if (progress > 0f && UITextures.ProgressBarFill != null && UITextures.ProgressBarFill.IsLoaded)
        {
            Texture2D fillTexture = UITextures.ProgressBarFill.Value;
            Rectangle fillArea = new Rectangle(frameRect.X + FillPaddingLeft, frameRect.Y + FillPaddingTop, frameRect.Width - FillPaddingLeft - FillPaddingRight, frameRect.Height - FillPaddingTop - FillPaddingBottom);

            int visibleWidth = (int)(fillArea.Width * progress);
            if (visibleWidth > 0)
            {
                float textureProgress = progress;
                Rectangle sourceRect = new Rectangle(0, 0, (int)(fillTexture.Width * textureProgress), fillTexture.Height);
                Rectangle destRect = new Rectangle(fillArea.X, fillArea.Y, visibleWidth, fillArea.Height);

                Color fillColor = Color.White;
                if (progress > 0.7f)
                {
                    float glow = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.2f + 0.1f;
                    fillColor = Color.Lerp(Color.White, new Color(255, 220, 180), glow);
                }

                spriteBatch.Draw(fillTexture, destRect, sourceRect, fillColor);
            }
        }
        else if (progress > 0f)
        {
            Rectangle fillArea = new Rectangle(frameRect.X + FillPaddingLeft, frameRect.Y + FillPaddingTop, (int)((frameRect.Width - FillPaddingLeft - FillPaddingRight) * progress), frameRect.Height - FillPaddingTop - FillPaddingBottom);
            Color fillColor = Color.Lerp(new Color(150, 40, 40), new Color(255, 60, 60), progress);

            spriteBatch.Draw(TextureAssets.MagicPixel.Value, fillArea, fillColor);
        }
        if (ShowPercentText)
        {
            string text = $"{(int)(progress * 100)}%";

            Vector2 textSize = FontAssets.MouseText.Value.MeasureString(text) * 0.75f;
            Vector2 textPos = new Vector2(frameRect.X + frameRect.Width / 2f - textSize.X / 2f, frameRect.Y + frameRect.Height / 2f - textSize.Y / 2f);

            Terraria.Utils.DrawBorderString(spriteBatch, text, textPos, Color.White, 0.75f);
        }
    }
    private void DrawBorder(SpriteBatch sb, Rectangle rect, Color color, int t)
    {
        Texture2D p = TextureAssets.MagicPixel.Value;

        sb.Draw(p, new Rectangle(rect.X, rect.Y, rect.Width, t), color);
        sb.Draw(p, new Rectangle(rect.X, rect.Bottom - t, rect.Width, t), color);
        sb.Draw(p, new Rectangle(rect.X, rect.Y, t, rect.Height), color);
        sb.Draw(p, new Rectangle(rect.Right - t, rect.Y, t, rect.Height), color);
    }
	}
	public static class UITextures
    {
        public static Asset<Texture2D> ConverterBackground { get; private set; }
        public static Asset<Texture2D> ProgressBarFill { get; private set; }
        public static Asset<Texture2D> ProgressBarFrame { get; private set; }
        public static void Load(Mod mod)
        {
            ConverterBackground = mod.Assets.Request<Texture2D>("UI/ConverterUI_BG");
            ProgressBarFill = mod.Assets.Request<Texture2D>("UI/ConverterBar_Fill");
            ProgressBarFrame = mod.Assets.Request<Texture2D>("UI/ConverterBar");
        }
        public static void Unload()
        {
            ConverterBackground = null;
            ProgressBarFill = null;
            ProgressBarFrame = null;
        }
    }
    #endregion
    #region ModSystem
    public class EnergyConverterUISystem : ModSystem
    {
        private UserInterface userInterface;
        private Converter_UI uiState;
        private bool isUIOpen = false;
        public bool IsUIOpen => isUIOpen;
        public override void Load()
        {
            if (Main.dedServ)
                return;
            userInterface = new UserInterface();
            uiState = new Converter_UI();
            uiState.Activate();
        }
        public override void Unload()
        {
            uiState = null;
            userInterface = null;
        }
        public void ToggleUI(EnergyConverterTE tileEntity)
        {
            if (isUIOpen)
            {
                CloseUI();
            }
            else
            {
                OpenUI(tileEntity);
            }
        }
        public void OpenUI(EnergyConverterTE tileEntity)
        {
            if (Main.dedServ)
                return;

            SoundEngine.PlaySound(SoundID.MenuOpen);

            uiState.SetTileEntity(tileEntity);

            isUIOpen = true;
            userInterface.SetState(uiState);
            Main.playerInventory = true;
        }
        public void CloseUI()
        {
            if (!isUIOpen)
                return;

            SoundEngine.PlaySound(SoundID.MenuClose);

            isUIOpen = false;
            userInterface.SetState(null);
        }
        public override void UpdateUI(GameTime gameTime)
        {
            if (isUIOpen && userInterface?.CurrentState != null)
            {
                userInterface.Update(gameTime);
            }
        }
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int inventoryIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
            if (inventoryIndex != -1)
            {
                layers.Insert(inventoryIndex + 1, new LegacyGameInterfaceLayer("Jarkos: Energy Converter UI", delegate
                {
                    if (isUIOpen && userInterface?.CurrentState != null)
                    {
                        userInterface.Draw(Main.spriteBatch, new GameTime());
                    }
                    return true;
                }, InterfaceScaleType.UI));
            }
        }
    }
	#endregion
}
