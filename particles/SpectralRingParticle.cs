using System;
using jarkos.core.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace jarkos.particles
{
    public class SpectralRingParticle : Particles
    {
        public float TargetScale;
        public override BlendState DrawBlendState => BlendState.Additive;
        public override string TexturePath => "jarkos/particles/nova/Circleedge_novaburst";

        public SpectralRingParticle(Vector2 position, Color color, int lifetime, float startScale, float targetScale, float opacity)
        {
            Position = position;
            Velocity = Vector2.Zero;
            Color = color;
            Scale = startScale;
            TargetScale = targetScale;
            Lifetime = lifetime;
            Opacity = opacity;
            Time = 0;
        }

        public override void Update()
        {
            Time++;
            float progress = Time / (float)Lifetime;
            Scale = MathHelper.Lerp(Scale, TargetScale, 0.15f);
            Opacity = MathHelper.Lerp(Opacity, 0f, progress * progress);

            if (Time >= Lifetime || Opacity <= 0.01f)
            {
                Kill();
            }
        }
    }
}
