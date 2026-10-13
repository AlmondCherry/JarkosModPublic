using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.npc.BloodMoon
{
    public class HemophageWraith : ModNPC
    {
        private ref float State => ref NPC.ai[0];
        private ref float Timer => ref NPC.ai[1];
        private ref float AttackCounter => ref NPC.ai[2];
        private const int State_Ram = 0;
        private const int State_Spew = 1;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 6;
            NPCID.Sets.TrailCacheLength[Type] = 6;
            NPCID.Sets.TrailingMode[Type] = 1;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Bleeding] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
        }
        public override void SetDefaults()
        {
            NPC.width = 94;
            NPC.height = 66;
            NPC.damage = 50;
            NPC.defense = 20;
            NPC.lifeMax = 200;
            NPC.HitSound = SoundID.NPCHit54;
            NPC.DeathSound = SoundID.NPCDeath52;
            NPC.value = Item.buyPrice(silver: 15);
            NPC.knockBackResist = 0.2f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.aiStyle = -1;
            NPC.alpha = 50;
        }
        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Events.BloodMoon, new FlavorTextBestiaryInfoElement("A manifestation of coagulated blood, this wraith stalks the night during Blood Moons, seeking fresh victims to drain.")
            });
        }
        public override void AI()
        {
            Lighting.AddLight(NPC.Center, 0.8f, 0.1f, 0.1f);

            NPC.TargetClosest(false);
            Player target = Main.player[NPC.target];

            if (!target.active || target.dead)
            {
                NPC.velocity.Y -= 0.4f;
                NPC.EncourageDespawn(10);
                if (NPC.timeLeft > 10) NPC.timeLeft = 10;
                if (NPC.position.Y < 0 || NPC.timeLeft <= 1)
                {
                    NPC.active = false;
                    NPC.netUpdate = true;
                }
                return;
            }

            Timer++;

            switch (State)
            {
                case State_Ram:
                    HandleRamState(target);
                    break;
                case State_Spew:
                    HandleSpewState(target);
                    break;
            }

            if (Main.rand.NextBool(4))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Blood, 0f, 0f, 100, default, 1.5f);
                dust.noGravity = true;
                dust.velocity *= 0.5f;
            }

            if (NPC.velocity.X > 0)
                NPC.spriteDirection = 1;
            else if (NPC.velocity.X < 0)
                NPC.spriteDirection = -1;
            NPC.rotation = NPC.velocity.X * 0.05f;
        }
        private void HandleRamState(Player target)
        {
            if (Timer < 60)
            {
                Vector2 targetPos = target.Center + new Vector2(NPC.direction * 150f, -100f);
                MoveTowards(targetPos, 8f, 20f);
            }

            else if (Timer == 60)
            {
                Vector2 dashDir = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
                NPC.velocity = dashDir * 16f;
                SoundEngine.PlaySound(SoundID.Roar with { Pitch = 0.5f, Volume = 0.8f }, NPC.Center);
            }

            else if (Timer > 60 && Timer < 100)
            {
                NPC.velocity *= 0.98f;
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, 0, 0, 100, default, 2f);
            }

            else if (Timer >= 120)
            {
                Timer = 0;
                AttackCounter++;
                if (AttackCounter >= 2)
                {
                    AttackCounter = 0;
                    State = State_Spew;
                }
            }
        }
        private void HandleSpewState(Player target)
        {
            Vector2 targetPos = target.Center + new Vector2((float)Math.Sin(Timer * 0.05f) * 150f, -200f);
            MoveTowards(targetPos, 6f, 15f);
            if (Timer > 30 && Timer < 150 && Timer % 8 == 0)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 shootVel = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * 8f;
                    shootVel = shootVel.RotatedByRandom(MathHelper.ToRadians(20));
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Bottom, shootVel, ModContent.ProjectileType<HemophageBloodDrop>(), 20, 2f, Main.myPlayer);
                }
                SoundEngine.PlaySound(SoundID.Item17, NPC.Center);
            }
            if (Timer >= 180)
            {
                Timer = 0;
                State = State_Ram;
            }
        }
        private void MoveTowards(Vector2 targetPos, float speed, float inertia)
        {
            Vector2 direction = targetPos - NPC.Center;
            float length = direction.Length();
            if (length > 0)
            {
                direction.Normalize();
            }
            NPC.velocity = (NPC.velocity * (inertia - 1) + direction * speed) / inertia;
        }
        public override void FindFrame(int frameHeight)
        {
            float speed = NPC.velocity.Length();
            int frameSpeed = (int)MathHelper.Clamp(8f - speed * 0.3f, 2f, 8f);
            NPC.frameCounter++;
            if (NPC.frameCounter >= frameSpeed)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;
                if (NPC.frame.Y >= frameHeight * 6)
                {
                    NPC.frame.Y = 0;
                }
            }
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, hit.HitDirection, -1f, 0, default, 1.5f);
            }
        }
        public override void OnKill()
        {
            for (int i = 0; i < 40; i++)
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Blood, Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f), 100, default, 2.5f);
                dust.noGravity = true;
            }
            SoundEngine.PlaySound(SoundID.NPCDeath52, NPC.Center);
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
                Color trailColor = drawColor * progress * 0.5f;
                Vector2 trailPos = NPC.oldPos[i] + NPC.Size / 2f - screenPos;
                spriteBatch.Draw(texture, trailPos, NPC.frame, trailColor, NPC.rotation, drawOrigin, NPC.scale, effects, 0f);
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.ZoomMatrix);

            float time = Main.GlobalTimeWrappedHourly;
            float pulseRadius = 15f + (float)Math.Sin(time * 3f) * 10f;
            int copies = 6;

            for(int i = 0; i < copies; i++)
            {
                float angle = time * 4f + MathHelper.TwoPi * i / copies;
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * pulseRadius;
                Color auraColor = new Color(255, 0, 0, 0) * 0.6f;
                spriteBatch.Draw(texture, drawPos + offset, NPC.frame, auraColor, NPC.rotation, drawOrigin, NPC.scale, effects, 0f);
            }
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.ZoomMatrix);
            spriteBatch.Draw(texture, drawPos, NPC.frame, drawColor, NPC.rotation, drawOrigin, NPC.scale, effects, 0f);
            return false;
        }
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (Main.bloodMoon && spawnInfo.SpawnTileY < Main.worldSurface)
            {
                return 0.0095f;
            }
            return 0f;
        }
    }
    public class HemophageBloodDrop : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.BloodRain;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 4;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
        }
        public override void AI()
        {
            Projectile.velocity.Y += 0.2f;
            if (Projectile.velocity.Y > 12f) Projectile.velocity.Y = 12f;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Lighting.AddLight(Projectile.Center, 0.5f, 0.1f, 0f);
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, 0f, 0f, 100, default, 1.2f);
                dust.noGravity = true;
                Dust fireDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 1.2f);
                fireDust.noGravity = true;
            }
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.NPCDeath19, Projectile.Center);
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-3, 3), 100, default, 1.5f);
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-2, 2), 100, default, 1.2f);
            }
        }
        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            target.AddBuff(BuffID.Bleeding, 300);
            target.AddBuff(BuffID.OnFire, 180);
        }
    }
}
