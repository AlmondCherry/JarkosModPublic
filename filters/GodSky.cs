using jarkos.npc.godofdeath;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace Jarkos.filters
{
    public class GodSky : CustomSky
    {
        private bool skyActive;
        private float intensity = 0.4f;
        private float _opacity;
        private float _time;

        private Asset<Effect> backgroundShader;

        public override void Update(GameTime gameTime)
        {
            jarkos.Jarkos_config config = jarkos.Jarkos_config.Instance;
            if (skyActive && config.EnableBossBackground)
            {
                if (_opacity < 1f)
                    _opacity += 0.02f;
            }
            else
            {
                if (_opacity > 0f)
                    _opacity -= 0.02f;
            }
            _opacity = MathHelper.Clamp(_opacity, 0f, 1f);

            _time += (float)gameTime.ElapsedGameTime.TotalSeconds * 0.16f;
        }

        public override void OnLoad()
        {
            if (Main.dedServ)
                return;
            backgroundShader = ModContent.Request<Effect>("Jarkos/npc/godofdeath/Effects/GodBossBackground", AssetRequestMode.ImmediateLoad);
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            float randomAhhVariable = 3.40282347E+38f;
            if (maxDepth >= randomAhhVariable && minDepth < randomAhhVariable)
            {
                if (backgroundShader == null)
                    return;
                jarkos.Jarkos_config config = jarkos.Jarkos_config.Instance;
                if (!config.EnableBossBackground)
                    return;

                Effect shader = backgroundShader.Value;
                shader.Parameters["uResolution"]?.SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
                shader.Parameters["iTime"]?.SetValue(_time);
                shader.Parameters["uOpacity"]?.SetValue(intensity * _opacity);

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, shader, Matrix.Identity);

                spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.Black * (0.65f * _opacity));
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
            }
        }

        public override float GetCloudAlpha()
        {
            return 1f - _opacity;
        }

        public override bool IsActive()
        {
            return skyActive || _opacity > 0.001f;
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
        }

        public float Opacity => _opacity;
    }

    #region ModScene
    public class GoDSceneEffect : ModSceneEffect
    {
        public override SceneEffectPriority Priority => SceneEffectPriority.BossMedium;
        public override ModSurfaceBackgroundStyle SurfaceBackgroundStyle => ModContent.GetInstance<TenderoteEmptyBackgroundStyle>();

        public override void Load()
        {
            if (Main.dedServ)
                return;
            SkyManager.Instance["GodSky"] = new GodSky();
        }

        public override bool IsSceneEffectActive(Player player)
        {
            if (!ModContent.GetInstance<jarkos.Jarkos_config>().EnableBossBackground) return false;
            return NPC.AnyNPCs(ModContent.NPCType<God_of_death>());
        }

        public override float GetWeight(Player player)
        {
            int bossIndex = NPC.FindFirstNPC(ModContent.NPCType<God_of_death>());
            if (bossIndex == -1)
                return 0f;
            NPC boss = Main.npc[bossIndex];
            float distance = Vector2.Distance(player.Center, boss.Center);
            if (distance < 1500f)
                return 1f;
            else if (distance < 3000f)
                return 1f - (distance - 1500f) / 1500f;
            else
                return 0f;
        }

        public override void SpecialVisuals(Player player, bool isActive)
        {
            if (isActive)
            {
                if (!SkyManager.Instance["GodSky"].IsActive())
                {
                    SkyManager.Instance.Activate("GodSky");
                }
            }
            else
            {
                if (SkyManager.Instance["GodSky"].IsActive())
                {
                    SkyManager.Instance.Deactivate("GodSky");
                }
            }
        }
    }
    #endregion
}
