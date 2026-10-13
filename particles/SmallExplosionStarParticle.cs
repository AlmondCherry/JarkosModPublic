using System;
using jarkos.core.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
namespace jarkos.particles
{
    public class SmallExplosionStarParticle : Particles
    {
        public float Spin;
        public float FadeSpeed;
        public override BlendState DrawBlendState => BlendState.Additive;
        public override string TexturePath => "jarkos/particles/SmallExplosionStar";
        public override int FrameCount => 5;
        public SmallExplosionStarParticle(Vector2 position, Vector2 velocity, Color color, int lifetime, float scale, float opacity, float rotationSpeed = 0f)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Scale = scale;
            Lifetime = lifetime;
            Opacity = opacity;
            Spin = rotationSpeed;
            FadeSpeed = opacity / lifetime;
            Frame = 0;
        }
        public override void Update()
        {
            Velocity *= 0.92f;
            Opacity -= FadeSpeed;
            if (Opacity <= 0f)
            {
                Kill();
                return;
            }
            Frame = (int)(LifetimeRatio * 5f);
            if (Frame >= 5)
                Frame = 4;
            if (LifetimeRatio < 0.3f)
                Scale *= 1.05f;
            else
                Scale *= 0.95f;
            Rotation += Spin;
        }
    }
}
