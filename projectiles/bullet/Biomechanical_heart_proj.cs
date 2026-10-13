using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using Terraria.DataStructures;
using jarkos.particles;
using Terraria.GameContent;
namespace jarkos.projectiles.bullet
{
	public class Biomechanical_heart_proj : ModProjectile
    {
        private enum HeartState
        {
            Spawning, Idle, MovingToEnemy, Pulsing, Draining, Shooting, Despawning, Dead
        }
        private HeartState state = HeartState.Idle;
        public float powerFactor = 0f;
        private int pulseCount = 0;
        private int pulseTimer = 0;
        private const int PULSE_INTERVAL = 20;
        private const int TOTAL_PULSES = 5;
        private float pulseScale = 1f;
        private int drainTimer = 0;
        private const int DRAIN_DURATION_BASE = 120;
        private float drainRadius = 200f;
        private float bloodAccumulated = 0f;
        private const float DRAIN_RATE_BASE = 2f;
        private int targetNPC = -1;
        private int SpawnTimer = 0;
        private const int SPAWN_DURATION = 50;
        private float SpawnScale = 1f;
        private bool SpawnRequested = false;
        private int despawnTimer = 0;
        private const int DESPAWN_DURATION = 120;
        private float despawnScale = 1f;
        private bool despawnRequested = false;
        private float idleAngle = 0f;
        private int idleCooldown = 0;
        private float glowIntensity = 1.5f;
        private float baseScale = 1.5f;
        private List<Vector2> drainParticles = new List<Vector2>();
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = false;
            ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.width = 46;
            Projectile.height = 52;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = int.MaxValue;
            Projectile.netImportant = true;
        }
        public override void OnSpawn(IEntitySource source)
        {
            StartSpawn();
        }
        public void StartSpawn()
        {
            SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/fun_appear") with { Volume = 1f, Pitch = 0.5f }, Projectile.Center);
            SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/bioheart_appear") with { Volume = 0.8f, Pitch = 0.5f }, Projectile.Center);
            if (state != HeartState.Spawning && state != HeartState.Dead)
            {
                SpawnRequested = true;
            }
        }
        public void StartDespawn()
        {
            if (state != HeartState.Despawning && state != HeartState.Dead)
            {
                despawnRequested = true;
            }
        }
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (owner.dead || !owner.active)
            {
                if (state != HeartState.Despawning && state != HeartState.Dead)
                {
                    despawnRequested = true;
                }
            }
            if (despawnRequested && state != HeartState.Despawning && state != HeartState.Dead)
            {
                SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/fun_appear_reverse") with { Volume = 1f, Pitch = 0.5f }, Projectile.Center);
                SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/bioheart_appear") with { Volume = 0.8f, Pitch = 0.5f }, Projectile.Center);
                state = HeartState.Despawning;
                despawnTimer = 0;
                despawnRequested = false;
            }
            if (SpawnRequested && state != HeartState.Spawning && state != HeartState.Dead)
            {
                state = HeartState.Spawning;
                SpawnTimer = 0;
                SpawnRequested = false;
            }
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 6)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % Main.projFrames[Projectile.type];
            }
            glowIntensity = 0.7f + powerFactor * 0.7f;
            Projectile.rotation = Projectile.velocity.X * 5.722f;
            SpawnAmbientDust();
            switch (state)
            {
                case HeartState.Spawning:
                    AI_Spawning(owner);
                    break;
                case HeartState.Idle:
                    AI_Idle(owner);
                    break;
                case HeartState.MovingToEnemy:
                    AI_MovingToEnemy(owner);
                    break;
                case HeartState.Pulsing:
                    AI_Pulsing(owner);
                    break;
                case HeartState.Draining:
                    AI_Draining(owner);
                    break;
                case HeartState.Shooting:
                    AI_Shooting(owner);
                    break;
                case HeartState.Despawning:
                    AI_Despawning(owner);
                    break;
                case HeartState.Dead:
                    Projectile.Kill();
                    break;
            }
        }
        private void AI_Idle(Player owner)
        {
            idleAngle += 0.03f;
            float orbitRadius = 60f;
            Vector2 targetPos = owner.Center + new Vector2((float)Math.Cos(idleAngle) * orbitRadius, (float)Math.Sin(idleAngle) * orbitRadius - 40f);
            Projectile.Center = Vector2.Lerp(Projectile.Center, targetPos, 0.1f);
            Projectile.velocity = Vector2.Zero;
            baseScale = 1f;
            if (idleCooldown > 0)
            {
                idleCooldown--;
                return;
            }
            targetNPC = FindClosestEnemy(owner.Center, 600f);
            if (targetNPC >= 0)
            {
                state = HeartState.MovingToEnemy;
            }
        }
        private void AI_MovingToEnemy(Player owner)
        {
            if (!ValidateTarget())
            {
                ReturnToIdle();
                return;
            }
            NPC target = Main.npc[targetNPC];
            Vector2 targetPos = target.Center + new Vector2(0, -60f);
            float dist = Vector2.Distance(Projectile.Center, targetPos);
            if (dist > 10f)
            {
                float speed = MathHelper.Lerp(4f, 16f, powerFactor);
                Vector2 direction = (targetPos - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = direction * Math.Min(speed, dist);
                Projectile.Center += Projectile.velocity;
                Projectile.velocity = Vector2.Zero;
            }
            else
            {
                state = HeartState.Pulsing;
                pulseCount = 0;
                pulseTimer = 0;
                SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/bioheart_sfx") with { Volume = 1f, Pitch = 0.3f }, Projectile.Center);
            }
        }
        private void AI_Pulsing(Player owner)
        {
            if (!ValidateTarget())
            {
                ReturnToIdle();
                return;
            }
            NPC target = Main.npc[targetNPC];
            Vector2 targetPos = target.Center + new Vector2(0, -60f);
            Projectile.Center = Vector2.Lerp(Projectile.Center, targetPos, 0.15f);
            pulseTimer++;
            Projectile.rotation *= 0.982f;
            int adjustedInterval = Math.Max(10, (int)(PULSE_INTERVAL * (1f - powerFactor * 0.4f)));
            float pulseProgress = (float)(pulseTimer % adjustedInterval) / adjustedInterval;
            pulseScale = 1f + 0.1f * (float)Math.Sin(pulseProgress * MathHelper.TwoPi);
            baseScale = pulseScale;
            if (pulseTimer % adjustedInterval == adjustedInterval / 2)
            {
                pulseCount++;
                Projectile.rotation += 0.4f;
                SpawnPulseEffect(target);
                SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/bioheart_sfx") with { Volume = 0.45f, Pitch = Main.rand.NextFloat(-0.3f, 0.3f) }, Projectile.Center);
                float pulseDamage = 5f + 15f * powerFactor;
                if (target.active && !target.friendly)
                {
                    owner.ApplyDamageToNPC(target, (int)pulseDamage, 0f, 0, false);
                }
            }
            if (pulseCount >= TOTAL_PULSES)
            {
                state = HeartState.Draining;
                drainTimer = 0;
                bloodAccumulated = 0f;
                baseScale = 1f;
                drainRadius = 150f + 150f * powerFactor;
                SoundEngine.PlaySound(SoundID.NPCDeath19 with { Volume = 0.6f, Pitch = -0.5f }, Projectile.Center);
            }
        }
        private void AI_Draining(Player owner)
        {
            int drainDuration = (int)(DRAIN_DURATION_BASE * (1f + powerFactor * 0.5f));
            drainTimer++;
            float drainProgress = (float)drainTimer / drainDuration;
            baseScale = 1f + 0.15f * (float)Math.Sin(drainProgress * MathHelper.TwoPi * 3f);
            glowIntensity = 0.1f + 0.35f * drainProgress;
            float drainRate = DRAIN_RATE_BASE + DRAIN_RATE_BASE * powerFactor * 3f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.immortal)
                    continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist > drainRadius)
                    continue;
                float distFactor = 1f - (dist / drainRadius);
                float damage = drainRate * distFactor;
                if (drainTimer % 5 == 0)
                {
                    int actualDamage = Math.Max(1, (int)damage);
                    owner.ApplyDamageToNPC(npc, actualDamage, 0f, 0, false);
                    bloodAccumulated += actualDamage * 0.5f;
                    SpawnBloodDrainParticle(npc.Center);
                }
            }
            if (drainTimer % 3 == 0 && bloodAccumulated > 0)
            {
                int dustType = DustID.Blood;
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(15f, 15f);
                Dust d = Dust.NewDustDirect(dustPos, 4, 4, dustType);
                d.velocity = (Projectile.Center - d.position) * 0.05f;
                d.noGravity = true;
                d.scale = 1f + bloodAccumulated * 0.005f;
            }
            if (drainTimer >= drainDuration)
            {
                state = HeartState.Shooting;
                SoundEngine.PlaySound(SoundID.Item29 with { Volume = 0.8f }, Projectile.Center);
            }
        }
        private void AI_Shooting(Player owner)
        {
            if (bloodAccumulated > 0)
            {
                float healAmount = bloodAccumulated;
                healAmount = Math.Min(healAmount, owner.statLifeMax2 * 0.3f);
                int projIdx = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, (owner.Center - Projectile.Center).SafeNormalize(Vector2.UnitY) * 8f, ModContent.ProjectileType<HealingBloodProjectile>(), 0, 0f, Projectile.owner, healAmount);
                for (int i = 0; i < 20; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                    Dust d = Dust.NewDustDirect(Projectile.Center, 6, 6, DustID.LifeDrain);
                    d.velocity = vel;
                    d.noGravity = true;
                    d.scale = 1.5f;
                }
            }
            bloodAccumulated = 0f;
            idleCooldown = 120;
            ReturnToIdle();
        }
        #region Spawning
        private void AI_Spawning(Player owner)
        {
            SpawnTimer++;
            float t = (float)SpawnTimer / SPAWN_DURATION;
            float eased = EaseInOut(t);
            SpawnScale = 0f + eased;
            baseScale = Math.Max(0.01f, SpawnScale);
            glowIntensity = 0.5f + eased * 1.5f;
            if (SpawnTimer % 2 == 0)
            {
                for (int i = 0; i < (int)(3 * eased) + 1; i++)
                {
                    Vector2 offset = Main.rand.NextVector2Circular(20f * (1f - eased), 20f * (1f - eased));
                    new StarParticle(Projectile.Center + offset, -offset * 0.5f, Color.IndianRed, 200, 0.05f, 1f, 0f).Spawn();
                    Dust d = Dust.NewDustDirect(Projectile.Center + offset, 2, 2, DustID.MagicMirror);
                    d.velocity = -offset * 0.1f;
                    d.noGravity = true;
                    d.scale = 1.5f * (1f - eased);
                    d.color = new Color(255, 100, 100);
                }
            }
            if (SpawnTimer >= SPAWN_DURATION)
            {
                SpawnDespawnFlash();
                state = HeartState.Idle;
            }
        }
        #endregion
        private void AI_Despawning(Player owner)
        {
            despawnTimer++;
            float t = (float)despawnTimer / DESPAWN_DURATION;
            float eased = EaseInOut(t);
            despawnScale = 1f - eased;
            baseScale = Math.Max(0.01f, despawnScale);
            glowIntensity = 0.5f + eased * 1.5f;
            if (despawnTimer % 2 == 0)
            {
                for (int i = 0; i < (int)(3 * eased) + 1; i++)
                {
                    Vector2 offset = Main.rand.NextVector2Circular(20f * (1f - eased), 20f * (1f - eased));
                    new StarParticle(Projectile.Center + offset, -offset * 0.5f, Color.IndianRed, 200, 0.05f, 1f, 0f).Spawn();
                    Dust d = Dust.NewDustDirect(Projectile.Center + offset, 2, 2, DustID.MagicMirror);
                    d.velocity = -offset * 0.1f;
                    d.noGravity = true;
                    d.scale = 1.5f * (1f - eased);
                    d.color = new Color(255, 100, 100);
                }
            }
            if (despawnTimer >= DESPAWN_DURATION)
            {
                state = HeartState.Dead;
            }
        }
        private float EaseInOut(float t)
        {
            if (t < 0.5f)
                return 4f * t * t * t;
            else
            {
                float f = (2f * t - 2f);
                return 0.5f * f * f * f + 1f;
            }
        }
        private int FindClosestEnemy(Vector2 position, float maxRange)
        {
            int closest = -1;
            float closestDist = maxRange;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.immortal || npc.CountsAsACritter)
                    continue;
                float dist = Vector2.Distance(position, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = i;
                }
            }
            return closest;
        }
        private bool ValidateTarget()
        {
            if (targetNPC < 0 || targetNPC >= Main.maxNPCs)
                return false;
            NPC npc = Main.npc[targetNPC];
            return npc.active && !npc.friendly && !npc.dontTakeDamage;
        }
        private void ReturnToIdle()
        {
            state = HeartState.Idle;
            targetNPC = -1;
            pulseCount = 0;
            pulseTimer = 0;
            drainTimer = 0;
            baseScale = 1f;
        }
        private void SpawnAmbientDust()
        {
            if (state == HeartState.Despawning || state == HeartState.Dead)
                return;
            if (Main.rand.NextBool(8))
            {
                Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                Dust d = Dust.NewDustDirect(Projectile.Center + offset, 4, 4, DustID.LifeDrain);
                d.velocity = -offset * 0.03f;
                d.noGravity = true;
                d.scale = 0.8f * glowIntensity;
                d.color = Color.Lerp(Color.DarkRed, Color.Red, powerFactor);
            }
        }
        private void SpawnPulseEffect(NPC target)
        {
            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi / 16f * i;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 4f;
                Dust d = Dust.NewDustDirect(Projectile.Center, 4, 4, DustID.LifeDrain);
                d.velocity = vel;
                d.noGravity = true;
                d.scale = 1.2f + powerFactor * 0.5f;
                d.color = new Color(255, 50, 50);
            }
        }
        private void SpawnBloodDrainParticle(Vector2 from)
        {
            Vector2 direction = (Projectile.Center - from).SafeNormalize(Vector2.Zero);
            float dist = Vector2.Distance(from, Projectile.Center);
            for (int i = 0; i < 3; i++)
            {
                Vector2 pos = from + Main.rand.NextVector2Circular(10f, 10f);
                Dust d = Dust.NewDustDirect(pos, 4, 4, DustID.Blood);
                d.velocity = direction * (dist * 0.02f) + Main.rand.NextVector2Circular(1f, 1f);
                d.noGravity = true;
                d.scale = 1.3f;
                d.fadeIn = 1.5f;
            }
        }
        private void SpawnDespawnFlash()
        {
            for (int i = 0; i < 40; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(8f, 8f);
                Dust d = Dust.NewDustDirect(Projectile.Center, 4, 4, DustID.MagicMirror);
                d.velocity = vel;
                d.noGravity = true;
                d.scale = 2f;
                d.color = new Color(255, 150, 150);
            }
            for (int i = 0; i < 30; i++)
            {
                float angle = MathHelper.TwoPi / 30f * i;
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 6f;
                Dust d = Dust.NewDustDirect(Projectile.Center, 4, 4, DustID.GoldFlame);
                d.velocity = vel;
                d.noGravity = true;
                d.scale = 1.5f;
            }
            SoundEngine.PlaySound(SoundID.Item29 with { Volume = 1f, Pitch = 0.5f }, Projectile.Center);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.End();
            Texture2D glowTexture = ModContent.Request<Texture2D>("jarkos/projectiles/bullet/Biomechanical_heart_proj_glow").Value;
            Texture2D glowTexture2 = ModContent.Request<Texture2D>("jarkos/projectiles/bullet/Biomechanical_heart_proj_glowtwo").Value;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            Rectangle sourceRect = new Rectangle(0, frameHeight * Projectile.frame, texture.Width, frameHeight);
            Vector2 originGlowTwo = new Vector2(glowTexture2.Width / 2f, glowTexture2.Height / 2f);
            Vector2 originGlow = new Vector2(glowTexture.Width / 2f, glowTexture.Height / 2f);
            Vector2 origin = new Vector2(texture.Width / 2f, frameHeight / 2f);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float drawScale = baseScale;
            Vector2 true_scale = new Vector2(drawScale);
            Vector2 true_scale_increased = new Vector2(drawScale);
            Vector2 true_scale_reversed = new Vector2(drawScale);
            float rotation = Projectile.rotation;
            float t = (float)despawnTimer / DESPAWN_DURATION;
            float eased = EaseInOut(t);
            float t2 = (float)SpawnTimer / SPAWN_DURATION;
            float eased2 = EaseInOut(t2);
            if(despawnTimer > 1)
            {
                true_scale = new Vector2(1f, drawScale - eased);
                true_scale_reversed = new Vector2(drawScale - eased, 1f);
            }
            if(SpawnTimer > 1)
            {
                true_scale_increased = new Vector2(1f, 0f + eased2);
            }
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
            Color glowColor = new Color(255, 50, 50) * glowIntensity * 0.5f;
            for (int i = 0; i < 4; i++)
            {
                float glowAngle = MathHelper.TwoPi / 4f * i;
                Vector2 glowOffset = new Vector2((float)Math.Cos(glowAngle), (float)Math.Sin(glowAngle)) * 3f * glowIntensity;
                Main.EntitySpriteDraw(glowTexture, drawPos + glowOffset, null, glowColor, rotation, originGlow, (true_scale * 2.5f) * true_scale_increased, SpriteEffects.None, 0);
            }
            for (int i = 0; i < 4; i++)
            {
                float glowAngle = MathHelper.TwoPi / 4f * i;
                Vector2 glowOffset = new Vector2((float)Math.Cos(glowAngle), (float)Math.Sin(glowAngle)) * 3f * glowIntensity;
                Main.EntitySpriteDraw(glowTexture2, drawPos + glowOffset, null, glowColor, rotation, originGlowTwo, (true_scale_reversed * 0.65f) * true_scale_increased, SpriteEffects.None, 0);
            }
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
            Color drawColor = Color.White * (0.7f + glowIntensity * 0.3f);
            if (state == HeartState.Draining)
            {
                float bloodFactor = Math.Min(1f, bloodAccumulated / 100f);
                drawColor = Color.Lerp(Color.White, new Color(255, 100, 100), bloodFactor);
            }
            Main.EntitySpriteDraw(texture, drawPos, sourceRect, drawColor, rotation, origin, true_scale * true_scale_increased, SpriteEffects.None, 0);
            if (state == HeartState.Draining)
            {
                DrawDrainBeams();
            }
            return false;
        }
        private void DrawDrainBeams()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage)
                    continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist > drainRadius)
                    continue;
                float alpha = (1f - dist / drainRadius) * 0.5f;
                Vector2 start = Projectile.Center - Main.screenPosition;
                Vector2 end = npc.Center - Main.screenPosition;
                int segments = (int)(dist / 16f);
                for (int j = 0; j < segments; j++)
                {
                    float lerp = (float)j / segments;
                    float wave = (float)Math.Sin((lerp * 8f + Main.GameUpdateCount * 0.1f)) * 5f;
                    Vector2 perpendicular = (end - start).SafeNormalize(Vector2.Zero);
                    perpendicular = new Vector2(-perpendicular.Y, perpendicular.X) * wave;
                    Vector2 pos = Vector2.Lerp(start, end, lerp) + perpendicular;
                    Dust d = Dust.NewDustDirect(pos + Main.screenPosition, 2, 2, DustID.Blood);
                    d.velocity = (Projectile.Center - d.position) * 0.02f;
                    d.noGravity = true;
                    d.noLight = false;
                    d.scale = 0.8f * alpha * 2f;
                    d.color = Color.DarkRed;
                }
            }
        }
        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => false;
    }
    #region Healing Projectile
    public class HealingBloodProjectile : ModProjectile
    {
        public override string Texture => "jarkos/projectiles/Empty_proj";
        private float healAmount => Projectile.ai[0];
        private float homingStrength = 0f;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 300;
            Projectile.alpha = 50;
        }
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }
            Vector2 toPlayer = owner.Center - Projectile.Center;
            float dist = toPlayer.Length();
            homingStrength = Math.Min(homingStrength + 0.02f, 0.3f);
            if (dist > 0)
            {
                Vector2 desiredVelocity = toPlayer.SafeNormalize(Vector2.Zero) * 14f;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, homingStrength);
            }
            float maxSpeed = 16f;
            if (Projectile.velocity.Length() > maxSpeed)
            {
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * maxSpeed;
            }
            Projectile.rotation = Projectile.velocity.ToRotation();
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.LifeDrain);
                d.velocity = -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f);
                d.noGravity = true;
                d.scale = 1.2f;
                d.color = Color.Lerp(Color.Red, Color.Pink, Main.rand.NextFloat());
            }
            if (dist < 30f)
            {
                HealPlayer(owner);
                Projectile.Kill();
            }
        }
        private void HealPlayer(Player player)
        {
            int heal = Math.Max(1, (int)healAmount);
            int maxHeal = player.statLifeMax2 - player.statLife;
            heal = Math.Min(heal, maxHeal);
            if (heal > 0)
            {
                player.Heal(heal);
                for (int i = 0; i < 25; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                    Dust d = Dust.NewDustDirect(player.Center, 8, 8, DustID.LifeDrain);
                    d.velocity = vel;
                    d.noGravity = true;
                    d.scale = 1.5f;
                    d.color = new Color(255, 100, 150);
                }
                SoundEngine.PlaySound(SoundID.Item4 with { Volume = 0.7f, Pitch = 0.3f }, player.Center);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float trailFactor = 1f - (float)i / Projectile.oldPos.Length;
                Color trailColor = new Color(255, 50, 80) * trailFactor * 0.6f;
                float trailScale = 0.8f * trailFactor;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Main.EntitySpriteDraw(texture, trailPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0);
            }
            Color glowColor = new Color(255, 80, 100) * 0.4f;
            for (int i = 0; i < 3; i++)
            {
                float angle = MathHelper.TwoPi / 3f * i + Main.GameUpdateCount * 0.1f;
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 4f;
                Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + offset, null, glowColor, Projectile.rotation, origin, 1.5f, SpriteEffects.None, 0);
            }
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, origin, 1f, SpriteEffects.None, 0);
            return false;
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.LifeDrain);
                d.velocity = Main.rand.NextVector2Circular(4f, 4f);
                d.noGravity = true;
                d.scale = 1.2f;
            }
        }
    }
    #endregion
}
