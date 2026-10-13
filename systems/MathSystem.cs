using System;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace jarkos
{
    public static class MathSystem
    {
        public static float GetProgressValue(float timeleft, float totaltime)
        {
            float result = MathHelper.Clamp(1f - timeleft / totaltime, 0f, 1f);
            return result;
        }
        public static float EaseOutCubic(float progress)
        {
            float one = 1f - MathF.Pow(1f - progress, 3f);
            return one;
        }

        public static float EaseInOutCubic(float x)
        {
            return x < 0.5f ? 4f * x * x * x
                : 1f - MathF.Pow(-2f * x + 2f, 3f) / 2f;
        }
        public static float EaseInCubic(float x) => x * x * x;

        public static float EaseInQuint(float x)
        {
            x = MathHelper.Clamp(x, 0f, 1f);
            return x * x * x * x * x;
        }

        public static float EaseOutQuart(float t) => 1f - (float)Math.Pow(1f - t, 4);
        public static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
        public static float EaseInQuad(float t) => t * t;
        public static float EaseInExpo(float x)
        {
            x = MathHelper.Clamp(x, 0f, 1f);
            return x <= 0f ? 0f : (float)Math.Pow(2.0, 10.0 * x - 10.0);
        }
        public static float ScalePulse(float t)
        {
            float u = t * 2f - 1f;
            return MathF.Max(0f, 1f - u * u);
        }

        public static float BaseEasingSwing(float t, float border_value, float power)
        {
            if (t < border_value) 
            {
                float a = t / border_value;
                return -0.085f * (float)Math.Sin(a * MathHelper.PiOver2);
            }
            
            float s = (t - border_value) / (1f - border_value); 
            float e = 1f - (float)Math.Pow(1f - s, power);

            return MathHelper.Lerp(-0.085f, 1f, e);
        }

        public static float BaseStretchSwing(float t, float border_value, float timer, float swingtime)
        {
            

            if (t <= border_value || timer > swingtime) return 1f;

            float s = (t - border_value) / (1f - border_value);
            float bump = (float)Math.Sin(MathHelper.Pi * s);

            return 1f + 0.22f * bump * bump;
        }

        public static Vector2 CubicBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float u = 1f - t;

            float uuu = u * u * u;
            float uut = 3f * u * u * t;
            float utt = 3f * u * t * t;
            float ttt = t * t * t;

            return uuu * p0 + uut * p1 + utt * p2 + ttt * p3;
        }

        public static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;

            return 1f + c3 * MathF.Pow(t - 1f, 3f) + c1 * MathF.Pow(t - 1f, 2f);
        }
    }
}
