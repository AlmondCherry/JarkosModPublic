using System;
using jarkos.core.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
namespace jarkos.particles
{
    public class Raindrop_particle : Particles
    {
        public float Spin;
        public float rand_rot = Main.rand.NextFloat(-34f, 186);
        public override BlendState DrawBlendState => BlendState.Additive;
        public override string TexturePath => "jarkos/particles/Raindrop_particle";
        public Raindrop_particle(Vector2 position, Vector2 velocity, Color color, int lifetime, float scale, float opacity, float rotationSpeed = 0f)
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
            Velocity.Y += Main.rand.NextFloat(0.25f, 0.45f);
            Scale -= 0.005f;
            Rotation = Velocity.ToRotation() + MathHelper.PiOver2;
        }
         }
}
