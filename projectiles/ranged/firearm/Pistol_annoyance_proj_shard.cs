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
	public class Pistol_annoyance_proj_shard : ModProjectile
	{
        private float lifeTimer = 0f;
        private float wavePhase;
        private List<Vector2> trail = new List<Vector2>();
        private const int TRAIL_MAX = 20;
        private const float HOMING_RANGE = 350f;
        private const float HOMING_STRENGTH = 0.045f;
        private const float WAVE_AMP = 4f;
        private const float WAVE_FREQ = 0.25f;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.light = 0.35f;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }
        public override void AI()
        {
            if (lifeTimer == 0f)
            {
                wavePhase = Main.rand.NextFloat(MathHelper.TwoPi);
            }
            lifeTimer++;
            ApplyHoming();
            ApplyWave();
            Projectile.rotation = Projectile.velocity.ToRotation();
            trail.Insert(0, Projectile.Center);
            if (trail.Count > TRAIL_MAX)
                trail.RemoveAt(trail.Count - 1);
            SpawnFlightDust();
            float pulse = (float)Math.Sin(lifeTimer * 0.18f) * 0.1f;
            Projectile.light = 0.35f + pulse;
        }
        private void ApplyHoming()
        {
            NPC closest = null;
            float closestDist = HOMING_RANGE;
            Player owner = Main.player[Projectile.owner];
            if (owner.HasMinionAttackTargetNPC)
            {
                NPC t = Main.npc[owner.MinionAttackTargetNPC];
                if (t.CanBeChasedBy() && Vector2.Distance(Projectile.Center, t.Center) < HOMING_RANGE)
                {
                    closest = t;
                    closestDist = Vector2.Distance(Projectile.Center, t.Center);
                }
            }
            if (closest == null)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.CanBeChasedBy()) continue;
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }
            if (closest != null)
            {
                Vector2 toTarget = (closest.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                float currentAngle = Projectile.velocity.ToRotation();
                float targetAngle = toTarget.ToRotation();
                float angleDiff = MathHelper.WrapAngle(targetAngle - currentAngle);
                if (Math.Abs(angleDiff) > MathHelper.ToRadians(140f))
                    return;
                float distFactor = 1f - MathHelper.Clamp(closestDist / HOMING_RANGE, 0f, 1f);
                float power = HOMING_STRENGTH * (0.5f + distFactor * 0.5f);
                float newAngle = currentAngle + angleDiff * power;
                float speed = Projectile.velocity.Length();
                Projectile.velocity = new Vector2((float)Math.Cos(newAngle), (float)Math.Sin(newAngle)) * speed;
            }
        }
        private void ApplyWave()
        {
            Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Vector2 perp = new Vector2(-forward.Y, forward.X);
            float current = (float)Math.Sin(lifeTimer * WAVE_FREQ + wavePhase) * WAVE_AMP;
            float prev = (float)Math.Sin((lifeTimer - 1f) * WAVE_FREQ + wavePhase) * WAVE_AMP;
            Projectile.position += perp * (current - prev);
        }
        private void SpawnFlightDust()
        {
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(3f, 3f), 0, 0, DustID.Blood, 0, 0, 100, default, 0.6f);
                d.noGravity = true;
                d.velocity *= 0.02f;
            }
            if (Main.rand.NextBool(4))
            {
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.RedTorch, 0, 0, 0, default, 0.5f);
                d.noGravity = true;
                d.velocity *= 0.01f;
            }
            if (Main.rand.NextBool(6))
            {
                Vector2 vel = Main.rand.NextVector2Circular(1f, 1f);
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Blood, vel.X, vel.Y, 80, default, 0.5f);
                d.noGravity = false;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player owner = Main.player[Projectile.owner];
            if (owner.active && !owner.dead)
            {
                owner.Heal(1);
            }
            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(4f, 4f)
                    * Main.rand.NextFloat(0.3f, 1f);
                Dust d = Dust.NewDustDirect(target.Center, 0, 0, DustID.Blood, vel.X, vel.Y, 60, default, 1.2f);
                d.noGravity = true;
                d.fadeIn = 1.4f;
            }
            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);
                Dust d = Dust.NewDustDirect(target.Center, 0, 0, DustID.LifeDrain, vel.X, vel.Y, 0, default, 0.8f);
                d.noGravity = true;
            }
            SoundEngine.PlaySound(SoundID.NPCHit18 with
            {
                Volume = 0.4f
            }, target.Center);
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Blood, vel.X, vel.Y, 60, default, 0.8f);
                d.noGravity = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            DrawShardTrail(pixel);
            DrawShardCore(pixel);
            return false;
        }
        private void DrawShardTrail(Texture2D pixel)
        {
            if (trail.Count < 2) return;
            Color darkCrimson = new Color(60, 0, 0);
            Color deepBlood = new Color(120, 8, 8);
            Color bloodRed = new Color(180, 20, 20);
            Color brightRed = new Color(240, 60, 60);
            Color hotPink = new Color(255, 180, 180);
            DrawTrailLayer(pixel, darkCrimson, 14f, 0.08f);
            DrawTrailLayer(pixel, deepBlood, 10f, 0.15f);
            DrawTrailLayer(pixel, bloodRed, 6f, 0.3f);
            DrawTrailLayer(pixel, brightRed, 3.5f, 0.5f);
            DrawTrailLayer(pixel, hotPink, 1.5f, 0.7f);
        }
        private void DrawTrailLayer(Texture2D pixel, Color baseColor, float width, float alphaBase)
        {
            for (int i = 0; i < trail.Count - 1; i++)
            {
                Vector2 start = trail[i] - Main.screenPosition;
                Vector2 end = trail[i + 1] - Main.screenPosition;
                Vector2 diff = end - start;
                float length = diff.Length();
                if (length < 0.5f) continue;
                float rotation = diff.ToRotation();
                float progress = (float)i / trail.Count;
                float fade = 1f - progress;
                fade = fade * fade;
                float pulse = 1f + (float)Math.Sin(lifeTimer * 0.2f + i * 0.6f) * 0.18f;
                float segWidth = width * (1f - progress * 0.65f) * pulse;
                float alpha = alphaBase * fade;
                Color col = baseColor * alpha;
                col.A = 0;
                Main.EntitySpriteDraw(pixel, start, new Rectangle(0, 0, 1, 1), col, rotation, new Vector2(0f, 0.5f), new Vector2(length, segWidth), SpriteEffects.None, 0);
            }
        }
        private void DrawShardCore(Texture2D pixel)
        {
            Vector2 pos = Projectile.Center - Main.screenPosition;
            float pulse = 1f + (float)Math.Sin(lifeTimer * 0.18f) * 0.15f;
            DrawGlow(pixel, pos, 16f * pulse, new Color(100, 5, 5) * 0.15f);
            DrawGlow(pixel, pos, 10f * pulse, new Color(180, 15, 15) * 0.25f);
            DrawGlow(pixel, pos, 5f * pulse, new Color(230, 40, 40) * 0.5f);
            DrawGlow(pixel, pos, 2.5f * pulse, new Color(255, 150, 150) * 0.7f);
            DrawGlow(pixel, pos, 1.2f * pulse, Color.White * 0.8f);
            float rayLen = 8f * pulse;
            float rayRot = lifeTimer * 0.07f + Projectile.rotation;
            Color rayCol = new Color(200, 25, 25) * 0.25f;
            rayCol.A = 0;
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.PiOver2 * i + rayRot;
                Main.EntitySpriteDraw(pixel, pos, new Rectangle(0, 0, 1, 1), rayCol, angle, new Vector2(0f, 0.5f), new Vector2(rayLen, 1.2f), SpriteEffects.None, 0);
            }
        }
        private void DrawGlow(Texture2D pixel, Vector2 center, float size, Color color)
        {
            color.A = 0;
            Main.EntitySpriteDraw(pixel, center - new Vector2(size * 0.5f), new Rectangle(0, 0, 1, 1), color, 0f, Vector2.Zero, size, SpriteEffects.None, 0);
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(200, 40, 40, 160);
        }
    }
}
