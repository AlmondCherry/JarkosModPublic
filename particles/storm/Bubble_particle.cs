using System;
using jarkos.core.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
namespace jarkos.particles.storm
{
    public class Bubble_particle : Particles
    {
        public float Spin;
        public override BlendState DrawBlendState => BlendState.Additive;
        public override string TexturePath => "jarkos/particles/storm/Bubble_particle";
        public Bubble_particle(Vector2 position, Vector2 velocity, Color color, int lifetime, float scale, float opacity, float rotationSpeed = 0f)
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
            Opacity *= 0.965f;
            if(Opacity < 0.05f)
            {
                Kill();
            }
            Scale -= 0.01f;
            Rotation += Spin;
        }
         }
}
