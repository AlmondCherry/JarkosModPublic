using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;
using jarkos.items.usable;
using jarkos.UI.Common;

namespace jarkos.cradle_biome.system
{
    public class CradleStormSystem : ModSystem
    {
        private class WormCloud
        {
            public Vector2 HeadPosition;
            public float SpeedX;
            public float BaseY;
            public float SineSpeed;
            public float SineAmp;
            public float SinePhase;
            public int SegmentCount;
            public float MaxScale;
            public float Opacity;
            public Color CloudColor;
        }

        private class LightParticle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Scale;
            public float Opacity;
            public float Life;
            public float MaxLife;
            public Color Color;
        }

        private static readonly List<WormCloud> _wormClouds = new();
        private static readonly List<LightParticle> _lightParticles = new();
        private static float _biomeAlpha = 0f;
        private static Texture2D _cloudTexture;

        private static Texture2D CloudTexture
        {
            get
            {
                if (_cloudTexture == null || _cloudTexture.IsDisposed)
                {
                    int size = 128;
                    _cloudTexture = new Texture2D(Main.graphics.GraphicsDevice, size, size);
                    Color[] data = new Color[size * size];
                    float center = size / 2f;

                    for (int y = 0; y < size; y++)
                    {
                        for (int x = 0; x < size; x++)
                        {
                            float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                            float normDist = dist / center;
                            if (normDist <= 1f)
                            {
                                float alpha = 1f - normDist;
                                alpha = alpha * alpha * (3f - 2f * alpha);
                                byte a = (byte)(alpha * 255f);
                                data[y * size + x] = new Color(a, a, a, a);
                            }
                            else
                            {
                                data[y * size + x] = Color.Transparent;
                            }
                        }
                    }
                    _cloudTexture.SetData(data);
                }
                return _cloudTexture;
            }
        }

        public static bool HasLanternActive(Player player)
        {
            if (player == null || !player.active) return false;

            int lanternType = ModContent.ItemType<CradleLanternItem>();

            if (player.HeldItem != null && player.HeldItem.type == lanternType)
                return true;

            for (int i = 3; i < 10; i++)
            {
                if (player.armor[i] != null && player.armor[i].type == lanternType)
                    return true;
            }

            return false;
        }

        public override void PostUpdatePlayers()
        {
            if (Main.dedServ) return;
            Player player = Main.LocalPlayer;

            bool inCradle = player != null && player.active && player.InModBiome<PetrifiedAbyssBiome>();

            if (inCradle)
            {
                _biomeAlpha = Math.Min(1f, _biomeAlpha + 0.03f);
            }
            else
            {
                _biomeAlpha = Math.Max(0f, _biomeAlpha - 0.03f);
            }

            if (_biomeAlpha <= 0.001f)
            {
                _wormClouds.Clear();
                _lightParticles.Clear();
                return;
            }

            EnsureUniformClouds();

            for (int i = _wormClouds.Count - 1; i >= 0; i--)
            {
                var worm = _wormClouds[i];
                worm.HeadPosition.X -= worm.SpeedX;
                worm.HeadPosition.Y = worm.BaseY + MathF.Sin((Main.GameUpdateCount * worm.SineSpeed) + worm.SinePhase) * worm.SineAmp;

                float totalLength = worm.SegmentCount * 14f * (worm.MaxScale * 0.5f);
                if (worm.HeadPosition.X + totalLength < -300)
                {
                    worm.HeadPosition.X = Main.screenWidth + Main.rand.NextFloat(100, 400);
                    worm.BaseY = Main.rand.NextFloat(-80, Main.screenHeight + 80);
                    worm.SinePhase = Main.rand.NextFloat(MathHelper.TwoPi);
                    worm.MaxScale = Main.rand.NextFloat(1.5f, 6.0f);
                    worm.SegmentCount = Main.rand.Next(25, 60);
                    worm.CloudColor = Color.Black;
                }
            }

            bool holdingLantern = HasLanternActive(player);
            float baseLightRadius = holdingLantern ? 1.4f : 0.6f;

            Lighting.AddLight(player.Center, 1.2f * baseLightRadius, 0.95f * baseLightRadius, 0.5f * baseLightRadius);

            if (holdingLantern)
            {
                Vector2 coneOriginWorld = player.Center;
                Vector2 mouseDir = Main.MouseWorld - player.Center;
                float coneAngle = mouseDir.ToRotation();
                float coneSpread = MathHelper.ToRadians(68f);
                float coneLength = 800f;

                int numRays = 11;
                int stepsPerRay = 14;
                for (int r = 0; r < numRays; r++)
                {
                    float progress = r / (float)(numRays - 1);
                    float angle = coneAngle - (coneSpread / 2f) + progress * coneSpread;
                    Vector2 rayDir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));

                    for (int s = 1; s <= stepsPerRay; s++)
                    {
                        float stepDist = s / (float)stepsPerRay * coneLength;
                        Vector2 lightPos = coneOriginWorld + rayDir * stepDist;

                        float lightIntensity = 1.25f - (s / (float)stepsPerRay * 0.45f);
                        Lighting.AddLight(lightPos, 1.3f * lightIntensity, 1.05f * lightIntensity, 0.6f * lightIntensity);
                    }
                }

                if (_biomeAlpha > 0.1f && Main.rand.NextBool(2))
                {
                    Vector2 coneOriginScreen = player.Center - Main.screenPosition;
                    float angleOffset = Main.rand.NextFloat(-0.55f, 0.55f);
                    float pAngle = coneAngle + angleOffset;
                    float speed = Main.rand.NextFloat(3.5f, 9.5f);
                    Vector2 vel = new Vector2(MathF.Cos(pAngle), MathF.Sin(pAngle)) * speed;

                    _lightParticles.Add(new LightParticle
                    {
                        Position = coneOriginScreen + vel * 2f,
                        Velocity = vel,
                        Scale = Main.rand.NextFloat(0.4f, 1.1f),
                        Opacity = 0.9f,
                        Life = 0f,
                        MaxLife = Main.rand.NextFloat(30, 60),
                        Color = Color.Lerp(Color.Gold, Color.Orange, Main.rand.NextFloat())
                    });
                }
            }

            for (int i = _lightParticles.Count - 1; i >= 0; i--)
            {
                var lp = _lightParticles[i];
                lp.Life++;
                lp.Position += lp.Velocity;
                lp.Velocity *= 0.96f;
                lp.Opacity = MathHelper.Clamp(1f - (lp.Life / lp.MaxLife), 0f, 1f);

                if (lp.Life >= lp.MaxLife)
                {
                    _lightParticles.RemoveAt(i);
                }
            }
        }

        private static void EnsureUniformClouds()
        {
            int targetCount = 48;
            if (_wormClouds.Count >= targetCount) return;

            int cols = 8;
            int rows = 6;
            float colWidth = (Main.screenWidth + 600f) / cols;
            float rowHeight = (Main.screenHeight + 160f) / rows;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (_wormClouds.Count >= targetCount) break;

                    float startX = -300f + c * colWidth + Main.rand.NextFloat(-40f, 40f);
                    float startY = -80f + r * rowHeight + Main.rand.NextFloat(-25f, 25f);

                    _wormClouds.Add(new WormCloud
                    {
                        HeadPosition = new Vector2(startX, startY),
                        SpeedX = Main.rand.NextFloat(4.0f, 8.5f),
                        BaseY = startY,
                        SineSpeed = Main.rand.NextFloat(0.035f, 0.085f),
                        SineAmp = Main.rand.NextFloat(22f, 65f),
                        SinePhase = Main.rand.NextFloat(MathHelper.TwoPi),
                        SegmentCount = Main.rand.Next(25, 60),
                        MaxScale = Main.rand.NextFloat(1.5f, 6.0f),
                        Opacity = Main.rand.NextFloat(0.08f, 0.16f),
                        CloudColor = Color.Black
                    });
                }
            }
        }

        public static float GetLightConeIntensity(Vector2 pos, Vector2 coneOrigin, float coneAngle, float coneSpread, float coneLength, bool holdingLantern)
        {
            float distToPlayer = Vector2.Distance(pos, coneOrigin);

            float radialRadius = holdingLantern ? 300f : 150f;
            float radialIntensity = 0f;
            if (distToPlayer < radialRadius)
            {
                float rFactor = 1f - MathHelper.Clamp((distToPlayer - (radialRadius - 80f)) / 80f, 0f, 1f);
                radialIntensity = rFactor * rFactor * (3f - 2f * rFactor);
            }

            float coneIntensity = 0f;
            if (holdingLantern && distToPlayer <= coneLength)
            {
                float angleToPos = MathF.Atan2(pos.Y - coneOrigin.Y, pos.X - coneOrigin.X);
                float diff = angleToPos - coneAngle;

                while (diff > MathF.PI) diff -= MathHelper.TwoPi;
                while (diff < -MathF.PI) diff += MathHelper.TwoPi;

                float angleDiff = MathF.Abs(diff);
                float halfSpread = coneSpread / 2f;
                float featherAngle = MathHelper.ToRadians(22f);

                if (angleDiff <= halfSpread + featherAngle)
                {
                    float angleFactor = 1f - MathHelper.Clamp((angleDiff - (halfSpread - featherAngle)) / (featherAngle * 2f), 0f, 1f);
                    float distFactor = 1f - MathHelper.Clamp((distToPlayer - (coneLength - 220f)) / 220f, 0f, 1f);
                    float rawCone = angleFactor * distFactor;
                    coneIntensity = rawCone * rawCone * (3f - 2f * rawCone);
                }
            }

            return Math.Max(radialIntensity, coneIntensity);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            if (Main.dedServ || Main.gameMenu || _biomeAlpha <= 0.001f) return;

            int targetIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
            if (targetIndex == -1)
            {
                targetIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
            }

            if (targetIndex != -1)
            {
                layers.Insert(targetIndex, new LegacyGameInterfaceLayer(
                    "jarkos:CradleFogLayer",
                    delegate
                    {
                        DrawCradleFog(Main.spriteBatch);
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }

        private void DrawCradleFog(SpriteBatch sb)
        {
            Player player = Main.LocalPlayer;
            if (player == null || !player.active) return;

            bool holdingLantern = HasLanternActive(player);

            Vector2 coneOrigin = player.Center - Main.screenPosition;
            Vector2 mouseDir = Main.MouseWorld - player.Center;
            float coneAngle = mouseDir.ToRotation();
            float coneSpread = MathHelper.ToRadians(68f);
            float coneLength = 850f;

            Texture2D cloudTex = CloudTexture;

            float playerConeLight = GetLightConeIntensity(coneOrigin, coneOrigin, coneAngle, coneSpread, coneLength, holdingLantern);
            float darkAlpha = 0.20f * (1f - playerConeLight * 0.85f) * _biomeAlpha;

            Rectangle fullScreen = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
            sb.Draw(TextureAssets.MagicPixel.Value, fullScreen, new Color(5, 4, 12) * darkAlpha);

            Draw.RadialGlow(sb, new Vector2(0, 0), 650f, new Color(3, 2, 8) * (darkAlpha * 0.95f));
            Draw.RadialGlow(sb, new Vector2(Main.screenWidth, 0), 650f, new Color(3, 2, 8) * (darkAlpha * 0.95f));
            Draw.RadialGlow(sb, new Vector2(0, Main.screenHeight), 650f, new Color(3, 2, 8) * (darkAlpha * 0.95f));
            Draw.RadialGlow(sb, new Vector2(Main.screenWidth, Main.screenHeight), 650f, new Color(3, 2, 8) * (darkAlpha * 0.95f));

            foreach (var worm in _wormClouds)
            {
                float stepDist = 14f * (worm.MaxScale * 0.42f);
                for (int s = 0; s < worm.SegmentCount; s++)
                {
                    float progress = s / (float)(worm.SegmentCount - 1);

                    float segX = worm.HeadPosition.X + (s * stepDist);
                    float segY = worm.BaseY + MathF.Sin((Main.GameUpdateCount * worm.SineSpeed) + worm.SinePhase + (s * 0.22f)) * worm.SineAmp;
                    Vector2 segPos = new Vector2(segX, segY);

                    float doublePointedFactor = MathF.Sin(progress * MathF.PI);
                    float segScale = worm.MaxScale * MathF.Pow(doublePointedFactor, 0.65f);

                    if (segScale <= 0.05f) continue;

                    float segConeLight = GetLightConeIntensity(segPos, coneOrigin, coneAngle, coneSpread, coneLength, holdingLantern);
                    float segAlpha = worm.Opacity * (1f - segConeLight * 1.0f) * _biomeAlpha;

                    if (segAlpha <= 0.01f) continue;

                    Color segColor = worm.CloudColor * segAlpha;
                    sb.Draw(cloudTex, segPos, null, segColor, 0f, cloudTex.Size() / 2f, segScale, SpriteEffects.None, 0f);
                }
            }

            if (holdingLantern)
            {
                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);

                Draw.RadialGlow(sb, coneOrigin, 140f, Color.Gold * (0.85f * _biomeAlpha));

                foreach (var lp in _lightParticles)
                {
                    float pConeLight = GetLightConeIntensity(lp.Position, coneOrigin, coneAngle, coneSpread, coneLength, holdingLantern);
                    Color pColor = lp.Color * (lp.Opacity * pConeLight * _biomeAlpha);
                    Draw.RadialGlow(sb, lp.Position, 22f * lp.Scale, pColor);
                }

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
            }
        }
    }
}
