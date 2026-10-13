using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using jarkos.items;
using Terraria.UI.Chat;
using jarkos.core.Data;
using Terraria.Audio;
using Terraria.ID;
using jarkos.items.usable;
using Terraria.Localization;
using Terraria.GameInput;

namespace jarkos.UI
{
    public class RuneAltarUI
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
                Velocity *= 0.982f;
                Rotation += RotationSpeed;

                float ratio = TimeAlive / (float)Lifetime;
                Opacity = MathHelper.Clamp(1f - ratio * ratio, 0f, 1f);
                Scale *= 0.996f;
            }

            public void Draw(SpriteBatch sb, Texture2D tex, float alpha)
            {
                if (IsDead || tex == null) return;
                Color drawColor = ParticleColor * (Opacity * alpha);

                sb.Draw(tex, Position, null, drawColor, Rotation, tex.Size() / 2f, Scale, SpriteEffects.None, 0f);
            }
        }

        private class UISparkParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public Color ParticleColor;
            public Vector2 TargetCenter;
            public float Scale;
            public float Opacity;
            public float Rotation;
            public int Lifetime;
            public int TimeAlive;

            public bool IsDead => TimeAlive >= Lifetime || Opacity <= 0.01f;

            public UISparkParticle(Vector2 position, Vector2 velocity, Color color, Vector2 targetCenter, int lifetime, float scale, float opacity)
            {
                Position = position;
                Velocity = velocity;
                ParticleColor = color;
                TargetCenter = targetCenter;
                Lifetime = lifetime;
                Scale = scale;
                Opacity = opacity;
                Rotation = velocity.ToRotation() + MathHelper.PiOver2;
                TimeAlive = 0;
            }

            public void Update()
            {
                TimeAlive++;

                Vector2 toTarget = TargetCenter - Position;
                float dist = toTarget.Length();
                if (dist > 4f)
                {
                    Vector2 desiredVel = Vector2.Normalize(toTarget) * MathHelper.Clamp(dist * 0.15f, 4f, 12f);
                    Velocity = Vector2.Lerp(Velocity, desiredVel, 0.14f);
                }
                else
                {
                    Opacity *= 0.4f;
                }

                Position += Velocity;
                Rotation = Velocity.ToRotation() + MathHelper.PiOver2;

                float ratio = TimeAlive / (float)Lifetime;
                Opacity = MathHelper.Clamp(1f - ratio * ratio, 0f, 1f);
            }

            public void Draw(SpriteBatch sb, Texture2D tex, float alpha)
            {
                if (IsDead || tex == null) return;
                Color drawColor = ParticleColor * (Opacity * alpha);
                sb.Draw(tex, Position, null, drawColor, Rotation, tex.Size() / 2f, Scale, SpriteEffects.None, 0f);
            }
        }

        private class UIPixelParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public Color ParticleColor;
            public float Size;
            public float Opacity;
            public int Lifetime;
            public int TimeAlive;

            public bool IsDead => TimeAlive >= Lifetime || Opacity <= 0.01f;

            public UIPixelParticle(Vector2 position, Vector2 velocity, Color color, int lifetime, float size)
            {
                Position = position;
                Velocity = velocity;
                ParticleColor = color;
                Lifetime = lifetime;
                Size = size;
                Opacity = 1f;
                TimeAlive = 0;
            }

            public void Update()
            {
                TimeAlive++;
                Position += Velocity;
                Velocity *= 0.94f;

                float ratio = TimeAlive / (float)Lifetime;
                Opacity = MathHelper.Clamp(1f - ratio, 0f, 1f);
                Size *= 0.96f;
            }

            public void Draw(SpriteBatch sb, Texture2D tex, float alpha)
            {
                if (IsDead || tex == null) return;
                Rectangle rect = new Rectangle((int)(Position.X - Size / 2f), (int)(Position.Y - Size / 2f), (int)Size, (int)Size);
                sb.Draw(tex, rect, ParticleColor * (Opacity * alpha));
            }
        }

        private readonly List<UIStarParticle> _starParticles = new();
        private readonly List<UISparkParticle> _sparkParticles = new();
        private readonly List<UIPixelParticle> _pixelParticles = new();

        public bool IsOpen { get; private set; }
        public bool IsClosing { get; private set; }
        public float OpenProgress { get; private set; } = 0f;

        private Item _armorItem = new Item();
        private Item[] _placedRunes = new Item[4];

        private bool _isAnimating;
        private float _animTimer;
        private float _flashAlpha = 0f;
        private float _armorErrorBlinkTimer = 0f;

        private Point? _tileCoords;

        public void Open(Item armorItem = null, Point? tileCoords = null)
        {
            IsOpen = true;
            IsClosing = false;
            OpenProgress = 0f;
            _tileCoords = tileCoords;
            _armorItem = (armorItem != null && !armorItem.IsAir) ? armorItem.Clone() : new Item();
            _isAnimating = false;
            _animTimer = 0f;
            _flashAlpha = 0f;
            _armorErrorBlinkTimer = 0f;
            _starParticles.Clear();
            _sparkParticles.Clear();
            _pixelParticles.Clear();

            for (int i = 0; i < 4; i++)
            {
                _placedRunes[i] = new Item();
            }

            Main.playerInventory = true;
            SoundEngine.PlaySound(SoundID.MenuOpen);
        }

        public void Close()
        {
            if (!IsOpen || IsClosing) return;
            IsClosing = true;
            SoundEngine.PlaySound(SoundID.MenuClose);
        }

        private void ReturnAllItems()
        {
            Player player = Main.LocalPlayer;
            if (_armorItem != null && !_armorItem.IsAir)
            {
                player.QuickSpawnItem(player.GetSource_Misc("RuneAltarReturn"), _armorItem, 1);
                _armorItem = new Item();
            }

            for (int i = 0; i < 4; i++)
            {
                if (_placedRunes[i] != null && !_placedRunes[i].IsAir)
                {
                    player.QuickSpawnItem(player.GetSource_Misc("RuneAltarReturn"), _placedRunes[i], 1);
                    _placedRunes[i] = new Item();
                }
            }
        }
        public void Update(GameTime gameTime)
        {
            if (!IsOpen) return;
            Player player = Main.LocalPlayer;

            if (IsClosing)
            {
                OpenProgress -= (float)gameTime.ElapsedGameTime.TotalSeconds / 0.4f;
                if (OpenProgress <= 0f)
                {
                    OpenProgress = 0f;
                    IsClosing = false;
                    IsOpen = false;
                    ReturnAllItems();
                    _starParticles.Clear();
                    _sparkParticles.Clear();
                    _pixelParticles.Clear();
                    return;
                }
            }
            else if (OpenProgress < 1f)
            {
                OpenProgress = Math.Min(1f, OpenProgress + (float)gameTime.ElapsedGameTime.TotalSeconds / 0.5f);
            }

            player.mouseInterface = true;

            if (!IsClosing)
            {
                if (player.dead || !player.active || PlayerInput.Triggers.JustPressed.Inventory || Main.mapFullscreen)
                {
                    Close();
                }
                else if (_tileCoords.HasValue)
                {
                    Vector2 tileWorldPos = new Vector2(_tileCoords.Value.X * 16f + 24f, _tileCoords.Value.Y * 16f + 24f);
                    if (Vector2.Distance(player.Center, tileWorldPos) > 192f)
                    {
                        Close();
                    }
                }
            }

            Main.playerInventory = true;

            if (_flashAlpha > 0f)
            {
                _flashAlpha *= 0.90f;
                if (_flashAlpha < 0.01f) _flashAlpha = 0f;
            }

            if (_armorErrorBlinkTimer > 0f)
            {
                _armorErrorBlinkTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds * 1.0f;
                if (_armorErrorBlinkTimer < 0f) _armorErrorBlinkTimer = 0f;
            }

            Vector2 basePos = GetUIBasePos();
            Vector2 armorCenter = basePos + new Vector2(60f, 60f);

            if (_isAnimating)
            {
                _animTimer += (float)gameTime.ElapsedGameTime.TotalSeconds * 0.25f;
                float easedAnim = MathSystem.EaseOutCubic(_animTimer);

                for (int s = 0; s < 2; s++)
                {
                    float spawnDist = Main.rand.NextFloat(130f, 220f);
                    float spawnAngle = Main.rand.NextFloat(MathHelper.TwoPi);

                    Vector2 sparkPos = armorCenter + new Vector2(MathF.Cos(spawnAngle), MathF.Sin(spawnAngle)) * spawnDist;
                    Vector2 sparkVel = Vector2.Normalize(armorCenter - sparkPos) * Main.rand.NextFloat(3f, 6f);

                    Color sparkColor = Main.rand.NextBool() ? Color.White : new Color(255, 130, 130);
                    float sparkScale = Main.rand.NextFloat(0.35f, 0.65f);

                    _sparkParticles.Add(new UISparkParticle(sparkPos, sparkVel, sparkColor, armorCenter, 40, sparkScale, 1f));
                }

                for (int i = 0; i < 4; i++)
                {
                    if (_placedRunes[i] != null && !_placedRunes[i].IsAir)
                    {
                        Vector2 startPos = GetRuneSlotCenter(basePos, i);

                        int dirMultiplier = (i % 2 == 0) ? 1 : -1;
                        float initialAngle = i * MathHelper.PiOver2;
                        float spiralAngle = initialAngle + (1f - easedAnim) * MathHelper.TwoPi * 2.0f * dirMultiplier;

                        Vector2 spiralOffset = new Vector2(MathF.Cos(spiralAngle), MathF.Sin(spiralAngle)) * (1f - easedAnim) * 75f;
                        Vector2 currentPos = Vector2.Lerp(startPos, armorCenter, easedAnim) + spiralOffset;

                        for (int p = 0; p < 2; p++)
                        {
                            Vector2 pxVel = Main.rand.NextVector2Circular(1.5f, 1.5f);
                            Color pxColor = Color.Lerp(new Color(120, 10, 20), new Color(180, 25, 35), Main.rand.NextFloat());
                            float pxScale = Main.rand.NextFloat(3f, 6f);

                            _pixelParticles.Add(new UIPixelParticle(currentPos + Main.rand.NextVector2Circular(6f, 6f), pxVel, pxColor, 35, pxScale));
                        }

                        if (Main.rand.NextBool(2))
                        {
                            Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                            Color pColor = Color.Lerp(Color.Gold, Color.OrangeRed, Main.rand.NextFloat());
                            _starParticles.Add(new UIStarParticle(currentPos + Main.rand.NextVector2Circular(8f, 8f), vel, pColor, 50, Main.rand.NextFloat(0.12f, 0.22f), 0.9f, Main.rand.NextFloat(-0.1f, 0.1f)));
                        }
                    }
                }

                if (_animTimer >= 1f)
                {
                    _flashAlpha = 1.0f;

                    for (int i = 0; i < 50; i++)
                    {
                        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                        float speed = Main.rand.NextFloat(7f, 19f);

                        Vector2 vel = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * speed;
                        Color pColor = Color.Lerp(Color.Gold, Color.OrangeRed, Main.rand.NextFloat());

                        float scale = Main.rand.NextFloat(0.15f, 0.35f);
                        float rotSpeed = Main.rand.NextFloat(-0.2f, 0.2f);
                        int life = Main.rand.Next(90, 160);

                        _starParticles.Add(new UIStarParticle(armorCenter, vel, pColor, life, scale, 1f, rotSpeed));
                    }

                    FinishEnchanting();
                }
            }

            for (int i = _sparkParticles.Count - 1; i >= 0; i--)
            {
                _sparkParticles[i].Update();
                if (_sparkParticles[i].IsDead) _sparkParticles.RemoveAt(i);
            }

            for (int i = _pixelParticles.Count - 1; i >= 0; i--)
            {
                _pixelParticles[i].Update();
                if (_pixelParticles[i].IsDead) _pixelParticles.RemoveAt(i);
            }

            for (int i = _starParticles.Count - 1; i >= 0; i--)
            {
                _starParticles[i].Update();
                if (_starParticles[i].IsDead) _starParticles.RemoveAt(i);
            }
        }

        private Vector2 GetUIBasePos()
        {
            float eased = MathSystem.EaseOutCubic(OpenProgress);
            float offsetY = (1f - eased) * (Main.screenHeight + 300f);
            return new Vector2(Main.screenWidth / 2f + 60f, Main.screenHeight / 2f - 110f + offsetY);
        }

        private Vector2 GetRuneSlotCenter(Vector2 basePos, int index)
        {
            Vector2 gridBase = basePos + new Vector2(160f, 0f);
            int col = index % 2;
            int row = index / 2;
            return gridBase + new Vector2(col * 66f + 27f, row * 66f + 27f);
        }

        private void FinishEnchanting()
        {
            _isAnimating = false;
            int runeCount = 0;
            List<Item> consumedRunes = new List<Item>();

            for (int i = 0; i < 4; i++)
            {
                if (_placedRunes[i] != null && !_placedRunes[i].IsAir)
                {
                    runeCount++;
                    consumedRunes.Add(_placedRunes[i]);
                    _placedRunes[i] = new Item();
                }
            }

            Player player = Main.LocalPlayer;
            bool hasScroll = player.HasItem(ModContent.ItemType<RuneEnchantmentScroll>());
            float successChance = hasScroll ? 1f : (runeCount == 1 ? 1f : (runeCount == 2 ? 0.75f : (runeCount == 3 ? 0.50f : 0.25f)));

            float roll = Main.rand.NextFloat();
            if (roll <= successChance)
            {
                if (hasScroll)
                {
                    player.ConsumeItem(ModContent.ItemType<RuneEnchantmentScroll>());
                }

                if (_armorItem.TryGetGlobalItem(out EnchantedArmorGlobalItem globalItem))
                {
                    globalItem.IsEnchantedByRune = true;
                    foreach (Item r in consumedRunes)
                    {
                        if (r.ModItem is BaseRuneItem runeItem)
                        {
                            for (int i = 0; i < runeItem.MaxSlots; i++)
                            {
                                if (runeItem.Types[i] != (EssenceType)(-1) && runeItem.Amounts[i] > 0)
                                {
                                    if (!globalItem.Essences.ContainsKey(runeItem.Types[i])) globalItem.Essences[runeItem.Types[i]] = 0;
                                    globalItem.Essences[runeItem.Types[i]] += runeItem.Amounts[i];
                                }
                            }
                        }
                    }
                }
                SoundEngine.PlaySound(SoundID.Item29);

                LocalizedText msg = Language.GetOrRegister("Mods.jarkos.RuneAltarUI.EnchantmentCompleted", () => "The enchantment has been successfully completed!");
                Main.NewText(msg.Value, Color.LightGreen);
            }
            else
            {
                SoundEngine.PlaySound(SoundID.Item27);

                LocalizedText msg = Language.GetOrRegister("Mods.jarkos.RuneAltarUI.EnchantmentFailed", () => "The enchantment failed... Runes destroyed");
                Main.NewText(msg.Value, Color.IndianRed);
            }
        }

        public void Draw(SpriteBatch sb)
        {
            if (!IsOpen) return;

            float uiAlpha = MathSystem.EaseOutCubic(OpenProgress);

            Vector2 basePos = GetUIBasePos();
            Vector2 armorCenter = basePos + new Vector2(60f, 60f);

            Vector2 viseCenter = new Vector2(basePos.X + 113f, basePos.Y - 55f);

            if (ModContent.HasAsset("jarkos/UI/RuneAltarGlowVise"))
            {
                Texture2D glowViseTex = ModContent.Request<Texture2D>("jarkos/UI/RuneAltarGlowVise").Value;
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

                float glowPulse = 0.85f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2.5f) * 0.15f;
                Vector2 glowOrigin = new Vector2(glowViseTex.Width / 2f, 0f);
                Vector2 glowCenter = viseCenter + new Vector2(0f, -15f);
                sb.Draw(glowViseTex, glowCenter, null, Color.White * (glowPulse * uiAlpha), 0f, glowOrigin, 1f, SpriteEffects.None, 0f);

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
            }

            if (ModContent.HasAsset("jarkos/UI/RuneAltarVise"))
            {
                Texture2D viseTex = ModContent.Request<Texture2D>("jarkos/UI/RuneAltarVise").Value;
                sb.Draw(viseTex, viseCenter, null, Color.White * uiAlpha, 0f, viseTex.Size() / 2f, 1f, SpriteEffects.None, 0f);
            }

            DrawArmorSlot(sb, basePos, uiAlpha);
            DrawRuneSlots(sb, basePos, uiAlpha);

            if (!_isAnimating)
            {
                DrawEnchantButton(sb, basePos, uiAlpha);
            }

            if (_pixelParticles.Count > 0)
            {
                Texture2D pxTex = TextureAssets.MagicPixel.Value;
                foreach (var p in _pixelParticles)
                {
                    p.Draw(sb, pxTex, uiAlpha);
                }
            }

            if (_isAnimating)
            {
                float easedAnim = MathSystem.EaseOutCubic(_animTimer);
                for (int i = 0; i < 4; i++)
                {
                    if (_placedRunes[i] != null && !_placedRunes[i].IsAir)
                    {
                        Vector2 startPos = GetRuneSlotCenter(basePos, i);

                        int dirMultiplier = (i % 2 == 0) ? 1 : -1;
                        float initialAngle = i * MathHelper.PiOver2;

                        float spiralAngle = initialAngle + (1f - easedAnim) * MathHelper.TwoPi * 2.0f * dirMultiplier;

                        Vector2 spiralOffset = new Vector2(MathF.Cos(spiralAngle), MathF.Sin(spiralAngle)) * (1f - easedAnim) * 75f;
                        Vector2 currentPos = Vector2.Lerp(startPos, armorCenter, easedAnim) + spiralOffset;

                        float runeRotation = easedAnim * MathHelper.TwoPi * 1.5f * dirMultiplier;

                        Texture2D rTex = TextureAssets.Item[_placedRunes[i].type].Value;
                        sb.Draw(rTex, currentPos, null, Color.White * uiAlpha, runeRotation, rTex.Size() / 2f, 1.3f, SpriteEffects.None, 0f);
                    }
                }
            }

            if (_sparkParticles.Count > 0)
            {
                Texture2D sparkTex = null;
                if (ModContent.HasAsset("jarkos/particles/Spark"))
                {
                    sparkTex = ModContent.Request<Texture2D>("jarkos/particles/Spark").Value;
                }

                if (sparkTex != null)
                {
                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

                    foreach (var p in _sparkParticles)
                    {
                        p.Draw(sb, sparkTex, uiAlpha);
                    }

                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
                }
            }

            if (_flashAlpha > 0f)
            {
                if (ModContent.HasAsset("jarkos/particles/Circular_glow"))
                {
                    Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/particles/Circular_glow").Value;
                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

                    sb.Draw(glowTex, armorCenter, null, Color.White * _flashAlpha * uiAlpha, 0f, glowTex.Size() / 2f, 1.8f * _flashAlpha, SpriteEffects.None, 0f);

                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
                }
            }

            if (_starParticles.Count > 0)
            {
                Texture2D starTex = null;
                if (ModContent.HasAsset("jarkos/particles/StarParticle"))
                {
                    starTex = ModContent.Request<Texture2D>("jarkos/particles/StarParticle").Value;
                }

                if (starTex != null)
                {
                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

                    foreach (var p in _starParticles)
                    {
                        p.Draw(sb, starTex, uiAlpha);
                    }

                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
                }
            }

            DrawInfoButton(sb, basePos, uiAlpha);

            LocalizedText msg = Language.GetOrRegister("Mods.jarkos.RuneAltarUI.ESCClose", () => "Press ESC to close");
            ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, msg.Value, new Vector2(basePos.X, basePos.Y - 30f), Color.White * uiAlpha, 0f, Vector2.Zero, Vector2.One);
        }

        private void DrawInfoButton(SpriteBatch sb, Vector2 basePos, float uiAlpha)
        {
            Rectangle infoRect = new Rectangle((int)basePos.X - 45, (int)basePos.Y - 45, 34, 34);
            bool hover = !IsClosing && infoRect.Contains(Main.MouseScreen.ToPoint());

            Texture2D fillTex = ModContent.Request<Texture2D>("jarkos/UI/ButtonSquare_Fill").Value;
            Color fillColor = hover ? new Color(90, 160, 220, (int)(240 * uiAlpha)) : new Color(50, 100, 160, (int)(190 * uiAlpha));

            sb.Draw(fillTex, infoRect, fillColor);

            Texture2D outlineTex = ModContent.Request<Texture2D>("jarkos/UI/ButtonSquare").Value;
            Color outlineColor = hover ? Color.Cyan * uiAlpha : Color.White * (0.85f * uiAlpha);

            sb.Draw(outlineTex, infoRect, outlineColor);

            Texture2D bookTex = ModContent.Request<Texture2D>("jarkos/UI/BookGuidePic").Value;
            Vector2 bookCenter = new Vector2(infoRect.X + infoRect.Width / 2f, infoRect.Y + infoRect.Height / 2f);
            float bookScale = MathF.Min(22f / bookTex.Width, 22f / bookTex.Height);

            sb.Draw(bookTex, bookCenter, null, Color.White * uiAlpha, 0f, bookTex.Size() / 2f, bookScale, SpriteEffects.None, 0f);

            if (hover)
            {
                Main.LocalPlayer.mouseInterface = true;

                LocalizedText msg1 = Language.GetOrRegister("Mods.jarkos.RuneAltarUI.MSGOne", () => "Runic Altar - Instructions:");
                LocalizedText msg2 = Language.GetOrRegister("Mods.jarkos.RuneAltarUI.MSGTwo", () => "Place the armor element in the central slot");
                LocalizedText msg3 = Language.GetOrRegister("Mods.jarkos.RuneAltarUI.MSGThree", () => "Place 1 to 4 RUNs in the right slots");
                LocalizedText msg4 = Language.GetOrRegister("Mods.jarkos.RuneAltarUI.MSGFour", () => "Each essence in the rune provides its own enchantment bonuses");
                LocalizedText msg5 = Language.GetOrRegister("Mods.jarkos.RuneAltarUI.MSGFive", () => "Click 'Enchant' to launch the altar");
                LocalizedText msg6 = Language.GetOrRegister("Mods.jarkos.RuneAltarUI.MSGSix", () => "The chance of success depends on the number of runes (or 100% with a scroll)");

                string[] lines = new string[]
                {
                    msg1.Value,
                    msg2.Value,
                    msg3.Value,
                    msg4.Value,
                    msg5.Value,
                    msg6.Value
                };

                float maxWidth = 0f;
                float lineHeight = 20f;
                foreach (var l in lines)
                {
                    Vector2 s = FontAssets.MouseText.Value.MeasureString(l);
                    if (s.X > maxWidth) maxWidth = s.X;
                }

                int padding = 12;
                int panelWidth = (int)maxWidth + padding * 2;
                int panelHeight = (int)(lines.Length * lineHeight) + padding * 2;

                Vector2 mousePos = Main.MouseScreen + new Vector2(16f, 16f);
                if (mousePos.X + panelWidth > Main.screenWidth - 10) mousePos.X = Main.screenWidth - panelWidth - 10;
                if (mousePos.Y + panelHeight > Main.screenHeight - 10) mousePos.Y = Main.screenHeight - panelHeight - 10;

                Rectangle tipRect = new Rectangle((int)mousePos.X, (int)mousePos.Y, panelWidth, panelHeight);
                
                Terraria.Utils.DrawInvBG(sb, tipRect, new Color(15, 25, 45, (int)(240 * uiAlpha)));

                for (int lIdx = 0; lIdx < lines.Length; lIdx++)
                {
                    Color textColor = (lIdx == 0) ? Color.Gold * uiAlpha : ((lIdx == 5) ? Color.Yellow * uiAlpha : Color.LightCyan * uiAlpha);
                    Vector2 linePos = new Vector2(tipRect.X + padding, tipRect.Y + padding + lIdx * lineHeight);
                    ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, lines[lIdx], linePos, textColor, 0f, Vector2.Zero, Vector2.One);
                }
            }
        }

        private void DrawArmorSlot(SpriteBatch sb, Vector2 basePos, float uiAlpha)
        {
            Rectangle rect = new Rectangle((int)basePos.X, (int)basePos.Y, 120, 120);
            bool hover = !IsClosing && rect.Contains(Main.MouseScreen.ToPoint());

            Color bgColor = hover ? new Color(70, 10, 15, (int)(210 * uiAlpha)) : new Color(70, 10, 15, (int)(130 * uiAlpha));

            if (_armorErrorBlinkTimer > 0f)
            {
                float blinkPulse = (float)Math.Sin((1f - _armorErrorBlinkTimer) * MathHelper.TwoPi * 3f) * 0.5f + 0.5f;
                bgColor = Color.Lerp(bgColor, new Color(255, 40, 40, (int)(240 * uiAlpha)), blinkPulse);
            }

            Terraria.Utils.DrawInvBG(sb, rect, bgColor);

            if (_armorItem != null && !_armorItem.IsAir)
            {
                Texture2D itemTex = TextureAssets.Item[_armorItem.type].Value;
                Vector2 itemCenter = new Vector2(rect.X + rect.Width / 2f, rect.Y + rect.Height / 2f);
                float itemScale = MathF.Min(80f / itemTex.Width, 80f / itemTex.Height);

                sb.Draw(itemTex, itemCenter, null, Color.White * uiAlpha, 0f, itemTex.Size() / 2f, itemScale, SpriteEffects.None, 0f);
            }
            else
            {
                string label = "Armor";
                Vector2 labelSize = FontAssets.MouseText.Value.MeasureString(label);
                Color labelColor = (_armorErrorBlinkTimer > 0f) ? Color.Red * uiAlpha : Color.White * (0.8f * uiAlpha);

                ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, label, new Vector2(rect.X + (rect.Width - labelSize.X) / 2f, rect.Y + (rect.Height - labelSize.Y) / 2f), labelColor, 0f, Vector2.Zero, Vector2.One);
            }

            if (hover && !IsClosing && !_isAnimating)
            {
                Main.LocalPlayer.mouseInterface = true;

                if (_armorItem != null && !_armorItem.IsAir)
                {
                    Main.HoverItem = _armorItem.Clone();
                    Main.hoverItemName = _armorItem.Name;
                }

                if (Main.mouseLeft && Main.mouseLeftRelease)
                {
                    Item held = Main.mouseItem;

                    if (held != null && !held.IsAir)
                    {
                        bool isArmor = held.headSlot >= 0 || held.bodySlot >= 0 || held.legSlot >= 0;
                        if (isArmor)
                        {
                            Item temp = _armorItem.Clone();
                            _armorItem = held.Clone();

                            Main.mouseItem = temp;

                            SoundEngine.PlaySound(SoundID.Grab);
                        }
                        else
                        {
                            LocalizedText msg = Language.GetOrRegister("Mods.jarkos.RuneAltarUI.SlotOnlyArmor", () => "Only armor can be placed in this slot!");
                            Main.NewText(msg.Value, Color.Red);

                            SoundEngine.PlaySound(SoundID.MenuTick);
                            _armorErrorBlinkTimer = 1.0f;
                        }
                    }
                    else if (_armorItem != null && !_armorItem.IsAir)
                    {
                        Main.mouseItem = _armorItem.Clone();
                        _armorItem = new Item();
                        SoundEngine.PlaySound(SoundID.Grab);
                    }
                }
            }
        }

        private void DrawRuneSlots(SpriteBatch sb, Vector2 basePos, float uiAlpha)
        {
            Vector2 gridBase = basePos + new Vector2(160f, 0f);

            for (int i = 0; i < 4; i++)
            {
                int col = i % 2;
                int row = i / 2;
                Rectangle rect = new Rectangle((int)(gridBase.X + col * 66f), (int)(gridBase.Y + row * 66f), 54, 54);
                bool hover = !IsClosing && rect.Contains(Main.MouseScreen.ToPoint());

                Color bgColor = hover ? new Color(70, 10, 15, (int)(210 * uiAlpha)) : new Color(70, 10, 15, (int)(130 * uiAlpha));

                Terraria.Utils.DrawInvBG(sb, rect, bgColor);

                Item rune = _placedRunes[i];
                if (!_isAnimating && rune != null && !rune.IsAir)
                {
                    Texture2D rTex = TextureAssets.Item[rune.type].Value;
                    Vector2 rCenter = new Vector2(rect.X + rect.Width / 2f, rect.Y + rect.Height / 2f);
                    float rScale = MathF.Min(38f / rTex.Width, 38f / rTex.Height);
                    sb.Draw(rTex, rCenter, null, Color.White * uiAlpha, 0f, rTex.Size() / 2f, rScale, SpriteEffects.None, 0f);
                }

                if (hover && !IsClosing && !_isAnimating)
                {
                    Main.LocalPlayer.mouseInterface = true;

                    if (rune != null && !rune.IsAir)
                    {
                        Main.HoverItem = rune.Clone();
                        Main.hoverItemName = rune.Name;
                    }

                    if (Main.mouseLeft && Main.mouseLeftRelease)
                    {
                        Item held = Main.mouseItem;
                        if (held != null && !held.IsAir)
                        {
                            if (held.ModItem is BaseRuneItem)
                            {
                                Item temp = _placedRunes[i].Clone();
                                _placedRunes[i] = held.Clone();
                                Main.mouseItem = temp;
                                SoundEngine.PlaySound(SoundID.Grab);
                            }
                            else
                            {
                                LocalizedText msg = Language.GetOrRegister("Mods.jarkos.RuneAltarUI.SlotOnlyRunes", () => "Only runes can be placed in this slot!");
                                Main.NewText(msg.Value, Color.Yellow);
                                SoundEngine.PlaySound(SoundID.MenuTick);
                            }
                        }
                        else if (rune != null && !rune.IsAir)
                        {
                            Main.mouseItem = rune.Clone();
                            _placedRunes[i] = new Item();

                            SoundEngine.PlaySound(SoundID.Grab);
                        }
                    }
                }
            }
        }

        private void DrawEnchantButton(SpriteBatch sb, Vector2 basePos, float uiAlpha)
        {
            Rectangle btn = new Rectangle((int)basePos.X, (int)(basePos.Y + 150f), 226, 50);
            bool hover = !IsClosing && btn.Contains(Main.MouseScreen.ToPoint());

            Color btnColor = hover ? new Color(70, 10, 15, (int)(210 * uiAlpha)) : new Color(70, 10, 15, (int)(130 * uiAlpha));

            Terraria.Utils.DrawInvBG(sb, btn, btnColor);

            LocalizedText msg = Language.GetOrRegister("Mods.jarkos.RuneAltarUI.EnchantBtn", () => "Enchant");
            string btnText = msg.Value;

            Vector2 textSize = FontAssets.MouseText.Value.MeasureString(btnText);
            Vector2 textPos = new Vector2(btn.X + (btn.Width - textSize.X) / 2f, btn.Y + (btn.Height - textSize.Y) / 2f);

            ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, btnText, textPos, (hover ? Color.Gold : Color.White) * uiAlpha, 0f, Vector2.Zero, Vector2.One);

            if (hover && !IsClosing)
            {
                Main.LocalPlayer.mouseInterface = true;
                if (Main.mouseLeft && Main.mouseLeftRelease)
                {
                    if (_armorItem == null || _armorItem.IsAir)
                    {
                        LocalizedText msg2 = Language.GetOrRegister("Mods.jarkos.RuneAltarUI.PlaceArmorInSlot", () => "Place the armor item in the slot!");
                        Main.NewText(msg.Value, Color.Orange);

                        SoundEngine.PlaySound(SoundID.MenuTick);
                        _armorErrorBlinkTimer = 1.0f;
                        return;
                    }

                    int runeCount = 0;
                    for (int i = 0; i < 4; i++)
                    {
                        if (_placedRunes[i] != null && !_placedRunes[i].IsAir) runeCount++;
                    }

                    if (runeCount == 0)
                    {
                        LocalizedText msg3 = Language.GetOrRegister("Mods.jarkos.RuneAltarUI.PlaceRuneInSlot", () => "Place at least one rune in the slots!");
                        Main.NewText(msg3.Value, Color.Orange);

                        SoundEngine.PlaySound(SoundID.MenuTick);
                        return;
                    }

                    _isAnimating = true;
                    _animTimer = 0f;
                    SoundEngine.PlaySound(SoundID.Item4);
                }
            }
        }
    }
}
