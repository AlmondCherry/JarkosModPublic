using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using jarkos.UI.Common;
using jarkos.core.Data;
using jarkos.core.PlayerInteract;
using ReLogic.Graphics;
using ReLogic.Content;

namespace jarkos.UI
{
    public class ConfirmPopup
    {
        private readonly Tween _open = new(0.40f, MathSystem.EaseOutCubic);
        public ConstellationEntry Target { get; private set; }
        public bool Visible => Target != null;
        public Rectangle ResearchBtn { get; private set; }
        public Rectangle ForceCompleteBtn { get; private set; }
        public Rectangle CancelBtn { get; private set; }
        private Vector2 _sourcePos;
        public void Show(ConstellationEntry e, Vector2 sourcePos) { Target = e; _sourcePos = sourcePos; _open.Set(0f); _open.To(1f); }
        public void Hide() => _open.To(0f);
        public void Update(float dt)
        {
            _open.Update(dt);
            if (Target != null && _open.Target < 0.5f && _open.Value < 0.01f) Target = null;
        }
        public void CheckClick(Vector2 screenMouse, out bool research, out bool cancel, out bool force)
        {
            research = false; cancel = false; force = false;
            if (Target == null || _open.Eased <= 0.001f) return;
            
            float tweentime = _open.Eased;
            int width = 460, height = 280;
            float scale = MathHelper.Lerp(0.05f, 1f, tweentime);
            Vector2 targetCenter = new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);
            Vector2 currentCenter = Vector2.SmoothStep(_sourcePos, targetCenter, tweentime);
            Matrix localTransform = Matrix.CreateTranslation(-width / 2f, -height / 2f, 0f) * Matrix.CreateScale(scale) * Matrix.CreateTranslation(currentCenter.X, currentCenter.Y, 0f);
            Matrix invTransform = Matrix.Invert(localTransform);
            Vector2 localMouse = Vector2.Transform(screenMouse, invTransform);
            var pt = localMouse.ToPoint();
            research = ResearchBtn.Contains(pt);
            cancel = CancelBtn.Contains(pt);
            bool allowForce = Jarkos_config.Instance != null && Jarkos_config.Instance.EnableForceFinish;
            force = allowForce && ForceCompleteBtn.Contains(pt);
        }
        public void Draw(SpriteBatch sb, ResearchPlayer plr, float time)
        {
            if (Target == null) return;
            float k = _open.Value;
            float tweentime = _open.Eased;

            if (tweentime <= 0.001f) return;
            var font = FontAssets.MouseText.Value;
            int w = 460, h = 280;
            int cx = Main.screenWidth / 2;
            int cy = Main.screenHeight / 2;

            sb.Draw(Common.Draw.Pixel, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Black * (0.58f * k));
            float scale = MathHelper.Lerp(0.05f, 1f, tweentime);

            Vector2 targetCenter = new Vector2(cx, cy);
            Vector2 currentCenter = Vector2.SmoothStep(_sourcePos, targetCenter, tweentime);

            Matrix localTransform = Matrix.CreateTranslation(-w / 2f, -h / 2f, 0f) * Matrix.CreateScale(scale) * Matrix.CreateTranslation(currentCenter.X, currentCenter.Y, 0f);
            Matrix popTransform = localTransform * Main.UIScaleMatrix;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, popTransform);
            var r = new Rectangle(0, 0, w, h);

            Color tier = EssencePalette.ColorOf(Target.Tier);
            float breath = 0.6f + 0.4f * Easing.Breathe(time, 2.4f);

            Common.Draw.RadialGlow(sb, r.Center.ToVector2(), w * 0.7f, tier * (0.10f * tweentime));

            Texture2D tex1 = null; 
            Texture2D tex2 = null;
            Texture2D tex3 = null;

            try
            {
                tex1 = ModContent.Request<Texture2D>("jarkos/UI/Textures/PopupPanel_1", AssetRequestMode.ImmediateLoad).Value;
                tex2 = ModContent.Request<Texture2D>("jarkos/UI/Textures/PopupPanel_2", AssetRequestMode.ImmediateLoad).Value;
                tex3 = ModContent.Request<Texture2D>("jarkos/UI/Textures/PopupPanel_3", AssetRequestMode.ImmediateLoad).Value;
            }
            catch (Exception e)
            {
                Main.NewText(e.Message);
            }

            if (tex1 != null && tex2 != null && tex3 != null)
            {
                Color bgColor = Color.Lerp(new Color(6, 8, 16), tier, 0.1f) * 0.35f * tweentime;
                sb.Draw(tex2, r, bgColor);
                sb.End();

                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, popTransform);
                sb.Draw(tex3, r, tier * tweentime * 0.4f);
                sb.End();

                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, popTransform);
                Color outlineColor = Color.Lerp(new Color(2, 2, 4), tier, 0.05f) * tweentime;
                sb.Draw(tex1, r, outlineColor);
            }
            else
            {
                Panel.Rounded(sb, r, 20, new Color(18, 20, 40) * (0.97f * tweentime), Color.Lerp(tier * 0.6f, tier, breath) * tweentime, 3);
            }

            float textAlpha = MathHelper.Clamp((_open.Value - 0.5f) / 0.5f, 0f, 1f);
            if (textAlpha <= 0.001f)
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
                return;
            }
            var iconCenter = new Vector2(r.X + 60, r.Y + 64);
            DrawConstellationIcon(sb, iconCenter, tier, time, textAlpha);

            Terraria.Utils.DrawBorderString(sb, Target.DisplayName, new Vector2(r.X + 108, r.Y + 34), Color.White * textAlpha, 1.1f);
            Terraria.Utils.DrawBorderString(sb, NodeStatusLabel(plr), new Vector2(r.X + 108, r.Y + 62), tier * 0.9f * textAlpha, 0.72f);

            string descText = plr.IsCompleted(Target.ID) ? Target.Description : "???";

            DrawWrapped(sb, font, descText, new Vector2(r.X + 26, r.Y + 110), r.Width - 52, new Color(205, 210, 230) * textAlpha, 0.8f);

            Terraria.Utils.DrawBorderString(sb, Target.PuzzleRequirementText, new Vector2(r.X + 26, r.Y + 166), tier * textAlpha, 0.74f);

            bool locked = plr.HasActive && plr.ActiveResearchID != Target.ID;
            bool completed = plr.IsCompleted(Target.ID);
            bool allowForce = Jarkos_config.Instance != null && Jarkos_config.Instance.EnableForceFinish;

            if (allowForce && !completed)
            {
                ResearchBtn = new Rectangle(26, h - 56, 130, 36);
                ForceCompleteBtn = new Rectangle(166, h - 56, 130, 36);
                CancelBtn = new Rectangle(w - 156, h - 56, 130, 36);
            }
            else
            {
                ResearchBtn = new Rectangle(45, h - 56, 170, 36);
                ForceCompleteBtn = Rectangle.Empty;
                CancelBtn = new Rectangle(w - 215, h - 56, 170, 36);
            }

            Matrix invTransform = Matrix.Invert(localTransform);
            Vector2 localMouse = Vector2.Transform(new Vector2(Main.mouseX, Main.mouseY), invTransform);

            var mouse = new Point((int)localMouse.X, (int)localMouse.Y);
            string label = completed ? "Researched" : (locked ? "Locked" : "Research");
            Color btnCol = completed ? new Color(70, 120, 90) : (locked ? new Color(70, 70, 84) : tier);

            DrawButton(sb, font, ResearchBtn, label, btnCol, !locked && !completed && ResearchBtn.Contains(mouse), time, popTransform, textAlpha);

            if (allowForce && !completed)
            {
                DrawButton(sb, font, ForceCompleteBtn, "Force Finish", new Color(150, 100, 40), ForceCompleteBtn.Contains(mouse), time, popTransform, textAlpha);
            }

            DrawButton(sb, font, CancelBtn, "Close", new Color(150, 45, 45), CancelBtn.Contains(mouse), time, popTransform, textAlpha);
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
        }
        private string NodeStatusLabel(ResearchPlayer plr)
        {
            if (plr.IsCompleted(Target.ID)) return "Researched";
            if (plr.ActiveResearchID == Target.ID) return "Research in Progress";
            if (plr.HasActive) return "Complete active research first";

            return "Ready for Research";
        }
        private void DrawConstellationIcon(SpriteBatch sb, Vector2 c, Color tier, float time, float alpha)
        {
            Common.Draw.RadialGlow(sb, c, 40f, tier * (0.4f + 0.2f * Easing.Breathe(time, 3f)) * alpha);
            Texture2D tex = null;
            
            try
            {
                if (ModContent.HasAsset(Target.IconTexture))
                    tex = ModContent.Request<Texture2D>(Target.IconTexture).Value;
            }
            catch { }

            if (tex != null)
            {
                float rot = MathF.Sin(time * 1.5f) * 0.05f;
                float scale = 0.8f + 0.05f * MathF.Sin(time * 3f);

                sb.Draw(tex, c, null, Color.Black * alpha, rot, tex.Size() / 2f, scale, SpriteEffects.None, 0f);
            }
            else
            {
                var rng = new Random(Target.ID.GetHashCode());
                Vector2 prev = c;

                for (int i = 0; i < 6; i++)
                {
                    float ang = (float)(rng.NextDouble() * Math.PI * 2);
                    float rad = 6f + (float)rng.NextDouble() * 16f;

                    Vector2 p = c + new Vector2(MathF.Cos(ang), MathF.Sin(ang)) * rad;

                    if (i > 0) Common.Draw.Line(sb, prev, p, tier * 0.4f * alpha, 1f);
                    float tw = 0.5f + 0.5f * MathF.Sin(time * 2.5f + i);

                    sb.Draw(Common.Draw.Pixel, p, null, Color.Lerp(tier, Color.White, 0.5f) * tw * alpha, 0f, new Vector2(0.5f), new Vector2(2f + (i == 0 ? 1.5f : 0f)), SpriteEffects.None, 0f);
                    prev = p;
                }
            }
        }
        public bool CanConfirm(ResearchPlayer plr) => Target != null && !plr.IsCompleted(Target.ID) && !(plr.HasActive && plr.ActiveResearchID != Target.ID);
        private void DrawButton(SpriteBatch sb, DynamicSpriteFont font, Rectangle r, string label, Color col, bool hover, float time, Matrix popTransform, float alpha)
        {
            float glow = hover ? 0.55f + 0.45f * Easing.Breathe(time, 6f) : 0.3f;
            if (hover) Common.Draw.RadialGlow(sb, r.Center.ToVector2(), r.Width * 0.6f, col * 0.25f * alpha);

            Texture2D tex1 = null;
            Texture2D tex2 = null;
            Texture2D tex3 = null;

            try 
            {
                tex1 = ModContent.Request<Texture2D>("jarkos/UI/Textures/Button_1", AssetRequestMode.ImmediateLoad).Value;
                tex2 = ModContent.Request<Texture2D>("jarkos/UI/Textures/Button_2", AssetRequestMode.ImmediateLoad).Value;
                tex3 = ModContent.Request<Texture2D>("jarkos/UI/Textures/Button_3", AssetRequestMode.ImmediateLoad).Value;
            } 
            catch (Exception e) 
            {
                Main.NewText(e.Message);
            }
            
            if (tex1 != null && tex2 != null && tex3 != null)
            {
                Color bgColor = Color.Lerp(new Color(24, 28, 42), col, hover ? 0.4f : 0.2f) * 0.95f * alpha;
                sb.Draw(tex2, r, bgColor);
                sb.End();

                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, popTransform);
                sb.Draw(tex3, r, col * (hover ? 1.0f : 0.6f) * alpha);
                sb.End();

                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, popTransform);
                Color outlineColor = Color.Lerp(new Color(2, 2, 4), col, 0.1f) * alpha;
                sb.Draw(tex1, r, outlineColor);
            }
            else
            {
                Panel.Rounded(sb, r, 11, col * (hover ? 0.5f : 0.28f) * alpha, col * glow * alpha, 2);
            }

            var size = font.MeasureString(label) * 0.85f;
            Terraria.Utils.DrawBorderString(sb, label, new Vector2(r.Center.X - size.X / 2, r.Center.Y - size.Y / 2 + 4), Color.White * alpha, 0.85f);
        }
        private void DrawWrapped(SpriteBatch sb, DynamicSpriteFont font, string text, Vector2 pos, int maxWidth, Color col, float scale)
        {
            string[] words = text.Split(' ');
            string line = ""; float y = pos.Y;
            
            foreach (var word in words)
            {
                string test = line.Length == 0 ? word : line + " " + word;
                if (font.MeasureString(test).X * scale > maxWidth)
                {
                    Terraria.Utils.DrawBorderString(sb, line, new Vector2(pos.X, y), col, scale);
                    line = word; y += font.MeasureString("A").Y * scale + 2;
                }
                else line = test;
            }
            if (line.Length > 0) Terraria.Utils.DrawBorderString(sb, line, new Vector2(pos.X, y), col, scale);
        }
    }
}
