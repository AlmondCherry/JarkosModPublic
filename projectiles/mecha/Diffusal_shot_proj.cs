using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;
using ReLogic.Content;
using jarkos.systems;
using jarkos.particles;
using jarkos.systems;
namespace Jarkos.projectiles.mecha
{
    public class Diffusal_shot_proj : ModProjectile
    {
        public override string Texture => "jarkos/projectiles/mecha/Astra_bomb_charged";
        private List<Vector2> trailPositions = new List<Vector2>();
        private const int MaxTrailLength = 25;
        private int seekerTimer = 0;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 25;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 240;
            Projectile.light = 1.0f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
        }
        public override void AI()
        {
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > MaxTrailLength)
                trailPositions.RemoveAt(trailPositions.Count - 1);
            Projectile.rotation = Projectile.velocity.ToRotation();
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.4f) * 0.25f + 0.95f;
            Lighting.AddLight(Projectile.Center, 0.2f * pulse, 0.9f * pulse, 1.4f * pulse);
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6, 6), DustID.Electric, Projectile.velocity * -0.2f, 0, default, Main.rand.NextFloat(0.4f, 0.8f));
                dust.noGravity = true;
            }
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8, 8), DustID.Shadowflame, Projectile.velocity * -0.15f, 100, default, Main.rand.NextFloat(0.8f, 1.3f));
                dust.noGravity = true;
            }
            seekerTimer++;
            if (seekerTimer >= 8)
            {
                seekerTimer = 0;
                if (Projectile.owner == Main.myPlayer)
                {
                    NPC closestNPC = null;
                    float closestDist = 250f;
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
                    if (closestNPC != null)
                    {
                        Vector2 targetDir = (closestNPC.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                        SoundEngine.PlaySound(SoundID.Item93 with { Volume = 0.25f, Pitch = 0.5f }, Projectile.Center);
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, targetDir * 12f, ModContent.ProjectileType<DiffusalLightningSpark>(), (int)(Projectile.damage * 0.35f), Projectile.knockBack * 0.2f, Projectile.owner);
                    }
                }
            }
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.1f, Volume = 1.2f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item94 with { Pitch = 0.3f, Volume = 1.0f }, Projectile.Center);
            PunchCameraModifier explosionShake = new PunchCameraModifier(Projectile.Center, Main.rand.NextVector2CircularEdge(1f, 1f), 4.5f, 8f, 8, 800f, "DiffusalExplosion");
            Main.instance.CameraModifiers.Add(explosionShake);
            if (Projectile.owner == Main.myPlayer)
            {
                for (int i = 0; i < 2; i++)
                {
                    float angle = i * MathHelper.Pi + Main.rand.NextFloat(-0.2f, 0.2f);
                    Vector2 shardVel = angle.ToRotationVector2() * Main.rand.NextFloat(7f, 11f);
                    Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, shardVel, ModContent.ProjectileType<DiffusalShard>(), (int)(Projectile.damage * 0.25f), Projectile.knockBack * 0.4f, Projectile.owner);
                }
                Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<DiffusalShockwaveRing>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
            }
            Color cyanColor = new Color(0, 220, 255);
            Color magentaColor = new Color(255, 30, 200);
            for (int i = 0; i < 8; i++)
            {
                new Spark(Projectile.Center, Main.rand.NextVector2Circular(6f, 6f), cyanColor, 15, Main.rand.NextFloat(0.8f, 1.4f), 1f).Spawn();
                new Spark(Projectile.Center, Main.rand.NextVector2Circular(6f, 6f), magentaColor, 15, Main.rand.NextFloat(0.8f, 1.4f), 1f).Spawn();
            }
            for (int i = 0; i < 20; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(8f, 8f);
                int type = Main.rand.NextBool(2) ? DustID.Electric : DustID.Shadowflame;
                Dust dust = Dust.NewDustPerfect(Projectile.Center, type, vel, 0, default, 1.6f);
                dust.noGravity = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            SpriteBatch spriteBatch = Main.spriteBatch;
            Color baseColor = new Color(0, 220, 255);
            if (HealShaders.Stream != null && HealShaders.HealNoise != null)
            {
                Effect effect = HealShaders.Stream;
                spriteBatch.End();
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 6f + Projectile.identity * 0.15f);
                effect.Parameters["uOpacity"]?.SetValue(1.3f);
                effect.Parameters["uColor"]?.SetValue(baseColor.ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(new Color(255, 30, 200).ToVector4());
                List<Vector2> points = new List<Vector2>();
                for (int i = 0; i < trailPositions.Count; i++)
                {
                    if (trailPositions[i] == Vector2.Zero) continue;
                    points.Add(trailPositions[i]);
                }
                if (points.Count > 1)
                {
                    TrailRenderer.Draw(effect, points, t => 16f * (1f - t), t => Color.White * (1f - t), HealShaders.HealNoise);
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
                        float width = (1f - progress) * 14f + 2f;
                        Vector2 start = trailPositions[i] - Main.screenPosition;
                        Vector2 end = trailPositions[Math.Min(i + 1, trailPositions.Count - 1)] - Main.screenPosition;
                        float rotation = (end - start).ToRotation();
                        float length = Vector2.Distance(start, end);
                        if (length < 1) continue;
                        Color glowColor = baseColor * alpha * 0.35f;
                        spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), glowColor, rotation, new Vector2(0, 0.5f), new Vector2(length, width * 2.5f), SpriteEffects.None, 0f);
                        Color coreColor = Color.White * alpha * 0.8f;
                        spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), coreColor, rotation, new Vector2(0, 0.5f), new Vector2(length, width * 0.6f), SpriteEffects.None, 0f);
                    }
                }
            }
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.4f) * 0.15f + 0.1f;
            spriteBatch.Draw(texture, drawPos, null, Color.White * 0.95f, Projectile.rotation, origin, pulse, SpriteEffects.None, 0f);
            return false;
        }
    }
    #region Seeker Lightning Spark (In-flight seeking attack)
    public class DiffusalLightningSpark : ModProjectile
    {
        public override string Texture => "jarkos/projectiles/mecha/Astra_bomb_charged";
        private List<Vector2> trailPositions = new List<Vector2>();
        private const int MaxTrailLength = 12;
        private int targetNPC = -1;
        private float homingStrength = 0.04f;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 35;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }
        public override void AI()
        {
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > MaxTrailLength)
                trailPositions.RemoveAt(trailPositions.Count - 1);
            Projectile.rotation = Projectile.velocity.ToRotation();
            if (targetNPC == -1 || !Main.npc[targetNPC].active || Main.npc[targetNPC].friendly || Main.npc[targetNPC].dontTakeDamage)
            {
                float closestDist = 400f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.dontTakeDamage)
                    {
                        float dist = Vector2.Distance(Projectile.Center, npc.Center);
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            targetNPC = i;
                        }
                    }
                }
            }
            if (targetNPC != -1)
            {
                NPC target = Main.npc[targetNPC];
                Vector2 dir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                homingStrength = Math.Min(0.25f, homingStrength + 0.015f);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity.SafeNormalize(Vector2.UnitY), dir, homingStrength) * 11.5f;
            }
            Lighting.AddLight(Projectile.Center, 0.1f, 0.7f, 0.9f);
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Electric, Projectile.velocity * -0.2f, 0, default, 0.7f);
                dust.noGravity = true;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item93 with { Pitch = 0.7f, Volume = 0.3f }, Projectile.Center);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            SpriteBatch spriteBatch = Main.spriteBatch;
            Color baseColor = new Color(0, 220, 255);
            if (HealShaders.Stream != null && HealShaders.HealNoise != null)
            {
                Effect effect = HealShaders.Stream;
                spriteBatch.End();
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 7f + Projectile.identity * 0.1f);
                effect.Parameters["uOpacity"]?.SetValue(1.1f);
                effect.Parameters["uColor"]?.SetValue(baseColor.ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(new Color(255, 30, 200).ToVector4());
                List<Vector2> points = new List<Vector2>();
                for (int i = 0; i < trailPositions.Count; i++)
                {
                    if (trailPositions[i] == Vector2.Zero) continue;
                    points.Add(trailPositions[i]);
                }
                if (points.Count > 1)
                {
                    TrailRenderer.Draw(effect, points, t => 6f * (1f - t), t => Color.White * (1f - t), HealShaders.HealNoise);
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
                        float alpha = (1f - progress) * 0.7f;
                        float width = (1f - progress) * 6f + 1f;
                        Vector2 start = trailPositions[i] - Main.screenPosition;
                        Vector2 end = trailPositions[Math.Min(i + 1, trailPositions.Count - 1)] - Main.screenPosition;
                        float rotation = (end - start).ToRotation();
                        float length = Vector2.Distance(start, end);
                        if (length < 1) continue;
                        spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), baseColor * alpha * 0.4f, rotation, new Vector2(0, 0.5f), new Vector2(length, width * 2f), SpriteEffects.None, 0f);
                    }
                }
            }
            return false;
        }
    }
    #endregion
    #region Homing Shard (On-explosion homing shard)
    public class DiffusalShard : ModProjectile
    {
        public override string Texture => "jarkos/projectiles/mecha/diffusal_shard";
        private List<Vector2> trailPositions = new List<Vector2>();
        private const int MaxTrailLength = 18;
        private int targetNPC = -1;
        private float homingStrength = 0.05f;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 18;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 25;
        }
        public override void AI()
        {
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > MaxTrailLength)
                trailPositions.RemoveAt(trailPositions.Count - 1);
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (targetNPC == -1 || !Main.npc[targetNPC].active || Main.npc[targetNPC].friendly || Main.npc[targetNPC].dontTakeDamage)
            {
                float closestDist = 550f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.dontTakeDamage)
                    {
                        float dist = Vector2.Distance(Projectile.Center, npc.Center);
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            targetNPC = i;
                        }
                    }
                }
            }
            if (targetNPC != -1)
            {
                NPC target = Main.npc[targetNPC];
                Vector2 targetDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                homingStrength = Math.Min(0.18f, homingStrength + 0.012f);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity.SafeNormalize(Vector2.UnitY), targetDir, homingStrength) * 12f;
            }
            else
            {
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * 12f;
            }
            Lighting.AddLight(Projectile.Center, 0.6f, 0.1f, 0.8f);
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Shadowflame, Projectile.velocity * -0.2f, 100, default, 0.8f);
                dust.noGravity = true;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item93 with { Pitch = 0.6f, Volume = 0.35f }, Projectile.Center);
            Color sparkColor = new Color(255, 30, 200);
            for (int i = 0; i < 3; i++)
            {
                new Spark(Projectile.Center, Main.rand.NextVector2Circular(3f, 3f), sparkColor, 12, Main.rand.NextFloat(0.6f, 1.1f), 1f).Spawn();
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
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 6.5f + Projectile.identity * 0.1f);
                effect.Parameters["uOpacity"]?.SetValue(1.1f);
                effect.Parameters["uColor"]?.SetValue(baseColor.ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(new Color(255, 200, 255).ToVector4());
                List<Vector2> points = new List<Vector2>();
                for (int i = 0; i < trailPositions.Count; i++)
                {
                    if (trailPositions[i] == Vector2.Zero) continue;
                    points.Add(trailPositions[i]);
                }
                if (points.Count > 1)
                {
                    TrailRenderer.Draw(effect, points, t => 7f * (1f - t), t => Color.White * (1f - t), HealShaders.HealNoise);
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
                        float alpha = (1f - progress) * 0.7f;
                        float width = (1f - progress) * 6f + 1f;
                        Vector2 start = trailPositions[i] - Main.screenPosition;
                        Vector2 end = trailPositions[Math.Min(i + 1, trailPositions.Count - 1)] - Main.screenPosition;
                        float rotation = (end - start).ToRotation();
                        float length = Vector2.Distance(start, end);
                        if (length < 1) continue;
                        spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), baseColor * alpha * 0.4f, rotation, new Vector2(0, 0.5f), new Vector2(length, width * 2f), SpriteEffects.None, 0f);
                    }
                }
            }
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.5f) * 0.2f + 0.9f;
            spriteBatch.Draw(texture, drawPos, null, Color.White, Projectile.rotation, origin, pulse, SpriteEffects.None, 0f);
            return false;
        }
    }
    #endregion
    #region Diffusal Shockwave Ring (Expanding shockwave ring)
    public class DiffusalShockwaveRing : ModProjectile
    {
        public override string Texture => "jarkos/projectiles/melee/unique_melee/StormWave";
        private float MaxRadius = 160f;
        public const int LIFE = 14;
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
            int particleCount = (int)(3 * (1f - Progress));
            for (int i = 0; i < particleCount; i++)
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 pos = Projectile.Center + angle.ToRotationVector2() * Radius;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Color sparkColor = Main.rand.NextBool(2) ? new Color(0, 220, 255) : new Color(255, 30, 200);
                new Spark(pos, vel, sparkColor, 12, Main.rand.NextFloat(0.6f, 1.1f), 0.9f).Spawn();
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 targetCenter = targetHitbox.Center.ToVector2();
            float distance = Vector2.Distance(Projectile.Center, targetCenter);
            float r = Radius;
            float ringWidth = 24f;
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
                ringShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 4f);
                ringShader.Parameters["uColor"]?.SetValue(new Vector3(0.1f, 0.7f, 1.0f));
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
                Color fallbackColor = Color.Lerp(Color.Cyan, Color.Magenta, Progress);
                spriteBatch.Draw(tex, drawPos, null, fallbackColor * alpha * 0.8f, 0f, new Vector2(half, half), currentScale, SpriteEffects.None, 0f);
            }
            return false;
        }
    }
    #endregion
}
