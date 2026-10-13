using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.items.weapons.melee.other_melee;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using jarkos.particles;
namespace jarkos.projectiles.melee.other_melee
{
    public class Fusion_fist_proj : ModProjectile
    {
        private Vector2 launchDir = Vector2.Zero;
        private Vector2 peakPos = Vector2.Zero;
        private float curveSign = 0f;
        private bool initialized = false;
        private float fadeAlpha = 1f;
        private float timer = 0f;
        private const float launchDuration = 22f;
        private const float pauseDuration = 3f;
        private const float returnDuration = 20f;
        private bool hasImpacted = false;
        private Player Owner => Main.player[Projectile.owner];
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 360;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.alpha = 0;
        }
        public override void AI()
        {
            if (!Owner.active || Owner.dead)
            {
                Projectile.Kill();
                return;
            }
            if (!initialized)
            {
                launchDir = Projectile.velocity;
                if (launchDir == Vector2.Zero)
                {
                    launchDir = (Main.MouseWorld - Owner.Center).SafeNormalize(Vector2.UnitX);
                }
                else
                {
                    launchDir.Normalize();
                }
                Projectile.velocity = Vector2.Zero;
                curveSign = Main.rand.NextBool() ? 1f : -1f;
                initialized = true;
            }
            timer++;
            float targetRange = Projectile.ai[1];
            if (targetRange <= 0f)
                targetRange = 400f;
            Vector2 lastPos = Projectile.Center;
            if (timer <= launchDuration)
            {
                float t = timer / launchDuration;
                float easedT = EaseInOutQuad(t);
                Projectile.Center = Owner.MountedCenter + launchDir * (targetRange * easedT);
                Projectile.rotation = launchDir.ToRotation() + MathHelper.Pi;
                Projectile.scale = MathHelper.Lerp(0.3f, 1.8f, easedT);
                peakPos = Projectile.Center;
                if (Main.rand.NextBool(3))
                {
                    Vector2 pVel = -launchDir * Main.rand.NextFloat(2f, 5f) + Main.rand.NextVector2Circular(1f, 1f);
                    new jarkos.particles.StarParticle(Projectile.Center, pVel, new Color(255, 120, 30), 15, Main.rand.NextFloat(0.05f, 0.1f), 1f).Spawn();
                }
            }
            else if (timer <= launchDuration + pauseDuration)
            {
                Projectile.Center = peakPos;
                Projectile.rotation = launchDir.ToRotation() + MathHelper.Pi;
                Projectile.scale = 1.8f;
            }
            else
            {
                float returnTimer = timer - launchDuration - pauseDuration;
                float t2 = returnTimer / returnDuration;
                if (t2 >= 1f)
                {
                    Projectile.Kill();
                    return;
                }
                float easedT2 = EaseInOutQuad(t2);
                Vector2 basePos = Vector2.Lerp(peakPos, Owner.MountedCenter, easedT2);
                Vector2 perp = launchDir.RotatedBy(MathHelper.PiOver2);
                float arc = (float)Math.Sin(easedT2 * MathHelper.Pi) * 120f * curveSign;
                Projectile.Center = basePos + perp * arc;
                Vector2 moveDelta = Projectile.Center - lastPos;
                if (moveDelta != Vector2.Zero)
                {
                    Projectile.rotation = moveDelta.ToRotation() + MathHelper.Pi;
                }
                Projectile.scale = MathHelper.Lerp(1.8f, 0.2f, easedT2);
                if (Main.rand.NextBool(3))
                {
                    Vector2 pVel = -moveDelta.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(2f, 4f) + Main.rand.NextVector2Circular(1f, 1f);
                    new jarkos.particles.StarParticle(Projectile.Center, pVel, new Color(255, 120, 30), 15, Main.rand.NextFloat(0.05f, 0.1f), 1f).Spawn();
                }
            }
            Lighting.AddLight(Projectile.Center, new Vector3(1.2f, 0.6f, 0.2f) * fadeAlpha);
        }
        private float EaseInOutQuad(float t)
        {
            return t < 0.5f ? 2f * t * t : 1f - (float)Math.Pow(-2f * t + 2f, 2f) / 2f;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 12; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(5f, 5f);
                new jarkos.particles.StarParticle(target.Center, velocity, new Color(255, 140, 40), 20, Main.rand.NextFloat(0.05f, 0.1f), 1f).Spawn();
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (fadeAlpha <= 0f)
                return false;
            DrawChainToPlayer();
            DrawFistWithEffects(lightColor);
            return false;
        }
        private Vector2 CalculateBezierPoint(Vector2 start, Vector2 end, float t)
        {
            return Vector2.Lerp(start, end, t);
        }
        private List<Vector2> GenerateChainPoints(Vector2 start, Vector2 end, int segments)
        {
            List<Vector2> points = new List<Vector2>();
            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                points.Add(CalculateBezierPoint(start, end, t));
            }
            return points;
        }
        private void DrawChainToPlayer()
        {
            Texture2D chainTex = ModContent.Request<Texture2D>("jarkos/projectiles/melee/other_melee/fist_chain").Value;
            Vector2 playerPos = Owner.MountedCenter;
            Vector2 fistPos = Projectile.Center;
            float distance = Vector2.Distance(playerPos, fistPos);
            float chainStep = chainTex.Height / 2;
            int segments = Math.Max((int)(distance / chainStep), 16);
            List<Vector2> chainPoints = GenerateChainPoints(playerPos, fistPos, segments);
            for (int i = 0; i < chainPoints.Count - 1; i++)
            {
                Vector2 currentPoint = chainPoints[i];
                Vector2 nextPoint = chainPoints[i + 1];
                Vector2 segmentDir = nextPoint - currentPoint;
                float rotation = segmentDir.ToRotation() + MathHelper.PiOver2;
                float progress = (float)i / (chainPoints.Count - 1);
                float chainFadeProgress = fadeAlpha;
                float segmentAppearDelay = (1f - progress) * 0.5f;
                float segmentAlpha = MathHelper.Clamp((chainFadeProgress - segmentAppearDelay) / 0.5f, 0f, 1f);
                segmentAlpha = 1f - (1f - segmentAlpha) * (1f - segmentAlpha);
                if (segmentAlpha <= 0f)
                    continue;
                Color baseColor = Color.Lerp(new Color(80, 80, 90), new Color(255, 120, 50), progress * 0.7f);
                float pulse = 0.8f + (float)Math.Sin(Main.GameUpdateCount * 0.1f + i * 0.2f) * 0.2f;
                Color finalColor = Lighting.GetColor((int)(currentPoint.X / 16), (int)(currentPoint.Y / 16), baseColor);
                finalColor *= segmentAlpha;
                float scale = MathHelper.Lerp(0.9f, 1.1f, progress);
                float spawnScale = MathHelper.Lerp(0.3f, 1f, segmentAlpha);
                scale *= spawnScale;
                if (segmentAlpha < 1f && segmentAlpha > 0.1f)
                {
                    Main.EntitySpriteDraw(chainTex, currentPoint - Main.screenPosition, null, Color.White * 0.6f * (1f - segmentAlpha), rotation, new Vector2(chainTex.Width / 2f, chainTex.Height / 2f), scale * 1.3f, SpriteEffects.None, 0);
                }
                if (!hasImpacted && segmentAlpha > 0.5f)
                {
                    Main.EntitySpriteDraw(chainTex, currentPoint - Main.screenPosition, null, Color.OrangeRed * 0.3f * pulse * progress * segmentAlpha, rotation, new Vector2(chainTex.Width / 2f, chainTex.Height / 2f), scale * 1.4f, SpriteEffects.None, 0);
                }
                Main.EntitySpriteDraw(chainTex, currentPoint - Main.screenPosition, null, finalColor * pulse, rotation, new Vector2(chainTex.Width / 2f, chainTex.Height / 2f), scale, SpriteEffects.None, 0);
                if (segmentAlpha < 0.9f && segmentAlpha > 0.2f && Main.rand.NextBool(10))
                {
                    Dust spawnDust = Dust.NewDustDirect(currentPoint - new Vector2(4), 8, 8, DustID.FireworkFountain_Yellow, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), 100, default, 0.8f);
                    spawnDust.noGravity = true;
                    spawnDust.fadeIn = 0.5f;
                }
                if (!hasImpacted && Main.rand.NextBool(80) && progress > 0.3f && segmentAlpha > 0.8f)
                {
                    Dust chainDust = Dust.NewDustDirect(currentPoint - new Vector2(4), 8, 8, DustID.Torch, 0, -1f, 100, default, 0.8f * progress);
                    chainDust.noGravity = true;
                    chainDust.velocity *= 0.5f;
                    chainDust.velocity.Y -= 1f;
                }
            }
            if (!hasImpacted && fadeAlpha > 0.7f)
            {
                for (int i = 0; i < 2; i++)
                {
                    float angle = Main.GameUpdateCount * 0.15f + i * MathHelper.Pi;
                    Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 8f;
                    Dust energy = Dust.NewDustDirect(fistPos + offset - new Vector2(2), 4, 4, DustID.OrangeTorch, 0, 0, 0, default, 1.2f * fadeAlpha);
                    energy.noGravity = true;
                    energy.velocity = offset * 0.1f;
                }
            }
        }
        private void DrawFistWithEffects(Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;
            float spawnScale = MathHelper.Lerp(0.2f, 1f, fadeAlpha);
            float finalScale = Projectile.scale * spawnScale;
            float stretchX = 1f;
            float stretchY = 1f;
            if (timer <= launchDuration)
            {
                float t = timer / launchDuration;
                stretchX = 1f + 0.6f * (float)Math.Sin(t * MathHelper.Pi);
                stretchY = 1f - 0.2f * (float)Math.Sin(t * MathHelper.Pi);
            }
            else if (timer > launchDuration + pauseDuration)
            {
                float returnTimer = timer - launchDuration - pauseDuration;
                float t2 = returnTimer / returnDuration;
                if (t2 < 1f)
                {
                    stretchX = 1f + 0.4f * (float)Math.Sin(t2 * MathHelper.Pi);
                    stretchY = 1f - 0.15f * (float)Math.Sin(t2 * MathHelper.Pi);
                }
            }
            Vector2 drawScale = new Vector2(finalScale * stretchX, finalScale * stretchY);
            bool facingLeft = (float)Math.Cos(Projectile.rotation) > 0f;
            SpriteEffects effects = facingLeft ? SpriteEffects.FlipVertically : SpriteEffects.None;
            if (fadeAlpha < 1f && fadeAlpha > 0.1f)
            {
                float materializeGlow = (1f - fadeAlpha) * 0.8f;
                for (int i = 0; i < 4; i++)
                {
                    float angle = MathHelper.PiOver2 * i + Main.GameUpdateCount * 0.1f;
                    Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (15f * (1f - fadeAlpha));
                    Main.EntitySpriteDraw(texture, Projectile.Center + offset - Main.screenPosition, null, Color.White * materializeGlow * 0.5f, Projectile.rotation, origin, drawScale * 1.3f, effects, 0);
                }
                Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Color.Orange * materializeGlow, Projectile.rotation, origin, drawScale * 1.5f, effects, 0);
            }
            if (!hasImpacted && fadeAlpha > 0.6f)
            {
                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    if (Projectile.oldPos[i] == Vector2.Zero) continue;
                    float progress = (float)i / Projectile.oldPos.Length;
                    float alpha = (1f - progress) * 0.6f * fadeAlpha;
                    float scaleVal = finalScale * (1f - progress * 0.4f);
                    Vector2 trailScale = new Vector2(scaleVal * stretchX, scaleVal * stretchY);
                    Color trailColor = Color.Lerp(Color.Orange, Color.DarkRed, progress) * alpha;
                    bool trailFacingLeft = (float)Math.Cos(Projectile.oldRot[i]) > 0f;
                    SpriteEffects trailEffects = trailFacingLeft ? SpriteEffects.FlipVertically : SpriteEffects.None;
                    Main.EntitySpriteDraw(texture, Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition, null, trailColor, Projectile.oldRot[i], origin, trailScale, trailEffects, 0);
                }
            }
            if (!hasImpacted && fadeAlpha > 0.4f)
            {
                float glowPulse = 0.8f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.2f;
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi / 6 * i + Main.GameUpdateCount * 0.05f;
                    Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 6f;
                    Main.EntitySpriteDraw(texture, Projectile.Center + offset - Main.screenPosition, null, Color.OrangeRed * 0.3f * glowPulse * fadeAlpha, Projectile.rotation, origin, drawScale * 1.15f, effects, 0);
                }
            }
            Color drawColor = hasImpacted
                ? lightColor
                : Color.Lerp(lightColor, Color.Orange, 0.3f);
            drawColor *= fadeAlpha;
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, drawColor, Projectile.rotation, origin, drawScale, effects, 0);
            if (!hasImpacted && fadeAlpha > 0.5f)
            {
                Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Color.White * 0.4f * fadeAlpha, Projectile.rotation, origin, drawScale * 0.6f, effects, 0);
            }
        }
    }
}
