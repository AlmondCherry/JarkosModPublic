using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Filters = Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.projectiles.enemy.tenderote;
using jarkos.systems;
using Terraria.Localization;
using jarkos.items.bags;
using jarkos.items.materials;
using jarkos.items.weapons.magic.toms;
using jarkos.items.weapons.melee.other_melee;
using jarkos.items.weapons.ranged.firing;
using Terraria.GameContent;
using jarkos.tiles;
using Jarkos.items.usetiles;
using jarkos.items.usetiles;
using jarkos.particles;
using Jarkos.systems;

namespace jarkos.npc.tenderote
{
    [AutoloadBossHead]
    public class Tenderote_head : ModNPC
    {
        public static bool HostCheck => Main.netMode != NetmodeID.MultiplayerClient;
        private TenderoteState CurrentState
        {
            get => (TenderoteState)NPC.ai[0];
            set => NPC.ai[0] = (float)value;
        }
        public ref float StateTimer => ref NPC.ai[1];
        public ref float AttackCounter => ref NPC.ai[2];
        public ref float SubTimer => ref NPC.ai[3];
        private float currentSpeed = 0f;
        private float orbitAngle = 0f;
        private float portalYOffset = 0f;
        private enum TenderoteState
        {
            Emerging = 0, Phase1_Daggers = 1, Phase1_Grid = 2, Phase1_Rain = 3, Phase2_LaserSpheres = 4, Phase2_WhiteGrid = 5, Phase3_RoarWave = 6, Phase3_FireCircle = 7, Phase3_CircularDaggers = 8, Phase3_FireCurve = 9, Dying = 10, Phase2_Dash = 11, Phase2_DoubleDaggers = 12, Phase3_InkSphere = 13, Phase3_SpamDaggers = 14
        }
        public override void SetStaticDefaults()
        {
            NPCID.Sets.TrailCacheLength[NPC.type] = 10;
            NPCID.Sets.TrailingMode[NPC.type] = 1;
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.ImmuneToRegularBuffs[Type] = true;
            NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                CustomTexturePath = $"jarkos/npc/tenderote/bestiary_tenderote", Scale = 1.25f, Position = new Vector2(16 * 10.5f * 1.25f, 0), PortraitScale = 1.25f, PortraitPositionXOverride = 16 * 8 * 1.25f
            });
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime, BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface, new FlavorTextBestiaryInfoElement("...")
            });
        }
        public override void SetDefaults()
        {
            NPC.width = 100;
            NPC.height = 100;
            NPC.damage = 140;
            NPC.defense = 80;
            NPC.lifeMax = 170000;
            NPC.HitSound = SoundID.NPCHit41;
            NPC.DeathSound = SoundID.NPCDeath14;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.knockBackResist = 0f;
            NPC.lavaImmune = true;
            NPC.aiStyle = -1;
            NPC.value = Item.buyPrice(5, 45, 25, 15);
            NPC.boss = true;
            SceneEffectPriority = SceneEffectPriority.BossLow;
            NPC.trapImmune = true;
            NPC.scale = 1.5f;
            if (!Main.dedServ)
            {
                Music = MusicLoader.GetMusicSlot(Mod, "Music/Tenderote");
            }
            NPC.BossBar = ModContent.GetInstance<TenderoteBossBar>();
        }
        public override bool CheckActive()
        {
            return false;
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(currentSpeed);
            writer.Write(orbitAngle);
            writer.Write(portalYOffset);
        }
        public override void ReceiveExtraAI(BinaryReader reader)
        {
            currentSpeed = reader.ReadSingle();
            orbitAngle = reader.ReadSingle();
            portalYOffset = reader.ReadSingle();
        }
        public override void AI()
        {
            Player player = Main.player[NPC.target];
            if (!player.active || player.dead)
            {
                NPC.TargetClosest(false);
                player = Main.player[NPC.target];
                if (!player.active || player.dead)
                {
                    NPC.TargetClosest(false);
                    player = Main.player[NPC.target];
                    if (!player.active || player.dead)
                    {
                        TenderodScreenSystem.TargetDarken = 0f;
                        NPC.velocity.Y += 0.5f;
                        NPC.EncourageDespawn(10);
                        if (NPC.timeLeft > 10) NPC.timeLeft = 10;
                        if (NPC.position.Y > Main.maxTilesY * 16 || NPC.position.Y < 0 || NPC.timeLeft <= 1)
                        {
                            NPC.active = false;
                            NPC.netUpdate = true;
                        }
                        return;
                    }
                }
            }
            NPC.timeLeft = NPC.activeTime * 30;
            TenderodScreenSystem.TargetDarken = 1f;
            NPC.rotation = 0f;
            switch (CurrentState)
            {
                case TenderoteState.Emerging:
                    DoEmerging(player);
                    break;
                case TenderoteState.Phase1_Daggers:
                    DoPhase1_Daggers(player);
                    break;
                case TenderoteState.Phase1_Grid:
                    DoPhase1_Grid(player);
                    break;
                case TenderoteState.Phase1_Rain:
                    DoPhase1_Rain(player);
                    break;
                case TenderoteState.Phase2_LaserSpheres:
                    DoPhase2_LaserSpheres(player);
                    break;
                case TenderoteState.Phase2_WhiteGrid:
                    DoPhase2_WhiteGrid(player);
                    break;
                case TenderoteState.Phase3_RoarWave:
                    DoPhase3_RoarWave(player);
                    break;
                case TenderoteState.Phase3_FireCircle:
                    DoPhase3_FireCircle(player);
                    break;
                case TenderoteState.Phase3_CircularDaggers:
                    DoPhase3_CircularDaggers(player);
                    break;
                case TenderoteState.Phase3_FireCurve:
                    DoPhase3_FireCurve(player);
                    break;
                case TenderoteState.Phase2_Dash:
                    DoPhase2_Dash(player);
                    break;
                case TenderoteState.Phase2_DoubleDaggers:
                    DoPhase2_DoubleDaggers(player);
                    break;
                case TenderoteState.Phase3_InkSphere:
                    DoPhase3_InkSphere(player);
                    break;
                case TenderoteState.Phase3_SpamDaggers:
                    DoPhase3_SpamDaggers(player);
                    break;
                case TenderoteState.Dying:
                    DoDying();
                    break;
            }
            if (CurrentState >= TenderoteState.Phase3_RoarWave && CurrentState != TenderoteState.Dying)
            {
                if (Vector2.Distance(NPC.Center, player.Center) > 1200f)
                {
                    NPC.Center = player.Center + new Vector2(Main.rand.NextBool() ? -500f : 500f, -200f);
                    if (!Main.dedServ)
                    {
                        for (int i = 0; i < 30; i++)
                        {
                            Dust d = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Wraith);
                            d.noGravity = true;
                            d.scale = 2f;
                        }
                    }
                    NPC.netUpdate = true;
                }
            }
            if (CurrentState != TenderoteState.Emerging)
            {
                NPC.rotation = 0f;
            }
            float lifePct = (float)NPC.life / NPC.lifeMax;
            bool isDying = CurrentState == TenderoteState.Dying;
            if (lifePct <= 0.25f && NPC.localAI[0] == 0f && !isDying)
            {
                NPC.localAI[0] = 1f;
                NPC.localAI[1] = 1f;
                ChangeState(TenderoteState.Phase3_RoarWave);
                return;
            }
            else if (lifePct <= 0.5f && NPC.localAI[1] == 0f && !isDying)
            {
                NPC.localAI[1] = 1f;
                ChangeState(TenderoteState.Phase2_LaserSpheres);
                return;
            }
        }
        private void ChangeState(TenderoteState newState)
        {
            CurrentState = newState;
            StateTimer = 0;
            AttackCounter = 0;
            SubTimer = 0;
            NPC.netUpdate = true;
        }
        private void DoEmerging(Player player)
        {
            StateTimer++;
            NPC.dontTakeDamage = true;
            NPC.velocity = Vector2.Zero;
            if (StateTimer < 90)
            {
                float progress = StateTimer / 90f;
                portalYOffset = MathHelper.Lerp(100f, 0f, progress);
                NPC.rotation = -MathHelper.PiOver2;
            }
            else
            {
                NPC.dontTakeDamage = false;
                ChangeState(TenderoteState.Phase1_Daggers);
            }
        }
        private void DoPhase1_Daggers(Player player)
        {
            StateTimer++;
            Vector2 targetPos = player.Center + new Vector2(0, -350f);
            if (StateTimer < 40)
            {
                targetPos.Y -= 150f;
                NPC.velocity = Vector2.Lerp(NPC.velocity, (targetPos - NPC.Center) * 0.02f, 0.05f);
                NPC.rotation = MathHelper.Lerp(NPC.rotation, -0.3f, 0.1f);
            }
            else if (StateTimer < 60)
            {
                float t = (StateTimer - 40f) / 20f;
                float easeOut = 1f - (1f - t) * (1f - t);
                NPC.velocity = Vector2.Lerp(NPC.velocity, (targetPos - NPC.Center) * 0.2f, easeOut * 0.5f);
                NPC.rotation = MathHelper.Lerp(NPC.rotation, 0.2f, 0.2f);
            }
            else
            {
                NPC.velocity = Vector2.Lerp(NPC.velocity, (targetPos - NPC.Center) * 0.05f, 0.1f);
                NPC.rotation = MathHelper.Lerp(NPC.rotation, 0f, 0.1f);

            if (StateTimer > 60 && StateTimer % 5 == 0 && AttackCounter < 35)
            {
                if (HostCheck)
                {
                    float sweep = (float)Math.Sin(AttackCounter * 0.3f) * 0.8f;
                    float baseAngle = MathHelper.PiOver2 + sweep;

                    for (int i = -2; i <= 2; i++)
                    {
                        Vector2 dir = (baseAngle + i * 0.2f).ToRotationVector2();
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, dir * 2f, ModContent.ProjectileType<Tenderote_dagger>(), NPC.damage / 4, 0f, Main.myPlayer, player.whoAmI);
                        if (!Main.dedServ)
                        {
                            new Spark(NPC.Center, dir * 5f, Color.Red, 40, 1.5f, 1f).Spawn();
                        }
                    }
                }
                AttackCounter++;
            }
            if (StateTimer > 240)
            {
                ChangeState(TenderoteState.Phase1_Grid);
            }
        }
        }
        private void DoPhase1_Grid(Player player)
        {
            StateTimer++;
            if (StateTimer < 60)
            {
                float t = StateTimer / 60f;
                float easeIn = t * t * t;
                NPC.rotation += easeIn * 1.5f;
                NPC.velocity *= 0.9f;
            }
            else
            {
                NPC.rotation = MathHelper.Lerp(NPC.rotation, 0f, 0.3f);
                NPC.velocity = Vector2.Zero;
            }
            if (StateTimer == 60)
            {
                if (HostCheck)
                {
                    int size = 250;
                    Vector2 center = player.Center;

                    center.X = (float)Math.Round(center.X / size) * size;
                    center.Y = (float)Math.Round(center.Y / size) * size;

                    for (int x = -6; x <= 6; x++)
                    {
                        for (int y = -4; y <= 4; y++)
                        {
                            if (Main.rand.NextBool(10)) continue;

                            Vector2 pos = center + new Vector2(x * size, y * size);
                            float dist = new Vector2(x, y).Length();
                            float delay = dist * 1f;
                            
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), pos, Vector2.Zero, ModContent.ProjectileType<Tenderote_grid_square>(), NPC.damage / 2, 0f, Main.myPlayer, size, delay);
                        }
                    }
                }
            }
            if (StateTimer > 300)
            {
                ChangeState(TenderoteState.Phase1_Rain);
            }
        }
        private void DoPhase1_Rain(Player player)
        {
            StateTimer++;

            float sweepDistance = 600f;
            float sweepSpeed = 0.04f;
            float targetX = player.Center.X + (float)Math.Sin(StateTimer * sweepSpeed) * sweepDistance;
            float targetY = player.Center.Y - 400f;

            Vector2 targetPos = new Vector2(targetX, targetY);
            NPC.velocity = Vector2.Lerp(NPC.velocity, (targetPos - NPC.Center) * 0.05f, 0.1f);
            NPC.dontTakeDamage = true;

            if (StateTimer > 60 && StateTimer < 300 && StateTimer % 5 == 0)
            {
                if (HostCheck)
                {
                    for (int i = 0; i < 15; i++)
                    {
                        float offsetX;
                        do
                        {
                            offsetX = Main.rand.NextFloat(-2500, 2500);
                        } while (Math.Abs(offsetX) < 250f);
                        float depth = Main.rand.NextFloat(0.5f, 2.0f);
                        Vector2 pos = new Vector2(NPC.Center.X + offsetX, player.Center.Y - 1000f);
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), pos, new Vector2(0, 1f), ModContent.ProjectileType<Tenderote_dark_drop>(), NPC.damage / 3, 0f, Main.myPlayer, NPC.whoAmI, depth);
                        if (!Main.dedServ && depth > 1.5f)
                        {
                            new Circleedge(pos, Vector2.Zero, Color.White, 30, 0.5f, 1f, 0f).Spawn();
                        }
                    }
                }
            }
            if (StateTimer > 360)
            {
                NPC.dontTakeDamage = false;
                ChangeState(TenderoteState.Phase1_Daggers);
            }
        }
        private void DoPhase2_LaserSpheres(Player player)
        {
            StateTimer++;
            Vector2 targetPos = player.Center + new Vector2(0, -350f);
            NPC.velocity = Vector2.Lerp(NPC.velocity, (targetPos - NPC.Center) * 0.05f, 0.1f);
            if (StateTimer == 1 && !Main.dedServ)
            {
                new StarParticleBigger(NPC.Center + new Vector2(-400, 0), Vector2.Zero, Color.White, 60, 2f, 0.8f, 0.1f).Spawn();
                new StarParticleBigger(NPC.Center + new Vector2(400, 0), Vector2.Zero, Color.White, 60, 2f, 0.8f, -0.1f).Spawn();
            }
            if (StateTimer == 60)
            {
                if (HostCheck)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(-400, 0), Vector2.Zero, ModContent.ProjectileType<Laser_sphere>(), NPC.damage, 0f, Main.myPlayer, player.whoAmI, 1f);
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(400, 0), Vector2.Zero, ModContent.ProjectileType<Laser_sphere>(), NPC.damage, 0f, Main.myPlayer, player.whoAmI, -1f);
                    if (!Main.dedServ)
                    {
                        new Circleedge(NPC.Center + new Vector2(-400, 0), Vector2.Zero, Color.White, 60, 1f, 1f, 0.1f).Spawn();
                        new Circleedge(NPC.Center + new Vector2(400, 0), Vector2.Zero, Color.White, 60, 1f, 1f, -0.1f).Spawn();

                        TenderodScreenSystem.ScreenShakeAmount = 15f;
                        TenderodScreenSystem.RoarDistortion = 1f;
                    }
                }
            }
            if (StateTimer > 300)
            {
                ChangeState(TenderoteState.Phase2_WhiteGrid);
            }
        }
        private void DoPhase2_WhiteGrid(Player player)
        {
            StateTimer++;
            NPC.velocity = Vector2.Lerp(NPC.velocity, Vector2.Zero, 0.05f);
            if (StateTimer == 60)
            {
                if (HostCheck)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), player.Center, Vector2.Zero, ModContent.ProjectileType<White_grid>(), NPC.damage, 0f, Main.myPlayer, player.whoAmI);
                }
            }
            if (StateTimer > 300)
            {
                if ((float)NPC.life / NPC.lifeMax <= 0.25f)
                    ChangeState(TenderoteState.Phase3_RoarWave);
                else
                    ChangeState(TenderoteState.Phase2_Dash);
            }
        }
        private void DoPhase2_Dash(Player player)
        {
            StateTimer++;
            SubTimer++;
            if (SubTimer < 30)
            {
                Vector2 dir = Vector2.Normalize(player.Center - NPC.Center);
                NPC.velocity = Vector2.Lerp(NPC.velocity, -dir * 3f, 0.1f);
                if (SubTimer == 29)
                {
                    NPC.velocity = dir * 32f;
                    SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
                }
            }
            else if (SubTimer < 70)
            {
                if (HostCheck && SubTimer % 2 == 0)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<Tenderote_black_fire>(), NPC.damage / 3, 0f, Main.myPlayer);
                }
            }
            else
            {
                NPC.velocity *= 0.9f;
            }
            if (SubTimer > 80)
            {
                SubTimer = 0;
                AttackCounter++;
            }
            if (AttackCounter >= 3)
            {
                ChangeState(TenderoteState.Phase2_DoubleDaggers);
            }
        }
        private void DoPhase2_DoubleDaggers(Player player)
        {
            StateTimer++;
            NPC.velocity = Vector2.Lerp(NPC.velocity, Vector2.Zero, 0.1f);
            if (StateTimer > 30 && StateTimer < 210)
            {
                if (StateTimer % 4 == 0 && HostCheck)
                {
                    float timePhase = StateTimer * 0.25f;
                    Vector2 p1 = NPC.Center + new Vector2(100f, 0).RotatedBy(timePhase);
                    Vector2 p2 = NPC.Center + new Vector2(-100f, 0).RotatedBy(-timePhase);

                    Vector2 v1 = Vector2.Normalize(p1 - NPC.Center) * 12f;
                    Vector2 v2 = Vector2.Normalize(p2 - NPC.Center) * 12f;

                    Projectile.NewProjectile(NPC.GetSource_FromAI(), p1, v1, ModContent.ProjectileType<Tenderote_black_dagger>(), NPC.damage / 3, 0f, Main.myPlayer, 0f);
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), p2, v2, ModContent.ProjectileType<Tenderote_black_dagger>(), NPC.damage / 3, 0f, Main.myPlayer, 0f);
                }
            }
            if (StateTimer > 240)
            {
                ChangeState(TenderoteState.Phase2_LaserSpheres);
            }
        }
        private void DoPhase3_RoarWave(Player player)
        {
            StateTimer++;
            NPC.velocity = Vector2.Lerp(NPC.velocity, Vector2.Zero, 0.1f);
            if (StateTimer < 240)
            {
                float shakeIntensity = (StateTimer / 240f) * 3f;
                NPC.position += Main.rand.NextVector2Circular(shakeIntensity, shakeIntensity);
            }
            if (StateTimer == 240)
            {
                SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
                if (!Main.dedServ)
                {
                    new Circleedge(NPC.Center, Vector2.Zero, Color.Red, 60, 2f, 1f, 0.1f).Spawn();
                    for (int i = 0; i < 20; i++)
                    {
                        Vector2 vel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(5f, 15f);
                        new Spark(NPC.Center, vel, Color.White, 60, 2f, 1f).Spawn();
                    }
                    TenderodScreenSystem.ScreenShakeAmount = 35f;
                    TenderodScreenSystem.InvertIntensity = 1f;
                    TenderodScreenSystem.RoarDistortion = 1.5f;
                }
                if (HostCheck)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<Boss_roar_wave>(), NPC.damage * 2, 0f, Main.myPlayer, NPC.whoAmI);
                }
            }
            if (StateTimer > 360)
            {
                ChangeState(TenderoteState.Phase3_FireCircle);
            }
        }
        private void DoPhase3_CircularDaggers(Player player)
        {
            StateTimer++;
            NPC.velocity = Vector2.Lerp(NPC.velocity, Vector2.Zero, 0.1f);
            if (StateTimer == 60)
            {
                if (HostCheck)
                {
                    int daggerCount = 20;
                    if (!Main.dedServ)
                    {
                        new Circleedge(NPC.Center, Vector2.Zero, Color.Red, 45, 1f, 1f, 0.05f).Spawn();
                    }
                    for (int i = 0; i < daggerCount; i++)
                    {
                        Vector2 velocity = (MathHelper.TwoPi * ((float)i / daggerCount)).ToRotationVector2() * 8f;
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, ModContent.ProjectileType<Tenderote_dagger>(), NPC.damage / 3, 0f, Main.myPlayer, player.whoAmI);
                    }
                }
            }
            if (StateTimer > 180)
            {
                ChangeState(TenderoteState.Phase3_FireCurve);
            }
        }
        private void DoPhase3_FireCircle(Player player)
        {
            StateTimer++;
            NPC.velocity = Vector2.Lerp(NPC.velocity, Vector2.Zero, 0.1f);
            if (StateTimer == 30)
            {
                if (HostCheck)
                {
                    int fireCount = 16;
                    float radius = 500f;
                    for (int i = 0; i < fireCount; i++)
                    {
                        Vector2 offset = (MathHelper.TwoPi * ((float)i / fireCount)).ToRotationVector2() * radius;
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), player.Center + offset, Vector2.Zero, ModContent.ProjectileType<Tenderote_fire>(), NPC.damage / 3, 0f, Main.myPlayer, 0f);
                    }
                }
            }
            if (StateTimer > 210)
            {
                ChangeState(TenderoteState.Phase3_CircularDaggers);
            }
        }
        private void DoPhase3_FireCurve(Player player)
        {
            StateTimer++;
            NPC.velocity = Vector2.Lerp(NPC.velocity, Vector2.Zero, 0.1f);
            if (StateTimer == 30)
            {
                if (HostCheck)
                {
                    int fireCount = 12;
                    for (int i = 0; i < fireCount; i++)
                    {
                        float angle = MathHelper.TwoPi * ((float)i / fireCount);
                        Vector2 offset = angle.ToRotationVector2() * 20f;
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + offset, Vector2.Zero, ModContent.ProjectileType<Tenderote_fire>(), NPC.damage / 3, 0f, Main.myPlayer, 1f, angle);
                    }
                }
            }
            if (StateTimer > 240)
            {
                ChangeState(TenderoteState.Phase3_InkSphere);
            }
        }
        private void DoPhase3_InkSphere(Player player)
        {
            StateTimer++;
            Vector2 targetPos = player.Center + new Vector2(0, -100f);
            NPC.velocity = Vector2.Lerp(NPC.velocity, (targetPos - NPC.Center) * 0.05f, 0.1f);

            if (StateTimer == 1 && !Main.dedServ)
            {
                new StarParticleBigger(NPC.Center + new Vector2(0, -350f), Vector2.Zero, Color.Black, 60, 3f, 0.8f, 0.1f).Spawn();
            }
            if (StateTimer == 60 && HostCheck)
            {
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(0, -350f), Vector2.Zero, ModContent.ProjectileType<Tenderote_ink_sphere_proj>(), NPC.damage / 2, 0f, Main.myPlayer);
            }
            if (StateTimer > 330)
            {
                ChangeState(TenderoteState.Phase3_SpamDaggers);
            }
        }
        private void DoPhase3_SpamDaggers(Player player)
        {
            StateTimer++;
            if (StateTimer % 60 == 0)
            {
                Vector2 dashPos = player.Center + Main.rand.NextVector2CircularEdge(400f, 400f);
                NPC.velocity = Vector2.Normalize(dashPos - NPC.Center) * 18f;
            }
            NPC.velocity *= 0.98f;
            if (StateTimer % 5 == 0 && HostCheck)
            {
                Vector2 dir = Vector2.Normalize(player.Center - NPC.Center) * 16f;
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, dir.RotatedBy(0.2f), ModContent.ProjectileType<Tenderote_black_dagger>(), NPC.damage / 3, 0f, Main.myPlayer, 0f);
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, dir.RotatedBy(-0.2f), ModContent.ProjectileType<Tenderote_black_dagger>(), NPC.damage / 3, 0f, Main.myPlayer, 0f);
            }
            if (StateTimer > 300)
            {
                ChangeState(TenderoteState.Phase3_RoarWave);
            }
        }
        private void DoDying()
        {
            StateTimer++;
            NPC.velocity *= 0.95f;
            if (StateTimer < 140)
            {
                float shake = (1f - StateTimer / 140f) * 5f;
                NPC.position += Main.rand.NextVector2Circular(shake, shake);
            }
            if (StateTimer > 180)
            {
                TenderodScreenSystem.TargetDarken = 0f;
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
        public override bool CheckDead()
        {
            if (CurrentState != TenderoteState.Dying)
            {
                ChangeState(TenderoteState.Dying);
                NPC.life = 1;
                NPC.dontTakeDamage = true;
                SoundEngine.PlaySound(SoundID.NPCDeath14, NPC.Center);
                return false;
            }
            return true;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = TextureAssets.Npc[NPC.type].Value;
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / (Main.npcFrameCount[NPC.type] * 2f));
            Vector2 drawPosition = NPC.Center - screenPos;

            if (CurrentState == TenderoteState.Emerging)
            {
                drawPosition.Y += portalYOffset;
                float progress = StateTimer / 90f;
                drawColor *= progress;
            }

            for (int i = 0; i < NPC.oldPos.Length; i++)
            {
                float alpha = 1f - (i / (float)NPC.oldPos.Length);
                spriteBatch.Draw(tex, NPC.oldPos[i] + NPC.Size / 2f - screenPos, NPC.frame, drawColor * alpha * 0.4f, NPC.oldRot[i], origin, NPC.scale, SpriteEffects.None, 0f);
            }

            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = (MathHelper.PiOver2 * i).ToRotationVector2() * 2f;
                spriteBatch.Draw(tex, drawPosition + offset, NPC.frame, Color.Red * 0.8f, NPC.rotation, origin, NPC.scale, SpriteEffects.None, 0f);
            }
            
            if (CurrentState == TenderoteState.Phase3_RoarWave && StateTimer < 240)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                Texture2D auraTex = ModContent.Request<Texture2D>("jarkos/projectiles/melee/unique_melee/StormWave").Value;
                float alpha = (float)Math.Sin(StateTimer / 240f * MathHelper.Pi);
                float pulse = 1f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 15f) * 0.1f;
                float targetRadius = 300f;
                float scale = targetRadius / ((auraTex.Width / 2f) * 0.82f);
                spriteBatch.Draw(auraTex, drawPosition, null, Color.Red * alpha * 0.8f, Main.GlobalTimeWrappedHourly, new Vector2(auraTex.Width/2f, auraTex.Height/2f), scale * pulse, SpriteEffects.None, 0f);
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }

            spriteBatch.Draw(tex, drawPosition, NPC.frame, drawColor, NPC.rotation, origin, NPC.scale, SpriteEffects.None, 0f);
            if (CurrentState == TenderoteState.Phase1_Rain)
            {
                spriteBatch.End();
                Effect shieldShader = ModContent.Request<Effect>("jarkos/Shaders/TenderoteShield", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                Texture2D noiseTex = ModContent.Request<Texture2D>("jarkos/projectiles/melee/unique_melee/WaveNoise", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

                if (shieldShader != null && noiseTex != null)
                {
                    shieldShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
                    shieldShader.Parameters["uColor"]?.SetValue(new Vector3(1.0f, 1.0f, 1.0f));

                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, shieldShader, Main.GameViewMatrix.TransformationMatrix);

                    float pulse = 1f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 5f) * 0.05f;
                    float diameter = 200f * pulse;
                    float scale = diameter / noiseTex.Width;

                    spriteBatch.Draw(noiseTex, NPC.Center - screenPos, null, Color.White, 0f, noiseTex.Size() / 2f, scale, SpriteEffects.None, 0f);
                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }
            return false;
        }
        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = ItemID.SuperHealingPotion;
        }
        public override void OnKill()
        {
            Main.NewText(Language.GetTextValue("Mods.jarkos.antique_gen"), color: Color.Yellow);
            for (int k = 0; k < (int)(Main.maxTilesX * Main.maxTilesY * 6E-05); k++)
            {
                int x = WorldGen.genRand.Next(0, Main.maxTilesX);
                int y = WorldGen.genRand.Next(Main.maxTilesY - 500, Main.maxTilesY - 200);
                Tile tile = Framing.GetTileSafely(x, y);

                if (tile.HasTile && tile.TileType == TileID.Stone && !DownedBossSystem.downedTenderod)
                {
                    for (int j = 0; j < 30; j++)
                    {
                        WorldGen.TileRunner(x, y, WorldGen.genRand.Next(22, 26), WorldGen.genRand.Next(4, 8), ModContent.TileType<AnticvarOre>());
                    }
                }
            }
            NPC.SetEventFlagCleared(ref DownedBossSystem.downedTenderod, -1);
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<Tenderot_bag>()));
            npcLoot.Add(ItemDropRule.MasterModeCommonDrop(ModContent.ItemType<TenderoteRelicItem>()));

            if (!Main.expertMode)
            {
                npcLoot.Add(ItemDropRule.NotScalingWithLuck(ModContent.ItemType<TenderoteMechanism>(), 7));
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<TenderoteMechanism>(), 1, 4, 8));
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<AntiqueOre>(), 1, 38, 102));
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<BlackHole>(), 4, 1, 1));
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Antique_melee_axe>(), 4, 1, 1));
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Antique_beam_rifle>(), 4, 1, 1));
            }
        }
    }
}
