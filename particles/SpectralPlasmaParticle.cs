using System;
using jarkos.core.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace jarkos.particles
{
    public class SpectralPlasmaParticle : Particles
    {
        public float Spin;
        public float InitialScale;
        public Color EndColor;

        public override BlendState DrawBlendState => BlendState.Additive;
        public override string TexturePath => "jarkos/particles/Solid_particle";

        public SpectralPlasmaParticle(Vector2 position, Vector2 velocity, Color startColor, Color endColor, int lifetime, float scale, float opacity, float rotationSpeed = 0f)
        {
            Position = position;
            Velocity = velocity;
            Color = startColor;
            EndColor = endColor;
            InitialScale = scale;
            Scale = scale;
            Lifetime = lifetime;
            Opacity = opacity;
            Spin = rotationSpeed;
            Time = 0;
        }

        public override void Update()
        {
            Position += Velocity;
            Velocity *= 0.94f;
            Time++;

            float progress = Time / (float)Lifetime;
            Color = Color.Lerp(Color, EndColor, 0.1f);
            Scale = MathHelper.Lerp(InitialScale, 0f, progress);
            Opacity = MathHelper.Lerp(Opacity, 0f, progress * progress);

            Rotation += Spin;

            if (Time >= Lifetime || Opacity <= 0.01f)
            {
                Kill();
            }
        }
    }
}
