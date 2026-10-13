using System;
using jarkos.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.melee.true_melee
{
    public class Pyromelt_nuclear_orb : ModProjectile
    {
        public override string Texture => "jarkos/textures/Circle_slashglow";

        private Player Owner => Main.player[Projectile.owner];
        private ref float Timer => ref Projectile.ai[0];

        private const float CHARGE_TIME = 45f;
        private const float BLAST_TIME = 90f;
        private const float TOTAL_TIME = 135f;

        public static readonly Color ColorCore = new(255, 250, 220);
        public static readonly Color ColorGold = new(255, 200, 40);
        public static readonly Color ColorFire = new(255, 120, 15);
        public static readonly Color ColorDeep = new(200, 40, 5);

        private float orbScale = 0.1f;
        private bool exploded = false;

        private static Effect vortexShader;
        private static Effect orbShader;
        private static Effect ringShader;
        private static bool shadersTried = false;

        private static void LoadShaders()
        {
            if (shadersTried || Main.dedServ) return;
            shadersTried = true;

            try { vortexShader = ModContent.Request<Effect>("jarkos/Shaders/PortalVortex", AssetRequestMode.ImmediateLoad).Value; } catch {}
            try { orbShader = ModContent.Request<Effect>("jarkos/Shaders/SporeOrb", AssetRequestMode.ImmediateLoad).Value; } catch {}
            try { ringShader = ModContent.Request<Effect>("jarkos/Shaders/SporeRing", AssetRequestMode.ImmediateLoad).Value; } catch {}
        }

        public override void SetDefaults()
        {
            Projectile.width = 120;
            Projectile.height = 120;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = (int)TOTAL_TIME;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override bool? CanDamage()
        {
            if (Timer < CHARGE_TIME) return false;
            return base.CanDamage();
        }

        public override void AI()
        {
            Timer++;
            Lighting.AddLight(Projectile.Center, 3.5f, 2.0f, 0.4f);

            if (Timer < CHARGE_TIME)
            {
                
                float progress = Timer / CHARGE_TIME;
                orbScale = MathHelper.SmoothStep(0.2f, 1.8f, progress) * (1f + 0.15f * (float)Math.Sin(Timer * 0.4f));

                
                for (int i = 0; i < 3; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    float dist = Main.rand.NextFloat(60f, 180f) * (1f - progress * 0.4f);
                    Vector2 spawnPos = Projectile.Center + angle.ToRotationVector2() * dist;
                    Vector2 vel = (Projectile.Center - spawnPos) * 0.08f;

                    new Spark_gravit(spawnPos, vel, Main.rand.NextBool(2) ? ColorGold : ColorFire, 180, Main.rand.NextFloat(0.7f, 1.2f), 1f, 0f).Spawn();
                }

                if (Main.rand.NextBool(2))
                {
                    Vector2 dustVel = Main.rand.NextVector2Circular(4f, 4f);
                    Dust d = Dust.NewDustDirect(Projectile.Center - new Vector2(10), 20, 20, DustID.SolarFlare, dustVel.X, dustVel.Y, 100, default, 2f);
                    d.noGravity = true;
                }

                
                if (Timer % 6 == 0)
                {
                    Main.instance.CameraModifiers.Add(new PunchCameraModifier(Projectile.Center, Main.rand.NextVector2Circular(1f, 1f), 2f * progress, 3f, 4, 1000f, FullName));
                    SoundEngine.PlaySound(SoundID.DD2_BetsyWindAttack with { Pitch = -0.6f + progress * 0.8f, Volume = 0.6f + progress * 0.6f }, Projectile.Center);
                }
            }
            else if (Timer >= CHARGE_TIME && !exploded)
            {
                
                exploded = true;

                
                Main.instance.CameraModifiers.Add(new PunchCameraModifier(Projectile.Center, Main.rand.NextVector2Circular(1f, 1f), 32f, 12f, 30, 2000f, FullName));

                
                SoundEngine.PlaySound(SoundID.Item14 with { Volume = 2.0f, Pitch = -0.3f }, Projectile.Center);
                SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Volume = 1.8f, Pitch = -0.4f }, Projectile.Center);
                SoundEngine.PlaySound(SoundID.Item74 with { Volume = 1.5f, Pitch = 0.1f }, Projectile.Center);

                
                new Circleedge(Projectile.Center, Vector2.Zero, ColorCore, 35, 0.4f, 2.5f).Spawn();
                new Circleedge(Projectile.Center, Vector2.Zero, ColorGold, 30, 0.6f, 2.0f).Spawn();
                new Circleedge(Projectile.Center, Vector2.Zero, ColorFire, 25, 0.8f, 1.6f).Spawn();
                new Circular_glow(Projectile.Center, Vector2.Zero, ColorGold, 25, 0.9f, 1.8f).Spawn();

                
                for (int i = 0; i < 40; i++)
                {
                    Vector2 dir = Main.rand.NextVector2CircularEdge(1f, 1f);
                    float speed = Main.rand.NextFloat(8f, 24f);
                    new Spark_gravit(Projectile.Center, dir * speed, Main.rand.NextBool(3) ? ColorCore : ColorGold, 300, Main.rand.NextFloat(1.2f, 2.2f), 1f, 0.1f).Spawn();
                }

                for (int i = 0; i < 20; i++)
                {
                    Vector2 dir = Main.rand.NextVector2Circular(1f, 1f);
                    float speed = Main.rand.NextFloat(12f, 28f);
                    new Stretching_particle_out(Projectile.Center, dir * speed, Main.rand.NextBool(2) ? ColorCore : ColorFire, 30, Main.rand.NextFloat(60f, 120f), Main.rand.NextFloat(6f, 12f), 0.02f).Spawn();
                }

                
                for (int i = 0; i < 50; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(16f, 16f);
                    Dust d = Dust.NewDustDirect(Projectile.Center - new Vector2(16), 32, 32, DustID.SolarFlare, vel.X, vel.Y, 100, default, 3f);
                    d.noGravity = true;
                    d.fadeIn = 2.5f;
                }
                for (int i = 0; i < 60; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(20f, 20f);
                    Dust d = Dust.NewDustDirect(Projectile.Center - new Vector2(16), 32, 32, DustID.Torch, vel.X, vel.Y, 80, default, 2.5f);
                    d.noGravity = true;
                }
            }
            else
            {
                
                float blastProgress = (Timer - CHARGE_TIME) / BLAST_TIME;
                orbScale = 1.8f + 1.2f * blastProgress;
                if (Main.rand.NextBool(2))
                {
                    Vector2 vel = Main.rand.NextVector2Circular(10f, 10f);
                    Dust d = Dust.NewDustDirect(Projectile.Center - new Vector2(16), 32, 32, DustID.Torch, vel.X, vel.Y, 100, default, 2f);
                    d.noGravity = true;
                }
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (Timer < CHARGE_TIME) return false;
            float blastRadius = 1180f;
            return Vector2.Distance(targetHitbox.Center.ToVector2(), Projectile.Center) <= blastRadius;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            LoadShaders();

            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 texOrigin = texture.Size() / 2f;

            if (Timer < CHARGE_TIME)
            {
                
                float progress = Timer / CHARGE_TIME;
                float pulse = 1f + 0.15f * (float)Math.Sin(Timer * 0.5f);

                
                if (vortexShader != null)
                {
                    vortexShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 4f);
                    vortexShader.Parameters["uColor"]?.SetValue(new Vector3(1.0f, 0.45f, 0.05f)); 
                    vortexShader.Parameters["uSecondaryColor"]?.SetValue(new Vector3(1.0f, 0.95f, 0.4f)); 
                    vortexShader.Parameters["uOpacity"]?.SetValue(progress * 0.95f);
                    vortexShader.Parameters["uSaturation"]?.SetValue(1.0f);
                    vortexShader.Parameters["uIntensity"]?.SetValue(2.2f);
                    vortexShader.Parameters["uProgress"]?.SetValue(0.2f);

                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, vortexShader, Main.GameViewMatrix.TransformationMatrix);
                    Main.spriteBatch.Draw(texture, drawPos, null, Color.White * progress, 0f, texOrigin, orbScale * pulse * 2.5f, SpriteEffects.None, 0f);
                    Main.spriteBatch.End();
                    RestartSpriteBatch();
                }

                
                if (orbShader != null)
                {
                    orbShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 5f);
                    orbShader.Parameters["uSeed"]?.SetValue((float)Projectile.whoAmI * 0.17f);
                    orbShader.Parameters["uColor"]?.SetValue(new Vector3(1.0f, 0.55f, 0.08f)); 
                    orbShader.Parameters["uUnstable"]?.SetValue(1.8f);

                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, orbShader, Main.GameViewMatrix.TransformationMatrix);
                    Main.spriteBatch.Draw(texture, drawPos, null, Color.White * progress, Timer * 0.05f, texOrigin, orbScale * pulse * 1.8f, SpriteEffects.None, 0f);
                    Main.spriteBatch.End();
                    RestartSpriteBatch();
                }

                
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Color glowColor = ColorFire * (0.85f * progress);
                Main.spriteBatch.Draw(texture, drawPos, null, glowColor, Timer * 0.05f, texOrigin, orbScale * pulse * 1.5f, SpriteEffects.None, 0f);

                Color goldColor = ColorGold * (0.95f * progress);
                Main.spriteBatch.Draw(texture, drawPos, null, goldColor, -Timer * 0.08f, texOrigin, orbScale * pulse * 1.2f, SpriteEffects.None, 0f);

                Color coreColor = ColorCore * (1.0f * progress);
                Main.spriteBatch.Draw(texture, drawPos, null, coreColor, Timer * 0.12f, texOrigin, orbScale * pulse * 0.8f, SpriteEffects.None, 0f);

                Main.spriteBatch.End();
                RestartSpriteBatch();
            }
            else
            {
                
                float blastProgress = (Timer - CHARGE_TIME) / BLAST_TIME;
                float alpha = MathHelper.Clamp(1f - blastProgress, 0f, 1f);

                
                if (ringShader != null && alpha > 0.01f)
                {
                    ringShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 6f);
                    ringShader.Parameters["uProgress"]?.SetValue(blastProgress);
                    ringShader.Parameters["uColor"]?.SetValue(new Vector3(1.0f, 0.55f, 0.08f));
                    ringShader.Parameters["uSeed"]?.SetValue((float)Projectile.whoAmI * 0.1f);

                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, ringShader, Main.GameViewMatrix.TransformationMatrix);
                    Main.spriteBatch.Draw(texture, drawPos, null, Color.White * alpha, 0f, texOrigin, orbScale * 3.2f, SpriteEffects.None, 0f);
                    Main.spriteBatch.End();
                    RestartSpriteBatch();
                }

                
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Main.spriteBatch.Draw(texture, drawPos, null, ColorFire * (alpha * 0.9f), Timer * 0.1f, texOrigin, orbScale * 2.2f, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(texture, drawPos, null, ColorGold * (alpha * 0.95f), -Timer * 0.15f, texOrigin, orbScale * 1.7f, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(texture, drawPos, null, ColorCore * (alpha * 1.0f), Timer * 0.2f, texOrigin, orbScale * 1.1f, SpriteEffects.None, 0f);

                Main.spriteBatch.End();
                RestartSpriteBatch();

                
                if (blastProgress < 0.65f)
                {
                    float flashAlpha = (1f - blastProgress / 0.65f);
                    flashAlpha = flashAlpha * flashAlpha; 

                    Texture2D pixel = TextureAssets.MagicPixel.Value;
                    Color flashColor = Color.Lerp(Color.White, ColorGold, (1f - flashAlpha) * 0.4f);

                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
                    Main.spriteBatch.Draw(pixel, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), flashColor * (flashAlpha * 0.99f));
                    Main.spriteBatch.End();
                    RestartSpriteBatch();
                }
            }

            return false;
        }

        private static void RestartSpriteBatch()
        {
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
