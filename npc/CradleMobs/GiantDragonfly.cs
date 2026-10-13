using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
using Microsoft.Xna.Framework;
using System;
using Terraria.GameContent.Bestiary;
using jarkos.cradle_biome;
using Terraria.Audio;
using Terraria.GameContent.ItemDropRules;
using jarkos.items.materials;
using jarkos.systems;

namespace jarkos.npc.CradleMobs
{
    public class GiantDragonfly : ModNPC
    {
        public ref float AI_State => ref NPC.ai[0];
        public ref float AI_Timer => ref NPC.ai[1];
        public float frameCurrent = 0;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 6;
        }
        public override void SetDefaults()
        {
            NPC.width = 70;
            NPC.height = 58;
            NPC.damage = 25;
            NPC.defense = 5;
            NPC.lifeMax = 200;
            NPC.HitSound = null;
            NPC.DeathSound = null;
            NPC.value = 5000f;
            NPC.knockBackResist = 0.5f;
            NPC.noGravity = true;
            NPC.noTileCollide = false;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<PetrifiedAbyssBiome>().Type };
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                new FlavorTextBestiaryInfoElement(LangSystem.GiantDragonfly_desc.Value)
            });
        }
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.InModBiome<PetrifiedAbyssBiome>())
            {
                return SpawnCondition.Cavern.Chance * 0.15f;
            }
            return 0f;
        }
        private const int State_Wander = 0;
        private const int State_Aiming = 1;
        private const int State_Dashing = 2;
        private const int State_Resting = 3;
        private const int State_GroundWalk = 4;
        public ref float AI_DashDirX => ref NPC.ai[2];
        public ref float AI_DashDirY => ref NPC.ai[3];
        public ref float AI_FlightTime => ref NPC.localAI[0];
        public override void AI()
        {
            NPC.TargetClosest(true);
            Player player = Main.player[NPC.target];
            float distanceToPlayer = Vector2.Distance(player.Center, NPC.Center);
            float agroRange = 1200f;
            if (AI_State != State_GroundWalk)
            {
                NPC.noGravity = true;
                NPC.noTileCollide = false;
            }
            if (AI_State == State_Wander)
            {
                AI_Timer++;
                AI_FlightTime++;
                float speedLimit = 2f;
                float acceleration = 0.02f;
                float randomDrift = (float)Math.Sin(AI_Timer / 30f) * 0.3f;
                float baseVelY = (float)Math.Sin(AI_Timer / 60f) * 0.5f + randomDrift;
                Vector2 forward = new Vector2(-NPC.spriteDirection, 0);
                float sensorLength = 120f;
                Vector2 futurePos = NPC.position + forward * sensorLength;
                bool wallAhead = !Collision.CanHit(NPC.position, NPC.width, NPC.height, futurePos, NPC.width, NPC.height);
                if (wallAhead)
                {
                    Vector2 upPos = futurePos + new Vector2(0, -100f);
                    Vector2 downPos = futurePos + new Vector2(0, 100f);
                    bool spaceAbove = Collision.CanHit(NPC.position, NPC.width, NPC.height, upPos, NPC.width, NPC.height);
                    bool spaceBelow = Collision.CanHit(NPC.position, NPC.width, NPC.height, downPos, NPC.width, NPC.height);
                    if (spaceAbove && !spaceBelow)
                    {
                        NPC.velocity.Y -= 0.15f;
                    }
                    else if (spaceBelow && !spaceAbove)
                    {
                        NPC.velocity.Y += 0.15f;
                    }
                    else if (!spaceAbove && !spaceBelow)
                    {
                        if (AI_Timer % 30 == 0)
                        {
                            NPC.spriteDirection *= -1;
                            NPC.velocity.X *= -0.5f;
                        }
                    }
                    else
                    {
                        NPC.velocity.Y += (Math.Sin(AI_Timer / 10f) > 0 ? 0.1f : -0.1f);
                    }
                    NPC.velocity.X *= 0.95f;
                }
                else
                {
                    NPC.velocity.X += (-NPC.spriteDirection) * acceleration;
                    NPC.velocity.Y = MathHelper.Lerp(NPC.velocity.Y, baseVelY, 0.05f);
                }
                if (NPC.velocity.X > speedLimit) NPC.velocity.X = speedLimit;
                if (NPC.velocity.X < -speedLimit) NPC.velocity.X = -speedLimit;
                if (NPC.velocity.Y > speedLimit * 1.5f) NPC.velocity.Y = speedLimit * 1.5f;
                if (NPC.velocity.Y < -speedLimit * 1.5f) NPC.velocity.Y = -speedLimit * 1.5f;
                if (AI_FlightTime > 420)
                {
                    AI_State = State_GroundWalk;
                    AI_Timer = 0;
                }
                CheckForPlayerAgro(player, distanceToPlayer, agroRange);
            }
            else if (AI_State == State_GroundWalk)
            {
                AI_Timer++;
                NPC.noGravity = false;
                float walkSpeed = 0.8f;
                if (NPC.velocity.Y == 0)
                {
                    NPC.velocity.X = (-NPC.spriteDirection) * walkSpeed;
                    if (AI_Timer % 120 == 0 || NPC.collideX)
                    {
                        NPC.spriteDirection *= -1;
                    }
                }
                if (AI_Timer > 300)
                {
                    NPC.velocity.Y = -4f;
                    AI_State = State_Wander;
                    AI_Timer = 0;
                    AI_FlightTime = 0;
                }
                CheckForPlayerAgro(player, distanceToPlayer, agroRange);
            }
            else if (AI_State == State_Aiming)
            {
                AI_Timer++;
                NPC.velocity *= 0.95f;
                if (player.Center.X < NPC.Center.X)
                    NPC.spriteDirection = 1;
                else
                    NPC.spriteDirection = -1;
                if (AI_Timer >= 60)
                {
                    AI_State = State_Dashing;
                    AI_Timer = 0;
                    Vector2 dashDir = player.Center - NPC.Center;
                    dashDir.Normalize();
                    AI_DashDirX = dashDir.X;
                    AI_DashDirY = dashDir.Y;
                }
            }
            else if (AI_State == State_Dashing)
            {
                AI_Timer++;
                float dashSpeed = 14f;
                NPC.velocity = new Vector2(AI_DashDirX, AI_DashDirY) * dashSpeed;
                if (AI_Timer >= 45)
                {
                    AI_State = State_Resting;
                    AI_Timer = 0;
                }
            }
            else if (AI_State == State_Resting)
            {
                AI_Timer++;
                NPC.velocity *= 0.96f;
                if (AI_Timer >= 90)
                {
                    AI_State = State_Wander;
                    AI_Timer = 0;
                    AI_FlightTime = 0;
                }
                if (AI_Timer >= 30)
                {
                    CheckForPlayerAgro(player, distanceToPlayer, agroRange);
                }
            }
            if (AI_State == State_GroundWalk && NPC.velocity.Y == 0)
            {
                NPC.rotation = 0;
            }
            else if (AI_State == State_Resting && AI_Timer >= 30)
            {
                NPC.rotation = NPC.velocity.X * 0.02f;
            }
            else
            {
                NPC.rotation = NPC.velocity.X * 0.05f;
            }
            if (AI_State == State_Wander || AI_State == State_Dashing)
            {
                NPC.spriteDirection = NPC.velocity.X > 0 ? -1 : 1;
            }
            else if (AI_State == State_Resting)
            {
                if (AI_Timer < 30)
                {
                    NPC.spriteDirection = NPC.velocity.X > 0 ? -1 : 1;
                }
                else
                {
                    NPC.spriteDirection = player.Center.X < NPC.Center.X ? 1 : -1;
                }
            }
            if ((NPC.collideX || NPC.collideY) && AI_State != State_GroundWalk && AI_State != State_Aiming)
            {
                if (NPC.oldVelocity.Length() > 4f)
                {
                    Collision.HitTiles(NPC.position, NPC.oldVelocity, NPC.width, NPC.height);
                }
                if (AI_State == State_Dashing)
                {
                    AI_State = State_Resting;
                    AI_Timer = 0;
                    if (NPC.collideX) NPC.velocity.X = -NPC.oldVelocity.X * 0.4f;
                    if (NPC.collideY) NPC.velocity.Y = -NPC.oldVelocity.Y * 0.4f;
                }
            }
            if(frameCurrent == 3)
            {
                SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/MothWing") with { Volume = 0.25f, Pitch = Main.rand.NextFloat(-0.5f, 0.5f) }, NPC.position);
            }
        }
        private void CheckForPlayerAgro(Player player, float distanceToPlayer, float agroRange)
        {
            if (player.active && !player.dead && distanceToPlayer < agroRange)
            {
                Vector2 facingVector = new Vector2(-NPC.spriteDirection, 0);
                Vector2 directionToPlayer = player.Center - NPC.Center;
                directionToPlayer.Normalize();
                float dotProduct = Vector2.Dot(facingVector, directionToPlayer);
                bool inFieldOfView = dotProduct > 0.707f;
                bool hasLineOfSight = Collision.CanHitLine(NPC.position, NPC.width, NPC.height, player.position, player.width, player.height);
                if (inFieldOfView && hasLineOfSight)
                {
                    AI_State = State_Aiming;
                    AI_Timer = 0;
                }
            }
        }
        public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/MothHurt") with { Volume = 0.45f, Pitch = Main.rand.NextFloat(-0.25f, 0.25f) }, NPC.position);
        }
        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/MothHurt") with { Volume = 0.45f, Pitch = Main.rand.NextFloat(-0.25f, 0.25f) }, NPC.position);
        }
        public override void OnKill()
        {
            SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/MothDead") with { Volume = 0.45f, Pitch = Main.rand.NextFloat(-0.25f, 0.25f) }, NPC.position);
        }
        public override void FindFrame(int frameHeight)
        {
            double animationSpeed = 0.45 + NPC.velocity.Length() * 0.35;
            NPC.frameCounter += animationSpeed;
            if (NPC.frameCounter > 6.0)
            {
                frameCurrent++;
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;
                if (NPC.frame.Y >= frameHeight * Main.npcFrameCount[NPC.type])
                {
                    frameCurrent = 0;
                    NPC.frame.Y = 0;
                }
            }
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<RemainsOfMoth>(), 1, 1, 2));
        }
    }
}
