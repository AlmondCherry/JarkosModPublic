using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using ReLogic.Content;
using jarkos.systems;
using jarkos.particles;
using jarkos.systems;
namespace jarkos.projectiles.mecha
{
    public class Nova_proj : ModProjectile
    {
        public override string Texture => "jarkos/projectiles/mecha/Nova_proj";
        private List<Vector2> trailPositions = new List<Vector2>();
        private const int MaxTrailLength = 30;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.light = 1.0f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 2;
        }
        public override void AI()
        {
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > MaxTrailLength)
                trailPositions.RemoveAt(trailPositions.Count - 1);
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.3f) * 0.3f + 0.7f;
            Lighting.AddLight(Projectile.Center, 1.2f * pulse, 0.2f * pulse, 0.9f * pulse);
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8, 8), DustID.PurpleTorch, Projectile.velocity * -0.2f, 0, default, 1.4f);
                dust.noGravity = true;
            }
            if (Projectile.timeLeft % 10 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item15 with { Volume = 0.25f, Pitch = 0.4f }, Projectile.Center);
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<NovaExplosionRing>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
            }
            return true;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                Projectile.NewProjectile(Projectile.GetSource_Death(), target.Center, Vector2.Zero, ModContent.ProjectileType<NovaSplitCenter>(), Projectile.damage / 3, Projectile.knockBack * 0.5f, Projectile.owner);
            }
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.1f, Volume = 1.3f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item62 with { Pitch = 0.2f, Volume = 1f }, Projectile.Center);
            Color sparkColor = new Color(255, 60, 200);
            for (int i = 0; i < 15; i++)
            {
                new Spark(target.Center, Main.rand.NextVector2Circular(6f, 6f), sparkColor, 18, Main.rand.NextFloat(0.8f, 1.4f), 1f).Spawn();
            }
            for (int i = 0; i < 20; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(8, 8);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch, vel, 0, default, 2f);
                dust.noGravity = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            SpriteBatch spriteBatch = Main.spriteBatch;
            Color baseColor = new Color(255, 30, 200);
            if (HealShaders.Stream != null && HealShaders.HealNoise != null)
            {
                Effect effect = HealShaders.Stream;
                spriteBatch.End();
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 6f + Projectile.identity * 0.1f);
                effect.Parameters["uOpacity"]?.SetValue(1.4f);
                effect.Parameters["uColor"]?.SetValue(baseColor.ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(new Color(255, 230, 255).ToVector4());
                List<Vector2> points = new List<Vector2>();
                points.Add(Projectile.Center);
                for (int i = 0; i < trailPositions.Count; i++)
                {
                    if (trailPositions[i] == Vector2.Zero) continue;
                    points.Add(trailPositions[i]);
                }
                if (points.Count > 1)
                {
                    TrailRenderer.Draw(effect, points, t => 18f * (1f - t), t => Color.White * (1f - t), HealShaders.HealNoise);
                }
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            else
            {
                if (trailPositions.Count >= 2)
                {
                    for (int i = 0; i < trailPositions.Count - 1; i++)
                    {
                        float progress = (float)i / trailPositions.Count;
                        float alpha = (1f - progress) * 0.8f;
                        float width = (1f - progress) * 16f + 2f;
                        Vector2 start = trailPositions[i] - Main.screenPosition;
                        Vector2 end = trailPositions[Math.Min(i + 1, trailPositions.Count - 1)] - Main.screenPosition;
                        float rotation = (end - start).ToRotation();
                        float length = Vector2.Distance(start, end);
                        if (length < 1) continue;
                        Color glowColor = baseColor * alpha * 0.3f;
                        spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), glowColor, rotation, new Vector2(0, 0.5f), new Vector2(length, width * 2.5f), SpriteEffects.None, 0f);
                        Color coreColor = Color.White * alpha * 0.8f;
                        spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), coreColor, rotation, new Vector2(0, 0.5f), new Vector2(length, width * 0.6f), SpriteEffects.None, 0f);
                    }
                }
            }
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.4f) * 0.15f + 1.2f;
            spriteBatch.Draw(texture, drawPos, null, Color.White * 0.95f, Projectile.rotation, origin, pulse, SpriteEffects.None, 0f);
            return false;
        }
    }
    #region Nova Split Center (Stationary Emitter)
    public class NovaSplitCenter : ModProjectile
    {
        public override string Texture => "jarkos/projectiles/mecha/Astra_bomb_charged";
        private int spawnTimer = 0;
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.ignoreWater = true;
        }
        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.4f) * 0.2f + 0.8f;
            Lighting.AddLight(Projectile.Center, 1.5f * pulse, 0.1f * pulse, 1.0f * pulse);
            if (Main.rand.NextBool(2))
            {
                Vector2 offset = Main.rand.NextVector2CircularEdge(25f, 25f);
                Dust d = Dust.NewDustPerfect(Projectile.Center + offset, DustID.PurpleTorch, -offset.RotatedBy(0.4f) * 0.08f, 0, default, Main.rand.NextFloat(0.8f, 1.5f));
                d.noGravity = true;
            }
            spawnTimer++;
            if (spawnTimer >= 30)
            {
                spawnTimer = 0;
                if (Projectile.owner == Main.myPlayer)
                {
                    float baseAngle = (180 - Projectile.timeLeft) * 0.08f;
                    float spinDir = Projectile.timeLeft % 60 == 0 ? 1f : -1f;
                    SoundEngine.PlaySound(SoundID.Item30 with { Volume = 0.5f, Pitch = 0.2f }, Projectile.position);
                    for (int i = 0; i < 4; i++)
                    {
                        float angle = baseAngle + i * MathHelper.PiOver2;
                        Vector2 velocity = angle.ToRotationVector2() * 8f;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<NovaSubProj>(), Projectile.damage, Projectile.knockBack * 0.5f, Projectile.owner, spinDir);
                    }
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.3f) * 0.25f + 1.0f;
            float rotation = Main.GameUpdateCount * 0.08f;
            spriteBatch.Draw(texture, drawPos, null, new Color(255, 60, 200) * 0.8f, rotation, origin, pulse * 1.6f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, Color.White * 0.95f, -rotation * 1.5f, origin, pulse * 0.9f, SpriteEffects.None, 0f);
            return false;
        }
        public override bool? CanDamage() => false;
    }
    #endregion
    #region Nova Sub-Projectile (Homing & Spiral)
    public class NovaSubProj : ModProjectile
    {
        public override string Texture => "jarkos/projectiles/mecha/NovaSubProj";
        private List<Vector2> trailPositions = new List<Vector2>();
        private const int MaxTrailLength = 20;
        private Vector2 spawnPosition = Vector2.Zero;
        private bool initialized = false;
        private bool hasLeftPortalRange = false;
        public float SpinDirection => Projectile.ai[0];
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1;
        }
        public override void AI()
        {
            if (!initialized)
            {
                spawnPosition = Projectile.Center;
                initialized = true;
            }
            if (!hasLeftPortalRange && Vector2.Distance(Projectile.Center, spawnPosition) >= 75f)
            {
                hasLeftPortalRange = true;
            }
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > MaxTrailLength)
                trailPositions.RemoveAt(trailPositions.Count - 1);
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            float timeLived = 120 - Projectile.timeLeft;
            if (timeLived < 40)
            {
                float rotStrength = 0.04f * (1f - timeLived / 40f);
                Projectile.velocity = Projectile.velocity.RotatedBy(rotStrength * SpinDirection);
            }
            NPC closestNPC = null;
            if (hasLeftPortalRange)
            {
                float closestDist = 600f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.dontTakeDamage)
                    {
                        float dist = Vector2.Distance(Projectile.Center, npc.Center);
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            closestNPC = npc;
                        }
                    }
                }
            }
            if (closestNPC != null)
            {
                Vector2 toTarget = (closestNPC.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                float homingWeight = timeLived < 30 ? 0.04f : 0.09f;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity.SafeNormalize(Vector2.UnitX), toTarget, homingWeight) * 8.5f;
            }
            else
            {
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * 8.5f;
            }
            Lighting.AddLight(Projectile.Center, 0.7f, 0.1f, 0.5f);
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch, Projectile.velocity * -0.15f, 0, default, 0.9f);
                dust.noGravity = true;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item93 with { Pitch = 0.6f, Volume = 0.4f }, Projectile.Center);
            Color sparkColor = new Color(255, 60, 200);
            for (int i = 0; i < 4; i++)
            {
                new Spark(Projectile.Center, Main.rand.NextVector2Circular(3f, 3f), sparkColor, 12, Main.rand.NextFloat(0.6f, 1.0f), 1f).Spawn();
            }
        }
        public override bool? CanDamage()
        {
            if (!initialized || !hasLeftPortalRange)
            {
                return false;
            }
            return null;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            SpriteBatch spriteBatch = Main.spriteBatch;
            Color baseColor = new Color(255, 30, 200);
            if (HealShaders.Stream != null && HealShaders.HealNoise != null)
            {
                Effect effect = HealShaders.Stream;
                spriteBatch.End();
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 6.5f + Projectile.identity * 0.1f);
                effect.Parameters["uOpacity"]?.SetValue(1.1f);
                effect.Parameters["uColor"]?.SetValue(baseColor.ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(new Color(255, 230, 255).ToVector4());
                List<Vector2> points = new List<Vector2>();
                points.Add(Projectile.Center);
                for (int i = 0; i < trailPositions.Count; i++)
                {
                    if (trailPositions[i] == Vector2.Zero) continue;
                    points.Add(trailPositions[i]);
                }
                if (points.Count > 1)
                {
                    TrailRenderer.Draw(effect, points, t => 8f * (1f - t), t => Color.White * (1f - t), HealShaders.HealNoise);
                }
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            else
            {
                if (trailPositions.Count >= 2)
                {
                    for (int i = 0; i < trailPositions.Count - 1; i++)
                    {
                        float progress = (float)i / trailPositions.Count;
                        float alpha = (1f - progress) * 0.8f;
                        float width = (1f - progress) * 8f + 1f;
                        Vector2 start = trailPositions[i] - Main.screenPosition;
                        Vector2 end = trailPositions[Math.Min(i + 1, trailPositions.Count - 1)] - Main.screenPosition;
                        float rotation = (end - start).ToRotation();
                        float length = Vector2.Distance(start, end);
                        if (length < 1) continue;
                        Color glowColor = baseColor * alpha * 0.4f;
                        spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), glowColor, rotation, new Vector2(0, 0.5f), new Vector2(length, width * 3f), SpriteEffects.None, 0f);
                        Color coreColor = new Color(255, 200, 255) * alpha;
                        spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), coreColor, rotation, new Vector2(0, 0.5f), new Vector2(length, width), SpriteEffects.None, 0f);
                    }
                }
            }
            return true;
        }
    }
    #endregion
    #region Nova Explosion Ring (Fast block impact ring)
    public class NovaExplosionRing : ModProjectile
    {
        public override string Texture => "jarkos/projectiles/melee/unique_melee/StormWave";
        private float MaxRadius = 320f;
        public const int LIFE = 16;
        private float Progress => 1f - Projectile.timeLeft / (float)LIFE;
        private float Radius => MaxRadius * EaseOut(Progress);
        private static float EaseOut(float t) => 1f - (1f - t) * (1f - t);
        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = LIFE;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void AI()
        {
            int size = (int)(Radius * 2);
            Projectile.position = Projectile.Center - new Vector2(size / 2);
            Projectile.width = size;
            Projectile.height = size;
            if (Projectile.timeLeft == LIFE)
            {
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.1f, Volume = 1.2f }, Projectile.Center);
                SoundEngine.PlaySound(SoundID.Item62 with { Pitch = -0.3f, Volume = 0.8f }, Projectile.Center);
            }
            int particleCount = (int)(4 * (1f - Progress));
            for (int i = 0; i < particleCount; i++)
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 pos = Projectile.Center + angle.ToRotationVector2() * Radius;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 6f);
                Color sparkColor = new Color(255, 60, 200);
                new Spark(pos, vel, sparkColor, 15, Main.rand.NextFloat(0.8f, 1.3f), 0.9f).Spawn();
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 targetCenter = targetHitbox.Center.ToVector2();
            float distance = Vector2.Distance(Projectile.Center, targetCenter);
            float r = Radius;
            float ringWidth = 32f;
            return distance <= r + ringWidth / 2 && distance >= r - ringWidth / 2;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float half = tex.Width / 2f;
            float scale = MaxRadius / (half * 0.82f);
            float currentScale = scale * EaseOut(Progress);
            Effect ringShader = null;
            try
            {
                ringShader = ModContent.Request<Effect>("jarkos/Shaders/SporeRing", AssetRequestMode.ImmediateLoad).Value;
            }
            catch { }
            if (ringShader != null)
            {
                ringShader.Parameters["uProgress"]?.SetValue(Progress);
                ringShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 3.5f);
                ringShader.Parameters["uColor"]?.SetValue(new Vector3(1.0f, 0.1f, 0.7f));
                ringShader.Parameters["uSeed"]?.SetValue(Projectile.identity * 0.1f);
                try
                {
                    Texture2D noise = ModContent.Request<Texture2D>("jarkos/projectiles/melee/unique_melee/WaveNoise", AssetRequestMode.ImmediateLoad).Value;
                    ringShader.Parameters["uImage0"]?.SetValue(noise);
                }
                catch { }
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, ringShader, Main.GameViewMatrix.TransformationMatrix);
                spriteBatch.Draw(tex, drawPos, null, Color.White, 0f, new Vector2(half, half), currentScale, SpriteEffects.None, 0f);
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            else
            {
                float alpha = 1f - Progress;
                spriteBatch.Draw(tex, drawPos, null, new Color(255, 60, 200, 0) * alpha * 0.9f, 0f, new Vector2(half, half), currentScale, SpriteEffects.None, 0f);
            }
            return false;
        }
    }
    #endregion
}
