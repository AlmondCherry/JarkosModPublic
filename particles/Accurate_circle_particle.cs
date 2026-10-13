using System;
using jarkos.core.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
namespace jarkos.particles
{
    public class Accurate_circle_particle : Particles
    {
        public float Spin;
        public override BlendState DrawBlendState => BlendState.Additive;
        public override string TexturePath => "jarkos/particles/Accurate_circle_particle";
        public Accurate_circle_particle(Vector2 position, Vector2 velocity, Color color, int lifetime, float scale, float opacity, float rotationSpeed = 0f)
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
            Vector2 magnitude = new Vector2(Main.rand.NextFloat(-1, 1), Main.rand.NextFloat(-1, 1));
			magnitude.Normalize();
            Velocity *= magnitude * 0.01f * Lifetime;
            Vector2 hz = new Vector2(3, 6);
            if(Velocity.X > 4)
            {
                Velocity.X = 4;
            }
            if(Velocity.X < -4)
            {
                Velocity.X = -4;
            }
            if(Velocity.Y > 4)
            {
                Velocity.Y = 4;
            }
            if(Velocity.Y < -4)
            {
                Velocity.Y = -4;
            }
            Opacity *= 0.85f;
            if(Opacity < 0.05f)
            {
                Kill();
            }
            Rotation += Spin;
        }
         }
}
