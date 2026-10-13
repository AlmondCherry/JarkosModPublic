using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.other_melee
{
    public class Lomprojectile : ModProjectile
    {
        private enum CrowbarState
        {
            Flying = 0, Lodged = 1, Ripping = 2, Returning = 3
        }
        private CrowbarState State
        {
            get => (CrowbarState)(int)Projectile.ai[0];
            set => Projectile.ai[0] = (float)value;
        }
        private float LodgeTimer
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        private int lodgedNPC
        {
            get => (int)Projectile.ai[2];
            set => Projectile.ai[2] = value;
        }
        private CrowbarState oldState = CrowbarState.Flying;
        private Vector2 lodgeOffset = Vector2.Zero;
        private float wobbleAngle = 0f;
        private float ripProgress = 0f;
        private List<FleshChunk> fleshChunks = new List<FleshChunk>();
        private bool hasPlayedLodgeSound = false;
        private bool hasPlayedRipSound = false;
        private float bloodDripTimer = 0f;
        private struct FleshChunk
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Rotation;
            public float RotationSpeed;
            public float Scale;
            public float Life;
            public float MaxLife;
            public Color ChunkColor;
            public void Update()
            {
                Position += Velocity;
                Velocity.Y += 0.3f;
                Velocity *= 0.98f;
                Rotation += RotationSpeed;
                Life--;
            }
        }
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.light = 0.2f;
            Projectile.extraUpdates = 0;
        }
        public override void AI()
        {
            if (State != oldState)
            {
                if (State == CrowbarState.Lodged)
                {
                    if (lodgedNPC >= 0 && lodgedNPC < Main.maxNPCs)
                    {
                        NPC target = Main.npc[lodgedNPC];
                        if (target.active)
                        {
                            lodgeOffset = Projectile.Center - target.Center;
                        }
                    }
                }
                oldState = State;
            }

            Player owner = Main.player[Projectile.owner];
            Projectile.timeLeft = 600;
            switch (State)
            {
                case CrowbarState.Flying:
                    FlyingAI(owner);
                    break;
                case CrowbarState.Lodged:
                    LodgedAI(owner);
                    break;
                case CrowbarState.Ripping:
                    RippingAI(owner);
                    break;
                case CrowbarState.Returning:
                    ReturningAI(owner);
                    break;
            }
            for (int i = fleshChunks.Count - 1; i >= 0; i--)
            {
                FleshChunk chunk = fleshChunks[i];
                chunk.Update();
                fleshChunks[i] = chunk;
                if (chunk.Life <= 0)
                    fleshChunks.RemoveAt(i);
            }
        }
        private void FlyingAI(Player owner)
        {
            Projectile.rotation += Projectile.velocity.X * 0.06f;
            Projectile.velocity.Y += 0.15f;
            if (Main.netMode != NetmodeID.Server)
            {
                if (Main.rand.NextBool(3))
                {
                    Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Iron, 0, 0, 100, default, 0.6f);
                    dust.noGravity = true;
                    dust.velocity = -Projectile.velocity * 0.1f;
                }
                if (Main.rand.NextBool(5))
                {
                    Dust wind = Dust.NewDustDirect(Projectile.Center - Projectile.velocity, 0, 0, DustID.Smoke, 0, 0, 150, Color.Gray, 0.4f);
                    wind.noGravity = true;
                    wind.velocity = -Projectile.velocity * 0.05f;
                    wind.fadeIn = 0.5f;
                }
            }
        }
        private void LodgedAI(Player owner)
        {
            LodgeTimer++;
            if (lodgedNPC < 0 || lodgedNPC >= Main.maxNPCs || !Main.npc[lodgedNPC].active)
            {
                if (Projectile.owner == Main.myPlayer)
                {
                    State = CrowbarState.Returning;
                    Projectile.netUpdate = true;
                }
                if (Main.netMode != NetmodeID.Server) SpawnBloodBurst(Projectile.Center, 15);
                return;
            }
            NPC target = Main.npc[lodgedNPC];
            Projectile.Center = target.Center + lodgeOffset;
            Projectile.velocity = Vector2.Zero;
            float shakeIntensity = MathHelper.Lerp(0.02f, 0.15f, LodgeTimer / 120f);
            wobbleAngle = (float)Math.Sin(LodgeTimer * 0.5f) * shakeIntensity;
            Vector2 dirFromNPC = (Projectile.Center - target.Center).SafeNormalize(Vector2.UnitX);
            Projectile.rotation = dirFromNPC.ToRotation() + MathHelper.PiOver4 + wobbleAngle;
            if (!hasPlayedLodgeSound && Main.netMode != NetmodeID.Server)
            {
                SoundEngine.PlaySound(SoundID.NPCHit1 with { Pitch = -0.5f, Volume = 0.8f }, Projectile.Center);
                hasPlayedLodgeSound = true;
            }
            if (Main.netMode != NetmodeID.Server)
            {
                bloodDripTimer++;
                if (bloodDripTimer > 8f - (LodgeTimer / 120f * 5f))
                {
                    bloodDripTimer = 0;
                    SpawnBloodDrip(Projectile.Center);
                }
                if (Main.rand.NextBool(8))
                {
                    Vector2 bloodVel = dirFromNPC.RotatedByRandom(0.5f) * Main.rand.NextFloat(1f, 3f);
                    Dust blood = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Blood, bloodVel.X, bloodVel.Y, 0, default, Main.rand.NextFloat(0.8f, 1.4f));
                    blood.noGravity = false;
                }
                if (LodgeTimer > 90)
                {
                    Projectile.Center += Main.rand.NextVector2Circular(2f, 2f);
                    if (Main.rand.NextBool(3))
                    {
                        Vector2 bv = dirFromNPC.RotatedByRandom(0.8f) * Main.rand.NextFloat(2f, 5f);
                        Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Blood, bv.X, bv.Y, 0, default, 1.5f);
                        d.noGravity = false;
                    }
                }
            }
            if (LodgeTimer % 20 == 0)
            {
                if (Projectile.owner == Main.myPlayer)
                {
                    target.SimpleStrikeNPC(Projectile.damage / 5, 0, false, 0, DamageClass.Melee);
                    if (Main.netMode != NetmodeID.SinglePlayer) NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, target.whoAmI);
                }
                if (Main.netMode != NetmodeID.Server)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 v = Main.rand.NextVector2Circular(3f, 3f);
                        Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Blood, v.X, v.Y, 0, default, 1f);
                        d.noGravity = false;
                    }
                }
            }
            if (LodgeTimer >= 120f)
            {
                if (Projectile.owner == Main.myPlayer)
                {
                    State = CrowbarState.Ripping;
                    Projectile.netUpdate = true;
                }
                ripProgress = 0f;
            }
        }
        private void RippingAI(Player owner)
        {
            ripProgress++;
            if (!hasPlayedRipSound)
            {
                if (Main.netMode != NetmodeID.Server)
                {
                    SoundEngine.PlaySound(SoundID.NPCDeath1 with { Pitch = -0.3f, Volume = 1f }, Projectile.Center);
                    SoundEngine.PlaySound(SoundID.Item71 with { Pitch = -0.8f, Volume = 0.7f }, Projectile.Center);
                    SpawnBloodBurst(Projectile.Center, 40);
                    SpawnFleshChunks(Projectile.Center, 8);
                }
                hasPlayedRipSound = true;
                if (Projectile.owner == Main.myPlayer && lodgedNPC >= 0 && lodgedNPC < Main.maxNPCs && Main.npc[lodgedNPC].active)
                {
                    NPC target = Main.npc[lodgedNPC];
                    target.SimpleStrikeNPC(Projectile.damage * 2, Projectile.direction, false, 8f, DamageClass.Melee);
                    if (Main.netMode != NetmodeID.SinglePlayer) NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, target.whoAmI);
                }
            }
            float ripDuration = 15f;
            if (ripProgress <= ripDuration)
            {
                Vector2 ripDirection;
                if (lodgedNPC >= 0 && lodgedNPC < Main.maxNPCs && Main.npc[lodgedNPC].active)
                {
                    ripDirection = (Projectile.Center - Main.npc[lodgedNPC].Center).SafeNormalize(Vector2.UnitX);
                }
                else
                {
                    ripDirection = (owner.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                }
                float ripSpeed = MathHelper.Lerp(8f, 2f, ripProgress / ripDuration);
                Projectile.velocity = ripDirection * ripSpeed;
                Projectile.rotation += ripDirection.X * 0.3f;
                if (Main.netMode != NetmodeID.Server)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 bv = -ripDirection.RotatedByRandom(0.6f) * Main.rand.NextFloat(3f, 8f);
                        Dust blood = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Blood, bv.X, bv.Y, 0, default, Main.rand.NextFloat(1.2f, 2f));
                        blood.noGravity = false;
                    }
                    if (Main.rand.NextBool(3))
                    {
                        SpawnFleshChunks(Projectile.Center, 1);
                    }
                }
            }
            else
            {
                if (Projectile.owner == Main.myPlayer)
                {
                    State = CrowbarState.Returning;
                    Projectile.netUpdate = true;
                }
                Projectile.tileCollide = false;
                lodgedNPC = -1;
            }
        }
        private void ReturningAI(Player owner)
        {
            Projectile.tileCollide = false;
            Vector2 toOwner = owner.Center - Projectile.Center;
            float distance = toOwner.Length();
            if (distance < 30f)
            {
                Projectile.Kill();
                return;
            }
            Vector2 desired = toOwner.SafeNormalize(Vector2.Zero) * Math.Max(16f, distance * 0.08f);
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, desired, 0.15f);
            if (Projectile.velocity.Length() > 28f)
            {
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 28f;
            }
            Projectile.rotation += Projectile.velocity.X * 0.05f;
            if (Main.netMode != NetmodeID.Server)
            {
                if (Main.rand.NextBool(4))
                {
                    Dust blood = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Blood, Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(1f, 3f), 0, default, 0.8f);
                    blood.noGravity = false;
                }
                if (Main.rand.NextBool(5))
                {
                    Dust Spark = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Iron, 0, 0, 100, default, 0.5f);
                    Spark.noGravity = true;
                    Spark.velocity *= 0.3f;
                }
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (State == CrowbarState.Flying)
            {
                if (Projectile.owner == Main.myPlayer)
                {
                    State = CrowbarState.Lodged;
                    LodgeTimer = 0;
                    lodgedNPC = target.whoAmI;
                    lodgeOffset = Projectile.Center - target.Center;
                    Projectile.velocity = Vector2.Zero;
                    Projectile.tileCollide = false;
                    Projectile.netUpdate = true;
                }
                hasPlayedLodgeSound = false;
                hasPlayedRipSound = false;
                if (Main.netMode != NetmodeID.Server)
                {
                    for (int i = 0; i < 15; i++)
                    {
                        Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                        int dustType = Main.rand.NextBool() ? DustID.Blood : DustID.Iron;
                        float scale = dustType == DustID.Blood ? 1.3f : 0.8f;
                        Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, dustType, vel.X, vel.Y, 0, default, scale);
                        d.noGravity = dustType == DustID.Iron;
                    }
                    SoundEngine.PlaySound(SoundID.Item22 with { Pitch = -0.4f }, Projectile.Center);
                }
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (State == CrowbarState.Flying)
            {
                if (Projectile.owner == Main.myPlayer)
                {
                    State = CrowbarState.Returning;
                    Projectile.tileCollide = false;
                    Projectile.netUpdate = true;
                }
                if (Main.netMode != NetmodeID.Server)
                {
                    SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
                    for (int i = 0; i < 12; i++)
                    {
                        Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                        Dust Spark = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Iron, vel.X, vel.Y, 0, default, 1f);
                        Spark.noGravity = false;
                    }
                    for (int i = 0; i < 6; i++)
                    {
                        Vector2 vel = -oldVelocity.RotatedByRandom(0.8f) * Main.rand.NextFloat(0.2f, 0.6f);
                        Dust Spark = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Torch, vel.X, vel.Y, 0, new Color(255, 220, 100), 0.8f);
                        Spark.noGravity = true;
                    }
                }
                return false;
            }
            return false;
        }
        public override bool? CanHitNPC(NPC target)
        {
            if (State == CrowbarState.Lodged || State == CrowbarState.Ripping)
                return false;
            return null;
        }
        private void SpawnBloodBurst(Vector2 position, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(10f, 10f);
                float scale = Main.rand.NextFloat(1f, 2.2f);
                Dust blood = Dust.NewDustDirect(position, 0, 0, DustID.Blood, vel.X, vel.Y, 0, default, scale);
                blood.noGravity = Main.rand.NextBool(3);
            }
            for (int i = 0; i < count / 3; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(7f, 7f);
                Dust gore = Dust.NewDustDirect(position, 0, 0, DustID.Blood, vel.X, vel.Y, 80, new Color(120, 0, 0), 2.5f);
                gore.noGravity = false;
            }
        }
        private void SpawnBloodDrip(Vector2 position)
        {
            Dust drip = Dust.NewDustDirect(position, 0, 0, DustID.Blood, Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(1f, 2.5f), 0, default, Main.rand.NextFloat(0.8f, 1.2f));
            drip.noGravity = false;
        }
        private void SpawnFleshChunks(Vector2 position, int count)
        {
            for (int i = 0; i < count; i++)
            {
                FleshChunk chunk = new FleshChunk
                {
                    Position = position + Main.rand.NextVector2Circular(8f, 8f),
                    Velocity = Main.rand.NextVector2Circular(6f, 6f) + new Vector2(0, -2f),
                    Rotation = Main.rand.NextFloat(MathHelper.TwoPi),
                    RotationSpeed = Main.rand.NextFloat(-0.3f, 0.3f),
                    Scale = Main.rand.NextFloat(3f, 8f),
                    MaxLife = Main.rand.Next(40, 80),
                    ChunkColor = Color.Lerp(new Color(180, 20, 20), new Color(100, 10, 10), Main.rand.NextFloat())
                };
                chunk.Life = chunk.MaxLife;
                fleshChunks.Add(chunk);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Texture2D projTexture = TextureAssets.Projectile[Projectile.type].Value;

            if (State == CrowbarState.Flying || State == CrowbarState.Returning)
            {
                DrawMetalTrail(spriteBatch, pixel);
            }
            if (State == CrowbarState.Ripping || State == CrowbarState.Returning)
            {
                DrawBloodTrail(spriteBatch, pixel);
            }
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float drawRotation = Projectile.rotation;

            spriteBatch.Draw(projTexture, drawPos, null, lightColor, drawRotation, projTexture.Size() / 2f, Projectile.scale, SpriteEffects.None, 0f);
            if (State != CrowbarState.Flying)
            {
                Color bloodTint = new Color(150, 20, 20, 0) * 0.3f;
                spriteBatch.Draw(projTexture, drawPos, null, bloodTint, drawRotation, projTexture.Size() / 2f, Projectile.scale * 1.02f, SpriteEffects.None, 0f);
            }
            return false;
        }
        private void DrawMetalTrail(SpriteBatch spriteBatch, Texture2D pixel)
        {
            for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero || Projectile.oldPos[i + 1] == Vector2.Zero)
                    continue;

                float progress = (float)i / Projectile.oldPos.Length;

                Vector2 pos1 = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 pos2 = Projectile.oldPos[i + 1] + Projectile.Size / 2f - Main.screenPosition;

                Vector2 direction = pos2 - pos1;

                float length = direction.Length();
                if (length == 0) continue;

                float rotation = direction.ToRotation();

                Color outerColor = new Color(160, 160, 170, 0) * (1f - progress) * 0.3f;
                float outerWidth = MathHelper.Lerp(8f, 2f, progress);

                spriteBatch.Draw(pixel, pos1, new Rectangle(0, 0, 1, 1), outerColor, rotation, new Vector2(0, 0.5f), new Vector2(length, outerWidth), SpriteEffects.None, 0f);

                Color coreColor = new Color(220, 225, 230, 0) * (1f - progress) * 0.5f;
                float coreWidth = MathHelper.Lerp(3f, 0.5f, progress);

                spriteBatch.Draw(pixel, pos1, new Rectangle(0, 0, 1, 1), coreColor, rotation, new Vector2(0, 0.5f), new Vector2(length, coreWidth), SpriteEffects.None, 0f);
                if (State == CrowbarState.Returning && !Main.rand.NextBool(3))
                {
                    Color sparkColor = new Color(255, 180, 50, 0) * (1f - progress) * 0.4f;

                    float sparkWidth = MathHelper.Lerp(2f, 0.5f, progress);
                    float sparkOffset = (float)Math.Sin(progress * 20f + Main.GameUpdateCount * 0.5f) * 3f;

                    Vector2 perp = direction.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2) * sparkOffset;
                    spriteBatch.Draw(pixel, pos1 + perp, new Rectangle(0, 0, 1, 1), sparkColor, rotation, new Vector2(0, 0.5f), new Vector2(length * 0.5f, sparkWidth), SpriteEffects.None, 0f);
                }
            }
        }
        private void DrawBloodTrail(SpriteBatch spriteBatch, Texture2D pixel)
        {
            for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero || Projectile.oldPos[i + 1] == Vector2.Zero)
                    continue;
                float progress = (float)i / Projectile.oldPos.Length;

                Vector2 pos1 = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 pos2 = Projectile.oldPos[i + 1] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 direction = pos2 - pos1;

                float length = direction.Length();
                if (length == 0) continue;

                float rotation = direction.ToRotation();
                Color bloodOuter = new Color(120, 10, 10, 0) * (1f - progress) * 0.4f;
                float bloodOuterWidth = MathHelper.Lerp(10f, 3f, progress);

                spriteBatch.Draw(pixel, pos1, new Rectangle(0, 0, 1, 1), bloodOuter, rotation, new Vector2(0, 0.5f), new Vector2(length, bloodOuterWidth), SpriteEffects.None, 0f);
                Color bloodInner = new Color(220, 30, 30, 0) * (1f - progress) * 0.6f;
                float bloodInnerWidth = MathHelper.Lerp(5f, 1f, progress);

                spriteBatch.Draw(pixel, pos1, new Rectangle(0, 0, 1, 1), bloodInner, rotation, new Vector2(0, 0.5f), new Vector2(length, bloodInnerWidth), SpriteEffects.None, 0f);
                float wave = (float)Math.Sin(progress * 12f + Main.GameUpdateCount * 0.2f) * 4f * (1f - progress);
                Vector2 perp = direction.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2) * wave;
                Color wavyBlood = new Color(180, 15, 15, 0) * (1f - progress) * 0.25f;
                
                spriteBatch.Draw(pixel, pos1 + perp, new Rectangle(0, 0, 1, 1), wavyBlood, rotation, new Vector2(0, 0.5f), new Vector2(length, 3f), SpriteEffects.None, 0f);
            }
        }
        public override void OnKill(int timeLeft)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 6; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                    Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Iron, vel.X, vel.Y, 0, default, 0.7f);
                    d.noGravity = true;
                }
            }
        }
    }
}
