using jarkos.core.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace jarkos.particles
{
    public class UpGlow_particle : Particles
    {
        public NPC TargetEnemy;
        public float TargetWidth;
        public float UpDistance;
        public float WidthScale = 1f;
        public float InitialVerticalScale = 1f;
        public Vector2 StartPosition;
        public bool TrackEnemy;

        public override string TexturePath => "jarkos/particles/UpGlow_particle";
        public override BlendState DrawBlendState => BlendState.Additive;

        public UpGlow_particle(Vector2 position, float targetWidth, Color color, int lifetime, float upDistance = 60f, float scale = 1f, float widthScale = 1f, float initialVerticalScale = 0.1f)
        {
            StartPosition = position;
            Position = position;
            TargetWidth = targetWidth;
            Color = color;
            Lifetime = lifetime;
            UpDistance = upDistance;
            Scale = scale;
            WidthScale = widthScale;
            InitialVerticalScale = initialVerticalScale;
            Opacity = 1f;
            TrackEnemy = false;
        }

        public UpGlow_particle(NPC enemy, Color color, int lifetime, float upDistance = 60f, float scale = 1f, float widthScale = 1f, float initialVerticalScale = 0.1f)
        {
            TargetEnemy = enemy;
            if (enemy != null)
            {
                StartPosition = enemy.Center;
                TargetWidth = enemy.width;
            }
            else
            {
                StartPosition = Vector2.Zero;
                TargetWidth = 40f;
            }
            Position = StartPosition;
            Color = color;
            Lifetime = lifetime;
            UpDistance = upDistance;
            Scale = scale;
            WidthScale = widthScale;
            InitialVerticalScale = initialVerticalScale;
            Opacity = 1f;
            TrackEnemy = true;
        }

        public override void Update()
        {
            float progress = MathHelper.Clamp(LifetimeRatio, 0f, 1f);

            float easeOutProgress = MathSystem.EaseOutCubic(progress);

            if (TrackEnemy && TargetEnemy != null && TargetEnemy.active)
            {
                TargetWidth = TargetEnemy.width;
                StartPosition = TargetEnemy.Center;
            }

            Position = StartPosition - new Vector2(0f, UpDistance * easeOutProgress);

            if (progress < 0.15f)
            {
                Opacity = MathHelper.Lerp(0f, 1f, progress / 0.15f);
            }
            else if (progress > 0.65f)
            {
                Opacity = MathHelper.Lerp(1f, 0f, (progress - 0.65f) / 0.35f);
            }
            else
            {
                Opacity = 1f;
            }

            if (Time >= Lifetime || Opacity <= 0.001f)
            {
                Kill();
            }
        }

        public override void Draw()
        {
            Texture2D tex = ModContent.Request<Texture2D>(TexturePath, ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            if (tex == null)
                return;

            Vector2 drawPos = Position - Main.screenPosition;
            Vector2 origin = tex.Size() * 0.5f;

            float progress = MathHelper.Clamp(LifetimeRatio, 0f, 1f);

            float effectiveWidth = TargetWidth * WidthScale;
            float scaleX = effectiveWidth / tex.Width * Scale;

            float verticalFactor = MathHelper.Clamp(progress, 0f, 1f);
            
            float scaleY = Scale * (1 - verticalFactor);

            Vector2 mainScale = new Vector2(scaleX, scaleY);

            Color mainColor = Color * (Opacity * 0.85f);
            Main.spriteBatch.Draw(tex, drawPos, null, mainColor, Rotation, origin, mainScale, SpriteEffects.None, 0f);
        }
    }
}
