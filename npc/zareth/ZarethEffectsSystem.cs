using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
namespace jarkos.npc.zareth
{
    public class ZarethEffectsSystem : ModSystem
    {
        public const string ShockwaveFilterName = "Jarkos:ZarethShockwave";
        public const string ShockFilterName = "Jarkos:ZarethShock";
        private static float shockwaveProgress = -1f;
        private static float shockwaveStrength = 1f;
        private static float shockIntensity;
        private static bool shockActiveThisFrame;
        public override void Load()
        {
            if (Main.dedServ)
                return;
            Asset<Effect> shockwave = ModContent.Request<Effect>(
                "jarkos/Shaders/ZarethShockwave", AssetRequestMode.ImmediateLoad);
            Filters.Scene[ShockwaveFilterName] = new Filter(
                new ScreenShaderData(new Ref<Effect>(shockwave.Value), "ZarethShockwavePass"),
                EffectPriority.VeryHigh);
            Asset<Effect> shock = ModContent.Request<Effect>(
                "jarkos/Shaders/ZarethShock", AssetRequestMode.ImmediateLoad);
            Filters.Scene[ShockFilterName] = new Filter(
                new ScreenShaderData(new Ref<Effect>(shock.Value), "ZarethShockPass"),
                EffectPriority.VeryHigh);
        }
        public static void TriggerShockwave(Vector2 worldPosition, float strength = 1f)
        {
            if (Main.dedServ) return;
            shockwaveProgress = 0f;
            shockwaveStrength = strength;
            if (!Filters.Scene[ShockwaveFilterName].IsActive())
                Filters.Scene.Activate(ShockwaveFilterName, worldPosition);
            Filters.Scene[ShockwaveFilterName].GetShader()
                .UseTargetPosition(worldPosition).UseProgress(0f).UseIntensity(strength);
        }
        public static void TriggerShock(float intensity)
        {
            if (Main.dedServ) return;
            shockIntensity = intensity;
            shockActiveThisFrame = true;
            if (!Filters.Scene[ShockFilterName].IsActive())
                Filters.Scene.Activate(ShockFilterName);
        }
        public static void MaintainShock(float intensity)
        {
            if (Main.dedServ) return;
            shockIntensity = intensity;
            shockActiveThisFrame = true;
            if (!Filters.Scene[ShockFilterName].IsActive())
                Filters.Scene.Activate(ShockFilterName);
        }
        public override void PostUpdateEverything()
        {
            if (shockwaveProgress >= 0f)
            {
                shockwaveProgress += 0.02f;
                Filters.Scene[ShockwaveFilterName].GetShader()
                    .UseProgress(shockwaveProgress)
                    .UseIntensity(shockwaveStrength * (1f - shockwaveProgress));
                if (shockwaveProgress >= 1f)
                {
                    shockwaveProgress = -1f;
                    if (Filters.Scene[ShockwaveFilterName].IsActive())
                        Filters.Scene.Deactivate(ShockwaveFilterName);
                }
            }
            if (Filters.Scene[ShockFilterName].IsActive())
            {
                if (shockActiveThisFrame)
                {
                    Filters.Scene[ShockFilterName].GetShader()
                        .UseIntensity(shockIntensity)
                        .UseProgress(Main.GlobalTimeWrappedHourly);
                }
                else
                {
                    Filters.Scene.Deactivate(ShockFilterName);
                }
            }
            shockActiveThisFrame = false;
        }
    }
}
