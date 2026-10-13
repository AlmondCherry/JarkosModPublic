using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using jarkos.items;
using jarkos.core.Data;
using Terraria.UI.Chat;
using jarkos.core.PlayerInteract;
using Terraria.GameInput;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;

namespace jarkos.UI
{
    public class RuneChargerUI
    {
        private class UIStarParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public Color ParticleColor;
            public float Scale;
            public float Opacity;
            public float Rotation;
            public float RotationSpeed;
            public int Lifetime;
            public int TimeAlive;

            public bool IsDead => TimeAlive >= Lifetime || Opacity <= 0.01f;

            public UIStarParticle(Vector2 position, Vector2 velocity, Color color, int lifetime, float scale, float opacity, float rotationSpeed)
            {
                Position = position;
                Velocity = velocity;
                ParticleColor = color;
                Lifetime = lifetime;
                Scale = scale;
                Opacity = opacity;
                RotationSpeed = rotationSpeed;
                Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                TimeAlive = 0;
            }

            public void Update()
            {
                TimeAlive++;
                Position += Velocity;
                Velocity *= 0.93f;
                Rotation += RotationSpeed;
                RotationSpeed *= 0.95f;

                float progress = TimeAlive / (float)Lifetime;
                float fadeIn = Math.Min(1f, progress * 5f);
                float fadeOut = (1f - progress) * (1f - progress);
                Opacity = MathHelper.Clamp(fadeIn * fadeOut, 0f, 1f);
                Scale *= 0.988f;
            }

            public void Draw(SpriteBatch sb, Texture2D tex, float uiAlpha)
            {
                if (IsDead || tex == null) return;
                Color drawColor = ParticleColor * (Opacity * uiAlpha);
                sb.Draw(tex, Position, null, drawColor, Rotation, tex.Size() / 2f, Scale, SpriteEffects.None, 0f);
            }
        }

        private readonly CosmicBackground _bg = new();
        private readonly TreeCamera _cam = new();
        private readonly List<UIStarParticle> _particles = new();

        public bool IsOpen { get; private set; }
        public bool IsClosing { get; private set; }
        public float OpenProgress { get; private set; } = 0f;

        private BaseRuneItem _rune;
        private float _orbitAngle;
        private bool _panelOpen;
        private float _panelSlide;
        private int _selectedSlot = -1;
        private bool _confirmOpen;
        private EssenceType _selectedEssence = (EssenceType)(-1);
        private bool _sliderOpen;
        private int _donateAmount = 1;
        private int _maxDonate = 1;
        private bool _isDraggingSlider;
        private bool _isAnimating;
        private float _animTimer;
        private Vector2 _animStartPos;
        private Vector2 _animEndPos;
        private float _scrollOffset = 0f;

        public void Open(BaseRuneItem rune)
        {
            IsOpen = true;
            IsClosing = false;
            OpenProgress = 0f;

            _rune = rune;

            _orbitAngle = 0f;
            _panelOpen = false;
            _panelSlide = 0f;
            _confirmOpen = false;
            _sliderOpen = false;
            _isDraggingSlider = false;
            _isAnimating = false;
            _selectedSlot = -1;

            _particles.Clear();
            Main.playerInventory = false;
            SoundEngine.PlaySound(SoundID.MenuOpen);
        }

        public void Close()
        {
            if (!IsOpen || IsClosing) return;
            IsClosing = true;
            SoundEngine.PlaySound(SoundID.MenuClose);
        }

        public void Update(GameTime gameTime)
        {
            if (!IsOpen || _rune == null) return;
            Player player = Main.LocalPlayer;
            player.mouseInterface = true;
            PlayerInput.LockVanillaMouseScroll("jarkos:RuneChargerUI");

            if (IsClosing)
            {
                OpenProgress -= (float)gameTime.ElapsedGameTime.TotalSeconds / 0.4f;
                if (OpenProgress <= 0f)
                {
                    OpenProgress = 0f;
                    IsClosing = false;
                    IsOpen = false;
                    _rune = null;
                    _particles.Clear();
                    return;
                }
            }
            else if (OpenProgress < 1f)
            {
                OpenProgress = Math.Min(1f, OpenProgress + (float)gameTime.ElapsedGameTime.TotalSeconds / 1.2f);
            }

            if (player.dead || !player.active || player.HeldItem.ModItem is not BaseRuneItem || PlayerInput.Triggers.JustPressed.Inventory || Main.mapFullscreen)
            {
                Close();
                return;
            }

            _bg.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            _cam.Update(true);
            _orbitAngle += (float)gameTime.ElapsedGameTime.TotalSeconds * 0.5f;

            if (_panelOpen)
            {
                _panelSlide = MathHelper.Lerp(_panelSlide, 1f, 0.1f);
                int scroll =PlayerInput.ScrollWheelDeltaForUI;
                if (scroll != 0)
                {
                    _scrollOffset -= Math.Sign(scroll) * 35f;
                }
            }
            else
            {
                _panelSlide = MathHelper.Lerp(_panelSlide, 0f, 0.1f);
            }

            if (_isAnimating)
            {
                _animTimer += (float)gameTime.ElapsedGameTime.TotalSeconds * 1.5f;
                float easedAnim = MathSystem.EaseOutCubic(MathHelper.Clamp(_animTimer, 0f, 1f));
                Vector2 currentFlyingPos = Vector2.Lerp(_animStartPos, _animEndPos, easedAnim);

                if (Main.rand.NextBool(2))
                {
                    Vector2 trailVel = Main.rand.NextVector2Circular(1.2f, 1.2f);
                    Color essenceColor = EssencePalette.ColorOf(_selectedEssence);
                    _particles.Add(new UIStarParticle(currentFlyingPos, trailVel, essenceColor, 28, Main.rand.NextFloat(0.18f, 0.3f), 0.85f, Main.rand.NextFloat(-0.03f, 0.03f)));
                }

                if (_animTimer >= 1f)
                {
                    _isAnimating = false;
                    if (_selectedSlot >= 0 && _selectedSlot < _rune.MaxSlots)
                    {
                        if (_rune.Types[_selectedSlot] == (EssenceType)(-1) || _rune.Types[_selectedSlot] == _selectedEssence)
                        {
                            _rune.Types[_selectedSlot] = _selectedEssence;
                            _rune.Amounts[_selectedSlot] += _donateAmount;
                        }
                    }
                    player.GetModPlayer<ResearchPlayer>().Essences[_selectedEssence] -= _donateAmount;

                    Color essenceColor = EssencePalette.ColorOf(_selectedEssence);
                    for (int i = 0; i < 26; i++)
                    {
                        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                        float speed = Main.rand.NextFloat(1.5f, 5.5f);

                        Vector2 vel = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * speed;

                        float scale = Main.rand.NextFloat(0.22f, 0.48f);
                        float rotSpeed = Main.rand.NextFloat(-0.03f, 0.03f);
                        int life = Main.rand.Next(40, 65);

                        _particles.Add(new UIStarParticle(_animEndPos, vel, essenceColor, life, scale, 1f, rotSpeed));
                    }

                    _panelOpen = false;
                    _confirmOpen = false;
                    _sliderOpen = false;
                    _selectedSlot = -1;
                }
            }

            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                _particles[i].Update();
                if (_particles[i].IsDead)
                {
                    _particles.RemoveAt(i);
                }
            }
        }

        public void Draw(SpriteBatch sb)
        {
            if (!IsOpen || _rune == null) return;

            float eased = MathSystem.EaseOutCubic(OpenProgress);
            float offsetY = (1f - eased) * (Main.screenHeight + 300f);
            float alpha = OpenProgress;

            _bg.Draw(sb, _cam, 2, alpha);

            Vector2 center = new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f + offsetY);
            Texture2D runeTex = TextureAssets.Item[_rune.Item.type].Value;
            sb.Draw(runeTex, center, null, Color.White * alpha, 0f, runeTex.Size() / 2f, 1f, SpriteEffects.None, 0f);

            float r = 150f;
            for (int i = 0; i < _rune.MaxSlots; i++)
            {
                float angle = _orbitAngle + (i * MathHelper.TwoPi / _rune.MaxSlots);
                Vector2 pos = center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * r;
                DrawSlot(sb, pos, _rune.Types[i], _rune.Amounts[i], i, alpha);
            }

            if (_panelSlide > 0.01f)
            {
                DrawEssencePanel(sb, offsetY, alpha);
            }

            if (_confirmOpen) DrawConfirmPopup(sb, center, alpha);
            else if (_sliderOpen) DrawSliderPopup(sb, center, alpha);

            if (_isAnimating)
            {
                float easedAnim = MathSystem.EaseOutCubic(MathHelper.Clamp(_animTimer, 0f, 1f));
                Vector2 animPos = Vector2.Lerp(_animStartPos + new Vector2(0, offsetY), _animEndPos + new Vector2(0, offsetY), easedAnim);
                Texture2D essenceTex = EssencePalette.GetTexture(_selectedEssence);
                if (essenceTex != null)
                {
                    float rot = _animTimer * MathHelper.TwoPi * 3f;
                    float scale = 1.2f - easedAnim * 0.3f;
                    sb.Draw(essenceTex, animPos, null, Color.White * alpha, rot, essenceTex.Size() / 2f, scale, SpriteEffects.None, 0f);
                }
            }

            if (_particles.Count > 0)
            {
                Texture2D starTex = null;
                if (ModContent.HasAsset("jarkos/particles/StarParticle"))
                {
                    starTex = ModContent.Request<Texture2D>("jarkos/particles/StarParticle").Value;
                }
                if (starTex == null && ModContent.HasAsset("jarkos/UI/Textures/StarMedium"))
                {
                    starTex = ModContent.Request<Texture2D>("jarkos/UI/Textures/StarMedium").Value;
                }

                if (starTex != null)
                {
                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

                    foreach (var p in _particles)
                    {
                        p.Draw(sb, starTex, alpha);
                    }

                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
                }
            }

            ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, "Press ESC to close", new Vector2(20, 20 + offsetY), Color.White * alpha, 0f, Vector2.Zero, Vector2.One);
        }

        private void DrawSlot(SpriteBatch sb, Vector2 pos, EssenceType type, int amount, int slotIndex, float alpha)
        {
            Texture2D slotTex = ModContent.Request<Texture2D>("jarkos/UI/RuneSlot").Value;

            bool hover = !IsClosing && Vector2.Distance(Main.MouseScreen, pos) < slotTex.Width / 2f * 1.5f;
            float scale = hover ? 1.7f : 1.5f;

            sb.Draw(slotTex, pos, null, Color.White * alpha, 0f, slotTex.Size() / 2f, scale, SpriteEffects.None, 0f);

            if (type != (EssenceType)(-1) && amount > 0)
            {
                Texture2D eTex = EssencePalette.GetTexture(type);
                if (eTex != null)
                {
                    sb.Draw(eTex, pos, null, Color.White * alpha, 0f, eTex.Size() / 2f, scale, SpriteEffects.None, 0f);
                    ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, amount.ToString(), pos + new Vector2(10, 10), Color.White * alpha, 0f, Vector2.Zero, Vector2.One);
                }
            }

            if (!IsClosing && hover && !_confirmOpen && !_sliderOpen && !_isAnimating)
            {
                Main.LocalPlayer.mouseInterface = true;
                if (Main.mouseLeft && Main.mouseLeftRelease)
                {
                    if (_rune.TotalEssence >= _rune.MaxCapacity)
                    {
                        Main.NewText("The rune is full!", Color.Red);
                    }
                    else
                    {
                        _selectedSlot = slotIndex;
                        _panelOpen = true;
                        SoundEngine.PlaySound(SoundID.MenuTick);
                    }
                }
            }
        }

        private void DrawEssencePanel(SpriteBatch sb, float offsetY, float alpha)
        {
            float panelWidth = 300f;
            float panelX = Main.screenWidth - (panelWidth * _panelSlide);
            Rectangle panelRect = new Rectangle((int)panelX, (int)(50 + offsetY), (int)panelWidth, Main.screenHeight - 100);

            Terraria.Utils.DrawInvBG(sb, panelRect, new Color(20, 24, 42, (int)(235 * alpha)));
            if (!IsClosing && panelRect.Contains(Main.MouseScreen.ToPoint()))
                Main.LocalPlayer.mouseInterface = true;

            var player = Main.LocalPlayer.GetModPlayer<ResearchPlayer>();
            var availableList = new List<KeyValuePair<EssenceType, int>>();
            foreach (var kvp in player.Essences)
            {
                if (kvp.Value > 0) availableList.Add(kvp);
            }

            float totalContentHeight = availableList.Count * 50;
            float maxScroll = Math.Max(0f, totalContentHeight - (panelRect.Height - 40));
            _scrollOffset = MathHelper.Clamp(_scrollOffset, 0f, maxScroll);

            float startY = 80 + offsetY - _scrollOffset;
            string hoveredBonusText = null;
            Color hoveredColor = Color.White;

            Rectangle oldScissor = sb.GraphicsDevice.ScissorRectangle;
            sb.End();

            RasterizerState rasterizerState = new RasterizerState
            {
                CullMode = CullMode.None,
                ScissorTestEnable = true
            };

            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, rasterizerState, null, Main.UIScaleMatrix);
            sb.GraphicsDevice.ScissorRectangle = Rectangle.Intersect(panelRect, sb.GraphicsDevice.Viewport.Bounds);

            foreach (var kvp in availableList)
            {
                EssenceType slotType = _selectedSlot >= 0 && _selectedSlot < _rune.MaxSlots ? _rune.Types[_selectedSlot] : (EssenceType)(-1);

                bool canSelect = slotType == (EssenceType)(-1) || slotType == kvp.Key;

                Rectangle row = new Rectangle((int)panelX + 20, (int)startY, (int)panelWidth - 40, 40);
                bool hover = !IsClosing && row.Contains(Main.MouseScreen.ToPoint()) && panelRect.Contains(Main.MouseScreen.ToPoint());

                sb.Draw(TextureAssets.MagicPixel.Value, row, (hover && canSelect ? Color.Gray * 0.5f : Color.Transparent) * alpha);

                Color drawColor = (canSelect ? Color.White : Color.Gray * 0.5f) * alpha;
                Texture2D eTex = EssencePalette.GetTexture(kvp.Key);

                if (eTex != null)
                    sb.Draw(eTex, new Vector2(row.X + 20, row.Y + 20), null, drawColor, 0f, eTex.Size() / 2f, 1f, SpriteEffects.None, 0f);

                ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, $"{EssencePalette.DisplayName(kvp.Key)} ({kvp.Value})", new Vector2(row.X + 50, row.Y + 10), drawColor, 0f, Vector2.Zero, Vector2.One);

                if (!IsClosing && hover && canSelect && Main.mouseLeft && Main.mouseLeftRelease && !_confirmOpen && !_sliderOpen)
                {
                    _selectedEssence = kvp.Key;
                    _confirmOpen = true;
                    SoundEngine.PlaySound(SoundID.MenuTick);
                }
                if (!IsClosing && hover && !_confirmOpen && !_sliderOpen)
                {
                    hoveredBonusText = EssencePalette.GetBonusText(kvp.Key, 1);
                    hoveredColor = EssencePalette.ColorOf(kvp.Key);
                }
                startY += 50;
            }

            sb.End();
            sb.GraphicsDevice.ScissorRectangle = oldScissor;
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            if (!string.IsNullOrEmpty(hoveredBonusText))
            {
                string prefix = "per 1 pc:";

                string text = $"{prefix} {hoveredBonusText}";
                var font = FontAssets.MouseText.Value;

                float maxWidth = 300f;

                string[] words = text.Split(' ');

                string wrappedText = "";
                string currentLine = "";

                foreach (string word in words)
                {
                    if (font.MeasureString(currentLine + word).X > maxWidth)
                    {
                        wrappedText += currentLine + "\n";
                        currentLine = word + " ";
                    }
                    else
                    {
                        currentLine += word + " ";
                    }
                }
                wrappedText += currentLine.TrimEnd();
                
                Vector2 textSize = font.MeasureString(wrappedText);
                Vector2 mPos = new Vector2(Main.mouseX + 16, Main.mouseY + 16);

                if (mPos.X + textSize.X > Main.screenWidth)
                {
                    mPos.X = Main.mouseX - textSize.X - 16;
                }
                if (mPos.Y + textSize.Y > Main.screenHeight)
                {
                    mPos.Y = Main.screenHeight - textSize.Y - 16;
                }
                ChatManager.DrawColorCodedStringWithShadow(sb, font, wrappedText, mPos, hoveredColor * alpha, 0f, Vector2.Zero, Vector2.One);
            }
        }

        private void DrawConfirmPopup(SpriteBatch sb, Vector2 center, float alpha)
        {
            int w = 400, h = 200;
            Rectangle rect = new Rectangle((int)center.X - w / 2, (int)center.Y - h / 2, w, h);
            
            Terraria.Utils.DrawInvBG(sb, rect, new Color(20, 24, 42, (int)(235 * alpha)));

            if (!IsClosing && rect.Contains(Main.MouseScreen.ToPoint()))
                Main.LocalPlayer.mouseInterface = true;

            ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, "Are you sure you want to transfer\nessence to this rune? (Irreversible)", new Vector2(rect.X + 20, rect.Y + 30), Color.White * alpha, 0f, Vector2.Zero, Vector2.One);

            Rectangle btnYes = new Rectangle(rect.X + 50, rect.Y + 120, 100, 40);
            Rectangle btnNo = new Rectangle(rect.X + 250, rect.Y + 120, 100, 40);
            bool hoverYes = !IsClosing && btnYes.Contains(Main.MouseScreen.ToPoint());
            bool hoverNo = !IsClosing && btnNo.Contains(Main.MouseScreen.ToPoint());

            Terraria.Utils.DrawInvBG(sb, btnYes, hoverYes ? new Color(40, 180, 80, (int)(230 * alpha)) : new Color(20, 110, 50, (int)(200 * alpha)));
            Terraria.Utils.DrawInvBG(sb, btnNo, hoverNo ? new Color(180, 40, 40, (int)(230 * alpha)) : new Color(100, 30, 30, (int)(200 * alpha)));

            ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, "Yes", new Vector2(btnYes.X + 30, btnYes.Y + 10), Color.White * alpha, 0f, Vector2.Zero, Vector2.One);
            ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, "No", new Vector2(btnNo.X + 35, btnNo.Y + 10), Color.White * alpha, 0f, Vector2.Zero, Vector2.One);

            if (!IsClosing && Main.mouseLeft && Main.mouseLeftRelease)
            {
                if (hoverYes)
                {
                    _confirmOpen = false;
                    _sliderOpen = true;
                    _maxDonate = Math.Min(_rune.MaxCapacity - _rune.TotalEssence, Main.LocalPlayer.GetModPlayer<ResearchPlayer>().Essences[_selectedEssence]);
                    if (_maxDonate < 1) _maxDonate = 1;
                    _donateAmount = 1;
                    SoundEngine.PlaySound(SoundID.MenuTick);
                }
                else if (hoverNo)
                {
                    _confirmOpen = false;
                    _panelOpen = false;
                    SoundEngine.PlaySound(SoundID.MenuClose);
                }
            }
        }

        private void DrawSliderPopup(SpriteBatch sb, Vector2 center, float alpha)
        {
            int w = 440, h = 230;
            Rectangle rect = new Rectangle((int)center.X - w / 2, (int)center.Y - h / 2, w, h);
            Terraria.Utils.DrawInvBG(sb, rect, new Color(20, 24, 42, (int)(235 * alpha)));

            if (!IsClosing && rect.Contains(Main.MouseScreen.ToPoint()))
                Main.LocalPlayer.mouseInterface = true;

            Color essenceColor = EssencePalette.ColorOf(_selectedEssence);

            ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, $"How much to donate? ({_donateAmount} / {_maxDonate})", new Vector2(rect.X + 20, rect.Y + 20), Color.White * alpha, 0f, Vector2.Zero, Vector2.One);

            string bonusText = EssencePalette.GetBonusText(_selectedEssence, _donateAmount);
            string essenceBonusLine = $"{EssencePalette.DisplayName(_selectedEssence)}: {bonusText}";

            Vector2 bonusSize = FontAssets.MouseText.Value.MeasureString(essenceBonusLine) * 0.9f;
            Vector2 bonusPos = new Vector2(rect.Center.X - bonusSize.X / 2f, rect.Y + 55);
            ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, essenceBonusLine, bonusPos, essenceColor * alpha, 0f, Vector2.Zero, new Vector2(0.9f));

            Rectangle sliderBar = new Rectangle(rect.X + 50, rect.Y + 115, 340, 16);
            Rectangle sliderHitbox = new Rectangle(sliderBar.X - 15, sliderBar.Y - 15, sliderBar.Width + 30, sliderBar.Height + 30);

            if (!IsClosing && Main.mouseLeft)
            {
                if (_isDraggingSlider || (sliderHitbox.Contains(Main.MouseScreen.ToPoint()) && Main.mouseLeftRelease))
                {
                    _isDraggingSlider = true;
                }
            }
            else
            {
                _isDraggingSlider = false;
            }

            int knobWidth = 14;
            int knobHeight = 22;
            int usableWidth = sliderBar.Width - knobWidth;

            if (_isDraggingSlider && _maxDonate > 1)
            {
                float relativeX = Main.MouseScreen.X - (sliderBar.X + knobWidth / 2f);
                float pctDrag = MathHelper.Clamp(relativeX / usableWidth, 0f, 1f);
                _donateAmount = 1 + (int)Math.Round(pctDrag * (_maxDonate - 1));
                _donateAmount = (int)MathHelper.Clamp(_donateAmount, 1, _maxDonate);
            }

            float pct = (_maxDonate > 1) ? (float)(_donateAmount - 1) / (_maxDonate - 1) : 1f;

            Terraria.Utils.DrawInvBG(sb, sliderBar, new Color(20, 24, 40, (int)(220 * alpha)));

            int fillWidth = (int)(pct * (sliderBar.Width - 6));
            if (fillWidth > 0)
            {
                Rectangle fillRect = new Rectangle(sliderBar.X + 3, sliderBar.Y + 3, fillWidth, sliderBar.Height - 6);
                sb.Draw(TextureAssets.MagicPixel.Value, fillRect, essenceColor * (0.85f * alpha));
            }

            int knobX = (int)(sliderBar.X + 2 + pct * (usableWidth - 4));
            knobX = (int)MathHelper.Clamp(knobX, sliderBar.X + 2, sliderBar.Right - knobWidth - 2);

            Rectangle knobRect = new Rectangle(knobX, sliderBar.Y - 3, knobWidth, knobHeight);
            bool hoverKnob = !IsClosing && (knobRect.Contains(Main.MouseScreen.ToPoint()) || _isDraggingSlider);

            Color knobFill = hoverKnob ? Color.Lerp(essenceColor, Color.White, 0.4f) * alpha : essenceColor * alpha;
            Terraria.Utils.DrawInvBG(sb, knobRect, knobFill);

            Rectangle btnCancel = new Rectangle(rect.X + 60, rect.Y + 165, 130, 40);
            Rectangle btnDonate = new Rectangle(rect.X + 250, rect.Y + 165, 130, 40);

            bool hoverCancel = !IsClosing && !_isDraggingSlider && btnCancel.Contains(Main.MouseScreen.ToPoint());
            bool hoverDonate = !IsClosing && !_isDraggingSlider && btnDonate.Contains(Main.MouseScreen.ToPoint());

            Terraria.Utils.DrawInvBG(sb, btnCancel, hoverCancel ? new Color(180, 40, 40, (int)(230 * alpha)) : new Color(100, 30, 30, (int)(200 * alpha)));
            Terraria.Utils.DrawInvBG(sb, btnDonate, hoverDonate ? new Color(220, 160, 30, (int)(230 * alpha)) : new Color(140, 100, 20, (int)(200 * alpha)));

            Vector2 cancelSize = FontAssets.MouseText.Value.MeasureString("Cancel");
            Vector2 donateSize = FontAssets.MouseText.Value.MeasureString("Donate");

            ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, "Cancel", new Vector2(btnCancel.Center.X - cancelSize.X / 2f, btnCancel.Center.Y - cancelSize.Y / 2f), Color.White * alpha, 0f, Vector2.Zero, Vector2.One);
            ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, "Donate", new Vector2(btnDonate.Center.X - donateSize.X / 2f, btnDonate.Center.Y - donateSize.Y / 2f), Color.White * alpha, 0f, Vector2.Zero, Vector2.One);

            if (!IsClosing && !_isDraggingSlider && Main.mouseLeft && Main.mouseLeftRelease)
            {
                if (hoverCancel)
                {
                    _sliderOpen = false;
                    _confirmOpen = false;
                    _panelOpen = false;
                    _selectedSlot = -1;

                    SoundEngine.PlaySound(SoundID.MenuClose);
                }
                else if (hoverDonate)
                {
                    _sliderOpen = false;
                    _isAnimating = true;
                    _animTimer = 0f;
                    _animStartPos = new Vector2(Main.screenWidth - 150, 100);
                    
                    float r = 150f;
                    float angle = _orbitAngle + (_selectedSlot * MathHelper.TwoPi / _rune.MaxSlots);
                    _animEndPos = center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * r;

                    SoundEngine.PlaySound(SoundID.Item4);
                }
            }
        }
    }
}
