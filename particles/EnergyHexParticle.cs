using System;
using jarkos.core.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
namespace jarkos.particles
{
    public class EnergyHexParticle : Particles
    {
        public float Spin;
        public override BlendState DrawBlendState => BlendState.Additive;
        public override string TexturePath => "jarkos/particles/EnergyHexParticle";
        public EnergyHexParticle(Vector2 position, Vector2 velocity, Color color, int lifetime, float scale, float opacity, float rotationSpeed = 0f)
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
        }
        public override void Update()
        {
            Velocity *= 0.93f;
            Rotation += Spin;
            if (Main.rand.NextBool(10))
                Opacity = Math.Min(1f, Opacity + 0.35f);
            else
                Opacity = Math.Max(0f, Opacity - 0.012f);
            if (LifetimeRatio > 0.6f)
                Scale *= 0.93f;
        }
    }
}
