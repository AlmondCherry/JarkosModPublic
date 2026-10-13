using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using System.Collections.Generic;
using jarkos.particles;
using Terraria.GameContent;
using System.IO;
using Terraria.DataStructures;
using Jarkos.systems;
using Terraria.Graphics.CameraModifiers;
using ReLogic.Content;
using jarkos.projectiles.enemy.celestia;

namespace jarkos.npc.Celestia
{
    [AutoloadBossHead]
    public class Celestia : ModNPC
    {
        private const int State_Hover = 0;
        private const int State_PrepSpiral = 1;
        private const int State_AttackSpiral = 2;
        private const int State_Wait = 3;
        private const int State_PrepDeathray = 4;
        private const int State_Deathray = 5;
        private const int State_PrepConvergingOrbs = 6;
        private const int State_ConvergingOrbs = 7;
        private const int State_PhantomDash = 8;
        private const int State_Phase2Transition = 9;

        private const int State_P2_CollapsePrep = 20;
        private const int State_P2_CollapseBurst = 21;

        private const int State_P2_StormPrep = 22;
        private const int State_P2_Storm = 23;

        private const int State_P2_Sunstrike = 24;

        private const int State_P2_BallLightning = 25;

        private const int State_SpawnAnimation = 30;
        private const int State_PreFightWait = 31;

        private const int State_P3_Spears = 40;
        private const int State_P3_GroundOrbs = 41;
        private const int State_P3_OrbitalArena = 42;
        private const int State_P3_GridSpears = 43;
        private const int State_P3_SideWallSpears = 44;

        private const int State_P4_FinalDeath = 50;

        private const int StormAppearTime = 42;
        private const int StormDashTime = 48;
        private const int StormDashCycle = StormAppearTime + StormDashTime;

        private const float StormDashSpeedBase = 34f;
        private const float StormDashSpeedStep = 3f;

        private const int StormStarsBase = 3;
        private const int StormStarsStep = 1;

        private const int CollapseChargeTime = 110;

        public ref float AITimer => ref NPC.ai[1];
        public ref float AIState => ref NPC.ai[0];
        public ref float AttackCounter => ref NPC.ai[2];

        public bool isPhase2 = false;
        public float phaseTransitionProgress = 0f;
        public float AuraTimer = 0f;
        public float auraIntensity = 0f;
        private bool phase3TransitionStarted;
        private bool phase3;
        private bool phase3AuraSpawned;
        public bool enteredFinalPhase;
        
        private int P3_SpearWave = 1;
        private int P3_SpearsInWave = 0;
        private int P3_SpearWaveTarget = 8;
        private int P3_GridWave = 1;
        private int P3_WallWave = 1;
        private int P3_WallWaveTarget = 4;
        private int P3_SubTimer = 0;
        
        private readonly List<float> sunstrikeXs = new();

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(isPhase2);
            writer.Write(phaseTransitionProgress);
            writer.Write(AuraTimer);
            writer.Write(enteredFinalPhase);
            writer.Write(NPC.localAI[1]);
            writer.Write(NPC.localAI[2]);
            writer.Write(NPC.localAI[3]);
            
            writer.Write(phase3TransitionStarted);
            writer.Write(phase3);
            writer.Write(phase3AuraSpawned);
            writer.Write(P3_SpearWave);
            writer.Write(P3_SpearsInWave);
            writer.Write(P3_SpearWaveTarget);
            writer.Write(P3_GridWave);
            writer.Write(P3_WallWave);
            writer.Write(P3_WallWaveTarget);
            writer.Write(P3_SubTimer);
            writer.Write(NPC.localAI[0]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            isPhase2 = reader.ReadBoolean();
            phaseTransitionProgress = reader.ReadSingle();
            AuraTimer = reader.ReadSingle();
            enteredFinalPhase = reader.ReadBoolean();
            NPC.localAI[1] = reader.ReadSingle();
            NPC.localAI[2] = reader.ReadSingle();
            NPC.localAI[3] = reader.ReadSingle();
            
            phase3TransitionStarted = reader.ReadBoolean();
            phase3 = reader.ReadBoolean();
            phase3AuraSpawned = reader.ReadBoolean();
            P3_SpearWave = reader.ReadInt32();
            P3_SpearsInWave = reader.ReadInt32();
            P3_SpearWaveTarget = reader.ReadInt32();
            P3_GridWave = reader.ReadInt32();
            P3_WallWave = reader.ReadInt32();
            P3_WallWaveTarget = reader.ReadInt32();
            P3_SubTimer = reader.ReadInt32();
            NPC.localAI[0] = reader.ReadSingle();
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 24;
            
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.MPAllowedEnemies[Type] = true;
        }

        public override void SetDefaults()
        {
            NPC.width = 158;
            NPC.height = 154;
            NPC.damage = 50;
            NPC.defense = 20;
            NPC.lifeMax = 545000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = Item.buyPrice(gold: 20);
            NPC.SpawnWithHigherTime(30);
            NPC.boss = true;
            NPC.npcSlots = 10f;
            NPC.aiStyle = -1;
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC.ai[0] = 30f;
            NPC.ai[1] = 0f;
            NPC.TargetClosest(true);
            NPC.netUpdate = true;

            if (NPC.target >= 0 && NPC.target < 255)
            {
                Player p = Main.player[NPC.target];
                if (NPC.Distance(p.Center) < 200f)
                {
                    NPC.Center = p.Center + new Vector2(0f, -1100f);
                }
            }
        }
        private readonly List<CelestiaAfterimage> _afterimages = new();

        private void UpdateAfterimages()
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            for (int i = _afterimages.Count - 1; i >= 0; i--)
            {
                _afterimages[i].Alpha -= 0.035f;
                if (_afterimages[i].Alpha <= 0f)
                {
                    _afterimages.RemoveAt(i);
                }
            }

            if (NPC.alpha < 255 && !NPC.hide && (NPC.velocity.Length() > 0.2f || AIState == State_PhantomDash || AIState == State_P2_Storm))
            {
                int spawnInterval = NPC.velocity.Length() > 8f ? 1 : 2;
                if (Main.GameUpdateCount % spawnInterval == 0)
                {
                    Texture2D texture = TextureAssets.Npc[NPC.type].Value;
                    int frameHeight = texture.Height / Main.npcFrameCount[NPC.type];
                    Rectangle sourceRect = new Rectangle(0, NPC.frame.Y, texture.Width, frameHeight);

                    _afterimages.Add(new CelestiaAfterimage
                    {
                        Position = NPC.Center,
                        Rotation = NPC.rotation,
                        SourceRect = sourceRect,
                        SpriteDirection = NPC.spriteDirection,
                        Alpha = 0.85f,
                        Scale = NPC.scale
                    });
                }
            }
        }
        public override void AI()
        {
            UpdateAfterimages();

            if (Main.netMode != NetmodeID.Server)
            {
                if ((int)AIState == State_P4_FinalDeath)
                {
                    Music = 0;
                }
                else if (AIState == State_SpawnAnimation || AIState == State_PreFightWait)
                {
                    Music = MusicLoader.GetMusicSlot(Mod, "Music/BeforeFight");
                }
                else
                {
                    Music = MusicLoader.GetMusicSlot(Mod, "Music/CelestiaTheme");
                }
            }

            if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active)
            {
                NPC.TargetClosest(false);
            }

            Player player = Main.player[NPC.target];
            if (!player.active || player.dead)
            {
                bool anyAlive = false;
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player p = Main.player[i];
                    if (p != null && p.active && !p.dead)
                    {
                        NPC.target = i;
                        player = p;
                        anyAlive = true;
                        break;
                    }
                }

                if (!anyAlive)
                {
                    NPC.velocity.Y -= 0.5f;
                    NPC.velocity.X *= 0.95f;

                    NPC.EncourageDespawn(10);

                    if (NPC.timeLeft > 10)
                        NPC.timeLeft = 10;

                    if (NPC.position.Y < 0 || NPC.timeLeft <= 1)
                    {
                        NPC.active = false;
                        NPC.netUpdate = true;
                    }
                    return;
                }
            }

            NPC.timeLeft = NPC.activeTime * 30;

            if ((int)AIState == State_P4_FinalDeath)
            {
                FinalDeathBehavior(player);
                return;
            }

            if (HandlePhase3Transition(player))
                return;

            if (Main.netMode != NetmodeID.Server)
            {
                CelestiaEffectsSystem.BossCenter = NPC.Center;
                if (isPhase2 || phase3)
                {
                    CelestiaEffectsSystem.Phase2KeepAlive = 5;
                }
                else
                {
                    CelestiaEffectsSystem.Phase1KeepAlive = 5;
                }
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Main.dayTime = true;
                Main.time = 27000.0;

                if (Main.netMode == NetmodeID.Server && Main.GameUpdateCount % 60 == 0)
                    NetMessage.SendData(MessageID.WorldData);
            }

            if (NPC.life < NPC.lifeMax / 2 && !isPhase2 && AIState != State_Phase2Transition)
            {
                AIState = State_Phase2Transition;
                AITimer = 0;
                NPC.netUpdate = true;
                
                ClearHostileProjectiles();
            }

            if (player.dead || !player.active)
            {
                NPC.TargetClosest(true);
                player = Main.player[NPC.target];
                if (player.dead || !player.active)
                {
                    NPC.velocity.Y -= 0.04f;
                    NPC.EncourageDespawn(10);
                    return;
                }
            }

            if (AIState == State_Hover)
            {
                NPC.direction = player.Center.X < NPC.Center.X ? -1 : 1;
                NPC.spriteDirection = -NPC.direction;
            }

            switch (AIState)
            {
                case State_Hover:
                    HoverBehavior(player);
                    break;
                case State_PrepSpiral:
                    PrepSpiralBehavior(player);
                    break;
                case State_AttackSpiral:
                    AttackSpiralBehavior(player);
                    break;
                case State_Wait:
                    WaitBehavior();
                    break;
                case State_PrepDeathray:
                    PrepDeathrayBehavior(player);
                    break;
                case State_Deathray:
                    DeathrayBehavior(player);
                    break;
                case State_PrepConvergingOrbs:
                    PrepConvergingOrbsBehavior(player);
                    break;
                case State_ConvergingOrbs:
                    ConvergingOrbsBehavior(player);
                    break;
                case State_PhantomDash:
                    PhantomDashBehavior(player);
                    break;
                case State_Phase2Transition:
                    Phase2TransitionBehavior(player);
                    break;
                case State_P2_CollapsePrep:
                    CollapsePrepBehavior(player);
                    break;
                case State_P2_CollapseBurst:
                    CollapseBurstBehavior(player);
                    break;
                case State_P2_StormPrep:
                    StormPrepBehavior(player);
                    break;
                case State_P2_Storm:
                    StormBehavior(player);
                    break;
                case State_P2_Sunstrike:
                    SunstrikeBehavior(player);
                    break;
                case State_P2_BallLightning:
                    BallLightningBehavior(player);
                    break;
                case State_SpawnAnimation:
                    SpawnAnimationBehavior(player);
                    break;
                case State_PreFightWait:
                    PreFightWaitBehavior(player);
                    break;
            }

            if (AIState != State_Hover && AIState != State_SpawnAnimation && AIState != State_PreFightWait)
            {
                NPC.rotation = MathHelper.Lerp(NPC.rotation, 0f, 0.1f);
            }

            if (AuraTimer > 0)
            {
                AuraTimer--;

                if (!isPhase2 && !phase3)
                {
                    float distanceToPlayer = Vector2.Distance(player.Center, NPC.Center);
                    if (distanceToPlayer > 1000f)
                    {
                        float slowStrength = (distanceToPlayer - 1000f) / 600f;
                        if (slowStrength > 0.15f) slowStrength = 0.15f; 

                        Vector2 toPlayer = player.Center - NPC.Center;
                        toPlayer.Normalize();

                        float outwardSpeed = Vector2.Dot(player.velocity, toPlayer);

                        if (outwardSpeed > 0)
                        {
                            player.velocity -= toPlayer * (outwardSpeed * slowStrength);
                        }

                        if (Main.rand.NextBool(3))
                        {
                            int dust = Dust.NewDust(player.position, player.width, player.height, DustID.PinkCrystalShard, 0, 0, 100, default, 1.2f);
                            Main.dust[dust].noGravity = true;
                            Main.dust[dust].velocity *= 0.2f;
                        }
                    }
                }
            }

            if (Main.netMode != NetmodeID.Server)
            {
                bool explosionHappened = isPhase2 || (AIState == State_Phase2Transition && AITimer >= 120);

                if (explosionHappened)
                {
                    CelestiaEffectsSystem.Phase2KeepAlive = 10;
                    CelestiaEffectsSystem.BossCenter = NPC.Center;
                }
            }
        }

        public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            if (AIState == State_PreFightWait && AITimer >= 300)
            {
                StartBattle();
            }
        }

        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            if (AIState == State_PreFightWait && AITimer >= 300)
            {
                StartBattle();
            }
        }

        private void StartBattle()
        {
            AIState = State_Hover;
            AITimer = 0;

            NPC.dontTakeDamage = false;
            NPC.netUpdate = true;

            SoundEngine.PlaySound(SoundID.Roar with { Pitch = 0.2f, Volume = 1.2f }, NPC.Center);

            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 40; i++)
                {
                    Vector2 burstVel = Main.rand.NextVector2Circular(12f, 12f);
                    Color burstColor = Main.rand.NextBool() ? Color.HotPink : Color.Cyan;
                    new StarParticle(NPC.Center, burstVel, burstColor, 45, Main.rand.NextFloat(0.9f, 1.8f), 1f, Main.rand.NextFloat(-0.4f, 0.4f)).Spawn();
                }
            }
        }

        private void SpawnAnimationBehavior(Player player)
        {
            if (AITimer == 0)
            {
                NPC.dontTakeDamage = true;
                NPC.Center = player.Center + new Vector2(0f, -1100f);

                NPC.velocity = Vector2.Zero;
                NPC.netUpdate = true;

                SoundEngine.PlaySound(SoundID.Item117 with { Pitch = -0.3f, Volume = 1.2f }, NPC.Center);
            }

            NPC.dontTakeDamage = true;
            AITimer++;

            float duration = 140f;
            float t = MathHelper.Clamp(AITimer / duration, 0f, 1f);

            float easeOut = 1f - (float)Math.Pow(1f - t, 3.0);

            Vector2 startPos = player.Center + new Vector2(0f, -1100f);
            Vector2 targetPos = player.Center + new Vector2(0f, -320f);

            Vector2 newCenter = Vector2.Lerp(startPos, targetPos, easeOut);
            NPC.velocity = newCenter - NPC.Center;
            NPC.Center = newCenter;

            NPC.direction = player.Center.X < NPC.Center.X ? -1 : 1;
            NPC.spriteDirection = -NPC.direction;
            NPC.rotation = MathHelper.Clamp(NPC.velocity.X * 0.02f, -0.25f, 0.25f);

            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 particleVel = -NPC.velocity * 0.2f + Main.rand.NextVector2Circular(2f, 2f);
                    Color pColor = Main.rand.NextBool() ? Color.HotPink : Color.DeepSkyBlue;
                    new StarParticle(NPC.Center + Main.rand.NextVector2Circular(40f, 40f), particleVel, pColor, 30, Main.rand.NextFloat(0.6f, 1.2f), 1f, Main.rand.NextFloat(-0.2f, 0.2f)).Spawn();
                }

                int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.PinkCrystalShard, 0, 0, 100, default, 1.4f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity = -NPC.velocity * 0.2f;
            }

            if (t > 0.5f)
            {
                TenderodScreenSystem.ScreenShakeAmount = MathF.Max(TenderodScreenSystem.ScreenShakeAmount, (t - 0.5f) * 6f);
            }

            if (AITimer >= duration)
            {
                SoundEngine.PlaySound(SoundID.Item29 with { Volume = 1.0f, Pitch = -0.2f }, NPC.Center);
                if (Main.netMode != NetmodeID.Server)
                {
                    for (int i = 0; i < 35; i++)
                    {
                        Vector2 burstVel = Main.rand.NextVector2Circular(10f, 10f);
                        Color burstColor = Main.rand.NextBool() ? Color.HotPink : Color.Cyan;
                        new StarParticle(NPC.Center, burstVel, burstColor, 40, Main.rand.NextFloat(0.8f, 1.6f), 1f, Main.rand.NextFloat(-0.3f, 0.3f)).Spawn();
                    }
                }

                AIState = State_PreFightWait;
                AITimer = 0;
                NPC.netUpdate = true;
            }
        }

        private void PreFightWaitBehavior(Player player)
        {
            AITimer++;

            if (AITimer >= 300 && (NPC.life < NPC.lifeMax || NPC.justHit))
            {
                StartBattle();
                return;
            }

            if (AITimer < 300)
            {
                NPC.dontTakeDamage = true;

                if (AITimer == 299)
                {
                    SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.3f, Volume = 1.0f }, NPC.Center);
                    if (Main.netMode != NetmodeID.Server)
                    {
                        for (int i = 0; i < 25; i++)
                        {
                            Vector2 pVel = Main.rand.NextVector2Circular(7f, 7f);
                            new StarParticle(NPC.Center, pVel, Color.DeepSkyBlue, 30, 1.2f, 1f, 0f).Spawn();
                        }
                    }
                }
            }
            else
            {
                NPC.dontTakeDamage = false;
            }

            Vector2 targetPosition = player.Center + new Vector2(0f, -320f);
            targetPosition.Y += (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2.5f) * 15f;
            targetPosition.X += (float)Math.Cos(Main.GlobalTimeWrappedHourly * 1.8f) * 35f;

            Vector2 direction = targetPosition - NPC.Center;
            float speed = 9f;
            float inertia = 15f;

            direction.Normalize();
            direction *= speed;

            NPC.velocity = (NPC.velocity * (inertia - 1) + direction) / inertia;

            NPC.direction = player.Center.X < NPC.Center.X ? -1 : 1;
            NPC.spriteDirection = -NPC.direction;
            NPC.rotation = MathHelper.Clamp(NPC.velocity.X * 0.02f, -0.25f, 0.25f);

            if (Main.netMode != NetmodeID.Server && Main.rand.NextBool(3))
            {
                int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.PinkCrystalShard, 0, 0, 100, default, 1.0f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0.3f;
            }
        }

        private void HoverBehavior(Player player)
        {
            AITimer++;

            Vector2 targetPosition = player.Center + new Vector2(0, -300);
            
            targetPosition.Y += (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2f) * 20f;
            targetPosition.X += (float)Math.Cos(Main.GlobalTimeWrappedHourly * 1.5f) * 50f;

            Vector2 direction = targetPosition - NPC.Center;
            float speed = 7f;
            float inertia = 20f;

            direction.Normalize();
            direction *= speed;

            NPC.velocity = (NPC.velocity * (inertia - 1) + direction) / inertia;

            if (AITimer >= 180)
            {
                AITimer = 0;
                
                if (isPhase2)
                {
                    AttackCounter++;
                    int pick = (int)AttackCounter % 4;
                    AIState = pick == 0 ? State_P2_CollapsePrep : pick == 1 ? State_P2_StormPrep : pick == 2 ? State_P2_Sunstrike : State_P2_BallLightning;
                    NPC.netUpdate = true;
                }
                else
                {
                    int attackChoice = Main.rand.Next(4);
                    if (attackChoice == 0)
                        AIState = State_PrepSpiral;
                    else if (attackChoice == 1)
                        AIState = State_PrepDeathray;
                    else if (attackChoice == 2)
                        AIState = State_PrepConvergingOrbs;
                    else
                        AIState = State_PhantomDash;
                    
                    NPC.netUpdate = true;
                }
            }

            NPC.rotation = MathHelper.Lerp(NPC.rotation, NPC.velocity.X * 0.03f, 0.1f);
        }

        private void PrepSpiralBehavior(Player player)
        {
            AITimer++;
            NPC.velocity *= 0.95f;

            if (AITimer >= 60)
            {
                AITimer = 0;
                AIState = State_AttackSpiral;
            }
        }

        private void AttackSpiralBehavior(Player player)
        {
            AITimer++;
            NPC.velocity *= 0.9f;

            Vector2 handOffset = new Vector2(46 * NPC.direction, -47);
            Vector2 handPosition = NPC.Center + handOffset;

            if (Main.rand.NextBool(3))
            {
                new StarParticle(handPosition, Main.rand.NextVector2Circular(2, 2), Color.HotPink, 30, 0.5f, 1f, Main.rand.NextFloat(-0.2f, 0.2f)).Spawn();
            }

            AuraTimer = 300;

            if (AITimer % 4 == 0)
            {
                int numStreams = 4;
                float baseAngle = AITimer * 0.05f;

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (int i = 0; i < numStreams; i++)
                    {
                        float angle = baseAngle + MathHelper.TwoPi / numStreams * i;
                        Vector2 velocity = angle.ToRotationVector2() * 8f;
                        
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), handPosition, velocity, ModContent.ProjectileType<CelestiaStar>(), NPC.damage / 3, 0f, Main.maxPlayers);
                        }
                    }
                }
            }

            if (AITimer >= 180)
            {
                AITimer = 0;
                AIState = State_Wait;
            }
        }

        private void PrepDeathrayBehavior(Player player)
        {
            AITimer++;
            NPC.velocity *= 0.9f;

            Vector2 handPosition = NPC.Center + new Vector2(46 * NPC.direction, -47);
            float charge = AITimer / 60f;

            if (Main.netMode != NetmodeID.Server)
            {
                if (Main.rand.NextBool(2))
                {
                    Vector2 p = handPosition + Main.rand.NextVector2CircularEdge(90f, 90f) * (1f - charge * 0.5f);
                    new Spark(p, (handPosition - p) * 0.09f, Color.HotPink, 18, 0.5f, 1f, 0f).Spawn();
                }

                new Circular_glow(handPosition, Vector2.Zero, Color.Magenta * charge, 5, 0.4f + charge * 0.8f, 1f, 0f).Spawn();
            }
            Lighting.AddLight(handPosition, 1f * charge, 0.3f * charge, 0.9f * charge);

            if (AITimer == 45)
                SoundEngine.PlaySound(SoundID.Item117, NPC.Center);

            if (AITimer >= 60)
            {
                AITimer = 0;
                AIState = State_Deathray;
            }
        }

        private void DeathrayBehavior(Player player)
        {
            AITimer++;
            NPC.velocity *= 0.9f;

            if (AITimer == 1 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                Vector2 handPosition = NPC.Center + new Vector2(46 * NPC.direction, -47);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), handPosition, Vector2.Zero, ModContent.ProjectileType<CelestiaDeathray>(), NPC.damage, 0f, ai0: NPC.whoAmI);
            }

            AuraTimer = 280;

            if (AITimer % 24 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                float sweepAngle = (float)Math.Sin(AITimer * 0.08f) * MathHelper.Pi;
                for (int s = -1; s <= 1; s += 2)
                {
                    Vector2 vel = (sweepAngle + s * 0.25f).ToRotationVector2() * 18f;
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, vel, ModContent.ProjectileType<CelestiaStar>(), NPC.damage / 4, 0f, Main.maxPlayers);
                }
            }

            if (AITimer >= 265)
            {
                AITimer = 0;
                AuraTimer = 30;
                AIState = State_Wait;
            }
        }

        private void WaitBehavior()
        {
            NPC.velocity *= 0.9f;

            if (AuraTimer <= 0)
            {
                AIState = State_Hover;
            }
        }

        private void PrepConvergingOrbsBehavior(Player player)
        {
            AITimer++;

            Vector2 targetPosition = player.Center + new Vector2(0, -300);
            Vector2 direction = targetPosition - NPC.Center;
            direction.Normalize();
            NPC.velocity = (NPC.velocity * 15f + direction * 5f) / 16f;

            if (AITimer >= 60)
            {
                AITimer = 0;
                AIState = State_ConvergingOrbs;
            }
        }

        private void ConvergingOrbsBehavior(Player player)
        {
            AITimer++;

            Vector2 targetPosition = player.Center + new Vector2(0, -300);
            Vector2 direction = targetPosition - NPC.Center;
            direction.Normalize();
            NPC.velocity = (NPC.velocity * 15f + direction * 5f) / 16f;

            if (AITimer == 1 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                int numOrbs = 20;
                int gapSize = 3;
                int gapStart = Main.rand.Next(numOrbs);

                Vector2 ringCenter = player.Center;
                float radius = 550f;

                for (int i = 0; i < numOrbs; i++)
                {
                    bool isGap = false;
                    for (int g = 0; g < gapSize; g++)
                    {
                        if (i == (gapStart + g) % numOrbs)
                        {
                            isGap = true;
                            break;
                        }
                    }

                    if (isGap) continue;

                    float angle = MathHelper.TwoPi / numOrbs * i;
                    Vector2 spawnPos = ringCenter + angle.ToRotationVector2() * radius;
                    Vector2 velocity = (ringCenter - spawnPos).SafeNormalize(Vector2.Zero) * 2.5f;

                    Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, velocity, ModContent.ProjectileType<CelestiaConvergingOrb>(), NPC.damage / 3, 0f, Main.maxPlayers);
                }
                AuraTimer = 300;
            }

            if (AITimer >= 150)
            {
                AITimer = 0;
                AuraTimer = 10;
                AIState = State_Wait;
            }
        }

        private void PhantomDashBehavior(Player player)
        {
            AITimer++;

            if (AITimer == 1)
            {
                NPC.alpha = 255;
                NPC.dontTakeDamage = true;
                
                for (int i = 0; i < 20; i++)
                {
                    new Stretching_particle(NPC.Center, Main.rand.NextVector2Circular(10, 10), Color.Magenta, 20, 0.5f, 1f, 0f).Spawn();
                }
                SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
            }

            if ((AITimer == 1 || AITimer == 60 || AITimer == 120) && Main.netMode != NetmodeID.MultiplayerClient)
            {
                Vector2 randomPos = player.Center + Main.rand.NextVector2CircularEdge(450, 450);
                
                Projectile.NewProjectile(NPC.GetSource_FromAI(), randomPos, Vector2.Zero, ModContent.ProjectileType<CelestiaPhantom>(), NPC.damage / 3, 0f, ai0: 50f);

                if (AITimer == 120)
                {
                    NPC.localAI[1] = randomPos.X;
                    NPC.localAI[2] = randomPos.Y;
                }
            }

            NPC.velocity *= 0.8f;

            if (AITimer == 200)
            {
                NPC.Center = new Vector2(NPC.localAI[1], NPC.localAI[2]);
            }

            if (AITimer >= 200 && AITimer < 230)
            {
                if (NPC.alpha > 0)
                {
                    NPC.alpha -= 10;
                    if (NPC.alpha < 0) NPC.alpha = 0;
                }
            }

            if (AITimer >= 230)
            {
                AITimer = 0;
                NPC.alpha = 0;
                NPC.dontTakeDamage = false;
                AIState = State_Wait;
                AuraTimer = 30;
            }
        }

        private void CollapsePrepBehavior(Player player)
        {
            AITimer++;

            Vector2 epicentre = player.Center + new Vector2(0, -260);
            Vector2 toTarget = epicentre - NPC.Center;

            NPC.velocity = (NPC.velocity * 14f + toTarget * 0.06f) / 15f;
            if (toTarget.Length() < 30f) NPC.velocity *= 0.85f;

            float charge = MathHelper.Clamp(AITimer / CollapseChargeTime, 0f, 1f);

            int spawnCount = 1 + (int)(charge * 3f);
            for (int s = 0; s < spawnCount; s++)
            {
                if (!Main.rand.NextBool(2)) continue;

                float startRadius = MathHelper.Lerp(420f, 160f, charge);
                float ang = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 edge = NPC.Center + ang.ToRotationVector2() * startRadius;

                Vector2 inward = (NPC.Center - edge).SafeNormalize(Vector2.Zero);
                Vector2 tangent = new Vector2(-inward.Y, inward.X);

                float swirl = MathHelper.Lerp(2.5f, 6f, charge);
                Vector2 vel = inward * MathHelper.Lerp(2f, 5f, charge) + tangent * swirl;

                Color c = Main.rand.NextBool(3) ? new Color(255, 230, 255) : Color.HotPink;
                new StarParticle(edge, vel, c, 26, 0.45f, 1f,Main.rand.NextFloat(-0.2f, 0.2f)).Spawn();
            }

            if (Main.rand.NextBool(2))
            {
                float r = MathHelper.Lerp(90f, 30f, charge);
                Vector2 p = NPC.Center + Main.rand.NextVector2CircularEdge(r, r);
                Vector2 v = (NPC.Center - p) * 0.05f;
                new Spark(p, v, Color.Magenta, 24, 0.5f, 1f, 0f).Spawn();
            }

            float lightPow = 0.4f + charge * 1.2f;
            Lighting.AddLight(NPC.Center, 1f * lightPow, 0.3f * lightPow, 0.9f * lightPow);

            AuraTimer = 320;

            if (AITimer == 1)
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.5f }, NPC.Center);

            if (AITimer >= CollapseChargeTime - 25)
            {
                if (AITimer == CollapseChargeTime - 25)
                {
                    var preShake = new Terraria.Graphics.CameraModifiers.PunchCameraModifier(NPC.Center, Main.rand.NextVector2Unit(), 6f, 5f, 25, 1200f, "CelestiaCharge");
                    Main.instance.CameraModifiers.Add(preShake);
                    SoundEngine.PlaySound(SoundID.Item117, NPC.Center);
                }

                if (Main.rand.NextBool(2))
                {
                    float ang = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 p = NPC.Center + ang.ToRotationVector2() * 70f;
                    new Circleedge(p, (NPC.Center - p) * 0.08f, Color.White, 18, 0.7f, 1f, 0f).Spawn();
                }
            }

            if (AITimer >= CollapseChargeTime)
            {
                AITimer = 0;
                AIState = State_P2_CollapseBurst;
                NPC.netUpdate = true;
            }
        }

        private void CollapseBurstBehavior(Player player)
        {
            AITimer++;
            NPC.velocity *= 0.9f;
            NPC.localAI[0] = 360;

            if (AITimer == 1)
            {
                SoundEngine.PlaySound(SoundID.Item62, NPC.Center);
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.4f }, NPC.Center);

                var shake = new PunchCameraModifier(NPC.Center, Main.rand.NextVector2Unit(), 22f, 9f, 35, 1400f, "CelestiaBurst");
                Main.instance.CameraModifiers.Add(shake);

                if (Main.netMode != NetmodeID.Server)
                {
                    for (int i = 0; i < 40; i++)
                        new Spark(NPC.Center, Main.rand.NextVector2CircularEdge(12f, 12f), Main.rand.NextBool() ? Color.White : Color.HotPink, 30, 0.8f, 1f, 0f).Spawn();
                    for (int i = 0; i < 18; i++)
                        new Circleedge(NPC.Center, Main.rand.NextVector2CircularEdge(7f, 7f), Color.Magenta, 35, 1.3f, 1f, 0f).Spawn();
                }
                Lighting.AddLight(NPC.Center, 3f, 1.5f, 3f);

                NPC.localAI[1] = 0f;
                NPC.localAI[2] = 2f;
                NPC.ai[3] = Main.rand.Next(28);
                NPC.netUpdate = true;
            }

            int maxWaves = 6;
            NPC.localAI[2] -= 1f;

            if (NPC.localAI[2] <= 0f && NPC.localAI[1] < maxWaves && Main.netMode != NetmodeID.MultiplayerClient)
            {
                int waveIndex = (int)NPC.localAI[1];

                int rays = 28;
                int gapSize = 5;


                int baseGap = (int)NPC.ai[3];
                int gapStart = (baseGap + waveIndex) % rays;

                SoundEngine.PlaySound(SoundID.Item125, NPC.Center);

                for (int i = 0; i < rays; i++)
                {
                    bool isGap = false;
                    for (int g = 0; g < gapSize; g++)
                        if (i == (gapStart + g) % rays) { isGap = true; break; }
                    if (isGap) continue;

                    float angle = MathHelper.TwoPi / rays * i;
                    Vector2 vel = angle.ToRotationVector2() * 4f;
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, vel, ModContent.ProjectileType<CelestiaStar>(), NPC.damage / 4, 0f, Main.maxPlayers);
                }

                NPC.localAI[1] += 1f;
                float nextDelay = MathHelper.Lerp(38f, 12f, waveIndex / (float)(maxWaves - 1));
                NPC.localAI[2] = nextDelay;
                NPC.netUpdate = true;
            }

            float fade = 1f - MathHelper.Clamp(AITimer / 160f, 0f, 1f);
            Lighting.AddLight(NPC.Center, 1.5f * fade, 0.6f * fade, 1.4f * fade);

            if (NPC.localAI[1] >= maxWaves && AITimer > 4)
            {
                if (NPC.localAI[2] <= -40f)
                {
                    AITimer = 0;
                    NPC.localAI[0] = 30;
                    AIState = State_Wait;
                    NPC.netUpdate = true;
                }
            }
        }

        private void StormPrepBehavior(Player player)
        {
            AITimer++;

            if (AITimer == 1)
            {
                NPC.alpha = 255;
                NPC.dontTakeDamage = true;
                NPC.velocity = Vector2.Zero;

                NPC.ai[3] = Main.rand.Next(1, 11);
                NPC.localAI[1] = NPC.ai[3];

                SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
                SoundEngine.PlaySound(SoundID.Zombie105 with { Pitch = -0.4f }, NPC.Center);

                if (Main.netMode != NetmodeID.Server)
                {
                    for (int i = 0; i < 30; i++)
                        new Stretching_particle(NPC.Center,
                            Main.rand.NextVector2Circular(14, 14), Color.Magenta, 22, 0.5f, 1f, 0f).Spawn();
                    for (int i = 0; i < 16; i++)
                        new Spark(NPC.Center,
                            Main.rand.NextVector2CircularEdge(9, 9), Color.HotPink, 30, 0.6f, 1f, 0f).Spawn();
                }

                var shake = new PunchCameraModifier(NPC.Center, Main.rand.NextVector2Unit(), 9f, 6f, 24, 1200f, "CelestiaStorm");
                Main.instance.CameraModifiers.Add(shake);

                NPC.netUpdate = true;
            }

            NPC.velocity *= 0.85f;
            NPC.localAI[0] = 500;

            if (Main.rand.NextBool(2) && Main.netMode != NetmodeID.Server)
            {
                float ang = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 p = player.Center + ang.ToRotationVector2() * Main.rand.NextFloat(350f, 520f);
                Vector2 v = (player.Center - p) * 0.015f;
                new Spark(p, v, Color.Magenta, 28, 0.4f, 1f, 0f).Spawn();
            }

            if (AITimer >= 45)
            {
                AITimer = 0;
                AIState = State_P2_Storm;
                NPC.netUpdate = true;
            }
        }

        private void StormBehavior(Player player)
        {
            AITimer++;
            NPC.localAI[0] = 500;

            int totalDashes = (int)NPC.localAI[1];
            int dashIndex = totalDashes - (int)NPC.ai[3]; 

            
            float dashSpeed = StormDashSpeedBase + StormDashSpeedStep * dashIndex;
            int starCount = StormStarsBase + StormStarsStep * dashIndex;

            if (AITimer == 1)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    float ringAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                    float ringDist = Main.rand.NextFloat(560f, 680f);
                    Vector2 entry = player.Center + ringAngle.ToRotationVector2() * ringDist;
                    NPC.Center = entry;
                    NPC.velocity = Vector2.Zero;

                    NPC.localAI[2] = (player.Center - entry).ToRotation();
                    NPC.netUpdate = true;
                }

                NPC.alpha = 255;
               SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.5f }, NPC.Center);
            }

            float dashAngle = NPC.localAI[2];
            Vector2 dashDir = dashAngle.ToRotationVector2();

            if (AITimer <= StormAppearTime)
            {
                NPC.velocity *= 0.8f;

                if (NPC.alpha > 0)
                {
                    NPC.alpha -= 8;
                    if (NPC.alpha < 0) NPC.alpha = 0;
                }

                NPC.rotation = MathHelper.Lerp(NPC.rotation, dashDir.X * 0.04f, 0.2f);

                float chargeProg = AITimer / StormAppearTime;

                if (Main.netMode != NetmodeID.Server)
                {
                    if (Main.rand.NextBool(2))
                    {
                        float dist = MathHelper.Lerp(150f, 40f, chargeProg);
                        Vector2 behind = NPC.Center - dashDir * dist;
                        new Stretching_particle(behind, dashDir * MathHelper.Lerp(2f, 6f, chargeProg), Color.Magenta, 16, 0.5f, 1f, 0f).Spawn();
                    }
                    if (Main.rand.NextBool(2))
                    {
                        float along = Main.rand.NextFloat(-650f, 650f);
                        Vector2 p = NPC.Center + dashDir * along;
                        new Spark(p, Vector2.Zero, Color.HotPink * 0.5f, 10, 0.3f, 1f, 0f).Spawn();
                    }
                }

                Lighting.AddLight(NPC.Center, 1.1f, 0.35f, 1f);
            }
            else if (AITimer <= StormDashCycle)
            {
                if (AITimer == StormAppearTime + 1)
                {
                    NPC.alpha = 0;
                    NPC.velocity = dashDir * dashSpeed;
                    SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
                    SoundEngine.PlaySound(SoundID.Item62, NPC.Center);

                    var shake = new PunchCameraModifier(NPC.Center, dashDir, 22f + dashIndex * 3f, 9f, 30, 1500f, "CelestiaStormDash");
                    Main.instance.CameraModifiers.Add(shake);

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f }, NPC.Center);

                        float spread = MathHelper.ToRadians(40f);
                        for (int i = 0; i < starCount; i++)
                        {
                            float t = starCount == 1 ? 0.5f : i / (float)(starCount - 1);
                            float offset = MathHelper.Lerp(-spread * 0.5f, spread * 0.5f, t);
                            float starAngle = dashAngle + offset;
                            Vector2 starVel = starAngle.ToRotationVector2() * 9f;

                            int p = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, starVel,
                                ModContent.ProjectileType<CelestiaWaveStar>(), NPC.damage / 4, 0f, Main.maxPlayers);

                            if (p < Main.maxProjectiles)
                                Main.projectile[p].ai[1] = t * MathHelper.TwoPi;
                        }
                    }

                    if (Main.netMode != NetmodeID.Server)
                        for (int i = 0; i < 26; i++)
                            new Circleedge(NPC.Center, dashDir.RotatedByRandom(0.6) * -Main.rand.NextFloat(3f, 8f), Color.HotPink, 30, 1.1f, 1f, 0f).Spawn();
                }

                NPC.velocity *= 0.99f;
                NPC.rotation = NPC.velocity.X * 0.02f;
                Lighting.AddLight(NPC.Center, 1.1f, 0.35f, 1f);

                if (Main.netMode != NetmodeID.Server && Main.rand.NextBool())
                {
                    new Spark(NPC.Center + Main.rand.NextVector2Circular(45, 45), NPC.velocity * -0.15f, Color.HotPink, 26, 0.6f, 1f, 0f).Spawn();
                    new Stretching_particle(NPC.Center, NPC.velocity * -0.1f, Color.Magenta, 18, 0.5f, 1f, 0f).Spawn();
                }

                int fadeStart = StormDashCycle - 16;
                if (AITimer >= fadeStart)
                {
                    if (NPC.alpha < 255)
                    {
                        NPC.alpha += 18;
                        if (NPC.alpha > 255) NPC.alpha = 255;
                    }
                    if (Main.netMode != NetmodeID.Server && Main.rand.NextBool())
                        new Stretching_particle(NPC.Center, Main.rand.NextVector2Circular(8, 8), Color.Magenta, 16, 0.4f, 1f, 0f).Spawn();
                }
            }
            else
            {
                NPC.ai[3] -= 1;

                if (NPC.ai[3] > 0)
                {
                    AITimer = 0;
                    NPC.alpha = 255;
                    NPC.netUpdate = true;
                }
                else
                {
                    AITimer = 0;
                    NPC.alpha = 0;
                    NPC.dontTakeDamage = false;
                    NPC.rotation = 0f;
                    NPC.velocity *= 0.9f;
                    NPC.localAI[0] = 30;

                    AIState = State_Wait;
                    NPC.netUpdate = true;
                }
            }
        }

        private void SunstrikeBehavior(Player player)
        {
            AITimer++;
            NPC.velocity *= 0.9f;

            if (AITimer == 1)
            {
                int strikes = Main.rand.Next(4, 8);
                if (Main.rand.NextBool(5))
                    strikes = Main.rand.Next(10, 13);

                NPC.ai[3] = strikes;
                NPC.localAI[1] = 0f;
                sunstrikeXs.Clear();

                SoundEngine.PlaySound(SoundID.Zombie104 with { Pitch = 0.2f }, NPC.Center);
                NPC.netUpdate = true;
            }

            AuraTimer = 420;

            int total = (int)NPC.ai[3];
            int fired = (int)NPC.localAI[1];

            if (fired < total && AITimer % 15 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                Vector2 predicted = player.Center + player.velocity * Main.rand.NextFloat(30f, 80f);

                const float MinGap = 420f;
                const float MaxScatter = 900f;
                float strikeX = predicted.X;

                if (fired > 0)
                {
                    bool found = false;
                    for (int attempt = 0; attempt < 20 && !found; attempt++)
                    {
                        float candidate = predicted.X + Main.rand.NextFloat(-MaxScatter, MaxScatter);
                        found = true;
                        foreach (float prevX in sunstrikeXs)
                        {
                            if (Math.Abs(candidate - prevX) < MinGap)
                            {
                                found = false;
                                break;
                            }
                        }
                        if (found)
                            strikeX = candidate;
                    }
                    if (!found)
                    {
                        float edge = predicted.X;
                        foreach (float prevX in sunstrikeXs)

                            if (Math.Abs(prevX - predicted.X) > Math.Abs(edge - predicted.X))
                                edge = prevX;

                        strikeX = edge + MinGap * (edge >= predicted.X ? 1f : -1f);
                    }
                }

                sunstrikeXs.Add(strikeX);
                Vector2 ground = FindGroundBelow(new Vector2(strikeX, predicted.Y));

                Projectile.NewProjectile(NPC.GetSource_FromAI(), ground, Vector2.Zero, ModContent.ProjectileType<CelestiaSunStrike>(), NPC.damage * 2, 0f, Main.myPlayer, ai0: Main.rand.NextFloat(100f));

                NPC.localAI[1] += 1f;
                NPC.netUpdate = true;
            }

            if (Main.netMode != NetmodeID.Server && Main.rand.NextBool(2))
                new Stretching_particle(NPC.Center + Main.rand.NextVector2Circular(70f, 40f), new Vector2(0f, -Main.rand.NextFloat(5f, 10f)), Color.Cyan, 20, 0.5f, 1f, 0f).Spawn();

            if (fired >= total && AITimer >= 15 * total + 260)
            {
                AITimer = 0;
                AuraTimer = 30;
                AIState = State_Wait;
                NPC.netUpdate = true;
            }
        }

        private static Vector2 FindGroundBelow(Vector2 from)
        {
            int tx = (int)(from.X / 16f);
            int ty = (int)(from.Y / 16f);

            for (int i = 0; i < 120; i++)
            {
                int y = ty + i;
                if (y >= Main.maxTilesY - 10) break;

                if (WorldGen.SolidTile(tx, y))
                    return new Vector2(from.X, y * 16f);
            }
            return from + new Vector2(0f, 600f);
        }

        private void BallLightningBehavior(Player player)
        {
            AITimer++;
            NPC.velocity *= 0.9f;

            if (AITimer == 1)
            {
                SoundEngine.PlaySound(SoundID.Item121, NPC.Center);

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int count = Main.rand.NextBool() ? 6 : 8;

                    int[] slots = new int[count];

                    for (int i = 0; i < count; i++) slots[i] = i;
                    for (int i = count - 1; i > 0; i--)
                    {
                        int j = Main.rand.Next(i + 1);
                        (slots[i], slots[j]) = (slots[j], slots[i]);
                    }

                    for (int i = 0; i < count; i++)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), player.Center, Vector2.Zero, ModContent.ProjectileType<CelestiaBallLightning>(), NPC.damage / 3, 0f, 
                            ai0: slots[i], 
                            ai1: i / 2, 
                            ai2: count);
                    }
                    NPC.netUpdate = true;
                }
            }

            AuraTimer = 60;

            if (Main.netMode != NetmodeID.Server && Main.rand.NextBool(3))
                new Spark(NPC.Center + Main.rand.NextVector2Circular(70f, 40f), (player.Center - NPC.Center).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(4f, 8f), new Color(80, 170, 255), 20, 0.45f, 1f, 0f).Spawn();

            if (AITimer > 60)
            {
                bool anyAlive = false;
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    if (Main.projectile[i].active &&
                        Main.projectile[i].type == ModContent.ProjectileType<CelestiaBallLightning>())
                    {
                        anyAlive = true;
                        break;
                    }
                }
                if (!anyAlive)
                {
                    AITimer = 0;
                    AuraTimer = 30;
                    AIState = State_Wait;
                    NPC.netUpdate = true;
                }
            }
        }

        private void MaintainPhase2Background()
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            CelestiaEffectsSystem.Phase2KeepAlive = 10;
            CelestiaEffectsSystem.BossCenter = NPC.Center;
        }

        private bool HandlePhase3Transition(Player target)
        {
            if (!phase3TransitionStarted && NPC.life <= NPC.lifeMax / 3)
            {
                phase3TransitionStarted = true;
                NPC.velocity = Vector2.Zero;
                NPC.dontTakeDamage = true;
                
                ClearHostileProjectiles();

                if (Main.netMode != NetmodeID.MultiplayerClient)
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<CelestiaPhase3Transition>(), 0, 0f, Main.myPlayer, NPC.whoAmI);
            }

            if (CelestiaTransitionSystem.TransitionActive)
            {
                MaintainPhase2Background();
                NPC.velocity *= 0.9f;
                return true;
            }

            if (phase3TransitionStarted && !phase3)
            {
                phase3 = true;
                NPC.dontTakeDamage = false;

                if (!phase3AuraSpawned && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    phase3AuraSpawned = true;
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<CelestiaPhase3Aura>(), 0, 0f, Main.myPlayer, NPC.whoAmI);
                }

                AIState = State_P3_Spears;
                AITimer = 0;
                P3_SpearWave = 1;
                P3_SpearsInWave = 0;
                P3_SpearWaveTarget = Main.rand.Next(8, 13);
                P3_SubTimer = 0;
            }

            if (phase3)
            {
                MaintainPhase2Background();

                float p3MaxHp = NPC.lifeMax / 3f;
                float currentP3Hp = MathHelper.Clamp(NPC.life, 1f, p3MaxHp);
                float missingHpRatio = MathHelper.Clamp(1f - (currentP3Hp / p3MaxHp), 0f, 1f);

                float speedScale = 0.07f + missingHpRatio * 0.06f;
                float lerpSpeed = 0.12f + missingHpRatio * 0.08f;

                Vector2 hoverOffset = new Vector2((float)Math.Sin(Main.GlobalTimeWrappedHourly * (2.2f + missingHpRatio * 0.8f)) * 360f, -250f + (float)Math.Cos(Main.GlobalTimeWrappedHourly * (1.6f + missingHpRatio * 0.6f)) * 45f);
                Vector2 targetPos = target.Center + hoverOffset;
                Vector2 toTarget = targetPos - NPC.Center;
                NPC.velocity = Vector2.Lerp(NPC.velocity, toTarget * speedScale, lerpSpeed);

                NPC.direction = target.Center.X < NPC.Center.X ? -1 : 1;
                NPC.spriteDirection = -NPC.direction;
                NPC.rotation = MathHelper.Clamp(NPC.velocity.X * 0.02f, -0.2f, 0.2f);

                if (Main.netMode != NetmodeID.Server && Main.rand.NextBool(3))
                {
                    Color sparkColor = Main.rand.NextBool() ? new Color(150, 60, 255) : Color.Cyan;
                    new Spark(NPC.Center + Main.rand.NextVector2Circular(60f, 60f), Main.rand.NextVector2Circular(3f, 3f), sparkColor, 20, 0.5f, 1f, 0f).Spawn();
                }

                UpdatePhase3Attacks(target);

                return true;
            }

            return false;
        }

        private float GetPhase3MissingHpRatio()
        {
            float p3MaxHp = NPC.lifeMax / 3f;
            float currentP3Hp = MathHelper.Clamp(NPC.life, 1f, p3MaxHp);

            return MathHelper.Clamp(1f - (currentP3Hp / p3MaxHp), 0f, 1f);
        }

        private void UpdatePhase3Attacks(Player target)
        {
            switch ((int)AIState)
            {
                case State_P3_Spears:
                    UpdateP3Spears(target);
                    break;
                case State_P3_GridSpears:
                    UpdateP3GridSpears(target);
                    break;
                case State_P3_SideWallSpears:
                    UpdateP3SideWallSpears(target);
                    break;
                case State_P3_GroundOrbs:
                    UpdateP3GroundOrbs(target);
                    break;
                case State_P3_OrbitalArena:
                    UpdateP3OrbitalArena(target);
                    break;
            }
        }

        private void UpdateP3Spears(Player target)
        {
            P3_SubTimer++;

            float missingHp = GetPhase3MissingHpRatio();

            int spearInterval = (int)MathHelper.Lerp(10f, 3f, missingHp);
            float telegraphDuration = MathHelper.Lerp(36f, 16f, missingHp);

            if (P3_SubTimer >= spearInterval && P3_SpearsInWave < P3_SpearWaveTarget)
            {
                P3_SubTimer = 0;
                P3_SpearsInWave++;

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    float targetX = target.Center.X + Main.rand.NextFloat(-350f, 350f);
                    if (P3_SpearsInWave % 3 == 0) targetX = target.Center.X + target.velocity.X * 18f;

                    float slantAngle = Main.rand.NextFloat(-0.42f, 0.42f);

                    Vector2 telegraphPos = new Vector2(targetX, target.Center.Y);
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), telegraphPos, Vector2.Zero, ModContent.ProjectileType<CelestiaSpearTelegraph>(), 0, 0f, Main.myPlayer, slantAngle, telegraphDuration);
                }

            }

            if (P3_SpearsInWave >= P3_SpearWaveTarget)
            {
                int pauseTicks = P3_SpearWave == 1 ? 25 : (P3_SpearWave == 2 ? 15 : 10);

                if (P3_SubTimer >= pauseTicks)
                {
                    P3_SubTimer = 0;
                    P3_SpearsInWave = 0;
                    P3_SpearWave++;

                    if (P3_SpearWave > 3)
                    {
                        AIState = State_P3_GridSpears;
                        AITimer = 0;
                        P3_SpearWave = 1;
                        P3_GridWave = 1;
                        P3_SubTimer = 0;
                    }
                    else
                    {
                        P3_SpearWaveTarget = Main.rand.Next(8, 13);
                    }
                }
            }
        }

        private void UpdateP3GridSpears(Player target)
        {
            AITimer++;
            P3_SubTimer++;

            float missingHp = GetPhase3MissingHpRatio();
            int waveInterval = (int)MathHelper.Lerp(48f, 20f, missingHp);
            float telegraphDuration = MathHelper.Lerp(32f, 16f, missingHp);

            if ((P3_SubTimer >= waveInterval || AITimer == 1) && P3_GridWave <= 4)
            {
                P3_SubTimer = 0;

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    float baseAngle = Main.rand.NextFloat(0f, MathHelper.TwoPi);
                    float angle1 = baseAngle;
                    float angle2 = baseAngle + MathHelper.PiOver2;

                    for (float offset = -720f; offset <= 720f; offset += 180f)
                    {
                        Vector2 dir1 = Vector2.UnitY.RotatedBy(angle1);
                        Vector2 perp1 = dir1.RotatedBy(MathHelper.PiOver2);
                        Vector2 pos1 = target.Center + perp1 * offset;

                        Projectile.NewProjectile(NPC.GetSource_FromAI(), pos1, Vector2.Zero, ModContent.ProjectileType<CelestiaSpearTelegraph>(), 0, 0f, Main.myPlayer, angle1, telegraphDuration);

                        Vector2 dir2 = Vector2.UnitY.RotatedBy(angle2);
                        Vector2 perp2 = dir2.RotatedBy(MathHelper.PiOver2);
                        Vector2 pos2 = target.Center + perp2 * offset;

                        Projectile.NewProjectile(NPC.GetSource_FromAI(), pos2, Vector2.Zero, ModContent.ProjectileType<CelestiaSpearTelegraph>(), 0, 0f, Main.myPlayer, angle2, telegraphDuration);
                    }
                }

                SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.6f + missingHp * 0.3f, Volume = 0.85f }, NPC.Center);
                P3_GridWave++;
            }

            if (P3_GridWave > 4 && P3_SubTimer >= waveInterval)
            {
                AIState = State_P3_SideWallSpears;
                AITimer = 0;
                P3_GridWave = 1;
                P3_WallWave = 1;
                P3_WallWaveTarget = Main.rand.Next(4, 7);
                P3_SubTimer = 0;
            }
        }

        private void UpdateP3SideWallSpears(Player target)
        {
            AITimer++;
            P3_SubTimer++;

            float missingHp = GetPhase3MissingHpRatio();
            int waveInterval = (int)MathHelper.Lerp(42f, 18f, missingHp);
            float telegraphDuration = MathHelper.Lerp(30f, 14f, missingHp);

            if ((P3_SubTimer >= waveInterval || AITimer == 1) && P3_WallWave <= P3_WallWaveTarget)
            {
                P3_SubTimer = 0;

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int side = Main.rand.Next(4);
                    float angle = 0f;
                    Vector2 perpDir = Vector2.UnitX;

                    switch (side)
                    {
                        case 0:
                            angle = 0f;
                            perpDir = Vector2.UnitX;
                            break;
                        case 1:
                            angle = MathHelper.Pi;
                            perpDir = Vector2.UnitX;
                            break;
                        case 2:
                            angle = -MathHelper.PiOver2;
                            perpDir = Vector2.UnitY;
                            break;
                        case 3:
                            angle = MathHelper.PiOver2;
                            perpDir = Vector2.UnitY;
                            break;
                    }

                    for (float offset = -750f; offset <= 750f; offset += 250f)
                    {
                        Vector2 pos = target.Center + perpDir * offset;
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), pos, Vector2.Zero, ModContent.ProjectileType<CelestiaSpearTelegraph>(), 0, 0f, Main.myPlayer, angle, telegraphDuration);
                    }
                }

                SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.55f + missingHp * 0.35f, Volume = 0.85f }, NPC.Center);
                P3_WallWave++;
            }

            if (P3_WallWave > P3_WallWaveTarget && P3_SubTimer >= waveInterval)
            {
                AIState = State_P3_GroundOrbs;
                AITimer = 0;
                P3_WallWave = 1;
                P3_SubTimer = 0;
            }
        }

        private void UpdateP3GroundOrbs(Player target)
        {
            AITimer++;

            if (AITimer % 12 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                float playerX = target.Center.X;
                float offset = Main.rand.NextFloat(130f, 650f);

                if (Main.rand.NextBool()) offset = -offset;
                float spawnX = playerX + offset;

                float spawnY = target.Center.Y + 800f;
                int tileX = (int)(spawnX / 16f);
                int startTileY = (int)(target.Center.Y / 16f);

                for (int y = startTileY; y < startTileY + 50; y++)
                {
                    if (WorldGen.SolidTile(tileX, y))
                    {
                        spawnY = y * 16f;
                        break;
                    }
                }

                float targetY = target.Center.Y + Main.rand.NextFloat(-750f, 700f);

                Vector2 spawnPos = new Vector2(spawnX, spawnY);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, Vector2.Zero, ModContent.ProjectileType<CelestiaGroundOrb>(), 45, 0f, Main.myPlayer, targetY);
            }

            if (AITimer >= 900)
            {
                AIState = State_P3_OrbitalArena;
                AITimer = 0;
            }
        }

        private void UpdateP3OrbitalArena(Player target)
        {
            AITimer++;

            if (AITimer == 1 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                Projectile.NewProjectile(NPC.GetSource_FromAI(), target.Center, Vector2.Zero, ModContent.ProjectileType<CelestiaOrbitalArena>(), 0, 0f, Main.myPlayer);
            }

            if (AITimer >= 1080)
            {
                AIState = State_P3_Spears;
                AITimer = 0;
                P3_SpearWave = 1;
                P3_SpearsInWave = 0;
                P3_SpearWaveTarget = Main.rand.Next(8, 13);
                P3_SubTimer = 0;
            }
        }

        private static float SmoothEnvelope(float timer, float total, float fade)
        {
            float fadeIn  = MathHelper.Clamp(timer / fade, 0f, 1f);
            float fadeOut = MathHelper.Clamp((total - timer) / fade, 0f, 1f);
            float e = Math.Min(fadeIn, fadeOut);

            return e * e * (3f - 2f * e);
        }

        private void Phase2TransitionBehavior(Player player)
        {
            AITimer++;
            NPC.velocity *= 0.9f;
            NPC.dontTakeDamage = true;

            float maxTransitionTime = 240f;
            phaseTransitionProgress = AITimer / maxTransitionTime;

            if (Main.netMode != NetmodeID.Server)
            {
                string filter = CelestiaEffectsSystem.FilterName;

                if (AITimer == 1 && !Terraria.Graphics.Effects.Filters.Scene[filter].IsActive())
                    Terraria.Graphics.Effects.Filters.Scene.Activate(filter, NPC.Center);

                if (Terraria.Graphics.Effects.Filters.Scene[filter].IsActive())
                {
                    float envelope = SmoothEnvelope(AITimer, maxTransitionTime, 45f);
                    Terraria.Graphics.Effects.Filters.Scene[filter].GetShader()
                        .UseProgress(phaseTransitionProgress)
                        .UseIntensity(envelope)
                        .UseTargetPosition(NPC.Center);
                }
            }

            if (AITimer == 1)
                SoundEngine.PlaySound(SoundID.Roar, NPC.Center);

            if (AITimer < maxTransitionTime * 0.45f)
            {
                float charge = AITimer / (maxTransitionTime * 0.45f);
                int count = 1 + (int)(charge * 4f);

                for (int i = 0; i < count; i++)
                {
                    float ang = Main.rand.NextFloat(MathHelper.TwoPi);
                    float r = MathHelper.Lerp(600f, 200f, charge);
                    Vector2 p = NPC.Center + ang.ToRotationVector2() * r;
                    Vector2 inward = (NPC.Center - p).SafeNormalize(Vector2.Zero);
                    Vector2 tangent = new Vector2(-inward.Y, inward.X);
                    Vector2 vel = inward * MathHelper.Lerp(4f, 12f, charge) + tangent * 3f;

                    new StarParticle(p, vel, Main.rand.NextBool(3) ? Color.White : Color.HotPink, 24, 0.45f, 1f, Main.rand.NextFloat(-0.2f, 0.2f)).Spawn();
                }

                NPC.position += Main.rand.NextVector2Circular(1f + charge * 2.5f, 1f + charge * 2.5f);
            }
            else if (AITimer == (int)(maxTransitionTime * 0.5f))
            {
                SoundEngine.PlaySound(SoundID.Item62, NPC.Center);
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.5f }, NPC.Center);
                Main.instance.CameraModifiers.Add(new PunchCameraModifier(
                    NPC.Center, Main.rand.NextVector2Unit(), 28f, 10f, 45, 1600f, "Celestia"));

                if (Main.netMode != NetmodeID.Server)
                {
                    for (int i = 0; i < 60; i++)
                        new Spark(NPC.Center, Main.rand.NextVector2CircularEdge(16f, 16f), Main.rand.NextBool() ? Color.White : Color.HotPink, 35, 0.9f, 1f, 0f).Spawn();
                    for (int i = 0; i < 24; i++)
                        new Circleedge(NPC.Center, Main.rand.NextVector2CircularEdge(9f, 9f), Color.Magenta, 40, 1.5f, 1f, 0f).Spawn();
                }
                Lighting.AddLight(NPC.Center, 4f, 2f, 4f);
            }
            else if (AITimer > maxTransitionTime * 0.5f && Main.rand.NextBool(2))
            {
                Vector2 p = NPC.Center + Main.rand.NextVector2Circular(90, 110);
                new Stretching_particle(p, new Vector2(0, -Main.rand.NextFloat(3f, 7f)), Color.HotPink, 22, 0.5f, 1f, 0f).Spawn();
            }

            if (AITimer >= maxTransitionTime)
            {
                AITimer = 0;
                isPhase2 = true;
                NPC.dontTakeDamage = false;
                AIState = State_Wait;
                AuraTimer = 30;

                if (Main.netMode != NetmodeID.Server &&
                    Terraria.Graphics.Effects.Filters.Scene[CelestiaEffectsSystem.FilterName].IsActive())
                    Terraria.Graphics.Effects.Filters.Scene[CelestiaEffectsSystem.FilterName].Deactivate();
            }
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            
            int startFrame = 0;
            int endFrame = 5;
            int speed = 6;

            if (AIState == State_Hover || AIState == State_Wait || AIState == State_PrepConvergingOrbs || AIState == State_ConvergingOrbs)
            {
                startFrame = 0;
                endFrame = 5;
                speed = 6;
            }
            else if (AIState == State_PrepSpiral)
            {
                startFrame = 12;
                endFrame = 23;
                speed = 5;
            }
            else if (AIState == State_AttackSpiral)
            {
                startFrame = 6;
                endFrame = 11;
                speed = 4;
            }
            else if (AIState == State_PrepDeathray || AIState == State_Deathray)
            {
                startFrame = 6;
                endFrame = 11;
                speed = 4;
            }
            else if (AIState == State_P2_CollapsePrep 
            || AIState == State_P2_CollapseBurst
            || AIState == State_P2_StormPrep 
            || AIState == State_P2_Storm
            || AIState == State_P2_Sunstrike 
            || AIState == State_P2_BallLightning
            || AIState == State_P3_Spears 
            || AIState == State_P3_GridSpears 
            || AIState == State_P3_SideWallSpears 
            || AIState == State_P3_GroundOrbs
            || AIState == State_P3_OrbitalArena)
            {
                startFrame = 6;
                endFrame = 11;
                speed = 4;
            }
            else if (AIState == State_Phase2Transition)
            {
                startFrame = 12;
                endFrame = 23;
                speed = Math.Max(2, 6 - (int)(phaseTransitionProgress * 4f));
            }

            if (NPC.frame.Y < startFrame * frameHeight || NPC.frame.Y > endFrame * frameHeight)
            {
                NPC.frame.Y = startFrame * frameHeight;
                NPC.frameCounter = 0;
            }
            else if (NPC.frameCounter >= speed)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;
                if (NPC.frame.Y > endFrame * frameHeight)
                {
                    NPC.frame.Y = startFrame * frameHeight;
                }
            }
        }

        private void DrawAfterimages(SpriteBatch spriteBatch, Vector2 screenPos)
        {
            if (_afterimages.Count == 0)
                return;

            Texture2D texture = TextureAssets.Npc[NPC.type].Value;
            int frameHeight = texture.Height / Main.npcFrameCount[NPC.type];
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, frameHeight * 0.5f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

            foreach (var img in _afterimages)
            {
                SpriteEffects effects = img.SpriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

                Vector2 drawPos = img.Position - screenPos;
                float alphaProgress = MathHelper.Clamp(img.Alpha, 0f, 1f);

                Color cyanColor = new Color(0, 220, 255, 255) * (alphaProgress * 0.75f);
                spriteBatch.Draw(texture, drawPos, img.SourceRect, cyanColor, img.Rotation, drawOrigin, img.Scale, effects, 0f);

                Color coreColor = new Color(180, 255, 255, 255) * (alphaProgress * 0.45f);
                spriteBatch.Draw(texture, drawPos, img.SourceRect, coreColor, img.Rotation, drawOrigin, img.Scale * 0.98f, effects, 0f);
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
        }

        private static Asset<Effect> _auraShader;

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            DrawAfterimages(spriteBatch, screenPos);

            if (isPhase2 || AIState == State_Phase2Transition || auraIntensity > 0.02f)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

                _auraShader ??= ModContent.Request<Effect>("jarkos/Effects/CelestiaSSGSSAura", AssetRequestMode.ImmediateLoad);
                Effect auraShader = _auraShader.Value;

                Texture2D texture = TextureAssets.Npc[NPC.type].Value;
                int frameHeight = texture.Height / Main.npcFrameCount[NPC.type];

                Rectangle sourceRect = new Rectangle(0, NPC.frame.Y, texture.Width, frameHeight);
                Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, frameHeight * 0.5f);
                Vector2 drawPos = NPC.Center - screenPos;
                
                SpriteEffects effects = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

                auraShader.Parameters["uTime"].SetValue(Main.GlobalTimeWrappedHourly);

                float targetIntensity = isPhase2 ? 1f : (AIState == State_Phase2Transition ? phaseTransitionProgress : 0f);
                if (!isPhase2 && AIState == State_Phase2Transition)
                {
                    float burst = 1f - Math.Abs(phaseTransitionProgress - 0.5f) * 8f;
                    if (burst > 0f) targetIntensity += burst;
                }

                auraIntensity = MathHelper.Lerp(auraIntensity, targetIntensity, 0.05f);
                auraShader.Parameters["uIntensity"].SetValue(auraIntensity);

                float uvX = (float)sourceRect.X / texture.Width;
                float uvY = (float)sourceRect.Y / texture.Height;
                float uvW = (float)sourceRect.Width / texture.Width;
                float uvH = (float)sourceRect.Height / texture.Height;
                auraShader.Parameters["uSourceRect"].SetValue(new Vector4(uvX, uvY, uvW, uvH));
                
                auraShader.CurrentTechnique.Passes[0].Apply();

                spriteBatch.Draw(texture, drawPos, sourceRect, Color.White * MathHelper.Clamp(auraIntensity * 3f, 0f, 1f), NPC.rotation, drawOrigin, new Vector2(3.0f + auraIntensity * 0.6f, 4.0f + auraIntensity * 0.8f), effects, 0f);

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
            }

            DrawCollapseCore(spriteBatch, screenPos);

            return true;
        }

        private void DrawCollapseCore(SpriteBatch spriteBatch, Vector2 screenPos)
        {
            if (AIState != State_P2_CollapsePrep && AIState != State_P2_CollapseBurst)
                return;

            var bloom = TextureAssets.Extra[ExtrasID.ScreenObfuscation].Value;
            Vector2 origin = bloom.Size() * 0.5f;
            Vector2 pos = NPC.Center - screenPos;

            float coreScale;
            float bright;

            if (AIState == State_P2_CollapsePrep)
            {
                float charge = MathHelper.Clamp(AITimer / CollapseChargeTime, 0f, 1f);
                float pulse = 1f + (float)Math.Sin(AITimer * 0.35f) * 0.12f;

                coreScale = MathHelper.Lerp(0.4f, 1.3f, charge) * pulse;
                bright = MathHelper.Lerp(0.3f, 1f, charge);
            }
            else
            {
                float t = MathHelper.Clamp(AITimer / 30f, 0f, 1f);
                coreScale = MathHelper.Lerp(3.5f, 0.8f, t);
                bright = 1f - t * 0.7f;
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Color halo = new Color(220, 90, 230) * (bright * 0.7f);
            spriteBatch.Draw(bloom, pos, null, halo, 0f, origin, coreScale * 2.2f,
                SpriteEffects.None, 0f);

            Color mid = new Color(255, 120, 220) * (bright * 0.9f);
            spriteBatch.Draw(bloom, pos, null, mid, 0f, origin, coreScale * 1.3f,
                SpriteEffects.None, 0f);

            Color hot = new Color(255, 245, 255) * bright;
            spriteBatch.Draw(bloom, pos, null, hot, 0f, origin, coreScale * 0.6f,
                SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {

            if (NPC.alpha >= 255 || NPC.hide) return;

            int currentPhase = 1;
            int lagFrequency = Math.Max(2, 8 - currentPhase * 2); 
            
            int seed = (int)(Main.GameUpdateCount / lagFrequency) + NPC.whoAmI;
            Random rand = new Random(seed);

            float jitterAmount = currentPhase * 1.5f; 
            float jitterX = (float)(rand.NextDouble() * 2 - 1) * jitterAmount;
            float jitterY = (float)(rand.NextDouble() * 2 - 1) * jitterAmount;

            float eyeOffsetY = -53f;
            float eyeOffsetX = -10f * NPC.spriteDirection;
            
            Vector2 baseEyeOffset = new Vector2(eyeOffsetX, eyeOffsetY);
            Vector2 laggedOffset = baseEyeOffset + new Vector2(jitterX, jitterY);
            Vector2 eyePos = NPC.Center + laggedOffset.RotatedBy(NPC.rotation);

            Texture2D magicPixel = TextureAssets.MagicPixel.Value;
            Vector2 rectSize = new Vector2(40, 15);
            
            Main.EntitySpriteDraw(magicPixel, eyePos - screenPos, new Rectangle(0, 0, 1, 1), Color.White, NPC.rotation, new Vector2(0.5f, 0.5f), rectSize, SpriteEffects.None, 0);
        }

        public override void ModifyHitByItem(Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            if (enteredFinalPhase || (int)AIState == State_P4_FinalDeath)
            {
                modifiers.FinalDamage *= 0f;
            }
        }

        public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (enteredFinalPhase || (int)AIState == State_P4_FinalDeath)
            {
                modifiers.FinalDamage *= 0f;
            }
        }

        public override bool CheckActive()
        {
            return false;
        }

        public override bool CheckDead()
        {
            if (!enteredFinalPhase)
            {
                enteredFinalPhase = true;
                NPC.life = 1;
                NPC.dontTakeDamage = true;
                AIState = State_P4_FinalDeath;
                AITimer = 0;
                NPC.netUpdate = true;

                ClearHostileProjectiles();

                return false;
            }

            if ((int)AIState == State_P4_FinalDeath && AITimer >= 1680)
            {
                return true;
            }

            NPC.life = 1;
            return false;
        }

        private void FinalDeathBehavior(Player target)
        {
            AITimer++;
            NPC.dontTakeDamage = true;
            NPC.velocity *= 0.88f;
            NPC.life = 1;

            if (Main.netMode != NetmodeID.Server)
            {
                CelestiaEffectsSystem.BossCenter = NPC.Center;
                CelestiaEffectsSystem.Phase2KeepAlive = 10;
            }

            int waitDuration = 600;
            int attackDuration = 1080;

            if (AITimer <= waitDuration)
            {
                float progress = AITimer / waitDuration;

                if (AITimer % 25 == 0)
                {
                    float volume = MathHelper.Lerp(0.15f, 1.0f, progress);
                    SoundEngine.PlaySound(SoundID.Item117 with { Volume = volume, Pitch = -0.6f + progress * 0.5f }, NPC.Center);
                }

                if (Main.netMode != NetmodeID.Server && Main.rand.NextBool(2))
                {
                    Vector2 off = Main.rand.NextVector2Circular(140f, 140f);
                    Dust d = Dust.NewDustPerfect(NPC.Center + off, DustID.PinkTorch, -off * 0.04f, Scale: 1.2f + progress * 0.8f);
                    d.noGravity = true;
                }
            }
            else if (AITimer <= waitDuration + attackDuration)
            {
                float attackProgress = (AITimer - waitDuration) / attackDuration;

                float maxRadius = 1400f;
                for (int p = 0; p < Main.maxPlayers; p++)
                {
                    Player pl = Main.player[p];
                    if (!pl.active || pl.dead) continue;

                    float dist = Vector2.Distance(pl.Center, NPC.Center);
                    if (dist > maxRadius)
                    {
                        Vector2 dir = (pl.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
                        pl.Center = NPC.Center + dir * maxRadius;

                        if (Vector2.Dot(pl.velocity, dir) > 0)
                        {
                            pl.velocity -= dir * Vector2.Dot(pl.velocity, dir);
                        }
                    }

                    if (Main.netMode != NetmodeID.Server && Main.rand.NextBool(2))
                    {
                        float angle = Main.rand.NextFloat(0f, MathHelper.TwoPi);
                        Vector2 barrierPos = NPC.Center + angle.ToRotationVector2() * maxRadius;
                        Dust d = Dust.NewDustPerfect(barrierPos, DustID.PurpleTorch, Vector2.Zero, Scale: 1.6f);
                        d.noGravity = true;
                    }
                }

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int spawnInterval = (int)MathHelper.Lerp(8f, 1f, attackProgress);
                    int starsCount = attackProgress > 0.65f ? (int)MathHelper.Lerp(1f, 3f, (attackProgress - 0.65f) / 0.35f) : 1;

                    if (AITimer % Math.Max(1, spawnInterval) == 0)
                    {
                        for (int s = 0; s < starsCount; s++)
                        {
                            float screenHalfW = Main.screenWidth * 0.85f;
                            float spawnX = target.Center.X + Main.rand.NextFloat(-screenHalfW, screenHalfW);
                            float spawnY = target.Center.Y + (Main.screenHeight * 0.6f) + Main.rand.NextFloat(30f, 120f);

                            Vector2 spawnPos = new Vector2(spawnX, spawnY);

                            float roll = Main.rand.NextFloat();
                            float waveAmp = 0f;

                            if (roll > 0.75f)
                                waveAmp = Main.rand.NextFloat(0.05f, 0.085f);

                            else if (roll > 0.30f)
                                waveAmp = Main.rand.NextFloat(0.02f, 0.045f);

                            float waveFreq = Main.rand.NextFloat(0.035f, 0.07f);
                            float starScale = Main.rand.NextFloat(0.6f, 2.4f);

                            int proj = Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, new Vector2(0f, -8f), ModContent.ProjectileType<CelestiaRisingStar>(), 35, 0f, Main.myPlayer, waveAmp, waveFreq);

                            if (proj >= 0 && proj < Main.maxProjectiles)
                            {
                                Main.projectile[proj].scale = starScale;
                            }
                        }
                    }
                }
            }
            else
            {
                SoundEngine.PlaySound(SoundID.Item62 with { Volume = 1.5f, Pitch = -0.3f }, NPC.Center);
                SoundEngine.PlaySound(SoundID.Item122 with { Volume = 1.5f, Pitch = -0.5f }, NPC.Center);

                if (Main.netMode != NetmodeID.Server)
                {
                    Main.instance.CameraModifiers.Add(new PunchCameraModifier(NPC.Center, Main.rand.NextVector2Unit(), 35f, 12f, 60, 2000f, "CelestiaFinalDeath"));

                    for (int i = 0; i < 90; i++)
                    {
                        Vector2 vel = Main.rand.NextVector2Circular(22f, 22f);
                        Dust d = Dust.NewDustPerfect(NPC.Center, DustID.PinkTorch, vel, Scale: 2.6f);
                        d.noGravity = true;
                    }
                }

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.dontTakeDamage = false;
                    NPC.life = 0;
                    NPC.HitEffect();
                    NPC.NPCLoot();
                }
                
                NPC.active = false;
            }
        }

        private void ClearHostileProjectiles()
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.hostile)
                {
                    p.Kill();
                }
            }
        }
    }

    public class CelestiaAfterimage
    {
        public Vector2 Position;
        public float Rotation;
        public Rectangle SourceRect;
        public int SpriteDirection;
        public float Alpha;
        public float Scale;
    }
}
