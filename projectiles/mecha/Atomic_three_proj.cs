using jarkos.particles;
using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Jarkos.projectiles.mecha
{
	public class Atomic_three_proj : ModProjectile
	{
        private List<Vector2> trailPositions = new List<Vector2>();
        private const int TrailLength = 15;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TrailLength;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.aiStyle = 0;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Default;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.alpha = 0;
            Projectile.light = 0.8f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 2;
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            UpdateTrail();
            float pulse = 0.8f + (float)Math.Sin(Projectile.timeLeft * 0.3f) * 0.2f;
            Lighting.AddLight(Projectile.Center, 0.3f * pulse, 1f * pulse, 0.2f * pulse);
            if (Main.rand.NextBool(6))
            {
                new Circleedge_reducing(Projectile.Center, Projectile.velocity * -0.2f + Main.rand.NextVector2Circular(1f, 1f), Color.LimeGreen, 12, Main.rand.NextFloat(0.2f, 0.4f), 1f).Spawn();
            }
            if (Main.rand.NextBool(8))
            {
                new Circleedge_reducing(Projectile.Center, Main.rand.NextVector2Circular(3f, 3f), new Color(150, 255, 150), 10, Main.rand.NextFloat(0.2f, 0.4f), 1f).Spawn();
            }
            if (Projectile.ai[0]++ % 10 == 0 && Main.rand.NextBool(6))
            {
                SpawnDecayParticle();
            }
        }
        private void UpdateTrail()
        {
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > TrailLength)
            {
                trailPositions.RemoveAt(trailPositions.Count - 1);
            }
        }
        private void SpawnDecayParticle()
        {
            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 velocity = Main.rand.NextVector2CircularEdge(2, 2);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<AtomicDecayParticle>(), Projectile.damage / 2, 0, Projectile.owner);
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item94 with { Volume = 0.6f, Pitch = 0.5f }, Projectile.Center);
            CreateHitEffect();
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.5f, Pitch = 0.8f }, Projectile.Center);
            if (Main.myPlayer == Projectile.owner)
            {
                int particleCount = Main.rand.Next(1, 2);
                for (int i = 0; i < particleCount; i++)
                {
                    float angle = MathHelper.TwoPi / particleCount * i + Main.rand.NextFloat(-0.3f, 0.3f);
                    Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(4, 8);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<AtomicDecayParticle>(), Projectile.damage / 2, Projectile.knockBack * 0.5f, Projectile.owner, 1);
                }
            }
            CreateExplosionEffect();
        }
        private void CreateHitEffect()
        {
            for (int i = 0; i < 2; i++)
            {
                new Circular_glow(Projectile.Center, Main.rand.NextVector2Circular(5f, 5f), Color.LimeGreen, 14, Main.rand.NextFloat(0.1f, 0.2f), 1f).Spawn();
            }
            Lighting.AddLight(Projectile.Center, 0.5f, 1.5f, 0.3f);
        }
        private void CreateExplosionEffect()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 2; i++)
                {
                    new Circular_glow(Projectile.Center, Main.rand.NextVector2Circular(8f, 8f), Color.LimeGreen, 20, Main.rand.NextFloat(0.1f, 0.2f), 1f).Spawn();
                }
                for (int i = 0; i < 2; i++)
                {
                    new Spark(Projectile.Center, Main.rand.NextVector2Circular(6f, 6f), new Color(150, 255, 150), 16, Main.rand.NextFloat(0.8f, 1.3f), 1f).Spawn();
                }
            }
            Lighting.AddLight(Projectile.Center, 1f, 2f, 0.5f);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            DrawTrail();
            DrawGlow();
            DrawProjectile();
            return false;
        }
        private void DrawTrail()
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            if (trailPositions.Count > 1 && HealShaders.Stream != null && HealShaders.HealNoise != null)
            {
                Effect effect = HealShaders.Stream;
                spriteBatch.End();
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 5f);
                effect.Parameters["uOpacity"]?.SetValue(1.5f);
                effect.Parameters["uColor"]?.SetValue(new Color(100, 255, 100).ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(new Color(200, 255, 200).ToVector4());
                TrailRenderer.Draw(effect, trailPositions, t => MathHelper.Lerp(12f, 0f, t), t => Color.White * (1f - t), HealShaders.HealNoise);
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }
        private void DrawGlow()
        {
            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/particles/Circular_glow").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.2f;
            SpriteBatch spriteBatch = Main.spriteBatch;
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            spriteBatch.Draw(glowTex, drawPos, null, new Color(50, 200, 50) * 0.5f * pulse, 0f, glowTex.Size() / 2f, 0.4f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, new Color(150, 255, 150) * 0.8f * pulse, 0f, glowTex.Size() / 2f, 0.2f * pulse, SpriteEffects.None, 0f);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        private void DrawProjectile()
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Rectangle frame = texture.Bounds;
            Vector2 origin = frame.Size() / 2;
            Color drawColor = new Color(255, 255, 255, 200);
            Main.EntitySpriteDraw(texture, drawPos, frame, drawColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(150, 255, 150, 150);
        }
	}
    #region Decay Projectile
    public class AtomicDecayParticle : ModProjectile
    {
        public override string Texture => "jarkos/projectiles/mecha/Astra_bomb_charged";
        private List<Vector2> trailPositions = new List<Vector2>();
        private const int TrailLength = 10;
        private bool IsDecayFragment => Projectile.ai[0] == 1;
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.aiStyle = 0;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 60;
            Projectile.alpha = 100;
            Projectile.light = 0.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.extraUpdates = 1;
        }
        public override void AI()
        {
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > TrailLength)
            {
                trailPositions.RemoveAt(trailPositions.Count - 1);
            }
            Projectile.rotation += 0.3f;
            if (IsDecayFragment)
            {
                Projectile.velocity *= 0.97f;
                Projectile.velocity.Y += 0.1f;
            }
            else
            {
                Projectile.velocity += Main.rand.NextVector2Circular(0.2f, 0.2f);
            }
            float pulse = 0.6f + (float)Math.Sin(Projectile.timeLeft * 0.5f) * 0.4f;
            Lighting.AddLight(Projectile.Center, 0.2f * pulse, 0.6f * pulse, 0.1f * pulse);
            if (Main.rand.NextBool(6))
            {
                new Circular_glow(Projectile.Center, Projectile.velocity * 0.3f + Main.rand.NextVector2Circular(1f, 1f), Color.LimeGreen, 10, Main.rand.NextFloat(0.1f, 0.2f), 1f).Spawn();
            }
            Projectile.alpha = (int)MathHelper.Lerp(100, 255, 1f - (Projectile.timeLeft / 60f));
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.3f, Pitch = 1.2f }, Projectile.Center);
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.3f, Pitch = 0.9f }, Projectile.Center);
            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < 2; i++)
                {
                    new Circular_glow(Projectile.Center, Main.rand.NextVector2Circular(4f, 4f), Color.LimeGreen, 12, Main.rand.NextFloat(0.1f, 0.2f), 1f).Spawn();
                }
            }
            Lighting.AddLight(Projectile.Center, 0.4f, 1f, 0.2f);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            DrawTrail();
            DrawParticle();
            return false;
        }
        private void DrawTrail()
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            if (trailPositions.Count > 1 && HealShaders.Stream != null && HealShaders.HealNoise != null)
            {
                Effect effect = HealShaders.Stream;
                spriteBatch.End();
                float alphaMultiplier = 1f - (Projectile.alpha / 255f);
                effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 4f);
                effect.Parameters["uOpacity"]?.SetValue(1.5f * alphaMultiplier);
                effect.Parameters["uColor"]?.SetValue(new Color(150, 255, 100).ToVector4());
                effect.Parameters["uColorCore"]?.SetValue(new Color(255, 255, 150).ToVector4());
                TrailRenderer.Draw(effect, trailPositions, t => MathHelper.Lerp(6f, 0f, t), t => Color.White * (1f - t) * alphaMultiplier, HealShaders.HealNoise);
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }
        private void DrawParticle()
        {
            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/particles/Circular_glow").Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.4f) * 0.3f;
            float alphaMultiplier = 1f - (Projectile.alpha / 255f);
            SpriteBatch spriteBatch = Main.spriteBatch;
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            spriteBatch.Draw(glowTex, drawPos, null, new Color(100, 255, 50) * 0.6f * alphaMultiplier * pulse, 0f, glowTex.Size() / 2f, 0.25f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTex, drawPos, null, new Color(200, 255, 150) * alphaMultiplier * pulse, 0f, glowTex.Size() / 2f, 0.12f * pulse, SpriteEffects.None, 0f);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(150, 255, 100, 100) * (1f - Projectile.alpha / 255f);
        }
    }
    #endregion
}
