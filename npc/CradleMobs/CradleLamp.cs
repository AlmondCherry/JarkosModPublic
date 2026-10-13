using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.npc.CradleMobs
{
    public class CradleLamp : ModNPC
    {
        private ref float State => ref NPC.ai[0];
        private ref float Timer => ref NPC.ai[1];
        private ref float AttackCounter => ref NPC.ai[2];

        private const int State_Hovering1 = 0;
        private const int State_Fireballs = 1;
        private const int State_Hovering2 = 2;
        private const int State_PollenRain = 3;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 4;
            NPCID.Sets.TrailCacheLength[Type] = 6;
            NPCID.Sets.TrailingMode[Type] = 1;
        }
        public override void SetDefaults()
        {
            NPC.width = 30;
            NPC.height = 40;
            NPC.damage = 30;
            NPC.defense = 5;
            NPC.lifeMax = 210;
            NPC.HitSound = new SoundStyle("Terraria/Sounds/Shatter") { PitchVariance = 0.5f };
            NPC.DeathSound = SoundID.NPCDeath14;
            NPC.value = Item.buyPrice(silver: 75);
            NPC.knockBackResist = 0.3f;
            NPC.noGravity = true;
            NPC.noTileCollide = false;
            NPC.lavaImmune = true;
            NPC.aiStyle = -1;
        }
        public override void AI()
        {
            Lighting.AddLight(NPC.Center, 1f, 0.6f, 0f);
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
                case State_Hovering1:
                case State_Hovering2:
                    Vector2 targetHoverPos = target.Center + new Vector2(NPC.direction * 250f, -150f);
                    MoveTowards(targetHoverPos, 6f, 20f);
                    if (Timer > 120)
                    {
                        Timer = 0;
                        AttackCounter = 0;
                        State = (State == State_Hovering1) ? State_Fireballs : State_PollenRain;
                    }
                    break;

                case State_Fireballs:
                    NPC.velocity *= 0.9f;
                    NPC.spriteDirection = target.Center.X > NPC.Center.X ? 1 : -1;

                    if (Timer % 30 == 0)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Vector2 shootVel = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero) * 8f;
                            shootVel = shootVel.RotatedByRandom(MathHelper.ToRadians(15));
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, shootVel, ModContent.ProjectileType<BloodDividerFireball>(), 25, 3f, Main.myPlayer);
                        }
                        SoundEngine.PlaySound(SoundID.Item20, NPC.Center);
                        AttackCounter++;
                        if (AttackCounter >= 3)
                        {
                            State = State_Hovering2;
                            Timer = 0;
                        }
                    }
                    break;

                case State_PollenRain:
                    Vector2 pollenHoverPos = target.Center + new Vector2(0, -250f);
                    MoveTowards(pollenHoverPos, 8f, 15f);
                    NPC.spriteDirection = target.Center.X > NPC.Center.X ? 1 : -1;
                    if (Timer % 10 == 0 && Math.Abs(NPC.Center.X - target.Center.X) < 150f)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Vector2 shootVel = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(2f, 4f));
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Bottom, shootVel, ModContent.ProjectileType<BloodDividerPollen>(), 0, 0f, Main.myPlayer);
                        }
                        AttackCounter++;
                    }
                    if (Timer > 180)
                    {
                        State = State_Hovering1;
                        Timer = 0;
                    }
                    break;
            }
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Torch, 0f, -2f, 100, default, 1.5f);
                dust.noGravity = true;
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
            if (State == State_Hovering1 || State == State_Hovering2)
            {
                if (NPC.velocity.X > 0) NPC.spriteDirection = 1;
                else if (NPC.velocity.X < 0) NPC.spriteDirection = -1;
            }
        }
        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            if (NPC.frameCounter >= 6)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;
                if (NPC.frame.Y >= frameHeight * 4)
                {
                    NPC.frame.Y = 0;
                }
            }
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int i = 0; i < 15; i++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Glass, hit.HitDirection, -1f, 0, default, 1.2f);
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Torch, hit.HitDirection * 2f, -2f, 0, default, 1.5f);
            }
        }
        public override void OnKill()
        {
            for (int i = 0; i < 30; i++)
            {
                Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Torch, Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f), 100, default, 2.5f);
                dust.noGravity = true;
            }
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[Type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width / 2, NPC.frame.Height / 2);
            Vector2 drawPos = NPC.Center - screenPos;
            SpriteEffects effects = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.ZoomMatrix);
            Texture2D circularGlow = ModContent.Request<Texture2D>("jarkos/textures/Circular_glow").Value;

            spriteBatch.Draw(circularGlow, drawPos, null, new Color(255, 128, 0, 0) * 0.5f, 0f, circularGlow.Size() / 2f, NPC.scale * 1.5f, SpriteEffects.None, 0f);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.ZoomMatrix);

            for (int i = 0; i < NPC.oldPos.Length; i++)
            {
                if (NPC.oldPos[i] == Vector2.Zero) continue;
                float progress = 1f - (float)i / NPC.oldPos.Length;
                Color trailColor = new Color(255, 150, 50, 0) * progress * 0.5f;
                Vector2 trailPos = NPC.oldPos[i] + NPC.Size / 2f - screenPos;
                spriteBatch.Draw(texture, trailPos, NPC.frame, trailColor, NPC.rotation, drawOrigin, NPC.scale, effects, 0f);
            }

            spriteBatch.Draw(texture, drawPos, NPC.frame, drawColor, NPC.rotation, drawOrigin, NPC.scale, effects, 0f);
            spriteBatch.Draw(texture, drawPos, NPC.frame, new Color(255, 255, 255, 0) * 0.5f, NPC.rotation, drawOrigin, NPC.scale, effects, 0f);
            return false;
        }
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            bool inCaverns = spawnInfo.Player.ZoneRockLayerHeight || spawnInfo.Player.ZoneDirtLayerHeight;
            bool notInSpecialBiome = !spawnInfo.Player.ZoneDungeon && !spawnInfo.Player.ZoneJungle && !spawnInfo.Player.ZoneUnderworldHeight;
            if (inCaverns && notInSpecialBiome && !NPC.AnyNPCs(NPC.type) && Main.hardMode)
            {
                return 0.02f;
            }
            return 0f;
        }
    }
    public class BloodDividerFireball : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Fireball;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 5;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
        }
        public override void AI()
        {
            Projectile.velocity.Y += 0.15f;
            if (Projectile.velocity.Y > 12f) Projectile.velocity.Y = 12f;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Lighting.AddLight(Projectile.Center, 0.8f, 0.4f, 0f);
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 1.5f);
                dust.noGravity = true;
                dust.velocity *= 0.3f;
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.Kill();
            return false;
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
            for (int i = 0; i < 20; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.Center - new Vector2(30, 30), 60, 60, DustID.Torch, Main.rand.NextFloat(-4, 4), Main.rand.NextFloat(-4, 4), 100, default, 2f);
                dust.noGravity = true;
            }
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                float explosionRadius = 60f;
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player target = Main.player[i];
                    if (target.active && !target.dead && Vector2.Distance(Projectile.Center, target.Center) < explosionRadius)
                    {
                        target.Hurt(PlayerDeathReason.ByProjectile(target.whoAmI, Projectile.whoAmI), Projectile.damage, Math.Sign(target.Center.X - Projectile.Center.X));
                        target.AddBuff(BuffID.OnFire, 180);
                    }
                }
            }
        }
        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            target.AddBuff(BuffID.OnFire, 180);
        }
    }
    public class BloodDividerPollen : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_4";
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
            Projectile.alpha = 255;
        }
        public override void AI()
        {
            Projectile.velocity.Y += 0.05f;
            if (Projectile.velocity.Y > 3f) Projectile.velocity.Y = 3f;
            Projectile.velocity.X = (float)Math.Sin(Projectile.timeLeft * 0.05f) * 1.5f;

            Lighting.AddLight(Projectile.Center, 0.6f, 0.3f, 0f);

            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 1.2f);
                dust.noGravity = true;
                dust.velocity *= 0.1f;
            }
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player p = Main.player[i];
                if (p.active && !p.dead && Projectile.Hitbox.Intersects(p.Hitbox))
                {
                    p.GetModPlayer<BloodDividerPlayer>().InPollen = true;
                }
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.Kill();
            return false;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }
    }
    public class BloodDividerPlayer : ModPlayer
    {
        public bool InPollen;
        public bool WasInPollen;
        public int PollenTimer;
        public override void PostUpdateMiscEffects()
        {
            if (InPollen)
            {
                if (!WasInPollen)
                {
                    PollenTimer = 14;
                }
                PollenTimer++;
                if (PollenTimer >= 15)
                {
                    PollenTimer = 0;
                    int damage = 5;
                    Player.statLife -= damage;
                    CombatText.NewText(Player.getRect(), CombatText.LifeRegenNegative, damage);
                    if (Player.statLife <= 0)
                    {
                        Player.KillMe(PlayerDeathReason.ByCustomReason(Player.name + " burned in the pollen."), damage, 0);
                    }
                }
            }
            else
            {
                PollenTimer = 0;
            }
            WasInPollen = InPollen;
            InPollen = false;
        }
    }
}
