using System;
using jarkos.core.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
namespace jarkos.particles
{
    public class MagmaParticle : Particles
    {
        public float Spin;
        public override BlendState DrawBlendState => BlendState.Additive;
        public override string TexturePath => "jarkos/particles/MagmaParticle";
        public MagmaParticle(Vector2 position, Vector2 velocity, Color color, int lifetime, float scale, float opacity, float rotationSpeed = 0f)
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
            Scale -= 0.0003f;
            Velocity.Y = 4f;
            Rotation = Velocity.ToRotation() + MathHelper.PiOver2;
        }
         }
}
