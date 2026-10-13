using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using jarkos.UI.Common;
using jarkos.core.Data;
using jarkos.core.PlayerInteract;
using Terraria.ID;
using Terraria.Audio;
using Terraria.UI.Chat;

namespace jarkos.UI
{
    public class EssencePanel
    {
        private const float BaseWidth = 280f;
        private readonly ParticlePool _particles;
        private bool _nudge;
        private bool _expanded = false;
        private Tween _expandAnim = new Tween(0.35f, MathSystem.EaseOutCubic);
        private Tween _hideAnim = new Tween(0.6f, MathSystem.EaseInOutCubic);
        private Rectangle _btnRect;
        private Rectangle _currentBounds;
        private Dictionary<EssenceType, float> _hoverProgress = new();
        private Dictionary<EssenceType, Rectangle> _cellRects = new();
        public EssencePanel(ParticlePool particles) => _particles = particles;
        public bool Expanded => _expanded;
        public void RequestAttention(bool on) => _nudge = on;
        public void SetHidden(bool hidden, bool instant = false)
        {
            if (hidden)
            {
                if (instant) _hideAnim.Set(1f);
                else _hideAnim.To(1f);
            }
            else
            {
                if (instant) _hideAnim.Set(0f);
                else _hideAnim.To(0f);
            }
        }
        public void Update(float dt)
        {
            _expandAnim.Update(dt);
            _hideAnim.Update(dt);
            for (int i = _flyingEssences.Count - 1; i >= 0; i--)
            {
                var fe = _flyingEssences[i];
                fe.Progress += dt * 1.5f;
                if (fe.Progress >= 1f)
                {
                    fe.OnArrived?.Invoke();
                    _flyingEssences.RemoveAt(i);
                }
                else
                {
                    Vector2 target = GetCellPosition(fe.Type);
                    Vector2 mid = (fe.StartPos + target) / 2f;
                    mid.Y -= 100f;
                    float t = fe.Progress;
                    float u = 1f - t;
                    fe.CurrentPos = u * u * fe.StartPos + 2f * u * t * mid + t * t * target;
                }
            }
        }
        public static int GetNodeDistance(ConstellationEntry entry)
        {
            if (entry == null || entry.IsRoot) return 0;
            if (entry is BranchNode branch)
            {
                return Math.Max(0, branch.Index);
            }
            if (entry.Prerequisites != null && entry.Prerequisites.Count > 0)
            {
                int maxParent = 0;
                foreach (var preId in entry.Prerequisites)
                {
                    if (ConstellationRegistry.TryGet(preId, out var parent))
                    {
                        maxParent = Math.Max(maxParent, GetNodeDistance(parent));
                    }
                }
                return maxParent + 1;
            }
            return (int)Math.Max(1, Math.Round(entry.WorldPosition.Length() / 300f));
        }

        public static int GetRequiredEssenceCost(ConstellationEntry entry)
        {
            if (entry == null || entry.IsRoot) return 0;
            int distance = GetNodeDistance(entry);
            if (distance <= 1) return 10;

            float t = (distance - 1f) / 7f;
            float costFloat = 10f + 70f * MathF.Pow(t, 1.95f);
            return (int)Math.Round(costFloat);
        }

        public static int GetRequiredEssenceCost(ConstellationEntry entry, EssenceType essenceType)
        {
            if (entry == null || entry.IsRoot) return 0;
            if (essenceType == EssenceType.Luna)
            {
                return 5;
            }
            int baseCost = GetRequiredEssenceCost(entry);
            bool isMainEssence = entry.Tier == essenceType;

            if (isMainEssence) return baseCost;

            return (int)Math.Max(1, Math.Round(baseCost * 0.15f));
        }

        public bool HandleClick(Vector2 mouse, ResearchPlayer plr, PuzzleNodeData selectedNode, out bool puzzlePaid, out EssenceType paidType, out int paidCost, out Vector2 startPos)
        {
            puzzlePaid = false;
            paidType = default;
            paidCost = 0;
            startPos = default;
            var mousePt = mouse.ToPoint();
            if (_btnRect.Contains(mousePt))
            {
                _expanded = !_expanded;
                if (_expanded) _expandAnim.To(1f);
                else _expandAnim.To(0f);
                return true;
            }
            if (selectedNode != null && _hideAnim.Value < 0.999f)
            {
                foreach (var kvp in _cellRects)
                {
                    if (kvp.Value.Contains(mousePt))
                    {
                        EssenceType typeClicked = kvp.Key;
                        if (typeClicked != selectedNode.RequiredEssence)
                        {
                            continue;
                        }

                        ConstellationEntry activeNode = null;
                        if (plr.HasActive)
                        {
                            ConstellationRegistry.TryGet(plr.ActiveResearchID, out activeNode);
                        }
                        int cost = GetRequiredEssenceCost(activeNode, typeClicked);

                        if (plr.GetEssence(typeClicked) >= cost)
                        {
                            plr.AddEssence(typeClicked, -cost);
                            puzzlePaid = true;
                            paidType = typeClicked;
                            paidCost = cost;
                            startPos = kvp.Value.Center.ToVector2();
                            SoundEngine.PlaySound(SoundID.Item43);
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public class FlyingEssenceToPanel
        {
            public EssenceType Type;
            public Vector2 StartPos;
            public Vector2 CurrentPos;
            public float Progress;
            public Action OnArrived;
        }
        private List<FlyingEssenceToPanel> _flyingEssences = new();
        public void FlyInEssence(EssenceType type, Vector2 startPos, Action onArrived)
        {
            _flyingEssences.Add(new FlyingEssenceToPanel            {
                Type = type, StartPos = startPos, CurrentPos = startPos, Progress = 0f, OnArrived = onArrived
            });
            SetHidden(false);
        }
        public Vector2 GetCellPosition(EssenceType type)
        {
            if (_cellRects.TryGetValue(type, out var r)) return r.Center.ToVector2();
            return new Vector2(_currentBounds.X + _currentBounds.Width / 2, _currentBounds.Y + _currentBounds.Height / 2);
        }
        public Rectangle GetPanelBounds() => _currentBounds;
        private IEnumerable<EssenceType> ActiveEssences(ResearchPlayer plr)
        {
            var active = plr.Essences.Where(kv => kv.Value > 0 && Enum.IsDefined(typeof(EssenceType), kv.Key)).Select(kv => kv.Key).ToList();
            foreach (var fe in _flyingEssences)
            {
                if (Enum.IsDefined(typeof(EssenceType), fe.Type) && !active.Contains(fe.Type)) active.Add(fe.Type);
            }
            return active;
        }
        public void Draw(SpriteBatch sb, ResearchPlayer plr, float time)
        {
            if (_hideAnim.Value >= 0.999f) return;

            float hideP = _hideAnim.Eased;
            float masterAlpha = 1f - hideP;
            float marginX = 20f;
            float marginY = 20f;
            float panelX = Main.screenWidth - BaseWidth - marginX + hideP * 400f;
            float panelY = marginY;
            var active = ActiveEssences(plr).ToList();

            int totalItems = active.Count;
            if (totalItems == 0) totalItems = 1;

            int cols = 4;
            int cellSize = 54;
            int spacing = 8;
            int totalRows = (int)Math.Ceiling((float)totalItems / cols);
            int collapsedRows = Math.Min(totalRows, 2);
            bool canExpand = totalRows > collapsedRows;
            float rowAnim = MathHelper.Lerp(collapsedRows, totalRows, _expandAnim.Eased);

            float innerHeight = rowAnim * (cellSize + spacing) - spacing;
            float panelHeight = 60f + innerHeight + (canExpand ? 40f : 10f);
            _currentBounds = new Rectangle((int)panelX, (int)panelY, (int)BaseWidth, (int)panelHeight);
            float pulse = _nudge ? Easing.Breathe(time, 4f) : 0f;
            Color borderColor = Color.Lerp(new Color(40, 50, 80), new Color(100, 130, 220), pulse) * masterAlpha;

            Panel.Rounded(sb, _currentBounds, 12, new Color(14, 16, 32) * 0.92f * masterAlpha, borderColor, 2);
            Terraria.Utils.DrawBorderString(sb, "ESSENCES", new Vector2(panelX + 24, panelY + 18), Color.White * masterAlpha, 0.95f);

            if (active.Count == 0)
            {
                Terraria.Utils.DrawBorderString(sb, "Empty", new Vector2(panelX + 24, panelY + 56), Color.Gray * masterAlpha, 0.85f);
                return;
            }

            float gridStartX = panelX + (BaseWidth - (cols * cellSize + (cols - 1) * spacing)) / 2f;
            float gridStartY = panelY + 50f;

            var mousePt = new Point(Main.mouseX, Main.mouseY);
            _cellRects.Clear();

            for (int i = 0; i < active.Count; i++)
            {
                EssenceType type = active[i];
                int c = i % cols;
                int r = i / cols;
                float cx = gridStartX + c * (cellSize + spacing);
                float cy = gridStartY + r * (cellSize + spacing);
                if (r > rowAnim + 0.1f) continue;

                float rowAlpha = 1f;

                if (r >= collapsedRows)
                {
                    float rowProgress = rowAnim - r;
                    rowAlpha = Easing.Clamp01(rowProgress * 1.5f);
                }

                if (rowAlpha <= 0f) continue;

                rowAlpha *= masterAlpha;
                var cellRect = new Rectangle((int)cx, (int)cy, cellSize, cellSize);
                _cellRects[type] = cellRect;

                bool hover = cellRect.Contains(mousePt);

                if (!_hoverProgress.ContainsKey(type)) _hoverProgress[type] = 0f;
                float currentHover = _hoverProgress[type];

                currentHover = MathHelper.Clamp(currentHover + (hover ? 0.08f : -0.06f), 0f, 1f);
                _hoverProgress[type] = currentHover;
                float hoverAnim = MathSystem.EaseOutCubic(currentHover);
                Color typeColor = EssencePalette.ColorOf(type);
                Common.Draw.RadialGlow(sb, cellRect.Center.ToVector2(), cellSize * 0.7f, typeColor * 0.15f * rowAlpha);

                if (hoverAnim > 0f)
                {
                    Common.Draw.RadialGlow(sb, cellRect.Center.ToVector2(), cellSize * (0.8f + 0.3f * hoverAnim), typeColor * 0.35f * hoverAnim * rowAlpha);
                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
                    Common.Draw.RadialGlow(sb, cellRect.Center.ToVector2(), cellSize * (0.6f + 0.2f * hoverAnim), typeColor * 0.5f * hoverAnim * rowAlpha);
                    sb.End();
                    sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
                }

                Texture2D tex = null;
                string path = "jarkos/textures/Essences/" + type.ToString();
                if (ModContent.HasAsset(path))
                {
                    tex = ModContent.Request<Texture2D>(path).Value;
                }

                if (tex != null)
                {
                    float texScale = Math.Min((cellSize - 12f) / tex.Width, (cellSize - 12f) / tex.Height);
                    texScale *= 1f + 0.05f * hoverAnim;
                    float iconAlpha = 1f - 0.75f * hoverAnim;
                    sb.Draw(tex, cellRect.Center.ToVector2(), null, Color.White * rowAlpha * iconAlpha, 0f, tex.Size() / 2f, texScale, SpriteEffects.None, 0f);
                }
                int amount = plr.GetEssence(type);
                string amtText = amount.ToString();
                float numScale = MathHelper.Lerp(0.7f, 1.8f, hoverAnim);

                Vector2 normalPos = new Vector2(cellRect.Right - 4, cellRect.Bottom - 2);
                Vector2 centerPos = cellRect.Center.ToVector2();
                centerPos.X -= 4f * hoverAnim;
                centerPos.Y += 4f * hoverAnim;

                Vector2 amtPos = Vector2.Lerp(normalPos, centerPos, hoverAnim);
                float ax = MathHelper.Lerp(1f, 0.5f, hoverAnim);
                float ay = MathHelper.Lerp(1f, 0.5f, hoverAnim);
                Terraria.Utils.DrawBorderString(sb, amtText, amtPos, Color.White * rowAlpha, numScale, ax, ay);

                if (hoverAnim > 0.01f)
                {
                    string name = EssencePalette.DisplayName(type);
                    float nameScale = 0.8f;
                    Vector2 namePos = new Vector2(cellRect.Center.X, cellRect.Top - 6f);
                    Terraria.Utils.DrawBorderString(sb, name, namePos, typeColor * hoverAnim * rowAlpha, nameScale, 0.5f, 0f);
                }
            }
            foreach (var fe in _flyingEssences)
            {
                Color col = EssencePalette.ColorOf(fe.Type);
                float alpha = Easing.Clamp01(MathF.Sin(fe.Progress * MathF.PI));
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

                Common.Draw.RadialGlow(sb, fe.CurrentPos, 60f, col * alpha);
                
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
                Texture2D tex = EssencePalette.GetTexture(fe.Type);
                if (tex != null)
                {
                    float rot = time * 5f;
                    sb.Draw(tex, fe.CurrentPos, null, Color.White, rot, tex.Size() / 2f, 1f, SpriteEffects.None, 0f);
                }
            }
            if (canExpand)
            {
                float btnY = panelY + panelHeight - 34f;
                _btnRect = new Rectangle((int)panelX + 20, (int)btnY, (int)BaseWidth - 40, 24);
                bool btnHover = _btnRect.Contains(mousePt);
                Color btnCol = new Color(50, 60, 90) * masterAlpha;

                Panel.Rounded(sb, _btnRect, 6, btnCol * (btnHover ? 0.8f : 0.4f), btnCol * 0.8f, 1);
                string btnText = _expanded ? "Collapse" : "Expand";
                var tSize = FontAssets.MouseText.Value.MeasureString(btnText) * 0.7f;

                Terraria.Utils.DrawBorderString(sb, btnText, new Vector2(_btnRect.Center.X - tSize.X / 2f, _btnRect.Center.Y - tSize.Y / 2f), Color.White * masterAlpha, 0.7f);
            }
            else
            {
                _btnRect = Rectangle.Empty;
            }
            foreach (var kvp in _cellRects)
            {
                if (kvp.Value.Contains(mousePt))
                {
                    int amount = plr.GetEssence(kvp.Key);
                    string bonus = EssencePalette.GetBonusText(kvp.Key, amount);
                    if (!string.IsNullOrEmpty(bonus))
                    {
                        Vector2 mPos = new Vector2(Main.mouseX + 16, Main.mouseY + 16);
                        Vector2 textSize = FontAssets.MouseText.Value.MeasureString(bonus);
                        if (mPos.X + textSize.X > Main.screenWidth)
                        {
                            mPos.X = Main.mouseX - textSize.X - 16;
                        }
                        ChatManager.DrawColorCodedStringWithShadow(sb, FontAssets.MouseText.Value, bonus, mPos, EssencePalette.ColorOf(kvp.Key), 0f, Vector2.Zero, Vector2.One);
                    }
                    break;
                }
            }
        }
        public bool Contains(Vector2 mouse)
        {
            if (_currentBounds != Rectangle.Empty)
                return _currentBounds.Contains(mouse.ToPoint());
            float marginX = 20f;
            float panelX = Main.screenWidth - BaseWidth - marginX;
            return mouse.X >= panelX && mouse.Y <= Main.screenHeight / 2f;
        }
    }
}
