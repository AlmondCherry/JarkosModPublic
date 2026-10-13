using Terraria;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;

namespace jarkos.systems
{
    public class JarkosShaderCleanupSystem : ModSystem
    {
        private readonly string[] JarkosFilters = new string[]        {
            "Jarkos:GlitchScreen", "Jarkos:ProjectileSunFlare", "jarkos:AxePulseShockwave", "Jarkos:PortalDistortion", "jarkos:CollapsarSlashDistortion", "jarkos:CelestiaEnergyBurst", "Jarkos:BlHole", "Jarkos:WatcherGaze", "jarkos:BlackHoleShockwave", "jarkos:BlackHoleColorSplit", "Jarkos:ShaderEcho", "Jarkos:TenderodDarkness", "Jarkos:TenderoteRoarFilter", "Jarkos:NoxPixelTrail", "jarkos:PhaseBloodShieldOverlay"
        };
        private readonly string[] JarkosSkies = new string[]        {
            "TenderoteSky", "Jarkos:TenderoteSky", "PunisherSky", "Jarkos:PunisherSky", "CelestiaSky", "GodSky", "CEbiome"
        };
        public override void OnWorldUnload()
        {
            CleanupShaders();
        }
        public override void OnWorldLoad()
        {
            CleanupShaders();
        }
        public override void PreSaveAndQuit()
        {
            CleanupShaders();
        }
        private void CleanupShaders()
        {
            if (Main.dedServ)
                return;
            foreach (var filterKey in JarkosFilters)
            {
                try
                {
                    if (Filters.Scene[filterKey] != null && Filters.Scene[filterKey].IsActive())
                    {
                        Filters.Scene[filterKey].Deactivate();
                    }
                }
                catch
                {
                }
            }
            foreach (var skyKey in JarkosSkies)
            {
                try
                {
                    if (SkyManager.Instance[skyKey] != null && SkyManager.Instance[skyKey].IsActive())
                    {
                        SkyManager.Instance.Deactivate(skyKey);
                    }
                }
                catch
                {
                }
            }
        }
    }
}
