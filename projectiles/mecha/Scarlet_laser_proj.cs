using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.systems;
using jarkos.particles;
using jarkos.systems;
namespace Jarkos.projectiles.mecha
{
    public class Scarlet_laser_proj : ModProjectile
    {
        public override string Texture => "jarkos/projectiles/Empty_proj";
        private const int MaxTimeLeft = 15;
        private const float BeamLength = 1400f;
        public int HoldoutIndex
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = MaxTimeLeft;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }
        public override void AI()
        {
            int holdoutIdx = HoldoutIndex;
            if (holdoutIdx >= 0 && holdoutIdx < Main.maxProjectiles)
            {
                Projectile holdout = Main.projectile[holdoutIdx];
                if (holdout.active && holdout.type == ModContent.ProjectileType<Scarlet_traveler_Holdout>())
                {
                    Vector2 muzzlePos = holdout.Center + holdout.velocity * 38f + holdout.velocity.RotatedBy(MathHelper.PiOver2) * 2f * holdout.direction;
                    Projectile.Center = muzzlePos;
                    Projectile.velocity = holdout.velocity;
                }
            }
            Projectile.rotation = Projectile.velocity.ToRotation();
            Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.Zero);
            float progress = Projectile.timeLeft / (float)MaxTimeLeft;
            for (float d = 0f; d < BeamLength; d += 150f)
            {
                Lighting.AddLight(Projectile.Center + direction * d, 1f * progress, 0.1f * progress, 0.1f * progress);
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float collisionPoint = 0f;
            Vector2 start = Projectile.Center;
            Vector2 end = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * BeamLength;
            float progress = Projectile.timeLeft / (float)MaxTimeLeft;
            float currentWidth = 14f * progress;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, currentWidth, ref collisionPoint);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(4f, 4f);
                new Spark(target.Center, sparkVel, new Color(255, 60, 60), 16, Main.rand.NextFloat(0.6f, 1.1f), 1f).Spawn();
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            if (HealShaders.Stream != null && HealShaders.HealNoise != null)
            {
                Effect effect = HealShaders.Stream;
                spriteBatch.End();
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 6f);
                effect.Parameters["uOpacity"]?.SetValue(1.5f);
                effect.Parameters["uColor"]?.SetValue(new Color(255, 30, 30).ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(new Color(255, 230, 190).ToVector4());
                List<Vector2> points = new List<Vector2>();
                Vector2 start = Projectile.Center;
                Vector2 direction = Projectile.velocity.SafeNormalize(Vector2.Zero);
                int segmentCount = 20;
                for (int i = 0; i < segmentCount; i++)
                {
                    float t = (float)i / (segmentCount - 1);
                    points.Add(start + direction * BeamLength * t);
                }
                float progress = Projectile.timeLeft / (float)MaxTimeLeft;
                TrailRenderer.Draw(effect, points, t => {
                        float baseWidth = 44f * progress;
                        float startTaper = MathHelper.Clamp(t * 8f, 0f, 1f);
                        float endTaper = MathHelper.Clamp((1f - t) * 4f, 0f, 1f);
                        return baseWidth * startTaper * endTaper;
                    }, t => Color.White * (1f - t), HealShaders.HealNoise);
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            else
            {
                Texture2D pixel = TextureAssets.MagicPixel.Value;
                Vector2 start = Projectile.Center - Main.screenPosition;
                float progress = Projectile.timeLeft / (float)MaxTimeLeft;
                float rotation = Projectile.velocity.ToRotation();
                Vector2 scale = new Vector2(BeamLength, 8f * progress);
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), new Color(255, 50, 50) * progress, rotation, new Vector2(0f, 0.5f), scale, SpriteEffects.None, 0f);
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            return false;
        }
    }
}
