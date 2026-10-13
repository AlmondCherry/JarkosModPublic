using System;
using jarkos.core.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace jarkos.particles
{
    public class BloodAscentParticle : Particles
    {
        public float InitialScale;
        public float TargetY;

        public override string TexturePath => "jarkos/particles/Dust2_particle";
        public override BlendState DrawBlendState => BlendState.Additive;

        public BloodAscentParticle(Vector2 position, Vector2 velocity, Color color, int lifetime, float scale, float targetY)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Lifetime = lifetime;
            InitialScale = scale;
            Scale = scale;
            Opacity = 1f;
            TargetY = targetY;
        }

        public override void Update()
        {
            Position += Velocity;

            if (Time >= 24)
            {
                Velocity.Y *= 1.07f;
            }

            float progress = MathHelper.Clamp((float)Time / Lifetime, 0f, 1f);

            Scale = MathHelper.Lerp(InitialScale, 0f, progress);
            
            if (Position.Y <= TargetY || progress >= 0.85f)
            {
                Opacity -= 0.08f;
            }

            if (Time >= Lifetime || Scale <= 0.01f || Opacity <= 0.01f)
            {
                Kill();
            }
        }

        public override void Draw()
        {
            Texture2D tex = ModContent.Request<Texture2D>(TexturePath, ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            Vector2 drawPos = Position - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;

            Vector2 scale2 = new Vector2(Scale * 1.35f);
            Color color2 = Color * Opacity;
            Main.spriteBatch.Draw(tex, drawPos, null, color2, Rotation, origin, scale2, SpriteEffects.None, 0f);

            Vector2 scale1 = new Vector2(Scale);
            Color color1 = Color.White * Opacity;
            Main.spriteBatch.Draw(tex, drawPos, null, color1, Rotation, origin, scale1, SpriteEffects.None, 0f);
        }
    }
}
