using jarkos.buffs.summonbuff;
using jarkos.particles;
using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.summon.whip
{
	public class Perfection_phantom_blade : ModProjectile
	{
        private const int DAY_LIFETIME = 300;
        private const int NIGHT_LIFETIME = 600;
        private const int TRAIL_LENGTH = 24;
        private const float HEAL_COOLDOWN = 90f;
        private const int HEAL_AMOUNT = 3;
        private const int SPIN_DURATION = 50;
        private const int LUNGE_DURATION = 14;
        private const int PAUSE_DURATION = 16;
        private ref float Timer => ref Projectile.localAI[0];
        private ref float SpinAngle => ref Projectile.localAI[2];
        private float healTimer = 0;
        private int phaseTimer = 0;
        private int currentAttackPattern = 0;
        private Vector2[] trailPositions = new Vector2[TRAIL_LENGTH];
        private float[] trailRotations = new float[TRAIL_LENGTH];
        private bool trailInitialized = false;
        private bool IsNight => !Main.dayTime;
        private float PowerMultiplier => IsNight ? 1.8f : 0.6f;
        private Color PrimaryColor => IsNight
            ? new Color(160, 50, 255)
            : new Color(140, 80, 200, 150);
        private Color GlowColor => IsNight
            ? new Color(180, 60, 255, 0)
            : new Color(120, 60, 180, 0);
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = TRAIL_LENGTH;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.SummonMeleeSpeed;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
            Projectile.alpha = 255;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.timeLeft = IsNight ? NIGHT_LIFETIME * 2 : DAY_LIFETIME * 2;
        }
        public override void AI()
        {
            Timer++;
            phaseTimer++;
            NPC target = GetTarget();
            if (target == null)
            {
                target = FindNewTarget();
                if (target == null)
                {
                    IdleBehavior();
                    UpdateTrail();
                    SpawnAmbientDust();
                    return;
                }
                Projectile.ai[0] = target.whoAmI;
            }
            float fadeTime = 20f;
            if (Timer <= fadeTime)
                Projectile.alpha = (int)(255 * (1f - Timer / fadeTime));
            else
                Projectile.alpha = 0;
            if (!IsNight)
            {
                healTimer++;
                if (healTimer >= HEAL_COOLDOWN * 2)
                {
                    healTimer = 0;
                    Player owner = Main.player[Projectile.owner];
                    if (owner.active && !owner.dead)
                    {
                        owner.Heal(HEAL_AMOUNT);
                        for (int i = 0; i < 5; i++)
                        {
                            Dust d = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.PurificationPowder);
                            d.velocity = (owner.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 3f
                                + Main.rand.NextVector2Circular(1f, 1f);
                            d.scale = 0.8f;
                            d.noGravity = true;
                            d.alpha = 100;
                        }
                    }
                }
            }
            UpdateAttackBehavior(target);
            if (Projectile.velocity.LengthSquared() > 1f)
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            SpawnAmbientDust();
            UpdateTrail();
            Lighting.AddLight(Projectile.Center, PrimaryColor.ToVector3() * (IsNight ? 0.7f : 0.3f));
        }
        private NPC GetTarget()
        {
            int index = (int)Projectile.ai[0];
            if (index >= 0 && index < Main.maxNPCs)
            {
                NPC npc = Main.npc[index];
                if (npc.active && npc.life > 0 && !npc.friendly && !npc.dontTakeDamage)
                    return npc;
            }
            return null;
        }
        private NPC FindNewTarget()
        {
            float searchRadius = IsNight ? 900f : 500f;
            float closestDist = searchRadius;
            NPC closest = null;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.life <= 0)
                    continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc;
                }
            }
            if (closest != null)
            {
                SoundEngine.PlaySound(SoundID.Item101 with
                {
                    Volume = 0.3f, Pitch = 0.5f
                }, Projectile.Center);
            }
            return closest;
        }
        private void IdleBehavior()
        {
            Player owner = Main.player[Projectile.owner];
            Vector2 idleTarget = owner.Center + new Vector2(MathF.Cos(Timer * 0.03f) * 60f, -50f + MathF.Sin(Timer * 0.05f) * 20f);
            Vector2 toIdle = idleTarget - Projectile.Center;
            float dist = toIdle.Length();
            if (dist > 2f)
            {
                toIdle.Normalize();
                float speed = MathHelper.Clamp(dist * 0.1f, 1f, 8f);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toIdle * speed, 0.15f);
            }
            Projectile.velocity *= 0.95f;
            if (Projectile.velocity.LengthSquared() > 1f)
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
        }
        private void UpdateAttackBehavior(NPC target)
        {
            int cycleLength = (SPIN_DURATION + LUNGE_DURATION + PAUSE_DURATION) * 2;
            int spinDur = SPIN_DURATION * 2;
            int lungeDur = LUNGE_DURATION * 2;
            int pauseDur = PAUSE_DURATION * 2;

            int timeInCycle = phaseTimer % cycleLength;
            int cycleCount = phaseTimer / cycleLength;

            currentAttackPattern = cycleCount % 3;
            if (IsNight && currentAttackPattern == 0 && cycleCount % 2 == 0)
                currentAttackPattern = 1;
            if (timeInCycle < spinDur)
            {
                PerformSpin(target, timeInCycle, spinDur);
            }
            else if (timeInCycle < spinDur + lungeDur)
            {
                int attackTime = timeInCycle - spinDur;
                PerformAttack(target, attackTime, lungeDur);
            }
            else
            {
                PerformPause(target, timeInCycle - spinDur - lungeDur, pauseDur);
            }
        }
        private void PerformSpin(NPC target, int time, int duration)
        {
            float progress = (float)time / duration;
            float spinSpeed = 0.08f + progress * progress * 0.25f;
            SpinAngle += spinSpeed;
            float radius = MathHelper.Lerp(130f, 50f, progress * progress);
            float heightMod = currentAttackPattern == 2 ? 0.3f : 0.6f;

            Vector2 orbitPos = target.Center + new Vector2(MathF.Cos(SpinAngle) * radius, MathF.Sin(SpinAngle) * radius * heightMod);
            Vector2 toOrbit = orbitPos - Projectile.Center;
            float dist = toOrbit.Length();
            if (dist > 1f)
            {
                toOrbit.Normalize();
                float speed = MathHelper.Clamp(dist * 0.2f, 3f, 18f);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toOrbit * speed, 0.25f);
            }
            if (progress > 0.6f)
            {
                float charge = (progress - 0.6f) / 0.4f;
                Lighting.AddLight(Projectile.Center, PrimaryColor.ToVector3() * charge * 0.8f);
                if (Main.rand.NextBool(2))
                {
                    Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(30, 30);
                    Dust d = Dust.NewDustDirect(dustPos, 1, 1, DustID.PurpleTorch);
                    d.velocity = (Projectile.Center - dustPos) * 0.1f;
                    d.scale = 0.5f + charge * 0.8f;
                    d.noGravity = true;
                    d.alpha = 80;
                }
            }
            if (time == 2)
            {
                SoundEngine.PlaySound(SoundID.Item15 with { Volume = 0.3f, Pitch = -0.5f + currentAttackPattern * 0.2f}, Projectile.Center);
            }
        }
        private void PerformAttack(NPC target, int time, int duration)
        {
            switch (currentAttackPattern)
            {
                case 0:
                    AttackDirectLunge(target, time, duration);
                    break;
                case 1:
                    AttackLongPierce(target, time, duration);
                    break;
                case 2:
                    AttackSpiralStrike(target, time, duration);
                    break;
            }
        }
        private void AttackDirectLunge(NPC target, int time, int duration)
        {
            if (time <= 2)
            {
                SoundEngine.PlaySound(SoundID.Item71 with
                {
                    Volume = 0.5f, Pitch = IsNight ? 0.2f : 0.5f
                }, Projectile.Center);
                Vector2 dir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                float speed = IsNight ? 26f : 18f;
                Projectile.velocity = dir * speed;
            }
            float progress = (float)time / duration;
            if (progress < 0.5f)
            {
                Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), 0.1f);
            }
            SpawnAttackDust();
        }
        private void AttackLongPierce(NPC target, int time, int duration)
        {
            if (time <= 2)
            {
                SoundEngine.PlaySound(SoundID.Item122 with { Volume = IsNight ? 0.7f : 0.4f, Pitch = -0.3f}, Projectile.Center);
                Vector2 dir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                float speed = IsNight ? 35f : 20f;
                Projectile.velocity = dir * speed;
                if (IsNight)
                {
                    Projectile.width = 60;
                    Projectile.height = 60;
                    Projectile.position -= new Vector2(14, 14);
                }
            }
            if (Main.rand.NextBool(1))
            {
                new Solid_particle(Projectile.Center - Projectile.velocity * 0.5f, -Projectile.velocity * Main.rand.NextFloat(0.05f, 0.2f) + Main.rand.NextVector2Circular(3f, 3f), Color.MediumPurple, 200, 0.5f, 2f, 0f).Spawn();
            }
            if (time >= duration - 2 && IsNight)
            {
                Projectile.position += new Vector2(14, 14);
                Projectile.width = 32;
                Projectile.height = 32;
            }
        }
        private void AttackSpiralStrike(NPC target, int time, int duration)
        {
            float progress = (float)time / duration;
            if (time <= 2)
            {
                SoundEngine.PlaySound(SoundID.Item117 with { Volume = 0.5f, Pitch = 0.4f}, Projectile.Center);
            }
            float spiralAngle = SpinAngle + progress * MathHelper.TwoPi * 3f;
            float spiralRadius = MathHelper.Lerp(80f, 0f, progress);
            Vector2 spiralTarget = target.Center + new Vector2(MathF.Cos(spiralAngle) * spiralRadius, MathF.Sin(spiralAngle) * spiralRadius);
            Vector2 toSpiral = spiralTarget - Projectile.Center;
            float dist = toSpiral.Length();
            if (dist > 1f)
            {
                toSpiral.Normalize();
                float speed = MathHelper.Clamp(dist * 0.25f, 4f, 22f);
                Projectile.velocity = toSpiral * speed;
            }
            SpawnAttackDust();
        }
        private void PerformPause(NPC target, int time, int duration)
        {
            if (time <= 4)
            {
                Vector2 awayDir = (Projectile.Center - target.Center).SafeNormalize(Main.rand.NextVector2Unit());
                Projectile.velocity = awayDir * 12f;
            }
            Projectile.velocity *= 0.92f;
        }
        private void SpawnAttackDust()
        {
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustDirect(Projectile.Center - Projectile.velocity, 1, 1, DustID.PurpleTorch);
                d.velocity = -Projectile.velocity * Main.rand.NextFloat(0.05f, 0.15f)
                    + Main.rand.NextVector2Circular(2f, 2f);
                d.scale = Main.rand.NextFloat(1.0f, 1.6f);
                d.noGravity = true;
            }
        }
        private void SpawnAmbientDust()
        {
            if (!Main.rand.NextBool(4)) return;
            Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.PurpleTorch);
            d.velocity = Projectile.velocity * 0.05f + Main.rand.NextVector2Circular(1f, 1f);
            d.scale = Main.rand.NextFloat(0.5f, 1.0f) * (IsNight ? 1.2f : 0.7f);
            d.noGravity = true;
            d.alpha = IsNight ? 60 : 120;
            if (IsNight && Main.rand.NextBool(5))
            {
                Dust shadow = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(15, 15), 1, 1, DustID.Shadowflame);
                shadow.velocity = Main.rand.NextVector2Circular(1.5f, 1.5f);
                shadow.scale = 0.6f;
                shadow.noGravity = true;
            }
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.FinalDamage *= PowerMultiplier;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundStyle hitSound = IsNight ? SoundID.Item100 with { Volume = 0.4f, Pitch = -0.2f } : SoundID.Item101 with { Volume = 0.3f, Pitch = 0.3f };
            SoundEngine.PlaySound(hitSound, target.Center);
            for (int i = 0; i < (IsNight ? 12 : 6); i++)
            {
                new Spark(target.Center, Main.rand.NextVector2Circular(4f, 4f), Color.MediumPurple, 200, 0.5f, 2f, 0f).Spawn();
            }
            Player owner = Main.player[Projectile.owner];
            if (owner.HasBuff(ModContent.BuffType<NoctiliumExplosiveBuff>()))
            {
                SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.4f, Pitch = 0.2f }, target.Center);
            }
            if (!IsNight && Main.myPlayer == Projectile.owner)
            {
                owner.Heal(1);
            }
        }
        public override void OnKill(int timeLeft)
        {
            if (Main.dedServ) return;
            SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.5f, Pitch = IsNight ? -0.3f : 0.3f}, Projectile.Center);
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                Vector2 vel = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * Main.rand.NextFloat(2f, 5f);
                Dust d = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.PurpleTorch);
                d.velocity = vel;
                d.scale = Main.rand.NextFloat(1.0f, 1.8f);
                d.noGravity = true;
                d.alpha = 60;
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity) => false;
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
            effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 1.8f);
            effect.Parameters["uOpacity"]?.SetValue(opacity);

            float colorCycle = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 4f + Projectile.whoAmI) * 0.5f + 0.5f;
            Color deepPurple = new Color(130, 0, 255);
            Color brightViolet = new Color(220, 80, 255);
            Color outerColor = Color.Lerp(deepPurple, brightViolet, colorCycle);
            Color innerColor = Color.Lerp(brightViolet, new Color(255, 180, 255), colorCycle);

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
            float baseWidth = IsNight ? 24f : 16f;
            TrailRenderer.Draw(effect, smoothPoints, t => baseWidth * MathF.Pow(1f - t, 1.2f) + 0.5f, t => Color.White * (1f - t), HealShaders.HealNoise);
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        private void DrawGlow(SpriteBatch sb, float opacity)
        {
            Texture2D magicPixel = TextureAssets.MagicPixel.Value;
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            float glowPulse = 1f + MathF.Sin(Timer * 0.08f) * 0.3f;
            float baseGlow = IsNight ? 55f : 30f;
            float glowSize = baseGlow * glowPulse * opacity;
            Color glowCol = GlowColor * (IsNight ? 0.3f : 0.15f) * opacity;
            for (int layer = 0; layer < 5; layer++)
            {
                float layerScale = glowSize * (1f + layer * 0.35f);
                float layerAlpha = 1f / (1f + layer * 1.3f);
                float rot = MathHelper.PiOver4 + Timer * 0.02f * (layer % 2 == 0 ? 1 : -1);
                sb.Draw(magicPixel, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), glowCol * layerAlpha, rot, new Vector2(0.5f, 0.5f), new Vector2(layerScale, layerScale), SpriteEffects.None, 0f);
            }
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        private void DrawBlade(SpriteBatch sb, Color lightColor, float opacity)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Rectangle frame = texture.Bounds;
            Vector2 origin = frame.Size() / 2f;
            Color drawColor = Color.Lerp(lightColor, PrimaryColor, 0.5f) * opacity;
            if (!IsNight)
                drawColor *= 0.7f;
            sb.Draw(texture, Projectile.Center - Main.screenPosition, frame, drawColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            Color addCol = PrimaryColor * (IsNight ? 0.6f : 0.25f) * opacity;
            sb.Draw(texture, Projectile.Center - Main.screenPosition, frame, addCol, Projectile.rotation, origin, Projectile.scale * 1.08f, SpriteEffects.None, 0f);
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
	}
}
