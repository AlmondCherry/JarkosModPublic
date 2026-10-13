using System;
using jarkos.core.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
namespace jarkos.particles
{
    public class StarParticle : Particles
    {
        public float Spin;
        public bool Ambient;
        private float _phase;
        private float _initialOpacity;
        public override BlendState DrawBlendState => BlendState.Additive;
        public override string TexturePath => "jarkos/particles/StarParticle";
        public StarParticle(Vector2 position, Vector2 velocity, Color color, int lifetime, float scale, float opacity, float rotationSpeed = 0f, bool ambient = false)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Scale = MathHelper.Clamp(scale, 0.05f, 0.45f);
            Frame = Main.rand.Next(FrameCount);
            Lifetime = lifetime;
            Opacity = opacity;
            Spin = rotationSpeed;
            Ambient = ambient;
            if (Ambient)
            {
                _initialOpacity = opacity;
                _phase = Main.rand.NextFloat(MathHelper.TwoPi);
                Opacity = 0f;
            }
        }
        public override void Update()
        {
            if (Ambient)
            {
                _phase += 0.05f;
                float fade = 1f;
                if (LifetimeRatio < 0.1f) fade = LifetimeRatio / 0.1f;
                else if (LifetimeRatio > 0.8f) fade = 1f - ((LifetimeRatio - 0.8f) / 0.2f);
                Opacity = _initialOpacity * fade * (0.6f + 0.4f * (float)Math.Sin(_phase));
                if (LifetimeRatio >= 1f)
                {
                    Kill();
                }
            }
            else
            {
                Velocity *= 0.95f;
                Opacity -= 0.025f;
                if(Opacity < 0.05f)
                {
                    Kill();
                }
                if (LifetimeRatio < 0.7f)
                    Scale *= 0.98f;
                Rotation += Spin;
            }
        }
    }

    public class CurvedStarParticle : Particles
    {
        public float Spin;
        public float CurveRate;

        public override BlendState DrawBlendState => BlendState.Additive;
        public override string TexturePath => "jarkos/particles/StarParticle";

        public CurvedStarParticle(Vector2 position, Vector2 velocity, Color color, int lifetime, float scale, float opacity, float curveRate, float rotationSpeed = 0f)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Scale = MathHelper.Clamp(scale, 0.05f, 0.45f);
            Frame = Main.rand.Next(FrameCount);
            Lifetime = lifetime;
            Opacity = opacity;
            CurveRate = curveRate;
            Spin = rotationSpeed;
        }

        public override void Update()
        {
            Velocity = Velocity.RotatedBy(CurveRate) * 0.96f;
            Opacity -= 0.02f;
            if (Opacity < 0.05f || LifetimeRatio >= 1f)
            {
                Kill();
            }
            if (LifetimeRatio < 0.7f)
                Scale *= 0.98f;

            Rotation += Spin;
        }
    }
}
