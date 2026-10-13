using System;
using jarkos.core.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
namespace jarkos.particles
{
    public class StarParticleHorizontal : Particles
    {
        public float Spin;
        public override BlendState DrawBlendState => BlendState.Additive;
        public override string TexturePath => "jarkos/particles/StarParticleHorizontal";
        public StarParticleHorizontal(Vector2 position, Vector2 velocity, Color color, int lifetime, float scale, float opacity, float rotationSpeed = 0f)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Scale = scale;
            Frame = Main.rand.Next(FrameCount);
            Lifetime = lifetime;
            Opacity = opacity;
            Spin = rotationSpeed;
        }
        public override void Update()
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
