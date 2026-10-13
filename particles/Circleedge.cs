using System;
using jarkos.core.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
namespace jarkos.particles
{
    public class Circleedge : Particles
    {
        public float Spin;
        public float ExpansionRate = 0.10f;
        public override BlendState DrawBlendState => BlendState.Additive;
        public override string TexturePath => "jarkos/particles/Circleedge";
        public Circleedge(Vector2 position, Vector2 velocity, Color color, int lifetime, float scale, float opacity, float rotationSpeed = 0f)
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
            Scale += ExpansionRate;
            Rotation += Spin;
        }
         }
}
