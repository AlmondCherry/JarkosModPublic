using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.systems;
using jarkos.particles;
using ReLogic.Content;

namespace Jarkos.projectiles.mecha
{
	public class Plasma_proj : ModProjectile
	{
        private List<Vector2> oldPositions = new List<Vector2>();
        private List<float> oldRotations = new List<float>();
        private const int TrailLength = 25;
        private float pulseTimer = 0f;
        private bool hasExploded = false;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TrailLength;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 2; 
            Projectile.alpha = 0;
            Projectile.light = 0f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            pulseTimer++;
            Projectile.rotation = Projectile.velocity.ToRotation();

            
            oldPositions.Insert(0, Projectile.Center);
            oldRotations.Insert(0, Projectile.rotation);
            if (oldPositions.Count > TrailLength)
            {
                oldPositions.RemoveAt(oldPositions.Count - 1);
                oldRotations.RemoveAt(oldRotations.Count - 1);
            }

            float lightIntensity = 0.8f + MathF.Sin(pulseTimer * 0.2f) * 0.2f;
            Lighting.AddLight(Projectile.Center, 0f, lightIntensity * 1.2f, lightIntensity);
            SpawnParticles();
            AttractEnemies();

            if (pulseTimer % 20 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item15 with { Volume = 0.15f, Pitch = 0.5f }, Projectile.Center);
            }
        }

        private void SpawnParticles()
        {
            if (Main.rand.NextBool(3))
            {
                Vector2 sparkVel = -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1f, 1f);
                new Spark(Projectile.Center + Main.rand.NextVector2Circular(8, 8), sparkVel, new Color(0, 255, 180), 14, Main.rand.NextFloat(0.5f, 0.9f), 0.8f).Spawn();
            }
            if (Main.rand.NextBool(10))
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(1.5f, 1.5f);
                new Nova_particle(Projectile.Center, sparkVel, new Color(0, 255, 180), 12, Main.rand.NextFloat(0.4f, 0.7f), 0.7f).Spawn();
            }
        }

        private void AttractEnemies()
        {
            float attractRange = 150f;
            float attractForce = 0.8f;
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || npc.dontTakeDamage)
                    continue;
                float distance = Vector2.Distance(Projectile.Center, npc.Center);
                if (distance < attractRange && distance > 20)
                {
                    if (npc.knockBackResist > 0.3f)
                    {
                        Vector2 direction = Projectile.Center - npc.Center;
                        direction.Normalize();
                        float strength = (1f - distance / attractRange) * attractForce * npc.knockBackResist;
                        npc.velocity += direction * strength;
                        if (Main.rand.NextBool(8))
                        {
                            Vector2 dustPos = Vector2.Lerp(npc.Center, Projectile.Center, Main.rand.NextFloat());
                            Dust dust = Dust.NewDustPerfect(dustPos, DustID.GreenTorch, direction * 2f, 0, default, 0.6f);
                            dust.noGravity = true;
                        }
                    }
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            if (oldPositions.Count > 1)
            {
                
                oldPositions[0] = Projectile.Center;

                if (HealShaders.Stream != null && HealShaders.HealNoise != null)
                {
                    Effect effect = HealShaders.Stream;
                    spriteBatch.End();
                    effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 6f);
                    effect.Parameters["uOpacity"]?.SetValue(1.5f);
                    effect.Parameters["uColor"]?.SetValue(new Color(0, 255, 180).ToVector4());
                    effect.Parameters["uColorCore"]?.SetValue(new Color(200, 255, 240).ToVector4());
                    TrailRenderer.Draw(effect, oldPositions, t => 18f * MathF.Pow(1f - t, 0.4f), t => Color.White * (1f - t), HealShaders.HealNoise);
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }
            return true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item93 with { Pitch = 0.2f, Volume = 0.5f }, Projectile.Center);
            for (int i = 0; i < 12; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(6, 6);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Electric, vel, 0, default, 1.5f);
                dust.noGravity = true;
            }
            Explode();
        }

        public override void OnKill(int timeLeft)
        {
            Explode();
        }

        private void Explode()
        {
            if (hasExploded) return;
            hasExploded = true;

            SoundEngine.PlaySound(SoundID.Item94 with { Pitch = -0.3f, Volume = 0.7f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.5f, Volume = 0.4f }, Projectile.Center);
            if (Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<PlasmaWaveExplosion>(), Projectile.damage / 2, Projectile.knockBack * 2, Projectile.owner);
            }
            for (int i = 0; i < 16; i++)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(8f, 8f);
                new Spark(Projectile.Center, vel, new Color(0, 255, 180), 24, Main.rand.NextFloat(0.8f, 1.4f), 1f).Spawn();
            }
            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(10f, 10f);
                new Nova_particle(Projectile.Center, vel, new Color(0, 255, 180), 18, Main.rand.NextFloat(0.7f, 1.2f), 1f).Spawn();
            }
            Lighting.AddLight(Projectile.Center, 1f, 2f, 1.8f);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Explode();
            return true;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
	}

    #region Plasma Wave Explosion
    public class PlasmaWaveExplosion : ModProjectile
    {
        public override string Texture => "jarkos/projectiles/melee/unique_melee/StormWave";
        private float MaxRadius = 180f;
        private float ExpansionSpeed = 25f;
        public const int LIFE = 42;
        private static Asset<Effect> ringShader;
        private static bool shaderLoadAttempted;
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
            Projectile.ai[0] += ExpansionSpeed;
            ExpansionSpeed *= 0.96f;
            if (Projectile.timeLeft <= 0)
            {
                Projectile.Kill();
                return;
            }
            float intensity = 1f - Progress;
            Lighting.AddLight(Projectile.Center, 0f, intensity * 1.5f, intensity * 1.3f);
            SpawnRingParticles();
            int size = (int)(Radius * 2);
            Projectile.position = Projectile.Center - new Vector2(size / 2);
            Projectile.width = size;
            Projectile.height = size;
            if (Projectile.ai[1] == 0)
            {
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.4f, Volume = 0.6f }, Projectile.Center);
                Projectile.ai[1] = 1;
            }
        }

        private void SpawnRingParticles()
        {
            float progress = Progress;
            int particleCount = (int)(4 * (1f - progress));
            for (int i = 0; i < particleCount; i++)
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 pos = Projectile.Center + angle.ToRotationVector2() * Radius;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(1f, 3f);
                new Spark(pos, vel, new Color(0, 255, 180), 18, Main.rand.NextFloat(0.7f, 1.2f), 0.9f).Spawn();
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 targetCenter = targetHitbox.Center.ToVector2();
            float distance = Vector2.Distance(Projectile.Center, targetCenter);
            float r = Radius;
            float ringWidth = 36f;
            return distance <= r + ringWidth / 2 && distance >= r - ringWidth / 2;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Vector2 pushDir = target.Center - Projectile.Center;
            if (pushDir != Vector2.Zero)
            {
                pushDir.Normalize();
                target.velocity += pushDir * 8f * target.knockBackResist;
            }
            for (int i = 0; i < 5; i++)
            {
                new Spark(target.Center, Main.rand.NextVector2Circular(4f, 4f), new Color(0, 255, 180), 15, Main.rand.NextFloat(0.6f, 1f), 0.9f).Spawn();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            if (!shaderLoadAttempted)
            {
                shaderLoadAttempted = true;
                try
                {
                    ringShader = ModContent.Request<Effect>("jarkos/Shaders/SporeRing", AssetRequestMode.ImmediateLoad);
                }
                catch { ringShader = null; }
            }
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float half = tex.Width / 2f;
            float scale = MaxRadius / (half * 0.82f);
            float currentScale = scale * EaseOut(Progress);
            if (ringShader != null && ringShader.Value != null)
            {
                Effect fx = ringShader.Value;
                fx.Parameters["uProgress"]?.SetValue(Progress);
                fx.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 2f);
                fx.Parameters["uColor"]?.SetValue(new Vector3(0.0f, 1.0f, 0.7f));
                fx.Parameters["uSeed"]?.SetValue(Projectile.identity * 0.1f);
                fx.Parameters["uImage0"]?.SetValue(ModContent.Request<Texture2D>("jarkos/projectiles/melee/unique_melee/WaveNoise", AssetRequestMode.ImmediateLoad).Value);
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, fx, Main.GameViewMatrix.TransformationMatrix);
                spriteBatch.Draw(tex, drawPos, null, Color.White, 0f, new Vector2(half, half), currentScale, SpriteEffects.None, 0f);
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            else
            {
                float alpha = 1f - Progress;
                spriteBatch.Draw(tex, drawPos, null, new Color(0, 255, 180, 0) * alpha * 0.7f, 0f, new Vector2(half, half), currentScale, SpriteEffects.None, 0f);
            }
            return false;
        }
    }
    #endregion
}
