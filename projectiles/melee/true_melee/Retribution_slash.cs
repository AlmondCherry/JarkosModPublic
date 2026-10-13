using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.ID;
using Terraria.Audio;
using Terraria.GameContent;
using jarkos.items.weapons.melee.true_melee;
using System.Collections.Generic;
using Terraria.UI;
using jarkos.buffs.debuff;
namespace jarkos.projectiles.melee.true_melee
{
  public class Retribution_slash : ModProjectile
  {
        public override string Texture => "jarkos/textures/MagicSphereProj";

        private enum ThreadState
        {
            Seeking, Attached, Returning
        }
        private ThreadState State
        {
            get => (ThreadState)Projectile.ai[1];
            set => Projectile.ai[1] = (float)value;
        }
        private int TargetNPC
        {
            get => (int)Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }
        private float AttachTimer
        {
            get => Projectile.localAI[1];
            set => Projectile.localAI[1] = value;
        }
        public bool IsAttached => State == ThreadState.Attached;
        public void ForceDetach()
        {
            if (State == ThreadState.Attached)
            {
                if (TargetNPC >= 0 && TargetNPC < Main.maxNPCs && Main.npc[TargetNPC].active)
                {
                    FinalBurst(Main.npc[TargetNPC]);
                }
                State = ThreadState.Returning;
            }
        }
        private List<Vector2> ropePoints = new List<Vector2>();
        private List<Vector2> previousRopePoints = new List<Vector2>();
        private const int RopeSegments = 20;
        private bool ropeInitialized = false;
        private float currentSag = 50f;
        private float targetSag = 50f;
        private float previousDistance = 0f;
        private float ropeSwing = 0f;
        private float smoothedTension = 0f;
        private const float MaxRopeLength = 700f;
        private const float MinSag = 8f;
        private const float MaxSag = 100f;
        private const float BreakDistance = 800f;
        private const float SagSmoothness = 0.06f;
        private const float TensionSmoothness = 0.1f;
        private const float SearchRange = 600f;
        private const float LifestealAmount = 0.15f;
        private const float MaxAttachTime = 300f;
        private const int DrainInterval = 60;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = DrainInterval;
            TargetNPC = -1;
            ropeInitialized = false;
        }
        private void InitializeRopeAtCurrentPosition(Player owner)
        {
            ropePoints.Clear();
            previousRopePoints.Clear();
            Vector2 start = owner.Center;
            Vector2 end = Projectile.Center;
            for (int i = 0; i <= RopeSegments; i++)
            {
                float t = i / (float)RopeSegments;
                Vector2 point = Vector2.Lerp(start, end, t);
                ropePoints.Add(point);
                previousRopePoints.Add(point);
            }
            previousDistance = Vector2.Distance(start, end);
            ropeInitialized = true;
        }
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }
            if (!ropeInitialized)
            {
                InitializeRopeAtCurrentPosition(owner);
            }
            Lighting.AddLight(Projectile.Center, 1.0f, 0.2f, 0.2f);
            switch (State)
            {
                case ThreadState.Seeking:
                    SeekingBehavior(owner);
                    break;
                case ThreadState.Attached:
                    AttachedBehavior(owner);
                    break;
                case ThreadState.Returning:
                    ReturningBehavior(owner);
                    break;
            }
            UpdateRopePhysics(owner);
            SpawnParticles();
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }
        private void SeekingBehavior(Player owner)
        {
            int closestNPC = -1;
            float closestDist = SearchRange;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.CanBeChasedBy() && !npc.dontTakeDamage)
                {
                    if (IsNPCAlreadyAttached(i))
                        continue;
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestNPC = i;
                    }
                }
            }
            if (closestNPC != -1)
            {
                NPC target = Main.npc[closestNPC];
                Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * 16f, 0.15f);
                if (Projectile.Hitbox.Intersects(target.Hitbox))
                {
                    AttachToEnemy(closestNPC);
                }
            }
            else
            {
                Projectile.velocity *= 0.98f;
                if (Projectile.velocity.Length() < 2f)
                {
                    State = ThreadState.Returning;
                }
            }
            if (Vector2.Distance(Projectile.Center, owner.Center) > 800f)
            {
                State = ThreadState.Returning;
            }
        }
        private bool IsNPCAlreadyAttached(int npcIndex)
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.type == Projectile.type && proj.whoAmI != Projectile.whoAmI && proj.owner == Projectile.owner)
                {
                    Retribution_slash threadProj = proj.ModProjectile as Retribution_slash;
                    if (threadProj != null && threadProj.State == ThreadState.Attached && threadProj.TargetNPC == npcIndex)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private void AttachToEnemy(int npcIndex)
        {
            if (IsNPCAlreadyAttached(npcIndex))
            {
                State = ThreadState.Returning;
                return;
            }
            State = ThreadState.Attached;
            TargetNPC = npcIndex;
            AttachTimer = 0;
            Projectile.velocity = Vector2.Zero;
            Player owner = Main.player[Projectile.owner];
            previousDistance = Vector2.Distance(owner.Center, Projectile.Center);
            currentSag = 50f;
            targetSag = 50f;
            ropeSwing = 0f;
            smoothedTension = 0f;
            SoundEngine.PlaySound(SoundID.NPCHit18 with { Pitch = 0.5f }, Projectile.Center);
            for (int i = 0; i < 15; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(5, 5);
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Blood, dustVel.X, dustVel.Y);
                dust.scale = Main.rand.NextFloat(1.5f, 2.5f);
                dust.noGravity = true;
            }
        }
        private void AttachedBehavior(Player owner)
        {
            if (TargetNPC < 0 || TargetNPC >= Main.maxNPCs)
            {
                State = ThreadState.Returning;
                return;
            }
            NPC target = Main.npc[TargetNPC];
            if (!target.active || target.dontTakeDamage)
            {
                State = ThreadState.Returning;
                return;
            }
            Vector2 offset = new Vector2(0, -10).RotatedBy(AttachTimer * 0.03f);
            Projectile.Center = target.Center + offset;
            float currentDistance = Vector2.Distance(owner.Center, Projectile.Center);
            float distanceRatio = MathHelper.Clamp(currentDistance / MaxRopeLength, 0f, 1f);
            float distanceDelta = currentDistance - previousDistance;
            previousDistance = currentDistance;
            if (distanceRatio > 0.85f)
            {
                targetSag = MinSag;
                if (Main.rand.NextBool(8))
                    SpawnTensionSparks();
            }
            else if (distanceRatio < 0.25f)
            {
                targetSag = MaxSag;
            }
            else
            {
                float normalizedRatio = (distanceRatio - 0.25f) / 0.6f;
                targetSag = MathHelper.Lerp(MaxSag, MinSag, normalizedRatio);
            }
            if (Math.Abs(distanceDelta) > 1f)
            {
                if (distanceDelta > 0)
                {
                    targetSag *= 0.8f;
                    ropeSwing += distanceDelta * 0.015f;
                }
                else
                {
                    targetSag *= 1.2f;
                    ropeSwing -= distanceDelta * 0.008f;
                }
            }
            targetSag = MathHelper.Clamp(targetSag, MinSag, MaxSag);
            currentSag = MathHelper.Lerp(currentSag, targetSag, SagSmoothness);
            ropeSwing *= 0.94f;
            ropeSwing = MathHelper.Clamp(ropeSwing, -2f, 2f);
            float targetTension = 1f - (currentSag - MinSag) / (MaxSag - MinSag);
            smoothedTension = MathHelper.Lerp(smoothedTension, targetTension, TensionSmoothness);
            if (currentDistance > BreakDistance)
            {
                BreakThread();
                return;
            }
            AttachTimer++;
            if (AttachTimer % DrainInterval == 0)
            {
                DrainLife(owner, target);
            }
            if (AttachTimer % 30 == 0)
            {
                PulseEffect(target);
            }
            if (AttachTimer >= MaxAttachTime)
            {
                State = ThreadState.Returning;
                FinalBurst(target);
            }
            target.AddBuff(ModContent.BuffType<Predict_buff>(), 60);
        }
        private void SpawnTensionSparks()
        {
            if (ropePoints.Count < 2) return;
            int randomIndex = Main.rand.Next(1, ropePoints.Count - 1);
            Vector2 sparkPos = ropePoints[randomIndex];
            for (int i = 0; i < 2; i++)
            {
                Dust Spark = Dust.NewDustDirect(sparkPos, 0, 0, DustID.RedTorch);
                Spark.velocity = Main.rand.NextVector2Circular(2, 2);
                Spark.scale = Main.rand.NextFloat(0.6f, 1.0f);
                Spark.noGravity = true;
                Spark.fadeIn = 0.5f;
            }
        }
        private void BreakThread()
        {
            SoundEngine.PlaySound(SoundID.Item56 with { Pitch = 0.8f }, Projectile.Center);
            foreach (Vector2 point in ropePoints)
            {
                if (Main.rand.NextBool(3))
                {
                    Dust dust = Dust.NewDustDirect(point, 0, 0, DustID.Blood);
                    dust.velocity = Main.rand.NextVector2Circular(5, 5);
                    dust.scale = Main.rand.NextFloat(1.2f, 1.8f);
                    dust.noGravity = true;
                }
            }
            Projectile.Kill();
        }
        private void UpdateRopePhysics(Player owner)
        {
            Vector2 start = owner.Center;
            Vector2 end = Projectile.Center;
            float distance = Vector2.Distance(start, end);
            if (distance < 1f)
            {
                for (int i = 0; i <= RopeSegments; i++)
                {
                    if (i < ropePoints.Count)
                    {
                        ropePoints[i] = start;
                        previousRopePoints[i] = start;
                    }
                }
                return;
            }
            if (ropePoints.Count != RopeSegments + 1 || previousRopePoints.Count != RopeSegments + 1)
            {
                InitializeRopeAtCurrentPosition(owner);
                return;
            }
            bool needsReset = false;
            float maxAllowedDeviation = distance + 200f;
            for (int i = 0; i < ropePoints.Count; i++)
            {
                float t = i / (float)RopeSegments;
                Vector2 expectedPos = Vector2.Lerp(start, end, t);
                float deviation = Vector2.Distance(ropePoints[i], expectedPos);
                if (deviation > maxAllowedDeviation)
                {
                    needsReset = true;
                    break;
                }
            }
            if (needsReset)
            {
                for (int i = 0; i <= RopeSegments; i++)
                {
                    float t = i / (float)RopeSegments;
                    Vector2 point = Vector2.Lerp(start, end, t);
                    ropePoints[i] = point;
                    previousRopePoints[i] = point;
                }
                return;
            }
            if (State == ThreadState.Attached)
            {
                UpdateAttachedRope(start, end, distance);
            }
            else
            {
                UpdateSimpleRope(start, end, distance);
            }
        }
        private void UpdateSimpleRope(Vector2 start, Vector2 end, float distance)
        {
            Vector2 direction = (end - start).SafeNormalize(Vector2.UnitX);
            Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
            float simpleSag = Math.Min(distance * 0.08f, 25f);
            for (int i = 0; i <= RopeSegments; i++)
            {
                previousRopePoints[i] = ropePoints[i];
                float t = i / (float)RopeSegments;
                Vector2 basePos = Vector2.Lerp(start, end, t);
                float sagCurve = (float)Math.Sin(t * MathHelper.Pi);
                Vector2 sagOffset = new Vector2(0, sagCurve * simpleSag);
                float wave = (float)Math.Sin(t * MathHelper.Pi * 2 + Main.GameUpdateCount * 0.1f) * 3f;
                Vector2 waveOffset = perpendicular * wave * sagCurve;
                Vector2 targetPos = basePos + sagOffset + waveOffset;
                ropePoints[i] = Vector2.Lerp(ropePoints[i], targetPos, 0.5f);
            }
            ropePoints[0] = start;
            ropePoints[RopeSegments] = end;
        }
        private void UpdateAttachedRope(Vector2 start, Vector2 end, float distance)
        {
            Vector2 direction = (end - start).SafeNormalize(Vector2.UnitX);
            Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
            for (int i = 0; i <= RopeSegments; i++)
            {
                previousRopePoints[i] = ropePoints[i];
                float t = i / (float)RopeSegments;
                Vector2 basePos = Vector2.Lerp(start, end, t);
                float sagCurve = (float)Math.Sin(t * MathHelper.Pi);
                float sagAmount = sagCurve * currentSag;
                Vector2 gravityOffset = new Vector2(0, sagAmount);
                float waveFrequency = 2f + smoothedTension;
                float waveAmplitude = (1f - smoothedTension) * 6f;
                float wave = (float)Math.Sin(t * MathHelper.Pi * waveFrequency + Main.GameUpdateCount * 0.06f);
                Vector2 waveOffset = perpendicular * wave * waveAmplitude * sagCurve;
                float swingPhase = (float)Math.Sin(Main.GameUpdateCount * 0.04f + ropeSwing);
                float swingAmount = ropeSwing * 15f * sagCurve;
                Vector2 swingOffset = perpendicular * swingPhase * swingAmount;
                Vector2 targetPos = basePos + gravityOffset + waveOffset + swingOffset;
                ropePoints[i] = Vector2.Lerp(ropePoints[i], targetPos, 0.15f);
            }
            ropePoints[0] = start;
            ropePoints[RopeSegments] = end;
            float segmentLength = distance / RopeSegments * 1.1f;
            for (int iteration = 0; iteration < 2; iteration++)
            {
                for (int i = 0; i < RopeSegments; i++)
                {
                    Vector2 delta = ropePoints[i + 1] - ropePoints[i];
                    float currentLength = delta.Length();
                    if (currentLength > segmentLength && currentLength > 0.1f)
                    {
                        float correction = (currentLength - segmentLength) / currentLength * 0.5f;
                        if (i > 0 && i < RopeSegments - 1)
                        {
                            ropePoints[i] += delta * correction;
                            ropePoints[i + 1] -= delta * correction;
                        }
                        else if (i == 0)
                        {
                            ropePoints[i + 1] -= delta * correction * 2f;
                        }
                        else
                        {
                            ropePoints[i] += delta * correction * 2f;
                        }
                    }
                }
                ropePoints[0] = start;
                ropePoints[RopeSegments] = end;
            }
        }
        private void DrainLife(Player owner, NPC target)
        {
            int damage = Projectile.damage / 3;
            target.SimpleStrikeNPC(damage, 0, false, 0, DamageClass.Magic, false, owner.luck);
            int healAmount = (int)(damage * LifestealAmount);
            if (healAmount > 0)
            {
                owner.Heal(healAmount);
                for (int i = ropePoints.Count - 1; i >= 0; i -= 2)
                {
                    if (i >= ropePoints.Count) continue;
                    Vector2 dustPos = ropePoints[i] + Main.rand.NextVector2Circular(3, 3);
                    Dust dust = Dust.NewDustDirect(dustPos, 0, 0, DustID.LifeDrain);
                    if (i > 0)
                    {
                        dust.velocity = (ropePoints[i - 1] - ropePoints[i]).SafeNormalize(Vector2.Zero) * 5f;
                    }
                    else
                    {
                        dust.velocity = (owner.Center - dustPos).SafeNormalize(Vector2.Zero) * 5f;
                    }
                    dust.scale = 1.3f;
                    dust.noGravity = true;
                    dust.fadeIn = 1.0f;
                }
            }
            if (Main.rand.NextBool(4))
            {
                SoundEngine.PlaySound(SoundID.Item3 with { Volume = 0.25f, Pitch = -0.5f }, Projectile.Center);
            }
        }
        private void PulseEffect(NPC target)
        {
            for (int i = 0; i < 360; i += 30)
            {
                Vector2 dir = new Vector2(1, 0).RotatedBy(MathHelper.ToRadians(i));
                Dust dust = Dust.NewDustDirect(target.Center + dir * 15, 0, 0, DustID.Blood);
                dust.velocity = dir * 2.5f;
                dust.scale = 1.3f;
                dust.noGravity = true;
            }
        }
        private void FinalBurst(NPC target)
        {
            SoundEngine.PlaySound(SoundID.NPCDeath1 with { Pitch = 0.3f, Volume = 0.7f }, Projectile.Center);
            for (int i = 0; i < 25; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(7, 7);
                Dust dust = Dust.NewDustDirect(target.Center, 15, 15, DustID.Blood, dustVel.X, dustVel.Y);
                dust.scale = Main.rand.NextFloat(1.8f, 2.5f);
                dust.noGravity = true;
            }
        }
        private void ReturningBehavior(Player owner)
        {
            Vector2 toOwner = owner.Center - Projectile.Center;
            float distance = toOwner.Length();
            if (distance < 30f)
            {
                Projectile.Kill();
                return;
            }
            Vector2 direction = toOwner / distance;
            float speed = Math.Min(distance * 0.12f, 22f);
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * speed, 0.15f);
        }
        private void SpawnParticles()
        {
            if (Main.rand.NextBool(4))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Blood);
                dust.velocity = Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(0.8f, 0.8f);
                dust.scale = Main.rand.NextFloat(1.0f, 1.5f);
                dust.noGravity = true;
            }
            if (Main.rand.NextBool(6))
            {
                Dust glow = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.RedTorch);
                glow.velocity = Vector2.Zero;
                glow.scale = Main.rand.NextFloat(0.6f, 1.0f);
                glow.noGravity = true;
            }
            if (State == ThreadState.Attached && ropePoints.Count > 2 && Main.rand.NextBool(6))
            {
                int randomIndex = Main.rand.Next(1, ropePoints.Count - 1);
                Vector2 pos = ropePoints[randomIndex];
                Dust threadDust = Dust.NewDustDirect(pos, 0, 0, DustID.Blood);
                threadDust.velocity = new Vector2(0, -0.8f) + Main.rand.NextVector2Circular(0.3f, 0.3f);
                threadDust.scale = 0.7f;
                threadDust.noGravity = true;
                threadDust.alpha = 120;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (!ropeInitialized || ropePoints.Count < 2)
                return false;
            SpriteBatch spriteBatch = Main.spriteBatch;
            DrawBloodThread(spriteBatch);
            DrawProjectileWithEffects(spriteBatch);
            return false;
        }
        private void DrawBloodThread(SpriteBatch spriteBatch)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            List<Vector2> smoothPoints = GetSmoothedPoints();
            if (smoothPoints.Count < 2) return;
            for (int i = 0; i < smoothPoints.Count - 1; i++)
            {
                Vector2 startPoint = smoothPoints[i];
                Vector2 endPoint = smoothPoints[i + 1];
                Vector2 delta = endPoint - startPoint;
                float length = delta.Length();
                if (length < 0.5f) continue;
                float rotation = delta.ToRotation();
                float t = i / (float)(smoothPoints.Count - 1);
                float baseThickness = State == ThreadState.Attached ? 3.5f : 2f;
                float tensionThickness = 1f - smoothedTension * 0.35f;
                float endTaper = (float)Math.Sin(t * MathHelper.Pi);
                endTaper = 0.4f + endTaper * 0.6f;
                float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f + i * 0.25f) * 0.1f;
                float thickness = baseThickness * tensionThickness * endTaper * pulse;
                thickness = Math.Max(thickness, 1f);
                Color relaxedColor = new Color(140, 10, 10, 220);
                Color tensedColor = new Color(255, 80, 80, 255);
                Color baseColor = Color.Lerp(relaxedColor, tensedColor, smoothedTension);
                Color threadColor = Color.Lerp(baseColor * 0.85f, baseColor, t);
                float glowIntensity = 0.25f + smoothedTension * 0.4f;
                glowIntensity += (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.15f;
                Color outerGlow = new Color(255, 30, 30, 0) * glowIntensity * 0.5f;
                float outerGlowThickness = thickness * 5f;
                spriteBatch.Draw(pixel, startPoint - Main.screenPosition, new Rectangle(0, 0, 1, 1), outerGlow, rotation, new Vector2(0, 0.5f), new Vector2(length, outerGlowThickness), SpriteEffects.None, 0);
                Color midGlow = new Color(255, 60, 60, 0) * glowIntensity;
                float midGlowThickness = thickness * 2.5f;
                spriteBatch.Draw(pixel, startPoint - Main.screenPosition, new Rectangle(0, 0, 1, 1), midGlow, rotation, new Vector2(0, 0.5f), new Vector2(length, midGlowThickness), SpriteEffects.None, 0);
                spriteBatch.Draw(pixel, startPoint - Main.screenPosition, new Rectangle(0, 0, 1, 1), threadColor, rotation, new Vector2(0, 0.5f), new Vector2(length, thickness), SpriteEffects.None, 0);
                if (State == ThreadState.Attached && smoothedTension > 0.3f)
                {
                    Color coreColor = new Color(255, 200, 200, 100) * smoothedTension * 0.7f;
                    float coreThickness = thickness * 0.4f;
                    spriteBatch.Draw(pixel, startPoint - Main.screenPosition, new Rectangle(0, 0, 1, 1), coreColor, rotation, new Vector2(0, 0.5f), new Vector2(length, coreThickness), SpriteEffects.None, 0);
                }
                float lightIntensity = 0.2f + smoothedTension * 0.25f;
                Lighting.AddLight(startPoint, lightIntensity * 1.0f, lightIntensity * 0.08f, lightIntensity * 0.08f);
            }
        }
        private List<Vector2> GetSmoothedPoints()
        {
            List<Vector2> smoothed = new List<Vector2>();
            if (ropePoints.Count < 2)
            {
                return ropePoints;
            }
            float smoothFactor = 0.7f;
            for (int i = 0; i < ropePoints.Count; i++)
            {
                Vector2 current = ropePoints[i];
                Vector2 previous = i < previousRopePoints.Count ? previousRopePoints[i] : current;
                smoothed.Add(Vector2.Lerp(previous, current, smoothFactor));
            }
            List<Vector2> splinePoints = new List<Vector2>();
            for (int i = 0; i < smoothed.Count - 1; i++)
            {
                Vector2 p0 = i > 0 ? smoothed[i - 1] : smoothed[i];
                Vector2 p1 = smoothed[i];
                Vector2 p2 = smoothed[i + 1];
                Vector2 p3 = i + 2 < smoothed.Count ? smoothed[i + 2] : smoothed[i + 1];
                splinePoints.Add(p1);
                if (i < smoothed.Count - 2)
                {
                    Vector2 mid = CatmullRom(p0, p1, p2, p3, 0.5f);
                    splinePoints.Add(mid);
                }
            }
            splinePoints.Add(smoothed[smoothed.Count - 1]);
            return splinePoints;
        }
        private Vector2 CatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            return 0.5f * ((2f * p1) + (-p0 + p2) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 + (-p0 + 3f * p1 - 3f * p2 + p3) * t3);
        }
        private void DrawProjectileWithEffects(SpriteBatch spriteBatch)
        {
            Texture2D texture = ModContent.Request<Texture2D>("jarkos/textures/MagicSphereProj", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            if (texture == null) return;

            Vector2 origin = texture.Size() / 2f;


            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = 1f - (i / (float)Projectile.oldPos.Length);
                Color trailColor = new Color(255, 30, 30) * progress * 0.45f;
                float trailScale = Projectile.scale * progress * 0.85f;
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                spriteBatch.Draw(texture, drawPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0);
            }

            float glowIntensity = 0.6f + (float)Math.Sin(Main.GameUpdateCount * 0.25f) * 0.25f;


            for (int i = 0; i < 4; i++)
            {
                float angle = i * MathHelper.PiOver2 + Main.GameUpdateCount * 0.08f;
                Vector2 offset = new Vector2(3.5f, 0).RotatedBy(angle);
                Color glowColor = new Color(255, 40, 40, 0) * glowIntensity * 0.6f;
                spriteBatch.Draw(texture, Projectile.Center + offset - Main.screenPosition, null, glowColor, Projectile.rotation, origin, Projectile.scale * 1.3f, SpriteEffects.None, 0);
            }


            Color mainRed = new Color(255, 40, 40, 255);
            spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, mainRed, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);


            if (State == ThreadState.Attached)
            {
                float pulse = 0.4f + (float)Math.Sin(Main.GameUpdateCount * 0.4f) * 0.4f;
                Color coreColor = Color.White * pulse;
                spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, coreColor, Projectile.rotation, origin, Projectile.scale * 0.45f, SpriteEffects.None, 0);
            }
            else
            {
                spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, Color.White * 0.7f, Projectile.rotation, origin, Projectile.scale * 0.35f, SpriteEffects.None, 0);
            }
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Blood);
                dust.velocity = Main.rand.NextVector2Circular(2.5f, 2.5f);
                dust.scale = Main.rand.NextFloat(0.9f, 1.3f);
                dust.noGravity = true;
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (State == ThreadState.Seeking)
            {
                float radius = 18f;
                return Vector2.Distance(Projectile.Center, targetHitbox.Center.ToVector2()) <
                    radius + Math.Max(targetHitbox.Width, targetHitbox.Height) / 2f;
            }
            return base.Colliding(projHitbox, targetHitbox);
        }
    }
}
