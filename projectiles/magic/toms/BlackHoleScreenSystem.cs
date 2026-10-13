using jarkos.projectiles.magic.toms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
namespace jarkos.projectiles.magic.toms
{
    public class BlackHoleScreenSystem : ModSystem
    {
        public const string FilterName = "jarkos:BlackHole";
        public override void Load()
        {
            if (Main.dedServ)
                return;
            try
            {
                Asset<Effect> fx = ModContent.Request<Effect>("jarkos/Shaders/BlackHoleScreen", AssetRequestMode.ImmediateLoad);
                Filters.Scene[FilterName] = new Filter(new BlackHoleScreenShader(new Ref<Effect>(fx.Value), "ScreenPass"), EffectPriority.VeryHigh);
            }
            catch
            {
            }
        }
        public override void PostUpdateEverything()
        {
            if (Main.dedServ || Filters.Scene[FilterName] == null
                || !Filters.Scene[FilterName].IsActive())
                return;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.owner == Main.myPlayer
                    && proj.type == ModContent.ProjectileType<BlackHoleProj>())
                    return;
            }
            BlackHoleScreenShader.Lens = 0f;
            BlackHoleScreenShader.Boom = 0f;
            BlackHoleScreenShader.ShadowR = 0f;
            Filters.Scene.Deactivate(FilterName);
        }
    }
    public class BlackHoleScreenShader : ScreenShaderData
    {
        public static Vector2 HoleWorldPos;
        public static float Lens;
        public static float Boom;
        public static float Grow;
        public static float ShadowR;
        private readonly Ref<Effect> fxRef;
        public BlackHoleScreenShader(Ref<Effect> shader, string passName)
            : base(shader, passName)
        {
            fxRef = shader;
        }
        public override void Apply()
        {
            Effect fx = fxRef?.Value;
            if (fx == null)
                return;
            Vector2 screen = new Vector2(Main.screenWidth, Main.screenHeight);
            Vector2 scr = Vector2.Transform(HoleWorldPos - Main.screenPosition, Main.GameViewMatrix.ZoomMatrix);
            Vector2 uvPos = scr / screen;
            fx.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            float zoom = Main.GameViewMatrix.Zoom.X;
            fx.Parameters["uPos"]?.SetValue(new Vector3(uvPos.X, uvPos.Y, ShadowR * zoom / screen.Y));
            fx.Parameters["uAspect"]?.SetValue(screen.X / screen.Y);
            fx.Parameters["uLens"]?.SetValue(Lens);
            fx.Parameters["uBoom"]?.SetValue(Boom);
            fx.Parameters["uGrow"]?.SetValue(Grow);
            fx.Parameters["uNoiseTexture"]?.SetValue(ModContent.Request<Texture2D>("jarkos/projectiles/magic/toms/VoidNoise", AssetRequestMode.ImmediateLoad).Value);
            fx.CurrentTechnique.Passes[0].Apply();
        }
    }
}
