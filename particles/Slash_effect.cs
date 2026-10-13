using System;
using jarkos.core.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
namespace jarkos.particles
{
    public class Slash_effect : Particles
    {
        public float Spin;
        public override BlendState DrawBlendState => BlendState.Additive;
        public override string TexturePath => "jarkos/particles/Slash_effect";
        public Slash_effect(Vector2 position, Vector2 velocity, Color color, int lifetime, float scale, float opacity, float rotationSpeed = 0f)
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
            Player player = Main.LocalPlayer;
            Position = player.Center;
            Opacity *= 0.85f;
            if(Opacity < 0.05f)
            {
                Kill();
            }
            Velocity *= 0.75f;
            Rotation = Velocity.ToRotation() + MathHelper.PiOver2;
        }
         }
}
