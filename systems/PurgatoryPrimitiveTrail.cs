using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;

namespace jarkos.systems
{
    public struct PurgatoryTrailSample
    {
        public Vector2 Tip;
        public float Time;
        public float Speed;
    }
    public static class PurgatoryPrimitiveTrail
    {
        public static void Draw(IList<PurgatoryTrailSample> samples, float baseWidth, float maxWidth, Color innerColor, Color outerColor)
        {
            if (samples == null || samples.Count < 2)
                return;

            Texture2D pixel = TextureAssets.MagicPixel.Value;
            if (pixel == null)
                return;

            int n = samples.Count;
            Vector2 origin = new Vector2(0.5f, 0.5f);

            for (int layer = 0; layer < 2; layer++)
            {
                bool outer = layer == 0;
                float widthMul = outer ? 1.6f : 1.0f;
                float alphaMul = outer ? 0.55f : 1.0f;
                Color c = outer ? outerColor : innerColor;

                for (int i = 0; i < n - 1; i++)
                {
                    PurgatoryTrailSample a = samples[i];
                    PurgatoryTrailSample b = samples[i + 1];
                    Vector2 delta = b.Tip - a.Tip;

                    float length = delta.Length();

                    if (length < 0.5f)
                        continue;

                    Vector2 mid = (a.Tip + b.Tip) * 0.5f;

                    float angle = delta.ToRotation();
                    float tNorm = i / (float)(n - 1);
                    float age = MathHelper.Clamp((a.Time + b.Time) * 0.5f, 0f, 1f);
                    float speed = MathF.Max(a.Speed, b.Speed);
                    float speedScale = MathHelper.Clamp(speed * 0.06f, 0f, 1f);
                    float bulge = 1f - MathF.Abs(tNorm - 0.4f) * 1.6f;

                    bulge = MathHelper.Clamp(bulge, 0f, 1f);

                    float width = MathHelper.Lerp(baseWidth, maxWidth, speedScale) * MathHelper.Lerp(0.55f, 1.05f, bulge) * MathHelper.Lerp(0.4f, 1f, age) * widthMul;

                    width = MathF.Max(width, 1.5f);

                    float headFade = MathHelper.Clamp(tNorm * 4.5f, 0f, 1f);
                    float tailFade = MathHelper.Clamp((1f - tNorm) * 1.6f, 0f, 1f);
                    float alpha = headFade * tailFade * age * alphaMul;
                    if (alpha <= 0.005f)
                        continue;

                    Color drawColor = c * alpha;
                    Vector2 drawPos = mid - Main.screenPosition;
                    Vector2 scale = new Vector2(length + 2f, width * 2f);
                    Main.spriteBatch.Draw(pixel, drawPos, null, drawColor, angle, origin, scale, SpriteEffects.None, 0f);
                }
            }
            PurgatoryTrailSample head = samples[0];
            float headSpeed = MathHelper.Clamp(head.Speed * 0.06f, 0f, 1f);
            float dotR = MathHelper.Lerp(baseWidth * 1.8f, maxWidth * 1.8f, headSpeed) * MathHelper.Clamp(head.Time, 0f, 1f);
            
            if (dotR > 1f)
            {
                Vector2 headPos = head.Tip - Main.screenPosition;
                Color hot = innerColor * MathHelper.Clamp(head.Time, 0f, 1f);
                Main.spriteBatch.Draw(pixel, headPos, null, hot, 0f, origin, new Vector2(dotR, dotR), SpriteEffects.None, 0f);
                Color halo = outerColor * (MathHelper.Clamp(head.Time, 0f, 1f) * 0.45f);
                Main.spriteBatch.Draw(pixel, headPos, null, halo, 0f, origin, new Vector2(dotR * 1.9f, dotR * 1.9f), SpriteEffects.None, 0f);
            }
        }
    }
}
