using System;
using jarkos.core.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
namespace jarkos.particles
{
    public class CrystalShardParticle : Particles
    {
        public float Spin;
        private readonly float gravity;
        public override BlendState DrawBlendState => BlendState.AlphaBlend;
        public override string TexturePath => "jarkos/particles/CrystalShardParticle";
        public CrystalShardParticle(Vector2 position, Vector2 velocity, Color color, int lifetime, float scale, float opacity, float rotationSpeed = 0f, float gravityStrength = 0.12f)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Scale = scale;
            Frame = Main.rand.Next(FrameCount);
            Lifetime = lifetime;
            Opacity = opacity;
            Spin = rotationSpeed;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
            gravity = gravityStrength;
        }
        public override void Update()
        {
            Velocity.X *= 0.97f;
            Velocity.Y += gravity;
            if (Velocity.Y > 7f)
                Velocity.Y = 7f;
            Rotation += Spin;
            if (Main.rand.NextBool(14))
                Opacity = Math.Min(1f, Opacity + 0.3f);
            if (LifetimeRatio > 0.66f)
            {
                Opacity -= 0.06f;
                Scale *= 0.96f;
            }
            if (Opacity <= 0.05f)
                Kill();
        }
    }
}
