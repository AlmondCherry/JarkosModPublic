using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
namespace jarkos.systems;
public sealed class CosmicItemShaderSystem : ModSystem
{
    public static string CosmicItemShaderKey { get; private set; } = "jarkos:CosmicItem";
    public static string CosmicSpikeFireShaderKey { get; private set; } = "jarkos:CosmicSpikeFire";
    public static string CosmicShieldShaderKey { get; private set; } = "jarkos:CosmicShield";
    public static string CosmicSoulFlameShaderKey { get; private set; } = "jarkos:CosmicSoulFlame";
    public static string CosmicStarStormUltimateShaderKey { get; private set; } = "jarkos:CosmicStarStormUltimate";
    public static string CosmicRiftShaderKey { get; private set; } = "jarkos:CosmicRift";
    public static string MiniSpaceDistortionShaderKey { get; private set; } = "jarkos:MiniSpaceDistortion";
    private const int MaxPortalWaves = 6;
    private const float PortalDurationFrames = 34f;
    private const float OffscreenPadding = 180f;
    private static readonly Vector2[] portalPositions = new Vector2[MaxPortalWaves];
    private static readonly float[] portalFrames = new float[MaxPortalWaves];
    private static readonly float[] portalPowers = new float[MaxPortalWaves];
    private static Vector2 starStormTarget;
    private static float starStormFrames;
    private static float starStormDuration;
    private static float starStormPower;
    private static bool registered;
    public static void SpawnMiniSpaceDistortion(Vector2 worldCenter, float power = 1f)
    {
        if (Main.dedServ)
            return;
        int slot = 0;
        float oldestFrame = float.MaxValue;
        for (int i = 0; i < MaxPortalWaves; i++)
        {
            if (portalFrames[i] <= 0f)
            {
                slot = i;
                break;
            }
            if (portalFrames[i] < oldestFrame)
            {
                oldestFrame = portalFrames[i];
                slot = i;
            }
        }
        portalPositions[slot] = worldCenter;
        portalFrames[slot] = PortalDurationFrames;
        portalPowers[slot] = MathHelper.Clamp(power, 0.15f, 1f);
    }
    public static void StartStarStormUltimate(Vector2 worldCenter, float power, int durationFrames = 132)
    {
        if (Main.dedServ)
            return;
        starStormTarget = worldCenter;
        starStormDuration = MathHelper.Clamp(durationFrames, 30, 240);
        starStormFrames = starStormDuration;
        starStormPower = MathHelper.Clamp(power, 0.1f, 1f);
    }
    public override void Load()
    {
        if (Main.dedServ)
            return;
        CosmicItemShaderKey = $"{Mod.Name}:CosmicItem";
        CosmicSpikeFireShaderKey = $"{Mod.Name}:CosmicSpikeFire";
        CosmicShieldShaderKey = $"{Mod.Name}:CosmicShield";
        CosmicSoulFlameShaderKey = $"{Mod.Name}:CosmicSoulFlame";
        CosmicStarStormUltimateShaderKey = $"{Mod.Name}:CosmicStarStormUltimate";
        CosmicRiftShaderKey = $"{Mod.Name}:CosmicRift";
        MiniSpaceDistortionShaderKey = $"{Mod.Name}:MiniSpaceDistortion";
        GameShaders.Misc[CosmicItemShaderKey] = new MiscShaderData(ModContent.Request<Effect>($"{Mod.Name}/Shaders/Helfeim/CosmicItem"), "Pass1");
        GameShaders.Misc[CosmicSpikeFireShaderKey] = new MiscShaderData(ModContent.Request<Effect>($"{Mod.Name}/Shaders/Helfeim/CosmicSpikeFire"), "Pass1");
        GameShaders.Misc[CosmicShieldShaderKey] = new MiscShaderData(ModContent.Request<Effect>($"{Mod.Name}/Shaders/Helfeim/CosmicShield"), "Pass1");
        GameShaders.Misc[CosmicSoulFlameShaderKey] = new MiscShaderData(ModContent.Request<Effect>($"{Mod.Name}/Shaders/Helfeim/CosmicSoulFlame"), "Pass1");
        Filters.Scene[CosmicStarStormUltimateShaderKey] = new Filter(new ScreenShaderData(ModContent.Request<Effect>($"{Mod.Name}/Shaders/Helfeim/CosmicStarStormUltimate"), "Pass1")
                .UseColor(new Color(92, 34, 180))
                .UseSecondaryColor(new Color(255, 82, 190))
                .UseOpacity(0f), EffectPriority.VeryHigh);
        Filters.Scene[MiniSpaceDistortionShaderKey] = new Filter(new ScreenShaderData(ModContent.Request<Effect>($"{Mod.Name}/Shaders/Helfeim/MiniSpaceDistortion"), "Pass1")
                .UseColor(new Color(10, 2, 28))
                .UseSecondaryColor(new Color(95, 34, 180))
                .UseOpacity(0f), EffectPriority.VeryHigh);
        Filters.Scene[CosmicStarStormUltimateShaderKey].Load();
        Filters.Scene[MiniSpaceDistortionShaderKey].Load();
        registered = true;
    }
    public override void Unload()
    {
        if (!Main.dedServ && registered && Filters.Scene[MiniSpaceDistortionShaderKey].IsActive())
            Filters.Scene.Deactivate(MiniSpaceDistortionShaderKey);
        if (!Main.dedServ && registered && Filters.Scene[CosmicStarStormUltimateShaderKey].IsActive())
            Filters.Scene.Deactivate(CosmicStarStormUltimateShaderKey);
        ClearPortalWaves();
        starStormFrames = 0f;
        starStormDuration = 0f;
        starStormPower = 0f;
        registered = false;
        CosmicItemShaderKey = "jarkos:CosmicItem";
        CosmicSpikeFireShaderKey = "jarkos:CosmicSpikeFire";
        CosmicShieldShaderKey = "jarkos:CosmicShield";
        CosmicSoulFlameShaderKey = "jarkos:CosmicSoulFlame";
        CosmicStarStormUltimateShaderKey = "jarkos:CosmicStarStormUltimate";
        CosmicRiftShaderKey = "jarkos:CosmicRift";
        MiniSpaceDistortionShaderKey = "jarkos:MiniSpaceDistortion";
    }
    public override void PostUpdateEverything()
    {
        if (Main.dedServ || !registered)
            return;
        UpdateStarStormUltimate();
        bool hasVisiblePortal = false;
        float strongestPower = 0f;
        Vector2 strongestPosition = Vector2.Zero;
        for (int i = 0; i < MaxPortalWaves; i++)
        {
            if (portalFrames[i] <= 0f)
            {
                portalPowers[i] = 0f;
                continue;
            }
            portalFrames[i]--;
            Vector2 screenPosition = portalPositions[i] - Main.screenPosition;
            if (!Main.gameMenu && IsOnScreen(screenPosition, OffscreenPadding))
            {
                hasVisiblePortal = true;
                if (portalPowers[i] > strongestPower)
                {
                    strongestPower = portalPowers[i];
                    strongestPosition = portalPositions[i];
                }
            }
        }
        if (!hasVisiblePortal)
        {
            DeactivateMiniSpaceDistortion();
            return;
        }
        if (!Filters.Scene[MiniSpaceDistortionShaderKey].IsActive())
            Filters.Scene.Activate(MiniSpaceDistortionShaderKey, strongestPosition);
        Vector2 screenSize = new Vector2(Main.screenWidth, Main.screenHeight);
        ScreenShaderData shader = Filters.Scene[MiniSpaceDistortionShaderKey].GetShader();
        shader
            .UseTargetPosition(strongestPosition)
            .UseProgress(0f)
            .UseIntensity(strongestPower)
            .UseOpacity(0.62f)
            .UseColor(new Color(10, 2, 28))
            .UseSecondaryColor(new Color(95, 34, 180))
            .UseImageOffset(screenSize)
            .UseDirection(Main.GameViewMatrix.Zoom);
        Effect effect = shader.Shader;
        effect.Parameters["uPortalPositions"]?.SetValue(portalPositions);
        effect.Parameters["uPortalFrames"]?.SetValue(portalFrames);
        effect.Parameters["uPortalPowers"]?.SetValue(portalPowers);
        effect.Parameters["uPortalCount"]?.SetValue((float)MaxPortalWaves);
        effect.Parameters["uPortalDuration"]?.SetValue(PortalDurationFrames);
        effect.Parameters["uPortalScreenPosition"]?.SetValue(Main.screenPosition);
    }
    private static bool IsOnScreen(Vector2 screenPosition, float padding)
    {
        return screenPosition.X > -padding
            && screenPosition.X < Main.screenWidth + padding
            && screenPosition.Y > -padding
            && screenPosition.Y < Main.screenHeight + padding;
    }
    private static void UpdateStarStormUltimate()
    {
        if (starStormFrames <= 0f)
        {
            DeactivateStarStormUltimate();
            return;
        }
        starStormFrames--;
        float duration = Math.Max(starStormDuration, 1f);
        float progress = MathHelper.Clamp(1f - starStormFrames / duration, 0f, 1f);
        float fadeInT = MathHelper.Clamp(progress / 0.20f, 0f, 1f);
        float fadeIn = fadeInT * fadeInT * (3f - 2f * fadeInT);
        float fadeOutT = 1f - MathHelper.Clamp((progress - 0.42f) / 0.58f, 0f, 1f);
        float fadeOut = fadeOutT * fadeOutT * (3f - 2f * fadeOutT);
        float pulse = 1f + 0.10f * (float)Math.Sin(progress * MathHelper.TwoPi * 3.4f);
        float opacity = fadeIn * fadeOut * (0.42f + starStormPower * 0.24f) * pulse;
        if (opacity <= 0.0008f)
        {
            DeactivateStarStormUltimate();
            return;
        }
        if (!Filters.Scene[CosmicStarStormUltimateShaderKey].IsActive())
            Filters.Scene.Activate(CosmicStarStormUltimateShaderKey, starStormTarget);
        Vector2 screenSize = new Vector2(Main.screenWidth, Main.screenHeight);
        ScreenShaderData shader = Filters.Scene[CosmicStarStormUltimateShaderKey].GetShader();
        shader
            .UseTargetPosition(starStormTarget)
            .UseProgress(progress)
            .UseIntensity(starStormPower)
            .UseOpacity(opacity)
            .UseColor(new Color(55, 6, 95))
            .UseSecondaryColor(new Color(255, 72, 190))
            .UseImageOffset(screenSize)
            .UseDirection(Main.GameViewMatrix.Zoom);
    }
    private static void DeactivateStarStormUltimate()
    {
        if (registered && Filters.Scene[CosmicStarStormUltimateShaderKey].IsActive())
            Filters.Scene.Deactivate(CosmicStarStormUltimateShaderKey);
    }
    private static void DeactivateMiniSpaceDistortion()
    {
        if (registered && Filters.Scene[MiniSpaceDistortionShaderKey].IsActive())
            Filters.Scene.Deactivate(MiniSpaceDistortionShaderKey);
    }
    private static void ClearPortalWaves()
    {
        for (int i = 0; i < MaxPortalWaves; i++)
        {
            portalPositions[i] = Vector2.Zero;
            portalFrames[i] = 0f;
            portalPowers[i] = 0f;
        }
    }
}
