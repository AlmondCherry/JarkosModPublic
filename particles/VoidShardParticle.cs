using System;
using jarkos.core.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
namespace jarkos.particles
{
    public class VoidShardParticle : Particles
    {
        public float Spin;
        public override BlendState DrawBlendState => BlendState.AlphaBlend;
        public override string TexturePath => "jarkos/particles/VoidShardParticle";
        public VoidShardParticle(Vector2 position, Vector2 velocity, Color color, int lifetime, float scale, float opacity, float rotationSpeed = 0f)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Lifetime = lifetime;
            Scale = scale;
            Opacity = opacity;
            Spin = rotationSpeed == 0f ? Main.rand.NextFloat(-0.3f, 0.3f) : rotationSpeed;
            Rotation = Main.rand.NextFloat(MathHelper.TwoPi);
        }
        public override void Update()
        {
            Velocity *= 1.045f;
            Rotation += Spin;
            if (LifetimeRatio > 0.65f)
            {
                Opacity -= 0.09f;
                Scale *= 0.93f;
            }
            if (Opacity <= 0.05f)
                Kill();
        }
    }
}
