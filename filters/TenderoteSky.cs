using Jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
namespace Jarkos.filters
{
    public class TenderoteSky : CustomSky
    {
        private const string EffectPath = "Jarkos/Shaders/TenderoteShaderSky";
        private bool skyActive;
        private float opacity;
        private float time;
        private Effect skyEffect;
        public override void OnLoad()
        {
            if (!Main.dedServ)
            {
                skyEffect = ModContent.Request<Effect>(EffectPath, AssetRequestMode.ImmediateLoad).Value;
            }
        }
        public override void Update(GameTime gameTime)
        {
            jarkos.Jarkos_config config = jarkos.Jarkos_config.Instance;
            if (Main.gameMenu || !config.EnableBossBackground)
            {
                skyActive = false;
                opacity = 0f;

                TenderodScreenSystem.TargetDarken = 0f;
                TenderodScreenSystem.DarkenIntensity = 0f;
                TenderodScreenSystem.DarknessRadius = 0f;
                return;
            }
            else
            {
                skyActive = TenderodScreenSystem.DarkenIntensity > 0.01f;
            }
            float targetOpacity = (TenderodScreenSystem.TargetDarken > 0.01f) ? 1f : 0f;
            if (targetOpacity > 0.5f) {
                opacity = MathHelper.Lerp(opacity, targetOpacity, 0.08f);
            } else {
                opacity = MathHelper.Lerp(opacity, targetOpacity, 0.015f);
            }
            if (opacity < 0.01f && !skyActive)
            {
                opacity = 0f;
            }
            time += (float)gameTime.ElapsedGameTime.TotalSeconds;
        }
        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (skyEffect == null || opacity <= 0f)
            {
                return;
            }
            jarkos.Jarkos_config config = jarkos.Jarkos_config.Instance;
            if (!config.EnableBossBackground)
                return;
            if (maxDepth >= float.MaxValue && minDepth < float.MaxValue)
            {
                Vector2 skyOffset = new Vector2(Main.screenPosition.X, Main.screenPosition.Y);
                skyEffect.Parameters["uScreenResolution"]?.SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
                skyEffect.Parameters["uSkyOffset"]?.SetValue(skyOffset);
                skyEffect.Parameters["uTime"]?.SetValue(time);
                skyEffect.Parameters["uOpacity"]?.SetValue(opacity);
                skyEffect.Parameters["uIntensity"]?.SetValue(1f);
                skyEffect.Parameters["uRadius"]?.SetValue(TenderodScreenSystem.DarkenIntensity * 2f);
                skyEffect.Parameters["uDarknessCenter"]?.SetValue(TenderodScreenSystem.DarknessCenter);
                skyEffect.Parameters["uDarknessRadius"]?.SetValue(TenderodScreenSystem.DarknessRadius);

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, skyEffect, Main.GameViewMatrix.TransformationMatrix);
                
                spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
        }
        public override bool IsActive()
        {
            return skyActive || opacity > 0f;
        }
        public override void Activate(Vector2 position, params object[] args)
        {
            skyActive = true;
        }
        public override void Deactivate(params object[] args)
        {
            skyActive = false;
        }
        public override void Reset()
        {
            skyActive = false;
            opacity = 0f;
            time = 0f;
        }
        public override float GetCloudAlpha()
        {
            return 1f - opacity;
        }
        public override Color OnTileColor(Color inColor)
        {
            return Color.Lerp(inColor, new Color(255, 138, 42), opacity * 0.16f);
        }
    }
    public class TenderoteSkySystem : ModSystem
    {
        public const string SkyKey = "Jarkos:TenderoteSky";
        public override void Load()
        {
            if (!Main.dedServ)
            {
                SkyManager.Instance.Bind(SkyKey, new TenderoteSky());
            }
        }
        public override void Unload()
        {
            if (!Main.dedServ)
            {
                SkyManager.Instance.Deactivate(SkyKey);
            }
        }
    }
    public class TenderoteEmptyBackgroundStyle : ModSurfaceBackgroundStyle
    {
        public override void ModifyFarFades(float[] fades, float transitionSpeed)
        {
            for (int i = 0; i < fades.Length; i++)
            {
                if (i == Slot)
                {
                    fades[i] += transitionSpeed;
                    if (fades[i] > 1f)
                    {
                        fades[i] = 1f;
                    }
                }
                else
                {
                    fades[i] -= transitionSpeed;
                    if (fades[i] < 0f)
                    {
                        fades[i] = 0f;
                    }
                }
            }
        }
        public override int ChooseFarTexture()
        {
            return -1;
        }
        public override int ChooseMiddleTexture()
        {
            return -1;
        }
        public override int ChooseCloseTexture(ref float scale, ref double parallax, ref float a, ref float b)
        {
            return -1;
        }
        public override bool PreDrawCloseBackground(SpriteBatch spriteBatch)
        {
            return false;
        }
    }
}
