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
    public class BigArmed : ModNPC
    {
        private ref float JumpCooldown => ref NPC.ai[0];
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 4;
            NPCID.Sets.TrailCacheLength[Type] = 8;
            NPCID.Sets.TrailingMode[Type] = 1;
            NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new()
            {
                Velocity = 1f
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
        }
        public override void SetDefaults()
        {
            NPC.width = 66;
            NPC.height = 58;
            NPC.damage = 60;
            NPC.defense = 28;
            NPC.lifeMax = 300;
            NPC.HitSound = SoundID.NPCHit18;
            NPC.DeathSound = SoundID.NPCDeath21;
            NPC.knockBackResist = 0.2f;
            NPC.value = Item.buyPrice(silver: 45);
            NPC.aiStyle = -1;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.lavaImmune = true;

            NPC.buffImmune[BuffID.Poisoned] = true;
            NPC.buffImmune[BuffID.OnFire] = true;
            NPC.buffImmune[BuffID.Bleeding] = true;
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Events.BloodMoon, new FlavorTextBestiaryInfoElement("An elite harvester of blood. It hunts relentlessly on the ground, jumping over obstacles and bursting into toxic boiling blood upon death.")
            });
        }
        public override void AI()
        {
            NPC.TargetClosest(true);
            Player target = Main.player[NPC.target];
            if (!target.active || target.dead)
            {
                NPC.velocity.X *= 0.9f;
                return;
            }

            NPC.spriteDirection = target.Center.X > NPC.Center.X ? 1 : -1;

            float distanceX = target.Center.X - NPC.Center.X;
            float distanceY = target.Center.Y - NPC.Center.Y;
            float maxSpeed = 7f;
            float acceleration = 0.2f;
            float friction = 0.15f;

            if (Math.Sign(distanceX) > 0)
            {
                if (NPC.velocity.X < maxSpeed)
                    NPC.velocity.X += acceleration;
                else
                    NPC.velocity.X *= 0.98f;
            }
            else
            {
                if (NPC.velocity.X > -maxSpeed)
                    NPC.velocity.X -= acceleration;
                else
                    NPC.velocity.X *= 0.98f;
            }

            if (Math.Sign(NPC.velocity.X) != Math.Sign(distanceX) && Math.Abs(NPC.velocity.X) > 0.5f)
            {
                NPC.velocity.X *= (1f - friction);
            }

            if (NPC.collideX && NPC.velocity.Y == 0f)
            {
                NPC.velocity.Y = -6.5f;
            }

            if (JumpCooldown > 0)
            {
                JumpCooldown--;
            }

            bool shouldJump = Math.Abs(distanceX) > 250f || distanceY < -100f;

            if (shouldJump && NPC.velocity.Y == 0f && JumpCooldown == 0f)
            {
                NPC.velocity.Y = -12.5f;
                NPC.velocity.X = Math.Sign(distanceX) * 9.5f;
                JumpCooldown = 80;
                SoundEngine.PlaySound(SoundID.DoubleJump, NPC.Center);
            }

            Lighting.AddLight(NPC.Center, 0.6f, 0.1f, 0.15f);

            if (Main.rand.NextBool(8))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Blood, 0f, 0f, 100, default, 1.3f);
                dust.velocity.X *= 0.5f;
                dust.velocity.Y *= -0.5f;
            }
        }
        public override void FindFrame(int frameHeight)
        {
            if (NPC.velocity.Y != 0f)
            {
                NPC.frame.Y = frameHeight * 1;
                NPC.frameCounter = 0;
            }
            else if (Math.Abs(NPC.velocity.X) < 0.2f)
            {
                NPC.frame.Y = 0;
                NPC.frameCounter = 0;
            }
            else
            {
                float speedScale = Math.Abs(NPC.velocity.X) * 0.18f;
                NPC.frameCounter += speedScale;
                if (NPC.frameCounter >= 6.0)
                {
                    NPC.frameCounter = 0;
                    NPC.frame.Y += frameHeight;
                    if (NPC.frame.Y >= frameHeight * 4)
                    {
                        NPC.frame.Y = 0;
                    }
                }
            }
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int i = 0; i < 15; i++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, hit.HitDirection * 1.5f, -1f, 0, default, 1.2f);
            }
        }
        public override void OnKill()
        {
            SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
            SoundEngine.PlaySound(SoundID.NPCDeath21, NPC.Center);
            
            for (int i = 0; i < 40; i++)
            {
                Vector2 speed = Main.rand.NextVector2Circular(6f, 6f);
                Dust d = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Blood, speed.X, speed.Y, 100, default, 2f);
                d.noGravity = Main.rand.NextBool();
                Vector2 speed2 = Main.rand.NextVector2Circular(5f, 5f);
                Dust d2 = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Torch, speed2.X, speed2.Y, 100, default, 1.5f);
                d2.noGravity = true;
            }
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int dropCount = Main.rand.Next(15, 25);
                for (int i = 0; i < dropCount; i++)
                {
                    Vector2 shootVelocity = Main.rand.NextVector2Circular(5f, 5f);
                    shootVelocity.Y = -Math.Abs(shootVelocity.Y) - Main.rand.NextFloat(1f, 4f);
                    shootVelocity.X *= 1.5f;
                    Projectile.NewProjectile(NPC.GetSource_Death(), NPC.Center, shootVelocity, ModContent.ProjectileType<EliteHarvesterBloodDrop>(), 25, 3f, Main.myPlayer);
                }
            }
        }
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (Main.bloodMoon && !spawnInfo.Player.ZoneDungeon)
                return 0.08f;
            return 0f;
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ItemID.BloodMoonStarter, 10));
            npcLoot.Add(ItemDropRule.Common(ItemID.MoneyTrough, 15));
            npcLoot.Add(ItemDropRule.Common(ItemID.SharkToothNecklace, 12));
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[Type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width / 2, NPC.frame.Height / 2);
            Vector2 drawPos = NPC.Center - screenPos;
            SpriteEffects effects = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            for (int i = 0; i < NPC.oldPos.Length; i++)
            {
                if (NPC.oldPos[i] == Vector2.Zero) continue;
                float progress = 1f - (float)i / NPC.oldPos.Length;
                Color trailColor = drawColor * progress * 0.4f;
                Vector2 trailPos = NPC.oldPos[i] + NPC.Size / 2f - screenPos;
                spriteBatch.Draw(texture, trailPos, NPC.frame, trailColor, NPC.rotation, drawOrigin, NPC.scale, effects, 0f);
            }
            spriteBatch.Draw(texture, drawPos, NPC.frame, drawColor, NPC.rotation, drawOrigin, NPC.scale, effects, 0f);
            return false;
        }
    }
    public class EliteHarvesterBloodDrop : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.BloodRain;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 4;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 360;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
        }
        public override void AI()
        {
            Projectile.velocity.Y += 0.22f;
            if (Projectile.velocity.Y > 12f) Projectile.velocity.Y = 12f;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Lighting.AddLight(Projectile.Center, 0.4f, 0.05f, 0.05f);
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, 0f, 0f, 100, default, 1.1f);
                dust.noGravity = true;
                dust.velocity *= 0.2f;
            }
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.NPCDeath19, Projectile.Center);
            for (int i = 0; i < 8; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f, 100, default, 1.2f);
            }
        }
        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            target.AddBuff(BuffID.OnFire, 1800);
            target.AddBuff(BuffID.Bleeding, 3600);
        }
    }
}
