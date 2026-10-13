using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace jarkos.npc.CradleMobs.WatcherEye.Effects
{
    public sealed class WatcherShaderSystem : ModSystem
    {
        public const string FilterKey = "Jarkos:WatcherGaze";
        private static Asset<Effect> registered;
        public override void Load()
        {
            if (Main.dedServ)
                return;

            try
            {
                registered = ModContent.Request<Effect>("Jarkos/npc/CradleMobs/WatcherEye/Effects/WatcherGazeShader", AssetRequestMode.ImmediateLoad);
            }
            catch
            {
                registered = null;
            }

            if(registered != null)
            {
                Filters.Scene[FilterKey] = new Filter(new ScreenShaderData(registered, "WatcherGaze"), EffectPriority.High);
                Filters.Scene[FilterKey].Load();
            }
        }
        public override void Unload()
        {
            if (!Main.dedServ && registered != null && Filters.Scene[FilterKey].IsActive())
                Filters.Scene.Deactivate(FilterKey);
            registered = null;
        }
    }
}