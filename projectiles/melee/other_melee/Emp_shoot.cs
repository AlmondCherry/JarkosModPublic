using System;
using jarkos.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.systems;

namespace jarkos.projectiles.melee.other_melee
{
    public class Emp_shoot : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = false;
            Projectile.scale = 1f;
            Projectile.timeLeft = 120;
            Projectile.light = 0.5f;
            DrawOffsetX = -(int)((22 / 2) - (Projectile.Size.X / 2));
            DrawOriginOffsetY = -(int)((40 / 2) - (Projectile.Size.Y / 2));
        }
        public override void AI()
        {
            int parentID = -1;
            for (int i = 0; i < Main.maxProjectiles; i++) {
                if (Main.projectile[i].active && Main.projectile[i].owner == Projectile.owner && Main.projectile[i].type == ModContent.ProjectileType<Empyoyoproj>() && Main.projectile[i].identity == (int)Projectile.ai[0]) {
                    parentID = i;
                    break;
                }
            }
            if (parentID == -1)
            {
                Projectile.Kill();
                return;
            }
            Projectile parent = Main.projectile[parentID];
            float timer = 120 - Projectile.timeLeft;
            float baseAngle = MathHelper.TwoPi / 8f * Projectile.ai[1];
            float currentAngle = baseAngle + timer * 0.02f;
            float cycle = MathF.Sin(timer * MathHelper.Pi / 120f);
            float radius = MathHelper.Lerp(0f, 350f, cycle);
            Vector2 targetCenter = parent.Center + currentAngle.ToRotationVector2() * radius;
            Projectile.Center = targetCenter;
            Projectile.velocity = Vector2.Zero;
            Projectile.rotation = currentAngle + MathHelper.PiOver2;
            if (Main.rand.NextBool(6))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.OrangeTorch, Main.rand.NextVector2Circular(1f, 1f), Scale: 1.2f);
                dust.noGravity = true;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int a = 0; a < 3; a++)
            {
                Vector2 speed = Main.rand.NextVector2Circular(1f, 1f).SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(4f, 8f);
                new Spark(target.Center, speed, Color.Orange, 20, Main.rand.NextFloat(0.3f, 0.8f), 1f, 0f).Spawn();
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int a = 0; a < 8; a++)
            {
                Vector2 speed = Main.rand.NextVector2CircularEdge(2f, 2f);
                Dust d = Dust.NewDustPerfect(Projectile.position, DustID.OrangeTorch, speed, Scale: 1.2f);
                d.noGravity = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            int parentID = -1;
            for (int i = 0; i < Main.maxProjectiles; i++) {
                if (Main.projectile[i].active && Main.projectile[i].owner == Projectile.owner && Main.projectile[i].type == ModContent.ProjectileType<Empyoyoproj>() && Main.projectile[i].identity == (int)Projectile.ai[0]) {
                    parentID = i;
                    break;
                }
            }
            if (parentID == -1) return false;
            Projectile parent = Main.projectile[parentID];
            int numPoints = 60;
            Vector2[] points = new Vector2[numPoints];
            float currentTimer = 120 - Projectile.timeLeft;
            for (int i = 0; i < numPoints; i++)
            {
                float t = currentTimer - (i * 0.5f);
                if (t < 0) t = 0;
                float baseAngle = MathHelper.TwoPi / 8 * Projectile.ai[1];
                float pastAngle = baseAngle + t * 0.02f;
                float pastCycle = MathF.Sin(t * MathHelper.Pi / 120f);
                float pastRadius = MathHelper.Lerp(0f, 350f, pastCycle);
                points[i] = parent.Center + pastAngle.ToRotationVector2() * pastRadius;
            }
            if (HealShaders.Stream != null && HealShaders.HealNoise != null)
            {
                Effect effect = HealShaders.Stream;
                Main.spriteBatch.End();
                float cycle = MathF.Sin(currentTimer * MathHelper.Pi / 120f);
                float bright = 0.2f + cycle * 1.5f;
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 2f);
                effect.Parameters["uOpacity"]?.SetValue(bright);
                effect.Parameters["uColor"]?.SetValue(new Color(255, 100, 0).ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(new Color(255, 200, 50).ToVector4());
                TrailRenderer.Draw(effect, points, t => 20f * MathF.Pow(1f - t, 1.2f) * bright + 2f, t => Color.White * (1f - t), HealShaders.HealNoise);
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            else
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
                Texture2D textureFallback = TextureAssets.Projectile[Projectile.type].Value;
                int numFramesF = Main.projFrames[Projectile.type];
                int frameHeightF = textureFallback.Height / numFramesF;
                int yPosF = frameHeightF * Projectile.frame;
                Rectangle rectangleF = new Rectangle(0, yPosF, textureFallback.Width, frameHeightF);
                Vector2 originF = rectangleF.Size() / 2f;
                for (int k = 0; k < numPoints; k++)
                {
                    Vector2 drawPos = points[k] - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
                    float progress = (numPoints - k) / (float)numPoints;
                    Color color = Color.Orange * progress * 0.8f;
                    float size = Projectile.scale * progress;
                    Main.EntitySpriteDraw(textureFallback, drawPos, rectangleF, color, Projectile.rotation, originF, size, SpriteEffects.None, 0);
                }
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
            }
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            int numFrames = Main.projFrames[Projectile.type];
            int frameHeight = texture.Height / numFrames;
            int yPos = frameHeight * Projectile.frame;
            Rectangle rectangle = new Rectangle(0, yPos, texture.Width, frameHeight);
            Vector2 origin = rectangle.Size() / 2f;
            float fade = MathF.Sin(currentTimer * MathHelper.Pi / 120f);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), rectangle, Color.White * fade, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), rectangle, Color.Orange * 0.8f * fade, Projectile.rotation, origin, Projectile.scale * 1.5f, SpriteEffects.None, 0);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
            return false;
        }
    }
}
