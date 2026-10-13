using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.systems;
namespace jarkos.projectiles.mecha
{
    public class Astra_laser_proj : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_1";
        public List<Vector2> segmentPoints = new List<Vector2>();
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 42;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }
        public override void AI()
        {
            Vector2 muzzlePos = Projectile.Center;
            Vector2 aimDir = Projectile.velocity;
            bool holdoutFound = false;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.type == ModContent.ProjectileType<Astra_Holdout>() && proj.owner == Projectile.owner)
                {
                    muzzlePos = proj.Center + proj.velocity * 155f + proj.velocity.RotatedBy(MathHelper.PiOver2) * -4f * proj.direction;
                    aimDir = proj.velocity;
                    holdoutFound = true;
                    break;
                }
            }
            if (!holdoutFound)
            {
                Projectile.Kill();
                return;
            }
            Projectile.Center = muzzlePos;
            Projectile.velocity = aimDir;
            Vector2 startPoint = muzzlePos;
            Vector2 endPoint = muzzlePos + aimDir * 2500f;
            segmentPoints.Clear();
            float dist = Vector2.Distance(startPoint, endPoint);
            int segments = (int)Math.Max(2, dist / 50f);
            for (int j = 0; j <= segments; j++)
            {
                segmentPoints.Add(Vector2.Lerp(startPoint, endPoint, j / (float)segments));
            }
            Lighting.AddLight(Projectile.Center, 0.4f, 0.8f, 1f);
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (segmentPoints.Count < 2) return false;
            float scale = GetLaserScale();
            float collisionWidth = 80f * scale;
            float collisionPoint = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), segmentPoints[0], segmentPoints[segmentPoints.Count - 1], collisionWidth, ref collisionPoint);
        }
        private float GetLaserScale()
        {
            float maxTime = 42f;
            float timeAlive = maxTime - Projectile.timeLeft;
            if (timeAlive < 6f)
            {
                return timeAlive / 6f;
            }
            else if (Projectile.timeLeft < 10f)
            {
                return Projectile.timeLeft / 10f;
            }
            return 1f;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (segmentPoints.Count < 2) return false;
            SpriteBatch spriteBatch = Main.spriteBatch;
            Color baseColor = new Color(100, 200, 255);
            Color coreColor = new Color(255, 255, 255);
            float scale = GetLaserScale();
            if (HealShaders.Stream != null && HealShaders.HealNoise != null)
            {
                Effect effect = HealShaders.Stream;
                spriteBatch.End();
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 8f + Projectile.identity * 0.1f);
                effect.Parameters["uOpacity"]?.SetValue(1.5f * scale);
                effect.Parameters["uColor"]?.SetValue(baseColor.ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(coreColor.ToVector4());
                TrailRenderer.Draw(effect, segmentPoints, t =>
                    {
                        float baseWidth = 120f * scale;
                        if (t < 0.05f)
                        {
                            return MathHelper.Lerp(20f * scale, baseWidth, t / 0.05f);
                        }
                        else if (t > 0.95f)
                        {
                            return MathHelper.Lerp(baseWidth, 0f, (t - 0.95f) / 0.05f);
                        }
                        return baseWidth;
                    }, t =>
                    {
                        if (t < 0.05f)
                        {
                            return Color.White * (t / 0.05f);
                        }
                        return Color.White;
                    }, HealShaders.HealNoise);
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            else
            {
                Texture2D pixel = TextureAssets.MagicPixel.Value;
                for (int i = 0; i < segmentPoints.Count - 1; i++)
                {
                    float width = 60f * scale;
                    Vector2 start = segmentPoints[i] - Main.screenPosition;
                    Vector2 end = segmentPoints[i + 1] - Main.screenPosition;
                    float rotation = (end - start).ToRotation();
                    float length = Vector2.Distance(start, end);
                    if (length < 1) continue;
                    spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), baseColor * 0.6f * scale, rotation, new Vector2(0, 0.5f), new Vector2(length, width * 2f), SpriteEffects.None, 0f);
                    spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), coreColor * 0.9f * scale, rotation, new Vector2(0, 0.5f), new Vector2(length, width * 0.5f), SpriteEffects.None, 0f);
                }
            }
            return false;
        }
    }
}
