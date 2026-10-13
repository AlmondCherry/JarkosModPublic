using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace jarkos.npc.Celestia
{
    public class CelestiaTransitionSystem : ModSystem
    {
        public const string SuctionFilter = "Jarkos:CelestiaSuction";
        public const string ShockFilter = "Jarkos:CelestiaShockwave";

        public static bool TransitionActive;
        public static bool SuctionPhase;
        public static Vector2 FocusPoint;
        public static float CameraLerp;
        public static float SuctionIntensity;
        public static float Whiteout;
        public static float ShockProgress = -1f;

        public static uint KnockbackTick;
        public static Vector2 KnockbackFrom;

        public override void Load()
        {
            if (Main.dedServ)
                return;

            Asset<Effect> suction = ModContent.Request<Effect>("jarkos/Effects/CelestiaSuctionScreen", AssetRequestMode.ImmediateLoad);
            Filters.Scene[SuctionFilter] = new Filter(new ScreenShaderData(suction, "SuctionPass"), EffectPriority.VeryHigh);

            Asset<Effect> shock = ModContent.Request<Effect>("jarkos/Effects/CelestiaShockwaveScreen", AssetRequestMode.ImmediateLoad);
            Filters.Scene[ShockFilter] = new Filter(new ScreenShaderData(shock, "ShockwavePass"), EffectPriority.VeryHigh);
        }

        public override void ModifyScreenPosition()
        {
            if (CameraLerp <= 0f)
                return;

            Vector2 lockedPos = FocusPoint - new Vector2(Main.screenWidth, Main.screenHeight) * 0.5f;
            Main.screenPosition = Vector2.Lerp(Main.screenPosition, lockedPos, CameraLerp);
        }

        public override void PostUpdateEverything()
        {
            if (!TransitionActive)
            {
                CameraLerp = MathHelper.Max(0f, CameraLerp - 0.04f);
                SuctionPhase = false;
                Whiteout = 0f;
                ShockProgress = -1f;

                if (!Main.dedServ)
                {
                    if (Filters.Scene[SuctionFilter].IsActive()) Filters.Scene[SuctionFilter].Deactivate();
                    if (Filters.Scene[ShockFilter].IsActive()) Filters.Scene[ShockFilter].Deactivate();
                }
            }
        }

        public override void OnWorldUnload()
        {
            TransitionActive = false;
            SuctionPhase = false;
            CameraLerp = 0f;
            SuctionIntensity = 0f;
            Whiteout = 0f;
            ShockProgress = -1f;
        }
    }
}
