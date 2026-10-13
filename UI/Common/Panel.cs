using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace jarkos.UI.Common
{
    public static class Panel
    {
        public static void Rounded(SpriteBatch sb, Rectangle r, int radius, Color fill, Color border, int thickness = 2)
        {
            radius = Math.Min(radius, Math.Min(r.Width, r.Height) / 2);
            var px = Draw.Pixel;

            sb.Draw(px, new Rectangle(r.X + radius, r.Y, r.Width - radius * 2, r.Height), fill);
            sb.Draw(px, new Rectangle(r.X, r.Y + radius, radius, r.Height - radius * 2), fill);
            sb.Draw(px, new Rectangle(r.Right - radius, r.Y + radius, radius, r.Height - radius * 2), fill);

            FillCorner(sb, new Vector2(r.X + radius, r.Y + radius), radius, fill, +1, +1);
            FillCorner(sb, new Vector2(r.Right - radius, r.Y + radius), radius, fill, -1, +1);
            FillCorner(sb, new Vector2(r.X + radius, r.Bottom - radius), radius, fill, +1, -1);
            FillCorner(sb, new Vector2(r.Right - radius, r.Bottom - radius), radius, fill, -1, -1);

            Draw.Line(sb, new Vector2(r.X + radius, r.Y), new Vector2(r.Right - radius, r.Y), border, thickness);
            Draw.Line(sb, new Vector2(r.X + radius, r.Bottom), new Vector2(r.Right - radius, r.Bottom), border, thickness);
            Draw.Line(sb, new Vector2(r.X, r.Y + radius), new Vector2(r.X, r.Bottom - radius), border, thickness);
            Draw.Line(sb, new Vector2(r.Right, r.Y + radius), new Vector2(r.Right, r.Bottom - radius), border, thickness);

            ArcCorner(sb, new Vector2(r.X + radius, r.Y + radius), radius, border, thickness, MathHelper.Pi, MathHelper.Pi * 1.5f);
            ArcCorner(sb, new Vector2(r.Right - radius, r.Y + radius), radius, border, thickness, MathHelper.Pi * 1.5f, MathHelper.TwoPi);
            ArcCorner(sb, new Vector2(r.X + radius, r.Bottom - radius), radius, border, thickness, MathHelper.PiOver2, MathHelper.Pi);
            ArcCorner(sb, new Vector2(r.Right - radius, r.Bottom - radius), radius, border, thickness, 0f, MathHelper.PiOver2);
        }
        private static void FillCorner(SpriteBatch sb, Vector2 center, int radius, Color col, int sx, int sy)
        {
            for (int y = 0; y <= radius; y++)
            {
                int x = (int)Math.Sqrt(radius * radius - y * y);
                var rect = new Rectangle((int)(sx > 0 ? center.X - x : center.X), (int)(sy > 0 ? center.Y - y : center.Y + y), x, 1);

                sb.Draw(Draw.Pixel, rect, col);
            }
        }
        private static void ArcCorner(SpriteBatch sb, Vector2 center, int radius, Color col, int thickness, float a0, float a1)
        {
            int steps = Math.Max(6, radius);
            Vector2 previous_point = center + new Vector2(MathF.Cos(a0), MathF.Sin(a0)) * radius;

            for (int i = 1; i <= steps; i++)
            {
                float interpolate = MathHelper.Lerp(a0, a1, i / (float)steps);
                Vector2 point = center + new Vector2(MathF.Cos(interpolate), MathF.Sin(interpolate)) * radius;
                Draw.Line(sb, previous_point, point, col, thickness);
                previous_point = point;
            }
        }
        public static void Tab(SpriteBatch sb, Rectangle r, Color fill, Color border, int thickness = 2)
        {
            Rounded(sb, r, 4, fill, border, thickness);
        }
    }
}
