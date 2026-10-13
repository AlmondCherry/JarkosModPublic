using System;
using jarkos.core.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
namespace jarkos.particles
{
    public class AccretionStreakParticle : Particles
    {
        private readonly Projectile anchor;
        private float orbitR;
        private float angle;
        private readonly float angVel;
        private readonly float startR;
        private readonly float killR;
        public override BlendState DrawBlendState => BlendState.Additive;
        public override string TexturePath => "jarkos/particles/AccretionStreakParticle";
        public AccretionStreakParticle(Projectile anchor, float orbitRadius, float killRadius, Color color, int lifetime, float scale, float opacity)
        {
            this.anchor = anchor;
            orbitR = orbitRadius;
            startR = orbitRadius;
            killR = killRadius;
            angle = Main.rand.NextFloat(MathHelper.TwoPi);
            angVel = (Main.rand.NextBool() ? 1f : 1f) * Main.rand.NextFloat(0.045f, 0.085f);
            Color = color;
            Lifetime = lifetime;
            Scale = scale;
            Opacity = 0f;
            opacityTarget = opacity;
            Position = anchor.Center + angle.ToRotationVector2() * orbitR * new Vector2(1f, 0.32f);
        }
        private readonly float opacityTarget;
        public override void Update()
        {
            if (anchor == null || !anchor.active)
            {
                Opacity -= 0.12f;
                if (Opacity <= 0f) Kill();
                return;
            }
            float speedup = MathHelper.Clamp(startR / Math.Max(orbitR, 8f), 1f, 4.5f);
            angle += angVel * speedup;
            orbitR *= 0.962f;
            Position = anchor.Center + angle.ToRotationVector2() * orbitR * new Vector2(1f, 0.32f);
            Velocity = Vector2.Zero;
            Vector2 tangent = new Vector2(-(float)Math.Sin(angle), 0.32f * (float)Math.Cos(angle));
            Rotation = tangent.ToRotation();
            Opacity = Math.Min(Opacity + 0.08f, opacityTarget);
            if (orbitR < killR)
            {
                Opacity -= 0.22f;
                Scale *= 0.9f;
                if (Opacity <= 0.05f) Kill();
            }
        }
    }
}
