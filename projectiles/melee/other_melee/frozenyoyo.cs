using System;
using System.Collections.Generic;
using jarkos.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.other_melee
{
	public class frozenyoyo : ModProjectile
	{
		private struct WaveData
        {
            public Vector2 baseBottom;
            public float waveHeight;
            public float amplitude;
            public float frequency;
            public float phase;
            public float progress;
            public float opacity;
            public Color color;
            public int direction;
            public float speed;
        }
        private readonly List<WaveData> activeWaves = new();
        private int ambientTimer = 0;
        private int shardTimer = 0;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.YoyosLifeTimeMultiplier[Projectile.type] = 20f;
            ProjectileID.Sets.YoyosMaximumRange[Projectile.type] = 320f;
            ProjectileID.Sets.YoyosTopSpeed[Projectile.type] = 16f;
        }
        public override void SetDefaults()
        {
            Projectile.extraUpdates = 1;
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = 99;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.scale = 1f;
            Projectile.light = 0.5f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
        }
        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 0.15f, 0.6f, 1.0f);
            UpdateWaves();
            if (Projectile.owner == Main.myPlayer)
            {
                shardTimer++;
                if (shardTimer >= 70)
                {
                    shardTimer = 0;
                    float baseAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 vel = (baseAngle + i * MathHelper.PiOver2).ToRotationVector2() * 6.5f;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, vel, ModContent.ProjectileType<Frozen_yoyo_shard>(), (int)(Projectile.damage * 0.8f), Projectile.knockBack * 0.5f, Projectile.owner);
                    }
                }
            }
            if (Main.netMode != NetmodeID.Server)
            {
                SpawnAmbientParticles();
            }
        }
        private void UpdateWaves()
        {
            for (int i = activeWaves.Count - 1; i >= 0; i--)
            {
                WaveData w = activeWaves[i];
                w.progress += w.speed;
                w.opacity -= 0.012f;
                if (w.progress >= 1f || w.opacity <= 0f)
                {
                    activeWaves.RemoveAt(i);
                    continue;
                }
                activeWaves[i] = w;
            }
        }
        private void SpawnAmbientParticles()
        {
            ambientTimer++;
            if (ambientTimer % 3 == 0)
            {
                Vector2 offset = Main.rand.NextVector2Circular(12f, 12f);
                Vector2 vel = -Projectile.velocity * 0.05f + Main.rand.NextVector2Circular(0.5f, 0.5f);
                Color col = Color.Lerp(Color.Cyan, Color.DeepSkyBlue, Main.rand.NextFloat());
                new Solid_particle(Projectile.Center + offset, vel, col, 25 + Main.rand.Next(15), 0.4f + Main.rand.NextFloat(0.3f), 0.7f, Main.rand.NextFloat(-0.1f, 0.1f)).Spawn();
            }
            if (ambientTimer % 5 == 0 && Projectile.velocity.LengthSquared() > 1f)
            {
                Vector2 sparkVel = -Projectile.velocity.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 3f);
                sparkVel = sparkVel.RotatedByRandom(0.4);
                Color sparkCol = Color.Lerp(Color.DeepSkyBlue, Color.White, Main.rand.NextFloat(0.4f));
                new Spark(Projectile.Center, sparkVel, sparkCol, 20 + Main.rand.Next(10), 0.35f + Main.rand.NextFloat(0.2f), 0.8f).Spawn();
            }
            if (ambientTimer % 25 == 0)
            {
                new Circleedge(Projectile.Center, Vector2.Zero, Color.Cyan * 0.4f, 30, 0.1f, 0.35f, 0.02f).Spawn();
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn, 300);
            if (Main.netMode == NetmodeID.Server)
                return;
            int waveCount = 2 + Main.rand.Next(2);
            Vector2 bottomCenter = new Vector2(target.Center.X, target.position.Y + target.height);
            for (int i = 0; i < waveCount; i++)
            {
                float xOffset = Main.rand.NextFloat(-target.width * 0.35f, target.width * 0.35f);
                Color waveColor = Color.Lerp(Color.Cyan, Color.DeepSkyBlue, Main.rand.NextFloat());
                waveColor = Color.Lerp(waveColor, Color.White, Main.rand.NextFloat(0.2f));
                WaveData wave = new WaveData                {
                    baseBottom = bottomCenter + new Vector2(xOffset, 0f), waveHeight = target.height + 30f + Main.rand.NextFloat(30f), amplitude = 10f + Main.rand.NextFloat(10f), frequency = 0.07f + Main.rand.NextFloat(0.06f), phase = Main.rand.NextFloat(MathHelper.TwoPi), progress = 0f, opacity = 0.9f + Main.rand.NextFloat(0.1f), color = waveColor, direction = Main.rand.NextBool() ? -1 : 1, speed = 0.018f + Main.rand.NextFloat(0.01f)
                };
                activeWaves.Add(wave);
            }
            SpawnHitParticles(target);
        }
        private void SpawnHitParticles(NPC target)
        {
            int solidCount = 6 + Main.rand.Next(3);
            for (int i = 0; i < solidCount; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 4f);
                vel.Y -= Main.rand.NextFloat(1f, 3f);
                Color col = Color.Lerp(Color.Cyan, Color.White, Main.rand.NextFloat());
                new Solid_particle(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f), vel, col, 30 + Main.rand.Next(20), 0.5f + Main.rand.NextFloat(0.4f), 0.9f, Main.rand.NextFloat(-0.15f, 0.15f)).Spawn();
            }
            int sparkCount = 4 + Main.rand.Next(2);
            for (int i = 0; i < sparkCount; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2CircularEdge(4f, 4f);
                sparkVel.Y -= Main.rand.NextFloat(2f);
                Color sparkCol = Color.Lerp(Color.DeepSkyBlue, Color.White, Main.rand.NextFloat());
                new Spark(Projectile.Center, sparkVel, sparkCol, 25 + Main.rand.Next(15), 0.45f + Main.rand.NextFloat(0.3f), 1f).Spawn();
            }
            new Circleedge(Projectile.Center, Vector2.Zero, Color.Cyan * 0.6f, 35, 0.15f, 0.55f, 0.03f).Spawn();
            for (int i = 0; i < 8; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(5f, 5f);
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.IceTorch, dustVel.X, dustVel.Y, 100, default, 1.2f);
                d.noGravity = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (activeWaves.Count > 0)
            {
                DrawWaves();
            }
            return true;
        }
        private void DrawWaves()
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            foreach (WaveData wave in activeWaves)
            {
                DrawSingleWave(sb, pixel, wave);
            }
        }
        private void DrawSingleWave(SpriteBatch sb, Texture2D pixel, WaveData wave)
        {
            int segmentCount = 28;
            float visibleHeight = wave.waveHeight * wave.progress;
            float tailFade = MathHelper.Clamp((wave.progress - 0.2f) / 0.6f, 0f, 1f);
            for (int i = 0; i < segmentCount; i++)
            {
                float t = (float)i / (segmentCount - 1);
                float segY = t * wave.waveHeight;
                if (segY > visibleHeight)
                    continue;
                float taperBottom = MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp(t * 4f, 0f, 1f));
                float taperTop = MathHelper.SmoothStep(0f, 1f, MathHelper.Clamp((1f - t) * 4f, 0f, 1f));
                float taper = taperBottom * taperTop;
                float tailAlpha = 1f;
                if (tailFade > 0f)
                {
                    float fadeLine = tailFade * wave.waveHeight;
                    if (segY < fadeLine)
                    {
                        tailAlpha = MathHelper.Clamp((segY - fadeLine + 30f) / 30f, 0f, 1f);
                    }
                }
                float frontAlpha = MathHelper.Clamp((visibleHeight - segY) / 20f, 0f, 1f);
                float segAlpha = wave.opacity * taper * tailAlpha * frontAlpha;
                if (segAlpha <= 0.01f)
                    continue;
                float sinOffset = MathF.Sin(segY * wave.frequency + wave.phase + wave.progress * 8f) * wave.amplitude * wave.direction;
                Vector2 worldPos = new Vector2(wave.baseBottom.X + sinOffset, wave.baseBottom.Y - segY);
                Vector2 drawPos = worldPos - Main.screenPosition;
                float baseWidth = 3.5f * taper;
                float segHeight = wave.waveHeight / segmentCount;
                float rotation = 0f;
                if (i < segmentCount - 1)
                {
                    float nextSegY = (float)(i + 1) / (segmentCount - 1) * wave.waveHeight;
                    float nextSin = MathF.Sin(nextSegY * wave.frequency + wave.phase + wave.progress * 8f) * wave.amplitude * wave.direction;
                    Vector2 nextWorldPos = new Vector2(wave.baseBottom.X + nextSin, wave.baseBottom.Y - nextSegY);
                    Vector2 dir = nextWorldPos - worldPos;
                    rotation = dir.ToRotation();
                    segHeight = dir.Length();
                }
                else
                {
                    float prevSegY = (float)(i - 1) / (segmentCount - 1) * wave.waveHeight;
                    float prevSin = MathF.Sin(prevSegY * wave.frequency + wave.phase + wave.progress * 8f) * wave.amplitude * wave.direction;
                    Vector2 prevWorldPos = new Vector2(wave.baseBottom.X + prevSin, wave.baseBottom.Y - prevSegY);
                    Vector2 dir = worldPos - prevWorldPos;
                    rotation = dir.ToRotation();
                    segHeight = dir.Length();
                }
                Rectangle sourceRect = new Rectangle(0, 0, 1, 1);
                Vector2 origin = new Vector2(0f, 0.5f);
                float pulse = 1f + 0.15f * MathF.Sin(Main.GameUpdateCount * 0.2f + segY * 0.1f);
                Color glowColor = wave.color * segAlpha * 0.25f;
                sb.Draw(pixel, drawPos, sourceRect, glowColor, rotation, origin, new Vector2(segHeight + 1f, baseWidth * 5f * pulse), SpriteEffects.None, 0f);
                Color mainColor = wave.color * segAlpha * 0.7f;
                sb.Draw(pixel, drawPos, sourceRect, mainColor, rotation, origin, new Vector2(segHeight + 1f, baseWidth * 2f * pulse), SpriteEffects.None, 0f);
                Color coreColor = Color.Lerp(Color.White, Color.LightBlue, 0.3f) * segAlpha * 0.9f;
                sb.Draw(pixel, drawPos, sourceRect, coreColor, rotation, origin, new Vector2(segHeight + 1f, baseWidth * 0.7f * pulse), SpriteEffects.None, 0f);
            }
        }
        public override void PostAI()
        {
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.IceTorch);
                dust.noGravity = true;
                dust.scale = 0.8f;
                dust.velocity *= 0.3f;
            }
        }
	}
}
