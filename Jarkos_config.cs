using System.ComponentModel;
using Terraria.ModLoader.Config;
using Terraria.ModLoader;

namespace jarkos
{
  [BackgroundColor(49, 32, 36, 216)]
  public class Jarkos_config : ModConfig
  {
    public override ConfigScope Mode => ConfigScope.ServerSide;

    [Header("$Mods.jarkos.Configs.Jarkos_config.Headers.UI")]
    [Label("UI Position Y")]
    [BackgroundColor(255, 122, 122, 216)]
    [Tooltip("Vertical position of the converter UI (0.0 - 1.0, where 0.5 is center)")]
    [DefaultValue(0.5f)]
    [Range(0f, 1f)]
    public float UIPositionY { get; set; } = 0.5f;

    [Label("ArcaneAnvil UI Position by X")]
    [BackgroundColor(255, 122, 122, 216)]
    [Tooltip("Horizontal position of the Arcane UI in percents of width screen")]
    [Range(0f, 100f)]
    [DefaultValue(50f)]
    [Slider]
    public float ArcaneUIPositionX { get; set; } = 50f;

    [Label("ArcaneAnvil UI Position by Y")]
    [BackgroundColor(255, 122, 122, 216)]
    [Tooltip("Vertical position of the Arcane UI in percents of height screen")]
    [Range(0f, 100f)]
    [DefaultValue(50f)]
    [Slider]
    public float ArcaneUIPositionY { get; set; } = 50f;

    [Label("Panel of Accessory cooldowns")]
    [BackgroundColor(255, 122, 122, 216)]
    [Tooltip("Allows enable/disable visibility of panel of Accessory cooldowns in the top of screen")]
    [DefaultValue(true)]
    public bool EnableAccessoryCooldownUI { get; set; } = true;

    [Label("Text of taking essence soul")]
    [BackgroundColor(255, 122, 122, 216)]
    [Tooltip("Allows enable/disable visibility of popup text above player")]
    [DefaultValue(true)]
    public bool ShowEssencePickupText { get; set; } = true;

    [Label("Flashlight of screen")]
    [BackgroundColor(255, 122, 122, 216)]
    [Tooltip("Allows enable/disable flashlights of some UI, for example table research and etc.")]
    [DefaultValue(true)]
    public bool EnableScreenFlashes { get; set; } = true;
    public static bool ScreenFlashesEnabled => Instance?.EnableScreenFlashes ?? true;

    [Label("Enable popup panel of all essences")]
    [BackgroundColor(255, 122, 122, 216)]
    [Tooltip("Allows enable a popup panel of table research when you targeting by cursor to table")]
    [DefaultValue(true)]
    public bool EnableHoverEssencePanel { get; set; } = true;
    public static bool HoverEssencePanelEnabled => Instance?.EnableHoverEssencePanel ?? true;

    [Header("$Mods.jarkos.Configs.Jarkos_config.Headers.Gameplay")]

    [Label("Enable new system forging for goblin")]
    [Tooltip("Allows enable new forging system of jarkos mod instead vanilla")]
    [BackgroundColor(255, 122, 122, 216)]
    [DefaultValue(true)]
    public bool EnableGoblinReforge { get; set; } = true;
    public static bool GoblinReforgeEnabled => Instance?.EnableGoblinReforge ?? true;

    [Label("Universal growth of any flowers for potions")]
    [Tooltip("Allows grow any flowers in any cases, without limits")]
    [BackgroundColor(255, 122, 122, 216)]
    [DefaultValue(true)]
    public bool AlwaysGrowHerbs { get; set; } = true;

    [Label("Automatic rejoin to your previous team")]
    [Tooltip("Allows to rejoin to your previous team if you restart game/world")]
    [BackgroundColor(255, 122, 122, 216)]
    [DefaultValue(true)]
    public bool AutoRejoinTeam { get; set; } = true;

    [Label("More faster recycling on the extractinator")]
    [Tooltip("Do more faster recycling in extractinator in 10 times")]
    [BackgroundColor(255, 122, 122, 216)]
    [DefaultValue(true)]
    public bool FastExtractinator { get; set; } = true;

    [Label("Instant recycling of the entire stack on the extractinator")]
    [Tooltip("When using the right click on the extractinator, the entire stack of blocks in your hands is processed at once")]
    [BackgroundColor(255, 122, 122, 216)]
    [DefaultValue(true)]
    public bool ExtractStackInstantly { get; set; } = true;

    [LabelKey("$Mods.jarkos.Configs.Jarkos_config.EnableBuffNPCVisual.Label")]
    [TooltipKey("$Mods.jarkos.Configs.Jarkos_config.EnableBuffNPCVisual.Tooltip")]
    [BackgroundColor(255, 122, 122, 216)]
    [DefaultValue(false)]
    public bool EnableBuffNPCVisual { get; set; } = false;

    [LabelKey("$Mods.jarkos.Configs.Jarkos_config.DisableResearchBuffs.Label")]
    [TooltipKey("$Mods.jarkos.Configs.Jarkos_config.DisableResearchBuffs.Tooltip")]
    [BackgroundColor(255, 122, 122, 216)]
    [DefaultValue(false)]
    public bool DisableResearchBuffs { get; set; } = false;

    [LabelKey("$Mods.jarkos.Configs.Jarkos_config.ScavengerPriceMultiplier.Label")]
    [TooltipKey("$Mods.jarkos.Configs.Jarkos_config.ScavengerPriceMultiplier.Tooltip")]
    [BackgroundColor(255, 122, 122, 216)]
    [SliderColor(224, 165, 56, 128)]
    [Range(0.1f, 10f)]
    [Increment(0.1f)]
    [DefaultValue(1.0f)]
    public float ScavengerPriceMultiplier { get; set; } = 1.0f;

    [LabelKey("$Mods.jarkos.Configs.Jarkos_config.DaylightAlwaysWorks.Label")]
    [TooltipKey("$Mods.jarkos.Configs.Jarkos_config.DaylightAlwaysWorks.Tooltip")]
    [BackgroundColor(255, 122, 122, 216)]
    [DefaultValue(false)]
    public bool DaylightAlwaysWorks { get; set; } = false;

    [LabelKey("$Mods.jarkos.Configs.Jarkos_config.DaylightProductionSpeed.Label")]
    [TooltipKey("$Mods.jarkos.Configs.Jarkos_config.DaylightProductionSpeed.Tooltip")]
    [BackgroundColor(255, 122, 122, 216)]
    [SliderColor(224, 165, 56, 128)]
    [Range(0.8f, 2.25f)]
    [Increment(0.05f)]
    [DefaultValue(1.0f)]
    public float DaylightProductionSpeed { get; set; } = 1.0f;

    [LabelKey("$Mods.jarkos.Configs.Jarkos_config.DaylightCapacityMultiplier.Label")]
    [TooltipKey("$Mods.jarkos.Configs.Jarkos_config.DaylightCapacityMultiplier.Tooltip")]
    [BackgroundColor(255, 122, 122, 216)]
    [SliderColor(224, 165, 56, 128)]
    [Range(1f, 10f)]
    [Increment(0.5f)]
    [DefaultValue(1.0f)]
    public float DaylightCapacityMultiplier { get; set; } = 1.0f;

    [LabelKey("$Mods.jarkos.Configs.Jarkos_config.EssenceDropMultiplier.Label")]
    [TooltipKey("$Mods.jarkos.Configs.Jarkos_config.EssenceDropMultiplier.Tooltip")]
    [BackgroundColor(255, 122, 122, 216)]
    [SliderColor(224, 165, 56, 128)]
    [Range(0.5f, 7.5f)]
    [Increment(0.1f)]
    [DefaultValue(1.0f)]
    public float EssenceDropMultiplier { get; set; } = 1.0f;

    [Label("Force Finish Research Button")]
    [BackgroundColor(255, 122, 122, 216)]
    [Tooltip("Shows 'Force Finish' button when selecting a node in the research table")]
    [DefaultValue(false)]
    public bool EnableForceFinish { get; set; } = false;

    [Label("Unlock All Essences")]
    [BackgroundColor(255, 122, 122, 216)]
    [Tooltip("Gives infinite quantity of each essence and unlocks all essences while enabled. Restores original amounts and research when disabled.")]
    [DefaultValue(false)]
    public bool UnlockAllEssences { get; set; } = false;

    [Label("Town NPC Arrival Rate")]
    [BackgroundColor(255, 122, 122, 216)]
    [SliderColor(224, 165, 56, 128)]
    [Range(0f, 100f)]
    [Increment(1f)]
    [Tooltip("The higher the value, the much higher the chance of any city NPCs showing up if there's an available house. 1 - normal setting, 0 - disabled.")]
    [DefaultValue(1f)]
    public float TownNPCSpawnMultiplier { get; set; } = 1f;

    [Label("Small gameplay changes")]
    [BackgroundColor(255, 122, 122, 216)]
    [SliderColor(224, 165, 56, 128)]
    [Tooltip("Adds more interesting things")]
    [DefaultValue(true)]
    public bool small_gameplay_changes { get; set; }

    [Label("More vanilla loot")]
    [Tooltip("When enabled, additional modified loot from the mod will drop from vanilla bosses. When disabled, no additional loot will drop.")]
    [BackgroundColor(255, 122, 122, 216)]
    [DefaultValue(true)]
    public bool MoreVanillaLoot { get; set; } = true;

    [Label("The effect coefficient of the essence")]
    [Tooltip("The coefficient of the essence effect value for rune enchantment of armor (from 0.5 to 1.75)")]
    [BackgroundColor(255, 122, 122, 216)]
    [SliderColor(224, 165, 56, 128)]
    [Range(0.5f, 1.75f)]
    [Increment(0.05f)]
    [DefaultValue(1.0f)]
    public float EssenceEffectMultiplier { get; set; } = 1.0f;

    public static Jarkos_config Instance => ModContent.GetInstance<Jarkos_config>();
    public static bool ArenaEnabled => Instance?.EnableArenaShader ?? true;
    public static bool BossBackgroundEnabled => Instance?.EnableBossBackground ?? true;

    [Header("$Mods.jarkos.Configs.Jarkos_config.Headers.Optimization")]
    [BackgroundColor(255, 122, 122, 216)]
    [DefaultValue(true)]
    [Label("Enable Arena Shader")]
    [Tooltip("Allows you to enable various arena shaders for bosses")]
    public bool EnableArenaShader { get; set; } = true;

    [DefaultValue(true)]
    [BackgroundColor(255, 122, 122, 216)]
    [Label("Enable Boss Background")]
    [Tooltip("Allows you to enable various BG shaders for bosses")]
    public bool EnableBossBackground { get; set; } = true;

    [DefaultValue(true)]
    [BackgroundColor(255, 122, 122, 216)]
    [Label("Enable Some Shaders")]
    [Tooltip("Allows you to enable various shaders")]
    public bool EnableSomeShaders { get; set; } = true;

    public static bool SomeShadersEnabled => Instance?.EnableSomeShaders ?? true;

    [DefaultValue(100)]
    [BackgroundColor(255, 122, 122, 216)]
    [Label("Max Particles")]
    [Tooltip("Allows you to set the maximum number of particles")]
    public int maxparticles { get; set; }

    [Label("Trail quality of Spline Subdivisions")]
    [BackgroundColor(255, 122, 122, 216)]
    [SliderColor(224, 165, 56, 128)]
    [Range(2, 12)]
    [Increment(1)]
    [DefaultValue(8)]
    [Tooltip("Larger values produce the smoothest curves for any trails")]
    public int TrailQuality { get; set; } = 8;

    [Label("Disable Animated Essence Sections")]
    [BackgroundColor(255, 122, 122, 216)]
    [DefaultValue(false)]
    [Tooltip("When the essence strip section is enabled, the interfaces become static square blocks without any wave-like animation")]
    public bool DisableAnimatedEssenceSections { get; set; } = false;

    public static bool StaticEssenceSections => Instance?.DisableAnimatedEssenceSections ?? false;

    public override void OnChanged()
    {
      base.OnChanged();
    }
  }
}
