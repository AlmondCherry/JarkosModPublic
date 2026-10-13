using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using jarkos.cradle_biome;
using jarkos.projectiles.enemy;
using System;
using Jarkos.items.materials;
using jarkos.items.materials;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.CameraModifiers;

namespace jarkos.npc.CradleMobs
{
    public class Rhinobeetle : ModNPC
    {
        public ref float AI_State => ref NPC.ai[0];
        public ref float AI_Timer => ref NPC.ai[1];
        private const int State_Sneak = 0;
        private const int State_Roll = 1;
        private const int State_Ram = 2;
        private const int State_Spit = 3;
        private const int State_Despawn = 4;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 6;
        }
        public override void SetDefaults()
        {
            NPC.width = 168;
            NPC.height = 90;
            NPC.damage = 50;
            NPC.defense = 5;
            NPC.lifeMax = 5000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = 10000f;
            NPC.knockBackResist = 0f;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.boss = false;
            NPC.rarity = 3;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<PetrifiedAbyssBiome>().Type };
        }
        public override void AI()
        {
            NPC.TargetClosest(true);
            Player player = Main.player[NPC.target];
            if (!player.active || player.dead || !player.InModBiome<PetrifiedAbyssBiome>())
            {
                NPC.TargetClosest(false);
                player = Main.player[NPC.target];
                if (!player.active || player.dead || !player.InModBiome<PetrifiedAbyssBiome>())
                {
                    AI_State = State_Despawn;
                }
            }
            if (AI_State == State_Despawn)
            {
                NPC.velocity.Y += 0.5f;
                NPC.noTileCollide = true;
                NPC.EncourageDespawn(10);
                if (NPC.timeLeft > 10) NPC.timeLeft = 10;
                if (NPC.position.Y > Main.maxTilesY * 16 || NPC.timeLeft <= 1)
                {
                    NPC.active = false;
                    NPC.netUpdate = true;
                }
                return;
            }
            NPC.defense = (AI_State == State_Roll) ? 50 : 5;
            if (AI_State == State_Sneak)
            {
                AI_Timer++;
                float speed = 1.5f;
                int dir = (player.Center.X > NPC.Center.X) ? 1 : -1;
                NPC.velocity.X = dir * speed;
                NPC.spriteDirection = -dir;
                if (AI_Timer >= 180)
                {
                    AI_Timer = 0;
                    int nextAttack = Main.rand.Next(3);
                    if (nextAttack == 0) AI_State = State_Roll;
                    else if (nextAttack == 1) AI_State = State_Ram;
                    else AI_State = State_Spit;
                }
            }
            else if (AI_State == State_Roll)
            {
                AI_Timer++;
                int dirToPlayer = (player.Center.X > NPC.Center.X) ? 1 : -1;
                float rollSpeed = 12f;
                NPC.noTileCollide = true;
                if (AI_Timer == 1)
                {
                    NPC.velocity.X = dirToPlayer * rollSpeed;
                    NPC.velocity.Y = -4f;
                }
                if (NPC.velocity.Y > 0)
                {
                    bool hitFloor = false;
                    float nextY = NPC.position.Y + NPC.height + NPC.velocity.Y;
                    int tileY = (int)(nextY / 16f);
                    int tileXLeft = (int)(NPC.position.X / 16f);
                    int tileXRight = (int)((NPC.position.X + NPC.width) / 16f);
                    int tileXCenter = (int)(NPC.Center.X / 16f);
                    if (WorldGen.InWorld(tileXCenter, tileY))
                    {
                        if (WorldGen.SolidTile(tileXLeft, tileY) || WorldGen.SolidTile(tileXRight, tileY) || WorldGen.SolidTile(tileXCenter, tileY))
                        {
                            hitFloor = true;
                        }
                    }
                    if (hitFloor)
                    {
                        if (NPC.velocity.Y > 5f)
                        {
                            PunchCameraModifier modifier = new PunchCameraModifier(NPC.Center, new Vector2(0, 1), 10f, 6f, 20, 1000f, FullName);
                            Main.instance.CameraModifiers.Add(modifier);
                            SoundEngine.PlaySound(SoundID.Item14, NPC.position);
                        }
                        float bounceY = -NPC.velocity.Y * 0.9f;
                        if (player.Center.Y < NPC.Center.Y - 100 && bounceY > -12f)
                        {
                            bounceY = -14f;
                        }
                        if (bounceY > -6f) bounceY = -6f;
                        NPC.velocity.Y = bounceY;
                        NPC.velocity.X = dirToPlayer * rollSpeed;
                    }
                }
                if (AI_Timer > 10 && AI_Timer < 710)
                {
                    NPC.rotation += NPC.velocity.X * 0.05f;
                }
                else
                {
                    NPC.rotation = 0f;
                }
                if (AI_Timer >= 720)
                {
                    AI_Timer = 0;
                    AI_State = State_Sneak;
                    NPC.noTileCollide = false;
                }
            }
            else if (AI_State == State_Ram)
            {
                AI_Timer++;
                if (AI_Timer < 60)
                {
                    NPC.velocity.X *= 0.9f;
                    NPC.spriteDirection = (player.Center.X > NPC.Center.X) ? -1 : 1;
                }
                else if (AI_Timer == 60)
                {
                    float dashSpeed = 16f;
                    Vector2 dashDir = player.Center - NPC.Center;
                    dashDir.Normalize();
                    NPC.velocity.X = dashDir.X * dashSpeed;
                    NPC.velocity.Y = -2f;
                }
                else
                {
                    if (NPC.collideX)
                    {
                        NPC.velocity.X = 0;
                        AI_Timer = 0;
                        AI_State = State_Sneak;
                        SoundEngine.PlaySound(SoundID.Item14, NPC.position);
                    }
                    if (AI_Timer >= 120)
                    {
                        AI_Timer = 0;
                        AI_State = State_Sneak;
                    }
                }
            }
            else if (AI_State == State_Spit)
            {
                AI_Timer++;
                NPC.velocity.X *= 0.9f;
                NPC.spriteDirection = (player.Center.X > NPC.Center.X) ? -1 : 1;
                if (AI_Timer == 60)
                {
                    SoundEngine.PlaySound(SoundID.Item17, NPC.position);
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Vector2 shootDir = player.Center - NPC.Center;
                        shootDir.Normalize();
                        int type = ModContent.ProjectileType<Rhinobeetle_thread>();
                        int damage = NPC.damage / 3;
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootDir, type, damage, 0f, Main.myPlayer, 0f, 1f);
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootDir, type, damage, 0f, Main.myPlayer, 0f, -1f);
                    }
                }
                if (AI_Timer >= 90)
                {
                    AI_Timer = 0;
                    AI_State = State_Sneak;
                }
            }
            if (AI_State != State_Roll)
            {
                NPC.rotation = 0f;
            }
        }
        public override void FindFrame(int frameHeight)
        {
            if (AI_State == State_Roll)
            {
                if (AI_Timer <= 10 || AI_Timer >= 710)
                {
                    NPC.frame.Y = 4 * frameHeight;
                }
                else
                {
                    NPC.frame.Y = 5 * frameHeight;
                }
                return;
            }
            float speed = Math.Abs(NPC.velocity.X);
            if (speed < 0.1f)
            {
                NPC.frameCounter = 0;
            }
            else
            {
                NPC.frameCounter += speed * 0.08f;
                if (NPC.frameCounter >= 4.0)
                {
                    NPC.frameCounter = 0.0;
                }
            }
            NPC.frame.Y = (int)NPC.frameCounter * frameHeight;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (AI_State == State_Roll)
            {
                Texture2D texture = TextureAssets.Npc[NPC.type].Value;
                Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, NPC.height * 0.5f);
                SpriteEffects effects = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                for (int i = 0; i < 4; i++)
                {
                    Vector2 offset = new Vector2(8, 0).RotatedBy(Main.GlobalTimeWrappedHourly * 5f + MathHelper.PiOver2 * i);
                    spriteBatch.Draw(texture, NPC.Center - screenPos + offset, NPC.frame, Color.OrangeRed * 0.6f, NPC.rotation, drawOrigin, NPC.scale, effects, 0f);
                }
            }
            return true;
        }
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ChitinBeetle>(), 1, 2, 6));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<DeliciousBait>(), 4, 1, 1));
            npcLoot.Add(ItemDropRule.Common(ItemID.GoldCoin, 1, 5, 15));
        }
    }
}
