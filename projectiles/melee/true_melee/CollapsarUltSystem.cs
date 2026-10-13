using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace jarkos.projectiles.melee.true_melee
{
    public class CollapsarUltSystem : ModSystem
    {
        public const string SkyName = "Jarkos:CollapsarSky";

        public override void Load()
        {
            if (Main.dedServ)
                return;

            SkyManager.Instance[SkyName] = new CollapsarSky();
        }

        public override void PostUpdateEverything()
        {
            if (Main.dedServ)
                return;

            bool shouldBeActive = CollapsarSky.Current > 0.01f || CollapsarSky.TargetValue > 0.01f;
            bool isActive = SkyManager.Instance[SkyName].IsActive();

            if (shouldBeActive && !isActive)
                SkyManager.Instance.Activate(SkyName);
            else if (!shouldBeActive && isActive)
                SkyManager.Instance.Deactivate(SkyName);
        }
    }

    public class CollapsarSky : CustomSky
    {
        private static float _target;
        private static float _current;
        private static float _flash;

        public static float Current => _current;
        public static float TargetValue => _target;

        private bool _active;
        public static void RequestDarkness(float intensity)
            => _target = MathHelper.Clamp(intensity, 0f, 1f);
        public static void Flash(float strength)
            => _flash = MathF.Max(_flash, MathHelper.Clamp(strength, 0f, 1f));

        public override void Update(GameTime gameTime)
        {
            _current = MathHelper.Lerp(_current, _target, 0.08f);
            _target *= 0.97f;
            _flash *= 0.88f;
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (maxDepth < 0f || minDepth >= 0f)
                return;
            if (_current <= 0.01f)
                return;

            Effect fx = ModContent.Request<Effect>(
                "Jarkos/Shaders/CollapsarSky",
                AssetRequestMode.ImmediateLoad).Value;

            fx.Parameters["uIntensity"]?.SetValue(_current);
            fx.Parameters["uFlash"]?.SetValue(_flash);
            fx.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            fx.Parameters["uColor"]?.SetValue(new Vector3(0.06f, 0.09f, 0.18f));
            fx.Parameters["uResolution"]?.SetValue(new Vector2(Main.screenWidth, Main.screenHeight));

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, fx);

            spriteBatch.Draw(TextureAssets.MagicPixel.Value,
                new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone,
                null, Main.BackgroundViewMatrix.TransformationMatrix);
        }

        public override float GetCloudAlpha() => 1f - _current * 0.5f;

        public override void Activate(Vector2 position, params object[] args) => _active = true;
        public override void Deactivate(params object[] args) => _active = false;
        public override void Reset() => _active = false;
        public override bool IsActive() => _active || _current > 0.01f;
    }
}
