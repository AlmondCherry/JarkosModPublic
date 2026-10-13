using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace jarkos.UI.Common
{
    public static class Draw
    {
        private static Texture2D _pixel;
        public static Texture2D Pixel
        {
            get
            {
                if (_pixel == null)
                {
                    _pixel = new Texture2D(Main.graphics.GraphicsDevice, 1, 1);
                    _pixel.SetData(new Color[] { Color.White });
                }
                return _pixel;
            }
        }
        private static Texture2D _glow;
        public static Texture2D Glow = ModContent.Request<Texture2D>("jarkos/textures/GlowTexture", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
        public static void Line(SpriteBatch sb, Vector2 a, Vector2 b, Color color, float thickness)
        {
            Vector2 d = b - a;
            float len = d.Length();
            if (len < 0.01f) return;
            float rot = MathF.Atan2(d.Y, d.X);
            sb.Draw(Pixel, a, null, color, rot, new Vector2(0f, 0.5f), new Vector2(len, thickness), SpriteEffects.None, 0f);
        }
        public static Vector2 GetCurvePoint(Vector2 a, Vector2 b, float t, float curveOffset)
        {
            Vector2 normal = new Vector2(b.Y - a.Y, -(b.X - a.X));
            if (normal != Vector2.Zero) normal.Normalize();
            float arc = MathF.Sin(t * MathF.PI);
            return Vector2.Lerp(a, b, t) + normal * arc * curveOffset;
        }
        public static void GradientLine(SpriteBatch sb, Vector2 a, Vector2 b, Color color, float thickness, float progress = 1f, int segments = 24, float curveOffset = 0f)
        {
            progress = Easing.Clamp01(progress);
            for (int i = 0; i < segments; i++)
            {
                float t0 = i / (float)segments * progress;
                float t1 = (i + 1) / (float)segments * progress;
                Vector2 p0 = GetCurvePoint(a, b, t0, curveOffset);
                Vector2 p1 = GetCurvePoint(a, b, t1, curveOffset);
                float mid = (t0 + t1) * 0.5f;
                float alpha = MathF.Sin(mid * MathF.PI);
                Line(sb, p0, p1, color * MathF.Max(0.15f, alpha * 0.85f), thickness);
            }
        }
        public static void RadialGlow(SpriteBatch sb, Vector2 center, float radius, Color color)
        {
            var tex = Glow;
            float scale = radius * 2f / tex.Width;
            sb.Draw(tex, center, null, color, 0f, new Vector2(tex.Width / 2f, tex.Height / 2f), scale, SpriteEffects.None, 0f);
        }

        private static Texture2D _circleKnob;
        public static Texture2D CircleKnob
        {
            get
            {
                if (_circleKnob == null || _circleKnob.IsDisposed)
                {
                    int size = 64;
                    _circleKnob = new Texture2D(Main.graphics.GraphicsDevice, size, size);
                    Color[] data = new Color[size * size];
                    float center = size / 2f;
                    float radius = center - 2f;
                    float borderThickness = 3.5f;

                    for (int y = 0; y < size; y++)
                    {
                        for (int x = 0; x < size; x++)
                        {
                            float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(center, center));
                            if (dist <= radius)
                            {
                                if (dist >= radius - borderThickness)
                                {
                                    data[y * size + x] = new Color(20, 22, 35, 255);
                                }
                                else
                                {
                                    data[y * size + x] = Color.White;
                                }
                            }
                            else
                            {
                                data[y * size + x] = Color.Transparent;
                            }
                        }
                    }
                    _circleKnob.SetData(data);
                }
                return _circleKnob;
            }
        }

        public static void DrawCircleKnob(SpriteBatch sb, Vector2 center, float diameter, Color fillColor)
        {
            Texture2D tex = CircleKnob;
            float scale = diameter / tex.Width;
            sb.Draw(tex, center, null, fillColor, 0f, new Vector2(tex.Width / 2f, tex.Height / 2f), scale, SpriteEffects.None, 0f);
        }
    }
}
