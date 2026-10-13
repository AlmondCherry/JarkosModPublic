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
using jarkos.systems;
namespace Jarkos.projectiles.mecha
{
	public class Toxic_blast : ModProjectile
	{
		private const int TrailLength = 25;
        private float ColorVariant => Projectile.ai[0];
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TrailLength;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Default;
            Projectile.penetrate = 4;
            Projectile.timeLeft = 240;
            Projectile.alpha = 0;
            Projectile.extraUpdates = 3;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }
        public override void AI()
        {
            Projectile.velocity.Y += 0.12f;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            float pulse = 0.8f + (float)Math.Sin(Main.GameUpdateCount * 0.3f + Projectile.whoAmI) * 0.2f;
            Color lightColor = GetLaserColor();
            Lighting.AddLight(Projectile.Center, lightColor.R / 255f * pulse, lightColor.G / 255f * pulse, lightColor.B / 255f * pulse);
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position - Projectile.velocity * 2, Projectile.width, Projectile.height, DustID.CursedTorch, -Projectile.velocity.X * 0.2f, -Projectile.velocity.Y * 0.2f, 100, default, 1.4f);
                dust.noGravity = true;
                dust.fadeIn = 0.8f;
                dust.velocity *= 0.3f;
            }
            if (Main.rand.NextBool(6))
            {
                Dust Spark = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.GreenTorch, 0, 0, 0, default, 1.5f);
                Spark.noGravity = true;
                Spark.velocity = Main.rand.NextVector2Circular(3f, 3f);
            }
        }
        private Color GetLaserColor()
        {
            return ColorVariant switch
            {
                0 => new Color(80, 255, 100), 1 => new Color(120, 255, 80), 2 => new Color(60, 220, 130), _ => new Color(100, 255, 100)
            };
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            SpriteBatch spriteBatch = Main.spriteBatch;
            Color baseColor = GetLaserColor();
            Vector2[] positions = Projectile.oldPos;
            float[] rotations = Projectile.oldRot;
            if (HealShaders.Stream != null && HealShaders.HealNoise != null)
            {
                Effect effect = HealShaders.Stream;
                spriteBatch.End();
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 5.5f + Projectile.identity * 0.1f);
                effect.Parameters["uOpacity"]?.SetValue(1.3f);
                effect.Parameters["uColor"]?.SetValue(baseColor.ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(new Color(220, 255, 200).ToVector4());
                List<Vector2> points = new List<Vector2>();
                points.Add(Projectile.Center);
                for (int i = 0; i < Projectile.oldPos.Length; i++)
                {
                    if (Projectile.oldPos[i] == Vector2.Zero) continue;
                    points.Add(Projectile.oldPos[i] + Projectile.Size / 2f);
                }
                if (points.Count > 1)
                {
                    TrailRenderer.Draw(effect, points, t => 12f * (1f - t), t => Color.White * (1f - t), HealShaders.HealNoise);
                }
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float mainPulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.4f) * 0.2f;
            return true;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<ToxicRingExplosion>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
            }
            return true;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Venom, 180);
            target.AddBuff(BuffID.Poisoned, 360);
            SoundEngine.PlaySound(SoundID.Item17 with { Pitch = 0.6f, Volume = 0.5f }, target.Center);
            SoundEngine.PlaySound(SoundID.NPCHit18 with { Pitch = 0.8f, Volume = 0.3f }, target.Center);
            Color dustColor = GetLaserColor();
            for (int i = 0; i < 6; i++)
            {
                new Spark(target.Center, Main.rand.NextVector2Circular(5f, 5f), dustColor, 15, Main.rand.NextFloat(0.7f, 1.1f), 1f).Spawn();
            }
            if (Projectile.owner == Main.myPlayer)
            {
                int steamCount = Main.rand.Next(2, 4);
                for (int i = 0; i < steamCount; i++)
                {
                    Vector2 steamVel = new Vector2(Main.rand.NextFloat(-1.2f, 1.2f), Main.rand.NextFloat(-4.5f, -2.5f));
                    Projectile.NewProjectile(Projectile.GetSource_OnHit(target), target.Center + Main.rand.NextVector2Circular(20f, 20f), steamVel, ModContent.ProjectileType<ToxicSteamVisual>(), 0, 0f, Projectile.owner);
                }
            }
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item10 with { Pitch = 1f, Volume = 0.4f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item86 with { Pitch = 0.5f, Volume = 0.25f }, Projectile.Center);
            for (int i = 0; i < 20; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(7f, 7f);
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.CursedTorch, vel.X, vel.Y, 80, default, Main.rand.NextFloat(1.3f, 2f));
                dust.noGravity = true;
                dust.fadeIn = 1.5f;
            }
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
	}
	#region Toxic Cloud
	public class ToxicCloud : ModProjectile
    {
        private float SizeMultiplier => Projectile.ai[0] == 0 ? 1f : Projectile.ai[0];
        private float CurrentScale => Projectile.scale * SizeMultiplier;
        public override string Texture => "jarkos/projectiles/Empty_proj";
        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 150;
            Projectile.alpha = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.scale = 0.3f;
        }
        public override void AI()
        {
            if (Projectile.scale < 1f)
            {
                Projectile.scale += 0.05f;
            }
            if (Projectile.timeLeft < 60)
            {
                Projectile.alpha += 3;
            }
            if (Projectile.alpha >= 255)
            {
                Projectile.Kill();
                return;
            }
            Projectile.velocity.Y -= 0.01f;
            Projectile.velocity *= 0.98f;
            float intensity = (255 - Projectile.alpha) / 255f * 0.4f;
            Lighting.AddLight(Projectile.Center, 0.15f * intensity, 0.6f * intensity, 0.2f * intensity);
            Projectile.rotation += 0.015f * (Projectile.whoAmI % 2 == 0 ? 1 : -1);
            if (Main.rand.NextBool(5))
            {
                Vector2 offset = Main.rand.NextVector2Circular(40f * CurrentScale, 40f * CurrentScale);
                Dust dust = Dust.NewDustDirect(Projectile.Center + offset, 0, 0, DustID.Venom, Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-1.5f, -0.5f), 180, default, 0.7f);
                dust.noGravity = true;
                dust.fadeIn = 0.8f;
            }
            if (Main.rand.NextBool(12))
            {
                Vector2 offset = Main.rand.NextVector2Circular(30f * CurrentScale, 30f * CurrentScale);
                Dust Spark = Dust.NewDustDirect(Projectile.Center + offset, 0, 0, DustID.CursedTorch, 0, -1, 0, default, 0.8f);
                Spark.noGravity = true;
            }
            int newSize = (int)(80 * CurrentScale);
            Projectile.Resize(newSize, newSize);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Vector2 center = Projectile.Center - Main.screenPosition;
            float alphaFactor = (255 - Projectile.alpha) / 255f;
            float time = Main.GameUpdateCount * 0.02f;
            return true;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.rand.NextBool(3))
            {
                SoundEngine.PlaySound(SoundID.Item17 with { Pitch = 1f, Volume = 0.25f }, target.Center);
            }
            for (int i = 0; i < 6; i++)
            {
                Dust dust = Dust.NewDustDirect(target.Center, 0, 0, DustID.Venom, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f), 100, default, 1f);
                dust.noGravity = true;
            }
        }
        public override bool? CanDamage()
        {
            return Projectile.scale > 0.5f ? null : false;
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Venom, 0, 0, 150, default, 0.8f);
                dust.velocity = Main.rand.NextVector2Circular(2f, 2f);
                dust.noGravity = true;
                dust.fadeIn = 0.5f;
            }
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White * ((255 - Projectile.alpha) / 255f);
        }
    }
	#endregion
    #region Toxic Ring Explosion & Steam
    public class ToxicRingExplosion : ModProjectile
    {
        public override string Texture => "jarkos/projectiles/melee/unique_melee/StormWave";
        private float MaxRadius = 160f;
        public const int LIFE = 30;
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
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.7f, Volume = 0.5f }, Projectile.Center);
            }
            int particleCount = (int)(3 * (1f - Progress));
            for (int i = 0; i < particleCount; i++)
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 pos = Projectile.Center + angle.ToRotationVector2() * Radius;
                Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(0.5f, 2.5f);
                Color sparkColor = Main.rand.NextBool(2) ? new Color(80, 255, 100) : new Color(120, 255, 80);
                new Spark(pos, vel, sparkColor, 15, Main.rand.NextFloat(0.6f, 1f), 0.9f).Spawn();
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
                ringShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 2.5f);
                ringShader.Parameters["uColor"]?.SetValue(new Vector3(0.3f, 1.0f, 0.1f));
                ringShader.Parameters["uSeed"]?.SetValue(Projectile.identity * 0.13f);
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
                spriteBatch.Draw(tex, drawPos, null, new Color(40, 255, 70, 0) * alpha * 0.8f, 0f, new Vector2(half, half), currentScale, SpriteEffects.None, 0f);
            }
            return false;
        }
    }
    public class ToxicSteamVisual : ModProjectile
    {
        public override string Texture => "jarkos/projectiles/Empty_proj";
        private List<Vector2> trailPositions = new List<Vector2>();
        private const int MaxTrailLength = 30;
        private const int MaxLifeTime = 80;
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = MaxLifeTime;
            Projectile.ignoreWater = true;
        }
        public override void AI()
        {
            Projectile.velocity.Y -= 0.1f;
            Projectile.velocity.X += (float)Math.Sin(Main.GameUpdateCount * 0.1f + Projectile.identity) * 0.18f;
            Projectile.velocity *= 0.94f;
            if (Main.rand.NextBool(4))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(12f, 12f), DustID.CursedTorch, Projectile.velocity * 0.2f, 120, default, Main.rand.NextFloat(0.8f, 1.5f));
                d.noGravity = true;
            }
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > MaxTrailLength)
            {
                trailPositions.RemoveAt(trailPositions.Count - 1);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (trailPositions.Count < 2) return false;
            SpriteBatch spriteBatch = Main.spriteBatch;
            Effect effect = HealShaders.Stream;
            float progress = Projectile.timeLeft / (float)MaxLifeTime;
            if (effect != null && HealShaders.HealNoise != null)
            {
                spriteBatch.End();
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 4f);
                effect.Parameters["uOpacity"]?.SetValue(1.0f * progress);
                effect.Parameters["uColor"]?.SetValue(new Color(60, 255, 40).ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(new Color(200, 255, 200).ToVector4());
                TrailRenderer.Draw(effect, trailPositions, t => { 
                    float width = 20f * progress;
                        return width * (1f - t);
                    }, t => Color.White * (1f - t) * progress, HealShaders.HealNoise);
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            else
            {
                Texture2D pixel = TextureAssets.MagicPixel.Value;
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                for (int i = 0; i < trailPositions.Count - 1; i++)
                {
                    Vector2 start = trailPositions[i] - Main.screenPosition;
                    Vector2 end = trailPositions[i + 1] - Main.screenPosition;
                    Vector2 direction = end - start;
                    float length = direction.Length();
                    if (length < 0.5f) continue;
                    float rotation = direction.ToRotation();
                    float t = (float)i / trailPositions.Count;
                    Color color = new Color(60, 255, 40) * (1f - t) * progress * 0.5f;
                    color.A = 0;
                    float width = 20f * (1f - t) * progress;
                    spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), color, rotation, new Vector2(0f, 0.5f), new Vector2(length, width), SpriteEffects.None, 0f);
                }
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            return false;
        }
    }
    #endregion
}
