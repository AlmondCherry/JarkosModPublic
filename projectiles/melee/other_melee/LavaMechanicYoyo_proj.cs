using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.other_melee
{
    public class LavaMechanicYoyo_proj : ModProjectile
    {
        private enum DiscState
        {
            Flying = 0, Sawing = 1, PassingThrough = 2, Returning = 3
        }
        private DiscState CurrentState
        {
            get => (DiscState)Projectile.ai[0];
            set => Projectile.ai[0] = (float)value;
        }
        private float SawCounter
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        private int targetNPCIndex = -1;
        private int sawTickTimer = 0;
        private const int SAW_INTERVAL = 16;
        private const int MAX_SAWS = 2;
        private const float MAX_FLIGHT_DISTANCE = 600f;
        private const float PASS_THROUGH_DISTANCE = 280f;
        private Vector2 startPosition;
        private Vector2 passThroughDirection;
        private float passThroughTraveled = 0f;
        private List<Vector2> trailPositions = new List<Vector2>();
        private List<float> trailRotations = new List<float>();
        private const int TRAIL_LENGTH = 25;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 44;
            Projectile.height = 44;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 900;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.extraUpdates = 1;
        }
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (Projectile.localAI[0] == 0f)
            {
                startPosition = Projectile.Center;
                Projectile.localAI[0] = 1f;
                InitializeTrail();
            }
            float rotationSpeed = CurrentState == DiscState.Sawing ? 0.5f : 0.25f;
            Projectile.rotation += rotationSpeed * Math.Sign(Projectile.velocity.X + 0.01f);
            UpdateTrail();
            SpawnAmbientDust();
            switch (CurrentState)
            {
                case DiscState.Flying:
                    HandleFlyingState(owner);
                    break;
                case DiscState.Sawing:
                    HandleSawingState(owner);
                    break;
                case DiscState.PassingThrough:
                    HandlePassingThroughState();
                    break;
                case DiscState.Returning:
                    HandleReturningState(owner);
                    break;
            }
            Lighting.AddLight(Projectile.Center, 0.3f, 0.5f, 0.8f);
        }
        private void InitializeTrail()
        {
            trailPositions.Clear();
            trailRotations.Clear();
            for (int i = 0; i < TRAIL_LENGTH; i++)
            {
                trailPositions.Add(Projectile.Center);
                trailRotations.Add(Projectile.rotation);
            }
        }
        private void UpdateTrail()
        {
            trailPositions.Insert(0, Projectile.Center);
            trailRotations.Insert(0, Projectile.rotation);
            if (trailPositions.Count > TRAIL_LENGTH)
            {
                trailPositions.RemoveAt(trailPositions.Count - 1);
                trailRotations.RemoveAt(trailRotations.Count - 1);
            }
        }
        private void SpawnAmbientDust()
        {
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f, 100, default, 0.7f);
                dust.noGravity = true;
                dust.velocity = Projectile.velocity.RotatedByRandom(0.3) * 0.2f;
            }
            if (Main.rand.NextBool(5))
            {
                Vector2 offset = Main.rand.NextVector2Circular(Projectile.width / 2, Projectile.height / 2);
                Dust Spark = Dust.NewDustDirect(Projectile.Center + offset, 0, 0, DustID.Iron, 0, 0, 0, default, 0.6f);
                Spark.noGravity = true;
                Spark.velocity = offset.SafeNormalize(Vector2.Zero) * 2f;
            }
        }
        private void HandleFlyingState(Player owner)
        {
            float distanceFromStart = Vector2.Distance(Projectile.Center, startPosition);
            if (distanceFromStart >= MAX_FLIGHT_DISTANCE)
            {
                TransitionToReturning();
                return;
            }
            SearchForTarget();
            if (Projectile.soundDelay <= 0)
            {
                SoundEngine.PlaySound(SoundID.Item15 with { Volume = 0.4f, Pitch = 0.8f }, Projectile.Center);
                Projectile.soundDelay = 20;
            }
        }
        private void SearchForTarget()
        {
            float detectionRadius = 80f;
            float closestDist = detectionRadius;
            int closestNPC = -1;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.CanBeChasedBy() && !npc.dontTakeDamage)
                {
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
                targetNPCIndex = closestNPC;
                CurrentState = DiscState.Sawing;
                SawCounter = 0;
                sawTickTimer = 0;
                SoundEngine.PlaySound(SoundID.Item22 with { Pitch = 0.5f }, Projectile.Center);
            }
        }
        private void HandleSawingState(Player owner)
        {
            if (targetNPCIndex == -1 || targetNPCIndex >= Main.maxNPCs || !Main.npc[targetNPCIndex].active || Main.npc[targetNPCIndex].dontTakeDamage)
            {
                TransitionToReturning();
                return;
            }
            NPC target = Main.npc[targetNPCIndex];
            Vector2 toTarget = target.Center - Projectile.Center;
            float distance = toTarget.Length();
            if (distance > 10f)
            {
                toTarget.Normalize();
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 12f, 0.3f);
            }
            else
            {
                Projectile.velocity = Main.rand.NextVector2Circular(3f, 3f);
            }
            sawTickTimer++;
            if (sawTickTimer >= SAW_INTERVAL)
            {
                sawTickTimer = 0;
                SawCounter++;
                PerformSawCut(target);
                if (SawCounter >= MAX_SAWS)
                {
                    TransitionToPassingThrough(target);
                }
            }
            SpawnSawingEffects(target);
        }
        private void PerformSawCut(NPC target)
        {
            SoundEngine.PlaySound(SoundID.Item22 with {
                Volume = 0.8f, Pitch = -0.2f + Main.rand.NextFloat(0.1f)
            }, Projectile.Center);
            if (Main.myPlayer == Projectile.owner)
            {
                Main.LocalPlayer.GetModPlayer<Screenshake_system>().stronger_shake++;
            }
            int direction = Math.Sign(Projectile.Center.X - target.Center.X);
            if (direction == 0) direction = 1;
            target.SimpleStrikeNPC(Projectile.damage, direction, true, Projectile.knockBack * 0.2f, Projectile.DamageType);
            SpawnBloodEffects(target);
            SpawnSparkEffects();
            if (SawCounter < MAX_SAWS)
            {
                Projectile.velocity *= 0.1f;
            }
        }
        private void SpawnBloodEffects(NPC target)
        {
            for (int i = 0; i < 15; i++)
            {
                Vector2 bloodVel = Main.rand.NextVector2Circular(8f, 8f);
                Dust blood = Dust.NewDustDirect(target.Center - new Vector2(5, 5), 10, 10, DustID.Blood, bloodVel.X, bloodVel.Y, 0, default, 1.4f);
                blood.noGravity = Main.rand.NextBool();
            }
            if (Main.netMode != NetmodeID.Server)
            {
                for (int g = 0; g < 4; g++)
                {
                    Gore gore = Gore.NewGoreDirect(Projectile.GetSource_FromThis(), target.Center, Main.rand.NextVector2Circular(4f, 4f), GoreID.Smoke1 + Main.rand.Next(3), 0.8f);
                }
            }
        }
        private void SpawnSparkEffects()
        {
            for (int i = 0; i < 12; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(6f, 6f);
                Dust Spark = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Torch, sparkVel.X, sparkVel.Y, 0, default, 1.0f);
                Spark.noGravity = true;
            }
            for (int i = 0; i < 6; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(5f, 5f);
                Dust whiteSpark = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Torch, sparkVel.X, sparkVel.Y, 0, default, 1.2f);
                whiteSpark.noGravity = true;
            }
        }
        private void SpawnSawingEffects(NPC target)
        {
            if (Main.rand.NextBool(2))
            {
                Vector2 sparkDir = Projectile.rotation.ToRotationVector2();
                Dust Spark = Dust.NewDustDirect(Projectile.Center + sparkDir * Projectile.width / 2, 0, 0, DustID.Torch, sparkDir.X * 5f, sparkDir.Y * 5f, 0, default, 0.8f);
                Spark.noGravity = true;
            }
            if (Main.rand.NextBool(3))
            {
                Dust blood = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Blood, Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-2, 2), 0, default, 1.0f);
            }
        }
        private void TransitionToPassingThrough(NPC target)
        {
            CurrentState = DiscState.PassingThrough;
            passThroughDirection = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            if (passThroughDirection == Vector2.Zero)
            {
                passThroughDirection = (Projectile.Center - Main.player[Projectile.owner].Center).SafeNormalize(Vector2.UnitX);
            }
            Projectile.velocity = passThroughDirection * 18f;
            passThroughTraveled = 0f;
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.7f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.NPCDeath1 with { Volume = 0.5f }, target.Center);
            for (int i = 0; i < 30; i++)
            {
                Vector2 bloodVel = Main.rand.NextVector2Circular(10f, 10f);
                Dust blood = Dust.NewDustDirect(target.Center, 0, 0, DustID.Blood, bloodVel.X, bloodVel.Y, 0, default, 1.8f);
                blood.noGravity = Main.rand.NextBool(3);
            }
            if (Main.myPlayer == Projectile.owner)
            {
                Main.LocalPlayer.GetModPlayer<Screenshake_system>().stronger_shake++;
            }
        }
        private void HandlePassingThroughState()
        {
            passThroughTraveled += Projectile.velocity.Length();
            Projectile.velocity *= 0.98f;
            if (passThroughTraveled >= PASS_THROUGH_DISTANCE || Projectile.velocity.Length() < 5f)
            {
                TransitionToReturning();
            }
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, 0, 0, 100, default, 0.8f);
                d.velocity = -Projectile.velocity * 0.1f;
            }
        }
        private void TransitionToReturning()
        {
            CurrentState = DiscState.Returning;
            targetNPCIndex = -1;
            SoundEngine.PlaySound(SoundID.Item10 with { Pitch = 0.2f }, Projectile.Center);
        }
        private void HandleReturningState(Player owner)
        {
            Vector2 toOwner = owner.Center - Projectile.Center;
            float distance = toOwner.Length();
            if (distance < 40f)
            {
                SoundEngine.PlaySound(SoundID.Item7, Projectile.Center);
                Projectile.Kill();
                return;
            }
            float returnSpeed = MathHelper.Lerp(15f, 28f, 1f - (distance / 800f));
            returnSpeed = MathHelper.Clamp(returnSpeed, 15f, 28f);
            toOwner.Normalize();
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, toOwner * returnSpeed, 0.15f);
            float maxSpeed = 30f;
            if (Projectile.velocity.Length() > maxSpeed)
            {
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * maxSpeed;
            }
            if (Main.rand.NextBool(4))
            {
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Torch, 0, 0, 150, default, 0.6f);
                d.noGravity = true;
                d.velocity = -Projectile.velocity * 0.1f;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            DrawBeautifulTrail(spriteBatch);
            DrawGlow(spriteBatch);
            DrawMainDisc(spriteBatch, lightColor);
            return false;
        }
        private void DrawBeautifulTrail(SpriteBatch spriteBatch)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            for (int i = 1; i < trailPositions.Count; i++)
            {
                Vector2 currentPos = trailPositions[i - 1];
                Vector2 previousPos = trailPositions[i];
                Vector2 direction = currentPos - previousPos;
                float length = direction.Length();
                if (length < 1f) continue;
                float rotation = direction.ToRotation();
                float progress = (float)i / trailPositions.Count;
                float alpha = 1f - progress;
                alpha = MathF.Pow(alpha, 0.7f);
                float baseWidth = 12f;
                float width = baseWidth * alpha;
                Color outerColor = Color.Lerp(Color.OliveDrab, Color.IndianRed, progress) * alpha * 0.4f;
                Color mainColor = Color.Lerp(Color.Orange, Color.OrangeRed, progress) * alpha * 0.8f;
                Color coreColor = Color.Lerp(Color.Yellow, Color.Red, progress) * alpha;
                Vector2 drawPos = previousPos - Main.screenPosition;
                Vector2 origin = new Vector2(0, 0.5f);
                spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), outerColor, rotation, origin, new Vector2(length, width * 3f), SpriteEffects.None, 0f);
                spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), mainColor, rotation, origin, new Vector2(length, width * 1.5f), SpriteEffects.None, 0f);
                spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), coreColor, rotation, origin, new Vector2(length, width * 0.6f), SpriteEffects.None, 0f);
            }
            DrawTrailSparkles(spriteBatch, pixel);
        }
        private void DrawTrailSparkles(SpriteBatch spriteBatch, Texture2D pixel)
        {
            for (int i = 0; i < trailPositions.Count; i += 3)
            {
                float progress = (float)i / trailPositions.Count;
                float alpha = (1f - progress) * (0.5f + 0.5f * MathF.Sin(Main.GameUpdateCount * 0.3f + i));
                Vector2 sparklePos = trailPositions[i] - Main.screenPosition;
                float sparkleSize = 4f * (1f - progress);
                Color sparkleColor = Color.White * alpha;
                spriteBatch.Draw(pixel, sparklePos, new Rectangle(0, 0, 1, 1), sparkleColor, 0f, new Vector2(0.5f, 0.5f), sparkleSize, SpriteEffects.None, 0f);
            }
        }
        private void DrawGlow(SpriteBatch spriteBatch)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = 1f + 0.1f * MathF.Sin(Main.GameUpdateCount * 0.15f);
            float glowScale = Projectile.scale * 1.3f * pulse;
            Color glowColor = CurrentState switch
            {
                DiscState.Sawing => Color.Red * 0.5f, DiscState.PassingThrough => Color.Red * 0.5f, _ => Color.Red * 0.5f
            };
            for (int i = 0; i < 6; i++)
            {
                Vector2 offset = (MathHelper.TwoPi * i / 6f).ToRotationVector2() * 3f;
                spriteBatch.Draw(texture, drawPos + offset, null, glowColor * 0.3f, Projectile.rotation, origin, glowScale, SpriteEffects.None, 0f);
            }
            spriteBatch.Draw(texture, drawPos, null, glowColor * 0.5f, Projectile.rotation, origin, glowScale * 1.1f, SpriteEffects.None, 0f);
        }
        private void DrawMainDisc(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            spriteBatch.Draw(texture, drawPos, null, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            Color highlightColor = Color.White * 0.4f;
            spriteBatch.Draw(texture, drawPos, null, highlightColor, Projectile.rotation, origin, Projectile.scale * 0.85f, SpriteEffects.None, 0f);
        }
        public override bool? CanHitNPC(NPC target)
        {
            if (CurrentState == DiscState.Sawing && target.whoAmI == targetNPCIndex)
                return false;
            return null;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (CurrentState != DiscState.Sawing)
            {
                for (int i = 0; i < 5; i++)
                {
                    Dust.NewDust(target.position, target.width, target.height, DustID.Blood, hit.HitDirection * 2, -2f, 0, default, 1.2f);
                }
            }
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.5f }, Projectile.Center);
            for (int i = 0; i < 5; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(4f, 4f);
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Torch, dustVel.X, dustVel.Y, 100, default, 0.35f);
                d.noGravity = true;
            }
        }
	}
}
