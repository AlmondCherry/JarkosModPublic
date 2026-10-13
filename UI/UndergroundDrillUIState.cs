using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.UI.Chat;
using jarkos.tiles;
using jarkos.items.materials;
using Terraria.Audio;

namespace jarkos.UI
{
    public class UndergroundDrillUIState : UIState
    {
        private class DrillMainPanel : UIPanel
        {
            public UndergroundDrillUIState ParentState;

            public DrillMainPanel(UndergroundDrillUIState parentState)
            {
                ParentState = parentState;
            }

            protected override void DrawSelf(SpriteBatch spriteBatch)
            {
                base.DrawSelf(spriteBatch);

                if (IsMouseHovering || GetDimensions().ToRectangle().Contains(Main.MouseScreen.ToPoint()))
                {
                    Main.LocalPlayer.mouseInterface = true;
                }

                if (ParentState == null || ParentState.Entity == null) return;
                UndergroundDrillTileEntity entity = ParentState.Entity;

                CalculatedStyle dims = GetDimensions();
                float time = (float)Main.timeForVisualEffects;


                float easeProgress = 1f - MathF.Pow(1f - ParentState.OpenAnimationProgress, 3f);
                Color alphaColor = Color.White * easeProgress;


                float ledX = dims.X + 20f;
                float ledY = dims.Y + 22f;
                Color ledColor = (!entity.IsActive ? Color.Red : (entity.IsFull ? Color.Yellow : Color.LimeGreen)) * easeProgress;

                Common.Draw.RadialGlow(spriteBatch, new Vector2(ledX, ledY), 14f, ledColor * 0.85f);
                spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle((int)ledX - 4, (int)ledY - 4, 8, 8), ledColor);


                string titleText = "Drill main-panel";
                ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, titleText, new Vector2(dims.X + 44f, dims.Y + 12f), Color.Gold * easeProgress, 0f, Vector2.Zero, new Vector2(0.85f));


                float slotX = dims.X + 20f;
                float slotY = dims.Y + 44f;
                for (int i = 0; i < 5; i++)
                {
                    Rectangle sRect = new Rectangle((int)(slotX + i * 50f), (int)slotY, 44, 44);
                    Terraria.Utils.DrawInvBG(spriteBatch, sRect, new Color(50, 50, 50, (int)(220 * easeProgress)));

                    Item item = entity.OutputSlots[i];
                    if (item != null && !item.IsAir)
                    {
                        Main.instance.LoadItem(item.type);
                        Texture2D itemTex = TextureAssets.Item[item.type].Value;
                        Rectangle srcRect = Main.itemAnimations[item.type] != null ? Main.itemAnimations[item.type].GetFrame(itemTex) : itemTex.Frame();
                        float scale = Math.Min(32f / srcRect.Width, 32f / srcRect.Height);
                        Vector2 origin = srcRect.Size() / 2f;
                        Vector2 itemCenter = new Vector2(sRect.X + 22, sRect.Y + 22);

                        spriteBatch.Draw(itemTex, itemCenter, srcRect, alphaColor, 0f, origin, scale, SpriteEffects.None, 0f);

                        if (item.stack > 1)
                        {
                            string stackStr = item.stack.ToString();
                            Vector2 stackPos = new Vector2(sRect.X + 4, sRect.Y + 24);
                            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, stackStr, stackPos, alphaColor, 0f, Vector2.Zero, new Vector2(0.75f));
                        }

                        if (sRect.Contains(Main.MouseScreen.ToPoint()))
                        {
                            Main.LocalPlayer.mouseInterface = true;
                            Main.HoverItem = item.Clone();
                            Main.hoverItemName = item.HoverName;
                        }
                    }
                }


                float barY = dims.Y + 98f;
                Rectangle barBgRect = new Rectangle((int)(dims.X + 20f), (int)barY, 250, 18);
                

                spriteBatch.Draw(TextureAssets.MagicPixel.Value, barBgRect, new Color(30, 30, 30, (int)(230 * easeProgress)));

                spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(barBgRect.X - 1, barBgRect.Y - 1, barBgRect.Width + 2, 1), new Color(100, 100, 100, (int)(200 * easeProgress)));
                spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(barBgRect.X - 1, barBgRect.Y + barBgRect.Height, barBgRect.Width + 2, 1), new Color(100, 100, 100, (int)(200 * easeProgress)));
                spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(barBgRect.X - 1, barBgRect.Y, 1, barBgRect.Height), new Color(100, 100, 100, (int)(200 * easeProgress)));
                spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(barBgRect.X + barBgRect.Width, barBgRect.Y, 1, barBgRect.Height), new Color(100, 100, 100, (int)(200 * easeProgress)));

                float progress = MathHelper.Clamp(entity.MineTimer / UndergroundDrillTileEntity.BaseMineTime, 0f, 1f);
                if (progress > 0f)
                {
                    int fillWidth = (int)((barBgRect.Width - 4) * progress);
                    Rectangle fillRect = new Rectangle(barBgRect.X + 2, barBgRect.Y + 2, fillWidth, barBgRect.Height - 4);
                    spriteBatch.Draw(TextureAssets.MagicPixel.Value, fillRect, Color.Lerp(Color.DarkBlue, Color.Cyan, progress) * easeProgress);
                    spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(fillRect.X, fillRect.Y, fillWidth, 2), Color.White * (0.4f * easeProgress));
                }


                float speedMult = entity.GetSpeedMultiplier();
                int speedPercent = (int)MathF.Round(speedMult * 100f);
                string speedText = $"Drill speed: {speedPercent}%";
                Color speedColor = speedMult > 1.0f ? Color.Turquoise : Color.Gray;
                ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, speedText, new Vector2(dims.X + 20f, barY + 24f), speedColor * easeProgress, 0f, Vector2.Zero, new Vector2(0.75f));


                Rectangle takeBtnRect = new Rectangle((int)(dims.X + 20f), (int)(barY + 48f), 110, 30);
                bool hoverTake = takeBtnRect.Contains(Main.MouseScreen.ToPoint());
                if (hoverTake) Main.LocalPlayer.mouseInterface = true;

                Color takeBg = (hoverTake ? new Color(50, 100, 60) : new Color(30, 65, 40)) * easeProgress;
                Terraria.Utils.DrawInvBG(spriteBatch, takeBtnRect, takeBg);

                Vector2 takeSize = FontAssets.MouseText.Value.MeasureString("Take") * 0.85f;
                Vector2 takePos = new Vector2(takeBtnRect.X + (takeBtnRect.Width - takeSize.X) / 2f, takeBtnRect.Y + 5f);
                ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, "Take", takePos, Color.White * easeProgress, 0f, Vector2.Zero, new Vector2(0.85f));

                if (hoverTake && Main.mouseLeft && Main.mouseLeftRelease)
                {
                    Main.mouseLeftRelease = false;
                    entity.TakeAllOresToPlayer(Main.LocalPlayer);
                }


                Rectangle toggleBtnRect = new Rectangle((int)(dims.X + 160f), (int)(barY + 48f), 110, 30);
                bool hoverToggle = toggleBtnRect.Contains(Main.MouseScreen.ToPoint());
                if (hoverToggle) Main.LocalPlayer.mouseInterface = true;

                Color toggleBg = (hoverToggle ? new Color(90, 90, 90) : new Color(60, 60, 60)) * easeProgress;
                Terraria.Utils.DrawInvBG(spriteBatch, toggleBtnRect, toggleBg);

                string toggleText = entity.IsActive ? "TURN OFF" : "TURN ON";
                Color toggleTextColor = (entity.IsActive ? Color.Crimson : Color.LimeGreen) * easeProgress;
                Vector2 toggleTextSize = FontAssets.MouseText.Value.MeasureString(toggleText) * 0.75f;
                Vector2 toggleTextPos = new Vector2(toggleBtnRect.X + (toggleBtnRect.Width - toggleTextSize.X) / 2f, toggleBtnRect.Y + 5f);
                ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, toggleText, toggleTextPos, toggleTextColor, 0f, Vector2.Zero, new Vector2(0.75f));

                if (hoverToggle && Main.mouseLeft && Main.mouseLeftRelease)
                {
                    Main.mouseLeftRelease = false;
                    entity.IsActive = !entity.IsActive;
                    entity.SyncData();
                }


                Rectangle closeBtnRect = new Rectangle((int)(dims.X + 258f), (int)(dims.Y + 8f), 22, 22);
                bool hoverClose = closeBtnRect.Contains(Main.MouseScreen.ToPoint());
                if (hoverClose) Main.LocalPlayer.mouseInterface = true;

                if (hoverClose)
                {
                    jarkos.UI.Common.Draw.RadialGlow(spriteBatch, closeBtnRect.Center.ToVector2(), 14f, Color.Crimson * (0.8f * easeProgress));
                }

                Color mainRed = (hoverClose ? new Color(230, 45, 60) : new Color(175, 30, 45)) * easeProgress;
                Color borderRed = (hoverClose ? new Color(140, 20, 30) : new Color(100, 15, 25)) * easeProgress;
                Color highlightRed = (hoverClose ? new Color(255, 130, 140) : new Color(220, 80, 95)) * easeProgress;


                spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(closeBtnRect.X + 1, closeBtnRect.Y, 20, 22), borderRed);
                spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(closeBtnRect.X, closeBtnRect.Y + 1, 22, 20), borderRed);


                spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(closeBtnRect.X + 2, closeBtnRect.Y + 1, 18, 20), mainRed);
                spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(closeBtnRect.X + 1, closeBtnRect.Y + 2, 20, 18), mainRed);


                spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(closeBtnRect.X + 2, closeBtnRect.Y + 1, 18, 2), highlightRed);


                spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(closeBtnRect.X + 2, closeBtnRect.Y + 19, 18, 2), new Color(60, 10, 15) * easeProgress);


                string crossText = "x";
                Vector2 crossSize = FontAssets.MouseText.Value.MeasureString(crossText) * 0.75f;
                Vector2 crossPos = new Vector2(closeBtnRect.X + (closeBtnRect.Width - crossSize.X) / 2f + 0.5f, closeBtnRect.Y + (closeBtnRect.Height - crossSize.Y) / 2f - 2f);
                ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, crossText, crossPos, Color.White * easeProgress, 0f, Vector2.Zero, new Vector2(0.75f));

                if (hoverClose && Main.mouseLeft && Main.mouseLeftRelease)
                {
                    Main.mouseLeftRelease = false;
                    UndergroundDrillUISystem.Instance.CloseUI();
                }


                if (entity.IsFull)
                {
                    float pulse = 0.7f + 0.3f * MathF.Sin(time * 0.15f);
                    string warnText = "NO SPACE FOR NEW ORE";
                    Vector2 warnSize = FontAssets.MouseText.Value.MeasureString(warnText) * 0.7f;
                    Vector2 warnPos = new Vector2(dims.X + (dims.Width - warnSize.X) / 2f, dims.Y + dims.Height - 22f);
                    ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, warnText, warnPos, Color.OrangeRed * (pulse * easeProgress), 0f, Vector2.Zero, new Vector2(0.7f));
                }
            }
        }

        private class DrillFuelPanel : UIPanel
        {
            public UndergroundDrillUIState ParentState;

            public DrillFuelPanel(UndergroundDrillUIState parentState)
            {
                ParentState = parentState;
            }

            protected override void DrawSelf(SpriteBatch spriteBatch)
            {
                if (ParentState == null || ParentState.Entity == null || ParentState.FuelPanelProgress <= 0.01f) return;
                base.DrawSelf(spriteBatch);

                if (IsMouseHovering || GetDimensions().ToRectangle().Contains(Main.MouseScreen.ToPoint()))
                {
                    Main.LocalPlayer.mouseInterface = true;
                }

                UndergroundDrillTileEntity entity = ParentState.Entity;
                CalculatedStyle dims = GetDimensions();

                float easeMain = 1f - MathF.Pow(1f - ParentState.OpenAnimationProgress, 3f);
                float easeFuel = 1f - MathF.Pow(1f - ParentState.FuelPanelProgress, 3f);
                float alphaMult = easeMain * easeFuel;
                Color alphaColor = Color.White * alphaMult;


                string titleText = "Fuel panel";
                Vector2 titleSize = FontAssets.MouseText.Value.MeasureString(titleText) * 0.85f;
                Vector2 titlePos = new Vector2(dims.X + (dims.Width - titleSize.X) / 2f, dims.Y + 12f);
                ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, titleText, titlePos, Color.Orange * alphaMult, 0f, Vector2.Zero, new Vector2(0.85f));


                float slotStartX = dims.X + (dims.Width - 44f) / 2f;
                float slotStartY = dims.Y + 50f;
                float slotSpacingY = 50f;

                for (int i = 0; i < 3; i++)
                {
                    Rectangle sRect = new Rectangle((int)slotStartX, (int)(slotStartY + i * slotSpacingY), 44, 44);
                    bool hoverSlot = sRect.Contains(Main.MouseScreen.ToPoint());

                    Color bgCol = (hoverSlot ? new Color(80, 80, 80) : new Color(50, 50, 50)) * alphaMult;
                    Terraria.Utils.DrawInvBG(spriteBatch, sRect, bgCol);

                    Item slotItem = entity.FuelSlots[i];
                    if (slotItem != null && !slotItem.IsAir)
                    {
                        Main.instance.LoadItem(slotItem.type);
                        Texture2D itemTex = TextureAssets.Item[slotItem.type].Value;
                        Rectangle srcRect = Main.itemAnimations[slotItem.type] != null ? Main.itemAnimations[slotItem.type].GetFrame(itemTex) : itemTex.Frame();
                        float scale = Math.Min(32f / srcRect.Width, 32f / srcRect.Height);
                        Vector2 origin = srcRect.Size() / 2f;
                        Vector2 itemCenter = new Vector2(sRect.X + 22, sRect.Y + 22);

                        spriteBatch.Draw(itemTex, itemCenter, srcRect, alphaColor, 0f, origin, scale, SpriteEffects.None, 0f);

                        if (slotItem.stack > 1)
                        {
                            string stackStr = slotItem.stack.ToString();
                            Vector2 stackPos = new Vector2(sRect.X + 4, sRect.Y + 24);
                            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.ItemStack.Value, stackStr, stackPos, alphaColor, 0f, Vector2.Zero, new Vector2(0.75f));
                        }
                    }

                    if (hoverSlot)
                    {
                        Main.LocalPlayer.mouseInterface = true;

                        if (slotItem != null && !slotItem.IsAir)
                        {
                            Main.HoverItem = slotItem.Clone();
                            Main.hoverItemName = slotItem.HoverName;
                        }

                        if (Main.mouseLeft && Main.mouseLeftRelease)
                        {
                            Main.mouseLeftRelease = false;
                            Item held = Main.mouseItem;

                            if (held != null && !held.IsAir)
                            {
                                bool isCooling = held.type == ModContent.ItemType<CoolingFuel>();
                                bool isSpectral = held.type == ModContent.ItemType<SpectralFuel>();

                                if (isCooling || isSpectral)
                                {
                                    if (slotItem == null || slotItem.IsAir)
                                    {
                                        entity.FuelSlots[i] = held.Clone();
                                        Main.mouseItem.TurnToAir();
                                        SoundEngine.PlaySound(SoundID.Grab);
                                        entity.SyncData();
                                    }
                                    else if (slotItem.type == held.type)
                                    {
                                        int spaceLeft = slotItem.maxStack - slotItem.stack;
                                        if (spaceLeft > 0)
                                        {
                                            int transfer = Math.Min(spaceLeft, held.stack);
                                            slotItem.stack += transfer;
                                            held.stack -= transfer;
                                            if (held.stack <= 0) Main.mouseItem.TurnToAir();
                                            SoundEngine.PlaySound(SoundID.Grab);
                                            entity.SyncData();
                                        }
                                    }
                                    else
                                    {
                                        Item temp = slotItem.Clone();
                                        entity.FuelSlots[i] = held.Clone();
                                        Main.mouseItem = temp;
                                        SoundEngine.PlaySound(SoundID.Grab);
                                        entity.SyncData();
                                    }
                                }
                            }
                            else if (slotItem != null && !slotItem.IsAir)
                            {
                                Main.mouseItem = slotItem.Clone();
                                entity.FuelSlots[i].TurnToAir();
                                SoundEngine.PlaySound(SoundID.Grab);
                                entity.SyncData();
                            }
                        }
                    }
                }
            }
        }

        private DrillMainPanel _mainPanel;
        private DrillFuelPanel _fuelPanel;
        public UndergroundDrillTileEntity Entity;
        public float OpenAnimationProgress = 0f;
        public float FuelPanelProgress = 0f;
        public bool IsClosing = false;

        public void ResetAnimation()
        {
            OpenAnimationProgress = 0f;
            FuelPanelProgress = 0f;
            IsClosing = false;
            if (_mainPanel != null)
            {
                _mainPanel.Top.Set(220f, 0f);
                _mainPanel.Left.Set(0f, 0f);
                _mainPanel.BackgroundColor = new Color(40, 40, 40, 0);
                _mainPanel.BorderColor = new Color(160, 160, 160, 0);
                _mainPanel.Recalculate();
            }
            if (_fuelPanel != null)
            {
                _fuelPanel.Top.Set(220f, 0f);
                _fuelPanel.Left.Set(0f, 0f);
                _fuelPanel.BackgroundColor = new Color(40, 40, 40, 0);
                _fuelPanel.BorderColor = new Color(160, 160, 160, 0);
                _fuelPanel.Recalculate();
            }
        }

        public void SetEntity(UndergroundDrillTileEntity entity)
        {
            Entity = entity;
        }

        public override void OnInitialize()
        {
            _mainPanel = new DrillMainPanel(this);
            _mainPanel.Width.Set(290f, 0f);
            _mainPanel.Height.Set(215f, 0f);
            _mainPanel.HAlign = 0.5f;
            _mainPanel.VAlign = 0.5f;
            _mainPanel.BackgroundColor = new Color(40, 40, 40, 235);
            _mainPanel.BorderColor = new Color(160, 160, 160, 200);
            Append(_mainPanel);

            _fuelPanel = new DrillFuelPanel(this);
            _fuelPanel.Width.Set(130f, 0f);
            _fuelPanel.Height.Set(215f, 0f);
            _fuelPanel.HAlign = 0.5f;
            _fuelPanel.VAlign = 0.5f;
            _fuelPanel.BackgroundColor = new Color(40, 40, 40, 235);
            _fuelPanel.BorderColor = new Color(160, 160, 160, 200);
            Append(_fuelPanel);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (IsClosing)
            {
                if (FuelPanelProgress > 0f)
                {
                    FuelPanelProgress = MathHelper.Clamp(FuelPanelProgress - 0.08f, 0f, 1f);
                }
                else if (OpenAnimationProgress > 0f)
                {
                    OpenAnimationProgress = MathHelper.Clamp(OpenAnimationProgress - 0.075f, 0f, 1f);
                }
                else
                {
                    IsClosing = false;
                    UndergroundDrillUISystem.Instance.FinishCloseUI();
                    return;
                }
            }
            else
            {
                if (OpenAnimationProgress < 1f)
                {
                    OpenAnimationProgress = MathHelper.Clamp(OpenAnimationProgress + 0.055f, 0f, 1f);
                }

                if (OpenAnimationProgress >= 0.85f && FuelPanelProgress < 1f)
                {
                    FuelPanelProgress = MathHelper.Clamp(FuelPanelProgress + 0.065f, 0f, 1f);
                }
            }

            float easeMain = 1f - MathF.Pow(1f - OpenAnimationProgress, 3f);
            float easeFuel = 1f - MathF.Pow(1f - FuelPanelProgress, 3f);

            float yOffset = (1f - easeMain) * 220f;
            float mainLeftOffset = -70f * easeFuel;

            float fuelLeftOffset = 145f + (1f - easeFuel) * 50f;

            if (_mainPanel != null)
            {
                _mainPanel.Top.Set(yOffset, 0f);
                _mainPanel.Left.Set(mainLeftOffset, 0f);
                _mainPanel.BackgroundColor = new Color(40, 40, 40, (int)(235 * easeMain));
                _mainPanel.BorderColor = new Color(160, 160, 160, (int)(200 * easeMain));
                _mainPanel.Recalculate();
            }

            if (_fuelPanel != null)
            {
                _fuelPanel.Top.Set(yOffset, 0f);
                _fuelPanel.Left.Set(fuelLeftOffset, 0f);
                _fuelPanel.BackgroundColor = new Color(40, 40, 40, (int)(235 * easeMain * easeFuel));
                _fuelPanel.BorderColor = new Color(160, 160, 160, (int)(200 * easeMain * easeFuel));
                _fuelPanel.Recalculate();
            }

            if (IsMouseHovering || (_mainPanel != null && _mainPanel.IsMouseHovering) || (_fuelPanel != null && _fuelPanel.IsMouseHovering))
            {
                Main.LocalPlayer.mouseInterface = true;
            }

            if (Entity == null)
            {
                UndergroundDrillUISystem.Instance.FinishCloseUI();
                return;
            }

            Player player = Main.LocalPlayer;
            Vector2 drillWorldPos = new Vector2(Entity.Position.X * 16 + 40, Entity.Position.Y * 16 + 40);
            if (!IsClosing && (Vector2.Distance(player.Center, drillWorldPos) > 300f || !Main.playerInventory))
            {
                UndergroundDrillUISystem.Instance.CloseUI();
            }
        }
    }
}
