using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace jarkos.UI.Common;
    public class WarpTransition
    {
        public enum Phase { Idle, Forward, Reverse }
        public Phase State { get; private set; } = Phase.Idle;
        public Vector2 FocusWorld { get; private set; }
        public Vector2 FocusScreen { get; set; }
        public Color WarpColor { get; private set; } = Color.White;
        public float Duration = 1.0f;
        public float Time { get; private set; }
        private readonly ParticlePool _streaks = new();
        private bool _peaked;
        public event Action OnForwardPeak;
        public event Action OnReverseDone;
        public bool Busy => State != Phase.Idle;
        public float Progress => Easing.Clamp01(Time / Duration);
        private float DirProgress => State == Phase.Reverse ? 1f - Progress : Progress;
        public void BeginForward(Vector2 focusWorld, Vector2 focusScreen, Color color)
        {
            if (Busy) return;
            FocusWorld = focusWorld; FocusScreen = focusScreen; WarpColor = color;
            State = Phase.Forward; Time = 0f; _peaked = false;
        }
        public void BeginReverse(Vector2 focusWorld, Vector2 focusScreen, Color color)
        {
            if (Busy) return;
            FocusWorld = focusWorld; FocusScreen = focusScreen; WarpColor = color;
            State = Phase.Reverse; Time = 0f; _peaked = false;
        }
        public void Update(float dt)
        {
            _streaks.Update(dt);
            if (State == Phase.Idle) return;
            Time += dt;
            float p = DirProgress;
            if (!_peaked && p >= 0.62f)
            {
                _peaked = true;
                _streaks.Burst(FocusScreen, WarpColor, 50, 800f, 0.45f);
                _streaks.Burst(FocusScreen, Color.White, 20, 1000f, 0.25f);
                if (State == Phase.Forward) OnForwardPeak?.Invoke();
            }
            if (Progress >= 1f)
            {
                if (State == Phase.Reverse) OnReverseDone?.Invoke();
                State = Phase.Idle; Time = 0f;
            }
        }
        public float ZoomMul()
        {
            if (State == Phase.Idle) return 1f;
            float p = DirProgress;
            float e = p * p * p * p;
            return Easing.Lerp(1f, 15f, e);
        }
        public float OffsetLerp()
        {
            if (State == Phase.Idle) return 0f;
            return Easing.EaseInOutCubic(DirProgress);
        }
        public void DrawTunnel(SpriteBatch sb)
        {
            if (State == Phase.Idle) 
            { 
                _streaks.Draw(sb, Draw.Pixel); 
                return; 
            }

            float p = DirProgress;
            int w = Main.screenWidth, h = Main.screenHeight;
            float intensity = Easing.EaseInCubic(p);
            float twist = p * MathF.PI * 0.5f;
            int lines = 60;

            for (int i = 0; i < lines; i++)
            {
                float rRand = i * 137.5f % 1f;
                float ang = MathHelper.TwoPi * i / lines + twist * (rRand > 0.5f ? 1 : -1);
                var dir = new Vector2(MathF.Cos(ang), MathF.Sin(ang));
                float inner = 20f + p * (100f + rRand * 200f);
                float outer = inner + 150f + p * (800f + rRand * 400f);

                Vector2 a = FocusScreen + dir * inner;
                Vector2 b = FocusScreen + dir * outer;
                Color c = Color.Lerp(WarpColor, Color.White, rRand);
                float thick = 1f + rRand * 3f;
                Draw.Line(sb, a, b, c * (intensity * (0.1f + rRand * 0.3f)), thick);
            }
            for (int r = 0; r < 3; r++)
            {
                float ringP = (p + r * 0.33f) % 1f;
                float ringRadius = ringP * 1200f * intensity;
                float ringAlpha = (1f - ringP) * intensity * 0.4f;
                if (ringRadius > 10f)
                    Draw.RadialGlow(sb, FocusScreen, ringRadius, WarpColor * ringAlpha);
            }
            if (Jarkos_config.ScreenFlashesEnabled)
            {
                float flash = Easing.Clamp01((p - 0.6f) / 0.4f);
                flash = MathSystem.EaseOutCubic(flash);

                if (State == Phase.Reverse) 
                flash = Easing.Clamp01((p - 0.6f) / 0.4f);
                sb.Draw(Draw.Pixel, new Rectangle(0, 0, w, h), WarpColor * (flash * 0.5f));
                sb.Draw(Draw.Pixel, new Rectangle(0, 0, w, h), Color.White * (flash * 0.75f));
            }
            Draw.RadialGlow(sb, FocusScreen, 100f + p * 800f, WarpColor * intensity);
            _streaks.Draw(sb, Draw.Pixel);
        }
    }
