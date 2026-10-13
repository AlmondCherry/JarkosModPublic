using System;
using jarkos.core.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace jarkos.particles
{
    public class SlashSparkParticle : Particles
    {
        public Vector2 SlashDirection;
        public float BaseScale;
        public float MaxStretchY;
        public Vector2 InitialVelocity;

        public override string TexturePath => "jarkos/particles/Spark";
        public override BlendState DrawBlendState => BlendState.Additive;

        public SlashSparkParticle(Vector2 position, Vector2 velocity, Color color, int lifetime = 6, float scale = 0.7f, float stretchFactor = 4.5f)
        {
            Position = position;
            Velocity = velocity;
            InitialVelocity = velocity;
            SlashDirection = velocity.LengthSquared() > 0.001f ? Vector2.Normalize(velocity) : Vector2.UnitX;
            Color = color;
            Lifetime = Math.Max(lifetime, 6);
            BaseScale = scale;
            MaxStretchY = scale * stretchFactor;
            Opacity = 1f;
            Rotation = SlashDirection.ToRotation() + MathHelper.PiOver2;
        }

        public override void Update()
        {
            float progress = MathHelper.Clamp((float)Time / Lifetime, 0f, 1f);
            float decay = MathF.Pow(1f - progress, 3.5f);
            float speedFactor = MathHelper.Lerp(0.05f, 2.2f, decay);
            Velocity = InitialVelocity * speedFactor;

            Opacity = MathHelper.Lerp(1f, 0f, MathF.Pow(progress, 1.3f));

            if (Time >= Lifetime || Opacity <= 0.01f)
            {
                Kill();
            }
        }

        public override void Draw()
        {
            Texture2D tex = Texture;
            if (tex == null) return;

            Vector2 drawPos = Position - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;

            float progress = MathHelper.Clamp((float)Time / Lifetime, 0f, 1f);

            float scaleX = BaseScale;
            float scaleY;

            if (progress <= 0.35f)
            {
                float stretchRatio = MathF.Sin(progress / 0.35f * MathHelper.PiOver2);
                scaleY = MathHelper.Lerp(BaseScale, MaxStretchY, stretchRatio);
            }
            else
            {
                float returnRatio = (progress - 0.35f) / 0.65f;
                float easeReturn = MathF.Sin(returnRatio * MathHelper.PiOver2);
                scaleY = MathHelper.Lerp(MaxStretchY, BaseScale, easeReturn);
            }

            Vector2 drawScale = new Vector2(scaleX, scaleY);
            Color drawColor = Color * Opacity;

            Main.spriteBatch.Draw(tex, drawPos, null, drawColor, Rotation, origin, drawScale, SpriteEffects.None, 0f);
        }
    }
}
