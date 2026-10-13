using Terraria.ModLoader.Config;
using System.ComponentModel;
using Terraria.ModLoader;
namespace jarkos
{
    public class ResearchClientConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;
        public static ResearchClientConfig Instance => ModContent.GetInstance<ResearchClientConfig>();
        [Header("Visuals")]
        [Label("Graphics Quality Level")]
        [Tooltip("0 = Low, 1 = Medium, 2 = High (default)")]
        [DefaultValue(2)]
        [Range(0, 2)]
        public int QualityLevel { get; set; }
        [Label("Enable Particles")]
        [Tooltip("Toggle cosmic particles on the research screen")]
        [DefaultValue(true)]
        public bool Particles { get; set; }
    }
}
