using System;
using jarkos.core.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
namespace jarkos.particles
{
    public class Dust1_particle : Particles
    {
        public float Spin;
        public override BlendState DrawBlendState => BlendState.Additive;
        public override string TexturePath => "jarkos/particles/Dust1_particle";
        public Dust1_particle(Vector2 position, Vector2 velocity, Color color, int lifetime, float scale, float opacity, float rotationSpeed = 0f)
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
            Opacity *= 0.85f;
            if(Opacity < 0.05f)
            {
                Kill();
            }
            Scale -= 0.005f;
            Rotation = Velocity.ToRotation() + MathHelper.PiOver2;
        }
         }
}
