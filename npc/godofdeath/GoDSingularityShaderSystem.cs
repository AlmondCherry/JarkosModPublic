using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace jarkos.npc.godofdeath
{
    public sealed class GoDSingularityShaderSystem : ModSystem
    {
        public const string FK_Singularity = "Jarkos:GoDSingularity";
        public const string FK_ScytheGlow = "Jarkos:GoDScytheGlow";
        public const string FK_VoidRift = "Jarkos:GoDVoidRift";
        public const string FK_RiftSlash = "Jarkos:GoDRiftSlash";
        public const string FK_VoidPool = "Jarkos:GoDVoidPool";
        private static Asset<Effect> registered_Singularity;
        private static Asset<Effect> registered_ScytheGlow;
        private static Asset<Effect> registered_VoidRift;
        private static Asset<Effect> registered_RiftSlash;
        private static Asset<Effect> registered_VoidPool;
        
        public override void Load()
        {
            if (Main.dedServ)
                return;

            try
            {
                registered_Singularity = ModContent.Request<Effect>("Jarkos/npc/godofdeath/Effects/GoDSingularityFilter", AssetRequestMode.ImmediateLoad);
                registered_ScytheGlow = ModContent.Request<Effect>("Jarkos/npc/godofdeath/Effects/GoDScytheGlow", AssetRequestMode.ImmediateLoad);
                registered_VoidRift = ModContent.Request<Effect>("Jarkos/npc/godofdeath/Effects/GoDVoidRift", AssetRequestMode.ImmediateLoad);
                registered_RiftSlash = ModContent.Request<Effect>("Jarkos/npc/godofdeath/Effects/GoDRiftSlash", AssetRequestMode.ImmediateLoad);
                registered_VoidPool = ModContent.Request<Effect>("Jarkos/npc/godofdeath/Effects/GoDVoidPool", AssetRequestMode.ImmediateLoad);
            }
            catch
            {
                registered_Singularity = null;
                registered_ScytheGlow = null;
                registered_VoidRift = null;
                registered_RiftSlash = null;
                registered_VoidPool = null;
            }

            if(registered_Singularity != null)
            {
                Filters.Scene[FK_Singularity] = new Filter(new ScreenShaderData(registered_Singularity, "GoDSingularity"), EffectPriority.VeryHigh);
                Filters.Scene[FK_Singularity].Load();
            }

            if(registered_ScytheGlow != null)
            {
                GameShaders.Misc[FK_ScytheGlow] = new MiscShaderData(registered_ScytheGlow, "GoDScytheGlow");
            }

            if(registered_VoidRift != null)
            {
                GameShaders.Misc[FK_VoidRift] = new MiscShaderData(registered_VoidRift, "GoDVoidRift");
            }

            if(registered_RiftSlash != null)
            {
                GameShaders.Misc[FK_RiftSlash] = new MiscShaderData(registered_RiftSlash, "GoDRiftSlash");
            }

            if(registered_VoidPool != null)
            {
                GameShaders.Misc[FK_VoidPool] = new MiscShaderData(registered_VoidPool, "GoDVoidPool");
            }
        }
        public override void Unload()
        {
            if (!Main.dedServ && registered_Singularity != null && Filters.Scene[FK_Singularity].IsActive())
                Filters.Scene.Deactivate(FK_Singularity);
            registered_Singularity = null;

            if (!Main.dedServ && registered_ScytheGlow != null && registered_ScytheGlow.IsLoaded)
                GameShaders.Misc.Remove(FK_ScytheGlow);
            registered_ScytheGlow = null;

            if (!Main.dedServ && registered_VoidRift != null && registered_VoidRift.IsLoaded)
                GameShaders.Misc.Remove(FK_VoidRift);
            registered_VoidRift = null;

            if (!Main.dedServ && registered_RiftSlash != null && registered_RiftSlash.IsLoaded)
                GameShaders.Misc.Remove(FK_RiftSlash);
            registered_RiftSlash = null;

            if (!Main.dedServ && registered_VoidPool != null && registered_VoidPool.IsLoaded)
                GameShaders.Misc.Remove(FK_VoidPool);
            registered_VoidPool = null;
        }
    }
}