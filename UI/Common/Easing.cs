using System;
using Microsoft.Xna.Framework;

namespace jarkos.UI.Common
{
    public static class Easing
    {
        public static float Linear(float t) => t;
        public static float SmoothStep(float t)
        {
            t = Clamp01(t);
            return t * t * (3f - 2f * t);
        }
        public static float EaseInCubic(float t)
        {
            t = Clamp01(t);
            return t * t * t;
        }
        public static float EaseInOutCubic(float t)
        {
            t = Clamp01(t);
            return t < 0.5f ? 4f * t * t * t : 1f - MathF.Pow(-2f * t + 2f, 3f) / 2f;
        }
        public static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f, c3 = 1.70158f + 1f;
            t = Clamp01(t);
            float inv = t - 1f;
            return 1f + c3 * inv * inv * inv + c1 * inv * inv;
        }
        public static float Clamp01(float t) => t < 0f ? 0f : (t > 1f ? 1f : t);
        public static float Lerp(float a, float b, float t) => a + (b - a) * Clamp01(t);
        public static Vector2 Lerp(Vector2 a, Vector2 b, float t) => Vector2.Lerp(a, b, Clamp01(t));
        public static float Breathe(float time, float frequency, float phase = 0f) => (MathF.Sin(time * frequency + phase) + 1f) * 0.5f;
    }
    public class Tween
    {
        public float Value { get; private set; }
        public float Target { get; private set; }
        public float Duration;
        public bool Active => MathF.Abs(Value - Target) > 0.0001f;
        private readonly Func<float, float> _ease;
        public Tween(float duration, Func<float, float> ease = null)
        {
            Duration = duration;
            _ease = ease ?? Easing.EaseInOutCubic;
        }
        public void Set(float v) { Value = Target = Easing.Clamp01(v); }
        public void To(float target) => Target = Easing.Clamp01(target);
        public void Toggle() => Target = Target > 0.5f ? 0f : 1f;
        public void Update(float dt)
        {
            if (!Active) return;
            float step = Duration <= 0f ? 1f : dt / Duration;
            if (Value < Target) Value = MathF.Min(Target, Value + step);
            else Value = MathF.Max(Target, Value - step);
        }
        public float Eased => _ease(Value);
    }
}
