using jarkos.dusts;
using jarkos.particles;
using jarkos.projectiles.melee.other_melee;
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
namespace jarkos.projectiles.melee.other_melee
{
    public class Grantores_proj : ModProjectile
    {
        private List<int> hitNPCs = new List<int>();
        private float pulseTimer = 0f;
        private const float MAX_SEARCH_RANGE = 600f;
        private const float SLOWDOWN_DURATION = 12f;
        private const float RETREAT_DISTANCE = 40f;
        private const int TRAIL_LENGTH = 20;
        private Vector2[] trailPositions = new Vector2[TRAIL_LENGTH];
        private float[] trailRotations = new float[TRAIL_LENGTH];
        private float[] trailAlphas = new float[TRAIL_LENGTH];
        private bool trailInitialized = false;
        private bool stuckInTile = false;
        private float stuckTimer = 0f;
        private float stuckRotation = 0f;
        private Vector2 stuckPosition = Vector2.Zero;
        private const float EXPLOSION_DELAY = 60f;
        private const float EXPLOSION_RADIUS = 120f;
        private enum SpearState
        {
            Flying, SlowingDown, Retreating, Retargeting, Homing, StuckInTile
        }
        private SpearState currentState
        {
            get => (SpearState)Projectile.ai[1];
            set => Projectile.ai[1] = (float)value;
        }
        private int targetNPC
        {
            get => (int)Projectile.ai[2] - 1;
            set => Projectile.ai[2] = value + 1;
        }
        private SpearState oldState = SpearState.Flying;
        private float stateTimer = 0f;
        private Vector2 retreatDirection = Vector2.Zero;
        private static readonly Color GlowYellowBright = new Color(255, 230, 80);
        private static readonly Color GlowYellowMid = new Color(255, 200, 40);
        private static readonly Color GlowYellowDark = new Color(200, 150, 20);
        private static readonly Color GlowOrange = new Color(255, 160, 30);
        private static readonly Color GlowWhiteYellow = new Color(255, 250, 200);
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TRAIL_LENGTH;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 7;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
            Projectile.light = 0.4f;
        }
        public override void AI()
        {
            pulseTimer += 0.05f;
            if (!trailInitialized)
            {
                for (int i = 0; i < TRAIL_LENGTH; i++)
                {
                    trailPositions[i] = Projectile.Center;
                    trailRotations[i] = Projectile.rotation;
                    trailAlphas[i] = 0f;
                }
                trailInitialized = true;
            }
            if (currentState != oldState)
            {
                if (currentState == SpearState.Homing)
                {
                    Projectile.tileCollide = false;
                    for (int i = 0; i < 12; i++)
                    {
                        Vector2 dustVel = Vector2.UnitX.RotatedBy(MathHelper.TwoPi / 12 * i) * 3f;
                        int dust = Dust.NewDust(Projectile.Center, 0, 0, DustID.YellowTorch, dustVel.X, dustVel.Y, 0, default, 1.8f);
                        Main.dust[dust].noGravity = true;
                    }
                }
                else if (currentState == SpearState.Flying)
                {
                    Projectile.tileCollide = true;
                }
                stateTimer = 0f;
                oldState = currentState;
            }
            if (currentState == SpearState.StuckInTile)
            {
                AI_StuckInTile();
                return;
            }
            UpdateTrail();
            if (Projectile.velocity.Length() > 0.1f)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            }
            SpawnAmbientDust();
            switch (currentState)
            {
                case SpearState.Flying:
                    AI_Flying();
                    break;
                case SpearState.SlowingDown:
                    AI_SlowingDown();
                    break;
                case SpearState.Retreating:
                    AI_Retreating();
                    break;
                case SpearState.Homing:
                    AI_Homing();
                    break;
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (currentState != SpearState.StuckInTile)
            {
                currentState = SpearState.StuckInTile;
                if (Projectile.owner == Main.myPlayer) Projectile.netUpdate = true;
                stuckInTile = true;
                stuckTimer = 0f;
                stuckRotation = Projectile.rotation;
                stuckPosition = Projectile.Center;
                Projectile.velocity = Vector2.Zero;
                Projectile.tileCollide = false;
                Projectile.timeLeft = (int)EXPLOSION_DELAY + 10;
                SpawnStickEffect();
                SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
            }
            return false;
        }
        private void AI_StuckInTile()
        {
            Projectile.Center = stuckPosition;
            Projectile.rotation = stuckRotation;
            Projectile.velocity = Vector2.Zero;
            stuckTimer++;
            float explosionProgress = stuckTimer / EXPLOSION_DELAY;
            float pulseIntensity = explosionProgress * 2f;
            float shakeMagnitude = explosionProgress * 2f;
            if (stuckTimer > EXPLOSION_DELAY * 0.4f)
            {
                Vector2 shake = Main.rand.NextVector2Circular(shakeMagnitude, shakeMagnitude);
                Projectile.Center = stuckPosition + shake;
            }
            if (Main.rand.NextFloat() < explosionProgress)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(2f, 2f);
                float scale = 0.8f + explosionProgress * 1.2f + (float)Math.Sin(pulseTimer * 6f) * 0.3f;
                int dust = Dust.NewDust(Projectile.Center - new Vector2(8), 16, 16, DustID.YellowTorch, dustVel.X, dustVel.Y, 100, default, scale);
                Main.dust[dust].noGravity = true;
            }
            if (stuckTimer > EXPLOSION_DELAY * 0.5f && Main.rand.NextBool(3))
            {
                Vector2 sparkVel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(1f, 4f) * explosionProgress;
                int dust = Dust.NewDust(Projectile.Center, 0, 0, DustID.GoldFlame, sparkVel.X, sparkVel.Y, 0, default, 1.2f);
                Main.dust[dust].noGravity = true;
            }
            Projectile.light = 0.4f + explosionProgress * 0.8f;
            if (stuckTimer >= EXPLOSION_DELAY)
            {
                Explode();
                Projectile.Kill();
            }
        }
        private void Explode()
        {
            int dustCount = 40;
            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi / dustCount * i;
                float speed = 4f + Main.rand.NextFloat() * 6f;
                Vector2 vel = Vector2.UnitX.RotatedBy(angle) * speed;
                int dustType = Main.rand.NextBool() ? DustID.YellowTorch : DustID.GoldFlame;
                int dust = Dust.NewDust(Projectile.Center, 0, 0, dustType, vel.X, vel.Y, 0, default, 2.5f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].scale = 1.5f + Main.rand.NextFloat() * 1.5f;
            }
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi / 20 * i;
                float speed = 2f + Main.rand.NextFloat() * 2f;
                Vector2 vel = Vector2.UnitX.RotatedBy(angle) * speed;
                int dust = Dust.NewDust(Projectile.Center, 0, 0, DustID.Torch, vel.X, vel.Y, 0, default, 2.0f);
                Main.dust[dust].noGravity = true;
            }
            for (int i = 0; i < 15; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(8f, 8f);
                int goreType = Main.rand.Next(61, 64);
                Gore.NewGore(Projectile.GetSource_Death(), Projectile.Center, vel, goreType);
            }
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item74, Projectile.Center);
            float explosionDamage = Projectile.damage * (1.5f + Projectile.ai[0] * 0.2f);
            Player owner = Main.player[Projectile.owner];
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.life <= 0)
                    continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist <= EXPLOSION_RADIUS)
                {
                    float damageFalloff = 1f - (dist / EXPLOSION_RADIUS) * 0.4f;
                    int finalDamage = (int)(explosionDamage * damageFalloff);
                    Vector2 hitDirection = npc.Center - Projectile.Center;
                    int dir = hitDirection.X > 0 ? 1 : -1;
                    npc.StrikeNPC(npc.CalculateHitInfo(finalDamage, dir, false, Projectile.knockBack * 2f, Projectile.DamageType, true));
                    npc.AddBuff(BuffID.OnFire, 300);
                    npc.AddBuff(BuffID.OnFire3, 180);
                    if (Main.netMode != NetmodeID.SinglePlayer)
                    {
                        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, i);
                    }
                }
            }
        }
        private void SpawnStickEffect()
        {
            for (int i = 0; i < 12; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                int dust = Dust.NewDust(Projectile.Center, 0, 0, DustID.YellowTorch, vel.X, vel.Y, 0, default, 1.5f);
                Main.dust[dust].noGravity = true;
            }
            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = -Projectile.oldVelocity.SafeNormalize(Vector2.UnitY).RotatedByRandom(0.5f) * Main.rand.NextFloat(1f, 3f);
                int dust = Dust.NewDust(Projectile.Center, 0, 0, DustID.GoldFlame, vel.X, vel.Y, 0, default, 1.0f);
                Main.dust[dust].noGravity = false;
            }
        }
        private void AI_Flying()
        {
            if (Projectile.timeLeft < 540)
            {
                Projectile.velocity *= 0.998f;
            }
        }
        private void AI_SlowingDown()
        {
            stateTimer++;
            Projectile.velocity *= 0.88f;
            if (Main.rand.NextBool(2))
            {
                Vector2 dustVel = Projectile.velocity.RotatedByRandom(MathHelper.Pi) * 0.3f;
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.YellowTorch, dustVel.X, dustVel.Y, 100, default, 1.5f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].scale = 1.2f + (float)Math.Sin(pulseTimer * 3f) * 0.3f;
            }
            if (stateTimer >= SLOWDOWN_DURATION)
            {
                currentState = SpearState.Retreating;
                stateTimer = 0f;
                oldState = currentState; 
                if (targetNPC >= 0 && targetNPC < Main.maxNPCs && Main.npc[targetNPC].active)
                {
                    retreatDirection = Projectile.Center - Main.npc[targetNPC].Center;
                    if (retreatDirection.Length() > 0)
                        retreatDirection.Normalize();
                    else
                        retreatDirection = -Projectile.velocity.SafeNormalize(Vector2.UnitY);
                }
                else
                {
                    retreatDirection = -Projectile.velocity.SafeNormalize(Vector2.UnitY);
                }
            }
        }
        private void AI_Retreating()
        {
            stateTimer++;
            float retreatTime = 15f;
            float progress = stateTimer / retreatTime;
            float retreatSpeed = RETREAT_DISTANCE / retreatTime * (1f - progress * 0.5f);
            Projectile.velocity = retreatDirection * retreatSpeed;
            if (Main.rand.NextBool(2))
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame, 0f, 0f, 150, default, 1.0f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0.3f;
            }
            if (stateTimer >= retreatTime)
            {
                if (Projectile.owner == Main.myPlayer)
                {
                    int newTarget = FindNewTarget();
                    if (newTarget >= 0)
                    {
                        targetNPC = newTarget;
                        currentState = SpearState.Homing;
                    }
                    else
                    {
                        currentState = SpearState.Flying;
                        Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * 8f;
                    }
                    Projectile.netUpdate = true;
                }
            }
        }
        private void AI_Homing()
        {
            if (targetNPC < 0 || targetNPC >= Main.maxNPCs || !Main.npc[targetNPC].active || Main.npc[targetNPC].life <= 0)
            {
                if (Projectile.owner == Main.myPlayer)
                {
                    int newTarget = FindNewTarget();
                    if (newTarget >= 0)
                    {
                        targetNPC = newTarget;
                        Projectile.netUpdate = true;
                    }
                    else
                    {
                        currentState = SpearState.Flying;
                        Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * 8f;
                        Projectile.netUpdate = true;
                    }
                }
                return;
            }
            NPC target = Main.npc[targetNPC];
            Vector2 dirToTarget = target.Center - Projectile.Center;
            float distance = dirToTarget.Length();
            if (distance > 0)
                dirToTarget.Normalize();
            float homingSpeed = 12f + Projectile.ai[0] * 1.5f;
            float turnSpeed = 0.15f + Projectile.ai[0] * 0.02f;
            homingSpeed = Math.Min(homingSpeed, 22f);
            turnSpeed = Math.Min(turnSpeed, 0.35f);
            Vector2 desiredVelocity = dirToTarget * homingSpeed;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, turnSpeed);
            if (Projectile.velocity.Length() > homingSpeed)
            {
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * homingSpeed;
            }
            if (Main.rand.NextBool(3))
            {
                float dustScale = 1.0f + (float)Math.Sin(pulseTimer * 4f) * 0.5f;
                int dust = Dust.NewDust(Projectile.Center - new Vector2(4), 8, 8, DustID.YellowTorch, 0f, 0f, 100, default, dustScale);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity = -Projectile.velocity * 0.1f;
            }
        }
        private int FindNewTarget()
        {
            float closestDist = MAX_SEARCH_RANGE;
            int closestNPC = -1;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.life <= 0 || npc.dontTakeDamage)
                    continue;
                if (hitNPCs.Contains(i))
                    continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    if (Collision.CanHitLine(Projectile.Center, 1, 1, npc.position, npc.width, npc.height))
                    {
                        closestDist = dist;
                        closestNPC = i;
                    }
                }
            }
            return closestNPC;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                if (!hitNPCs.Contains(target.whoAmI))
                {
                    hitNPCs.Add(target.whoAmI);
                }
                Projectile.ai[0]++;
                targetNPC = target.whoAmI;
                currentState = SpearState.SlowingDown;
                Projectile.netUpdate = true;
            }
            SpawnHitEffect(target.Center);
            SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.2f + Projectile.ai[0] * 0.1f }, Projectile.Center);
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            float damageMultiplier = 1f + Projectile.ai[0] * 0.25f;
            modifiers.SourceDamage *= damageMultiplier;
        }
        private void SpawnHitEffect(Vector2 position)
        {
            int particleCount = 16 + (int)(Projectile.ai[0] * 4);
            for (int i = 0; i < particleCount; i++)
            {
                float angle = MathHelper.TwoPi / particleCount * i;
                float speed = 6f + Main.rand.NextFloat() * 4f;
                Vector2 vel = Vector2.UnitX.RotatedBy(angle) * speed;
                
                
                Color sparkColor = Main.rand.NextBool() ? Color.Yellow : Color.Red;
                float scale = 0.6f + Projectile.ai[0] * 0.1f;
                new Spark(position, vel, sparkColor, Main.rand.Next(20, 35), scale, 0.9f).Spawn();
            }
            for (int i = 0; i < 12; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(8f, 8f);
                Color sparkColor = Main.rand.NextBool() ? Color.Orange : Color.Gold;
                new Spark(position, vel, sparkColor, Main.rand.Next(15, 25), 0.5f, 0.8f).Spawn();
            }
        }
        private void SpawnAmbientDust()
        {
            if (Main.rand.NextBool(2))
            {
                Vector2 offset = Main.rand.NextVector2Circular(4f, 4f);
                Vector2 dustPos = Projectile.Center + offset;
                float pulseFactor = (float)Math.Sin(pulseTimer * 3f) * 0.5f + 0.5f;
                int dust = Dust.NewDust(dustPos, 0, 0, DustID.YellowTorch, -Projectile.velocity.X * 0.15f, -Projectile.velocity.Y * 0.15f, 100, default, 0.8f + pulseFactor * 0.5f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].fadeIn = 1.2f;
            }
            if (Main.rand.NextBool(3))
            {
                Vector2 perpendicular = new Vector2(-Projectile.velocity.Y, Projectile.velocity.X);
                if (perpendicular.Length() > 0)
                    perpendicular.Normalize();
                float sideOffset = Main.rand.NextFloat(-8f, 8f);
                Vector2 dustPos = Projectile.Center + perpendicular * sideOffset;
                int dust = Dust.NewDust(dustPos, 0, 0, DustID.GoldFlame, -Projectile.velocity.X * 0.05f, -Projectile.velocity.Y * 0.05f, 150, default, 0.6f);
                Main.dust[dust].noGravity = true;
            }
        }
        private void UpdateTrail()
        {
            for (int i = TRAIL_LENGTH - 1; i > 0; i--)
            {
                trailPositions[i] = trailPositions[i - 1];
                trailRotations[i] = trailRotations[i - 1];
                trailAlphas[i] = trailAlphas[i - 1] * 0.92f;
            }
            trailPositions[0] = Projectile.Center;
            trailRotations[0] = Projectile.rotation;
            trailAlphas[0] = 1f;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Texture2D pixelTexture = TextureAssets.MagicPixel.Value;
            Vector2 origin = texture.Size() / 2f;
            SpriteBatch spriteBatch = Main.spriteBatch;
            if (currentState == SpearState.StuckInTile)
            {
                DrawStuckSpear(spriteBatch, texture, pixelTexture, origin);
                return false;
            }

            DrawSpearTrail();

            Color drawColor = Lighting.GetColor((int)(Projectile.Center.X / 16), (int)(Projectile.Center.Y / 16));
            drawColor = Color.Lerp(drawColor, Color.White, 0.5f);
            spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, drawColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }

        private void DrawSpearTrail()
        {
            if (trailPositions == null || trailPositions.Length < 2 || !trailInitialized) return;
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Effect pixelShader = Main.pixelShader;

            List<Vector2> validTrail = new List<Vector2>();
            for(int i = 0; i < TRAIL_LENGTH; i++) {
                if (trailPositions[i] != Vector2.Zero && trailAlphas[i] > 0.01f) {
                    validTrail.Add(trailPositions[i]);
                }
            }
            if (validTrail.Count >= 2) {
                validTrail.Reverse();
                TrailRenderer.Draw(
                    pixelShader, 
                    validTrail, 
                    t => MathHelper.Lerp(0f, 12f, t), 
                    t => Color.Lerp(Color.Red, Color.Yellow, t) * t * 0.9f, 
                    HealShaders.HealNoise 
                );
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        private void DrawStuckSpear(SpriteBatch spriteBatch, Texture2D texture, Texture2D pixelTexture, Vector2 origin)
        {
            float explosionProgress = stuckTimer / EXPLOSION_DELAY;
            float pulse = (float)Math.Sin(pulseTimer * 6f) * 0.5f + 0.5f;
            Color outerGlow = GlowYellowDark * (0.2f + explosionProgress * 0.5f) * pulse;
            float outerScale = Projectile.scale * (1.6f + explosionProgress * 0.8f + pulse * 0.3f);
            spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, outerGlow, stuckRotation, origin, outerScale, SpriteEffects.None, 0f);
            Color midGlow = GlowYellowMid * (0.3f + explosionProgress * 0.4f);
            float midScale = Projectile.scale * (1.3f + explosionProgress * 0.4f + pulse * 0.15f);
            spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, midGlow, stuckRotation, origin, midScale, SpriteEffects.None, 0f);
            Color drawColor = Color.Lerp(Color.White, GlowYellowBright, explosionProgress * 0.5f);
            spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, drawColor, stuckRotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            if (explosionProgress > 0.3f)
            {
                float rayAlpha = (explosionProgress - 0.3f) / 0.7f;
                int rayCount = 6;
                float rayLength = 15f + rayAlpha * 40f + pulse * 15f;
                float rayWidth = 1.5f + pulse * 1.5f;
                Color rayColor = GlowYellowBright * rayAlpha * 0.6f;
                Rectangle sourceRect = new Rectangle(0, 0, 1, 1);
                for (int r = 0; r < rayCount; r++)
                {
                    float rayAngle = MathHelper.TwoPi / rayCount * r + pulseTimer * 0.5f;
                    spriteBatch.Draw(pixelTexture, Projectile.Center - Main.screenPosition, sourceRect, rayColor, rayAngle, new Vector2(0f, 0.5f), new Vector2(rayLength, rayWidth), SpriteEffects.None, 0f);
                }
            }
            if (explosionProgress > 0.6f)
            {
                float ringProgress = (explosionProgress - 0.6f) / 0.4f;
                float ringRadius = EXPLOSION_RADIUS * ringProgress * 0.3f;
                int segments = 36;
                Color ringColor = GlowYellowBright * (1f - ringProgress) * 0.3f;
                Rectangle srcRect = new Rectangle(0, 0, 1, 1);
                for (int s = 0; s < segments; s++)
                {
                    float angle = MathHelper.TwoPi / segments * s;
                    float nextAngle = MathHelper.TwoPi / segments * (s + 1);
                    Vector2 start = Projectile.Center + Vector2.UnitX.RotatedBy(angle) * ringRadius;
                    Vector2 end = Projectile.Center + Vector2.UnitX.RotatedBy(nextAngle) * ringRadius;
                    Vector2 diff = end - start;
                    float segLen = diff.Length();
                    float segAngle = diff.ToRotation();
                    spriteBatch.Draw(pixelTexture, start - Main.screenPosition, srcRect, ringColor, segAngle, new Vector2(0f, 0.5f), new Vector2(segLen, 2f), SpriteEffects.None, 0f);
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (stuckInTile)
                return;
            int dustCount = 15 + (int)(Projectile.ai[0] * 5);
            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi / dustCount * i;
                float speed = 3f + Main.rand.NextFloat() * 4f;
                Vector2 vel = Vector2.UnitX.RotatedBy(angle) * speed;
                int dustType = Main.rand.NextBool() ? DustID.YellowTorch : DustID.GoldFlame;
                int dust = Dust.NewDust(Projectile.Center, 0, 0, dustType, vel.X, vel.Y, 0, default, 1.8f);
                Main.dust[dust].noGravity = true;
            }
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                int dust = Dust.NewDust(Projectile.Center, 0, 0, DustID.Torch, vel.X, vel.Y, 0, default, 1.5f);
                Main.dust[dust].noGravity = true;
            }
            SoundEngine.PlaySound(SoundID.Item27, Projectile.Center);
        }
        public override Color? GetAlpha(Color lightColor)
        {
            float glow = 0.6f + (float)Math.Sin(pulseTimer * 3f) * 0.2f + Projectile.ai[0] * 0.05f;
            glow = Math.Min(glow, 1f);
            return Color.Lerp(lightColor, GlowWhiteYellow, glow);
        }
    }
}
