using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.true_melee
{
    public class FrostSeed_proj : ModProjectile
    {
        public override string Texture => "jarkos/projectiles/melee/true_melee/FrostSeed_proj";
        private static readonly int[] StageDuration = { 36, 48, 54, 72 };
        private static readonly int[] StageEnd = {
            StageDuration[0], StageDuration[0] + StageDuration[1], StageDuration[0] + StageDuration[1] + StageDuration[2], StageDuration[0] + StageDuration[1] + StageDuration[2] + StageDuration[3], };
        private const int Frames = 4;
        private const float DecelLerp = 0.06f;
        private const float StopThreshold = 0.05f;
        private const float RepulseRadius = 80f;
        private const float RepulseStrength = 0.25f;
        private const int BurstDamageStage1 = 80;
        public const int VisualMaxSeeds = 15;
        private float Age
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        private float PhaseSeed
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        private float DetonationTimer
        {
            get => Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }
        private float SpinPhase
        {
            get => Projectile.localAI[1];
            set => Projectile.localAI[1] = value;
        }
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            Main.projFrames[Type] = Frames;
        }
        public override void SetDefaults()
        {
            Projectile.netImportant = true;
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 4000;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override bool? CanHitNPC(NPC target) => false;
        public override void OnSpawn(IEntitySource source)
        {
            DetonationTimer = -1f;
            PhaseSeed = Main.rand.NextFloat(0f, MathHelper.TwoPi);
            SpinPhase = Main.rand.NextFloat(0f, MathHelper.TwoPi);
            SoundEngine.PlaySound(SoundID.Item28 with { Pitch = 0.85f, Volume = 0.35f }, Projectile.Center);
            for (int i = 0; i < 8; i++)
            {
                Vector2 dv = Main.rand.NextVector2Circular(2.0f, 2.0f);
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.SnowflakeIce, dv, 80, default, Main.rand.NextFloat(0.8f, 1.4f));
                d.noGravity = true;
            }
        }
        public override void AI()
        {
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, Vector2.Zero, DecelLerp);
            if (Projectile.velocity.LengthSquared() < StopThreshold * StopThreshold)
                Projectile.velocity = Vector2.Zero;
            ApplyRepulsion();
            Age += 1f;
            int stage = ComputeStage(Age);
            Projectile.frame = stage;
            SpinPhase += 0.06f + 0.012f * stage;
            Projectile.rotation = MathF.Sin(SpinPhase) * 0.18f + PhaseSeed;
            float glowAmt = 0.25f + 0.18f * stage;
            Lighting.AddLight(Projectile.Center, 0.15f * glowAmt, 0.35f * glowAmt, 0.55f * glowAmt);
            if (Main.rand.NextBool(stage == 3 ? 4 : 12))
            {
                Vector2 dv = Main.rand.NextVector2Circular(1.2f, 1.2f);
                int dustId = stage >= 2 ? DustID.IceTorch : DustID.SnowflakeIce;
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f), dustId, dv, 80, default, Main.rand.NextFloat(0.8f, 1.2f));
                d.noGravity = true;
            }
            if (DetonationTimer >= 0f)
            {
                DetonationTimer -= 1f;
                if (DetonationTimer <= 0f)
                {
                    Explode(stage);
                    return;
                }
                Projectile.position += Main.rand.NextVector2Circular(1.5f, 1.5f);
            }
            if (Age >= StageEnd[3])
            {
                Explode(3);
            }
        }
        private void ApplyRepulsion()
        {
            if (Projectile.owner != Main.myPlayer)
                return;
            Vector2 push = Vector2.Zero;
            float r2 = RepulseRadius * RepulseRadius;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile o = Main.projectile[i];
                if (!o.active || o.whoAmI == Projectile.whoAmI) continue;
                if (o.type != Projectile.type) continue;
                Vector2 d = Projectile.Center - o.Center;
                float dist2 = d.LengthSquared();
                if (dist2 >= r2 || dist2 < 0.0001f) continue;
                float dist = MathF.Sqrt(dist2);
                float fall = 1f - dist / RepulseRadius;
                push += (d / dist) * (fall * fall * RepulseStrength);
            }
            Projectile.velocity += push;
        }
        private static int ComputeStage(float age)
        {
            if (age < StageEnd[0]) return 0;
            if (age < StageEnd[1]) return 1;
            if (age < StageEnd[2]) return 2;
            return 3;
        }
        public void RequestDetonation(int delay)
        {
            if (DetonationTimer < 0f)
                DetonationTimer = MathF.Max(1f, delay);
            SoundEngine.PlaySound(SoundID.Item28 with { Pitch = 1.0f, Volume = 0.30f }, Projectile.Center);
        }
        private void Explode(int stage)
        {
            Vector2 pos = Projectile.Center;
            int owner = Projectile.owner;
            if (Main.netMode != NetmodeID.MultiplayerClient && Projectile.owner == Main.myPlayer)
            {
                int burstDamage = ComputeBurstDamage(stage);
                float radiusScale = ComputeBurstRadiusScale(stage);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), pos, Vector2.Zero, ModContent.ProjectileType<FrostBurst_proj>(), burstDamage, 2.0f, owner, ai0: radiusScale, ai1: stage);
                if (stage >= 3)
                {
                    int shardDamage = Math.Max(1, (int)(Projectile.damage * 0.85f));
                    float shardSpeed = 11f;
                    for (int i = 0; i < 8; i++)
                    {
                        float a = MathHelper.TwoPi * i / 8f + Main.rand.NextFloat(-0.04f, 0.04f);
                        Vector2 vel = a.ToRotationVector2() * shardSpeed;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), pos, vel, ModContent.ProjectileType<IceShard_proj>(), shardDamage, 1.0f, owner);
                    }
                }
            }
            float pitch = stage == 3 ? -0.10f : 0.15f + 0.12f * (3 - stage);
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = pitch, Volume = 0.55f }, pos);
            SoundEngine.PlaySound(SoundID.Item28 with { Pitch = -0.30f, Volume = 0.50f }, pos);
            int dustCount = 20 + stage * 10;
            for (int i = 0; i < dustCount; i++)
            {
                Vector2 dv = Main.rand.NextVector2Circular(7f, 7f);
                Dust d = Dust.NewDustPerfect(pos, DustID.SnowflakeIce, dv, 60, default, Main.rand.NextFloat(1.2f, 2.0f));
                d.noGravity = Main.rand.NextBool(2);
            }
            for (int i = 0; i < 10 + stage * 4; i++)
            {
                Vector2 dv = Main.rand.NextVector2Circular(4f, 4f);
                Dust d = Dust.NewDustPerfect(pos, DustID.IceTorch, dv, 100, default, Main.rand.NextFloat(1.0f, 1.5f));
                d.noGravity = true;
            }
            Projectile.Kill();
        }
        private int ComputeBurstDamage(int stage)
        {
            float mul = stage switch
            {
                0 => 0.55f, 1 => 0.85f, 2 => 1.15f, _ => 1.55f, };
            return Math.Max(1, (int)(Projectile.damage * mul));
        }
        private float ComputeBurstRadiusScale(int stage)
        {
            return stage switch
            {
                0 => 0.65f, 1 => 0.85f, 2 => 1.10f, _ => 1.35f, };
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> texAsset = TextureAssets.Projectile[Projectile.type];
            if (!texAsset.IsLoaded) return true;
            Texture2D texture = texAsset.Value;
            int frameH = texture.Height / Frames;
            Rectangle srcRect = new Rectangle(0, Projectile.frame * frameH, texture.Width, frameH);
            Vector2 origin = new Vector2(texture.Width / 2f, frameH / 2f);
            Vector2 screen = Projectile.Center - Main.screenPosition;
            float pulse = 0.5f + 0.5f * MathF.Sin(SpinPhase * 0.7f);
            float baseScale = 1f + 0.07f * pulse + Projectile.frame * 0.02f;
            try
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                float stageBoost = 0.20f + 0.18f * Projectile.frame;
                Color haloOuter = new Color(80, 160, 220, 0) * (0.35f + 0.18f * pulse) * stageBoost;
                Color haloInner = new Color(200, 235, 255, 0) * (0.45f + 0.18f * pulse) * stageBoost;
                for (int i = 0; i < 4; i++)
                {
                    float a = MathHelper.TwoPi * i / 4f + SpinPhase * 0.3f;
                    Vector2 off = a.ToRotationVector2() * (1.0f + 0.8f * pulse);
                    Main.spriteBatch.Draw(texture, screen + off, srcRect, haloOuter, Projectile.rotation, origin, baseScale * 1.55f, SpriteEffects.None, 0f);
                }
                Main.spriteBatch.Draw(texture, screen, srcRect, haloInner, Projectile.rotation, origin, baseScale * 1.20f, SpriteEffects.None, 0f);
                if (DetonationTimer >= 0f)
                {
                    float flash = MathHelper.Clamp(1f - DetonationTimer / 12f, 0f, 1f);
                    Color preExplode = new Color(255, 255, 255, 0) * (flash * 0.7f);
                    Main.spriteBatch.Draw(texture, screen, srcRect, preExplode, Projectile.rotation, origin, baseScale * (1.0f + 0.25f * flash), SpriteEffects.None, 0f);
                }
            }
            finally
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            Color baseColor = Projectile.GetAlpha(lightColor);
            Main.EntitySpriteDraw(texture, screen, srcRect, baseColor, Projectile.rotation, origin, baseScale, SpriteEffects.None, 0);
            return false;
        }
    }
}
