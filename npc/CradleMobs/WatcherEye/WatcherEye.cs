using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
using Microsoft.Xna.Framework;
using System;
using jarkos.buffs.debuff;
using jarkos.systems;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using ReLogic.Utilities;
using Terraria.GameContent;
using jarkos.npc.CradleMobs.WatcherEye.Effects;

namespace jarkos.npc.CradleMobs.WatcherEye
{
    public class WatcherEye : ModNPC
    {
        private const int State_Scanning = 0;
        private const int State_LockedOn = 1;
        private const int State_Stunned = 2;
        private const int State_Surprised = 3;
        private const int State_Recovering = 4;
        public ref float AI_State => ref NPC.ai[0];
        public ref float AI_Timer => ref NPC.ai[1];
        public ref float AI_TargetPlayer => ref NPC.ai[2];
        public ref float AI_LostSightTimer => ref NPC.ai[3];
        public ref float AI_LastKnownRotation => ref NPC.localAI[0];
        public ref float AI_PupilScale => ref NPC.localAI[1];
        public ref float AI_SquishX => ref NPC.localAI[2];
        public ref float AI_SquishY => ref NPC.localAI[3];
        private float scanTargetRotation = 0f;
        private int scanHoldTimer = 0;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 1;
        }
        public override void SetDefaults()
        {
            NPC.width = 32;
            NPC.height = 32;
            NPC.damage = 10;
            NPC.defense = 5;
            NPC.lifeMax = 300;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = 100f;
            NPC.knockBackResist = 0.0f;
            NPC.noGravity = true;
            NPC.noTileCollide = false;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<cradle_biome.PetrifiedAbyssBiome>().Type };
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                new FlavorTextBestiaryInfoElement(LangSystem.WatcherEye_desc.Value)
            });
        }
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.InModBiome<cradle_biome.PetrifiedAbyssBiome>())
                return SpawnCondition.Cavern.Chance * 0.1f;
            return 0f;
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0)
            {
                if (Main.netMode != NetmodeID.Server)
                {
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("WatcherEyeGore1").Type, 1f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.position, NPC.velocity, Mod.Find<ModGore>("WatcherEyeGore1").Type, 1f);
                }
            }
            if (hit.Damage > 0)
            {
                AI_State = State_Stunned;
                AI_LostSightTimer = 0;
                float stunDuration = 60f + hit.Damage + (hit.Knockback * 5f);
                if (stunDuration > 240f) stunDuration = 240f;
                AI_Timer = stunDuration;
                float knockbackForce = 12f + hit.Knockback;
                if (hit.HitDirection != 0)
                {
                    NPC.velocity.X = hit.HitDirection * knockbackForce;
                    NPC.velocity.Y = -4f - (hit.Knockback * 0.2f);
                }
                else
                {
                    NPC.velocity.X = Main.rand.NextBool() ? knockbackForce : -knockbackForce;
                    NPC.velocity.Y = -4f;
                }
                AI_SquishX = 1.8f;
                AI_SquishY = 0.4f;
                AI_PupilScale = 0.3f;
            }
        }
        public override void AI()
        {
            if (AI_SquishX == 0f) AI_SquishX = 1f;
            if (AI_SquishY == 0f) AI_SquishY = 1f;
            if (AI_PupilScale == 0f) AI_PupilScale = 1f;
            float targetPupilScale = 1f;
            float targetSquishX = 1f;
            float targetSquishY = 1f;

            if (AI_State == State_Stunned)
            {
                targetPupilScale = 0.6f;
            }
            else if (AI_State == State_Recovering)
            {
                targetPupilScale = 0.8f;
            }
            else if (AI_State == State_Surprised)
            {
                targetPupilScale = 1.4f;
                targetSquishX = 0.85f;
                targetSquishY = 1.2f;
            }
            else if (AI_State == State_LockedOn)
            {
                targetPupilScale = 0.8f;
                targetSquishX = 1.1f;
                targetSquishY = 0.85f;
            }
            else
            {
                targetPupilScale = 1.0f;
                if (scanHoldTimer > 40)
                {
                    targetSquishX = 1.05f;
                    targetSquishY = 0.95f;
                }
            }
            
            AI_SquishX = MathHelper.Lerp(AI_SquishX, targetSquishX, 0.15f);
            AI_SquishY = MathHelper.Lerp(AI_SquishY, targetSquishY, 0.15f);
            AI_PupilScale = MathHelper.Lerp(AI_PupilScale, targetPupilScale, 0.15f);
            NPC.velocity *= 0.90f;

            if (AI_State == State_Stunned)
            {
                NPC.rotation += 0.35f;
                AI_Timer--;
                if (AI_Timer <= 0)
                {
                    AI_State = State_Recovering;
                    AI_Timer = 60;
                }
            }
            else if (AI_State == State_Recovering)
            {
                AI_Timer--;
                if (AI_Timer > 20)
                {
                    float speed = 0.35f * ((AI_Timer - 20f) / 40f);
                    NPC.rotation += speed;
                }
                else if (AI_Timer == 20)
                {
                    NPC.rotation = MathHelper.WrapAngle(NPC.rotation);
                    scanTargetRotation = NPC.rotation + Main.rand.NextFloat(-0.25f, 0.25f);
                }
                else
                {
                    NPC.rotation = MathHelper.Lerp(NPC.rotation, scanTargetRotation, 0.15f);
                }
                if (AI_Timer <= 0)
                {
                    AI_State = State_Scanning;
                    AI_Timer = 0;
                    scanHoldTimer = 0;
                }
            }
            else if (AI_State == State_Surprised)
            {
                AI_Timer--;
                Player player = Main.player[(int)AI_TargetPlayer];
                if (player.active && !player.dead)
                {
                    Vector2 directionToPlayer = player.Center - NPC.Center;
                    float angleToPlayer = directionToPlayer.ToRotation();
                    NPC.rotation = Terraria.Utils.AngleLerp(NPC.rotation, angleToPlayer, 0.3f);
                }
                if (AI_Timer <= 0)
                {
                    AI_State = State_LockedOn;
                    AI_Timer = 0;
                }
            }
            else if (AI_State == State_Scanning)
            {
                AI_Timer++;
                if (scanHoldTimer <= 0)
                {
                    scanTargetRotation = Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi);
                    scanHoldTimer = Main.rand.Next(50, 120);
                }
                scanHoldTimer--;
                NPC.rotation = Terraria.Utils.AngleLerp(NPC.rotation, scanTargetRotation, 0.3f);
                NPC.TargetClosest(false);
                Player player = Main.player[NPC.target];
                if (player.active && !player.dead)
                {
                    float distanceToPlayer = Vector2.Distance(player.Center, NPC.Center);
                    if (distanceToPlayer < 1000f)
                    {
                        Vector2 lookDirection = NPC.rotation.ToRotationVector2();
                        Vector2 directionToPlayer = player.Center - NPC.Center;
                        directionToPlayer.Normalize();
                        float dotProduct = Vector2.Dot(lookDirection, directionToPlayer);
                        if (dotProduct > 0.866f)
                        {
                            if (Collision.CanHitLine(NPC.position, NPC.width, NPC.height, player.position, player.width, player.height))
                            {
                                AI_State = State_Surprised;
                                AI_TargetPlayer = NPC.target;
                                AI_Timer = 25;
                                NPC.rotation = directionToPlayer.ToRotation();
                            }
                        }
                    }
                }
            }
            else if (AI_State == State_LockedOn)
            {
                Player player = Main.player[(int)AI_TargetPlayer];
                if (!player.active || player.dead)
                {
                    AI_State = State_Scanning;
                    AI_Timer = 0;
                    return;
                }
                float distanceToPlayer = Vector2.Distance(player.Center, NPC.Center);
                Vector2 directionToPlayer = player.Center - NPC.Center;
                directionToPlayer.Normalize();
                bool canSee = Collision.CanHitLine(NPC.position, NPC.width, NPC.height, player.position, player.width, player.height);
                if (canSee && distanceToPlayer < 1200f)
                {
                    AI_LastKnownRotation = directionToPlayer.ToRotation();
                    NPC.rotation = AI_LastKnownRotation;
                    if (AI_LostSightTimer > 0)
                    {
                        AI_Timer = 0;
                    }
                    AI_LostSightTimer = 0;
                    if (AI_Timer < 15)
                    {
                        AI_Timer++;
                    }
                    if (AI_Timer >= 15)
                    {
                        if (AI_Timer == 15)
                        {
                            for (int i = 0; i < 20; i++)
                            {
                                Vector2 offset = Main.rand.NextVector2Circular(24f, 24f);
                                Dust d = Dust.NewDustPerfect(player.Center + offset, DustID.PurpleTorch, offset * 0.15f);
                                d.noGravity = true;
                            }
                            SoundEngine.PlaySound(SoundID.Item100 with { Volume = 0.5f, Pitch = 0.2f }, player.Center);
                            AI_Timer = 16;
                        }
                        WatcherPlayer watcherPlayer = player.GetModPlayer<WatcherPlayer>();
                        watcherPlayer.IsBeingWatchedThisFrame = true;
                        watcherPlayer.GazeIntensity += 1f;
                        if (Main.rand.NextBool(5))
                        {
                            Dust d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.PurpleTorch);
                            d.noGravity = true;
                            d.velocity *= 0.2f;
                        }
                    }
                    if (Main.rand.NextBool(3))
                    {
                        Dust.NewDust(NPC.Center + directionToPlayer * 16f, 1, 1, DustID.PurpleTorch, directionToPlayer.X * 4f, directionToPlayer.Y * 4f);
                    }
                }
                else
                {
                    AI_LostSightTimer++;
                    NPC.rotation = AI_LastKnownRotation;
                    if (AI_LostSightTimer > 90)
                    {
                        AI_State = State_Scanning;
                        AI_Timer = 0;
                    }
                }
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D eyeballTexture = ModContent.Request<Texture2D>("Jarkos/npc/CradleMobs/WatcherEye/WatcherEye").Value;
            Texture2D pupilTexture = ModContent.Request<Texture2D>("Jarkos/npc/CradleMobs/WatcherEye/WatcherEye_Pupil").Value;

            Vector2 centerPos = NPC.Center - screenPos;
            float idlePulse = 1f;

            if (AI_State == State_Scanning) idlePulse = 1f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 2f) * 0.05f;

            Vector2 squishScale = new Vector2(AI_SquishX * idlePulse, AI_SquishY * idlePulse);
            spriteBatch.Draw(eyeballTexture, centerPos, null, drawColor, 0f, eyeballTexture.Size() / 2f, squishScale, SpriteEffects.None, 0f);

            float pupilOffsetRadius = 6f;

            if (AI_State == State_Surprised) pupilOffsetRadius = 2f;
            else if (AI_State == State_LockedOn) pupilOffsetRadius = 8f;
            else if (AI_State == State_Recovering) pupilOffsetRadius = 3f;

            Vector2 pupilOffset = NPC.rotation.ToRotationVector2() * pupilOffsetRadius;
            if (AI_State == State_LockedOn)
            {
                pupilOffset += Main.rand.NextVector2Circular(1f, 1f);
            }
            else if (AI_State == State_Surprised)
            {
                pupilOffset += Main.rand.NextVector2Circular(0.5f, 0.5f);
            }

            Vector2 pupilPos = centerPos + pupilOffset;
            float pupilRotation = NPC.rotation;
            
            if (AI_State == State_Stunned) pupilRotation += Main.GlobalTimeWrappedHourly * 15f;
            spriteBatch.Draw(pupilTexture, pupilPos, null, drawColor, pupilRotation, pupilTexture.Size() / 2f, AI_PupilScale, SpriteEffects.None, 0f);

            if (AI_State == State_LockedOn)
            {
                Player player = Main.player[(int)AI_TargetPlayer];
                if (player.active && !player.dead)
                {
                    bool canSee = Collision.CanHitLine(NPC.position, NPC.width, NPC.height, player.position, player.width, player.height);
                    float distanceToPlayer = Vector2.Distance(player.Center, NPC.Center);

                    if (canSee && distanceToPlayer < 1200f)
                    {
                        Texture2D texture = TextureAssets.MagicPixel.Value;
                        Vector2 startPos = pupilPos;
                        Vector2 endPos = player.Center - screenPos;

                        float progress = Math.Min(AI_Timer / 15f, 1f);
                        progress = 1f - (float)Math.Pow(1f - progress, 5);

                        Vector2 currentEndPos = Vector2.Lerp(startPos, endPos, progress);
                        Vector2 direction = currentEndPos - startPos;

                        float length = direction.Length();
                        if (length > 1f)
                        {
                            Vector2 directionNormalized = direction;
                            directionNormalized.Normalize();

                            Vector2 perpendicular = new Vector2(-directionNormalized.Y, directionNormalized.X);
                            float opacity = 0.5f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 8f) * 0.2f;
                            Color beamColor = new Color(130, 20, 180) * opacity;
                            float time = Main.GlobalTimeWrappedHourly * 6f;

                            Vector2 prevPos = startPos;
                            int segments = (int)(length / 8f);
                            if (segments > 0)
                            {
                                for (int i = 1; i <= segments; i++)
                                {
                                    float t = i / (float)segments;

                                    Vector2 basePos = Vector2.Lerp(startPos, currentEndPos, t);
                                    float waveEnvelope = (float)Math.Sin(t * Math.PI);
                                    float wave = (float)Math.Sin(t * 15f - time) * 18f * waveEnvelope;

                                    Vector2 currentPos = basePos + perpendicular * wave;
                                    Vector2 segmentDir = currentPos - prevPos;

                                    float segmentLength = segmentDir.Length();
                                    float segmentRot = segmentDir.ToRotation();
                                    float thickness = 2f + waveEnvelope * 1.5f;

                                    spriteBatch.Draw(texture, prevPos, new Rectangle(0, 0, 1, 1), beamColor, segmentRot, new Vector2(0, 0.5f), new Vector2(segmentLength, thickness), SpriteEffects.None, 0f);
                                    prevPos = currentPos;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
    public class WatcherPlayer : ModPlayer
    {
        public float GazeIntensity = 0f;
        public bool IsBeingWatchedThisFrame = false;
        public bool WasWatchedLastFrame = false;
        private SlotId droneSoundSlot;
        public override void PreUpdate()
        {
            if (Player.HasBuff(ModContent.BuffType<BundleOfMedicinesBuff>()))
            {
                GazeIntensity = 0f;
                IsBeingWatchedThisFrame = false;
                return;
            }
            if (!WasWatchedLastFrame)
            {
                if (GazeIntensity > 0)
                {
                    GazeIntensity -= 2f;
                    if (GazeIntensity < 0) GazeIntensity = 0;
                }
            }
        }
        public override void PostUpdateMiscEffects()
        {
            if (Player.HasBuff(ModContent.BuffType<BundleOfMedicinesBuff>()))
            {
                GazeIntensity = 0f;
                IsBeingWatchedThisFrame = false;
                Player.ClearBuff(ModContent.BuffType<VulnerabilityBuff>());
                if (!Main.dedServ && Terraria.Graphics.Effects.Filters.Scene[WatcherShaderSystem.FilterKey].IsActive())
                {
                    Terraria.Graphics.Effects.Filters.Scene.Deactivate(WatcherShaderSystem.FilterKey);
                }
                return;
            }
            if (GazeIntensity > 0)
            {
                Player.AddBuff(ModContent.BuffType<VulnerabilityBuff>(), 2);

                float maxGaze = 600f;
                float intensityRatio = GazeIntensity / maxGaze;
                if (intensityRatio > 1f) intensityRatio = 1f;
                float speedMultiplier = 1f - (0.8f * intensityRatio);

                Player.moveSpeed *= speedMultiplier;
                Player.maxRunSpeed *= speedMultiplier;
                Player.endurance -= 0.5f * intensityRatio;

                if (Main.myPlayer == Player.whoAmI)
                {
                    if (!SoundEngine.TryGetActiveSound(droneSoundSlot, out ActiveSound droneSound))
                    {
                        SoundStyle style = new SoundStyle("Jarkos/sounds/eye_bass") with { IsLooped = true, Volume = 1f };
                        droneSoundSlot = SoundEngine.PlaySound(style, Player.Center);
                    }
                    if (SoundEngine.TryGetActiveSound(droneSoundSlot, out droneSound))
                    {
                        droneSound.Position = Player.Center;
                        droneSound.Volume = intensityRatio * 1.5f;
                    }
                }
                if (!Main.dedServ)
                {
                    if (!Terraria.Graphics.Effects.Filters.Scene[WatcherShaderSystem.FilterKey].IsActive())
                    {
                        Terraria.Graphics.Effects.Filters.Scene.Activate(WatcherShaderSystem.FilterKey);
                    }
                    Terraria.Graphics.Effects.Filters.Scene[WatcherShaderSystem.FilterKey].GetShader()
                        .UseOpacity(intensityRatio)
                        .UseProgress(Main.GlobalTimeWrappedHourly)
                        .UseImageOffset(new Vector2(Main.screenWidth, Main.screenHeight));
                }
            }
            else
            {
                Player.ClearBuff(ModContent.BuffType<VulnerabilityBuff>());
                if (!Main.dedServ && Terraria.Graphics.Effects.Filters.Scene[WatcherShaderSystem.FilterKey].IsActive())
                {
                    Terraria.Graphics.Effects.Filters.Scene.Deactivate(WatcherShaderSystem.FilterKey);
                }
            }
        }
        public override void PostUpdate()
        {
            WasWatchedLastFrame = IsBeingWatchedThisFrame;
            IsBeingWatchedThisFrame = false;
        }
    }
}
