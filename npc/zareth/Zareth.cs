using Microsoft.Xna.Framework;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.npc.zareth
{
    [AutoloadBossHead]
    public class Zareth : ModNPC
    {
        private enum State
        {
            Spawn = 0,
            Fly = 1,
            Clap = 2,
            StoneArms = 3,
            ThrowLeft = 4,
            ThrowRight = 5,
            SlamRise = 6,
            SlamFall = 7,
            SlamLand = 8,
            Death = 9
        }
        private State CurrentState { get => (State)NPC.ai[0]; set => NPC.ai[0] = (float)value; }
        private ref float Timer => ref NPC.ai[1];
        private ref float LastAttack => ref NPC.ai[2];
        private ref float ChainFlag => ref NPC.localAI[0];
        public const float ClapRadius = 240f;
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 24;
        }
        public override void SetDefaults()
        {
            NPC.width = 96;
            NPC.height = 90;
            NPC.damage = 42;
            NPC.defense = 20;
            NPC.lifeMax = 3200;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.aiStyle = -1;
            NPC.HitSound = SoundID.NPCHit41;
            NPC.DeathSound = null;
            NPC.value = Item.buyPrice(gold: 3);
            NPC.rarity = 2;
        }
        public override bool CheckActive()
        {
            return false;
        }
        
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(ChainFlag);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            ChainFlag = reader.ReadSingle();
        }

        public override void OnSpawn(IEntitySource source)
        {
            CurrentState = State.Spawn;
            NPC.alpha = 255;
            NPC.scale = 0.3f;
            NPC.dontTakeDamage = true;
        }
        public override bool CheckDead()
        {
            if (CurrentState != State.Death)
            {
                NPC.life = 1;
                NPC.dontTakeDamage = true;
                NPC.velocity = Vector2.Zero;
                CurrentState = State.Death;
                Timer = 0f;
                NPC.netUpdate = true;
                return false;
            }
            return true;
        }
        public override void AI()
        {
            if (CurrentState == State.Spawn) { AI_Spawn(); return; }
            if (CurrentState == State.Death) { AI_Death(); return; }

            Player player = Main.player[NPC.target];

            if (NPC.target < 0 || NPC.target == 255 || player.dead || !player.active)
            {
                NPC.TargetClosest();
                NPC.TargetClosest(false);
                player = Main.player[NPC.target];
                
                if (!player.active || player.dead)
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
            
            NPC.timeLeft = NPC.activeTime * 30;

            if (CurrentState != State.SlamFall)
                NPC.spriteDirection = NPC.direction = (player.Center.X > NPC.Center.X) ? 1 : -1;
            Timer++;

            switch (CurrentState)
            {
                case State.Fly: 
                    AI_Fly(player);
                    break;
                case State.Clap:   
                    AI_Clap(player);
                    break;
                case State.StoneArms: 
                    AI_StoneArms(player);
                    break;
                case State.ThrowLeft: 
                    AI_Throw(player, left: true);
                    break;
                case State.ThrowRight: 
                    AI_Throw(player, left: false);
                    break;
                case State.SlamRise: 
                    AI_SlamRise(player);
                    break;
                case State.SlamFall: 
                    AI_SlamFall(player);
                    break;
                case State.SlamLand: 
                    AI_SlamLand(player);
                    break;
            }

            float targetRot;
            if (CurrentState == State.SlamFall)
                targetRot = 0f;
            else
                targetRot = MathHelper.Clamp(NPC.velocity.X * 0.05f, -0.35f, 0.35f);
            NPC.rotation = MathHelper.Lerp(NPC.rotation, targetRot, 0.12f);

            if (Main.rand.NextBool(10))
            {
                Dust d = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Stone, 0f, 0f, 100, default, 0.8f);
                d.velocity *= 0.3f;
                d.noGravity = true;
            }
            if (Main.rand.NextBool(25))
            {
                Dust d = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.YellowTorch, 0f, -0.5f, 150, default, 0.9f);
                d.noGravity = true;
            }
            Lighting.AddLight(NPC.Center, 0.35f, 0.3f, 0.2f);
        }
        private void AI_Spawn()
        {
            Timer++;
            NPC.velocity = Vector2.Zero;
            float progress = MathHelper.Clamp(Timer / 100f, 0f, 1f);

            for (int i = 0; i < 5; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi) + Timer * 0.1f;
                float dist = Main.rand.NextFloat(160f, 280f) * (1f - progress);

                Vector2 pos = NPC.Center + angle.ToRotationVector2() * dist;
                Vector2 toCenter = (NPC.Center - pos) * 0.1f;
                Vector2 tangent = toCenter.RotatedBy(MathHelper.PiOver2) * 0.5f;

                Dust d = Dust.NewDustPerfect(pos, DustID.Stone, toCenter + tangent, 0, default, Main.rand.NextFloat(1.2f, 2.2f));
                d.noGravity = true;
            }
            if (Timer % 25 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item44 with { Pitch = -0.8f + progress }, NPC.Center);
                for (int i = 0; i < 24; i++)
                {
                    Vector2 vel = (MathHelper.TwoPi / 24f * i).ToRotationVector2() * 3f;
                    Dust d = Dust.NewDustPerfect(NPC.Center, DustID.Sandnado, -vel * 2f, 100, default, 1.3f);
                    d.noGravity = true;
                }
            }

            NPC.alpha = (int)(255f * (1f - progress));
            NPC.scale = 0.3f + 0.7f * progress + (float)Math.Sin(Timer * 0.3f) * 0.03f * (1f - progress);
            Lighting.AddLight(NPC.Center, 0.6f * progress, 0.5f * progress, 0.35f * progress);

            if (Timer >= 100f)
            {
                SoundEngine.PlaySound(SoundID.NPCDeath43 with { Volume = 0.7f, Pitch = -0.3f }, NPC.Center);
                if (Main.netMode != NetmodeID.Server)
                    ZarethEffectsSystem.TriggerShockwave(NPC.Center, 0.6f);

                ShakeScreen(NPC.Center, 8f, 25);
                for (int i = 0; i < 40; i++)
                {
                    Vector2 vel = (MathHelper.TwoPi / 40f * i).ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                    Dust d = Dust.NewDustPerfect(NPC.Center, DustID.Stone, vel, 0, default, 1.8f);
                    d.noGravity = true;
                }
                NPC.alpha = 0;
                NPC.scale = 1f;
                NPC.dontTakeDamage = false;
                Timer = 0f;
                CurrentState = State.Fly;
                NPC.netUpdate = true;
            }
        }
        private void AI_Death()
        {
            Timer++;
            NPC.velocity = Vector2.Zero;
            float progress = Timer / 150f;

            NPC.position += Main.rand.NextVector2Circular(progress * 5f, progress * 5f);
            NPC.rotation = Main.rand.NextFloat(-0.08f, 0.08f) * progress;

            if (Main.rand.NextFloat() < progress)
            {
                Dust d = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Stone, 0f, 2f, 0, default, 1.6f);
                d.velocity *= 1.3f;
            }
            if (Main.rand.NextFloat() < progress * 0.7f)
            {
                Dust d = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Torch, 0f, 0f, 100, default, 1.4f);
                d.noGravity = true;
            }
            Lighting.AddLight(NPC.Center, 1f * progress, 0.6f * progress, 0.3f * progress);

            if (Timer % 15 == 0)
                SoundEngine.PlaySound(SoundID.NPCHit41 with { Pitch = -0.9f + progress * 1.2f }, NPC.Center);

            if (Timer == 90f && Main.netMode != NetmodeID.MultiplayerClient)
            {
                SoundEngine.PlaySound(SoundID.Item74, NPC.Center);
                for (int i = 0; i < 20; i++)
                {
                    float angle = MathHelper.TwoPi / 20f * i;
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + angle.ToRotationVector2() * 90f, Vector2.Zero, ModContent.ProjectileType<ZarethDeathStone>(), 0, 0f, Main.myPlayer, angle, NPC.whoAmI);
                }
            }
            if (Timer >= 150f)
            {
                SoundEngine.PlaySound(SoundID.NPCDeath43, NPC.Center);
                SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.5f }, NPC.Center);

                ShakeScreen(NPC.Center, 18f, 50);
                if (Main.netMode != NetmodeID.Server)
                    ZarethEffectsSystem.TriggerShockwave(NPC.Center, 1f);

                for (int ring = 0; ring < 2; ring++)
                {
                    float speed = ring == 0 ? 12f : 6f;
                    for (int i = 0; i < 50; i++)
                    {
                        Vector2 vel = (MathHelper.TwoPi / 50f * i).ToRotationVector2() * speed * Main.rand.NextFloat(0.8f, 1.2f);
                        Dust d = Dust.NewDustPerfect(NPC.Center, ring == 0 ? DustID.Stone : DustID.Smoke, vel, 100, default, 2f);
                        d.noGravity = ring == 0;
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
        private void AI_Fly(Player player)
        {
            Vector2 hoverPos = player.Center + new Vector2(NPC.direction * -180f, -160f);
            Vector2 toHover = hoverPos - NPC.Center;

            if (toHover.Length() > 20f)
            {
                toHover.Normalize();
                NPC.velocity = (NPC.velocity * 24f + toHover * 7f) / 25f;
            }
            if (Timer >= 90f && Main.netMode != NetmodeID.MultiplayerClient)
            {
                Timer = 0f;
                ChainFlag = 0f;
                int pick;

                do
                { 
                    pick = Main.rand.Next(4);
                }
                while (pick == (int)LastAttack);
                LastAttack = pick;

                switch (pick)
                {
                    case 0:
                        CurrentState = State.Clap; 
                        break;
                    case 1:
                        CurrentState = State.StoneArms; 
                        break;
                    case 2:
                        CurrentState = State.SlamRise; 
                        break;
                    case 3:
                        CurrentState = Main.rand.NextBool() ? State.ThrowLeft : State.ThrowRight;
                        break;
                }

                NPC.netUpdate = true;
            }
        }
        private void AI_Clap(Player player)
        {
            if (Timer < 24f)
                NPC.velocity = (player.Center - NPC.Center).SafeNormalize(Vector2.UnitX) * 11f;
            else
                NPC.velocity *= 0.85f;
            if (Timer == 42f)
            {
                SoundEngine.PlaySound(SoundID.Item70 with { Pitch = -0.5f }, NPC.Center);
                SoundEngine.PlaySound(SoundID.DD2_KoboldExplosion, NPC.Center);

                ShakeScreen(NPC.Center, 14f, 35);

                if (Main.netMode != NetmodeID.Server)
                    ZarethEffectsSystem.TriggerShockwave(NPC.Center);

                for (int i = 0; i < 50; i++)
                {
                    Vector2 vel = Vector2.UnitX.RotatedBy(MathHelper.TwoPi / 50f * i) * Main.rand.NextFloat(6f, 10f);
                    Dust d = Dust.NewDustPerfect(NPC.Center, DustID.Stone, vel, 100, default, 1.6f);
                    d.noGravity = true;
                }
                foreach (Player p in Main.ActivePlayers)
                {
                    if (Vector2.Distance(p.Center, NPC.Center) <= ClapRadius && !p.dead)
                    {
                        p.AddBuff(ModContent.BuffType<ZarethShockDebuff>(), 240);

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                            p.Hurt(PlayerDeathReason.ByNPC(NPC.whoAmI), 50, 0);
                            
                        p.velocity += (p.Center - NPC.Center).SafeNormalize(Vector2.UnitY) * 9f;
                        if (p.whoAmI == Main.myPlayer)
                            ZarethEffectsSystem.TriggerShock(1f);
                    }
                }
            }
            if (Timer >= 72f)
            {
                Timer = 0f;
                CurrentState = State.Fly;
                NPC.netUpdate = true;
            }
        }
        private void AI_StoneArms(Player player)
        {
            NPC.velocity *= 0.95f;
            float intensity = Timer / 108f;
            if (Main.rand.NextFloat() < 0.3f + intensity * 0.5f)
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    Dust d = Dust.NewDustDirect(NPC.Center + new Vector2(side * 34f - 8f, -10f), 16, 24, DustID.Stone, 0f, 0f, 120, default, 1f + intensity);
                    d.velocity *= 0.2f;
                    d.noGravity = true;
                }
            }
            if (Timer == 24f || Timer == 60f || Timer == 96f)
            {
                SoundEngine.PlaySound(SoundID.NPCHit41 with { Pitch = 0.2f + Timer / 200f }, NPC.Center);
                ShakeScreen(NPC.Center, 3f, 10);
            }
            if (Timer >= 108f)
            {
                Timer = 0f;
                ChainFlag = 1f;
                CurrentState = State.ThrowLeft;
                NPC.netUpdate = true;
            }
        }
        private void AI_Throw(Player player, bool left)
        {
            NPC.velocity *= 0.92f;
            float handSide = left ? -NPC.spriteDirection : NPC.spriteDirection;
            Vector2 handPos = NPC.Center + new Vector2(handSide * 40f, -10f);
            if (Timer < 30f)
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 offset = Main.rand.NextVector2CircularEdge(50f, 50f) * (1f - Timer / 30f);
                    Dust d = Dust.NewDustPerfect(handPos + offset, DustID.Stone, -offset * 0.15f, 100, default, 1.3f);
                    d.noGravity = true;
                }
                Lighting.AddLight(handPos, 0.4f, 0.35f, 0.25f);
            }
            if (Timer == 30f)
            {
                SoundEngine.PlaySound(SoundID.Item51, NPC.Center);
                for (int i = 0; i < 14; i++)
                {
                    Dust d = Dust.NewDustPerfect(handPos, DustID.Stone, Main.rand.NextVector2Circular(4f, 4f), 0, default, 1.5f);
                    d.noGravity = true;
                }
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 toPlayer = (player.Center - handPos).SafeNormalize(Vector2.UnitX);
                    for (int i = -2; i <= 2; i++)
                    {
                        Vector2 vel = toPlayer.RotatedBy(MathHelper.ToRadians(9f * i)) * Main.rand.NextFloat(9f, 11f);
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), handPos, vel, ModContent.ProjectileType<ZarethProjOne>(), 28, 1.5f, Main.myPlayer);
                    }
                }
            }
            if (Timer >= 48f)
            {
                Timer = 0f;
                if (left && ChainFlag == 1f)
                    CurrentState = State.ThrowRight;
                else
                {
                    ChainFlag = 0f;
                    CurrentState = State.Fly;
                }
                NPC.netUpdate = true;
            }
        }
        private void AI_SlamRise(Player player)
        {
            Vector2 target = player.Center + new Vector2(0f, -320f);
            Vector2 toTarget = target - NPC.Center;
            NPC.velocity = (NPC.velocity * 14f + toTarget.SafeNormalize(Vector2.Zero) * 13f) / 15f;

            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustDirect(NPC.BottomLeft, NPC.width, 8, DustID.Stone, 0f, 2f, 100, default, 1.2f);
                d.noGravity = true;
            }

            if (Timer >= 45f || toTarget.Length() < 40f)
            {
                Timer = 0f;
                NPC.velocity = Vector2.Zero;
                CurrentState = State.SlamFall;

                SoundEngine.PlaySound(SoundID.Item74, NPC.Center);
                NPC.netUpdate = true;
            }
        }
        private void AI_SlamFall(Player player)
        {
            NPC.velocity.X *= 0.9f;
            NPC.velocity.Y = Math.Min(NPC.velocity.Y + 1.2f, 26f);

            bool hitGround = false;

            int tileXStart = (int)(NPC.position.X / 16f);
            int tileXEnd = (int)((NPC.position.X + NPC.width) / 16f);
            int tileY = (int)((NPC.Bottom.Y + NPC.velocity.Y) / 16f);

            for (int x = tileXStart; x <= tileXEnd; x++)
            {
                if (WorldGen.SolidTile(x, tileY))
                {
                    hitGround = true;
                    NPC.Bottom = new Vector2(NPC.Bottom.X, tileY * 16f);
                    break;
                }
            }
            if (hitGround)
            {
                NPC.velocity = Vector2.Zero;
                Timer = 0f;

                CurrentState = State.SlamLand;
                NPC.netUpdate = true;

                SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
                SoundEngine.PlaySound(SoundID.NPCDeath43 with { Volume = 0.6f, Pitch = -0.6f }, NPC.Center);

                ShakeScreen(NPC.Center, 20f, 45);

                if (Main.netMode != NetmodeID.Server)
                    ZarethEffectsSystem.TriggerShockwave(NPC.Bottom, 0.8f);
                for (int i = 0; i < 40; i++)
                {
                    Dust d = Dust.NewDustDirect(NPC.BottomLeft - new Vector2(20f, 10f), NPC.width + 40, 10, DustID.Stone, 0f, -4f, 0, default, Main.rand.NextFloat(1.5f, 2.2f));
                    d.velocity.X *= 2f;
                }
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (int dir = -1; dir <= 1; dir += 2)
                        for (int i = 1; i <= 8; i++)
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Bottom + new Vector2(dir * i * 44f, -8f), Vector2.Zero, ModContent.ProjectileType<ZarethProjTwo>(), 32, 3f, Main.myPlayer, i, dir);
                }
            }
            if (Timer >= 180f)
            {
                Timer = 0f;
                CurrentState = State.Fly;
                NPC.netUpdate = true;
            }
        }
        private void AI_SlamLand(Player player)
        {
            NPC.velocity = Vector2.Zero;
            
            if (Timer >= 60f)
            {
                Timer = 0f;
                CurrentState = State.Fly;
                NPC.netUpdate = true;
            }
        }
        private static void ShakeScreen(Vector2 position, float strength, int frames)
        {
            if (Main.netMode == NetmodeID.Server)
                return;
            Main.instance.CameraModifiers.Add(new PunchCameraModifier(position, Main.rand.NextVector2Unit(), strength, 8f, frames, 3000f, "Zareth"));
        }
        public override void FindFrame(int frameHeight)
        {
            int frameSpeed = 12;
            switch (CurrentState)
            {
                case State.Spawn:
                case State.Fly:
                case State.SlamRise:
                case State.SlamLand:
                    NPC.frame.Y = 0;
                    break;
                case State.SlamFall:
                    NPC.frame.Y = 3 * frameHeight;
                    break;
                case State.Clap:
                    NPC.frame.Y = (1 + Math.Min((int)(Timer / frameSpeed), 5)) * frameHeight;
                    break;
                case State.StoneArms:
                    NPC.frame.Y = (7 + Math.Min((int)(Timer / frameSpeed), 8)) * frameHeight;
                    break;
                case State.ThrowLeft:
                    NPC.frame.Y = (16 + Math.Min((int)(Timer / frameSpeed), 3)) * frameHeight;
                    break;
                case State.ThrowRight:
                    NPC.frame.Y = (20 + Math.Min((int)(Timer / frameSpeed), 3)) * frameHeight;
                    break;
                case State.Death:
                    NPC.frame.Y = 0;
                    break;
            }
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int i = 0; i < 5; i++)
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Stone, hit.HitDirection, -1f);
        }
    }
}
