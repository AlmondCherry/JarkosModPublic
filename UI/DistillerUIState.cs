using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.UI;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using jarkos.tiles;
using jarkos.core.PlayerInteract;
using jarkos.core.Data;
using jarkos.UI.Common;
using Terraria.ID;
using Terraria.GameContent;
using Terraria.UI.Chat;
using Terraria.Audio;
using jarkos.systems;
using Terraria.DataStructures;
using Jarkos;
using Terraria.ModLoader.IO;

namespace jarkos.UI
{
    public class DistillerUIState : UIState
    {
        public static bool RightSlotWasLocked = true;
        private class DistillerIgnoreLaserButton : UIElement
        {
            public DistillerTileEntity Entity;
            public float Opacity = 1f;
            protected override void DrawSelf(SpriteBatch spriteBatch)
            {
                if (Entity == null) return;
                CalculatedStyle dim = GetDimensions();

                Color bgColor = Entity.IgnoreLaser ? new Color(100, 150, 100, (int)(200 * Opacity)) : new Color(150, 100, 100, (int)(200 * Opacity));

                if (IsMouseHovering) bgColor = Entity.IgnoreLaser ? new Color(130, 180, 130, (int)(200 * Opacity)) : new Color(180, 130, 130, (int)(200 * Opacity));
                Terraria.Utils.DrawInvBG(spriteBatch, dim.ToRectangle(), bgColor);

                string text = Entity.IgnoreLaser ? "Ignore: ON" : "Ignore: OFF";
                
                Terraria.Utils.DrawBorderString(spriteBatch, text, new Vector2(dim.X + dim.Width / 2f, dim.Y + dim.Height / 2f), Color.White * Opacity, 0.8f, 0.5f, 0.5f);
                if (IsMouseHovering)
                {
                    Main.hoverItemName = LangSystem.DistillerIgnoreTooltip.Value;
                }
            }
        }
        private class DistillerBarElement : UIElement
        {
            public DistillerTileEntity Entity;
            public float Opacity = 1f;
            public float CollectProgress = 1f;
            protected override void DrawSelf(SpriteBatch spriteBatch)
            {
                if (Entity == null) return;
                CalculatedStyle dims = GetDimensions();
                Terraria.Utils.DrawInvBG(spriteBatch, new Rectangle((int)dims.X - 2, (int)dims.Y - 2, (int)dims.Width + 4, (int)dims.Height + 4), new Color(40, 40, 40, (int)(200 * Opacity)));
                int total = Entity.GetTotalEssences();
                if (total == 0) return;
                float currentY = dims.Y + dims.Height;

                EssenceType? hoveredEssence = null;
                int hoveredAmount = 0;
                float time = Main.GlobalTimeWrappedHourly;

                foreach (var kvp in Entity.StoredEssences)
                {
                    if (kvp.Value <= 0) continue;
                    float segmentHeight = dims.Height * ((float)kvp.Value / DistillerTileEntity.MaxCapacity) * CollectProgress;
                    float topY = currentY - segmentHeight;
                    float bottomY = currentY;
                    Color baseColor = EssencePalette.ColorOf(kvp.Key) * Opacity;

                    if (Jarkos_config.StaticEssenceSections)
                    {
                        int sliceTop = (int)topY;
                        int sliceBottom = (int)bottomY;
                        if (sliceTop < dims.Y) sliceTop = (int)dims.Y;
                        if (sliceBottom > dims.Y + dims.Height) sliceBottom = (int)(dims.Y + dims.Height);
                        int sliceHeight = sliceBottom - sliceTop;
                        if (sliceHeight > 0)
                        {
                            Rectangle fillRect = new Rectangle((int)dims.X, sliceTop, (int)dims.Width, sliceHeight);
                            spriteBatch.Draw(TextureAssets.MagicPixel.Value, fillRect, baseColor);
                            spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle((int)dims.X, sliceTop, (int)dims.Width, Math.Min(2, sliceHeight)), Color.White * 0.25f * Opacity);
                        }
                    }
                    else
                    {
                        for (int x = (int)dims.X; x < (int)dims.X + (int)dims.Width; x += 2)
                        {
                            int sliceW = Math.Min(2, (int)(dims.X + dims.Width) - x);
                            float topWave = (float)(Math.Sin(x * 0.12f + topY * 0.05f + time * 3f) * 2.5f + Math.Cos(x * 0.07f - time * 1.5f) * 1.5f);
                            float bottomWave = (float)(Math.Sin(x * 0.12f + bottomY * 0.05f + time * 3f) * 2.5f + Math.Cos(x * 0.07f - time * 1.5f) * 1.5f);
                            if (bottomY >= dims.Y + dims.Height - 0.5f)
                                bottomWave = 0;
                            int sliceTop = (int)(topY + topWave);
                            int sliceBottom = (int)(bottomY + bottomWave);

                            if (sliceTop < dims.Y) sliceTop = (int)dims.Y;
                            if (sliceBottom > dims.Y + dims.Height) sliceBottom = (int)(dims.Y + dims.Height);
                            int sliceHeight = sliceBottom - sliceTop;

                            if (sliceHeight > 0)
                            {
                                Rectangle slice = new Rectangle(x, sliceTop, sliceW, sliceHeight);
                                spriteBatch.Draw(TextureAssets.MagicPixel.Value, slice, baseColor);
                                spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(x, sliceTop, sliceW, Math.Min(2, sliceHeight)), Color.White * 0.25f * Opacity);
                            }
                        }
                    }
                    Rectangle hoverRect = new Rectangle((int)dims.X, (int)topY, (int)dims.Width, (int)segmentHeight);
                    if (hoverRect.Contains(Main.mouseX, Main.mouseY))
                    {
                        hoveredEssence = kvp.Key;
                        hoveredAmount = kvp.Value;
                    }
                    currentY = topY;
                }
                if (hoveredEssence.HasValue)
                {
                    string text = $"{EssencePalette.DisplayName(hoveredEssence.Value)}: {hoveredAmount}";
                    Main.hoverItemName = text;
                    var tex = EssencePalette.GetTexture(hoveredEssence.Value);
                    if (tex != null)
                    {
                        spriteBatch.Draw(tex, new Vector2(Main.mouseX + 20, Main.mouseY + 20), Color.White * Opacity);
                    }
                }
            }
        }
        private class DistillerItemSlot : UIElement
        {
            public DistillerTileEntity Entity;
            public int SlotIndex;
            public enum SlotState { Unlocked, Locked, Unlocking }
            public SlotState State = SlotState.Unlocked;
            public float UnlockProgress = 0f;
            public float Opacity = 1f;
            public DistillerItemSlot(int slotIndex)
            {
                SlotIndex = slotIndex;
            }
            protected override void DrawSelf(SpriteBatch spriteBatch)
            {
                CalculatedStyle dims = GetDimensions();
                Terraria.Utils.DrawInvBG(spriteBatch, new Rectangle((int)dims.X, (int)dims.Y, (int)dims.Width, (int)dims.Height), new Color(150, 150, 150, (int)(150 * Opacity)));
                if (State != SlotState.Unlocked)
                {
                    Texture2D texLocked = null;
                    Texture2D texLock = null;
                    try { texLocked = ModContent.Request<Texture2D>("jarkos/UI/SlotLocked").Value; } catch {}
                    try { texLock = ModContent.Request<Texture2D>("jarkos/UI/LockIcon").Value; } catch {}
                    float alpha = 1f;
                    float lockYOffset = 0f;
                    float lockXOffset = 0f;
                    float lockRotation = 0f;
                    if (State == SlotState.Unlocking)
                    {
                        float t = UnlockProgress;
                        alpha = 1f - (t * t);
                        lockXOffset = 40f * t;
                        lockYOffset = -80f * t + 150f * t * t;
                        lockRotation = t * 6f;
                    }
                    if (texLocked != null)
                        spriteBatch.Draw(texLocked, dims.Center(), null, Color.White * alpha * Opacity, 0f, texLocked.Size() / 2f, 1f, SpriteEffects.None, 0f);
                    else
                        Terraria.Utils.DrawInvBG(spriteBatch, new Rectangle((int)dims.X, (int)dims.Y, (int)dims.Width, (int)dims.Height), new Color(50, 50, 50, (int)(255 * alpha * Opacity)));
                    if (texLock != null)
                        spriteBatch.Draw(texLock, dims.Center() + new Vector2(lockXOffset, lockYOffset), null, Color.White * alpha * Opacity, lockRotation, texLock.Size() / 2f, 1f, SpriteEffects.None, 0f);
                    if (State == SlotState.Locked || UnlockProgress < 0.8f) return;
                }
                if (Entity == null) return;
                Item item = Entity.InputSlots[SlotIndex];
                if (item != null && !item.IsAir)
                {
                    Main.instance.LoadItem(item.type);
                    Texture2D tex = TextureAssets.Item[item.type].Value;
                    Rectangle rect = tex.Frame(1, 1, 0, 0);
                    float scale = 1f;
                    if (rect.Width > dims.Width - 10 || rect.Height > dims.Height - 10)
                    {
                        scale = Math.Min((dims.Width - 10) / rect.Width, (dims.Height - 10) / rect.Height);
                    }
                    spriteBatch.Draw(tex, dims.Center(), rect, Color.White * Opacity, 0f, rect.Size() / 2f, scale, SpriteEffects.None, 0f);
                    if (item.stack > 1)
                    {
                        ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, item.stack.ToString(), dims.Position() + new Vector2(10, dims.Height - 20), Color.White * Opacity, 0f, Vector2.Zero, new Vector2(0.85f));
                    }
                    if (IsMouseHovering)
                    {
                        Main.hoverItemName = item.Name;
                    }
                }
            }
        }
        private class DistillerSlotProgressBar : UIElement
        {
            public DistillerTileEntity Entity;
            public int SlotIndex;
            public float Opacity = 1f;
            public DistillerSlotProgressBar(int slotIndex)
            {
                SlotIndex = slotIndex;
            }
            protected override void DrawSelf(SpriteBatch spriteBatch)
            {
                if (Entity == null) return;
                CalculatedStyle dims = GetDimensions();
                Terraria.Utils.DrawInvBG(spriteBatch, new Rectangle((int)dims.X, (int)dims.Y, (int)dims.Width, (int)dims.Height), new Color(40, 40, 40, (int)(150 * Opacity)));
                bool hasItem = Entity.InputSlots[SlotIndex] != null && !Entity.InputSlots[SlotIndex].IsAir;
                if (hasItem)
                {
                    int reqTime = 300;
                    int closestPlayerIndex = Player.FindClosest(new Vector2(Entity.Position.X * 16, Entity.Position.Y * 16), 480, 480);
                    if (closestPlayerIndex >= 0 && closestPlayerIndex < Main.maxPlayers)
                    {
                        Player closest = Main.player[closestPlayerIndex];
                        if (closest != null && closest.active)
                        {
                            var rp = closest.GetModPlayer<ResearchPlayer>();
                            if (rp.IsCompleted("jarkos:AquaNode6"))
                            {
                                reqTime = 195;
                            }
                        }
                    }
                    float progress = (float)Entity.ProcessTimers[SlotIndex] / reqTime;
                    if (progress > 1f) progress = 1f;
                    int maxFillHeight = (int)dims.Height - 4;
                    int totalFill = (int)(maxFillHeight * progress);
                    if (totalFill > 0)
                    {
                        Color fillColor = new Color(0, 255, 150, (int)(200 * Opacity));
                        int bottomY = (int)(dims.Y + dims.Height);
                        if (totalFill >= 1)
                            spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle((int)dims.X + 9, bottomY - 3, 2, 1), fillColor);
                        if (totalFill >= 2)
                            spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle((int)dims.X + 8, bottomY - 4, 4, 1), fillColor);
                        int bodyRows = Math.Min(totalFill - 2, maxFillHeight - 4);
                        if (bodyRows > 0)
                        {
                            int bodyY = bottomY - 4 - bodyRows;
                            spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle((int)dims.X + 7, bodyY, 6, bodyRows), fillColor);
                        }
                        if (totalFill >= maxFillHeight - 1)
                            spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle((int)dims.X + 8, (int)dims.Y + 3, 4, 1), fillColor);
                        if (totalFill >= maxFillHeight)
                            spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle((int)dims.X + 9, (int)dims.Y + 2, 2, 1), fillColor);
                    }

                    int percent = (int)(progress * 100f);
                    string percentText = percent + "%";
                    if (Entity.GetTotalEssences() >= DistillerTileEntity.MaxCapacity)
                    {
                        percentText = "FULL";
                    }
                    Vector2 textSize = FontAssets.MouseText.Value.MeasureString(percentText) * 0.7f;
                    Vector2 textPos = new Vector2(dims.X + (dims.Width - textSize.X) / 2f, dims.Y + dims.Height + 2f);
                    Color percentColor = (percentText == "FULL") ? Color.OrangeRed : Color.Cyan;
                    ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, percentText, textPos, percentColor * Opacity, 0f, Vector2.Zero, new Vector2(0.7f));
                }
            }
        }
        private class DistillerBackgroundPanel : UIElement
        {
            public float Opacity = 1f;
            protected override void DrawSelf(SpriteBatch spriteBatch)
            {
                var tex1 = ModContent.Request<Texture2D>("jarkos/UI/Textures/DistillatorUI_1");
                var tex2 = ModContent.Request<Texture2D>("jarkos/UI/Textures/DistillatorUI_2");
                var tex3 = ModContent.Request<Texture2D>("jarkos/UI/Textures/DistillatorUI_3");
                CalculatedStyle dims = GetDimensions();
                Rectangle rect = new Rectangle((int)dims.X, (int)dims.Y, (int)dims.Width, (int)dims.Height);

                if (tex1.IsLoaded && tex2.IsLoaded && tex3.IsLoaded)
                {
                    spriteBatch.Draw(tex2.Value, rect, new Color(40, 40, 40, (int)(240 * Opacity)));
                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
                    spriteBatch.Draw(tex3.Value, rect, new Color(150, 150, 150, (int)(200 * Opacity)));
                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
                    spriteBatch.Draw(tex1.Value, rect, Color.White * Opacity);
                }
            }
        }
        private DistillerBackgroundPanel _mainPanel;
        private DistillerBarElement _barContainer;
        private DistillerItemSlot _leftSlot;
        private DistillerItemSlot _rightSlot;
        private DistillerSlotProgressBar _leftProgress;
        private DistillerSlotProgressBar _rightProgress;
        private DistillerIgnoreLaserButton _ignoreLaserButton;
        private UIPanel _closeButton;
        private UIText _closeText;
        private UIPanel _collectButton;
        private UIText _collectText;
        private Point16 _tePosition = new Point16(-1, -1);
        private DistillerTileEntity _entity => (_tePosition.X != -1 && TileEntity.ByPosition.TryGetValue(_tePosition, out TileEntity te) && te is DistillerTileEntity dte) ? dte : null;
        private Tween _scaleTween;
        public void SetEntity(DistillerTileEntity entity)
        {
            _tePosition = entity != null ? entity.Position : new Point16(-1, -1);
            var ent = _entity;

            _barContainer.Entity = ent;

            if (_leftSlot != null) _leftSlot.Entity = ent;
            if (_rightSlot != null) _rightSlot.Entity = ent;
            if (_leftProgress != null) _leftProgress.Entity = ent;
            if (_rightProgress != null) _rightProgress.Entity = ent;

            if (_ignoreLaserButton != null) _ignoreLaserButton.Entity = ent;

            var rp = Main.LocalPlayer.GetModPlayer<ResearchPlayer>();
            bool nowUnlocked = rp.IsCompleted("jarkos:AquaNode7");

            if (nowUnlocked)
            {
                if (RightSlotWasLocked)
                {
                    if (_rightSlot != null)
                    {
                        _rightSlot.State = DistillerItemSlot.SlotState.Unlocking;
                        _rightSlot.UnlockProgress = 0f;
                        SoundEngine.PlaySound(SoundID.Unlock);
                    }
                    RightSlotWasLocked = false;
                }
                else
                {
                    if (_rightSlot != null) _rightSlot.State = DistillerItemSlot.SlotState.Unlocked;
                }
            }
            else
            {
                RightSlotWasLocked = true;
                if (_rightSlot != null) _rightSlot.State = DistillerItemSlot.SlotState.Locked;
            }
            _scaleTween.Set(0f);
            _scaleTween.To(1f);
        }
        public override void OnInitialize()
        {
            _scaleTween = new Tween(0.3f, Easing.EaseOutBack);

            _mainPanel = new DistillerBackgroundPanel();
            _mainPanel.SetPadding(0);
            _mainPanel.Width.Set(400, 0f);
            _mainPanel.Height.Set(250, 0f);
            _mainPanel.HAlign = 0.5f;
            _mainPanel.VAlign = 0.5f;
            Append(_mainPanel);

            _closeButton = new UIPanel();
            _closeButton.Width.Set(30, 0f);
            _closeButton.Height.Set(30, 0f);
            _closeButton.HAlign = 0.95f;
            _closeButton.VAlign = 0.05f;
            _closeButton.BackgroundColor = new Color(200, 40, 40, 180);
            _closeButton.OnLeftClick += (evt, el) => Close();
            _mainPanel.Append(_closeButton);
            
            _closeText = new UIText("X", 0.9f, true);
            _closeText.HAlign = 0.5f;
            _closeText.VAlign = 0.5f;
            _closeButton.Append(_closeText);

            _ignoreLaserButton = new DistillerIgnoreLaserButton();
            _ignoreLaserButton.Width.Set(100, 0f);
            _ignoreLaserButton.Height.Set(26, 0f);
            _ignoreLaserButton.HAlign = 0f;
            _ignoreLaserButton.VAlign = 0f;
            _ignoreLaserButton.Left.Set(20, 0f);
            _ignoreLaserButton.Top.Set(20, 0f);
            _ignoreLaserButton.OnLeftClick += (evt, el) =>
            {
                if (_entity != null)
                {
                    _entity.IgnoreLaser = !_entity.IgnoreLaser;
                    _entity.SyncData();
                    SoundEngine.PlaySound(SoundID.MenuTick);
                }
            };
            _mainPanel.Append(_ignoreLaserButton);

            _leftSlot = new DistillerItemSlot(0);
            _leftSlot.Width.Set(60, 0f);
            _leftSlot.Height.Set(60, 0f);
            _leftSlot.HAlign = 0.1f;
            _leftSlot.VAlign = 0.4f;
            _leftSlot.OnLeftClick += (evt, el) => HandleSlotClick(0);
            _mainPanel.Append(_leftSlot);

            _leftProgress = new DistillerSlotProgressBar(0);
            _leftProgress.Width.Set(20, 0f);
            _leftProgress.Height.Set(60, 0f);
            _leftProgress.HAlign = 0.25f;
            _leftProgress.VAlign = 0.4f;
            _mainPanel.Append(_leftProgress);

            _rightSlot = new DistillerItemSlot(1);
            _rightSlot.Width.Set(60, 0f);
            _rightSlot.Height.Set(60, 0f);
            _rightSlot.HAlign = 0.9f;
            _rightSlot.VAlign = 0.4f;
            _rightSlot.OnLeftClick += (evt, el) => HandleSlotClick(1);
            _mainPanel.Append(_rightSlot);

            _rightProgress = new DistillerSlotProgressBar(1);
            _rightProgress.Width.Set(20, 0f);
            _rightProgress.Height.Set(60, 0f);
            _rightProgress.HAlign = 0.73f;
            _rightProgress.VAlign = 0.4f;
            _mainPanel.Append(_rightProgress);

            _barContainer = new DistillerBarElement();
            _barContainer.Width.Set(40, 0f);
            _barContainer.Height.Set(150, 0f);
            _barContainer.HAlign = 0.5f;
            _barContainer.VAlign = 0.3f;
            _mainPanel.Append(_barContainer);

            _collectButton = new UIPanel();
            _collectButton.Width.Set(120, 0f);
            _collectButton.Height.Set(30, 0f);
            _collectButton.HAlign = 0.5f;
            _collectButton.VAlign = 0.9f;
            _collectButton.BackgroundColor = new Color(200, 40, 40, 180);
            _collectButton.OnLeftClick += (evt, el) => CollectEssences();
            _mainPanel.Append(_collectButton);

            _collectText = new UIText(LangSystem.CollectButtonText, 0.9f);
            _collectText.HAlign = 0.5f;
            _collectText.VAlign = 0.5f;
            _collectButton.Append(_collectText);
        }
        private void HandleSlotClick(int slotIndex)
        {
            if (slotIndex == 1 && _rightSlot != null && _rightSlot.State != DistillerItemSlot.SlotState.Unlocked) return;
            if (_entity == null) return;

            Item slotItem = _entity.InputSlots[slotIndex];
            Item mouseItem = Main.mouseItem;

            if (mouseItem != null && !mouseItem.IsAir)
            {
                if (slotItem == null || slotItem.IsAir)
                {
                    _entity.InputSlots[slotIndex] = mouseItem.Clone();
                    Main.mouseItem.TurnToAir();

                    SoundEngine.PlaySound(SoundID.Grab);
                }
                else if (slotItem.type == mouseItem.type && slotItem.stack < slotItem.maxStack)
                {
                    int space = slotItem.maxStack - slotItem.stack;
                    int toAdd = Math.Min(space, mouseItem.stack);

                    slotItem.stack += toAdd;
                    mouseItem.stack -= toAdd;

                    if (mouseItem.stack <= 0) Main.mouseItem.TurnToAir();
                    SoundEngine.PlaySound(SoundID.Grab);
                }
                else
                {
                    _entity.InputSlots[slotIndex] = mouseItem.Clone();
                    Main.mouseItem = slotItem.Clone();
                    SoundEngine.PlaySound(SoundID.Grab);
                }
            }
            else if (slotItem != null && !slotItem.IsAir)
            {
                Main.mouseItem = slotItem.Clone();
                _entity.InputSlots[slotIndex].TurnToAir();
                SoundEngine.PlaySound(SoundID.Grab);
            }

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                ModPacket packet = ModContent.GetInstance<Jarkos_mod>().GetPacket();
                packet.Write((byte)JarkosPacketType.DistillerSlotSync);
                packet.Write(_entity.Position.X);
                packet.Write(_entity.Position.Y);
                packet.Write((byte)slotIndex);
                Item item = _entity.InputSlots[slotIndex];
                bool sending = item != null && !item.IsAir;
                packet.Write(sending);
                if (sending)
                    ItemIO.Send(item, packet, true);
                packet.Send();
            }
        }
        private bool _isCollecting = false;
        private float _collectTimer = 0f;
        private void CollectEssences()
        {
            if (_entity == null || _entity.GetTotalEssences() == 0 || _isCollecting) return;
            _isCollecting = true;
            _collectTimer = 2f;
            SoundEngine.PlaySound(SoundID.MenuTick);
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_entity == null) return;
            base.Draw(spriteBatch);
        }
        public override void Update(GameTime gameTime)
        {
            var ent = _entity;
            if (ent == null)
            {
                Close();
                return;
            }
            _barContainer.Entity = ent;

            if (_leftSlot != null) _leftSlot.Entity = ent;
            if (_rightSlot != null) _rightSlot.Entity = ent;
            if (_leftProgress != null) _leftProgress.Entity = ent;
            if (_rightProgress != null) _rightProgress.Entity = ent;

            if (_ignoreLaserButton != null) _ignoreLaserButton.Entity = ent;

            base.Update(gameTime);

            if (ent != null)
            {
                int reqTime = 300;
                bool secondSlotUnlocked = false;
                int closestPlayerIndex = Player.FindClosest(new Vector2(_entity.Position.X * 16, _entity.Position.Y * 16), 480, 480);

                if (closestPlayerIndex >= 0 && closestPlayerIndex < Main.maxPlayers)
                {
                    Player closest = Main.player[closestPlayerIndex];
                    if (closest != null && closest.active)
                    {
                        var rp = closest.GetModPlayer<ResearchPlayer>();

                        if (rp.IsCompleted("jarkos:AquaNode6")) reqTime = 195;
                        if (rp.IsCompleted("jarkos:AquaNode7")) secondSlotUnlocked = true;
                    }
                }

                for (int i = 0; i < _entity.InputSlots.Length; i++)
                {
                    if (i == 1 && !secondSlotUnlocked) continue;
                    if (_entity.InputSlots[i] != null && !_entity.InputSlots[i].IsAir && _entity.GetTotalEssences() < DistillerTileEntity.MaxCapacity)
                    {
                        _entity.ProcessTimers[i]++;
                        if (_entity.ProcessTimers[i] >= reqTime)
                        {
                            _entity.ProcessTimers[i] = 0;
                        }
                    }
                    else
                    {
                        _entity.ProcessTimers[i] = 0;
                    }
                }
            }

            if (_mainPanel != null && _mainPanel.ContainsPoint(Main.MouseScreen))
            {
                Main.LocalPlayer.mouseInterface = true;
            }
            if (_isCollecting)
            {
                _collectTimer -= 0.016f;
                if (_collectTimer <= 0f)
                {
                    _isCollecting = false;
                    _collectTimer = 0f;
                    if (_entity != null && _entity.GetTotalEssences() > 0)
                    {
                        if (Main.netMode == NetmodeID.MultiplayerClient)
                        {
                            ModPacket packet = ModContent.GetInstance<Jarkos_mod>().GetPacket();
                            packet.Write((byte)JarkosPacketType.DistillerCollect);
                            packet.Write(_entity.Position.X);
                            packet.Write(_entity.Position.Y);
                            packet.Send();

                            SoundEngine.PlaySound(SoundID.Item4);
                        }
                        else
                        {
                            var rp = Main.LocalPlayer.GetModPlayer<ResearchPlayer>();
                            foreach (var kvp in _entity.StoredEssences)
                            {
                                rp.AddEssence(kvp.Key, kvp.Value);
                            }
                            _entity.ClearEssences();

                            SoundEngine.PlaySound(SoundID.Item4);
                        }
                    }
                }
            }
            if (_rightSlot != null && _rightSlot.State == DistillerItemSlot.SlotState.Unlocking)
            {
                _rightSlot.UnlockProgress += 0.02f;
                if (_rightSlot.UnlockProgress >= 1f)
                {
                    _rightSlot.UnlockProgress = 1f;
                    _rightSlot.State = DistillerItemSlot.SlotState.Unlocked;
                }
            }
            if (_entity != null && _scaleTween != null && _scaleTween.Target != 0f)
            {
                Player player = Main.LocalPlayer;
                float distance = Vector2.Distance(player.Center, new Vector2(_entity.Position.X * 16 + 16, _entity.Position.Y * 16 + 16));

                if (distance > 160f)
                {
                    Close();
                }
            }
            if (_scaleTween != null)
            {
                _scaleTween.Update(0.016f);
                float t = _scaleTween.Value;
                float easedT = _scaleTween.Eased;

                _mainPanel.Top.Set((1f - easedT) * 100f, 0f);
                _mainPanel.Recalculate();

                if (_mainPanel != null) _mainPanel.Opacity = t;
                if (_barContainer != null)
                {
                    _barContainer.Opacity = t;
                    _barContainer.CollectProgress = _isCollecting ? (_collectTimer / 2f) : 1f;
                }

                if (_leftSlot != null) _leftSlot.Opacity = t;
                if (_rightSlot != null) _rightSlot.Opacity = t;
                if (_leftProgress != null) _leftProgress.Opacity = t;
                if (_rightProgress != null) _rightProgress.Opacity = t;
                
                if (_ignoreLaserButton != null) _ignoreLaserButton.Opacity = t;
                if (_closeButton != null)
                {
                    _closeButton.BackgroundColor = new Color(200, 40, 40, (int)(180 * t));
                    _closeButton.BorderColor = Color.Black * t;
                }
                if (_closeText != null) _closeText.TextColor = Color.White * t;
                if (_collectButton != null)
                {
                    if (_isCollecting)
                    {
                        _collectButton.BackgroundColor = new Color(100, 100, 100, (int)(180 * t));
                    }
                    else
                    {
                        _collectButton.BackgroundColor = new Color(200, 40, 40, (int)(180 * t));
                    }
                    _collectButton.BorderColor = Color.Black * t;
                }
                if (_collectText != null)
                {
                    _collectText.TextColor = Color.White * t;
                    _collectText.SetText(_isCollecting ? "Collecting..." : LangSystem.CollectButtonText.Value);
                }
                if (_scaleTween.Target == 0f && !_scaleTween.Active)
                {
                    DistillerUISystem.Instance.ForceClose();
                }
            }
        }
        public void Close()
        {
            if (_isCollecting)
            {
                _isCollecting = false;
                if (_entity != null && _entity.GetTotalEssences() > 0)
                {
                    if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        ModPacket packet = ModContent.GetInstance<Jarkos_mod>().GetPacket();
                        packet.Write((byte)JarkosPacketType.DistillerCollect);
                        packet.Write(_entity.Position.X);
                        packet.Write(_entity.Position.Y);
                        packet.Send();
                    }
                    else
                    {
                        var rp = Main.LocalPlayer.GetModPlayer<ResearchPlayer>();
                        foreach (var kvp in _entity.StoredEssences)
                        {
                            rp.AddEssence(kvp.Key, kvp.Value);
                        }
                        _entity.ClearEssences();
                    }
                }
            }
            _scaleTween.To(0f);
        }
    }
}
