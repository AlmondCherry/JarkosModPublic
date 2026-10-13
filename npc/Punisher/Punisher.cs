using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.GameContent.ItemDropRules;
using Terraria.Audio;
using jarkos.particles;
using jarkos.systems;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;
using Terraria.UI.Chat;
using System;
using Filters = Terraria.Graphics.Effects.Filters;
using jarkos.items.bags;
using System.IO;
using Terraria.Graphics.CameraModifiers;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.Chat;
using Terraria.Localization;
using ReLogic.Graphics;

namespace jarkos.npc.Punisher
{
    [AutoloadBossHead]
    public class Punisher : ModNPC
    {
        private const int State_Idle = 0;
        private const int State_PhantomWalk = 1;
        private const int State_LazySlash = 2;
        private const int State_Parry = 3;
        private const int State_CounterAttack = 4;
        private const int State_AntiAir = 5;
        private const int State_PhaseTransition = 6;
        private const int State_RealitySlash = 7;
        private const int State_ExecutionersFlurry = 8;
        private const int State_DimensionShatter = 9;
        private const int State_Phase3Transition = 10;
        private const int State_Phase3Hover = 11;
        private const int State_Kick = 12;
        private const int State_Deathray = 13;
        private const int State_BulletHell = 14;
        private const int State_Omnislash = 15;
        private const int State_DimensionSunder = 16;
        private const int State_FinalPhaseStart = 17;
        private const int State_DialogueAndChoice = 18;
        private const int State_SpareDialogue = 19;
        private const int State_FinalPhaseAttack = 20;
        private int LastAttackState = -1;
        private bool isFinalPhase = false;
        public string currentFloatingText = "";
        public float textAlpha = 0f;
        public bool Enraged => !Main.dayTime == false;
        public ref float DeathrayAngle => ref NPC.ai[2];
        public ref float DeathraySweepDir => ref NPC.ai[3];
        public bool IsPhase2 => NPC.localAI[1] >= 1f;
        public bool IsPhase3 => NPC.localAI[1] == 2f;
        public ref float AI_State => ref NPC.ai[0];
        public ref float AI_Timer => ref NPC.ai[1];
        public Vector2 preFightPlayerPos = Vector2.Zero;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 20;
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.TrailCacheLength[NPC.type] = 6;
            NPCID.Sets.TrailingMode[NPC.type] = 0;
        }
        public override void SetDefaults()
        {
            NPC.width = 48;
            NPC.height = 52;
            NPC.damage = 50;
            NPC.defense = 20;
            NPC.lifeMax = 15000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = Item.buyPrice(gold: 15);
            NPC.knockBackResist = 0f;
            NPC.boss = true;
            NPC.lavaImmune = true;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            if (Main.netMode != NetmodeID.Server)
            {
                Music = MusicLoader.GetMusicSlot(Mod, "Music/Mamoru!");
            }
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                new FlavorTextBestiaryInfoElement("...")
            });
        }
        public bool IsEnraged => NPC.localAI[0] == 1f;
        public override void AI()
        {
            Main.cloudAlpha = 0f;
            NPC.TargetClosest(true);
            Player player = Main.player[NPC.target];

            if (player.dead || !player.active)
            {
                NPC.TargetClosest(false);
                player = Main.player[NPC.target];

                if (player.dead || !player.active)
                {
                    NPC.velocity.Y -= 0.5f;
                    NPC.EncourageDespawn(10);

                    if (NPC.timeLeft > 10) NPC.timeLeft = 10;
                    if (PunisherCinematicSystem.WorldBlackoutOpacity > 0f) PunisherCinematicSystem.WorldBlackoutOpacity -= 0.05f;

                    if (NPC.position.Y < 0 || NPC.timeLeft <= 1)
                    {
                        NPC.active = false;
                        NPC.netUpdate = true;
                    }
                    return;
                }
            }
            
            NPC.timeLeft = NPC.activeTime * 30;

            if (AI_State != State_CounterAttack && AI_State != State_RealitySlash)
                NPC.spriteDirection = NPC.Center.X < player.Center.X ? -1 : 1;

            if (IsPhase3)
            {
                PunisherCinematicSystem.HexBarrierCenter = new Vector2(Main.maxTilesX * 8f, 130f * 16f);
                PunisherCinematicSystem.HexBarrierSize = new Vector2(4000f, 1500f);
                PunisherCinematicSystem.HexBarrierOpacity = 1f;

                Lighting.AddLight(NPC.Center, 2f, 1.5f, 2f);
                int buffIndex = player.FindBuffIndex(ModContent.BuffType<ClippedWings>());
                if (buffIndex != -1)
                {
                    player.DelBuff(buffIndex);
                }
                player.wingTime = player.wingTimeMax;
                Lighting.AddLight(player.Center, 0.8f, 0.8f, 1f);

                float centerX = PunisherCinematicSystem.HexBarrierCenter.X;
                float centerY = PunisherCinematicSystem.HexBarrierCenter.Y;
                float arenaWidth = PunisherCinematicSystem.HexBarrierSize.X;
                float arenaHeight = PunisherCinematicSystem.HexBarrierSize.Y;

                float floorY = centerY + arenaHeight;
                float ceilY = centerY - arenaHeight;

                bool enforceBarrier = true;

                if (AI_State == State_SpareDialogue && AI_Timer >= 300) enforceBarrier = false;
                if (AI_State == State_FinalPhaseStart) enforceBarrier = false;
                if (AI_State == State_FinalPhaseAttack) enforceBarrier = false;

                if (enforceBarrier)
                {
                    if (player.position.Y + player.height > floorY)
                    {
                        player.position.Y = floorY - player.height;

                        if (player.velocity.Y > 0) player.velocity.Y = -3f;
                        player.fallStart = (int)(player.position.Y / 16f);
                    }
                    if (player.position.Y < ceilY)
                    {
                        player.position.Y = ceilY;
                        if (player.velocity.Y < 0) player.velocity.Y = 3f;
                    }
                    if (player.position.X < centerX - arenaWidth)
                    {
                        player.position.X = centerX - arenaWidth;
                        if (player.velocity.X < 0) player.velocity.X = 5f;
                    }
                    if (player.position.X + player.width > centerX + arenaWidth)
                    {
                        player.position.X = centerX + arenaWidth - player.width;
                        if (player.velocity.X > 0) player.velocity.X = -5f;
                    }
                    if (NPC.position.Y > floorY)
                    {
                        NPC.position.Y = floorY;
                        if (NPC.velocity.Y > 0) NPC.velocity.Y = -3f;
                    }
                    if (NPC.position.Y < ceilY)
                    {
                        NPC.position.Y = ceilY;
                        if (NPC.velocity.Y < 0) NPC.velocity.Y = 3f;
                    }
                    if (NPC.position.X < centerX - arenaWidth)
                    {
                        NPC.position.X = centerX - arenaWidth;
                        if (NPC.velocity.X < 0) NPC.velocity.X = 5f;
                    }
                    if (NPC.position.X > centerX + arenaWidth)
                    {
                        NPC.position.X = centerX + arenaWidth;
                        if (NPC.velocity.X > 0) NPC.velocity.X = -5f;
                    }
                }
            }
            if (!isFinalPhase && !IsPhase2 && NPC.life < NPC.lifeMax * 0.6f && AI_State != State_PhaseTransition && AI_State != State_Phase3Transition && AI_State != State_CounterAttack && AI_State != State_AntiAir)
            {
                AI_State = State_PhaseTransition;
                AI_Timer = 0;
                NPC.velocity = Vector2.Zero;
            }
            if (!isFinalPhase && IsPhase2 && !IsPhase3 && NPC.life < NPC.lifeMax * 0.2f && AI_State != State_Phase3Transition && AI_State != State_CounterAttack && AI_State != State_AntiAir)
            {
                AI_State = State_Phase3Transition;
                AI_Timer = 0;
                NPC.velocity = Vector2.Zero;
            }
            if (AI_State != State_PhaseTransition && AI_State != State_Phase3Transition)
            {
                if (Vector2.Distance(player.Center, NPC.Center) > 1200f)
                {
                    player.AddBuff(ModContent.BuffType<ClippedWings>(), 300);
                    if (player.mount.Active) player.mount.Dismount(player);

                    NPC.Center = player.Center + new Vector2(player.direction * 150f, -100f);
                    SoundEngine.PlaySound(SoundID.Item8, NPC.Center);

                    for (int i = 0; i < 30; i++)
                    {
                        new Spark(NPC.Center, Main.rand.NextVector2Circular(5f, 5f), Color.Purple, 30, Main.rand.NextFloat(1f, 2f), 1f).Spawn();
                    }
                }
            }
            if (AI_State == State_Idle)
            {
                float distX = Math.Abs(player.Center.X - NPC.Center.X);
                float dist = Vector2.Distance(NPC.Center, player.Center);

                bool canSeePlayer = Collision.CanHitLine(NPC.position, NPC.width, NPC.height, player.position, player.width, player.height);
                if (IsPhase3)
                {
                    NPC.noGravity = true;
                    NPC.noTileCollide = true;

                    Vector2 targetPos = player.Center + new Vector2(0, -300f);

                    targetPos.X += (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2f) * 300f;
                    targetPos.Y += (float)Math.Cos(Main.GlobalTimeWrappedHourly * 3.5f) * 100f;

                    Vector2 dir = targetPos - NPC.Center;
                    if (dir.Length() > 20f)
                    {
                        dir.Normalize();
                        NPC.velocity = (NPC.velocity * 15f + dir * 25f) / 16f;
                    }
                }
                else
                {
                    NPC.noTileCollide = false;
                    NPC.noGravity = false;

                    float speedBase = IsEnraged ? 6f : 2.5f;
                    float speedBoost = IsEnraged ? 14f : 9f;

                    float targetSpeedX = (distX > 300f) ? speedBoost : speedBase;
                    float moveDir = player.Center.X > NPC.Center.X ? 1f : -1f;

                    bool hitWall = NPC.collideX && Math.Abs(NPC.velocity.X) < 0.5f;

                    NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, moveDir * targetSpeedX, 0.05f);
                    Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);

                    bool pitAhead = false;
                    if (NPC.velocity.Y == 0)
                    {
                        int checkX = (int)(NPC.Center.X / 16f) + (int)(moveDir * 2f);
                        int checkY = (int)(NPC.Bottom.Y / 16f);
                        bool foundGround = false;

                        for (int i = -1; i < 4; i++)
                        {
                            if (WorldGen.SolidTile(checkX, checkY + i))
                            {
                                foundGround = true;
                                break;
                            }
                        }
                        if (!foundGround) pitAhead = true;
                    }
                    if (NPC.velocity.Y == 0 || hitWall)
                    {
                        if (hitWall || pitAhead || player.Bottom.Y < NPC.Top.Y - 20f)
                        {
                            NPC.velocity.Y = IsEnraged ? -16f : -12f;
                        }
                    }
                }
                if (NPC.velocity.Y < 0 && dist < 120f && player.Bottom.Y < NPC.Bottom.Y)
                {
                    AI_State = State_Kick;
                    AI_Timer = 0;
                    return;
                }
                if (!IsPhase3 && !player.HasBuff(ModContent.BuffType<ClippedWings>()) && player.Center.Y < NPC.Center.Y - 400f)
                {
                    AI_State = State_AntiAir;
                    AI_Timer = 0;
                    return;
                }
                AI_Timer++;
                if (dist > 800f || (!canSeePlayer && dist > 400f))
                {
                    AI_State = State_PhantomWalk;
                    AI_Timer = 0;
                }

                int attackDelay = IsEnraged ? 60 : 120;

                if (IsPhase2) attackDelay = 50;
                if (isFinalPhase) attackDelay = 15;
                if (AI_Timer > attackDelay)
                {
                    AI_Timer = 0;
                    int nextState = State_Idle;

                    for (int i = 0; i < 3; i++)
                    {
                        int rand = Main.rand.Next(100);
                        if (IsPhase3)
                        {
                            if (rand < 25) nextState = State_Deathray;
                            else if (rand < 50) nextState = State_BulletHell;
                            else if (rand < 75) nextState = State_Omnislash;
                            else nextState = State_DimensionSunder;
                        }
                        else if (IsPhase2)
                        {
                            if (rand < 5) nextState = State_Parry;
                            else if (rand < 35) nextState = State_RealitySlash;
                            else if (rand < 65) nextState = State_ExecutionersFlurry;
                            else nextState = State_DimensionShatter;
                        }
                        else
                        {
                            if (dist < 100f && rand < 40) nextState = State_Kick;
                            else if (rand < 85) nextState = State_LazySlash;
                            else nextState = State_Parry;
                        }
                        if (nextState != LastAttackState) break;
                    }
                    LastAttackState = nextState;
                    AI_State = nextState;
                }
            }
            else if (AI_State == State_LazySlash)
            {
                NPC.velocity *= 0.9f;
                AI_Timer++;
                if (AI_Timer == 30)
                {
                    SoundEngine.PlaySound(SoundID.Item71, NPC.Center);
                    PunisherCinematicSystem.ScreenShakeAmount = 8f;

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        if (isFinalPhase)
                        {
                            for (int i = -2; i <= 2; i++)
                            {
                                Vector2 shootDir = Vector2.Normalize(player.Center - NPC.Center).RotatedBy(i * 0.15f) * 20f;
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootDir, ModContent.ProjectileType<PunisherSwordProj>(), NPC.damage, 0f, Main.myPlayer, 0, NPC.whoAmI);
                            }
                        }
                        else
                        {
                            Vector2 shootDir = Vector2.Normalize(player.Center - NPC.Center) * 15f;
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootDir, ModContent.ProjectileType<PunisherSwordProj>(), NPC.damage / 2, 0f, Main.myPlayer, 0, NPC.whoAmI);
                        }
                    }
                }
                if (AI_Timer >= (isFinalPhase ? 40 : 60))
                {
                    AI_State = State_Idle;
                    AI_Timer = 0;
                }
            }
            else if (AI_State == State_Kick)
            {
                NPC.velocity *= 0.8f;
                AI_Timer++;
                if (AI_Timer == 15)
                {
                    SoundEngine.PlaySound(SoundID.Item71 with { Volume = 1.2f, Pitch = -0.2f }, NPC.Center);
                    PunisherCinematicSystem.ScreenShakeAmount = 15f;
                    for (int i = 0; i < 20; i++)
                    {
                        new Spark(NPC.Center, new Vector2(NPC.direction * Main.rand.NextFloat(2f, 8f), Main.rand.NextFloat(-2f, 2f)), Color.Gray, 40, Main.rand.NextFloat(1f, 3f), 1f).Spawn();
                    }
                    if (player.Distance(NPC.Center) < 120f)
                    {
                        player.velocity.X = NPC.direction * 25f;
                        player.velocity.Y = -5f;
                        SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/kick"), player.Center);
                        for (int i = 0; i < 15; i++)
                        {
                            new Spark(player.Center, Main.rand.NextVector2Circular(10f, 10f), Color.LightCyan, 30, Main.rand.NextFloat(0.5f, 1.2f), 1f).Spawn();
                            new High_smoke(player.Center, Main.rand.NextVector2Circular(5f, 5f), Color.Gray, 40, Main.rand.NextFloat(0.8f, 1.5f), 0.7f).Spawn();
                        }
                        player.AddBuff(ModContent.BuffType<PunisherKicked>(), 30);
                    }
                }
                if (AI_Timer >= (isFinalPhase ? 20 : 35))
                {
                    AI_State = State_Idle;
                    AI_Timer = 0;
                }
            }
            else if (AI_State == State_Deathray)
            {
                AI_Timer++;
                NPC.velocity *= 0.8f;

                if (AI_Timer <= 120)
                {
                    if (AI_Timer <= 90)
                    {
                        Vector2 dir = player.Center - NPC.Center;
                        DeathrayAngle = dir.ToRotation();
                        NPC.direction = player.Center.X > NPC.Center.X ? 1 : -1;
                    }
                    float focusProgress = AI_Timer / 120f;
                    if (!Filters.Scene["jarkos:DeathrayFocus"].IsActive())
                    {
                        Filters.Scene.Activate("jarkos:DeathrayFocus", NPC.Center);
                    }

                    Filters.Scene["jarkos:DeathrayFocus"].GetShader().UseOpacity(focusProgress);

                    if (Main.rand.NextBool(2))
                    {
                        Vector2 dustPos = NPC.Center + Main.rand.NextVector2CircularEdge(200f, 200f);
                        Vector2 velocity = (NPC.Center - dustPos) * 0.08f;
                        new Spark(dustPos, velocity, Color.White, 30, Main.rand.NextFloat(0.5f, 1.5f), 1f).Spawn();
                    }

                    if (AI_Timer > 60)
                    {
                        float shakeIntensity = (AI_Timer - 60f) / 60f * 4f;
                        NPC.Center += Main.rand.NextVector2Circular(shakeIntensity, shakeIntensity);
                    }

                    if (Main.curMusic >= 0 && Main.curMusic < Main.musicFade.Length)
                    {
                        Main.musicFade[Main.curMusic] = 1f - (AI_Timer / 120f);
                    }
                }
                if (AI_Timer == 120)
                {
                    SoundEngine.PlaySound(new SoundStyle("jarkos/npc/Punisher/DeathrayFire") with { Volume = 0.8f }, NPC.Center);
                    PunisherCinematicSystem.ScreenShakeAmount = 20f;

                    if (!Filters.Scene["jarkos:DeathrayOverbloom"].IsActive())
                    {
                        Filters.Scene.Activate("jarkos:DeathrayOverbloom", NPC.Center);
                    }
                }
                if (AI_Timer > 120 && AI_Timer < 324)
                {
                    Main.musicFade[Main.curMusic] = 0f;
                    
                    Vector2 toPlayerCurrent = player.Center - NPC.Center;

                    float targetAngleCurrent = toPlayerCurrent.ToRotation();
                    float diff = MathHelper.WrapAngle(targetAngleCurrent - DeathrayAngle);
                    float trackingSpeed = isFinalPhase ? 0.025f : 0.015f;

                    DeathrayAngle += Math.Clamp(diff, -trackingSpeed, trackingSpeed);
                    Vector2 toPlayer = player.Center - NPC.Center;

                    float angleToPlayer = toPlayer.ToRotation();
                    float angleDiff = MathHelper.WrapAngle(angleToPlayer - DeathrayAngle);

                    if (Math.Abs(angleDiff) < 0.2f && toPlayer.Length() < 2500f)
                    {
                        int damage = Main.expertMode ? (isFinalPhase ? 500 : 300) : (isFinalPhase ? 250 : 150);
                    }
                    Filters.Scene["jarkos:DeathrayFocus"].GetShader().UseOpacity(1f);
                    if (Filters.Scene["jarkos:DeathrayOverbloom"].IsActive())
                    {
                        float bloomIntensity = 0f;
                        if (AI_Timer < 150) bloomIntensity = (AI_Timer - 120f) / 30f;
                        else bloomIntensity = 1f - ((AI_Timer - 150f) / 180f);

                        Filters.Scene["jarkos:DeathrayOverbloom"].GetShader().UseOpacity(bloomIntensity);
                    }
                    if (AI_Timer % 3 == 0)
                    {
                        PunchCameraModifier modifier = new PunchCameraModifier(NPC.Center, Main.rand.NextVector2Circular(1f, 1f), 8f, 6f, 10, 3000f, NPC.FullName);
                        Main.instance.CameraModifiers.Add(modifier);
                    }
                }
                if (AI_Timer >= 324)
                {
                    float fadeProgress = 1f - ((AI_Timer - 324f) / 15f);
                    if (fadeProgress < 0f) fadeProgress = 0f;
                    Filters.Scene["jarkos:DeathrayFocus"].GetShader().UseOpacity(fadeProgress);
                    if (Filters.Scene["jarkos:DeathrayOverbloom"].IsActive())
                    {
                        Filters.Scene["jarkos:DeathrayOverbloom"].GetShader().UseOpacity(0f);
                    }
                }
                if (AI_Timer >= 339)
                {
                    if (Filters.Scene["jarkos:DeathrayFocus"].IsActive())
                    {
                        Filters.Scene.Deactivate("jarkos:DeathrayFocus");
                    }
                    if (Filters.Scene["jarkos:DeathrayOverbloom"].IsActive())
                    {
                        Filters.Scene.Deactivate("jarkos:DeathrayOverbloom");
                    }
                    AI_State = State_Idle;
                    AI_Timer = 0;
                }
            }
            else if (AI_State == State_BulletHell)
            {
                AI_Timer++;
                if (AI_Timer < 60)
                {
                    if (AI_Timer == 1)
                    {
                        SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
                        NPC.Center = player.Center - new Vector2(0, 400f);
                        NPC.velocity = Vector2.Zero;

                        for (int i = 0; i < 30; i++)
                        {
                            new Spark(NPC.Center, Main.rand.NextVector2Circular(6f, 6f), Color.Purple, 25, Main.rand.NextFloat(1f, 2f), 1f).Spawn();
                        }
                    }
                    NPC.velocity = Vector2.Zero;
                }
                else if (AI_Timer >= 60 && AI_Timer < 360)
                {
                    NPC.velocity = Vector2.Zero;
                    NPC.rotation += 0.2f;

                    int bulletHellTick = isFinalPhase ? 10 : 7;
                    if (AI_Timer % bulletHellTick == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        SoundEngine.PlaySound(SoundID.Item20 with { Volume = 0.5f, Pitch = -0.5f }, NPC.Center);
                        int projCount = isFinalPhase ? 4 : 2;

                        for (int i = 0; i < projCount; i++)
                        {
                            float shootAngle = (AI_Timer * 0.15f) + (i * MathHelper.TwoPi / projCount);
                            Vector2 shootVelocity = shootAngle.ToRotationVector2() * (isFinalPhase ? 3.5f : 3f);
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootVelocity, ModContent.ProjectileType<DespairSphere>(), NPC.damage / 3, 0f, Main.myPlayer);
                        }
                    }
                }
                else if (AI_Timer >= 360)
                {
                    NPC.rotation *= 0.9f;
                    if (AI_Timer > 420)
                    {
                        NPC.rotation = 0;
                        AI_State = State_Idle;
                        AI_Timer = 0;
                    }
                }
            }
            else if (AI_State == State_Omnislash)
            {
                AI_Timer++;
                NPC.dontTakeDamage = true;
                if (AI_Timer < 60)
                {
                    if (AI_Timer == 1)
                    {
                        SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
                    }
                    NPC.alpha += 5;
                    NPC.velocity *= 0.8f;
                }
                else if (AI_Timer >= 60 && AI_Timer < 600)
                {
                    NPC.alpha = 255;
                    NPC.velocity = Vector2.Zero;
                    NPC.localAI[0]++;

                    float progress = (AI_Timer - 60f) / 540f;
                    int currentDelay = (int)MathHelper.Lerp(15f, 1f, progress);

                    if (NPC.localAI[0] >= currentDelay && Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        NPC.localAI[0] = 0;
                        int spawnCount = progress > 0.8f ? 3 : (progress > 0.5f ? 2 : 1);
                        for (int i = 0; i < spawnCount; i++)
                        {
                            float gridSize = 400f;
                            int gridPhase = (int)(AI_Timer / 120);
                            float gridAngle = gridPhase * 0.5f;

                            Vector2 playerRotated = player.Center.RotatedBy(-gridAngle);

                            int playerGridX = (int)Math.Round(playerRotated.X / gridSize);
                            int playerGridY = (int)Math.Round(playerRotated.Y / gridSize);
                            int offsetIndex = Main.rand.Next(-5, 6);

                            bool isHorizontal = Main.rand.NextBool();
                            Vector2 spawnPosRotated;
                            float angle;

                            if (isHorizontal)
                            {
                                float lineY = (playerGridY + offsetIndex + 0.5f) * gridSize;
                                spawnPosRotated = new Vector2(playerRotated.X, lineY);
                                angle = gridAngle;
                            }
                            else
                            {
                                float lineX = (playerGridX + offsetIndex + 0.5f) * gridSize;
                                spawnPosRotated = new Vector2(lineX, playerRotated.Y);
                                angle = gridAngle + MathHelper.PiOver2;
                            }
                            Vector2 spawnPos = spawnPosRotated.RotatedBy(gridAngle);
                            float length = 6000f;
                            int proj = Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, Vector2.Zero, ModContent.ProjectileType<CosmicSlash>(), NPC.damage / 2, 0f, Main.myPlayer);
                            Main.projectile[proj].rotation = angle;
                            Main.projectile[proj].ai[1] = length;
                        }
                    }
                }
                else if (AI_Timer >= 600)
                {
                    NPC.alpha -= 10;
                    if (NPC.alpha <= 0)
                    {
                        NPC.alpha = 0;
                        NPC.dontTakeDamage = false;
                        AI_State = State_Idle;
                        AI_Timer = 0;
                    }
                }
            }
            else if (AI_State == State_DimensionSunder)
            {
                AI_Timer++;
                NPC.velocity *= 0.8f;
                if (AI_Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item117, NPC.Center);
                }
                if (AI_Timer == 45)
                {
                    SoundEngine.PlaySound(SoundID.Item71, NPC.Center);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Player targetPlayer = Main.player[NPC.target];
                        float isVertical = Main.rand.NextBool() ? 1f : 0f;
                        Vector2 spawnPos = targetPlayer.Center;
                        if (isVertical == 1f) spawnPos.X += Main.rand.NextFloat(-250f, 250f);
                        else spawnPos.Y += Main.rand.NextFloat(-250f, 250f);
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, Vector2.Zero, ModContent.ProjectileType<DimensionSunderProj>(), NPC.damage, 0f, Main.myPlayer, isVertical);
                    }
                }
                if (AI_Timer > 60)
                {
                    AI_State = State_Idle;
                    AI_Timer = 0;
                }
            }
            else if (AI_State == State_PhantomWalk)
            {
                AI_Timer++;
                if (AI_Timer < 30)
                {
                    NPC.alpha += 10;
                    NPC.velocity *= 0.8f;
                }
                else if (AI_Timer == 30)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, -2f), ModContent.ProjectileType<PunisherSmokeProj>(), 0, 0, Main.myPlayer);
                    }
                    Vector2[] offsets = new Vector2[] {
                        new Vector2(player.direction * -200f, -50f), new Vector2(player.direction * -200f, -150f), new Vector2(player.direction * 200f, -50f), new Vector2(0, -250f)
                    };
                    bool foundSpot = false;
                    foreach (Vector2 offset in offsets)
                    {
                        Vector2 testPos = player.Center + offset - NPC.Size / 2f;
                        if (!Collision.SolidCollision(testPos, NPC.width, NPC.height))
                        {
                            NPC.Center = testPos + NPC.Size / 2f;
                            foundSpot = true;
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, -1f), ModContent.ProjectileType<PunisherSmokeProj>(), 0, 0, Main.myPlayer);
                            }
                            break;
                        }
                    }
                    if (!foundSpot)
                    {
                        NPC.position = player.Center - NPC.Size / 2f;
                    }
                }
                else if (AI_Timer > 30 && AI_Timer < 60)
                {
                    NPC.alpha -= 10;
                }
                else if (AI_Timer >= 60)
                {
                    NPC.alpha = 0;
                    AI_State = State_Idle;
                    AI_Timer = 1000;
                }
            }
            else if (AI_State == State_DialogueAndChoice)
            {
                if (Main.curMusic >= 0 && Main.curMusic < Main.musicFade.Length) Main.musicFade[Main.curMusic] = 0f;
                Player p = Main.player[NPC.target];
                p.GetModPlayer<PunisherPlayer>().inCutscene = true;

                p.immune = true;
                p.immuneTime = 60;
                p.immuneNoBlink = true;

                for (int j = 0; j < Main.maxProjectiles; j++)
                {
                    if (Main.projectile[j].active && Main.projectile[j].hostile)
                        Main.projectile[j].Kill();
                }

                NPC.dontTakeDamage = true;
                NPC.velocity = Vector2.Lerp(NPC.velocity, (p.Center - new Vector2(0, 300f) - NPC.Center) * 0.05f, 0.1f);
                NPC.direction = (p.Center.X > NPC.Center.X) ? 1 : -1;

                NPC.spriteDirection = -NPC.direction;
                NPC.rotation = 0f;

                AI_Timer++;

                int[] textTimes = { 0, 300, 900, 1080, 1500 };
                
                string[] texts = {
                    "Do you think I can just die here?", "I'd be lying to myself now if I hadn't noticed that you were really strong in that battle.", "in any case...", "As a still worthy warrior, I will fight to the very end", "Let's have one last fight..."
                };
                for (int i = 0; i < 5; i++)
                {
                    if (AI_Timer == textTimes[i] + 1)
                    {
                        SoundEngine.PlaySound(new SoundStyle("jarkos/npc/Punisher/Sounds/Voice/say_" + (i + 1)), NPC.Center);
                    }
                }

                int[] textDurations = { 300, 600, 180, 420, 300 };
                bool displayingText = false;

                for (int i = 0; i < 5; i++)
                {
                    if (AI_Timer >= textTimes[i] && AI_Timer < textTimes[i] + textDurations[i])
                    {
                        currentFloatingText = texts[i];
                        textAlpha = MathHelper.Clamp((AI_Timer - textTimes[i]) / 30f, 0f, 1f);

                        if (AI_Timer > textTimes[i] + textDurations[i] - 30)
                            textAlpha = MathHelper.Clamp((textTimes[i] + textDurations[i] - AI_Timer) / 30f, 0f, 1f);

                        displayingText = true;
                        break;
                    }
                }
                if (!displayingText) textAlpha = 0f;
                if (AI_Timer == 1800 && Main.netMode != NetmodeID.Server)
                {
                    UI.PunisherChoiceUI.Visible = true;
                }
                if (AI_Timer > 1800)
                {
                    AI_Timer = 1800;
                }
            }
            else if (AI_State == State_SpareDialogue)
            {
                if (Main.curMusic >= 0 && Main.curMusic < Main.musicFade.Length) Main.musicFade[Main.curMusic] = 0f;
                Player p = Main.player[NPC.target];

                p.GetModPlayer<PunisherPlayer>().inCutscene = true;

                p.immune = true;
                p.immuneTime = 60;
                p.immuneNoBlink = true;

                for (int j = 0; j < Main.maxProjectiles; j++)
                {
                    if (Main.projectile[j].active && Main.projectile[j].hostile)
                        Main.projectile[j].Kill();
                }
                if (AI_Timer == 300)
                {
                    Vector2 groundPos = (preFightPlayerPos != Vector2.Zero) ? preFightPlayerPos : p.Center;
                    for (int y = (int)(groundPos.Y / 16f); y < Main.maxTilesY - 10; y++)
                    {
                        if (Main.tile[(int)(groundPos.X / 16f), y].HasTile && Main.tileSolid[Main.tile[(int)(groundPos.X / 16f), y].TileType])
                        {
                            groundPos.Y = y * 16f - p.height / 2f;
                            break;
                        }
                    }
                    float relativeY = NPC.Center.Y - p.Center.Y;

                    p.Center = groundPos;
                    p.velocity = Vector2.Zero;
                    p.fallStart = (int)(p.position.Y / 16f);

                    NPC.Center = p.Center + new Vector2(0, relativeY);
                    NPC.velocity = Vector2.Zero;
                    NPC.netUpdate = true;

                    p.ClearBuff(ModContent.BuffType<ClippedWings>());
                }
                if (AI_Timer >= 300)
                {
                    if (Filters.Scene["jarkos:PunisherVignette"] != null && Filters.Scene["jarkos:PunisherVignette"].IsActive())
                    {
                        Filters.Scene["jarkos:PunisherVignette"].Opacity -= 0.015f;
                        if (Filters.Scene["jarkos:PunisherVignette"].Opacity <= 0)
                            Filters.Scene.Deactivate("jarkos:PunisherVignette");
                    }
                    if (Filters.Scene["jarkos:VoidTear"] != null && Filters.Scene["jarkos:VoidTear"].IsActive())
                    {
                        Filters.Scene["jarkos:VoidTear"].Opacity -= 0.015f;
                        if (Filters.Scene["jarkos:VoidTear"].Opacity <= 0)
                            Filters.Scene.Deactivate("jarkos:VoidTear");
                    }
                    if (Filters.Scene["jarkos:DimensionSunder"] != null && Filters.Scene["jarkos:DimensionSunder"].IsActive())
                    {
                        Filters.Scene["jarkos:DimensionSunder"].Opacity -= 0.015f;
                        if (Filters.Scene["jarkos:DimensionSunder"].Opacity <= 0)
                            Filters.Scene.Deactivate("jarkos:DimensionSunder");
                    }
                    PunisherCinematicSystem.WorldBlackoutOpacity -= 0.015f;
                    if (PunisherCinematicSystem.WorldBlackoutOpacity < 0) PunisherCinematicSystem.WorldBlackoutOpacity = 0;
                }
                NPC.dontTakeDamage = true;
                NPC.direction = (p.Center.X > NPC.Center.X) ? 1 : -1;
                NPC.spriteDirection = -NPC.direction;
                NPC.rotation = 0f;
                if (AI_Timer < 300)
                {
                    NPC.noGravity = true;
                    NPC.noTileCollide = true;
                    NPC.velocity = Vector2.Lerp(NPC.velocity, (p.Center - new Vector2(0, 300f) - NPC.Center) * 0.05f, 0.1f);
                }
                else
                {
                    NPC.noTileCollide = false;
                    if (NPC.velocity.Y == 0 && NPC.oldVelocity.Y != 0 && NPC.ai[2] == 0 && AI_Timer > 420)
                    {
                        NPC.ai[2] = 1;
                        if (Main.netMode != NetmodeID.Server)
                        {
                            Gore.NewGore(NPC.GetSource_FromAI(), NPC.Center + new Vector2(-20, 10), new Vector2(-1.5f, -2f), ModContent.GoreType<PunisherWingOne>());
                            Gore.NewGore(NPC.GetSource_FromAI(), NPC.Center + new Vector2(20, 10), new Vector2(1.5f, -2f), ModContent.GoreType<PunisherWingsTwo>());
                        }
                        SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
                        PunisherCinematicSystem.ScreenShakeAmount = 5f;
                    }
                    if (NPC.ai[2] == 0)
                    {
                        NPC.noGravity = true;
                        NPC.velocity.X = 0f;
                        if (AI_Timer < 420)
                        {
                            NPC.velocity.Y = 0f;
                        }
                        else
                        {
                            NPC.velocity.Y = 0.4f;
                        }
                    }
                    else
                    {
                        NPC.noGravity = false;
                        if (NPC.velocity.Y == 0)
                        {
                            NPC.velocity.X *= 0.8f;
                        }
                    }
                }
                AI_Timer++;
                int[] textTimes = { 0, 300, 660, 960, 1440 };
                string[] texts = {
                    "what?", "this...", "this... That's really generous of you.", "Of course, I have disgraced my honor quite a lot in this battle.", "I'm ready to start training again to get a rematch with you!"
                };
                for (int i = 0; i < 5; i++)
                {
                    if (AI_Timer == textTimes[i] + 1)
                    {
                        SoundEngine.PlaySound(new SoundStyle("jarkos/npc/Punisher/Sounds/Voice/spare_" + (i + 1)), NPC.Center);
                    }
                }
                int[] textDurations = { 300, 300, 240, 420, 300 };
                bool displayingText = false;
                for (int i = 0; i < 5; i++)
                {
                    if (AI_Timer >= textTimes[i] && AI_Timer < textTimes[i] + textDurations[i])
                    {
                        currentFloatingText = texts[i];
                        textAlpha = MathHelper.Clamp((AI_Timer - textTimes[i]) / 30f, 0f, 1f);
                        if (AI_Timer > textTimes[i] + textDurations[i] - 30)
                            textAlpha = MathHelper.Clamp((textTimes[i] + textDurations[i] - AI_Timer) / 30f, 0f, 1f);
                        displayingText = true;
                        break;
                    }
                }
                if (!displayingText) textAlpha = 0f;
                if (AI_Timer == 1800)
                {
                    if (Main.netMode != NetmodeID.Server)
                    {
                        Main.NewText("The Punisher will definitely accept your rematch!", new Color(200, 50, 200));
                    }
                    p.GetModPlayer<PunisherPlayer>().inCutscene = false;
                    NPC.active = false;
                    NPC.netUpdate = true;
                    NPC.SetEventFlagCleared(ref DownedBossSystem.downedPunisher, -1);
                }
            }
            else if (AI_State == State_FinalPhaseStart)
            {
                if (AI_Timer == 0)
                {
                    if (Filters.Scene["jarkos:PunisherVignette"] != null && Filters.Scene["jarkos:PunisherVignette"].IsActive()) Filters.Scene.Deactivate("jarkos:PunisherVignette");
                    if (Filters.Scene["jarkos:VoidTear"] != null && Filters.Scene["jarkos:VoidTear"].IsActive()) Filters.Scene.Deactivate("jarkos:VoidTear");
                    if (Filters.Scene["jarkos:DimensionSunder"] != null && Filters.Scene["jarkos:DimensionSunder"].IsActive()) Filters.Scene.Deactivate("jarkos:DimensionSunder");
                    if (Filters.Scene["jarkos:DeathrayFocus"] != null && Filters.Scene["jarkos:DeathrayFocus"].IsActive()) Filters.Scene.Deactivate("jarkos:DeathrayFocus");
                    if (Filters.Scene["jarkos:DeathrayOverbloom"] != null && Filters.Scene["jarkos:DeathrayOverbloom"].IsActive()) Filters.Scene.Deactivate("jarkos:DeathrayOverbloom");
                    
                    PunisherCinematicSystem.PurpleFogOpacity = 0f;
                    PunisherCinematicSystem.SpaceDistortionOpacity = 0f;
                    PunisherCinematicSystem.KaleidoscopeOpacity = 0f;
                    PunisherCinematicSystem.Phase2SlashOpacity = 0f;
                }
                if (Main.curMusic >= 0 && Main.curMusic < Main.musicFade.Length)
                {
                    Main.musicFade[Main.curMusic] *= 0.1f;
                }
                NPC.dontTakeDamage = true;
                NPC.velocity = Vector2.Lerp(NPC.velocity, (Main.player[NPC.target].Center - new Vector2(0, 300f) - NPC.Center) * 0.05f, 0.1f);
                AI_Timer++;
                if (AI_Timer >= 300)
                {
                    isFinalPhase = true;
                    NPC.dontTakeDamage = false;
                    NPC.life = NPC.lifeMax;
                    NPC.localAI[1] = 2f;
                    AI_State = State_Idle;
                    AI_Timer = 0;
                    NPC.netUpdate = true;
                }
            }
            else if (AI_State == State_FinalPhaseAttack)
            {
                AI_Timer++;
                if (AI_Timer % 60 == 0)
                {
                    SoundEngine.PlaySound(SoundID.Item71, NPC.Center);
                    Vector2 dashDir = Vector2.Normalize(Main.player[NPC.target].Center - NPC.Center);
                    NPC.velocity = dashDir * 25f;

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<PunisherPhantomDash>(), NPC.damage, 0, Main.myPlayer, 0, Main.player[NPC.target].Center.X, Main.player[NPC.target].Center.Y);
                }
                if (AI_Timer % 180 == 0)
                {
                    PunisherCinematicSystem.ScreenShakeAmount = 20f;

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<DimensionSunderProj>(), NPC.damage, 0f, Main.myPlayer, Main.rand.NextBool() ? 1f : 0f);
                }
            }
            else if (AI_State == State_Parry)
            {
                NPC.velocity *= 0.8f;
                AI_Timer++;
                if (AI_Timer >= 120)
                {
                    AI_State = State_Idle;
                    AI_Timer = 0;
                }
            }
            else if (AI_State == State_CounterAttack)
            {
                AI_Timer += IsPhase2 ? 0.5f : 1f;
                if (AI_Timer < 100)
                {
                    player.AddBuff(ModContent.BuffType<ParalyzedByPunisher>(), 2);
                    if (IsPhase2 && player.velocity.Y > -2f)
                    {
                        player.velocity.Y = -0.4f;
                    }
                }
                if (AI_Timer < 60)
                {
                    float facing = player.direction > 0 ? 1f : -1f;
                    if (IsPhase2)
                    {
                        Vector2 targetPos = player.Center + new Vector2(150f * facing, 0f);
                        if (Collision.SolidCollision(targetPos - NPC.Size / 2f, NPC.width, NPC.height))
                        {
                            facing *= -1f;
                            targetPos = player.Center + new Vector2(150f * facing, 0f);
                        }
                        NPC.Center = Vector2.Lerp(NPC.Center, targetPos, 0.12f);
                    }
                    else
                    {
                        Vector2 targetPos = player.Center + new Vector2(100f * facing, -50f);
                        NPC.Center = Vector2.Lerp(NPC.Center, targetPos, 0.05f);
                    }

                    NPC.velocity = Vector2.Zero;
                    if (!IsPhase2)
                        PunisherCinematicSystem.PurpleFogOpacity = AI_Timer / 60f;

                    if (AI_Timer == 1)
                        SoundEngine.PlaySound(SoundID.Item122, NPC.Center);
                }
                if (AI_Timer >= 60 && AI_Timer < 80)
                {
                    if (IsPhase2)
                    {
                        PunisherCinematicSystem.Phase2SlashOpacity = (AI_Timer - 60f) / 100f;
                        float facing = player.direction > 0 ? 1f : -1f;
                        Vector2 targetPos = player.Center + new Vector2(150f * facing, 0f);

                        if (Collision.SolidCollision(targetPos - NPC.Size / 2f, NPC.width, NPC.height))
                        {
                            facing *= -1f;
                            targetPos = player.Center + new Vector2(150f * facing, 0f);
                        }

                        NPC.Center = Vector2.Lerp(NPC.Center, targetPos, 0.3f);
                        PunisherCinematicSystem.TearNormal = Vector2.UnitY;
                    }
                    else
                    {
                        PunisherCinematicSystem.PurpleFogOpacity = 1f;
                        PunisherCinematicSystem.Phase2SlashOpacity = (AI_Timer - 60f) / 100f;
                        Vector2 dashDir = Vector2.Normalize(player.Center - NPC.Center);

                        if (dashDir.Length() == 0) dashDir = new Vector2(player.direction * -1, 0);
                        Vector2 normal = new Vector2(-dashDir.Y * 1.77f, dashDir.X);
                        normal.Normalize();

                        PunisherCinematicSystem.TearNormal = normal;
                    }
                    NPC.Center += new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f));
                }
                if (AI_Timer == 80)
                {
                    SoundEngine.PlaySound(SoundID.Item71 with { Pitch = -0.8f }, NPC.Center);
                    Vector2 dashDir;
                    if (IsPhase2)
                    {
                        float facing = player.direction > 0 ? 1f : -1f;
                        dashDir = new Vector2(-facing, 0f);
                        PunisherCinematicSystem.TearNormal = Vector2.UnitY;
                    }
                    else
                    {
                        dashDir = Vector2.Normalize(player.Center - NPC.Center);
                        if (dashDir.Length() == 0) dashDir = new Vector2(player.direction * -1, 0);
                        Vector2 normal = new Vector2(-dashDir.Y * 1.77f, dashDir.X);
                        normal.Normalize();
                        PunisherCinematicSystem.TearNormal = normal;
                    }
                    NPC.velocity = dashDir * 40f;
                }
                if (AI_Timer > 80 && AI_Timer <= 90)
                {
                    if (NPC.velocity.Length() < 5f)
                    {
                        AI_Timer = 90;
                    }
                    if (IsPhase2)
                    {
                        PunisherCinematicSystem.Phase2SlashOpacity = 1f;
                    }
                    else
                    {
                        PunisherCinematicSystem.PurpleFogOpacity = 1f;
                        PunisherCinematicSystem.Phase2SlashOpacity = 1f;
                    }
                }
                if (AI_Timer == 90)
                {
                    NPC.velocity *= 0.1f;
                    NPC.spriteDirection *= -1;
                    if (!IsPhase2)
                    {
                        player.AddBuff(ModContent.BuffType<PunisherConcussion>(), 300);
                        PunisherCinematicSystem.PainOpacity = 1.0f;
                    }

                    PunisherCinematicSystem.UniverseTearOpacity = 0f;
                    PunisherCinematicSystem.PurpleFogOpacity = 0f;
                    PunisherCinematicSystem.Phase2SlashOpacity = 0f;

                    SoundEngine.PlaySound(SoundID.Item62, NPC.Center);

                    PunisherCinematicSystem.ScreenShakeAmount = 25f;
                    if (IsPhase2)
                    {
                        AI_State = State_AntiAir;
                        AI_Timer = 19;
                    }
                    else
                    {
                        AI_State = State_Idle;
                        AI_Timer = 0;
                    }
                }
            }
            else if (AI_State == State_AntiAir)
            {
                AI_Timer++;
                NPC.noTileCollide = true;
                if (IsPhase2 && AI_Timer < 60)
                {
                    player.AddBuff(ModContent.BuffType<ParalyzedByPunisher>(), 2);
                    if (player.velocity.Y > -2f)
                    {
                        player.velocity.Y = -0.4f;
                    }
                }
                if (AI_Timer < 20)
                {
                    NPC.alpha += 15;
                    NPC.velocity *= 0.5f;
                }
                else if (AI_Timer == 20)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, -2f), ModContent.ProjectileType<PunisherSmokeProj>(), 0, 0, Main.myPlayer);

                    NPC.Center = player.Center + new Vector2(0, -250f);
                    NPC.velocity = Vector2.Zero;

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0, -1f), ModContent.ProjectileType<PunisherSmokeProj>(), 0, 0, Main.myPlayer);
                }
                else if (AI_Timer > 20 && AI_Timer < 40)
                {
                    NPC.alpha -= 15;
                    if (NPC.alpha < 0) NPC.alpha = 0;
                }
                else if (AI_Timer == 40)
                {
                    SoundEngine.PlaySound(SoundID.Item71 with { Pitch = -0.5f }, NPC.Center);
                    NPC.velocity.Y = 30f;
                }
                else if (AI_Timer > 40 && AI_Timer < 60)
                {
                    if (NPC.ai[2] >= 2)
                    {
                        NPC.position.X = player.Center.X - NPC.width / 2;
                        new Spark(NPC.Center, Main.rand.NextVector2Circular(3f, 3f), Color.Red, 20, Main.rand.NextFloat(1f, 2f), 1f).Spawn();
                    }
                    if (NPC.Hitbox.Intersects(player.Hitbox))
                    {
                        player.AddBuff(ModContent.BuffType<ClippedWings>(), 360000);
                        player.AddBuff(ModContent.BuffType<PunisherConcussion>(), 300);
                        player.AddBuff(ModContent.BuffType<BrainDamage>(), 420);

                        PunisherCinematicSystem.WhiteFlashOpacity = 1.0f;
                        PunisherCinematicSystem.PainOpacity = 1.0f;
                        PunisherCinematicSystem.ScreenShakeAmount = 35f;
                        player.velocity.Y = 30f;

                        SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/catch!") { MaxInstances = 1, SoundLimitBehavior = SoundLimitBehavior.IgnoreNew }, player.Center);
                        AI_Timer = 60;
                    }
                    else if (NPC.velocity.Y == 0 || AI_Timer == 59)
                    {
                        if (NPC.ai[2] < 2)
                        {
                            NPC.ai[2]++;
                            AI_Timer = 0;
                            NPC.TargetClosest(true);
                        }
                        else
                        {
                            NPC.noTileCollide = false;
                            AI_State = State_Idle;
                            AI_Timer = 0;
                            NPC.ai[2] = 0;
                        }
                    }
                }
                else if (AI_Timer >= 60)
                {
                    NPC.noTileCollide = false;
                    if (AI_Timer == 60)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            Vector2 vel = Main.rand.NextVector2Circular(15f, 15f);
                            new Stretching_particle_out(NPC.Center, vel, Color.Purple, 30, Main.rand.NextFloat(20f, 40f), Main.rand.NextFloat(2f, 5f), 0f).Spawn();
                            new Spark(NPC.Center, Main.rand.NextVector2Circular(12f, 12f), Color.Plum, 40, Main.rand.NextFloat(1f, 2f), 1f).Spawn();
                        }
                        new Circular_glow(NPC.Center, Vector2.Zero, Color.DarkOrchid, 60, 4f, 1f).Spawn();
                    }
                    if (AI_Timer < 300)
                    {
                        if (Main.curMusic >= 0 && Main.curMusic < Main.musicFade.Length)
                        {
                            Main.musicFade[Main.curMusic] = 0f;
                        }
                    }
                    if (NPC.velocity.Y != 0 && !NPC.collideY)
                    {
                        NPC.velocity.Y = 1.5f;
                        NPC.velocity.X = 0f;
                    }
                    if (player.velocity.Y == 0 && NPC.ai[3] == 0)
                    {
                        NPC.ai[3] = 1;
                        SoundEngine.PlaySound(SoundID.Item14, player.Center);
                        for (int i = 0; i < 5; i++)
                        {
                            new Spark(player.Center, new Vector2(Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, -1f)), Color.DarkRed, 20, Main.rand.NextFloat(0.5f, 1f), 1f).Spawn();
                            new Spark(player.Center, new Vector2(Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, -1f)), Color.SaddleBrown, 20, Main.rand.NextFloat(0.5f, 0.8f), 1f).Spawn();
                        }
                    }
                    if (AI_Timer == 120)
                    {
                        if (!IsPhase2)
                        {
                            string message = "No flying today. Hahaha!";
                            if (NPC.ai[2] >= 2)
                            {
                                message = "Thought you could run? Pathetic loser.";
                                if (!IsEnraged)
                                {
                                    NPC.localAI[0] = 1f;
                                    NPC.damage = (int)(NPC.defDamage * 1.5f);
                                    NPC.defense = NPC.defDefense + 20;
                                    SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
                                }
                            }
                            Color messageColor = new Color(150, 0, 255);
                            if (Main.netMode == NetmodeID.SinglePlayer)
                            {
                                Main.NewText(message, messageColor);
                            }
                            else if (Main.netMode == NetmodeID.Server)
                            {
                                ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(message), messageColor);
                            }
                        }
                    }
                    if (AI_Timer > 400)
                    {
                        NPC.noTileCollide = false;
                        AI_State = State_Idle;
                        AI_Timer = 0;
                        NPC.ai[2] = 0;
                        NPC.ai[3] = 0;
                    }
                    else if (NPC.velocity.Y == 0 && AI_Timer > 65)
                    {
                        NPC.velocity.X *= 0.8f;
                    }
                }
            }
            else if (AI_State == State_PhaseTransition)
            {
                NPC.velocity.X *= 0.9f;
                NPC.velocity.Y *= 0.9f;
                NPC.noGravity = true;
                NPC.noTileCollide = true;
                NPC.dontTakeDamage = true;
                if (AI_Timer == 0)
                {
                    SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
                    string message = "Enough games. Welcome to my world.";
                    Color messageColor = new Color(150, 0, 255);
                    if (Main.netMode == NetmodeID.SinglePlayer) Main.NewText(message, messageColor);
                    else if (Main.netMode == NetmodeID.Server) ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(message), messageColor);
                }
                AI_Timer++;
                PunisherCinematicSystem.Phase2FogOpacity = MathHelper.Clamp(AI_Timer / 120f, 0f, 1f);
                for (int i = 0; i < 4; i++)
                {
                    Vector2 dustSpawn = NPC.Center + Main.rand.NextVector2CircularEdge(300f, 300f);
                    Vector2 dustVel = (NPC.Center - dustSpawn) * 0.1f;
                    new Spark(dustSpawn, dustVel, Color.MediumPurple, 20, Main.rand.NextFloat(0.5f, 1f), 1f).Spawn();
                }
                if (IsPhase3)
                {
                    Lighting.AddLight(NPC.Center, 2f, 1.5f, 2f);
                }
                float distToPlayer = Vector2.Distance(player.Center, NPC.Center);
                if (distToPlayer < 400f)
                {
                    Vector2 push = Vector2.Normalize(player.Center - NPC.Center) * (10f * (1f - distToPlayer/400f));
                    player.velocity += push * 0.5f;
                }
                PunisherCinematicSystem.PainOpacity = MathHelper.Clamp((float)Math.Sin(AI_Timer * 0.2f) * 0.15f, 0f, 1f);
                if (AI_Timer > 180)
                {
                    SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
                    for (int i = 0; i < 60; i++)
                    {
                        Vector2 vel = Main.rand.NextVector2Circular(20f, 20f);
                        new Stretching_particle_out(NPC.Center, vel, Color.DarkOrchid, 40, Main.rand.NextFloat(30f, 60f), Main.rand.NextFloat(4f, 8f), 0f).Spawn();
                        new Spark(NPC.Center, Main.rand.NextVector2Circular(15f, 15f), Color.Plum, 50, Main.rand.NextFloat(1f, 3f), 1f).Spawn();
                    }
                    new Circular_glow(NPC.Center, Vector2.Zero, Color.White, 60, 5f, 1f).Spawn();
                    int healAmount = 2000;
                    NPC.life += healAmount;

                    if (NPC.life > NPC.lifeMax) NPC.life = NPC.lifeMax;
                    NPC.HealEffect(healAmount, true);

                    NPC.defense += 20;
                    NPC.localAI[1] = 1f;
                    NPC.noGravity = false;
                    NPC.noTileCollide = false;
                    NPC.dontTakeDamage = false;
                    AI_State = State_Idle;
                    AI_Timer = 0;
                }
            }
            else if (AI_State == State_Phase3Transition)
            {
                NPC.velocity *= 0.9f;
                NPC.noGravity = true;
                NPC.noTileCollide = true;
                NPC.dontTakeDamage = true;
                if (AI_Timer == 0)
                {
                    preFightPlayerPos = Main.player[NPC.target].Bottom;
                    SoundEngine.PlaySound(SoundID.Roar, NPC.Center);

                    string message = "How... how can a mere insect push me this far?";
                    Color messageColor = new Color(200, 50, 255);

                    if (Main.netMode == NetmodeID.SinglePlayer) Main.NewText(message, messageColor);
                    else if (Main.netMode == NetmodeID.Server) ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(message), messageColor);
                }
                if (AI_Timer == 120)
                {
                    string message = "No more games. Reality itself will shatter before you!";
                    Color messageColor = new Color(255, 0, 0);

                    if (Main.netMode == NetmodeID.SinglePlayer) Main.NewText(message, messageColor);
                    else if (Main.netMode == NetmodeID.Server) ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(message), messageColor);

                    SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
                }
                if (AI_Timer >= 120 && AI_Timer < 240)
                {
                    float progress = (AI_Timer - 120f) / 120f;
                    PunisherCinematicSystem.VignetteIntensity = progress;
                    PunisherCinematicSystem.KaleidoscopeOpacity = progress;
                    PunisherCinematicSystem.PurpleFogOpacity = progress * 0.5f;
                    for (int i = 0; i < 8; i++)
                    {
                        Vector2 dustSpawn = NPC.Center + Main.rand.NextVector2CircularEdge(800f, 800f) * (1f - progress * 0.5f);
                        Vector2 dustVel = (NPC.Center - dustSpawn) * 0.08f;
                        new Spark(dustSpawn, dustVel, Color.MediumPurple, 30, Main.rand.NextFloat(0.5f, 1.5f), 1f).Spawn();
                    }
                    if (AI_Timer % 20 == 0)
                    {
                        SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
                    }
                }
                if (AI_Timer == 240)
                {
                    SoundEngine.PlaySound(SoundID.DD2_EtherianPortalOpen, NPC.Center);
                    PunisherCinematicSystem.KaleidoscopeOpacity = 0f;
                }
                if (AI_Timer >= 240 && AI_Timer < 360)
                {
                    float tearProgress = (AI_Timer - 240f) / 120f;
                    PunisherCinematicSystem.UniverseTearOpacity = tearProgress;
                }
                if (AI_Timer == 359)
                {
                    PunisherCinematicSystem.CaptureNextFrame = true;
                }
                if (AI_Timer == 360)
                {
                    PunisherCinematicSystem.WorldBlackoutOpacity = 1f;
                    PunisherCinematicSystem.PurpleFogOpacity = 0f;
                    PunisherCinematicSystem.SpaceDistortionOpacity = 0f;

                    string message = "I am a god! I will crush you from the skies!";
                    Color messageColor = new Color(255, 0, 0);

                    if (Main.netMode == NetmodeID.SinglePlayer) Main.NewText(message, messageColor);
                    else if (Main.netMode == NetmodeID.Server) ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(message), messageColor);

                    SoundEngine.PlaySound(SoundID.Item105, NPC.Center);
                    for (int i = 0; i < 100; i++)
                    {
                        Vector2 vel = Main.rand.NextVector2Circular(30f, 15f);
                        vel.Y -= 5f;

                        if (Math.Abs(vel.X) < 10f) vel.X *= 2f;
                        new Stretching_particle_out(NPC.Center, vel, Color.DarkOrchid, 45, Main.rand.NextFloat(50f, 100f), Main.rand.NextFloat(5f, 10f), 0.05f).Spawn();
                    }
                    for (int i = 0; i < 50; i++)
                    {
                        Vector2 vel = Main.rand.NextVector2Circular(25f, 25f);
                        new Spark(NPC.Center, vel, Color.MediumPurple, 50, Main.rand.NextFloat(1f, 3f), 1f).Spawn();
                    }
                    new Circular_glow(NPC.Center, Vector2.Zero, Color.White, 60, 6f, 1f).Spawn();
                    Vector2 spacePos = new Vector2(Main.maxTilesX * 8f, 150f * 16f);
                    
                    for (int p = 0; p < Main.maxPlayers; p++)
                    {
                        if (Main.player[p].active && !Main.player[p].dead)
                        {
                            Vector2 teleportOffset = new Vector2(Main.rand.NextFloat(-100f, 100f), 0f);
                            Main.player[p].Teleport(spacePos + teleportOffset, 1);
                            Main.player[p].velocity = Vector2.Zero;
                            Main.player[p].fallStart = (int)((spacePos.Y + teleportOffset.Y) / 16f);
                        }
                    }

                    NPC.Center = spacePos + new Vector2(0, -300f);

                    PunisherCinematicSystem.HexBarrierCenter = new Vector2(Main.maxTilesX * 8f, 130f * 16f);
                    PunisherCinematicSystem.HexBarrierSize = new Vector2(4000f, 1500f);
                    PunisherCinematicSystem.HexBarrierOpacity = 1f;

                    NPC.HealEffect(5000, true);
                    NPC.life += 5000;

                    if (NPC.life > NPC.lifeMax) NPC.life = NPC.lifeMax;

                    NPC.defense += 30;
                    NPC.localAI[1] = 2f;
                    NPC.dontTakeDamage = true;

                    AI_State = State_Phase3Hover;
                    AI_Timer = 0;
                }
                AI_Timer++;
            }
            else if (AI_State == State_Phase3Hover)
            {
                player.velocity = Vector2.Zero;
                NPC.velocity = Vector2.Zero;
                if (AI_Timer >= 60)
                {
                    NPC.dontTakeDamage = false;
                    AI_State = State_Idle;
                    AI_Timer = 0;
                }
                AI_Timer++;
            }
            else if (AI_State == State_RealitySlash)
            {
                if (AI_Timer == 0)
                {
                    SoundEngine.PlaySound(SoundID.Item71, NPC.Center);
                    NPC.velocity = Vector2.Zero;
                    NPC.ai[2] = player.Center.X;
                    NPC.ai[3] = player.Center.Y;
                }
                AI_Timer++;
                if (AI_Timer < 60)
                {
                    Vector2 targetPos = new Vector2(NPC.ai[2], NPC.ai[3]);
                    Vector2 dir = Vector2.Normalize(targetPos - NPC.Center);
                    if (dir.Length() == 0) dir = Vector2.UnitX;
                    for (int i = 0; i < 5; i++)
                    {
                        Vector2 dustPos = NPC.Center + dir * (AI_Timer * 15 + i * 30);
                        new Spark(dustPos, Vector2.Zero, Color.White, 20, 1f, 1f).Spawn();
                    }
                    if (AI_Timer < 40)
                    {
                        NPC.ai[2] = player.Center.X;
                        NPC.ai[3] = player.Center.Y;
                    }
                    NPC.direction = Math.Sign(dir.X);
                    if (NPC.direction != 0) NPC.spriteDirection = NPC.direction;
                }
                if (AI_Timer == 60)
                {
                    Vector2 targetPos = new Vector2(NPC.ai[2], NPC.ai[3]);
                    Vector2 dashDir = Vector2.Normalize(targetPos - NPC.Center);

                    if (dashDir.Length() == 0) dashDir = new Vector2(NPC.direction, 0);
                    NPC.velocity = dashDir * 45f;

                    SoundEngine.PlaySound(SoundID.Item62, NPC.Center);

                    PunisherCinematicSystem.SpaceDistortionOpacity = 1f;
                    PunisherCinematicSystem.DistortionNormal = dashDir;
                    PunisherCinematicSystem.ScreenShakeAmount = 25f;
                    PunisherCinematicSystem.ScreenShakeAmount = 25f;
                }
                if (AI_Timer > 60 && AI_Timer < 80)
                {
                    if (NPC.velocity.Length() < 5f)
                    {
                        AI_Timer = 80;
                    }
                    Vector2 screenPos = NPC.Center - Main.screenPosition;
                    Vector2 screenUV = new Vector2(screenPos.X / Main.screenWidth, screenPos.Y / Main.screenHeight);

                    PunisherCinematicSystem.DistortionPoint = screenUV;
                    PunisherCinematicSystem.SpaceDistortionOpacity = 1f;

                    for (int i = 0; i < 3; i++)
                    {
                        new Spark(NPC.Center + Main.rand.NextVector2Circular(10f, 10f), Vector2.Zero, Color.MediumPurple, 20, Main.rand.NextFloat(1f, 2f), 1f).Spawn();
                    }
                    if (NPC.Hitbox.Intersects(player.Hitbox))
                    {
                        player.AddBuff(ModContent.BuffType<PunisherConcussion>(), 180);

                        PunisherCinematicSystem.PainOpacity = 1.0f;
                        PunisherCinematicSystem.ScreenShakeAmount = 30f;
                        PunisherCinematicSystem.ScreenShakeAmount = 30f;
                    }
                }
                if (AI_Timer == 80)
                {
                    NPC.velocity *= 0.1f;
                }
                if (AI_Timer > 80 && AI_Timer < 100)
                {
                    NPC.velocity *= 0.8f;
                }
                if (AI_Timer >= 100)
                {
                    AI_State = State_Idle;
                    AI_Timer = 0;
                }
            }
            else if (AI_State == State_ExecutionersFlurry)
            {
                AI_Timer++;
                if (AI_Timer < 30)
                {
                    NPC.alpha += 10;
                    NPC.velocity *= 0.8f;
                }
                if (AI_Timer == 30)
                {
                    NPC.Center = player.Center + new Vector2(-300, -200);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<PunisherPhantomDash>(), NPC.damage, 0, Main.myPlayer, 0, player.Center.X, player.Center.Y);
                    SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
                }
                if (AI_Timer == 45)
                {
                    NPC.Center = player.Center + new Vector2(300, -200);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<PunisherPhantomDash>(), NPC.damage, 0, Main.myPlayer, 15, player.Center.X, player.Center.Y);
                    SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
                }
                if (AI_Timer == 60)
                {
                    NPC.Center = player.Center + new Vector2(0, 300);
                    NPC.alpha = 0;
                    SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
                }
                if (AI_Timer >= 60 && AI_Timer < 80)
                {
                    Vector2 dir = player.Center - NPC.Center;
                    NPC.direction = Math.Sign(dir.X);
                    if (NPC.direction != 0) NPC.spriteDirection = NPC.direction;
                    NPC.Center += new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f));
                }
                if (AI_Timer == 80)
                {
                    Vector2 dashDir = Vector2.Normalize(player.Center - NPC.Center);
                    if (dashDir.Length() == 0) dashDir = Vector2.UnitY;
                    NPC.velocity = dashDir * 35f;
                    SoundEngine.PlaySound(SoundID.Item71, NPC.Center);
                    PunisherCinematicSystem.ScreenShakeAmount = 25f;
                    new Slash_effect(NPC.Center, dashDir * 15f, Color.MediumPurple, 25, 2.5f, 0f, dashDir.ToRotation()).Spawn();
                }
                if (AI_Timer > 80 && AI_Timer <= 100)
                {
                    if (NPC.velocity.Length() < 5f) AI_Timer = 100;
                    if (NPC.Hitbox.Intersects(player.Hitbox))
                    {
                        PunisherCinematicSystem.ScreenShakeAmount = 30f;
                    }
                }
                if (AI_Timer == 100)
                {
                    NPC.velocity *= 0.1f;
                }
                if (AI_Timer >= 115)
                {
                    AI_State = State_Idle;
                    AI_Timer = 0;
                }
            }
            else if (AI_State == State_DimensionShatter)
            {
                AI_Timer++;
                if (AI_Timer < 60)
                {
                    Vector2 targetPos = player.Center + new Vector2(0, -250f);
                    NPC.Center = Vector2.Lerp(NPC.Center, targetPos, 0.1f);
                    NPC.velocity = Vector2.Zero;
                    if (AI_Timer == 1)
                    {
                        NPC.ai[2] = Main.rand.NextFloat(MathHelper.TwoPi);
                    }
                    if (AI_Timer % 8 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int index = (int)(AI_Timer / 8);
                        float baseAngle = NPC.ai[2];
                        float angle = baseAngle;
                        Vector2 offset = Vector2.Zero;
                        if (index <= 4)
                        {
                            angle = baseAngle;
                            float perpAngle = baseAngle + MathHelper.PiOver2;
                            float distance = -375f + (index - 1) * 250f;
                            offset = new Vector2((float)Math.Cos(perpAngle), (float)Math.Sin(perpAngle)) * distance;
                        }
                        else
                        {
                            angle = baseAngle + MathHelper.PiOver2;
                            float perpAngle = baseAngle;
                            float distance = -250f + (index - 5) * 250f;
                            offset = new Vector2((float)Math.Cos(perpAngle), (float)Math.Sin(perpAngle)) * distance;
                        }
                        float length = 4000f;
                        Vector2 spawnPos = player.Center + offset;
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, Vector2.Zero, ModContent.ProjectileType<PunisherDimensionShatter>(), NPC.damage, 0, Main.myPlayer, AI_Timer, angle, length);
                        SoundEngine.PlaySound(SoundID.Item1, NPC.Center);
                    }
                }
                if (AI_Timer == 60)
                {
                    NPC.velocity = new Vector2(0, -10f);
                    PunisherCinematicSystem.ScreenShakeAmount = 35f;
                }
                if (AI_Timer >= 100)
                {
                    AI_State = State_Idle;
                    AI_Timer = 0;
                }
            }
            if (IsPhase2)
            {
                PunisherCinematicSystem.Phase2FogOpacity = 1f;
            }
            if (IsEnraged && Main.rand.NextBool(3))
            {
                new Spark(NPC.Center + Main.rand.NextVector2Circular(NPC.width/2, NPC.height/2), new Vector2(0, -Main.rand.NextFloat(1f, 3f)), Color.Red, 30, Main.rand.NextFloat(0.5f, 1.5f), 1f).Spawn();
            }
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(isFinalPhase);
            writer.Write(NPC.localAI[0]);
            writer.Write(NPC.localAI[1]);
            writer.Write(NPC.localAI[2]);
            writer.Write(NPC.localAI[3]);
            writer.Write(LastAttackState);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            isFinalPhase = reader.ReadBoolean();
            NPC.localAI[0] = reader.ReadSingle();
            NPC.localAI[1] = reader.ReadSingle();
            NPC.localAI[2] = reader.ReadSingle();
            NPC.localAI[3] = reader.ReadSingle();
            LastAttackState = reader.ReadInt32();
        }

        public override bool CheckActive()
        {
            return false;
        }

        public override bool CheckDead()
        {
            if (!isFinalPhase)
            {
                NPC.life = 1;
                NPC.dontTakeDamage = true;
                AI_State = State_DialogueAndChoice;
                AI_Timer = 0;
                NPC.netUpdate = true;
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    if (Main.projectile[i].active && Main.projectile[i].hostile)
                    {
                        Main.projectile[i].Kill();
                    }
                }
                if (Terraria.Graphics.Effects.Filters.Scene["jarkos:VoidTear"] != null && Terraria.Graphics.Effects.Filters.Scene["jarkos:VoidTear"].IsActive()) Terraria.Graphics.Effects.Filters.Scene.Deactivate("jarkos:VoidTear");
                if (Terraria.Graphics.Effects.Filters.Scene["jarkos:DimensionSunder"] != null && Terraria.Graphics.Effects.Filters.Scene["jarkos:DimensionSunder"].IsActive()) Terraria.Graphics.Effects.Filters.Scene.Deactivate("jarkos:DimensionSunder");
                if (Terraria.Graphics.Effects.Filters.Scene["jarkos:DeathrayFocus"] != null && Terraria.Graphics.Effects.Filters.Scene["jarkos:DeathrayFocus"].IsActive()) Terraria.Graphics.Effects.Filters.Scene.Deactivate("jarkos:DeathrayFocus");
                if (Terraria.Graphics.Effects.Filters.Scene["jarkos:DeathrayOverbloom"] != null && Terraria.Graphics.Effects.Filters.Scene["jarkos:DeathrayOverbloom"].IsActive()) Terraria.Graphics.Effects.Filters.Scene.Deactivate("jarkos:DeathrayOverbloom");
                PunisherCinematicSystem.PurpleFogOpacity = 0f;
                PunisherCinematicSystem.SpaceDistortionOpacity = 0f;
                PunisherCinematicSystem.KaleidoscopeOpacity = 0f;
                PunisherCinematicSystem.Phase2SlashOpacity = 0f;
                return false;
            }
            return true;
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (AI_State == State_Parry)
            {
                AI_State = State_CounterAttack;
                AI_Timer = 0;
                if (IsPhase2)
                {
                    SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/Parry AD V2") { Volume = 1.5f }, NPC.Center);
                }
                else
                {
                    SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/Parry AD") { Volume = 1.5f }, NPC.Center);
                }
                for (int i = 0; i < 20; i++)
                {
                    new Stretching_particle_out(NPC.Center, Main.rand.NextVector2Circular(15f, 15f), Color.Yellow, 30, Main.rand.NextFloat(30f, 60f), Main.rand.NextFloat(4f, 8f), 0f).Spawn();
                    new Spark(NPC.Center, Main.rand.NextVector2Circular(10f, 10f), Color.White, 25, Main.rand.NextFloat(0.5f, 1.2f), 1f).Spawn();
                }
                if (IsPhase2)
                {
                    Player player = Main.player[NPC.target];
                    Vector2 teleportPos = player.Center + new Vector2(player.direction * -150f, 0);
                    if (!Collision.SolidCollision(teleportPos - NPC.Size / 2f, NPC.width, NPC.height))
                    {
                        NPC.Center = teleportPos;
                    }
                    else
                    {
                        NPC.position = player.Top - new Vector2(NPC.width / 2f, NPC.height + 50f);
                    }
                    player.velocity.Y = -80f;
                    for (int i = 0; i < 20; i++)
                    {
                        new Stretching_particle_out(NPC.Center, Main.rand.NextVector2Circular(10f, 10f), Color.Purple, 20, Main.rand.NextFloat(10f, 30f), Main.rand.NextFloat(2f, 4f), 0f).Spawn();
                    }
                }
                for (int i = 0; i < 15; i++)
                {
                    new Spark(NPC.Center, Main.rand.NextVector2Circular(8f, 8f), Color.Silver, 20, Main.rand.NextFloat(0.5f, 1.5f), 1f).Spawn();
                }
            }
            if (IsPhase3 && !Main.dedServ)
            {
            }
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Punisher_bag>()));
        }

        public override void OnKill()
        {
            NPC.SetEventFlagCleared(ref DownedBossSystem.downedPunisher, -1);
            NPC.SetEventFlagCleared(ref DownedBossSystem.killedPunisher, -1);
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                if (Main.player[i].active && !Main.player[i].dead)
                {
                    Main.player[i].ClearBuff(ModContent.BuffType<ClippedWings>());
                }
            }
            PunisherCinematicSystem.WorldBlackoutOpacity = 0f;
            if (!Main.dedServ && Terraria.Graphics.Effects.Filters.Scene["jarkos:VoidTear"].IsActive())
            {
                Terraria.Graphics.Effects.Filters.Scene.Deactivate("jarkos:VoidTear");
            }
        }
        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            if (AI_State == State_CounterAttack && AI_Timer > 80 && AI_Timer <= 90)
            {
                return false;
            }
            return base.CanHitPlayer(target, ref cooldownSlot);
        }
        public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers)
        {
            if (AI_State == State_Parry)
            {
                modifiers.SetMaxDamage(1);
            }
        }
        public override void FindFrame(int frameHeight)
        {
            if (AI_State == State_Idle)
            {
                if (IsPhase3)
                {
                    NPC.frame.Y = 19 * frameHeight;
                }
                else
                {
                    int startFrame = 9;
                    int endFrame = 12;
                    float speedX = Math.Abs(NPC.velocity.X);
                    NPC.frameCounter += 1.0 + (speedX * 0.2);
                    if (NPC.velocity.Y != 0)
                    {
                        NPC.frame.Y = 11 * frameHeight;
                    }
                    else
                    {
                        if (NPC.frameCounter >= 6)
                        {
                            NPC.frameCounter = 0;
                            NPC.frame.Y += frameHeight;
                            if (NPC.frame.Y < startFrame * frameHeight || NPC.frame.Y > endFrame * frameHeight)
                            {
                                NPC.frame.Y = startFrame * frameHeight;
                            }
                        }
                    }
                }
            }
            else if (AI_State == State_Phase3Transition)
            {
                NPC.frame.Y = 19 * frameHeight;
            }
            else if (AI_State == State_PhantomWalk)
            {
                int startFrame = 13;
                int endFrame = 18;
                int animLength = 60;
                int frame = startFrame + (int)(AI_Timer / animLength * (endFrame - startFrame + 1));
                if (frame > endFrame) frame = endFrame;
                NPC.frame.Y = frame * frameHeight;
            }
            else if (AI_State == State_LazySlash)
            {
                int startFrame = 0;
                int endFrame = 8;
                int animLength = 60;
                int frame = startFrame + (int)(AI_Timer / animLength * (endFrame - startFrame + 1));
                if (frame > endFrame) frame = endFrame;
                NPC.frame.Y = frame * frameHeight;
            }
            else if (AI_State == State_Parry)
            {
                NPC.frame.Y = 17 * frameHeight;
            }
            else if (AI_State == State_CounterAttack)
            {
                int startFrame = 0;
                int endFrame = 8;
                int animLength = 90;
                int frame = startFrame + (int)(AI_Timer / animLength * (endFrame - startFrame + 1));
                if (frame > endFrame) frame = endFrame;
                NPC.frame.Y = frame * frameHeight;
            }
            else if (AI_State == State_AntiAir)
            {
                if (AI_Timer < 20)
                {
                    int frame = 13 + (int)(AI_Timer / 20f * 5);
                    if (frame > 18) frame = 18;
                    NPC.frame.Y = frame * frameHeight;
                }
                else if (AI_Timer > 65 && NPC.velocity.Y == 0)
                {
                    NPC.frame.Y = 10 * frameHeight;
                }
                else
                {
                    NPC.frame.Y = 8 * frameHeight;
                }
            }
            else if (AI_State == State_SpareDialogue)
            {
                if (NPC.ai[2] == 1)
                {
                    NPC.frame.Y = 0;
                }
                else
                {
                    NPC.frame.Y = 19 * frameHeight;
                }
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (PunisherCinematicSystem.WorldBlackoutOpacity > 0f)
            {
                Texture2D pixel = TextureAssets.MagicPixel.Value;
                Rectangle screenRect = new Rectangle(-2000, -2000, Main.screenWidth + 4000, Main.screenHeight + 4000);

                spriteBatch.Draw(pixel, screenRect, Color.Black * PunisherCinematicSystem.WorldBlackoutOpacity);

                if (PunisherCinematicSystem.WorldBlackoutOpacity > 0.5f && PunisherCinematicSystem.CapturedScreen != null)
                {
                    foreach (var shard in PunisherCinematicSystem.WorldShards)
                    {
                        Vector2 origin = new Vector2(shard.SourceRect.Width / 2f, shard.SourceRect.Height / 2f);
                        float darkness = 0f;
                        if (PunisherCinematicSystem.BlackoutTimer > 120)
                        {
                            darkness = MathHelper.Min(0.75f, (PunisherCinematicSystem.BlackoutTimer - 120f) / 120f * 0.75f);
                        }
                        Color shardColor = Color.Lerp(Color.White, Color.Black, darkness) * PunisherCinematicSystem.WorldBlackoutOpacity;
                        spriteBatch.Draw(PunisherCinematicSystem.CapturedScreen, shard.DrawPosition, shard.SourceRect, shardColor, shard.Rotation, origin, 1f, SpriteEffects.None, 0f);
                    }
                }
            }
            if (IsPhase3 || isFinalPhase)
            {
                spriteBatch.End();
                float bossOpacity = (255 - NPC.alpha) / 255f;
                float localShaderOpacity = 1f;

                if (NPC.ai[0] == State_SpareDialogue && NPC.ai[1] >= 300)
                {
                    localShaderOpacity = MathHelper.Clamp(1f - (NPC.ai[1] - 300) / 120f, 0f, 1f);
                }
                
                Effect auraEffect = ModContent.Request<Effect>("jarkos/npc/Punisher/Effects/AuraRays", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                auraEffect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * (isFinalPhase ? 3f : 1f));

                float auraOpacity = isFinalPhase ? 0.8f : 0.30f;
                auraEffect.Parameters["uOpacity"]?.SetValue(auraOpacity * bossOpacity * localShaderOpacity);

                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, auraEffect, Main.GameViewMatrix.ZoomMatrix);
                Texture2D pixel = TextureAssets.MagicPixel.Value;
                int auraSize = 500;
                Vector2 drawPosAura = NPC.Center - screenPos + new Vector2(0f, NPC.gfxOffY);
                Rectangle drawRect = new Rectangle((int)drawPosAura.X - auraSize / 2, (int)drawPosAura.Y - auraSize / 2, auraSize, auraSize);

                spriteBatch.Draw(pixel, drawRect, Color.White);
                spriteBatch.End();
                
                Effect glitchEffect = ModContent.Request<Effect>("jarkos/npc/Punisher/Effects/RealityGlitch", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

                glitchEffect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * (isFinalPhase ? 2f : 1f));
                glitchEffect.Parameters["uOpacity"]?.SetValue(1f * localShaderOpacity);

                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, glitchEffect, Main.GameViewMatrix.ZoomMatrix);
                try
                {
                    if (!(NPC.ai[0] == State_SpareDialogue && NPC.ai[2] == 1))
                    {
                        Texture2D wingsTex = ModContent.Request<Texture2D>("jarkos/npc/Punisher/PunisherWings").Value;
                        
                        int frameHeight = 48;
                        int frameWidth = 66;
                        int currentFrame = (int)(Main.GlobalTimeWrappedHourly * 5f) % 4;

                        Rectangle sourceRect = new Rectangle(0, currentFrame * frameHeight, frameWidth, frameHeight);
                        Vector2 drawOrigin = new Vector2(frameWidth * 0.5f, frameHeight * 0.5f);

                        Vector2 backOffset = new Vector2(15f * NPC.spriteDirection, 10f);
                        Vector2 rotatedOffset = backOffset.RotatedBy(NPC.rotation);

                        Vector2 drawPos = NPC.Center - screenPos + rotatedOffset + new Vector2(0f, NPC.gfxOffY);
                        float wingRot = (float)Math.Cos(Main.GlobalTimeWrappedHourly * 4f) * 0.05f;

                        SpriteEffects effects = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                        spriteBatch.Draw(wingsTex, drawPos, sourceRect, Color.White * bossOpacity, NPC.rotation + wingRot, drawOrigin, NPC.scale, effects, 0f);
                    }
                }
                catch { }
            }
            if (AI_State == State_CounterAttack)
            {
                Texture2D texture = TextureAssets.Npc[NPC.type].Value;
                Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height / Main.npcFrameCount[NPC.type] * 0.5f);

                for (int k = 0; k < NPC.oldPos.Length; k++)
                {
                    Vector2 drawPos = NPC.oldPos[k] - screenPos + drawOrigin + new Vector2(0f, NPC.gfxOffY);
                    Color color = new Color(255, 100, 255, 100) * ((NPC.oldPos.Length - k) / (float)NPC.oldPos.Length);
                    SpriteEffects effects = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                    spriteBatch.Draw(texture, drawPos, NPC.frame, color, NPC.rotation, drawOrigin, NPC.scale, effects, 0f);
                }
            }
            return true;
        }
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (IsPhase3)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.ZoomMatrix);
            }
            if (AI_State == State_Deathray)
            {
                spriteBatch.End();
                Texture2D dummyTex = ModContent.Request<Texture2D>("jarkos/npc/Punisher/sword_fly", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                Vector2 drawPos = NPC.Center - screenPos + new Vector2(0f, NPC.gfxOffY) - DeathrayAngle.ToRotationVector2() * 60f;

                Vector2 origin = new Vector2(0, dummyTex.Height / 2f);

                if (AI_Timer <= 120)
                {
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.ZoomMatrix);
                    float alpha = (float)Math.Sin(AI_Timer / 120f * MathHelper.Pi);

                    Texture2D magicPixel = TextureAssets.MagicPixel.Value;
                    Rectangle pixelRect = new Rectangle(0, 0, 1, 1);

                    spriteBatch.Draw(magicPixel, drawPos, pixelRect, Color.Red * (alpha * 0.3f), DeathrayAngle, new Vector2(0, 0.5f), new Vector2(3000f, 2f), SpriteEffects.None, 0f);
                    spriteBatch.End();
                }
                else if (AI_Timer > 120 && AI_Timer < 339)
                {
                    Effect beamEffect = ModContent.Request<Effect>("jarkos/npc/Punisher/Effects/DeathrayBeam", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                    beamEffect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);

                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, beamEffect, Main.GameViewMatrix.ZoomMatrix);
                    float progress = 1f;

                    if (AI_Timer > 324) progress = 1f - ((AI_Timer - 324f) / 15f);
                    float beamLength = 3000f;
                    float beamWidth = 600f * progress;

                    Vector2 shakeOffset = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f));
                    Vector2 scale = new Vector2(beamLength / dummyTex.Width, beamWidth / dummyTex.Height);

                    spriteBatch.Draw(dummyTex, drawPos + shakeOffset, null, Color.White, DeathrayAngle, origin, scale, SpriteEffects.None, 0f);
                    spriteBatch.End();
                }
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.ZoomMatrix);
            }

            if (AI_State == State_Parry)
            {
                Texture2D swordTex = ModContent.Request<Texture2D>("jarkos/npc/Punisher/sword_fly").Value;
                int frameHeight = swordTex.Height / 7;

                Rectangle sourceRect = new Rectangle(0, 0, swordTex.Width, frameHeight);
                Vector2 drawPos = NPC.Center - screenPos + new Vector2(NPC.direction * 15f, -5f);

                SpriteEffects effects = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                Vector2 origin = new Vector2(swordTex.Width / 2f, frameHeight / 2f);

                spriteBatch.Draw(swordTex, drawPos, sourceRect, drawColor, NPC.rotation, origin, 1f, effects, 0f);
            }
            
            if (textAlpha > 0f && !string.IsNullOrEmpty(currentFloatingText))
            {
                DynamicSpriteFont font = FontAssets.DeathText.Value;

                int currentTextStart = 0;
                int[] textTimes = { 0, 300, 900, 1080, 1500 };
                if (AI_State == State_SpareDialogue)
                {
                    textTimes = new int[] { 0, 300, 660, 960, 1440 };
                }
                for (int i = 4; i >= 0; i--) { if (AI_Timer >= textTimes[i]) { currentTextStart = textTimes[i]; break; } }
                Vector2 totalSize = font.MeasureString(currentFloatingText);
                Vector2 textPos = NPC.Center - screenPos - new Vector2(totalSize.X / 2f, NPC.height / 2f + 50f);
                for (int i = 0; i < currentFloatingText.Length; i++)
                {
                    float charStartTime = currentTextStart + i * 2;
                    if (AI_Timer >= charStartTime)
                    {
                        string charStr = currentFloatingText[i].ToString();

                        Vector2 charOffset = new Vector2(i == 0 ? 0 : font.MeasureString(currentFloatingText.Substring(0, i)).X, 0);

                        float flyProgress = MathHelper.Clamp((AI_Timer - charStartTime) / 20f, 0f, 1f);
                        float easeOut = 1f - (float)Math.Pow(1f - flyProgress, 3);
                        float startYOffset = (i % 2 == 0) ? -100f : 100f;
                        float currentYOffset = startYOffset * (1f - easeOut);
                        Vector2 charPos = textPos + charOffset + new Vector2(0, currentYOffset);
                        float charAlpha = textAlpha * flyProgress;

                        for (int x = -1; x <= 1; x++)
                            for (int y = -1; y <= 1; y++)
                                if (x != 0 || y != 0)
                                    ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, charStr, charPos + new Vector2(x, y) * 2f, Color.Black * charAlpha, 0f, Vector2.Zero, Vector2.One);
                                    ChatManager.DrawColorCodedStringWithShadow(spriteBatch, font, charStr, charPos, Color.White * charAlpha, 0f, Vector2.Zero, Vector2.One);
                    }
                }
            }
        }
    }
}
