using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using System.Collections.Generic;
namespace jarkos.projectiles.ranged.firearm
{
	public class Glow_projectile : ModProjectile
	{
        private bool IsOverload => Projectile.ai[1] == 1f;
        private float lifeTimer = 0f;
        private List<Vector2> trailPoints = new List<Vector2>();
        private const int TRAIL_LENGTH = 30;
        private const float WAVE_AMPLITUDE = 3.5f;
        private const float WAVE_FREQUENCY = 0.35f;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 150;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.light = 0.4f;
            Projectile.extraUpdates = 3;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void AI()
        {
            lifeTimer++;
            ApplyWaveMotion();
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            trailPoints.Insert(0, Projectile.Center);
            if (trailPoints.Count > TRAIL_LENGTH)
                trailPoints.RemoveAt(trailPoints.Count - 1);
            SpawnFlightDust();
            float pulse = (float)Math.Sin(lifeTimer * 0.2f) * 0.15f;
            Projectile.light = (IsOverload ? 0.7f : 0.4f) + pulse;
        }
        private void ApplyWaveMotion()
        {
            Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Vector2 perp = new Vector2(-forward.Y, forward.X);
            float phase = Projectile.ai[0];
            float waveOffset = (float)Math.Sin(lifeTimer * WAVE_FREQUENCY + phase) * WAVE_AMPLITUDE;
            float prevWaveOffset = (float)Math.Sin((lifeTimer - 1f) * WAVE_FREQUENCY + phase) * WAVE_AMPLITUDE;
            float waveDelta = waveOffset - prevWaveOffset;
            if (IsOverload)
                waveDelta *= 1.5f;
            Projectile.position += perp * waveDelta;
        }
        private void SpawnFlightDust()
        {
            if (IsOverload)
            {
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.GoldFlame, 0, 0, 0, default, 0.8f);
                d.noGravity = true;
                d.velocity *= 0.05f;
                d.fadeIn = 1.1f;
                if (Main.rand.NextBool(2))
                {
                    Dust d2 = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(3f, 3f), 0, 0, DustID.WhiteTorch, 0, 0, 0, default, 0.6f);
                    d2.noGravity = true;
                    d2.velocity *= 0.03f;
                }
                if (Main.rand.NextBool(4))
                {
                    Dust d3 = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.YellowStarDust, 0, 0, 100, default, 0.5f);
                    d3.noGravity = true;
                    d3.velocity *= 0.02f;
                }
            }
            else
            {
                if (Main.rand.NextBool(2))
                {
                    Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.GoldFlame, 0, 0, 60, default, 0.5f);
                    d.noGravity = true;
                    d.velocity *= 0.03f;
                }
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SpawnImpactEffect(target.Center);
            if (IsOverload)
            {
                target.AddBuff(BuffID.Electrified, 120);
            }
        }
        public override void OnKill(int timeLeft)
        {
            SpawnDeathDust();
        }
        private void SpawnImpactEffect(Vector2 pos)
        {
            int count = IsOverload ? 4 : 2;
            for (int i = 0; i < count; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(4f, 4f)
                    * Main.rand.NextFloat(0.3f, 1f);
                Dust d = Dust.NewDustDirect(pos, 0, 0, DustID.GoldFlame, vel.X, vel.Y, 0, default, IsOverload ? 1.5f : 1f);
                d.noGravity = true;
                d.fadeIn = 1.5f;
            }
            if (IsOverload)
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 vel = Main.rand.NextVector2CircularEdge(3f, 3f);
                    Dust d = Dust.NewDustDirect(pos, 0, 0, DustID.WhiteTorch, vel.X, vel.Y, 0, default, 1.3f);
                    d.noGravity = true;
                }
                for (int i = 0; i < 2; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                    Dust d = Dust.NewDustDirect(pos, 0, 0, DustID.YellowStarDust, vel.X, vel.Y, 80, default, 0.9f);
                    d.noGravity = true;
                }
            }
        }
        private void SpawnDeathDust()
        {
            int count = IsOverload ? 10 : 5;
            for (int i = 0; i < count; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.GoldFlame, vel.X, vel.Y, 0, default, 0.8f);
                d.noGravity = true;
                d.fadeIn = 1f;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D magicPixel = TextureAssets.MagicPixel.Value;
            DrawIonTrail(magicPixel);
            return true;
        }
        private void DrawIonTrail(Texture2D pixel)
        {
            if (trailPoints.Count < 2) return;
            Color coreYellow, brightYellow, outerYellow, outerGlow;
            if (IsOverload)
            {
                coreYellow = Color.White;
                brightYellow = new Color(255, 255, 180);
                outerYellow = new Color(255, 220, 50);
                outerGlow = new Color(255, 180, 20);
            }
            else
            {
                coreYellow = new Color(255, 255, 220);
                brightYellow = new Color(255, 240, 100);
                outerYellow = new Color(255, 200, 30);
                outerGlow = new Color(200, 150, 10);
            }
            float baseWidth = IsOverload ? 1.6f : 1f;
            List<Vector2> currentTrail = new List<Vector2>();
            currentTrail.Add(Projectile.Center);
            currentTrail.AddRange(trailPoints);
            DrawTrailSegments(pixel, currentTrail, outerGlow, 14f * baseWidth, 0.12f);
            DrawTrailSegments(pixel, currentTrail, outerYellow, 9f * baseWidth, 0.2f);
            DrawTrailSegments(pixel, currentTrail, brightYellow, 5f * baseWidth, 0.45f);
            DrawTrailSegments(pixel, currentTrail, coreYellow, 2.5f * baseWidth, 0.75f);
            DrawTrailSegments(pixel, currentTrail, Color.White, 1f * baseWidth, 0.9f);
        }
        private void DrawTrailSegments(Texture2D pixel, List<Vector2> currentTrail, Color baseColor, float width, float alphaBase)
        {
            for (int i = 0; i < currentTrail.Count - 1; i++)
            {
                Vector2 start = currentTrail[i] - Main.screenPosition;
                Vector2 end = currentTrail[i + 1] - Main.screenPosition;
                Vector2 diff = end - start;
                float length = diff.Length();
                if (length < 0.5f) continue;
                float rotation = diff.ToRotation();
                float progress = (float)i / currentTrail.Count;
                float fadeAlpha = 1f - progress;
                fadeAlpha = fadeAlpha * fadeAlpha;
                float pulse = 1f + (float)Math.Sin(lifeTimer * 0.3f + i * 0.4f) * 0.12f;
                float segWidth = width * (1f - progress * 0.75f) * pulse;
                float alpha = alphaBase * fadeAlpha;
                Color segColor = baseColor * alpha;
                segColor.A = 0;
                Main.EntitySpriteDraw(pixel, start, new Rectangle(0, 0, 1, 1), segColor, rotation, new Vector2(0f, 0.5f), new Vector2(length, segWidth), SpriteEffects.None, 0);
            }
        }
        public override Color? GetAlpha(Color lightColor)
        {
            if (IsOverload)
                return new Color(255, 255, 200, 180);
            return new Color(255, 230, 100, 180);
        }
    }
}
