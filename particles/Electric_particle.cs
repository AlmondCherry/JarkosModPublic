using System;
using jarkos.core.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace jarkos.particles
{
    public class Electric_particle : Particles
    {
        public float InitialScale;
        public float Deceleration;

        public override string TexturePath => "jarkos/particles/Stretching_particle";
        public override BlendState DrawBlendState => BlendState.Additive;

        public Electric_particle(Vector2 position, Vector2 velocity, Color color, int lifetime, float scale = 1f, float opacity = 1f, float deceleration = 0.93f)
        {
            Position = position;
            Velocity = velocity;
            Color = color;
            Lifetime = lifetime;
            InitialScale = scale;
            Scale = scale;
            Opacity = opacity;
            Deceleration = deceleration;
            Rotation = velocity != Vector2.Zero ? velocity.ToRotation() : 0f;
        }

        public override void Update()
        {
            Velocity *= Deceleration;

            if (Velocity != Vector2.Zero)
            {
                Rotation = Velocity.ToRotation() + MathHelper.PiOver2;
            }
            float progress = MathHelper.Clamp((float)Time / Lifetime, 0f, 1f);
            Scale = MathHelper.Lerp(InitialScale, 0f, progress);
            Opacity = MathHelper.Lerp(1f, 0f, progress * progress);

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

            float speed = Velocity.Length();
            float stretchY = 1f + speed * 0.3f;
            Vector2 baseScale = new Vector2(Scale, Scale * stretchY);

            Vector2 scale3 = baseScale * 1.7f;
            Color color3 = Color * (Opacity * 0.25f);
            Main.spriteBatch.Draw(tex, drawPos, null, color3, Rotation, origin, scale3, SpriteEffects.None, 0f);

            Vector2 scale2 = baseScale * 1.25f;
            Color color2 = Color * Opacity;
            Main.spriteBatch.Draw(tex, drawPos, null, color2, Rotation, origin, scale2, SpriteEffects.None, 0f);

            Vector2 scale1 = baseScale;
            Color color1 = Color.White * Opacity;
            Main.spriteBatch.Draw(tex, drawPos, null, color1, Rotation, origin, scale1, SpriteEffects.None, 0f);
        }
    }
}
