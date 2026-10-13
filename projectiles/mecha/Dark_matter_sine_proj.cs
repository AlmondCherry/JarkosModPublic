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

namespace Jarkos.projectiles.mecha
{
    public class Dark_matter_sine_proj : ModProjectile
    {
        public override string Texture => "jarkos/projectiles/mecha/Dark_matter_proj";
        private List<Vector2> trailPositions = new List<Vector2>();
        private const int MaxTrailLength = 40;
        private Vector2 virtualPos = Vector2.Zero;
        private bool hasInitialized = false;
        public float SineTime
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        public float AmplitudePhase
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 40;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 90;
            Projectile.alpha = 0;
            Projectile.light = 0.8f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 2;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }
        public override void AI()
        {
            if (!hasInitialized)
            {
                hasInitialized = true;
                virtualPos = Projectile.Center;
            }
            virtualPos += Projectile.velocity;
            SineTime += 0.16f;
            Vector2 perp = new Vector2(-Projectile.velocity.Y, Projectile.velocity.X).SafeNormalize(Vector2.Zero);
            float amplitude = 32f * AmplitudePhase;
            Projectile.Center = virtualPos + perp * MathF.Sin(SineTime) * amplitude;
            Projectile.rotation = Projectile.velocity.ToRotation()  + MathHelper.PiOver2;
            trailPositions.Insert(0, Projectile.Center);
            if (trailPositions.Count > MaxTrailLength)
            {
                trailPositions.RemoveAt(trailPositions.Count - 1);
            }
            float pulse = 0.4f + 0.2f * (float)Math.Sin(Main.GameUpdateCount * 0.3f);
            Lighting.AddLight(Projectile.Center, 0.2f * pulse, 0.8f * pulse, 1.0f * pulse);
            if (Main.rand.NextBool(10))
            {
                new Spark(Projectile.Center, Main.rand.NextVector2Circular(2f, 2f), new Color(100, 220, 255), 14, Main.rand.NextFloat(0.5f, 0.9f), 0.8f).Spawn();
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.Item93 with { Volume = 0.7f, Pitch = 0.6f }, Projectile.Center);
            for (int i = 0; i < 6; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(5f, 5f);
                new Spark(target.Center, sparkVel, new Color(120, 220, 255), 20, Main.rand.NextFloat(0.7f, 1.2f), 1f).Spawn();
            }
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item62 with { Volume = 0.5f, Pitch = 1.3f }, Projectile.Center);
            for (int i = 0; i < 12; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(7f, 7f);
                new Spark(Projectile.Center, sparkVel, new Color(100, 200, 255), 25, Main.rand.NextFloat(0.8f, 1.4f), 1f).Spawn();
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            SpriteBatch spriteBatch = Main.spriteBatch;
            if (trailPositions.Count > 1)
            {
                if (HealShaders.Stream != null && HealShaders.HealNoise != null)
                {
                    Effect effect = HealShaders.Stream;
                    spriteBatch.End();
                    effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 6f);
                    effect.Parameters["uOpacity"]?.SetValue(1.6f);
                    effect.Parameters["uColor"]?.SetValue(new Color(0, 130, 255).ToVector4());
                    effect.Parameters["uColorCore"]?.SetValue(new Color(200, 240, 255).ToVector4());
                    TrailRenderer.Draw(effect, trailPositions, t => 18f * MathF.Pow(1f - t, 0.35f), t => Color.White * (1f - t), HealShaders.HealNoise);
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                }
                else
                {
                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                    for (int i = 0; i < trailPositions.Count - 1; i++)
                    {
                        Vector2 start = trailPositions[i] - Main.screenPosition;
                        Vector2 end = trailPositions[i + 1] - Main.screenPosition;
                        Vector2 direction = end - start;
                        float distance = direction.Length();
                        float rotation = direction.ToRotation();
                        float progress = (float)i / trailPositions.Count;
                        float alpha = (1f - progress) * 0.7f;
                        float thickness = (1f - progress) * 8f;
                        Color color = Color.Lerp(new Color(100, 200, 255) * alpha, new Color(30, 80, 180) * alpha * 0.5f, progress);
                        spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), color, rotation, new Vector2(0, 0.5f), new Vector2(distance, thickness), SpriteEffects.None, 0f);
                    }
                    spriteBatch.End();
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                }
            }
            return true;
        }
    }
}
