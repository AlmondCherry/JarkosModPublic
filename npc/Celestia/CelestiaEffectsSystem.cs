using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace jarkos.npc.Celestia
{
    public class CelestiaEffectsSystem : ModSystem
    {
        public const string FilterName = "Jarkos:CelestiaPhase2";
        public const string SkyName = "Jarkos:CelestiaSky";
        public const string BlueTintName = "Jarkos:CelestiaBlueTint";

        private const float MaxTintStrength = 0.45f;

        public static int Phase1KeepAlive;
        public static float Phase1Intensity;

        public static int Phase2KeepAlive;
        public static Vector2 BossCenter;
        public static bool ExplosionActive;

        private static float tintFade;
        private static Texture2D gradientTexture;

        private struct SkyStar
        {
            public Vector2 BasePosition;
            public Vector2 Velocity;
            public float Scale;
            public float ParallaxFactor;
            public float Alpha;
            public float TwinkleSpeed;
            public float TwinklePhase;
        }

        private static SkyStar[] stars;
        private static float timeAccumulator;

        private struct BottomGlow
        {
            public float XPercent;
            public float WidthScale;
            public float HeightScale;
            public float PulseSpeed;
            public float PulsePhase;
            public float MaxAlpha;
            public Color GlowColor;
        }

        private static BottomGlow[] bottomGlows;

        private static void EnsureBottomGlows()
        {
            if (bottomGlows != null) return;

            bottomGlows = new BottomGlow[8];
            UnifiedRandom rand = new UnifiedRandom(777);

            Color[] bluePalette = new Color[]
            {
                new Color(40, 140, 255),
                new Color(80, 180, 255),
                new Color(20, 100, 230),
                new Color(130, 210, 255)
            };

            for (int i = 0; i < bottomGlows.Length; i++)
            {
                bottomGlows[i] = new BottomGlow
                {
                    XPercent = (i + 0.5f) / bottomGlows.Length + rand.NextFloat(-0.06f, 0.06f),
                    WidthScale = rand.NextFloat(3.2f, 6.2f),
                    HeightScale = rand.NextFloat(0.45f, 0.95f),
                    PulseSpeed = rand.NextFloat(0.35f, 1.1f),
                    PulsePhase = rand.NextFloat(0f, MathHelper.TwoPi),
                    MaxAlpha = rand.NextFloat(0.22f, 0.48f),
                    GlowColor = bluePalette[i % bluePalette.Length]
                };
            }
        }

        public override void Load()
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            Asset<Effect> transition = ModContent.Request<Effect>("jarkos/Effects/CelestiaPhaseTransition", AssetRequestMode.ImmediateLoad);
            Filters.Scene[FilterName] = new Filter(new ScreenShaderData(transition, "CelestiaPhasePass"), EffectPriority.VeryHigh);

            Asset<Effect> tint = ModContent.Request<Effect>("jarkos/Effects/CelestiaBlueTint", AssetRequestMode.ImmediateLoad);
            Filters.Scene[BlueTintName] = new Filter(new ScreenShaderData(tint, "CelestiaBlueTintPass"), EffectPriority.High);

            SkyManager.Instance[SkyName] = new CelestiaSky();

            On_Main.DrawSurfaceBG += ReplaceVanillaBackground;
        }

        private static void EnsureGradientTexture()
        {
            if (gradientTexture == null || gradientTexture.IsDisposed)
            {
                int height = 64;
                gradientTexture = new Texture2D(Main.graphics.GraphicsDevice, 1, height);
                Color[] data = new Color[height];

                for (int y = 0; y < height; y++)
                {
                    float t = (float)y / (height - 1);
                    Color topColor = new Color(3, 5, 18);
                    Color bottomColor = new Color(12, 35, 75);
                    data[y] = Color.Lerp(topColor, bottomColor, MathF.Pow(t, 1.2f));
                }
                gradientTexture.SetData(data);
            }
        }

        private static void EnsureStars()
        {
            if (stars != null) return;

            stars = new SkyStar[260];
            UnifiedRandom rand = new UnifiedRandom(1337);

            for (int i = 0; i < stars.Length; i++)
            {
                float scale = rand.NextFloat(1.0f, 3.5f);
                float t = (scale - 1.0f) / 2.5f;
                float parallax = MathHelper.Lerp(0.65f, 0.08f, t);

                stars[i] = new SkyStar
                {
                    BasePosition = new Vector2(rand.NextFloat(0f, 3200f), rand.NextFloat(0f, 2200f)),
                    Velocity = rand.NextVector2Circular(0.12f, 0.12f),
                    Scale = scale,
                    ParallaxFactor = parallax,
                    Alpha = rand.NextFloat(0.5f, 1.0f),
                    TwinkleSpeed = rand.NextFloat(1.5f, 4.5f),
                    TwinklePhase = rand.NextFloat(0f, MathHelper.TwoPi)
                };
            }
        }

        private static Asset<Texture2D> softGradientTex;

        private static void EnsureSoftGradientTex()
        {
            if (softGradientTex == null)
            {
                softGradientTex = ModContent.Request<Texture2D>("jarkos/textures/soft_gradient");
            }
        }

        private void ReplaceVanillaBackground(On_Main.orig_DrawSurfaceBG orig, Main self)
        {
            float p2Intensity = CelestiaSky.CurrentIntensity;
            float bgIntensity = MathHelper.Clamp(Phase1Intensity + p2Intensity, 0f, 1f);

            if (!ExplosionActive && bgIntensity <= 0.01f)
            {
                orig(self);
                return;
            }

            Main.spriteBatch.End();

            EnsureGradientTexture();
            EnsureStars();
            EnsureBottomGlows();
            EnsureSoftGradientTex();

            timeAccumulator += 0.016f;

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);

            Rectangle screenRect = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
            Main.spriteBatch.Draw(gradientTexture, screenRect, Color.White * bgIntensity);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);

            Texture2D bloomTex = softGradientTex.Value;
            Vector2 bloomOrigin = new Vector2(bloomTex.Width * 0.5f, bloomTex.Height * 0.85f);

            for (int i = 0; i < bottomGlows.Length; i++)
            {
                ref BottomGlow glow = ref bottomGlows[i];

                float sine = (float)Math.Sin(timeAccumulator * glow.PulseSpeed + glow.PulsePhase);
                float fade = MathHelper.Clamp(sine * 0.5f + 0.5f, 0f, 1f);
                fade = fade * fade * (3f - 2f * fade);

                float alpha = glow.MaxAlpha * fade * bgIntensity;
                if (alpha <= 0.005f) continue;

                float posX = (glow.XPercent * Main.screenWidth) - (Main.screenPosition.X * 0.03f % (Main.screenWidth * 0.25f));
                posX = ((posX % Main.screenWidth) + Main.screenWidth) % Main.screenWidth;

                float posY = Main.screenHeight + (bloomTex.Height * 0.25f * glow.HeightScale);

                Vector2 drawPos = new Vector2(posX, posY);
                Vector2 scale = new Vector2(glow.WidthScale, glow.HeightScale);

                Main.spriteBatch.Draw(bloomTex, drawPos, null, glow.GlowColor * alpha, 0f, bloomOrigin, scale, SpriteEffects.None, 0f);
            }

            Main.spriteBatch.End();

            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);

            float margin = 120f;
            float viewW = Main.screenWidth + margin * 2f;
            float viewH = Main.screenHeight + margin * 2f;
            Texture2D starTex = TextureAssets.MagicPixel.Value;
            Rectangle srcPixel = new Rectangle(0, 0, 1, 1);

            for (int i = 0; i < stars.Length; i++)
            {
                ref SkyStar star = ref stars[i];
                Vector2 starWorldOffset = star.BasePosition + star.Velocity * (timeAccumulator * 60f) - Main.screenPosition * star.ParallaxFactor;

                float drawX = ((starWorldOffset.X % viewW) + viewW) % viewW - margin;
                float drawY = ((starWorldOffset.Y % viewH) + viewH) % viewH - margin;

                float twinkle = 0.65f + 0.35f * MathF.Sin(timeAccumulator * star.TwinkleSpeed + star.TwinklePhase);
                float starAlpha = star.Alpha * twinkle * bgIntensity;

                int starSize = Math.Max(1, (int)MathF.Round(star.Scale));
                Rectangle destRect = new Rectangle((int)drawX, (int)drawY, starSize, starSize);

                Main.spriteBatch.Draw(starTex, destRect, srcPixel, Color.White * starAlpha);
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.BackgroundViewMatrix.EffectMatrix);
        }

        public override void PostUpdateEverything()
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            if (Phase1KeepAlive > 0)
            {
                Phase1KeepAlive--;
                Phase1Intensity = MathHelper.Clamp(Phase1Intensity + 0.035f, 0f, 1f);
            }
            else
            {
                Phase1Intensity = MathHelper.Clamp(Phase1Intensity - 0.02f, 0f, 1f);
            }

            CelestiaSky.Phase1Intensity = Phase1Intensity;

            if (Phase2KeepAlive > 0)
                Phase2KeepAlive--;

            bool p1Active = Phase1KeepAlive > 0 || Phase1Intensity > 0.01f;
            bool p2Active = Phase2KeepAlive > 0;

            if (p1Active || p2Active)
            {
                if (!SkyManager.Instance[SkyName].IsActive())
                    SkyManager.Instance.Activate(SkyName, BossCenter);
            }
            else
            {
                if (SkyManager.Instance[SkyName].IsActive())
                    SkyManager.Instance.Deactivate(SkyName);
            }

            if (p2Active)
            {
                ExplosionActive = true;

                if (!Filters.Scene[BlueTintName].IsActive())
                    Filters.Scene.Activate(BlueTintName, BossCenter);

                tintFade = MathHelper.Clamp(tintFade + 0.015f, 0f, MaxTintStrength);
            }
            else
            {
                tintFade = MathHelper.Clamp(tintFade - 0.01f, 0f, MaxTintStrength);

                if (tintFade <= 0f && Filters.Scene[BlueTintName].IsActive())
                    Filters.Scene.Deactivate(BlueTintName);

                if (tintFade <= 0f && CelestiaSky.CurrentIntensity <= 0.01f)
                    ExplosionActive = false;
            }

            if (Filters.Scene[BlueTintName].IsActive())
            {
                Filters.Scene[BlueTintName].GetShader()
                    .UseIntensity(tintFade)
                    .UseTargetPosition(BossCenter);
            }
        }
    }
}
