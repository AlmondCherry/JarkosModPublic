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
namespace Jarkos.projectiles.mecha
{
    public class Rollback_proj : ModProjectile
    {
        public override string Texture => "jarkos/projectiles/mecha/Astra_bomb_charged";
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.timeLeft = 2;
            Projectile.active = false;
        }
    }
    #region Rollback Channeled Laser
    public class Rollback_laser_proj : ModProjectile
    {
        public override string Texture => "jarkos/projectiles/mecha/Astra_bomb_charged";
        public List<Vector2> segmentPoints = new List<Vector2>();
        private bool pathCalculated = false;
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 15;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void AI()
        {
            Projectile.scale = Projectile.timeLeft / 15f;
            Vector2 muzzlePos = Projectile.Center;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.type == ModContent.ProjectileType<Rollback_Holdout>() && proj.owner == Projectile.owner)
                {
                    muzzlePos = proj.Center + proj.velocity * 128f + proj.velocity.RotatedBy(MathHelper.PiOver2) * 2f * proj.direction;
                    break;
                }
            }
            Projectile.Center = muzzlePos;
            if (!pathCalculated)
            {
                pathCalculated = true;
                List<Vector2> keyPoints = new List<Vector2>();
                Vector2 startPoint = muzzlePos;
                Vector2 currentDir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
                keyPoints.Add(startPoint);
                Vector2 currentPos = startPoint;
                HashSet<int> hitSpheres = new HashSet<int>();
                Projectile firstSphere = null;
                float closestRayDist = 1400f;
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if (proj.active && proj.type == ModContent.ProjectileType<Rollback_sphere_proj>() && proj.owner == Projectile.owner)
                    {
                        float t = Vector2.Dot(proj.Center - currentPos, currentDir);
                        if (t > 0 && t < closestRayDist)
                        {
                            Vector2 closestPt = currentPos + currentDir * t;
                            if (Vector2.Distance(proj.Center, closestPt) < 45f)
                            {
                                closestRayDist = t;
                                firstSphere = proj;
                            }
                        }
                    }
                }
                if (firstSphere != null)
                {
                    currentPos = firstSphere.Center;
                    keyPoints.Add(currentPos);
                    hitSpheres.Add(firstSphere.whoAmI);
                    if (Projectile.owner == Main.myPlayer && firstSphere.ai[0] == 0)
                    {
                        firstSphere.ai[0] = 1f;
                        firstSphere.netUpdate = true;
                    }
                    bool foundNext = true;
                    while (foundNext && hitSpheres.Count < 10)
                    {
                        foundNext = false;
                        Projectile nextSphere = null;
                        float nextDist = 500f;
                        for (int i = 0; i < Main.maxProjectiles; i++)
                        {
                            Projectile proj = Main.projectile[i];
                            if (proj.active && proj.type == ModContent.ProjectileType<Rollback_sphere_proj>() && !hitSpheres.Contains(proj.whoAmI) && proj.owner == Projectile.owner)
                            {
                                float d = Vector2.Distance(currentPos, proj.Center);
                                if (d < nextDist)
                                {
                                    nextDist = d;
                                    nextSphere = proj;
                                }
                            }
                        }
                        if (nextSphere != null)
                        {
                            currentDir = (nextSphere.Center - currentPos).SafeNormalize(Vector2.UnitX);
                            currentPos = nextSphere.Center;
                            keyPoints.Add(currentPos);
                            hitSpheres.Add(nextSphere.whoAmI);
                            foundNext = true;
                            if (Projectile.owner == Main.myPlayer && nextSphere.ai[0] == 0)
                            {
                                nextSphere.ai[0] = 1f;
                                nextSphere.netUpdate = true;
                            }
                        }
                    }
                    NPC closestNPC = null;
                    float npcDist = 1000f;
                    for (int n = 0; n < Main.maxNPCs; n++)
                    {
                        NPC npc = Main.npc[n];
                        if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.dontTakeDamage)
                        {
                            float d = Vector2.Distance(currentPos, npc.Center);
                            if (d < npcDist)
                            {
                                npcDist = d;
                                closestNPC = npc;
                            }
                        }
                    }
                    Vector2 targetDir = currentDir;
                    if (closestNPC != null)
                    {
                        targetDir = (closestNPC.Center - currentPos).SafeNormalize(Vector2.UnitX);
                    }
                    Vector2 option1 = currentDir.RotatedBy(MathHelper.PiOver2);
                    Vector2 option2 = currentDir.RotatedBy(-MathHelper.PiOver2);
                    currentDir = (Vector2.Dot(option1, targetDir) > Vector2.Dot(option2, targetDir)) ? option1 : option2;
                    keyPoints.Add(currentPos + currentDir * 1200f);
                }
                else
                {
                    keyPoints.Add(currentPos + currentDir * 2000f);
                }
                segmentPoints.Clear();
                if (keyPoints.Count > 0)
                {
                    segmentPoints.Add(keyPoints[0]);
                    for (int i = 0; i < keyPoints.Count - 1; i++)
                    {
                        Vector2 p1 = keyPoints[i];
                        Vector2 p2 = keyPoints[i + 1];
                        float dist = Vector2.Distance(p1, p2);
                        int segments = (int)Math.Max(1, dist / 50f);
                        for (int j = 1; j <= segments; j++)
                        {
                            segmentPoints.Add(Vector2.Lerp(p1, p2, j / (float)segments));
                        }
                    }
                }
            }
            else
            {
                if (segmentPoints.Count > 0)
                {
                    segmentPoints[0] = muzzlePos;
                }
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (segmentPoints.Count < 2) return false;
            for (int i = 0; i < segmentPoints.Count - 1; i++)
            {
                float collisionPoint = 0f;
                if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), segmentPoints[i], segmentPoints[i + 1], 18f * Projectile.scale, ref collisionPoint))
                {
                    return true;
                }
            }
            return false;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (segmentPoints.Count < 2) return false;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            SpriteBatch spriteBatch = Main.spriteBatch;
            Color baseColor = new Color(255, 60, 20);
            if (HealShaders.Stream != null && HealShaders.HealNoise != null)
            {
                Effect effect = HealShaders.Stream;
                spriteBatch.End();
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 6f + Projectile.identity * 0.1f);
                effect.Parameters["uOpacity"]?.SetValue(1.5f * Projectile.scale);
                effect.Parameters["uColor"]?.SetValue(baseColor.ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(new Color(255, 220, 180).ToVector4());
                TrailRenderer.Draw(effect, segmentPoints, t => {
                        float scale = Projectile.scale;
                        float baseWidth = 108f * scale;
                        if (t < 0.12f)
                        {
                            float ratio = t / 0.12f;
                            float smoothRatio = ratio * ratio * (3f - 2f * ratio);
                            return MathHelper.Lerp(24f * scale, baseWidth, smoothRatio);
                        }
                        else
                        {
                            float ratio = (t - 0.12f) / 0.88f;
                            return MathHelper.Lerp(baseWidth, baseWidth * 0.70f, ratio);
                        }
                    }, t => {
                        if (t < 0.05f)
                        {
                            return Color.White * (t / 0.05f);
                        }
                        return Color.White;
                    }, HealShaders.HealNoise);
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            else
            {
                for (int i = 0; i < segmentPoints.Count - 1; i++)
                {
                    float width = 46f * Projectile.scale;
                    Vector2 start = segmentPoints[i] - Main.screenPosition;
                    Vector2 end = segmentPoints[i + 1] - Main.screenPosition;
                    float rotation = (end - start).ToRotation();
                    float length = Vector2.Distance(start, end);
                    if (length < 1) continue;
                    spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), baseColor * 0.4f * Projectile.scale, rotation, new Vector2(0, 0.5f), new Vector2(length, width * 2.5f), SpriteEffects.None, 0f);
                    spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), Color.White * 0.9f * Projectile.scale, rotation, new Vector2(0, 0.5f), new Vector2(length, width * 0.6f), SpriteEffects.None, 0f);
                }
            }
            return false;
        }
    }
    #endregion
    #region Rollback Orange Sphere (RMB Sphere)
    public class Rollback_sphere_proj : ModProjectile
    {
        public override string Texture => "jarkos/projectiles/mecha/Astra_bomb_charged";
        private float auraVisualTimer = 0f;
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.light = 0.8f;
            Projectile.ignoreWater = true;
        }
        public override void AI()
        {
            Projectile.velocity *= 0.93f;
            if (Projectile.velocity.Length() < 0.05f)
            {
                Projectile.velocity = Vector2.Zero;
            }
            if (Projectile.ai[0] == 1f)
            {
                Projectile.scale = 2.5f;
                SoundEngine.PlaySound(SoundID.Item62 with { Pitch = -0.3f, Volume = 1.3f }, Projectile.Center);
                SoundEngine.PlaySound(SoundID.Item94 with { Pitch = -0.4f, Volume = 1.1f }, Projectile.Center);
                if (Projectile.owner == Main.myPlayer)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<Rollback_wave_ring>(), 300, 8f, Projectile.owner);
                }
                for (int i = 0; i < 18; i++)
                {
                    new Spark(Projectile.Center, Main.rand.NextVector2Circular(9f, 9f), new Color(255, 120, 20), 22, Main.rand.NextFloat(0.9f, 1.6f), 1f).Spawn();
                }
                Projectile.Kill();
                return;
            }
            Projectile.ai[1]++;
            if (Projectile.ai[1] >= 30)
            {
                Projectile.ai[1] = 0;
                float auraRadius = 100f;
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.dontTakeDamage)
                    {
                        if (Vector2.Distance(Projectile.Center, npc.Center) < auraRadius)
                        {
                            npc.SimpleStrikeNPC(5, 0, false, 0, DamageClass.Ranged);
                            for (int d = 0; d < 3; d++)
                            {
                                Dust dust = Dust.NewDustPerfect(npc.Center + Main.rand.NextVector2Circular(15f, 15f), DustID.OrangeTorch, Vector2.Zero, 0, default, 1.0f);
                                dust.noGravity = true;
                            }
                        }
                    }
                }
            }
            auraVisualTimer += 0.05f;
            if (Main.rand.NextBool(6))
            {
                float angle = Main.rand.NextFloat(0f, MathHelper.TwoPi);
                Vector2 offset = angle.ToRotationVector2() * 100f;
                Dust d = Dust.NewDustPerfect(Projectile.Center + offset, DustID.OrangeTorch, -angle.ToRotationVector2() * 0.6f, 100, default, 0.8f);
                d.noGravity = true;
            }
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.15f + 1.0f;
            Lighting.AddLight(Projectile.Center, 1.2f * pulse, 0.5f * pulse, 0.1f * pulse);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = texture.Size() / 2f;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.15f + 1.0f;
            float rotation = Main.GameUpdateCount * 0.06f;
            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/textures/Circular_glow", AssetRequestMode.ImmediateLoad).Value;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            Texture2D boundaryTex = ModContent.Request<Texture2D>("jarkos/particles/Accurate_circle_particle", AssetRequestMode.ImmediateLoad).Value;
            Vector2 boundaryOrigin = boundaryTex.Size() / 2f;
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            spriteBatch.Draw(glowTex, drawPos, null, new Color(255, 120, 20) * 0.45f * pulse, 0f, glowOrigin, pulse * 1.6f, SpriteEffects.None, 0f);
            for (int i = 0; i < 40; i++)
            {
                float angle = i * (MathHelper.TwoPi / 40f) + auraVisualTimer;
                Vector2 ringDrawPos = drawPos + angle.ToRotationVector2() * 100f;
                spriteBatch.Draw(boundaryTex, ringDrawPos, null, new Color(255, 140, 20) * 0.40f, 0f, boundaryOrigin, 0.12f, SpriteEffects.None, 0f);
            }
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            spriteBatch.Draw(texture, drawPos, null, new Color(255, 120, 20) * 0.8f, rotation, origin, pulse * 1.5f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, Color.White * 0.95f, -rotation * 1.3f, origin, pulse * 0.85f, SpriteEffects.None, 0f);
            return false;
        }
        public override bool? CanDamage() => false;
    }
    #endregion
    #region Rollback Wave Ring (Explosion wave ring)
    public class Rollback_wave_ring : ModProjectile
    {
        public override string Texture => "jarkos/projectiles/melee/unique_melee/StormWave";
        private float MaxRadius = 680f;
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
            int particleCount = (int)(4 * (1f - Progress));
            for (int i = 0; i < particleCount; i++)
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 pos = Projectile.Center + angle.ToRotationVector2() * Radius;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Color sparkColor = new Color(255, 120, 20);
                new Spark(pos, vel, sparkColor, 15, Main.rand.NextFloat(0.7f, 1.2f), 0.9f).Spawn();
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
                ringShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 4f);
                ringShader.Parameters["uColor"]?.SetValue(new Vector3(1.0f, 0.45f, 0.05f));
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
                spriteBatch.Draw(tex, drawPos, null, new Color(255, 120, 20) * alpha * 0.85f, 0f, new Vector2(half, half), currentScale, SpriteEffects.None, 0f);
            }
            return false;
        }
    }
    #endregion
}
