using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.npc.BloodMoon
{
	public class Bloodskeleton : ModNPC
    {
        private enum State { Walking, Jumping, Attacking }
        private ref float AIState => ref NPC.ai[0];
        private ref float Timer => ref NPC.ai[1];
        private ref float WasAirborne => ref NPC.ai[2];
        private ref float AttackCooldown => ref NPC.ai[3];
        private const float MeleeRange = 70f;
        private const float JumpRange = 250f;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 12;
            NPCID.Sets.NPCBestiaryDrawModifiers value = new()
            {
                Velocity = 1f
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
        }
        public override void SetDefaults()
        {
            NPC.width = 90;
            NPC.height = 214;
            NPC.damage = 40;
            NPC.defense = 12;
            NPC.lifeMax = 1000;
            NPC.HitSound = SoundID.NPCHit2;
            NPC.DeathSound = SoundID.NPCDeath2;
            NPC.value = Item.buyPrice(gold: 1);
            NPC.knockBackResist = 0.35f;
            NPC.aiStyle = -1;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
        }
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (Main.bloodMoon && spawnInfo.Player.ZoneOverworldHeight && !spawnInfo.Player.ZoneDungeon)
                return 0.005f;
            return 0f;
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Events.BloodMoon, new FlavorTextBestiaryInfoElement("...")
            });
        }
        public override void AI()
        {
            Player target = Main.player[NPC.target];
            if (!ValidateTarget(ref target))
                return;
            float distance = Vector2.Distance(NPC.Center, target.Center);
            if (AttackCooldown > 0)
                AttackCooldown--;
            CheckLanding();
            switch ((State)AIState)
            {
                case State.Walking:
                    WalkingAI(target, distance);
                    break;
                case State.Jumping:
                    JumpingAI(target, distance);
                    break;
                case State.Attacking:
                    AttackingAI(target, distance);
                    break;
            }
            if ((State)AIState != State.Attacking)
                NPC.spriteDirection = NPC.direction;
            Lighting.AddLight(NPC.Center, 0.6f, 0.1f, 0.1f);
            SpawnAmbientDust();
        }
        private bool ValidateTarget(ref Player target)
        {
            if (NPC.target < 0 || NPC.target == 255 || target.dead || !target.active)
            {
                NPC.TargetClosest();
                target = Main.player[NPC.target];
            }
            if (target.dead || !target.active)
            {
                NPC.velocity.Y += 0.5f;
                NPC.EncourageDespawn(10);
                return false;
            }
            return true;
        }
        private void CheckLanding()
        {
            bool justLanded = NPC.velocity.Y == 0 && NPC.oldVelocity.Y > 2f;
            if (WasAirborne == 1f && justLanded)
            {
                CreateLandingExplosion();
                WasAirborne = 0f;
            }
            if (NPC.velocity.Y != 0)
                WasAirborne = 1f;
        }
        private void WalkingAI(Player target, float distance)
        {
            Timer++;
            NPC.direction = target.Center.X > NPC.Center.X ? 1 : -1;
            if (distance < MeleeRange && AttackCooldown <= 0 && NPC.velocity.Y == 0)
            {
                AIState = (float)State.Attacking;
                Timer = 0;
                NPC.velocity.X *= 0.3f;
                SoundEngine.PlaySound(SoundID.Item1 with { Pitch = -0.3f }, NPC.Center);
                return;
            }
            if (distance > JumpRange && NPC.velocity.Y == 0 && Timer > 25)
            {
                PerformJump(1f);
                return;
            }
            float speed = 3.5f;
            NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, NPC.direction * speed, 0.08f);
            TryJumpOverObstacle();
            if (NPC.velocity.Y == 0 && distance > 120 && Timer > 50 && Main.rand.NextBool(40))
            {
                PerformJump(0.7f);
            }
        }
        private void JumpingAI(Player target, float distance)
        {
            Timer++;
            float airControl = 0.15f;
            if (target.Center.X > NPC.Center.X)
                NPC.velocity.X += airControl;
            else
                NPC.velocity.X -= airControl;
            NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -8f, 8f);
            SpawnJumpTrail();
            if (NPC.velocity.Y == 0 && Timer > 5)
            {
                AIState = (float)State.Walking;
                Timer = 0;
            }
            if (distance < MeleeRange * 1.2f && AttackCooldown <= 0)
            {
                AIState = (float)State.Attacking;
                Timer = 0;
            }
        }
        private void AttackingAI(Player target, float distance)
        {
            Timer++;
            NPC.velocity.X *= 0.85f;
            if (Timer == 12)
            {
                CreateAttackEffect(target);
                SoundEngine.PlaySound(SoundID.NPCHit2 with { Pitch = 0.3f, Volume = 0.8f }, NPC.Center);
            }
            if (Timer >= 28)
            {
                AIState = (float)State.Walking;
                Timer = 0;
                AttackCooldown = 35;
            }
        }
        private void PerformJump(float power)
        {
            AIState = (float)State.Jumping;
            Timer = 0;

            float jumpX = 7f * power;
            float jumpY = -11f * power;
            
            NPC.velocity.X = NPC.direction * jumpX;
            NPC.velocity.Y = jumpY;
            CreateJumpBurst();

            SoundEngine.PlaySound(SoundID.NPCHit2 with { Pitch = -0.5f, Volume = 0.6f }, NPC.Center);
        }
        private void TryJumpOverObstacle()
        {
            if (NPC.velocity.Y != 0) return;

            int tileX = (int)(NPC.Center.X / 16) + NPC.direction;
            int tileY = (int)(NPC.Bottom.Y / 16);

            if (WorldGen.SolidTile(tileX, tileY) || WorldGen.SolidTile(tileX, tileY - 1))
            {
                NPC.velocity.Y = -9f;
                NPC.velocity.X = NPC.direction * 4f;
            }
        }
        #region Visual Effects
        private void CreateLandingExplosion()
        {
            SoundEngine.PlaySound(SoundID.Item70 with { Pitch = -0.6f, Volume = 0.9f }, NPC.Bottom);

            for (int i = 0; i < 40; i++)
            {
                float angle = MathHelper.TwoPi * i / 40f;
                float speed = Main.rand.NextFloat(5f, 12f);
                Vector2 velocity = new((float)Math.Cos(angle) * speed, (float)Math.Sin(angle) * speed * 0.4f - 2f);
                Dust dust = Dust.NewDustDirect(NPC.Bottom - new Vector2(15, 8), 30, 8, DustID.Blood, velocity.X, velocity.Y, 80, default, Main.rand.NextFloat(1.8f, 2.8f));
                dust.noGravity = Main.rand.NextBool(4);
                dust.fadeIn = 1.2f;
            }
            for (int i = 0; i < 20; i++)
            {
                Vector2 vel = new(Main.rand.NextFloat(-8f, 8f), Main.rand.NextFloat(-5f, -1f));
                Dust smoke = Dust.NewDustDirect(NPC.Bottom - new Vector2(20, 5), 40, 5, DustID.Smoke, vel.X, vel.Y, 200, new Color(180, 40, 40), Main.rand.NextFloat(1.5f, 2.5f));
                smoke.noGravity = true;
                smoke.fadeIn = 1.5f;
            }
            for (int i = 0; i < 12; i++)
            {
                Vector2 sparkVel = new(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-8f, -3f));
                Dust Spark = Dust.NewDustDirect(NPC.Bottom - new Vector2(10, 3), 20, 3, DustID.Torch, sparkVel.X, sparkVel.Y, 0, new Color(255, 100, 100), 1.2f);
                Spark.noGravity = true;
            }
            for (int i = 0; i < 8; i++)
            {
                Vector2 goreVel = new(Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-7f, -2f));
                Gore.NewGoreDirect(NPC.GetSource_FromAI(), NPC.Bottom, goreVel, GoreID.Smoke1 + Main.rand.Next(3), Main.rand.NextFloat(0.6f, 1f));
            }

            float shakeDistance = Vector2.Distance(Main.LocalPlayer.Center, NPC.Center);

            if (shakeDistance < 350f)
            {
                float intensity = (350f - shakeDistance) / 350f;
                Main.LocalPlayer.velocity.Y -= intensity * 1.5f;
            }
        }
        private void CreateJumpBurst()
        {
            for (int i = 0; i < 18; i++)
            {
                Vector2 vel = new(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-6f, -2f));
                Dust dust = Dust.NewDustDirect(NPC.Bottom - new Vector2(12, 0), 24, 5, DustID.Blood, vel.X, vel.Y, 100, default, Main.rand.NextFloat(1.3f, 2f));
                dust.noGravity = Main.rand.NextBool();
            }
        }
        private void SpawnJumpTrail()
        {
            if (Main.rand.NextBool(2))
            {
                Dust trail = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Blood, -NPC.velocity.X * 0.2f, -NPC.velocity.Y * 0.2f, 120, default, 1.4f);
                trail.noGravity = true;
                trail.fadeIn = 1.1f;
            }
        }
        private void SpawnAmbientDust()
        {
            if (Main.rand.NextBool(12))
            {
                Dust ambient = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Blood, 0, -1f, 150, default, 0.8f);
                ambient.noGravity = true;
                ambient.velocity *= 0.4f;
            }
        }
        private void CreateAttackEffect(Player target)
        {
            Vector2 attackDir = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX * NPC.direction);
            for (int i = 0; i < 20; i++)
            {
                Vector2 dustVel = attackDir.RotatedByRandom(MathHelper.PiOver4 * 0.5f) * Main.rand.NextFloat(4f, 10f);
                Dust dust = Dust.NewDustDirect(NPC.Center + attackDir * 20, 0, 0, DustID.Blood, dustVel.X, dustVel.Y, 80, default, Main.rand.NextFloat(1.5f, 2.2f));
                dust.noGravity = true;
            }
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.Lerp(-MathHelper.PiOver4, MathHelper.PiOver4, i / 7f);
                Vector2 waveVel = attackDir.RotatedBy(angle) * 6f;
                Dust wave = Dust.NewDustDirect(NPC.Center, 0, 0, DustID.Smoke, waveVel.X, waveVel.Y, 100, new Color(200, 50, 50), 1.5f);
                wave.noGravity = true;
            }
        }
        #endregion
        public override void FindFrame(int frameHeight)
        {
            int frameSpeed = 5;
            switch ((State)AIState)
            {
                case State.Walking:
                    NPC.frameCounter++;
                    if (NPC.frameCounter >= frameSpeed)
                    {
                        NPC.frameCounter = 0;
                        NPC.frame.Y += frameHeight;
                        if (NPC.frame.Y >= frameHeight * 6)
                            NPC.frame.Y = 0;
                    }
                    break;
                case State.Jumping:
                    NPC.frame.Y = NPC.velocity.Y < 0 ? frameHeight * 6 : frameHeight * 7;
                    break;
                case State.Attacking:
                    int attackFrame = Math.Clamp((int)(Timer / 7), 0, 3);
                    NPC.frame.Y = frameHeight * (8 + attackFrame);
                    break;
            }
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Blood, hit.HitDirection * 3, -2f, 100, default, 1.6f);
                dust.velocity *= 1.5f;
            }
            if (NPC.life <= 0)
            {
                for (int i = 0; i < 50; i++)
                {
                    Dust death = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Blood, Main.rand.NextFloat(-7f, 7f), Main.rand.NextFloat(-7f, 7f), 100, default, Main.rand.NextFloat(1.5f, 2.5f));
                    death.noGravity = Main.rand.NextBool(3);
                }
                for (int i = 0; i < 8; i++)
                {
                    Gore.NewGore(NPC.GetSource_Death(), NPC.Center, new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-6f, 0f)), GoreID.Smoke1 + Main.rand.Next(3));
                }
                for (int i = 0; i < 25; i++)
                {
                    Dust smoke = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Smoke, Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f), 180, new Color(180, 50, 50), 2.2f);
                    smoke.noGravity = true;
                }
                SoundEngine.PlaySound(SoundID.NPCDeath6 with { Pitch = -0.3f }, NPC.Center);
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[NPC.type].Value;
            Vector2 origin = new(texture.Width / 2, NPC.frame.Height / 2);

            Vector2 drawPos = NPC.Center - screenPos;
            SpriteEffects effects = NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.15f + 0.85f;
            Color auraColor = new Color(255, 30, 30, 0) * 0.35f * pulse;

            for (int i = 0; i < 6; i++)
            {
                Vector2 offset = (MathHelper.TwoPi * i / 6f).ToRotationVector2() * (3f * pulse);
                spriteBatch.Draw(texture, drawPos + offset, NPC.frame, auraColor, NPC.rotation, origin, NPC.scale, effects, 0f);
            }

            Color tintedColor = new Color(Math.Min(drawColor.R + 60, 255), (int)(drawColor.G * 0.7f), (int)(drawColor.B * 0.7f), drawColor.A);
            spriteBatch.Draw(texture, drawPos, NPC.frame, tintedColor, NPC.rotation, origin, NPC.scale, effects, 0f);

            return false;
        }
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            float eyeGlow = (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.4f + 0.6f;

            Vector2 eyeOffset = new(NPC.spriteDirection == 1 ? 6 : -6, -12);
            Vector2 eyePos = NPC.Center + eyeOffset - screenPos;
            Texture2D pixel = TextureAssets.MagicPixel.Value;

            for (int layer = 2; layer >= 0; layer--)
            {
                float size = (3 - layer) * 3f * eyeGlow;
                Color color = new Color(255, 50 + layer * 30, 50 + layer * 30, 0) * (0.15f + layer * 0.15f);
                
                spriteBatch.Draw(pixel, eyePos, new Rectangle(0, 0, 1, 1), color, 0f, new Vector2(0.5f), size, SpriteEffects.None, 0f);
            }
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemID.Bone, 2, 2, 5));
            npcLoot.Add(ItemDropRule.Common(ItemID.Vertebrae, 3, 1, 2));
            npcLoot.Add(ItemDropRule.Common(ItemID.BloodMoonStarter, 40));
        }
    }
}
