using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.summon.whip
{
	public class Diffusal_phantom_blade : ModProjectile
	{
        private const int TOTAL_LIFETIME = 240;
        private const int LUNGE_DURATION = 18;
        private const int ORBIT_DURATION = 40;
        private const int TRAIL_LENGTH = 20;
        private const float ORBIT_RADIUS = 120f;
        private const float ORBIT_SPEED = 0.12f;
        private const float LUNGE_SPEED = 22f;
        private const float SPAWN_FADE_TICKS = 15f;
        private ref float Timer => ref Projectile.localAI[0];
        private ref float OrbitAngle => ref Projectile.localAI[2];
        private Vector2[] trailPositions = new Vector2[TRAIL_LENGTH];
        private float[] trailRotations = new float[TRAIL_LENGTH];
        private bool trailInitialized = false;
        private bool IsRedBlade => Projectile.ai[1] == 0f;
        private bool IsBlueBlade => Projectile.ai[1] == 1f;
        private NPC TargetNPC
        {
            get
            {
                int index = (int)Projectile.ai[0];
                if (index >= 0 && index < Main.maxNPCs && Main.npc[index].active)
                    return Main.npc[index];
                return null;
            }
        }
        private Color PrimaryColor => IsRedBlade ? new Color(255, 60, 30)
            : new Color(40, 160, 255);
        private Color SecondaryColor => IsRedBlade ? new Color(255, 150, 50)
            : new Color(180, 220, 255);
        private Color GlowColor => IsRedBlade ? new Color(255, 80, 20, 0)
            : new Color(60, 180, 255, 0);
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = TRAIL_LENGTH;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 52;
            Projectile.height = 52;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = TOTAL_LIFETIME;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.alpha = 255;
        }
        public override void AI()
        {
            NPC target = TargetNPC;
            if (target == null || !target.active || target.life <= 0)
            {
                DissipateEffect();
                Projectile.Kill();
                return;
            }
            Timer++;
            if (Timer <= SPAWN_FADE_TICKS * 2)
            {
                Projectile.alpha = (int)(255 * (1f - Timer / (SPAWN_FADE_TICKS * 2)));
            }
            else
            {
                Projectile.alpha = 0;
            }
            UpdatePhaseAndMovement(target);
            if (Projectile.velocity.LengthSquared() > 1f)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            }
            SpawnAmbientDust(target);
            UpdateTrail();
            Lighting.AddLight(Projectile.Center, PrimaryColor.ToVector3() * 0.6f);
        }
        private void UpdatePhaseAndMovement(NPC target)
        {
            int cycleLength = (LUNGE_DURATION + ORBIT_DURATION) * 2;
            int lungeDuration = LUNGE_DURATION * 2;
            int orbitDuration = ORBIT_DURATION * 2;
            int timeInCycle = (int)Timer % cycleLength;
            if (timeInCycle < lungeDuration)
            {
                PerformLunge(target, timeInCycle, lungeDuration);
            }
            else
            {
                int orbitTime = timeInCycle - lungeDuration;
                PerformOrbit(target, orbitTime, orbitDuration);
            }
        }
        private void PerformLunge(NPC target, int timeInPhase, int phaseDuration)
        {
            float progress = (float)timeInPhase / phaseDuration;
            if (timeInPhase <= 2)
            {
                SoundStyle lungeSound = IsRedBlade ? SoundID.Item74 with { Volume = 0.5f, Pitch = 0.6f } : SoundID.Item74 with { Volume = 0.5f, Pitch = 0.2f };
                SoundEngine.PlaySound(lungeSound, Projectile.Center);
                Vector2 direction = target.Center - Projectile.Center;
                if (direction.LengthSquared() > 0)
                {
                    direction.Normalize();
                }
                float angleOffset = IsRedBlade ? -0.15f : 0.15f;
                direction = direction.RotatedBy(angleOffset);
                Projectile.velocity = direction * LUNGE_SPEED;
            }
            if (progress < 0.6f)
            {
                Vector2 toTarget = target.Center - Projectile.Center;
                if (toTarget.LengthSquared() > 0)
                {
                    toTarget.Normalize();
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * LUNGE_SPEED, 0.08f);
                }
            }
            if (Main.rand.NextBool(2))
            {
                SpawnLungeDust();
            }
        }
        private void PerformOrbit(NPC target, int timeInPhase, int phaseDuration)
        {
            float progress = (float)timeInPhase / phaseDuration;
            float baseAngle = IsRedBlade ? 0f : MathHelper.Pi;
            OrbitAngle += ORBIT_SPEED;
            float currentAngle = baseAngle + OrbitAngle;
            float dynamicRadius = ORBIT_RADIUS + MathF.Sin(OrbitAngle * 2f) * 30f;

            Vector2 orbitTarget = target.Center + new Vector2(MathF.Cos(currentAngle) * dynamicRadius, MathF.Sin(currentAngle) * dynamicRadius * 0.6f);
            Vector2 toOrbit = orbitTarget - Projectile.Center;
            float distance = toOrbit.Length();

            if (distance > 2f)
            {
                toOrbit.Normalize();
                float speed = MathHelper.Clamp(distance * 0.15f, 2f, 14f);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toOrbit * speed, 0.2f);
            }
            Projectile.velocity *= 0.96f;

            if (progress > 0.7f)
            {
                float chargeUp = (progress - 0.7f) / 0.3f;
                Lighting.AddLight(Projectile.Center, PrimaryColor.ToVector3() * chargeUp);
                if (Main.rand.NextBool(3))
                {
                    Vector2 dustVel = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 2f;
                    int dustType = IsRedBlade ? DustID.Torch : DustID.BlueTorch;
                    Dust d = Dust.NewDustDirect(Projectile.Center, 1, 1, dustType);
                    d.velocity = dustVel + Main.rand.NextVector2Circular(1f, 1f);
                    d.scale = 0.2f * 0.3f;
                    d.noGravity = true;
                }
            }
        }
        private void SpawnAmbientDust(NPC target)
        {
            if (!Main.rand.NextBool(3)) return;
            int dustType = IsRedBlade ? DustID.Torch : DustID.BlueTorch;
            Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, dustType);
            dust.velocity = Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1.5f, 1.5f);
            dust.scale = Main.rand.NextFloat(0.4f, 0.6f);
            dust.noGravity = true;
            dust.alpha = 80;
            if (IsRedBlade && Main.rand.NextBool(4))
            {
                Dust ash = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(12, 12), 1, 1, DustID.Smoke);
                ash.velocity = new Vector2(0, -1f) + Main.rand.NextVector2Circular(0.5f, 0.5f);
                ash.scale = 0.5f;
                ash.alpha = 150;
                ash.noGravity = true;
            }
            if (IsBlueBlade && Main.rand.NextBool(4))
            {
                Dust Spark = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(8, 8), 1, 1, DustID.BlueTorch);
                Spark.velocity = Main.rand.NextVector2Circular(4f, 4f);
                Spark.scale = Main.rand.NextFloat(0.3f, 0.5f);
                Spark.noGravity = true;
                Spark.fadeIn = 0.8f;
            }
        }
        private void SpawnLungeDust()
        {
            int dustType = IsRedBlade ? DustID.Torch : DustID.BlueTorch;
            for (int i = 0; i < 2; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.Center - Projectile.velocity * 2, 1, 1, dustType);
                dust.velocity = -Projectile.velocity * Main.rand.NextFloat(0.1f, 0.3f)
                    + Main.rand.NextVector2Circular(2f, 2f);
                dust.scale = Main.rand.NextFloat(0.4f, 0.6f);
                dust.noGravity = true;
                dust.alpha = 40;
            }
        }
        private void DissipateEffect()
        {
            if (Main.dedServ) return;
            int dustType = IsRedBlade ? DustID.Torch : DustID.Electric;
            for (int i = 0; i < 25; i++)
            {
                float angle = MathHelper.TwoPi * i / 25f;
                Vector2 vel = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * Main.rand.NextFloat(2f, 6f);
                Dust dust = Dust.NewDustDirect(Projectile.Center, 1, 1, dustType);
                dust.velocity = vel;
                dust.scale = Main.rand.NextFloat(0.5f, 0.8f);
                dust.noGravity = true;
                dust.alpha = 50;
                dust.fadeIn = 1.3f;
            }
            int extraDust = IsRedBlade ? DustID.Smoke : DustID.FireworksRGB;
            for (int i = 0; i < 12; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(20, 20), 1, 1, extraDust);
                d.velocity = Main.rand.NextVector2Circular(3f, 3f);
                d.scale = Main.rand.NextFloat(0.5f, 0.6f);
                d.noGravity = true;
                d.alpha = 100;
                if (IsBlueBlade)
                    d.color = new Color(60, 180, 255);
            }
        }
        private void UpdateTrail()
        {
            if (!trailInitialized)
            {
                for (int i = 0; i < TRAIL_LENGTH; i++)
                {
                    trailPositions[i] = Projectile.Center;
                    trailRotations[i] = Projectile.rotation;
                }
                trailInitialized = true;
            }
            for (int i = TRAIL_LENGTH - 1; i > 0; i--)
            {
                trailPositions[i] = trailPositions[i - 1];
                trailRotations[i] = trailRotations[i - 1];
            }
            trailPositions[0] = Projectile.Center;
            trailRotations[0] = Projectile.rotation;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.alpha >= 255) return false;
            SpriteBatch sb = Main.spriteBatch;
            float opacity = 1f - Projectile.alpha / 255f;

            DrawTrail(sb, opacity);
            DrawGlow(sb, opacity);
            DrawBlade(sb, lightColor, opacity);
            return false;
        }
        private Vector2 GetCatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float tension = 0.2f;
            return
                p1 * (2f * t * t * t - 3f * t * t + 1f) + (p2 - p0) * tension * (t * t * t - 2f * t * t + t) + p2 * (-2f * t * t * t + 3f * t * t) + (p3 - p1) * tension * (t * t * t - t * t);
        }
        private void DrawTrail(SpriteBatch sb, float opacity)
        {
            if (HealShaders.Stream == null)
            {
                return;
            }
            Effect effect = HealShaders.Stream;
            sb.End();

            effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 2f);
            effect.Parameters["uOpacity"]?.SetValue(opacity);
            float colorCycle = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 4f + Projectile.whoAmI) * 0.5f + 0.5f;
            Color outerColor = Color.Lerp(PrimaryColor, SecondaryColor, colorCycle);
            Color innerColor = Color.Lerp(SecondaryColor, Color.White, colorCycle);
            effect.Parameters["uColor"]?.SetValue(outerColor.ToVector4());
            effect.Parameters["uColorCore"]?.SetValue(innerColor.ToVector4());
            var rawPoints = new List<Vector2>();
            for (int i = 0; i < TRAIL_LENGTH; i++)
            {
                if (trailPositions[i] != Vector2.Zero)
                {
                    Vector2 nextPos = trailPositions[i];
                    if (rawPoints.Count == 0 || Vector2.DistanceSquared(rawPoints[rawPoints.Count - 1], nextPos) > 16f)
                    {
                        rawPoints.Add(nextPos);
                    }
                }
            }
            var smoothPoints = new List<Vector2>();
            if (rawPoints.Count >= 2)
            {
                int subdivisions = 5;
                for (int i = 0; i < rawPoints.Count - 1; i++)
                {
                    Vector2 p1 = rawPoints[i];
                    Vector2 p2 = rawPoints[i + 1];
                    Vector2 p0 = i > 0 ? rawPoints[i - 1] : p1 + (p1 - p2);
                    Vector2 p3 = i < rawPoints.Count - 2 ? rawPoints[i + 2] : p2 + (p2 - p1);
                    for (int j = 0; j < subdivisions; j++)
                    {
                        float t = (float)j / subdivisions;
                        Vector2 pt = GetCatmullRom(p0, p1, p2, p3, t);
                        if (smoothPoints.Count == 0 || Vector2.DistanceSquared(smoothPoints[smoothPoints.Count - 1], pt) > 0.25f)
                        {
                            smoothPoints.Add(pt);
                        }
                    }
                }
                Vector2 lastPt = rawPoints[rawPoints.Count - 1];
                if (smoothPoints.Count == 0 || Vector2.DistanceSquared(smoothPoints[smoothPoints.Count - 1], lastPt) > 0.25f)
                {
                    smoothPoints.Add(lastPt);
                }
            }
            TrailRenderer.Draw(effect, smoothPoints, t => 20f * MathF.Pow(1f - t, 1.2f) + 0.5f, t => Color.White * (1f - t), HealShaders.HealNoise);
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        private void DrawGlow(SpriteBatch sb, float opacity)
        {
            Texture2D magicPixel = TextureAssets.MagicPixel.Value;
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            float glowPulse = 1f + MathF.Sin(Timer * 0.1f) * 0.3f;
            float glowSize = 50f * glowPulse * opacity;
            Color glowCol = GlowColor * 0.25f * opacity;
            for (int layer = 0; layer < 4; layer++)
            {
                float layerScale = glowSize * (1f + layer * 0.4f);
                float layerAlpha = 1f / (1f + layer * 1.5f);
                sb.Draw(magicPixel, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), glowCol * layerAlpha, MathHelper.PiOver4, new Vector2(0.5f, 0.5f), new Vector2(layerScale, layerScale), SpriteEffects.None, 0f);
            }
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        private void DrawBlade(SpriteBatch sb, Color lightColor, float opacity)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Rectangle frame = texture.Bounds;
            Vector2 origin = frame.Size() / 2f;
            Color drawColor = Color.Lerp(lightColor, PrimaryColor, 0.4f) * opacity;
            sb.Draw(texture, Projectile.Center - Main.screenPosition, frame, drawColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            Color addColor = PrimaryColor * 0.5f * opacity;
            sb.Draw(texture, Projectile.Center - Main.screenPosition, frame, addColor, Projectile.rotation, origin, Projectile.scale * 1.05f, SpriteEffects.None, 0f);
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundStyle hitSound = IsRedBlade ? SoundID.DD2_BetsyFlameBreath with { Volume = 0.3f, Pitch = 0.8f } : SoundID.DD2_BetsyFlameBreath with { Volume = 0.4f, Pitch = 0.3f };
            SoundEngine.PlaySound(hitSound, target.Center);
            int dustType = IsRedBlade ? DustID.Torch : DustID.BlueTorch;
            for (int i = 0; i < 10; i++)
            {
                Dust d = Dust.NewDustDirect(target.position, target.width, target.height, dustType);
                d.velocity = Main.rand.NextVector2Circular(5f, 5f);
                d.scale = Main.rand.NextFloat(0.4f, 0.6f);
                d.noGravity = true;
            }
        }
        public override bool? CanHitNPC(NPC target)
        {
            NPC myTarget = TargetNPC;
            if (myTarget == null) return false;
            if (target.whoAmI == myTarget.whoAmI)
                return null;
            float dist = Vector2.Distance(target.Center, myTarget.Center);
            if (dist < 80f)
                return null;
            return false;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return false;
        }
	}
}
