using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.npc.Night
{
	public class SteamDrone : ModNPC
	{
        private enum AIState
        {
            Hovering, Aiming, Firing, Repositioning
        }
        private AIState State
        {
            get => (AIState)NPC.ai[0];
            set => NPC.ai[0] = (float)value;
        }
        private float StateTimer
        {
            get => NPC.ai[1];
            set => NPC.ai[1] = value;
        }
        private float HoverOffset
        {
            get => NPC.ai[2];
            set => NPC.ai[2] = value;
        }
        private int RocketsInBurst
        {
            get => (int)NPC.ai[3];
            set => NPC.ai[3] = value;
        }
        private float glowIntensity = 0f;
        private float eyeGlowPulse = 0f;
        private float thrusterFlame = 0f;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 4;
            NPCID.Sets.TrailingMode[NPC.type] = 1;
            NPCID.Sets.TrailCacheLength[NPC.type] = 6;
            NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                Velocity = 1f, Direction = -1
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, value);
        }
        public override void SetDefaults()
        {
            NPC.width = 92;
            NPC.height = 48;
            NPC.damage = 30;
            NPC.defense = 10;
            NPC.lifeMax = 500;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.NPCDeath14;
            NPC.value = Item.buyPrice(gold: 5);
            NPC.knockBackResist = 0.1f;
            NPC.aiStyle = -1;
            NPC.noGravity = true;
            NPC.noTileCollide = false;
            NPC.lavaImmune = true;
            NPC.buffImmune[BuffID.Poisoned] = true;
            NPC.buffImmune[BuffID.OnFire] = true;
            NPC.buffImmune[BuffID.Confused] = true;
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime, BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface, new FlavorTextBestiaryInfoElement("...")
            });
        }
        public override void AI()
        {
            Player target = Main.player[NPC.target];
            if (!target.active || target.dead || Vector2.Distance(NPC.Center, target.Center) > 2000f)
            {
                NPC.TargetClosest(false);
                target = Main.player[NPC.target];
                if (!target.active || target.dead)
                {
                    NPC.velocity.Y -= 0.5f;
                    NPC.EncourageDespawn(10);
                    if (NPC.timeLeft > 10) NPC.timeLeft = 10;
                    if (NPC.position.Y < 0 || NPC.timeLeft <= 1)
                    {
                        NPC.active = false;
                        NPC.netUpdate = true;
                    }
                    return;
                }
            }
            NPC.spriteDirection = NPC.Center.X < target.Center.X ? 1 : -1;
            UpdateVisualEffects();
            switch (State)
            {
                case AIState.Hovering:
                    AI_Hovering(target);
                    break;
                case AIState.Aiming:
                    AI_Aiming(target);
                    break;
                case AIState.Firing:
                    AI_Firing(target);
                    break;
                case AIState.Repositioning:
                    AI_Repositioning(target);
                    break;
            }
            SpawnThrusterParticles();
        }
        private void AI_Hovering(Player target)
        {
            StateTimer++;
            HoverOffset += 0.05f;
            Vector2 desiredPos = target.Center + new Vector2(NPC.spriteDirection * -300f, -200f + (float)Math.Sin(HoverOffset) * 50f);
            Vector2 moveDir = desiredPos - NPC.Center;
            float distance = moveDir.Length();
            if (distance > 10f)
            {
                moveDir.Normalize();
                float speed = Math.Min(distance / 20f, 8f);
                NPC.velocity = Vector2.Lerp(NPC.velocity, moveDir * speed, 0.1f);
            }
            if (StateTimer > 120 && distance < 100f)
            {
                State = AIState.Aiming;
                StateTimer = 0;
                glowIntensity = 0f;
            }
        }
        private void AI_Aiming(Player target)
        {
            StateTimer++;
            glowIntensity = Math.Min(glowIntensity + 0.02f, 1f);
            NPC.velocity *= 0.95f;
            if (StateTimer == 1)
            {
                SoundEngine.PlaySound(SoundID.Item15 with { Pitch = -0.3f }, NPC.Center);
            }
            if (StateTimer % 5 == 0)
            {
                Vector2 dustPos = NPC.Center + new Vector2(NPC.spriteDirection * 30f, 0);
                for (int i = 0; i < 3; i++)
                {
                    Dust dust = Dust.NewDustPerfect(dustPos + Main.rand.NextVector2Circular(20f, 20f), DustID.Electric, Vector2.Zero, 0, default, 0.8f);
                    dust.noGravity = true;
                    dust.velocity = (dustPos - dust.position) * 0.1f;
                }
            }
            if (StateTimer > 60)
            {
                State = AIState.Firing;
                StateTimer = 0;
                RocketsInBurst = 0;
            }
        }
        private void AI_Firing(Player target)
        {
            StateTimer++;
            int maxRockets = Main.expertMode ? 5 : 3;
            if (StateTimer % 15 == 0 && RocketsInBurst < maxRockets)
            {
                FireRocket(target);
                RocketsInBurst++;
            }
            NPC.velocity -= new Vector2(NPC.spriteDirection * 0.5f, 0);
            if (RocketsInBurst >= maxRockets)
            {
                State = AIState.Repositioning;
                StateTimer = 0;
            }
        }
        private void AI_Repositioning(Player target)
        {
            StateTimer++;
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            Vector2 desiredPos = target.Center + new Vector2((float)Math.Cos(angle) * 350f, (float)Math.Sin(angle) * 200f - 150f);
            Vector2 moveDir = desiredPos - NPC.Center;
            if (moveDir.Length() > 10f)
            {
                moveDir.Normalize();
                NPC.velocity = Vector2.Lerp(NPC.velocity, moveDir * 12f, 0.08f);
            }
            if (StateTimer > 90)
            {
                State = AIState.Hovering;
                StateTimer = 0;
            }
        }
        private void FireRocket(Player target)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;
            Vector2 spawnPos = NPC.Center + new Vector2(NPC.spriteDirection * 35f, 5f);
            Vector2 velocity = (target.Center - spawnPos).SafeNormalize(Vector2.UnitX) * 8f;
            velocity = velocity.RotatedByRandom(MathHelper.ToRadians(15f));
            int proj = Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, velocity, ModContent.ProjectileType<SmartRocket>(), NPC.damage / 2, 3f, Main.myPlayer, target.whoAmI);
            SoundEngine.PlaySound(SoundID.Item92, NPC.Center);
            for (int i = 0; i < 15; i++)
            {
                Dust dust = Dust.NewDustPerfect(spawnPos, DustID.Torch, velocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(0.5f, 1.5f), 0, default, 1.5f);
                dust.noGravity = true;
            }
        }
        private void UpdateVisualEffects()
        {
            eyeGlowPulse += 0.1f;
            thrusterFlame += 0.15f;
        }
        private void SpawnThrusterParticles()
        {
            if (Main.rand.NextBool(2))
            {
                SpawnThrusterDust(NPC.Center + new Vector2(-20f, 35f));
                SpawnThrusterDust(NPC.Center + new Vector2(20f, 35f));
            }
        }
        private void SpawnThrusterDust(Vector2 position)
        {
            Dust dust = Dust.NewDustPerfect(position + Main.rand.NextVector2Circular(5f, 3f), DustID.Torch, new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(3f, 6f)), 0, default, 1.2f);
            dust.noGravity = true;
            dust.fadeIn = 0.5f;
            if (Main.rand.NextBool(3))
            {
                Dust smoke = Dust.NewDustPerfect(position, DustID.Smoke, new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(2f, 4f)), 100, default, 0.8f);
                smoke.noGravity = true;
            }
        }
        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            if (NPC.frameCounter >= 6)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;
                if (NPC.frame.Y >= frameHeight * 4)
                    NPC.frame.Y = 0;
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[NPC.type].Value;
            Vector2 origin = new Vector2(texture.Width / 2, NPC.frame.Height / 2);
            SpriteEffects effects = NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            for (int i = 0; i < NPC.oldPos.Length; i++)
            {
                Vector2 drawPos = NPC.oldPos[i] + NPC.Size / 2f - screenPos;
                Color trailColor = drawColor * (1f - i / (float)NPC.oldPos.Length) * 0.3f;
                spriteBatch.Draw(texture, drawPos, NPC.frame, trailColor, NPC.rotation, origin, NPC.scale, effects, 0f);
            }
            spriteBatch.Draw(texture, NPC.Center - screenPos, NPC.frame, drawColor, NPC.rotation, origin, NPC.scale, effects, 0f);
            DrawEyeGlow(spriteBatch, screenPos);
            if (glowIntensity > 0)
            {
                DrawChargingGlow(spriteBatch, screenPos);
            }
            return false;
        }
        private void DrawEyeGlow(SpriteBatch spriteBatch, Vector2 screenPos)
        {
            float pulse = 0.7f + (float)Math.Sin(eyeGlowPulse) * 0.3f;
            Vector2 eyePos = NPC.Center + new Vector2(NPC.spriteDirection * 15f, -15f) - screenPos;
            Texture2D glowTex = TextureAssets.Extra[ExtrasID.ThePerfectGlow].Value;
            Color eyeColor = Color.Red * pulse;
            spriteBatch.Draw(glowTex, eyePos, null, eyeColor, 0f, glowTex.Size() / 2f, 0.3f * pulse, SpriteEffects.None, 0f);
        }
        private void DrawChargingGlow(SpriteBatch spriteBatch, Vector2 screenPos)
        {
            Vector2 cannonPos = NPC.Center + new Vector2(NPC.spriteDirection * 30f, 0) - screenPos;
            Texture2D glowTex = TextureAssets.Extra[ExtrasID.ThePerfectGlow].Value;
            Color glowColor = Color.OrangeRed * glowIntensity * 0.8f;
            spriteBatch.Draw(glowTex, cannonPos, null, glowColor, 0f, glowTex.Size() / 2f, 0.5f * glowIntensity, SpriteEffects.None, 0f);
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int i = 0; i < 10; i++)
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Electric, hit.HitDirection * 2f, -2f, 0, default, 1f);
                dust.noGravity = true;
            }
            if (NPC.life <= 0)
            {
                SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
                for (int i = 0; i < 50; i++)
                {
                    Vector2 velocity = Main.rand.NextVector2Circular(10f, 10f);
                    Dust dust = Dust.NewDustPerfect(NPC.Center + Main.rand.NextVector2Circular(30f, 40f), Main.rand.NextBool() ? DustID.Torch : DustID.Smoke, velocity, 0, default, Main.rand.NextFloat(1f, 2f));
                    dust.noGravity = true;
                }
                for (int i = 0; i < 20; i++)
                {
                    Gore gore = Gore.NewGoreDirect(NPC.GetSource_Death(), NPC.Center, Main.rand.NextVector2Circular(5f, 5f), GoreID.Smoke1 + Main.rand.Next(3));
                }
            }
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemID.Cog, 1, 5, 15));
            npcLoot.Add(ItemDropRule.Common(ItemID.RocketI, 2, 20, 50));
            npcLoot.Add(ItemDropRule.Common(ItemID.HallowedBar, 4, 2, 5));
            npcLoot.Add(ItemDropRule.Common(ItemID.SoulofFright, 3, 1, 3));
        }
    }
    #region Smart Rocket
    public class SmartRocket : ModProjectile
    {
        public override string Texture => "jarkos/textures/MobsProjectile/Squisher_Proj";
        private const float MaxSpeed = 4f;
        private const float Acceleration = 0.2f;
        private const float TurnSpeed = 0.02f;
        private const float PredictionFactor = 0.12f;
        private const int HomingDelay = 120;
        private Player Target => Main.player[(int)Projectile.ai[0]];
        private float HomingStrength
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        private List<Vector2> trailPositions = new List<Vector2>();
        private const int MaxTrailLength = 20;
        private float engineGlow = 0f;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }
        public override void AI()
        {
            UpdateTrail();
            Projectile.rotation = Projectile.velocity.ToRotation();
            engineGlow = 0.8f + (float)Math.Sin(Projectile.localAI[0] * 0.3f) * 0.2f;

            Projectile.localAI[0]++;
            Lighting.AddLight(Projectile.Center, new Vector3(1f, 0.5f, 0.2f) * engineGlow);

            if (Projectile.localAI[0] < HomingDelay)
            {
                if (Projectile.velocity.Length() < MaxSpeed)
                {
                    Projectile.velocity *= 1.05f;
                }
                SpawnExhaustParticles();
                return;
            }

            PerformSmartHoming();
            SpawnExhaustParticles();
            if (Projectile.localAI[0] % 30 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item13 with { Volume = 0.3f, Pitch = 0.5f }, Projectile.Center);
            }
        }
        private void PerformSmartHoming()
        {
            if (!Target.active || Target.dead)
            {
                return;
            }
            Vector2 targetPos = Target.Center;
            Vector2 targetVel = Target.velocity;

            float distanceToTarget = Vector2.Distance(Projectile.Center, targetPos);
            float timeToTarget = distanceToTarget / MaxSpeed;

            Vector2 predictedPos = targetPos + targetVel * timeToTarget * PredictionFactor;
            Vector2 toTarget = predictedPos - Projectile.Center;

            float targetAngle = toTarget.ToRotation();
            float currentAngle = Projectile.velocity.ToRotation();

            float angleDiff = MathHelper.WrapAngle(targetAngle - currentAngle);
            float adaptiveTurnSpeed = TurnSpeed;

            if (distanceToTarget < 200f)
            {
                adaptiveTurnSpeed *= 1.5f;
            }
            if (Math.Abs(angleDiff) < 0.1f)
            {
                adaptiveTurnSpeed *= 0.5f;
            }
            float newAngle = currentAngle + MathHelper.Clamp(angleDiff, -adaptiveTurnSpeed, adaptiveTurnSpeed);
            float currentSpeed = Projectile.velocity.Length();

            Projectile.velocity = new Vector2((float)Math.Cos(newAngle), (float)Math.Sin(newAngle)) * currentSpeed;

            if (currentSpeed < MaxSpeed)
            {
                Projectile.velocity *= 1f + Acceleration / currentSpeed;
            }
            AvoidObstacles();
        }
        private void AvoidObstacles()
        {
            Vector2 ahead = Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 50f;
            Point tilePos = ahead.ToTileCoordinates();
            if (WorldGen.InWorld(tilePos.X, tilePos.Y) && Main.tile[tilePos.X, tilePos.Y].HasTile && Main.tileSolid[Main.tile[tilePos.X, tilePos.Y].TileType])
            {
                Vector2 upDir = Projectile.velocity.RotatedBy(-MathHelper.PiOver4);
                Vector2 downDir = Projectile.velocity.RotatedBy(MathHelper.PiOver4);
                Vector2 upCheck = Projectile.Center + upDir.SafeNormalize(Vector2.Zero) * 50f;
                Vector2 downCheck = Projectile.Center + downDir.SafeNormalize(Vector2.Zero) * 50f;

                Point upTile = upCheck.ToTileCoordinates();
                Point downTile = downCheck.ToTileCoordinates();

                bool upBlocked = WorldGen.InWorld(upTile.X, upTile.Y) && Main.tile[upTile.X, upTile.Y].HasTile && Main.tileSolid[Main.tile[upTile.X, upTile.Y].TileType];
                bool downBlocked = WorldGen.InWorld(downTile.X, downTile.Y) && Main.tile[downTile.X, downTile.Y].HasTile && Main.tileSolid[Main.tile[downTile.X, downTile.Y].TileType];
                if (!upBlocked)
                {
                    Projectile.velocity = Projectile.velocity.RotatedBy(-0.1f);
                }
                else if (!downBlocked)
                {
                    Projectile.velocity = Projectile.velocity.RotatedBy(0.1f);
                }
            }
        }
        private void UpdateTrail()
        {
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > MaxTrailLength)
            {
                trailPositions.RemoveAt(trailPositions.Count - 1);
            }
        }
        private void SpawnExhaustParticles()
        {
            if (Projectile.localAI[0] % 2 != 0)
                return;
            Vector2 exhaustPos = Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.Zero) * 10f;
            Vector2 exhaustVel = -Projectile.velocity.SafeNormalize(Vector2.Zero) * 3f;
            for (int i = 0; i < 2; i++)
            {
                Dust fire = Dust.NewDustPerfect(exhaustPos + Main.rand.NextVector2Circular(4f, 4f), DustID.Torch, exhaustVel.RotatedByRandom(0.3f) * Main.rand.NextFloat(0.5f, 1.2f), 0, default, Main.rand.NextFloat(1.2f, 1.8f));
                fire.noGravity = true;
                fire.fadeIn = 0.3f;
            }
            if (Main.rand.NextBool(2))
            {
                Dust smoke = Dust.NewDustPerfect(exhaustPos, DustID.Smoke, exhaustVel * 0.5f + Main.rand.NextVector2Circular(1f, 1f), 150, default, Main.rand.NextFloat(0.8f, 1.2f));
                smoke.noGravity = true;
            }
            if (Main.rand.NextBool(5))
            {
                Dust Spark = Dust.NewDustPerfect(exhaustPos, DustID.Electric, exhaustVel.RotatedByRandom(0.5f) * Main.rand.NextFloat(1f, 2f), 0, default, 0.6f);
                Spark.noGravity = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;

            DrawCustomTrail(spriteBatch);

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero)
                    continue;

                float progress = 1f - (i / (float)Projectile.oldPos.Length);
                Color trailColor = Color.Lerp(Color.OrangeRed, Color.DarkRed, 1f - progress) * progress * 0.5f;
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;

                float scale = Projectile.scale * progress;

                spriteBatch.Draw(texture, drawPos, null, trailColor, Projectile.oldRot[i], origin, scale, SpriteEffects.None, 0f);
            }
            DrawEngineGlow(spriteBatch, origin);
            spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
        private void DrawCustomTrail(SpriteBatch spriteBatch)
        {
            if (trailPositions.Count < 2)
                return;

            Texture2D trailTex = TextureAssets.Extra[ExtrasID.ThePerfectGlow].Value;
            for (int i = 0; i < trailPositions.Count - 1; i++)
            {
                float progress = 1f - (i / (float)trailPositions.Count);
                Color trailColor;
                if (progress > 0.5f)
                {
                    trailColor = Color.Lerp(Color.Orange, Color.OrangeRed, (1f - progress) * 2f);
                }
                else
                {
                    trailColor = Color.Lerp(Color.OrangeRed, Color.DarkRed * 0.3f, (0.5f - progress) * 2f);
                }

                trailColor *= progress * 0.6f;
                Vector2 drawPos = trailPositions[i] - Main.screenPosition;

                float scale = 0.15f * progress;
                spriteBatch.Draw(trailTex, drawPos, null, trailColor, 0f, trailTex.Size() / 2f, scale, SpriteEffects.None, 0f);
            }
        }
        private void DrawEngineGlow(SpriteBatch spriteBatch, Vector2 origin)
        {
            Texture2D glowTex = TextureAssets.Extra[ExtrasID.ThePerfectGlow].Value;
            Vector2 glowPos = Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.Zero) * 8f - Main.screenPosition;
            Color innerGlow = Color.White * engineGlow * 0.8f;

            spriteBatch.Draw(glowTex, glowPos, null, innerGlow, 0f, glowTex.Size() / 2f, 0.15f, SpriteEffects.None, 0f);
            
            Color outerGlow = Color.OrangeRed * engineGlow * 0.5f;
            spriteBatch.Draw(glowTex, glowPos, null, outerGlow, 0f, glowTex.Size() / 2f, 0.3f, SpriteEffects.None, 0f);
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
            Lighting.AddLight(Projectile.Center, new Vector3(1f, 0.6f, 0.2f) * 2f);
            for (int i = 0; i < 30; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(8f, 8f);
                Dust fire = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f), DustID.Torch, velocity, 0, default, Main.rand.NextFloat(1.5f, 2.5f));
                fire.noGravity = true;
            }
            for (int i = 0; i < 15; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(5f, 5f);
                Dust smoke = Dust.NewDustPerfect(Projectile.Center, DustID.Smoke, velocity, 100, default, Main.rand.NextFloat(1f, 2f));
                smoke.noGravity = true;
            }
            for (int i = 0; i < 3; i++)
            {
                Gore.NewGore(Projectile.GetSource_Death(), Projectile.Center, Main.rand.NextVector2Circular(3f, 3f), GoreID.Smoke1 + Main.rand.Next(3));
            }
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ProjectileID.SolarWhipSwordExplosion, 0, 0f);
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return true;
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (Main.rand.NextBool(3))
            {
                target.AddBuff(BuffID.OnFire, 180);
            }
        }
    }
    #endregion
}
