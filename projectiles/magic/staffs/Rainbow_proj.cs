using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using jarkos.systems;
using Terraria.GameContent;
using jarkos.particles;
using ReLogic.Content;

namespace jarkos.projectiles.magic.staffs
{
    public class Rainbow_proj : ModProjectile
    {
        private List<Vector2> trailPositions = new List<Vector2>();
        private const int MaxTrailLength = 30;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 40;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.light = 0.8f;
            Projectile.alpha = 255;
            Projectile.extraUpdates = 2;
        }

        public override void AI()
        {
            Projectile.ai[0] += 1f;

            float fadeIn = MathHelper.Clamp(Projectile.ai[0] / 5f, 0f, 1f);
            Projectile.Opacity = fadeIn;

            float waveAmplitude = 3.5f;
            float waveFrequency = 0.15f;
            Vector2 velocity = Projectile.velocity;
            Vector2 perpendicular = new Vector2(-velocity.Y, velocity.X);
            perpendicular.Normalize();

            float wave = (float)Math.Sin(Projectile.ai[0] * waveFrequency) * waveAmplitude;
            float prevWave = (float)Math.Sin((Projectile.ai[0] - 1f) * waveFrequency) * waveAmplitude;
            float waveDelta = wave - prevWave;

            Projectile.position += perpendicular * waveDelta;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Main.rand.NextBool(2))
            {
                Color dustColor = GetRainbowColor(Projectile.ai[0] * 0.05f) * fadeIn;
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.RainbowMk2, 0f, 0f, 100, dustColor, 0.8f * fadeIn);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity *= 0.3f;
                Main.dust[dust].scale = Main.rand.NextFloat(0.8f, 1.4f) * fadeIn;
            }

            Color lightColor = GetRainbowColor(Projectile.ai[0] * 0.03f);
            Lighting.AddLight(Projectile.Center, lightColor.ToVector3() * 0.6f * fadeIn);
        }

        public static Color GetRainbowColor(float phase)
        {
            float r = (float)(Math.Sin(phase * MathHelper.TwoPi) * 0.5f + 0.5f);
            float g = (float)(Math.Sin(phase * MathHelper.TwoPi + MathHelper.TwoPi / 3f) * 0.5f + 0.5f);
            float b = (float)(Math.Sin(phase * MathHelper.TwoPi + 2f * MathHelper.TwoPi / 3f) * 0.5f + 0.5f);
            return new Color(r, g, b);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float fadeIn = MathHelper.Clamp(Projectile.ai[0] / 40f, 0f, 1f);

            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D projTexture = TextureAssets.Projectile[Projectile.type].Value;
            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/particles/Circular_glow", AssetRequestMode.ImmediateLoad).Value;

            Vector2 velocity = Projectile.velocity;
            float speed = velocity.Length();
            if (speed < 0.1f)
                velocity = Vector2.UnitX;

            Vector2 direction = velocity.SafeNormalize(Vector2.UnitX);
            float rotation = direction.ToRotation();
            Vector2 headPos = Projectile.Center - Main.screenPosition;


            if (ShaderRainbowTrail.RainbowFireTrail?.Value != null)
            {
                Effect trailEffect = ShaderRainbowTrail.RainbowFireTrail.Value;
                Texture2D trailTexture = TextureAssets.Extra[197].Value;

                float trailLength = MathHelper.Clamp(speed * 12f, 15f, 120f);
                float trailWidth = MathHelper.Clamp(Projectile.width * 2.6f + speed * 1.2f, 18f, 42f);

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, trailEffect, Main.GameViewMatrix.TransformationMatrix);

                for (int layer = 0; layer < 3; layer++)
                {
                    float widthMult = layer == 0 ? 1.85f : (layer == 1 ? 1.15f : 0.55f);
                    float alphaMult = layer == 0 ? 0.32f : (layer == 1 ? 0.70f : 1.00f);
                    float intensityMult = layer == 0 ? 0.85f : (layer == 1 ? 1.20f : 1.65f);

                    Color rainbow = GetRainbowColor(Projectile.ai[0] * 0.035f + layer * 0.11f);
                    trailEffect.Parameters["uTime"].SetValue((float)Main.GlobalTimeWrappedHourly);
                    trailEffect.Parameters["uOpacity"].SetValue(alphaMult * fadeIn);
                    trailEffect.Parameters["uIntensity"].SetValue(intensityMult * fadeIn);
                    trailEffect.Parameters["uProgress"].SetValue(Projectile.ai[0] * 0.010f + layer * 0.17f);
                    trailEffect.Parameters["uColor"].SetValue(rainbow.ToVector3());
                    trailEffect.Parameters["uSecondaryColor"].SetValue(Color.OrangeRed.ToVector3());
                    trailEffect.Parameters["uDirection"].SetValue(direction);
                    trailEffect.Parameters["uImageOffset"].SetValue(new Vector2(speed, layer));
                    trailEffect.CurrentTechnique.Passes[0].Apply();

                    spriteBatch.Draw(trailTexture, headPos, trailTexture.Bounds, Color.White * fadeIn, rotation, new Vector2(trailTexture.Width, trailTexture.Height * 0.5f), new Vector2(trailLength / trailTexture.Width, trailWidth * widthMult / trailTexture.Height), SpriteEffects.None, 0f);
                }

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }


            if (glowTex != null && fadeIn > 0.01f)
            {
                Color rainbowGlow = GetRainbowColor(Projectile.ai[0] * 0.05f) * (0.6f * fadeIn);
                Vector2 glowOrigin = glowTex.Size() * 0.5f;

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                spriteBatch.Draw(glowTex, headPos, null, rainbowGlow, 0f, glowOrigin, 0.45f, SpriteEffects.None, 0f);

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }


            if (fadeIn > 0.01f)
            {
                Vector2 projOrigin = projTexture.Size() * 0.5f;
                Color projColor = GetRainbowColor(Projectile.ai[0] * 0.05f) * fadeIn;

                spriteBatch.Draw(projTexture, headPos, null, projColor, Projectile.rotation, projOrigin, Projectile.scale, SpriteEffects.None, 0f);
            }

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            int[] burnBuffs = new int[] {
                BuffID.OnFire, BuffID.OnFire3, BuffID.Frostburn, BuffID.Frostburn2, BuffID.CursedInferno, BuffID.ShadowFlame
            };
            int randomBuff = Main.rand.Next(burnBuffs);
            target.AddBuff(randomBuff, Main.rand.Next(240, 360));


            int electricCount = Main.rand.Next(3, 5);
            for (int i = 0; i < electricCount; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(7.5f, 7.5f);
                float phase = (float)i / electricCount + Main.rand.NextFloat(-0.1f, 0.1f);
                Color rainbowColor = GetRainbowColor(phase);
                float scale = Main.rand.NextFloat(0.25f, 0.5f);
                int lifetime = Main.rand.Next(20, 38);

                new Electric_particle(target.Center, vel, rainbowColor, lifetime, scale, 1f, 0.92f).Spawn();
            }

            for (int i = 0; i < 2; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(7f, 7f);
                float phase = (float)i / 15f + Projectile.ai[0] * 0.05f;
                Color particleColor = GetRainbowColor(phase);
                if (Main.rand.NextBool())
                {
                    new StarParticle(target.Center + Main.rand.NextVector2Circular(15f, 15f), vel, particleColor, Main.rand.Next(30, 45), Main.rand.NextFloat(0.5f, 0.9f), 0.8f, Main.rand.NextFloat(-0.2f, 0.2f)).Spawn();
                }
                else
                {
                    new StarParticleBigger(target.Center + Main.rand.NextVector2Circular(25f, 25f), vel * 0.8f, particleColor * 0.8f, Main.rand.Next(25, 40), Main.rand.NextFloat(0.3f, 0.6f), 0.7f, Main.rand.NextFloat(-0.1f, 0.1f)).Spawn();
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 20; i++)
            {
                float phase = (float)i / 20f;
                Color color = GetRainbowColor(phase);
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.RainbowMk2, 0f, 0f, 100, color, 1.2f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity = Main.rand.NextVector2Circular(5f, 5f);
                Main.dust[dust].scale = Main.rand.NextFloat(1f, 2f);
            }
            for (int i = 0; i < 8; i++)
            {
                float phase = (float)i / 8f + Projectile.ai[0] * 0.03f;
                Color color = GetRainbowColor(phase);
                int dust = Dust.NewDust(Projectile.Center, 0, 0, DustID.RainbowTorch, 0f, 0f, 0, color, 1.5f);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(3f, 8f);
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White * Projectile.Opacity;
        }
    }
}
