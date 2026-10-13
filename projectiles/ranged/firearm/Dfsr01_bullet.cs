using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Shaders;
using System;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Enums;
using Terraria.Graphics.Effects;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.Localization;
using System.Collections.Generic;
namespace jarkos.projectiles.ranged.firearm
{
	public class Dfsr01_bullet : ModProjectile
	{
        private int Mode => (int)Projectile.ai[1];
        private bool IsAuto => Mode == 0;
        private bool IsBurst => Mode == 1;
        private bool IsSingle => Mode == 2;
        private float lifeTimer = 0f;
        private int bounceCount = 0;
        private const int MAX_BOUNCES = 1;
        private List<Vector2> trail = new List<Vector2>();
        private const int TRAIL_MAX = 22;
        private const float WAVE_AMP = 1.8f;
        private const float WAVE_FREQ = 0.22f;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 22;
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
            Projectile.timeLeft = 160;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.light = 0.25f;
            Projectile.extraUpdates = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void AI()
        {
            lifeTimer++;
            if (!IsSingle)
            {
                ApplyWave();
            }
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            trail.Insert(0, Projectile.Center);
            if (trail.Count > TRAIL_MAX)
                trail.RemoveAt(trail.Count - 1);
            SpawnFlightDust();
            float baseLt = IsSingle ? 0.45f : 0.25f;
            float pulse = (float)Math.Sin(lifeTimer * 0.2f) * 0.08f;
            Projectile.light = baseLt + pulse;
        }
        private void ApplyWave()
        {
            Vector2 fwd = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Vector2 perp = new Vector2(-fwd.Y, fwd.X);
            float phase = Projectile.ai[0];
            float amp = IsBurst ? WAVE_AMP * 0.5f : WAVE_AMP;
            float cur = (float)Math.Sin(lifeTimer * WAVE_FREQ + phase) * amp;
            float prev = (float)Math.Sin((lifeTimer - 1f) * WAVE_FREQ + phase) * amp;
            Projectile.position += perp * (cur - prev);
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (IsAuto && bounceCount < MAX_BOUNCES)
            {
                bounceCount++;
                if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
                    Projectile.velocity.X = -oldVelocity.X;
                if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
                    Projectile.velocity.Y = -oldVelocity.Y;
                Projectile.velocity *= 0.85f;
                SpawnRicochetSparks();
                SoundEngine.PlaySound(SoundID.Item10 with
                {
                    Volume = 0.4f, Pitch = 0.3f
                }, Projectile.Center);
                return false;
            }
            SpawnWallHitDust();
            return true;
        }
        private void SpawnRicochetSparks()
        {
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(4f, 4f)
                    * Main.rand.NextFloat(0.3f, 1f);
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Torch, vel.X, vel.Y, 0, default, 1f);
                d.noGravity = true;
                d.fadeIn = 1.2f;
            }
            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.WhiteTorch, vel.X, vel.Y, 0, default, 0.7f);
                d.noGravity = true;
            }
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Projectile.velocity.RotatedByRandom(MathHelper.Pi) * 0.3f
                    + Main.rand.NextVector2Circular(2f, 2f);
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Iron, vel.X, vel.Y, 0, default, 0.6f);
                d.noGravity = false;
            }
        }
        private void SpawnWallHitDust()
        {
            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Smoke, vel.X, vel.Y, 140, new Color(180, 180, 190), 0.8f);
                d.noGravity = true;
            }
            for (int i = 0; i < 3; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Torch, vel.X, vel.Y, 0, default, 0.6f);
                d.noGravity = true;
            }
        }
        private void SpawnFlightDust()
        {
            if (IsSingle)
            {
                Dust d = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(2f, 2f), 0, 0, DustID.WhiteTorch, 0, 0, 0, default, 0.5f);
                d.noGravity = true;
                d.velocity *= 0.03f;
                if (Main.rand.NextBool(2))
                {
                    Dust d2 = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Torch, 0, 0, 80, default, 0.4f);
                    d2.noGravity = true;
                    d2.velocity *= 0.02f;
                }
            }
            else
            {
                if (Main.rand.NextBool(3))
                {
                    Dust d = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(2f, 2f), 0, 0, DustID.Smoke, 0, 0, 140, new Color(180, 180, 190), 0.35f);
                    d.noGravity = true;
                    d.velocity *= 0.02f;
                }
                if (Main.rand.NextBool(5))
                {
                    Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Torch, 0, 0, 100, default, 0.3f);
                    d.noGravity = true;
                    d.velocity *= 0.01f;
                }
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SpawnHitEffect(target.Center);
            if (IsSingle)
            {
                SpawnPowerHitEffect(target.Center);
            }
        }
        private void SpawnHitEffect(Vector2 pos)
        {
            int count = IsSingle ? 14 : 6;
            float scale = IsSingle ? 1.3f : 0.9f;
            for (int i = 0; i < count; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(3f, 3f)
                    * Main.rand.NextFloat(0.3f, 1f);
                Dust d = Dust.NewDustDirect(pos, 0, 0, DustID.Smoke, vel.X, vel.Y, 120, new Color(200, 200, 210), scale);
                d.noGravity = true;
                d.fadeIn = scale + 0.2f;
            }
            for (int i = 0; i < count / 2; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                Dust d = Dust.NewDustDirect(pos, 0, 0, DustID.Torch, vel.X, vel.Y, 0, default, 0.7f);
                d.noGravity = true;
            }
        }
        private void SpawnPowerHitEffect(Vector2 pos)
        {
            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(5f, 5f)
                    * Main.rand.NextFloat(0.4f, 1f);
                Dust d = Dust.NewDustDirect(pos, 0, 0, DustID.WhiteTorch, vel.X, vel.Y, 0, default, 1.4f);
                d.noGravity = true;
                d.fadeIn = 1.6f;
            }
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                vel.Y -= Main.rand.NextFloat(1f, 3f);
                Dust d = Dust.NewDustDirect(pos, 0, 0, DustID.Iron, vel.X, vel.Y, 0, default, 0.7f);
                d.noGravity = false;
            }
            SoundEngine.PlaySound(SoundID.Item62 with
            {
                Volume = 0.3f, Pitch = 0.4f
            }, pos);
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 3; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Smoke, vel.X, vel.Y, 140, new Color(180, 180, 190), 0.5f);
                d.noGravity = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            DrawBulletTrail(pixel);
            DrawBulletCore(pixel);
            return false;
        }
        private void DrawBulletTrail(Texture2D pixel)
        {
            if (trail.Count < 2) return;
            Color steel, bright, mid, outer, faint;
            if (IsSingle)
            {
                steel = Color.White;
                bright = new Color(230, 235, 255);
                mid = new Color(180, 195, 220);
                outer = new Color(120, 140, 180);
                faint = new Color(60, 75, 110);
            }
            else if (IsBurst)
            {
                steel = new Color(240, 245, 255);
                bright = new Color(200, 215, 240);
                mid = new Color(150, 170, 200);
                outer = new Color(100, 120, 160);
                faint = new Color(50, 65, 100);
            }
            else
            {
                steel = new Color(255, 245, 230);
                bright = new Color(220, 210, 190);
                mid = new Color(170, 160, 140);
                outer = new Color(120, 110, 95);
                faint = new Color(70, 60, 50);
            }
            float widthMult = IsSingle ? 1.5f : 1f;
            List<Vector2> currentTrail = new List<Vector2>();
            currentTrail.Add(Projectile.Center);
            currentTrail.AddRange(trail);
            DrawTrailLayer(pixel, currentTrail, faint, 10f * widthMult, 0.08f);
            DrawTrailLayer(pixel, currentTrail, outer, 7f * widthMult, 0.14f);
            DrawTrailLayer(pixel, currentTrail, mid, 4.5f * widthMult, 0.28f);
            DrawTrailLayer(pixel, currentTrail, bright, 2.5f * widthMult, 0.5f);
            DrawTrailLayer(pixel, currentTrail, steel, 1f * widthMult, 0.8f);
        }
        private void DrawTrailLayer(Texture2D pixel, List<Vector2> currentTrail, Color baseColor, float width, float alphaBase)
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
                float fade = 1f - progress;
                fade *= fade;
                float pulse = 1f + (float)Math.Sin(lifeTimer * 0.2f + i * 0.4f) * 0.1f;
                float segWidth = width * (1f - progress * 0.7f) * pulse;
                float alpha = alphaBase * fade;
                Color col = baseColor * alpha;
                col.A = 0;
                Main.EntitySpriteDraw(pixel, start, new Rectangle(0, 0, 1, 1), col, rotation, new Vector2(0f, 0.5f), new Vector2(length, segWidth), SpriteEffects.None, 0);
            }
        }
        private void DrawBulletCore(Texture2D pixel)
        {
            Vector2 pos = Projectile.Center - Main.screenPosition;
            float pulse = 1f + (float)Math.Sin(lifeTimer * 0.2f) * 0.1f;
            float sizeMult = IsSingle ? 1.6f : 1f;
            DrawGlow(pixel, pos, 16f * sizeMult * pulse, new Color(100, 110, 130) * 0.1f);
            DrawGlow(pixel, pos, 10f * sizeMult * pulse, new Color(160, 170, 200) * 0.2f);
            DrawGlow(pixel, pos, 5f * sizeMult * pulse, new Color(210, 220, 240) * 0.45f);
            DrawGlow(pixel, pos, 2.5f * sizeMult * pulse, new Color(240, 245, 255) * 0.7f);
            DrawGlow(pixel, pos, 1.2f * sizeMult * pulse, Color.White * 0.9f);
            DrawDirectionalStreak(pixel, pos, pulse, sizeMult);
        }
        private void DrawDirectionalStreak(Texture2D pixel, Vector2 pos, float pulse, float sizeMult)
        {
            float streakLen = (IsSingle ? 14f : 8f) * pulse;
            float streakWidth = (IsSingle ? 2.5f : 1.5f) * pulse;
            Color streakColor = new Color(220, 225, 240) * 0.4f;
            streakColor.A = 0;
            Main.EntitySpriteDraw(pixel, pos, new Rectangle(0, 0, 1, 1), streakColor, Projectile.rotation, new Vector2(0.5f, 0.5f), new Vector2(streakLen, streakWidth), SpriteEffects.None, 0);
            Color coreStreak = Color.White * 0.3f;
            coreStreak.A = 0;
            Main.EntitySpriteDraw(pixel, pos, new Rectangle(0, 0, 1, 1), coreStreak, Projectile.rotation, new Vector2(0.5f, 0.5f), new Vector2(streakLen * 0.6f, streakWidth * 0.3f), SpriteEffects.None, 0);
            float crossLen = streakLen * 0.4f;
            float crossWidth = streakWidth * 0.6f;
            Color crossColor = new Color(200, 210, 230) * 0.2f;
            crossColor.A = 0;
            Main.EntitySpriteDraw(pixel, pos, new Rectangle(0, 0, 1, 1), crossColor, Projectile.rotation + MathHelper.PiOver2, new Vector2(0.5f, 0.5f), new Vector2(crossLen, crossWidth), SpriteEffects.None, 0);
        }
        private void DrawGlow(Texture2D pixel, Vector2 center, float size, Color color)
        {
            color.A = 0;
            Main.EntitySpriteDraw(pixel, center - new Vector2(size * 0.5f), new Rectangle(0, 0, 1, 1), color, 0f, Vector2.Zero, size, SpriteEffects.None, 0);
        }
        public override Color? GetAlpha(Color lightColor)
        {
            if (IsSingle)
                return new Color(240, 245, 255, 200);
            if (IsBurst)
                return new Color(220, 230, 250, 190);
            return new Color(230, 225, 215, 180);
        }
    }
}
