using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using ReLogic.Content;
using jarkos.core.Data;
using jarkos.core.PlayerInteract;
using Terraria.GameInput;
using Terraria.UI.Chat;
using Terraria.Localization;

namespace jarkos.UI
{
    public class EssenceCombinerUI : UIState
    {
        public bool IsOpen { get; private set; }
        public bool IsClosing { get; private set; }
        public float OpenProgress { get; private set; } = 0f;

        private EssenceType Slot1 = (EssenceType)(-1);
        private EssenceType Slot2 = (EssenceType)(-1);
        private bool IsSelecting = false;
        private bool TargetSelecting = false;
        private float SelectionAnimProgress = 0f;
        private int SelectingSlot = 1;
        private int AnimState = 0;
        private float AnimTimer = 0f;
        private EssenceType ResultType = (EssenceType)(-1);
        private bool CanRepeat = false;
        private EssenceType LastRepeatSlot1 = (EssenceType)(-1);
        private EssenceType LastRepeatSlot2 = (EssenceType)(-1);
        private int RepeatCount = 1;
        private bool IsDraggingSlider = false;
        private int LastResultAmount = 3;

        private class EssenceUIParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public Color ParticleColor;
            public float Scale;
            public float InitialScale;
            public float Rotation;
            public float RotationSpeed;
            public int Age;
            public int MaxLifetime;
            public int ParticleType;
        }

        private List<EssenceUIParticle> uiParticles = new List<EssenceUIParticle>();

        private float PostCombineFadeProgress = 1f;

        public void Open()
        {
            IsOpen = true;
            IsClosing = false;
            OpenProgress = 0f;

            Main.playerInventory = false;

            Slot1 = (EssenceType)(-1);
            Slot2 = (EssenceType)(-1);

            IsSelecting = false;
            TargetSelecting = false;
            SelectionAnimProgress = 0f;
            
            AnimState = 0;
            AnimTimer = 0f;
            CanRepeat = false;

            LastRepeatSlot1 = (EssenceType)(-1);
            LastRepeatSlot2 = (EssenceType)(-1);

            RepeatCount = 1;
            IsDraggingSlider = false;
            LastResultAmount = 3;
            PostCombineFadeProgress = 1f;

            uiParticles.Clear();
            SoundEngine.PlaySound(SoundID.MenuOpen);
        }

        public void Close()
        {
            if (!IsOpen || IsClosing) return;
            IsClosing = true;
            Main.blockInput = false;
            SoundEngine.PlaySound(SoundID.MenuClose);
        }

        private void PlayEssenceCombineSound()
        {
            if (Main.netMode == NetmodeID.Server) return;
            SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/EssenceLinkingone") { Pitch = Main.rand.NextFloat(-0.2f, 0.2f), Volume = 1f });
        }
        private static float GetStaggeredEase(float progress, float delayFraction)
        {
            float p = MathHelper.Clamp((progress - delayFraction) / (1f - delayFraction), 0f, 1f);
            return MathSystem.EaseOutCubic(p);
        }

        private void TriggerEssenceFireworks(Vector2 center, EssenceType e1, EssenceType e2, EssenceType result)
        {
            Color color1 = EssencePalette.ColorOf(e1);
            Color color2 = EssencePalette.ColorOf(e2);
            Color resultColor = EssencePalette.ColorOf(result);

            uiParticles.Clear();

            uiParticles.Add(new EssenceUIParticle
            {
                Position = center,
                Velocity = Vector2.Zero,
                ParticleColor = color1,
                Scale = 0.2f,
                InitialScale = 1.8f,
                Rotation = 0f,
                RotationSpeed = 0f,
                Age = 0,
                MaxLifetime = 35,
                ParticleType = 1
            });

            uiParticles.Add(new EssenceUIParticle
            {
                Position = center,
                Velocity = Vector2.Zero,
                ParticleColor = color2,
                Scale = 0.2f,
                InitialScale = 1.5f,
                Rotation = 0f,
                RotationSpeed = 0f,
                Age = 0,
                MaxLifetime = 35,
                ParticleType = 1
            });

            uiParticles.Add(new EssenceUIParticle
            {
                Position = center,
                Velocity = Vector2.Zero,
                ParticleColor = Color.White,
                Scale = 0.1f,
                InitialScale = 1.0f,
                Rotation = 0f,
                RotationSpeed = 0f,
                Age = 0,
                MaxLifetime = 25,
                ParticleType = 1
            });

            int sparkCount = 52;
            for (int i = 0; i < sparkCount; i++)
            {
                float angle = (float)i / sparkCount * MathHelper.TwoPi + Main.rand.NextFloat(-0.08f, 0.08f);
                float speed = Main.rand.NextFloat(3.5f, 13f);
                Vector2 vel = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * speed;

                Color pColor;
                int colorType = i % 4;
                if (colorType == 0) pColor = color1;
                else if (colorType == 1) pColor = color2;
                else if (colorType == 2) pColor = resultColor;
                else pColor = Color.Lerp(color1, color2, Main.rand.NextFloat());

                uiParticles.Add(new EssenceUIParticle
                {
                    Position = center,
                    Velocity = vel,
                    ParticleColor = pColor,
                    Scale = Main.rand.NextFloat(0.45f, 1.15f),
                    InitialScale = Main.rand.NextFloat(0.45f, 1.15f),
                    Rotation = vel.ToRotation() + MathHelper.PiOver2,
                    RotationSpeed = Main.rand.NextFloat(-0.1f, 0.1f),
                    Age = 0,
                    MaxLifetime = Main.rand.Next(32, 60),
                    ParticleType = 0
                });
            }

            int glowCount = 18;
            for (int i = 0; i < glowCount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(2f, 7.5f);
                Vector2 vel = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * speed;
                Color pColor = (i % 2 == 0) ? color1 : color2;

                uiParticles.Add(new EssenceUIParticle
                {
                    Position = center,
                    Velocity = vel,
                    ParticleColor = pColor,
                    Scale = Main.rand.NextFloat(0.3f, 0.75f),
                    InitialScale = Main.rand.NextFloat(0.3f, 0.75f),
                    Rotation = 0f,
                    RotationSpeed = 0f,
                    Age = 0,
                    MaxLifetime = Main.rand.Next(25, 45),
                    ParticleType = 1
                });
            }
        }

        public void UpdateAnimation()
        {
            if (!IsOpen) return;
            Main.LocalPlayer.mouseInterface = true;

            Vector2 screenCenter = new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f);

            if (AnimState == 0 && PostCombineFadeProgress < 1f)
            {
                PostCombineFadeProgress = Math.Min(1f, PostCombineFadeProgress + 1f / 24f);
            }

            for (int i = uiParticles.Count - 1; i >= 0; i--)
            {
                var p = uiParticles[i];
                p.Age++;
                p.Position += p.Velocity;
                p.Velocity *= 0.93f;
                p.Rotation += p.RotationSpeed;

                if (p.Age >= p.MaxLifetime)
                {
                    uiParticles.RemoveAt(i);
                }
            }

            if (IsClosing)
            {
                OpenProgress -= 1f / 16f;
                if (OpenProgress <= 0f)
                {
                    OpenProgress = 0f;
                    IsClosing = false;
                    IsOpen = false;
                    return;
                }
            }
            else if (OpenProgress < 1f)
            {
                OpenProgress = Math.Min(1f, OpenProgress + 1f / 22f);
            }

            if (TargetSelecting)
            {
                if (SelectionAnimProgress < 1f)
                {
                    SelectionAnimProgress = Math.Min(1f, SelectionAnimProgress + 1f / 18f);
                }
            }
            else
            {
                if (SelectionAnimProgress > 0f)
                {
                    SelectionAnimProgress = Math.Max(0f, SelectionAnimProgress - 1f / 18f);
                    if (SelectionAnimProgress == 0f)
                    {
                        IsSelecting = false;
                    }
                }
            }

            if (PlayerInput.Triggers.JustPressed.Inventory || Main.LocalPlayer.dead)
            {
                Close();
                return;
            }

            if (AnimState == 1)
            {
                AnimTimer++;
                if (AnimTimer >= 90)
                {
                    var rPlayer = Main.LocalPlayer.GetModPlayer<ResearchPlayer>();
                    LastResultAmount = 3;
                    rPlayer.AddEssence(ResultType, LastResultAmount);
                    AnimState = 2;
                    AnimTimer = 0;
                    CanRepeat = true;

                    LastRepeatSlot1 = Slot1;
                    LastRepeatSlot2 = Slot2;

                    PlayEssenceCombineSound();
                    TriggerEssenceFireworks(screenCenter + new Vector2(0, -50), Slot1, Slot2, ResultType);
                }
            }
            else if (AnimState == 2)
            {
                AnimTimer++;
                if (AnimTimer >= 90)
                {
                    AnimState = 0;
                    AnimTimer = 0;
                    PostCombineFadeProgress = 0f;
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            if (!IsOpen) return;

            Vector2 screenCenter = new Vector2(Main.screenWidth / 2f, Main.screenHeight / 2f);

            float bgAlpha = OpenProgress * 0.75f;
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Black * bgAlpha);

            float closeEased = MathSystem.EaseOutCubic(OpenProgress);
            int closeY = (int)(20 + (1f - closeEased) * -60f);
            Rectangle closeRect = new Rectangle(Main.screenWidth - 60, closeY, 40, 40);

            if (!IsClosing && closeRect.Contains(Main.MouseScreen.ToPoint()))
            {
                Terraria.Utils.DrawInvBG(spriteBatch, closeRect, new Color(200, 40, 40, (int)(230 * closeEased)));
                if (Main.mouseLeft && Main.mouseLeftRelease)
                {
                    Close();
                    return;
                }
            }
            else
            {
                Terraria.Utils.DrawInvBG(spriteBatch, closeRect, new Color(120, 30, 30, (int)(200 * closeEased)));
            }
            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, "X", new Vector2(closeRect.X + 13, closeRect.Y + 8), Color.White * closeEased, 0f, Vector2.Zero, Vector2.One);

            if (AnimState == 0)
            {
                DrawNormalState(spriteBatch, screenCenter);
            }
            else if (AnimState == 1)
            {
                DrawCombineAnimation(spriteBatch, screenCenter);
            }
            else if (AnimState == 2)
            {
                DrawResultState(spriteBatch, screenCenter);
            }

            DrawUIParticles(spriteBatch);
        }

        private List<EssenceType> GetPossibleResults(EssenceType e1, EssenceType e2)
        {
            var results = new List<EssenceType>();
            if ((int)e1 == -1 || (int)e2 == -1) return results;

            bool Match(EssenceType a, EssenceType b)
            {
                return (e1 == a && e2 == b) || (e1 == b && e2 == a);
            }

            if (Match(EssenceType.Aer, EssenceType.Lux)) results.Add(EssenceType.Pyros);
            if (Match(EssenceType.Aer, EssenceType.Terra)) { results.Add(EssenceType.Umbra); results.Add(EssenceType.Cruor); }
            if (Match(EssenceType.Aer, EssenceType.Vita)) results.Add(EssenceType.Venenum);
            if (Match(EssenceType.Anima, EssenceType.Terra)) results.Add(EssenceType.Arena);
            if (Match(EssenceType.Aqua, EssenceType.Aqua)) results.Add(EssenceType.Glacies);
            if (Match(EssenceType.Terra, EssenceType.Venenum)) results.Add(EssenceType.Fungus);
            if (Match(EssenceType.Vita, EssenceType.Cruor)) results.Add(EssenceType.Anima);
            if (Match(EssenceType.Vita, EssenceType.Umbra)) results.Add(EssenceType.Anima);
            if (Match(EssenceType.Arena, EssenceType.Pyros)) results.Add(EssenceType.Regalis);
            if (Match(EssenceType.Venenum, EssenceType.Aqua)) results.Add(EssenceType.Acus);
            if (Match(EssenceType.Pyros, EssenceType.Anima)) { results.Add(EssenceType.Caro); results.Add(EssenceType.Mechanus); }
            if (Match(EssenceType.Pyros, EssenceType.Aer)) results.Add(EssenceType.Lux);
            if (Match(EssenceType.Aer, EssenceType.Arena)) results.Add(EssenceType.Fulgur);
            if (Match(EssenceType.Venenum, EssenceType.Vita)) results.Add(EssenceType.Silva);
            if (Match(EssenceType.Acus, EssenceType.Aqua)) results.Add(EssenceType.Chitinus);
            if (Match(EssenceType.Glacies, EssenceType.Aer)) results.Add(EssenceType.Pelagus);
            if (Match(EssenceType.Regalis, EssenceType.Aer)) results.Add(EssenceType.Imperium);
            return results;
        }

        private void DrawMorphedPanel(SpriteBatch spriteBatch, Vector2 screenCenter, float openAlpha)
        {
            float sideEased = MathSystem.EaseOutCubic(OpenProgress);
            float sideOffsetY = (1f - sideEased) * 140f;
            float morphT = MathSystem.EaseOutCubic(SelectionAnimProgress);

            var rPlayer = Main.LocalPlayer.GetModPlayer<ResearchPlayer>();
            var available = rPlayer.Essences.Where(kv => kv.Value > 0).ToList();

            int colsSide = 4;
            int rowsSide = Math.Max(1, (int)Math.Ceiling(available.Count / (double)colsSide));
            int contentHeightSide = 65 + rowsSide * 58 + 15;
            int panelHeightSide = (int)MathHelper.Clamp(contentHeightSide, 220, 540);
            int panelWidthSide = 290;
            int panelYSide = (int)(screenCenter.Y - panelHeightSide / 2f + sideOffsetY);

            Vector2 sidePos = new Vector2(screenCenter.X - 460, panelYSide);
            Vector2 sideSize = new Vector2(panelWidthSide, panelHeightSide);

            Vector2 centerPos = new Vector2(screenCenter.X - 250, screenCenter.Y - 220 + sideOffsetY);
            Vector2 centerSize = new Vector2(500, 440);

            Vector2 curPos = Vector2.Lerp(sidePos, centerPos, morphT);
            Vector2 curSize = Vector2.Lerp(sideSize, centerSize, morphT);
            Rectangle panelRect = new Rectangle((int)curPos.X, (int)curPos.Y, (int)curSize.X, (int)curSize.Y);

            Terraria.Utils.DrawInvBG(spriteBatch, panelRect, new Color(20, 24, 42, (int)(235 * openAlpha * sideEased)));

            string titleSide = (string)CombinerEssenceLanguageSystem.CE_Available;
            string titleCenter = (string)CombinerEssenceLanguageSystem.CE_Select;
            string title = morphT > 0.5f ? titleCenter : titleSide;

            Color titleColor = Color.Gold * openAlpha * sideEased;
            Vector2 titleSize = FontAssets.MouseText.Value.MeasureString(title) * 0.9f;

            ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, title, new Vector2(panelRect.Center.X - titleSize.X / 2f, panelRect.Y + 14), titleColor, 0f, Vector2.Zero, new Vector2(0.9f));

            if (available.Count == 0)
            {
                string emptyText = morphT > 0.5f ? "No essences available" : "No essences";
                Vector2 emptySize = FontAssets.MouseText.Value.MeasureString(emptyText);
                ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, emptyText, panelRect.Center.ToVector2() - emptySize / 2f, Color.Gray * openAlpha * sideEased, 0f, Vector2.Zero, Vector2.One);
                return;
            }

            int colsCenter = 6;

            int startXSide = (int)sidePos.X + 18;
            int startYSide = (int)sidePos.Y + 54;

            int cellWidthSide = 64;
            int cellHeightSide = 58;

            int startXCenter = (int)centerPos.X + 28;
            int startYCenter = (int)centerPos.Y + 54;

            int cellWidthCenter = 74;
            int cellHeightCenter = 65;

            for (int i = 0; i < available.Count; i++)
            {
                int colS = i % colsSide;
                int rowS = i / colsSide;
                Vector2 slotPosSide = new Vector2(startXSide + colS * cellWidthSide, startYSide + rowS * cellHeightSide);

                int colC = i % colsCenter;
                int rowC = i / colsCenter;
                Vector2 slotPosCenter = new Vector2(startXCenter + colC * cellWidthCenter, startYCenter + rowC * cellHeightCenter);

                Vector2 slotPos = Vector2.Lerp(slotPosSide, slotPosCenter, morphT);
                Rectangle itemRect = new Rectangle((int)slotPos.X, (int)slotPos.Y, 52, 52);

                bool hovered = !IsClosing && itemRect.Contains(Main.MouseScreen.ToPoint());
                bool isResearched = rPlayer.DiscoveredEssences.Contains((int)available[i].Key);

                if (hovered)
                {
                    if (isResearched)
                    {
                        Main.hoverItemName = EssencePalette.DisplayName(available[i].Key);
                    }
                    else
                    {
                        Main.hoverItemName = $"{EssencePalette.DisplayName(available[i].Key)}" + (string)CombinerEssenceLanguageSystem.CE_Not_Researched;
                    }
                }

                Texture2D slotTex = hovered ? (isResearched ? TextureAssets.InventoryBack2.Value : TextureAssets.InventoryBack10.Value) : TextureAssets.InventoryBack.Value;
                Color slotColor = (isResearched ? Color.White : Color.Gray * 0.5f) * openAlpha * sideEased;
                spriteBatch.Draw(slotTex, slotPos, null, slotColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

                var essenceTex = EssencePalette.GetTexture(available[i].Key);
                if (essenceTex != null)
                {
                    Vector2 iconCenter = slotPos + new Vector2(26, 26);
                    Color iconColor = (isResearched ? Color.White : Color.Gray * 0.35f) * openAlpha * sideEased;
                    spriteBatch.Draw(essenceTex, iconCenter, null, iconColor, 0f, essenceTex.Size() / 2f, 0.9f, SpriteEffects.None, 0f);
                }

                string countStr = available[i].Value.ToString();
                ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, countStr, slotPos + new Vector2(8, 28), (isResearched ? Color.White : Color.Gray * 0.7f) * openAlpha * sideEased, 0f, Vector2.Zero, new Vector2(0.75f));

                if (!IsClosing && TargetSelecting && morphT > 0.6f && hovered && Main.mouseLeft && Main.mouseLeftRelease)
                {
                    if (isResearched)
                    {
                        if (SelectingSlot == 1)
                        {
                            if (Slot1 != available[i].Key)
                            {
                                Slot1 = available[i].Key;
                                CanRepeat = false;
                            }
                        }
                        else
                        {
                            if (Slot2 != available[i].Key)
                            {
                                Slot2 = available[i].Key;
                                CanRepeat = false;
                            }
                        }
                        TargetSelecting = false;
                        SoundEngine.PlaySound(SoundID.MenuTick);
                    }
                    else
                    {
                        SoundEngine.PlaySound(SoundID.MenuClose);
                    }
                }
            }

            if (morphT > 0.05f)
            {
                Rectangle cancelRect = new Rectangle(panelRect.Center.X - 60, panelRect.Bottom - 44, 120, 32);
                bool cancelHovered = !IsClosing && cancelRect.Contains(Main.MouseScreen.ToPoint());
                string cancel_text = (string)CombinerEssenceLanguageSystem.CE_Cancel;
                Color cancelColor = cancelHovered ? new Color(180, 50, 50, (int)(220 * openAlpha * morphT)) : new Color(60, 60, 80, (int)(200 * openAlpha * morphT));

                Terraria.Utils.DrawInvBG(spriteBatch, cancelRect, cancelColor);
                ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, cancel_text, new Vector2(cancelRect.X + 32, cancelRect.Y + 5), Color.White * openAlpha * morphT, 0f, Vector2.Zero, Vector2.One);

                if (!IsClosing && TargetSelecting && morphT > 0.6f && cancelHovered && Main.mouseLeft && Main.mouseLeftRelease)
                {
                    TargetSelecting = false;
                    SoundEngine.PlaySound(SoundID.MenuTick);
                }
            }
        }

        private void DrawNormalState(SpriteBatch spriteBatch, Vector2 screenCenter)
        {
            float sideEased = MathSystem.EaseOutCubic(OpenProgress);
            float sideOffsetY = (1f - sideEased) * 140f;
            float fadeAlpha = MathSystem.EaseOutCubic(PostCombineFadeProgress);

            DrawMorphedPanel(spriteBatch, screenCenter, OpenProgress * fadeAlpha);

            float morphT = MathSystem.EaseOutCubic(SelectionAnimProgress);
            float mainAlpha = sideEased * (1f - morphT) * fadeAlpha;

            if (mainAlpha > 0.01f)
            {
                float slot1Eased = GetStaggeredEase(OpenProgress, 0.08f);
                float slot1OffsetY = (1f - slot1Eased) * 180f;
                Vector2 slot1Pos = screenCenter + new Vector2(-100, -50 + slot1OffsetY);
                DrawSlot(spriteBatch, slot1Pos, Slot1, 1, mainAlpha);

                float slot2Eased = GetStaggeredEase(OpenProgress, 0.16f);
                float slot2OffsetY = (1f - slot2Eased) * 180f;
                Vector2 slot2Pos = screenCenter + new Vector2(100, -50 + slot2OffsetY);
                DrawSlot(spriteBatch, slot2Pos, Slot2, 2, mainAlpha);

                var possibleResults = GetPossibleResults(Slot1, Slot2);
                if (possibleResults.Count > 0)
                {
                    float btnEased = GetStaggeredEase(OpenProgress, 0.24f);
                    float btnOffsetY = (1f - btnEased) * 220f;
                    Rectangle btnRect = new Rectangle((int)screenCenter.X - 80, (int)(screenCenter.Y + 50 + btnOffsetY), 160, 45);

                    bool hovered = !IsClosing && !TargetSelecting && btnRect.Contains(Main.MouseScreen.ToPoint());
                    var rPlayer = Main.LocalPlayer.GetModPlayer<ResearchPlayer>();
                    
                    bool inputsResearched = rPlayer.DiscoveredEssences.Contains((int)Slot1) && rPlayer.DiscoveredEssences.Contains((int)Slot2);
                    List<EssenceType> unresearchedResults = new List<EssenceType>();
                    foreach (var res in possibleResults)
                    {
                        if (!rPlayer.DiscoveredEssences.Contains((int)res))
                        {
                            unresearchedResults.Add(res);
                        }
                    }
                    bool resultsResearched = unresearchedResults.Count == 0;
                    bool canCombine = inputsResearched && resultsResearched;

                    Color btnBgColor = hovered ? (canCombine ? new Color(220, 160, 30, (int)(230 * mainAlpha)) : new Color(180, 40, 40, (int)(230 * mainAlpha))) : (canCombine ? new Color(140, 100, 20, (int)(200 * mainAlpha)) : new Color(100, 30, 30, (int)(200 * mainAlpha)));
                    Terraria.Utils.DrawInvBG(spriteBatch, btnRect, btnBgColor);

                    string text = canCombine ? (string)CombinerEssenceLanguageSystem.CE_Combine : (string)CombinerEssenceLanguageSystem.CE_Locked;
                    Vector2 textSize = FontAssets.MouseText.Value.MeasureString(text);
                    ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, text, new Vector2(btnRect.Center.X - textSize.X / 2, btnRect.Center.Y - textSize.Y / 2), Color.White * mainAlpha, 0f, Vector2.Zero, Vector2.One);

                    if (hovered)
                    {
                        if (!inputsResearched)
                        {
                            Main.hoverItemName = (string)CombinerEssenceLanguageSystem.CE_Essences_not_rsched;
                        }
                        else if (!resultsResearched)
                        {
                            string namesStr = string.Join(", ", unresearchedResults.Select(r => EssencePalette.DisplayName(r)));

                            Main.hoverItemName = (string)CombinerEssenceLanguageSystem.CE_Result_not_rsched + " " + $"{namesStr}";
                        }
                    }

                    if (!IsClosing && !TargetSelecting && hovered && Main.mouseLeft && Main.mouseLeftRelease)
                    {
                        if (canCombine)
                        {
                            bool sameType = Slot1 == Slot2;
                            bool canConsume = sameType ? rPlayer.GetEssence(Slot1) >= 2 : (rPlayer.GetEssence(Slot1) >= 1 && rPlayer.GetEssence(Slot2) >= 1);

                            if (canConsume)
                            {
                                rPlayer.AddEssence(Slot1, -1);
                                rPlayer.AddEssence(Slot2, -1);

                                ResultType = possibleResults[Main.rand.Next(possibleResults.Count)];
                                AnimState = 1;
                                AnimTimer = 0f;
                                
                                if (Main.netMode == NetmodeID.Server) return;
                                SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/EssenceCombinerStart") { Pitch = Main.rand.NextFloat(-0.2f, 0.2f), Volume = 1f });
                            }
                        }
                        else
                        {
                            SoundEngine.PlaySound(SoundID.MenuClose);
                        }
                    }

                    if (CanRepeat && Slot1 == LastRepeatSlot1 && Slot2 == LastRepeatSlot2)
                    {
                        float repeatEased = GetStaggeredEase(OpenProgress, 0.28f);
                        float repeatOffsetY = (1f - repeatEased) * 240f;

                        bool sameType = Slot1 == Slot2;
                        int avail1 = rPlayer.GetEssence(Slot1);
                        int avail2 = rPlayer.GetEssence(Slot2);

                        int maxRepeats = sameType ? (avail1 / 2) : Math.Min(avail1, avail2);
                        if (maxRepeats < 1) maxRepeats = 1;

                        RepeatCount = (int)MathHelper.Clamp(RepeatCount, 1, maxRepeats);

                        Rectangle sliderTrackRect = new Rectangle((int)screenCenter.X - 100, (int)(screenCenter.Y + 130 + repeatOffsetY), 200, 16);
                        Rectangle sliderHitbox = new Rectangle(sliderTrackRect.X - 10, sliderTrackRect.Y - 8, sliderTrackRect.Width + 20, sliderTrackRect.Height + 16);

                        if (!IsClosing && !TargetSelecting && Main.mouseLeft)
                        {
                            if (IsDraggingSlider || (sliderHitbox.Contains(Main.MouseScreen.ToPoint()) && Main.mouseLeftRelease))
                            {
                                IsDraggingSlider = true;
                            }
                        }
                        else
                        {
                            IsDraggingSlider = false;
                        }

                        if (IsDraggingSlider && maxRepeats > 1)
                        {
                            float mouseRelX = MathHelper.Clamp(Main.MouseScreen.X - sliderTrackRect.X, 0f, sliderTrackRect.Width);
                            float pct = mouseRelX / sliderTrackRect.Width;

                            RepeatCount = (int)Math.Round(1 + pct * (maxRepeats - 1));
                            RepeatCount = (int)MathHelper.Clamp(RepeatCount, 1, maxRepeats);
                        }

                        float sliderProgress = maxRepeats > 1 ? (float)(RepeatCount - 1) / (maxRepeats - 1) : 1f;

                        Terraria.Utils.DrawInvBG(spriteBatch, sliderTrackRect, new Color(20, 24, 40, (int)(220 * mainAlpha)));

                        int fillWidth = (int)(sliderProgress * sliderTrackRect.Width);
                        if (fillWidth > 0)
                        {
                            Rectangle fillRect = new Rectangle(sliderTrackRect.X, sliderTrackRect.Y, fillWidth, sliderTrackRect.Height);
                            spriteBatch.Draw(TextureAssets.MagicPixel.Value, fillRect, new Color(220, 170, 30, (int)(180 * mainAlpha)));
                        }

                        int knobX = (int)(sliderTrackRect.X + sliderProgress * (sliderTrackRect.Width - 14));
                        Rectangle knobRect = new Rectangle(knobX, sliderTrackRect.Y - 3, 14, 22);

                        bool knobHovered = !IsClosing && !TargetSelecting && knobRect.Contains(Main.MouseScreen.ToPoint());
                        Terraria.Utils.DrawInvBG(spriteBatch, knobRect, (knobHovered || IsDraggingSlider) ? new Color(255, 230, 100, (int)(240 * mainAlpha)) : new Color(180, 150, 40, (int)(220 * mainAlpha)));

                        string sliderInfo = (string)CombinerEssenceLanguageSystem.CE_Quantity + " " + $"x{RepeatCount} (max: {maxRepeats})";
                        Vector2 infoSize = FontAssets.MouseText.Value.MeasureString(sliderInfo) * 0.8f;
                        ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, sliderInfo, new Vector2(screenCenter.X - infoSize.X / 2f, sliderTrackRect.Y - 18), Color.Gold * mainAlpha, 0f, Vector2.Zero, new Vector2(0.8f));

                        Rectangle repeatBtnRect = new Rectangle((int)screenCenter.X - 80, (int)(screenCenter.Y + 162 + repeatOffsetY), 160, 42);
                        bool repeatHovered = !IsClosing && !TargetSelecting && !IsDraggingSlider && repeatBtnRect.Contains(Main.MouseScreen.ToPoint());

                        bool canConsume = sameType ? (avail1 >= 2 * RepeatCount) : (avail1 >= RepeatCount && avail2 >= RepeatCount);
                        bool canRepeatCombine = canCombine && canConsume;

                        Color repeatBtnColor = repeatHovered ? (canRepeatCombine ? new Color(40, 180, 80, (int)(230 * mainAlpha)) : new Color(180, 40, 40, (int)(230 * mainAlpha))) : (canRepeatCombine ? new Color(20, 110, 50, (int)(200 * mainAlpha)) : new Color(100, 30, 30, (int)(200 * mainAlpha)));
                        Terraria.Utils.DrawInvBG(spriteBatch, repeatBtnRect, repeatBtnColor);

                        string repeatText = (string)CombinerEssenceLanguageSystem.CE_Repeat + " " + $"x{RepeatCount}";
                        Vector2 repeatTextSize = FontAssets.MouseText.Value.MeasureString(repeatText);
                        ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, repeatText, new Vector2(repeatBtnRect.Center.X - repeatTextSize.X / 2, repeatBtnRect.Center.Y - repeatTextSize.Y / 2), Color.White * mainAlpha, 0f, Vector2.Zero, Vector2.One);

                        if (repeatHovered)
                        {
                            if (!canConsume)
                            {
                                Main.hoverItemName = (string)CombinerEssenceLanguageSystem.CE_Not_enough;
                            }
                        }

                        if (!IsClosing && !TargetSelecting && !IsDraggingSlider && repeatHovered && Main.mouseLeft && Main.mouseLeftRelease)
                        {
                            if (canRepeatCombine)
                            {
                                if (sameType)
                                {
                                    rPlayer.AddEssence(Slot1, -2 * RepeatCount);
                                }
                                else
                                {
                                    rPlayer.AddEssence(Slot1, -RepeatCount);
                                    rPlayer.AddEssence(Slot2, -RepeatCount);
                                }

                                LastResultAmount = 3 * RepeatCount;

                                rPlayer.AddEssence(ResultType, LastResultAmount);

                                AnimState = 2;
                                AnimTimer = 0f;

                                PlayEssenceCombineSound();
                                TriggerEssenceFireworks(screenCenter + new Vector2(0, -50), Slot1, Slot2, ResultType);
                            }
                            else
                            {
                                SoundEngine.PlaySound(SoundID.MenuClose);
                            }
                        }
                    }
                }
            }
        }

        private void DrawSlot(SpriteBatch spriteBatch, Vector2 pos, EssenceType type, int slotId, float alpha = 1f)
        {
            Rectangle rect = new Rectangle((int)pos.X - 26, (int)pos.Y - 26, 52, 52);
            bool hovered = !IsClosing && !TargetSelecting && rect.Contains(Main.MouseScreen.ToPoint());
            bool hasEssence = (int)type != -1;

            if (hasEssence)
            {
                Rectangle innerBgRect = new Rectangle(rect.X + 4, rect.Y + 4, rect.Width - 8, rect.Height - 8);
                Color darkRedBg = (hovered ? new Color(70, 10, 15, 210) : new Color(40, 6, 10, 180)) * alpha;
                spriteBatch.Draw(TextureAssets.MagicPixel.Value, innerBgRect, darkRedBg);

                Texture2D slotTex = hovered ? TextureAssets.InventoryBack2.Value : TextureAssets.InventoryBack14.Value;
                Color darkRedBorder = (hovered ? new Color(150, 35, 45) : new Color(100, 22, 30)) * alpha;
                spriteBatch.Draw(slotTex, rect.TopLeft(), null, darkRedBorder, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

                var essenceTex = EssencePalette.GetTexture(type);
                if (essenceTex != null)
                {
                    var rPlayer = Main.LocalPlayer.GetModPlayer<ResearchPlayer>();
                    bool isResearched = rPlayer.DiscoveredEssences.Contains((int)type);

                    Color drawColor = (isResearched ? Color.White : Color.Gray * 0.4f) * alpha;
                    spriteBatch.Draw(essenceTex, pos, null, drawColor, 0f, essenceTex.Size() / 2f, 1f, SpriteEffects.None, 0f);

                    if (hovered)
                    {
                        if (isResearched)
                        {
                            Main.hoverItemName = EssencePalette.DisplayName(type);
                        }
                        else
                        {
                            Main.hoverItemName = $"{EssencePalette.DisplayName(type)}" + (string)CombinerEssenceLanguageSystem.CE_Not_Researched;
                        }
                    }
                }
            }
            else
            {
                Texture2D slotTex = hovered ? TextureAssets.InventoryBack2.Value : TextureAssets.InventoryBack.Value;
                spriteBatch.Draw(slotTex, rect.TopLeft(), null, Color.White * alpha, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

                string text = "+";
                Vector2 textSize = FontAssets.MouseText.Value.MeasureString(text);
                ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, text, pos - textSize / 2f, Color.Gold * alpha, 0f, Vector2.Zero, Vector2.One);
            }

            if (!IsClosing && !TargetSelecting && hovered && Main.mouseLeft && Main.mouseLeftRelease)
            {
                IsSelecting = true;
                TargetSelecting = true;
                SelectingSlot = slotId;
                SoundEngine.PlaySound(SoundID.MenuTick);
            }
        }

        private void DrawCombineAnimation(SpriteBatch spriteBatch, Vector2 screenCenter)
        {
            float progress = AnimTimer / 90f;
            float easeProgress = progress * progress * progress;
            
            Vector2 slot1Start = screenCenter + new Vector2(-100, -50);
            Vector2 slot2Start = screenCenter + new Vector2(100, -50);
            Vector2 target = screenCenter + new Vector2(0, -50);
            Vector2 pos1 = Vector2.Lerp(slot1Start, target, easeProgress);
            Vector2 pos2 = Vector2.Lerp(slot2Start, target, easeProgress);
            
            float rotation = progress * MathHelper.TwoPi * 4f;
            float scale = 1f - progress * 0.5f;

            if ((int)Slot1 != -1)
            {
                var tex1 = EssencePalette.GetTexture(Slot1);
                if (tex1 != null)
                    spriteBatch.Draw(tex1, pos1, null, Color.White, rotation, tex1.Size() / 2f, scale, SpriteEffects.None, 0f);
            }
            if ((int)Slot2 != -1)
            {
                var tex2 = EssencePalette.GetTexture(Slot2);
                if (tex2 != null)
                    spriteBatch.Draw(tex2, pos2, null, Color.White, -rotation, tex2.Size() / 2f, scale, SpriteEffects.None, 0f);
            }
            if (progress < 0.9f)
            {
                float dist = Vector2.Distance(pos1, pos2);
                Vector2 dir = pos2 - pos1;
                dir.Normalize();
                spriteBatch.Draw(TextureAssets.MagicPixel.Value, pos1, new Rectangle(0, 0, 1, 1), Color.Lerp(EssencePalette.ColorOf(Slot1), EssencePalette.ColorOf(Slot2), 0.5f) * 0.5f, dir.ToRotation(), new Vector2(0, 0.5f), new Vector2(dist, 4f + progress * 20f), SpriteEffects.None, 0f);
            }
        }

        private void DrawResultState(SpriteBatch spriteBatch, Vector2 screenCenter)
        {
            float progress = AnimTimer / 90f;
            Vector2 target = screenCenter + new Vector2(0, -50);
            float alpha = 1f;
            if (progress > 0.7f) alpha = 1f - ((progress - 0.7f) / 0.3f);
            float scale = 1f;
            if (progress < 0.2f) scale = progress / 0.2f;

            var tex = EssencePalette.GetTexture(ResultType);
            if (tex != null)
            {
                Vector2 pos1 = target + new Vector2(-40, (float)Math.Sin(Main.GlobalTimeWrappedHourly * 4f) * 5f);
                Vector2 pos2 = target + new Vector2(0, -20 + (float)Math.Cos(Main.GlobalTimeWrappedHourly * 4f) * 5f);
                Vector2 pos3 = target + new Vector2(40, (float)Math.Sin(Main.GlobalTimeWrappedHourly * 4f + 1f) * 5f);

                spriteBatch.Draw(tex, pos1, null, Color.White * alpha, 0f, tex.Size() / 2f, scale, SpriteEffects.None, 0f);
                spriteBatch.Draw(tex, pos2, null, Color.White * alpha, 0f, tex.Size() / 2f, scale, SpriteEffects.None, 0f);
                spriteBatch.Draw(tex, pos3, null, Color.White * alpha, 0f, tex.Size() / 2f, scale, SpriteEffects.None, 0f);

                ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, "+" + LastResultAmount, target + new Vector2(-14, 40), Color.LightGreen * alpha, 0f, Vector2.Zero, Vector2.One);
            }
        }

        private void DrawUIParticles(SpriteBatch spriteBatch)
        {
            if (uiParticles.Count == 0) return;

            Texture2D sparkTex = ModContent.Request<Texture2D>("jarkos/particles/Spark", AssetRequestMode.ImmediateLoad).Value;
            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/particles/Circular_glow", AssetRequestMode.ImmediateLoad).Value;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

            foreach (var p in uiParticles)
            {
                float progress = (float)p.Age / p.MaxLifetime;
                float alpha = 1f - progress;

                Texture2D tex = p.ParticleType == 0 ? sparkTex : glowTex;
                Vector2 origin = tex.Size() * 0.5f;

                float currentScale;
                if (p.ParticleType == 1 && p.Velocity == Vector2.Zero)
                {
                    currentScale = MathHelper.Lerp(p.Scale, p.InitialScale, MathF.Sin(progress * MathF.PI));
                }
                else
                {
                    currentScale = p.Scale * alpha;
                }

                Color drawColor = p.ParticleColor * (alpha * OpenProgress);
                spriteBatch.Draw(tex, p.Position, null, drawColor, p.Rotation, origin, currentScale, SpriteEffects.None, 0f);
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
        }
    }

    public class CombinerEssenceLanguageSystem : ModSystem
	{
		public static LocalizedText CE_Select { get; private set; }
		public static LocalizedText CE_Available { get; private set; }
		public static LocalizedText CE_No_Available { get; private set; }
		public static LocalizedText CE_Not_Researched { get; private set; }
		public static LocalizedText CE_Cancel { get; private set; }
		public static LocalizedText CE_Locked { get; private set; }
		public static LocalizedText CE_Essences_not_rsched { get; private set; }
		public static LocalizedText CE_Result_not_rsched { get; private set; }
		public static LocalizedText CE_Max { get; private set; }
		public static LocalizedText CE_Quantity { get; private set; }
		public static LocalizedText CE_Repeat { get; private set; }
        public static LocalizedText CE_Combine { get; private set; }
		public static LocalizedText CE_Not_enough { get; private set; }
		public static LocalizedText CE_not_rsched { get; private set; }
        public override void Load()
        {
            string category = "UI";
			CE_Select ??= Mod.GetLocalization($"{category}.CE_Select");
			CE_Available ??= Mod.GetLocalization($"{category}.CE_Available");
			CE_No_Available ??= Mod.GetLocalization($"{category}.CE_No_Available");
			CE_Not_Researched ??= Mod.GetLocalization($"{category}.CE_Not_Researched");
			CE_Cancel ??= Mod.GetLocalization($"{category}.CE_Cancel");
			CE_Locked ??= Mod.GetLocalization($"{category}.CE_Locked");
			CE_Essences_not_rsched ??= Mod.GetLocalization($"{category}.CE_Essences_not_rsched");
			CE_Result_not_rsched ??= Mod.GetLocalization($"{category}.CE_Result_not_rsched");
			CE_Max ??= Mod.GetLocalization($"{category}.CE_Max");
			CE_Quantity ??= Mod.GetLocalization($"{category}.CE_Quantity");
			CE_Repeat ??= Mod.GetLocalization($"{category}.CE_Repeat");
            CE_Combine ??= Mod.GetLocalization($"{category}.CE_Combine");
			CE_Not_enough ??= Mod.GetLocalization($"{category}.CE_Not_enough");
			CE_not_rsched ??= Mod.GetLocalization($"{category}.CE_not_rsched");
        }
	}
}
