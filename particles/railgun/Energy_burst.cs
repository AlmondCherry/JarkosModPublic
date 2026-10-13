using System;
using jarkos.core.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
namespace jarkos.particles.storm
{
    public class Energy_burst : Particles
    {
        public float Spin;
        public float size_var;
        public override BlendState DrawBlendState => BlendState.Additive;
        public override string TexturePath => "jarkos/particles/railgun/Energy_burst";
        public Energy_burst(Vector2 position, Vector2 velocity, Color color, int lifetime, float scale, float opacity, float rotationSpeed = 0f)
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
            size_var++;
            Opacity *= 0.93f;
            if (Opacity < 0.05f)
            {
                Kill();
            }
            Scale += 0.06f * size_var;
            if (Scale > 5.5f)
            {
                Scale = 5.5f;
            }
            Rotation += Spin;
        }
    }
}
