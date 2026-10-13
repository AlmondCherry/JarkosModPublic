using System;
using System.Collections.Generic;
using jarkos.items.accs.amulets;
using jarkos.items.weapons.melee.true_melee;
using jarkos.projectiles.magic.other;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace jarkos.systems
{
    #region Black Hole Projectile
    public class ShaderSystem : ModSystem
    {
        private const string ModInternalName = "Jarkos";
        private const string ShockwaveKey = "jarkos:BlackHoleShockwave";
        private const string ColorSplitKey = "jarkos:BlackHoleColorSplit";
        private const bool UsePs30Shaders = false;
        private const int ShockwaveLifetime = 72;
        private const int ColorSplitLifetime = 105;
        private bool shockwaveActive;
        private int shockwaveTimer;
        private Vector2 shockwaveCenter;
        private float shockwavePower;
        private bool colorSplitActive;
        private int colorSplitTimer;
        private float colorSplitPower;
        public override void Load()
        {
            if (Main.dedServ)
                return;
            string shockwavePath = UsePs30Shaders
                ? $"{ModInternalName}/Shaders/BlackHoleShockwavePs30"
                : $"{ModInternalName}/Shaders/BlackHoleShockwavePs20";
            string colorSplitPath = UsePs30Shaders
                ? $"{ModInternalName}/Shaders/BlackHoleColorSplitPs30"
                : $"{ModInternalName}/Shaders/BlackHoleColorSplitPs20";
                
            Filters.Scene[ShockwaveKey] = new Filter(new ScreenShaderData(ModContent.Request<Effect>(shockwavePath, AssetRequestMode.ImmediateLoad), "Pass1"), EffectPriority.High);
            Filters.Scene[ShockwaveKey].Load();
            Filters.Scene[ColorSplitKey] = new Filter(new ScreenShaderData(ModContent.Request<Effect>(colorSplitPath, AssetRequestMode.ImmediateLoad), "Pass1"), EffectPriority.High);
            Filters.Scene[ColorSplitKey].Load();
        }
        public override void Unload()
        {
            shockwaveActive = false;
            colorSplitActive = false;
        }
        public override void OnWorldUnload()
        {
            if (Main.dedServ)
                return;

            shockwaveActive = false;
            colorSplitActive = false;

            Filters.Scene.Deactivate(ShockwaveKey);
            Filters.Scene.Deactivate(ColorSplitKey);
        }
        public void StartExplosion(Vector2 worldPosition, float power = 1f)
        {
            if (Main.dedServ)
                return;
            power = MathHelper.Clamp(power, 0.65f, 1.65f);

            shockwaveActive = true;
            shockwaveTimer = 0;
            shockwaveCenter = worldPosition;
            shockwavePower = power;
            colorSplitActive = true;
            colorSplitTimer = 0;
            colorSplitPower = MathHelper.Clamp(power * 0.85f, 0.55f, 1.35f);

            Filters.Scene.Activate(ShockwaveKey, shockwaveCenter);
            Filters.Scene.Activate(ColorSplitKey, Main.LocalPlayer.Center);
        }
        public override void PostUpdateEverything()
        {
            if (Main.dedServ)
                return;
            if (Main.gameMenu)
            {
                DeactivateShockwave();
                DeactivateColorSplit();
                return;
            }

            UpdateShockwave();
            UpdateColorSplit();
        }
        private void UpdateShockwave()
        {
            if (!shockwaveActive)
                return;

            shockwaveTimer++;
            float progress = shockwaveTimer / (float)ShockwaveLifetime;

            if (progress >= 1f)
            {
                DeactivateShockwave();
                return;
            }

            float fade = 1f - progress;
            float intensity = shockwavePower * fade;
            float opacity = MathHelper.Clamp(fade * 1.15f, 0f, 1f);
            Filters.Scene[ShockwaveKey].GetShader()
                .UseTargetPosition(shockwaveCenter)
                .UseProgress(progress)
                .UseIntensity(intensity)
                .UseOpacity(opacity)
                .UseColor(new Color(140, 35, 255))
                .UseSecondaryColor(new Color(255, 120, 45));
        }
        private void UpdateColorSplit()
        {
            if (!colorSplitActive)
                return;

            colorSplitTimer++;

            float progress = colorSplitTimer / (float)ColorSplitLifetime;

            if (progress >= 1f)
            {
                DeactivateColorSplit();
                return;
            }

            float opacity = progress < 0.84f ? 1f : MathHelper.Clamp(1f - (progress - 0.84f) / 0.16f, 0f, 1f);
            Filters.Scene[ColorSplitKey].GetShader()
                .UseProgress(progress)
                .UseIntensity(colorSplitPower)
                .UseOpacity(opacity)
                .UseColor(new Color(170, 45, 255))
                .UseSecondaryColor(new Color(255, 160, 40));
        }
        private void DeactivateShockwave()
        {
            if (!shockwaveActive)
                return;
            shockwaveActive = false;
            Filters.Scene.Deactivate(ShockwaveKey);
        }
        private void DeactivateColorSplit()
        {
            if (!colorSplitActive)
                return;
            colorSplitActive = false;
            Filters.Scene.Deactivate(ColorSplitKey);
        }
    }
    #region Glitch Screen
    public class GlitchScreenSystem : ModSystem
    {
    public const string FilterKey = "Jarkos:GlitchScreen";
    public override void Load()
    {
        if (Main.dedServ)
            return;
        Filters.Scene[FilterKey] = new Filter(new ScreenShaderData(ModContent.Request<Effect>("Jarkos/Shaders/GlitchScreen", AssetRequestMode.ImmediateLoad), "Pass1"), EffectPriority.High);
        Filters.Scene[FilterKey].Load();
    }
    public override void PostUpdateEverything()
    {
        if (Main.dedServ)
            return;
        if (Filters.Scene[FilterKey].IsActive())
        {
            Vector2 screenCenter = Main.screenPosition + new Vector2(Main.screenWidth, Main.screenHeight) * 0.5f;
            Filters.Scene[FilterKey]
                .GetShader()
                .UseTargetPosition(screenCenter);
        }
    }
    }
    #region Sun Flare Laser
    public sealed class ShaderSunFlare : ModSystem
    {
        private const string FilterKey = "Jarkos:ProjectileSunFlare";
        private bool registered;
        public override void Load()
        {
            if (Main.dedServ)
                return;
            Filters.Scene[FilterKey] = new Filter(new ScreenShaderData(ModContent.Request<Effect>("Jarkos/Shaders/SunFlareEffect", AssetRequestMode.ImmediateLoad), "Pass1"), EffectPriority.VeryHigh);
            Filters.Scene[FilterKey].Load();
            registered = true;
        }
        public override void Unload()
        {
            if (!Main.dedServ && registered)
                Filters.Scene.Deactivate(FilterKey);
            registered = false;
        }
        public override void PostUpdateEverything()
        {
            if (Main.dedServ || !registered)
                return;
            if (Main.gameMenu)
            {
                DeactivateFlare();
                return;
            }
            int projectileIndex = FindFlareProjectileIndex();
            if (projectileIndex < 0)
            {
                DeactivateFlare();
                return;
            }
            Projectile projectile = Main.projectile[projectileIndex];
            if (!IsOnScreen(projectile.Center, 80f))
            {
                DeactivateFlare();
                return;
            }
            if (!Filters.Scene[FilterKey].IsActive())
                Filters.Scene.Activate(FilterKey, projectile.Center);

            float time = Main.GlobalTimeWrappedHourly;
            float pulse = 0.5f + 0.5f * (float)Math.Sin(time * 5.0f);

            Filters.Scene[FilterKey].GetShader()
                .UseTargetPosition(projectile.Center)
                .UseIntensity(0.85f)
                .UseOpacity(1f)
                .UseProgress(pulse)
                .UseColor(new Color(255, 190, 80))
                .UseSecondaryColor(new Color(120, 180, 255));
        }
        private static int FindFlareProjectileIndex()
        {
            return -1;
        }
        private static bool IsOnScreen(Vector2 worldPosition, float padding)
        {
            Vector2 screenPosition = worldPosition - Main.screenPosition;
            return screenPosition.X > -padding
                && screenPosition.X < Main.screenWidth + padding
                && screenPosition.Y > -padding
                && screenPosition.Y < Main.screenHeight + padding;
        }
        private void DeactivateFlare()
        {
            if (Filters.Scene[FilterKey].IsActive())
                Filters.Scene.Deactivate(FilterKey);
        }
    }
    #region Echo
    public sealed class ShaderEchoSystem : ModSystem
    {
        private const string FilterKey = "jarkos:AxePulseShockwave";
        private static Vector2 pulseCenter;
        private static float pulseAge;
        private static float pulsePower;
        private static bool pulseActive;
        private bool registered;
        public static void SpawnPulse(Vector2 worldCenter, float power)
        {
            if (Main.dedServ)
                return;
            pulseCenter = worldCenter;
            pulseAge = 0f;
            pulsePower = MathHelper.Clamp(power, 0.1f, 2f);
            pulseActive = true;
        }
        public override void Load()
        {
            if (Main.dedServ)
                return;

            Filters.Scene[FilterKey] = new Filter(new ScreenShaderData(ModContent.Request<Effect>("Jarkos/Shaders/ShaderEcho", AssetRequestMode.ImmediateLoad), "Pass1"), EffectPriority.VeryHigh);
            Filters.Scene[FilterKey].Load();
            registered = true;
        }
        public override void Unload()
        {
            if (!Main.dedServ && registered)
                Filters.Scene.Deactivate(FilterKey);

            pulseActive = false;
            pulseAge = 0f;
            pulsePower = 0f;
            registered = false;
        }
        public override void PostUpdateEverything()
        {
            if (Main.dedServ || !registered)
                return;
            if (Main.gameMenu || !pulseActive || !IsOnScreen(pulseCenter, 120f))
            {
                DeactivatePulse();
                return;
            }
            pulseAge += 1f / 34f;
            if (pulseAge >= 1f)
            {
                DeactivatePulse();
                return;
            }
            if (!Filters.Scene[FilterKey].IsActive())
                Filters.Scene.Activate(FilterKey, pulseCenter);

            float fade = 1f - pulseAge;
            Filters.Scene[FilterKey].GetShader()
                .UseTargetPosition(pulseCenter)
                .UseIntensity(pulsePower * fade)
                .UseOpacity(1f)
                .UseProgress(pulseAge)
                .UseColor(new Color(255, 185, 70))
                .UseSecondaryColor(new Color(255, 80, 35));
        }
        private static bool IsOnScreen(Vector2 worldPosition, float padding)
        {
            Vector2 screenPosition = worldPosition - Main.screenPosition;
            return screenPosition.X > -padding
                && screenPosition.X < Main.screenWidth + padding
                && screenPosition.Y > -padding
                && screenPosition.Y < Main.screenHeight + padding;
        }
        private static void DeactivatePulse()
        {
            pulseActive = false;
            if (Filters.Scene[FilterKey].IsActive())
                Filters.Scene.Deactivate(FilterKey);
        }
    }
    #region Rainbow Trail
    public sealed class ShaderRainbowTrail : ModSystem
    {
        public static Asset<Effect> RainbowFireTrail;
        public override void Load()
        {
            if (Main.dedServ)
                return;
            RainbowFireTrail = ModContent.Request<Effect>("Jarkos/Shaders/RainbowTrailEffect", AssetRequestMode.ImmediateLoad);
        }
        public override void Unload()
        {
            RainbowFireTrail = null;
        }
    }
    #region Celestia Arrow Rain Telegraph
    public class CelestiaShaderSystem : ModSystem
	{
		public static Asset<Effect> ArrowRainTelegraphShader { get; private set; }
		public override void Load()
		{
			if (!Main.dedServ)
				ArrowRainTelegraphShader = ModContent.Request<Effect>($"{Mod.Name}/Shaders/CelestiaArrowRainTelegraph", AssetRequestMode.ImmediateLoad);
		}
		public override void Unload()
		{
			ArrowRainTelegraphShader = null;
		}
	}
    #region Phase Blood Shield
    public class PhaseBloodShieldOverlaySystem : ModSystem
    {
        public const string ShaderKey = "jarkos:PhaseBloodShieldSphereOverlay";
        private static Asset<Texture2D> sphereMap;
        public override void Load()
        {
            if (Main.dedServ)
                return;
            sphereMap = ModContent.Request<Texture2D>("Jarkos/textures/VoronoiShapes_shader", AssetRequestMode.ImmediateLoad);
            GameShaders.Misc[ShaderKey] = new MiscShaderData(ModContent.Request<Effect>("Jarkos/Shaders/PhaseBloodShieldDistortion", AssetRequestMode.ImmediateLoad), "Pass1").UseImage1(sphereMap);
        }
        public override void Unload()
        {
            sphereMap = null;
        }
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int index = layers.FindIndex(layer => layer.Name == "Vanilla: Mouse Text");
            if (index == -1)
                return;
            layers.Insert(index, new LegacyGameInterfaceLayer("jarkos: Phase Blood Shield Sphere", DrawShieldLayer, InterfaceScaleType.Game));
        }
        private static bool DrawShieldLayer()
        {
            if (Main.gameMenu || Main.netMode == NetmodeID.Server)
                return true;
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (!player.active || player.dead)
                    continue;

                PhaseBloodPlayer phasePlayer = player.GetModPlayer<PhaseBloodPlayer>();
                if (!phasePlayer.shieldEquipped)
                    continue;

                DrawShield(player, phasePlayer);
            }
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.UIScaleMatrix);
            return true;
        }
        private static void DrawShield(Player player, PhaseBloodPlayer phasePlayer)
{
    Texture2D texture = sphereMap.Value;
    Rectangle sourceRect = new Rectangle(0, 0, texture.Width, texture.Height);

    float radius = phasePlayer.isDashing ? 108f : 92f;
    float opacity = phasePlayer.isDashing ? 0.88f : 0.68f;

    Color mainColor = phasePlayer.phaseImmune
        ? new Color(130, 70, 255)
        : new Color(170, 45, 20);
    Color edgeColor = phasePlayer.phaseImmune
        ? new Color(235, 105, 255)
        : new Color(255, 95, 25);

    float rotationSpeed = phasePlayer.phaseImmune ? 1.20f : 0.85f;
    float textureStrength = 1.35f;
    float edgeStrength = phasePlayer.isDashing ? 1.20f : 1.00f;
    float centerCutout = 0.35f;

    MiscShaderData shader = GameShaders.Misc[ShaderKey]
        .UseColor(mainColor)
        .UseSecondaryColor(edgeColor)
        .UseOpacity(opacity)
        .UseShaderSpecificData(new Vector4(rotationSpeed, textureStrength, edgeStrength, centerCutout));
    DrawData drawData = new DrawData(texture, player.Center - Main.screenPosition, sourceRect, Color.White, 0f, sourceRect.Size() * 0.5f, new Vector2((radius * 2f) / texture.Width, (radius * 2f) / texture.Height), SpriteEffects.None, 0);

    shader.Apply(drawData);
    drawData.Draw(Main.spriteBatch);
    }
    }
    #region Portal Screen
    public sealed class PortalScreenSystem : ModSystem
    {
    public const string DistortionFilterKey = "Jarkos:PortalDistortion";
    private static bool registered;
    public override void Load()
    {
        if (Main.dedServ)
            return;

        Asset<Effect> distortionAsset = ModContent.Request<Effect>("Jarkos/Shaders/PortalDistortion", AssetRequestMode.ImmediateLoad);
        Filters.Scene[DistortionFilterKey] = new Filter(new ScreenShaderData(distortionAsset, "Pass1"), EffectPriority.High);
        Filters.Scene[DistortionFilterKey].Load();

        registered = true;
    }
    public override void Unload()
    {
        if (!Main.dedServ && registered && Filters.Scene[DistortionFilterKey] != null
            && Filters.Scene[DistortionFilterKey].IsActive())
        {
            Filters.Scene.Deactivate(DistortionFilterKey);
        }
        registered = false;
    }
    public static bool IsActive => registered
        && Filters.Scene[DistortionFilterKey] != null
        && Filters.Scene[DistortionFilterKey].IsActive();
    public static void Activate(Vector2 worldCenter)
    {
        if (Main.dedServ || !registered)
            return;
        if (!IsActive)
            Filters.Scene.Activate(DistortionFilterKey, worldCenter);
    }
    public static void Deactivate()
    {
        if (Main.dedServ || !registered)
            return;
        if (IsActive)
            Filters.Scene.Deactivate(DistortionFilterKey);
    }
    public static void Update(Vector2 worldCenter, float progress, float intensity, float opacity, Color primary, Color secondary)
    {
        if (Main.dedServ || !registered || !IsActive)
            return;
            
        Filters.Scene[DistortionFilterKey].GetShader()
            .UseTargetPosition(worldCenter)
            .UseProgress(progress)
            .UseIntensity(intensity)
            .UseOpacity(opacity)
            .UseColor(primary.ToVector3())
            .UseSecondaryColor(secondary.ToVector3())
            .UseImageOffset(new Vector2(Main.screenWidth, Main.screenHeight));
    }
    public static bool IsOnScreen(Vector2 worldPosition, float padding = 200f)
    {
        Vector2 screenPosition = worldPosition - Main.screenPosition;
        return screenPosition.X > -padding
            && screenPosition.X < Main.screenWidth + padding
            && screenPosition.Y > -padding
            && screenPosition.Y < Main.screenHeight + padding;
    }
    }
    #region Pixelated Trail Nox
    public class NoxShaderSystem : ModSystem
    {
        public const string NoxPixelTrailShaderKey = "jarkos:NoxPixelTrail";
        public override void Load()
        {
            if (Main.dedServ)
                return;
            GameShaders.Misc[NoxPixelTrailShaderKey] = new MiscShaderData(ModContent.Request<Effect>("Jarkos/Shaders/NoxPixelTrail", AssetRequestMode.ImmediateLoad), "Pass1");
        }
        public override void Unload()
        {
            if (!Main.dedServ)
                GameShaders.Misc.Remove(NoxPixelTrailShaderKey);
        }
    }
    #region Collapsar Slash Distortion
    public sealed class CollapsarSlashDistortionSystem : ModSystem
    {
        public const string FilterKey = "jarkos:CollapsarSlashDistortion";
        private static bool queued;
        private static Vector2 targetPosition;
        private static Vector2 direction = Vector2.UnitX;
        private static float targetIntensity;
        private static float opacity;
        private static float radius;
        private static float sideWidth;
        private static float tailLength;
        private static float tipDistance;
        private static float chromaticStrength;
        private static float brightness;
        public override void Load()
        {
            if (Main.dedServ)
                return;
            
            Asset<Effect> shaderAsset = ModContent.Request<Effect>("Jarkos/Shaders/CollapsarSlashDistortion", AssetRequestMode.ImmediateLoad);
			Filters.Scene[FilterKey] = new Filter(new ScreenShaderData(shaderAsset, "SlashDistortion"), EffectPriority.High);
            Filters.Scene[FilterKey].Load();
        }
        public override void Unload()
        {
            queued = false;
        }
        public override void PostUpdateEverything()
        {
            if (Main.dedServ)
                return;
            if (!queued || opacity <= 0.01f || targetIntensity <= 0.01f)
            {
                Filters.Scene.Deactivate(FilterKey);
                queued = false;
                opacity = 0f;
                targetIntensity = 0f;
                return;
            }
            Filter filter = Filters.Scene[FilterKey];
            if (!filter.IsActive())
                Filters.Scene.Activate(FilterKey, targetPosition);

            filter.GetShader()
                .UseTargetPosition(targetPosition)
                .UseDirection(direction)
                .UseIntensity(targetIntensity)
                .UseOpacity(opacity)
                .UseProgress((Main.GameUpdateCount % 600) / 600f)
                .UseColor(new Color(MathHelper.Clamp(targetIntensity, 0f, 1f), chromaticStrength, tipDistance))
                .UseSecondaryColor(new Color(MathHelper.Clamp(radius / Main.screenHeight, 0.05f, 0.45f), MathHelper.Clamp(sideWidth / Main.screenHeight, 0.015f, 0.22f), MathHelper.Clamp(tailLength / Main.screenHeight, 0.04f, 0.60f)));

            opacity *= 0.82f;
            targetIntensity *= 0.84f;
            radius = MathHelper.Lerp(80f, radius, 0.93f);
            sideWidth = MathHelper.Lerp(28f, sideWidth, 0.9f);
            tailLength = MathHelper.Lerp(80f, tailLength, 0.92f);
            tipDistance = MathHelper.Lerp(0.04f, tipDistance, 0.9f);
            chromaticStrength *= 0.82f;
            brightness *= 0.85f;
        }
        public static void QueueSlash(Vector2 position, Vector2 slashDirection, float swingSpeed, float effectRadius = 170f, float effectSideWidth = 48f, float effectTailLength = 110f)
        {
            if (Main.dedServ)
                return;
            if (slashDirection.LengthSquared() < 0.001f)
                slashDirection = Vector2.UnitX;
            slashDirection.Normalize();

            queued = true;
            targetPosition = position;
            direction = slashDirection;

            float speedFactor = MathHelper.Clamp(swingSpeed / 0.45f, 0f, 1f);
            targetIntensity = MathHelper.Clamp(targetIntensity + 0.16f + speedFactor * 0.54f, 0f, 0.82f);
            opacity = MathHelper.Clamp(opacity + 0.30f + speedFactor * 0.28f, 0f, 0.86f);
            radius = MathHelper.Clamp(effectRadius, 100f, 320f);
            sideWidth = MathHelper.Clamp(effectSideWidth, 28f, 125f);
            tailLength = MathHelper.Clamp(effectTailLength * 0.5f, 55f, 230f);
            tipDistance = MathHelper.Clamp(effectRadius / Main.screenHeight * 0.45f, 0.035f, 0.20f);
            chromaticStrength = MathHelper.Clamp(0.32f + speedFactor * 0.58f, 0f, 0.90f);
            brightness = MathHelper.Clamp(0.20f + speedFactor * 0.34f, 0f, 0.60f);
        }
    }
    #region Celestia Energy Burst
    public sealed class CelestiaEnergyBurstSystem : ModSystem
	{
		private const string FilterKey = "jarkos:CelestiaEnergyBurst";
		private const int Lifetime = 24;
		private static bool registered;
		private static Vector2 center;
		private static int timer;
		private static float intensity;
		public override void Load()
		{
			if (Main.dedServ)
				return;

            Asset<Effect> shaderAsset = ModContent.Request<Effect>("Jarkos/Shaders/CelestiaEnergyBurst", AssetRequestMode.ImmediateLoad);
			Filters.Scene[FilterKey] = new Filter(new ScreenShaderData(shaderAsset, "EnergyBurst"), EffectPriority.High);
			Filters.Scene[FilterKey].Load();
			registered = true;
		}
		public override void Unload()
		{
			if (!Main.dedServ && registered && Filters.Scene[FilterKey].IsActive())
				Filters.Scene.Deactivate(FilterKey);

			registered = false;
			timer = 0;
			intensity = 0f;
			center = Vector2.Zero;
		}
		public override void PostUpdateEverything()
		{
			if (Main.dedServ || !registered)
				return;
			if (timer <= 0)
			{
				if (Filters.Scene[FilterKey].IsActive())
					Filters.Scene.Deactivate(FilterKey);
				return;
			}
			if (!IsOnScreen(center, 260f))
			{
				timer = 0;
				if (Filters.Scene[FilterKey].IsActive())
					Filters.Scene.Deactivate(FilterKey);
				return;
			}
			float progress = 1f - timer / (float)Lifetime;
			float opacity = 1f - progress;

			if (!Filters.Scene[FilterKey].IsActive())
				Filters.Scene.Activate(FilterKey, center);

			Filters.Scene[FilterKey].GetShader()
				.UseTargetPosition(center)
				.UseProgress(progress)
				.UseIntensity(intensity)
				.UseOpacity(opacity)
				.UseColor(new Color(235, 245, 255))
				.UseSecondaryColor(new Color(135, 85, 255))
				.UseImageOffset(new Vector2(Main.screenWidth, Main.screenHeight));

			timer--;
		}
		public static void Burst(Vector2 worldCenter, float power = 1f)
		{
			if (Main.dedServ || !registered || !IsOnScreen(worldCenter, 260f))
				return;
			center = worldCenter;
			timer = Lifetime;
			intensity = MathHelper.Clamp(power, 0.35f, 1.6f);
		}
		private static bool IsOnScreen(Vector2 worldPosition, float padding)
		{
			Vector2 screenPosition = worldPosition - Main.screenPosition;
			return screenPosition.X > -padding
				&& screenPosition.X < Main.screenWidth + padding
				&& screenPosition.Y > -padding
				&& screenPosition.Y < Main.screenHeight + padding;
		}
	}
    #region Blood Laser
    public class BloodLaserDrawSystem : ModSystem
    {
        public const string BloodLaserShaderKey = "jarkos:BloodLaserOnePass";
        public override void Load()
        {
            if (Main.dedServ)
                return;
            GameShaders.Misc[BloodLaserShaderKey] = new MiscShaderData(ModContent.Request<Effect>("Jarkos/Shaders/BloodLaser", AssetRequestMode.ImmediateLoad), "Pass1");
        }
        public override void PostDrawTiles()
        {
            if (Main.dedServ)
                return;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Rectangle sourceRect = new Rectangle(0, 0, 1, 1);
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile projectile = Main.projectile[i];
                if (!projectile.active || projectile.type != ModContent.ProjectileType<BloodLaserProjectile>())
                    continue;
                DrawLaser(projectile, pixel, sourceRect);
            }
            Main.spriteBatch.End();
        }
        private static void DrawLaser(Projectile projectile, Texture2D pixel, Rectangle sourceRect)
        {
            Vector2 direction = projectile.velocity.SafeNormalize(-Vector2.UnitY);

            float length = BloodLaserProjectile.GetLaserLength(projectile.Center, direction);
            float rotation = direction.ToRotation();
            float opacity = projectile.Opacity;

            Color beamColor = new Color(255, 30, 170);
            Color auraColor = new Color(220, 18, 70);
            
            MiscShaderData shader = GameShaders.Misc[BloodLaserShaderKey]
                .UseColor(beamColor)
                .UseSecondaryColor(auraColor)
                .UseOpacity(opacity)
                .UseShaderSpecificData(new Vector4(6.0f, 0.055f, 0f, 0f));
            shader.Apply();

            Main.spriteBatch.Draw(pixel, projectile.Center - Main.screenPosition, sourceRect, Color.White, rotation, new Vector2(0f, 0.5f), new Vector2(length, BloodLaserProjectile.LaserWidth), SpriteEffects.None, 0f);
        }
        #region WatcherShader
      }
    public class AegisEffects : ModSystem
    {
        private const string ShaderPath = "Shaders/";
        public static Asset<Effect> TrailShader;
        public static Asset<Effect> MarkShader;
        public static bool ShadersLoaded { get; private set; }
        public override void Load()
        {
            if (Main.dedServ)
                return;

            bool hasTrail = Mod.HasAsset(ShaderPath + "SwingTrail");
            bool hasMark = Mod.HasAsset(ShaderPath + "Mark");

            if (hasTrail && hasMark) 
            {
                TrailShader = Mod.Assets.Request<Effect>(ShaderPath + "SwingTrail");
                MarkShader = Mod.Assets.Request<Effect>(ShaderPath + "Mark");
                ShadersLoaded = true;
            }
            else 
            {
                ShadersLoaded = false;
            }
            Mod.Logger.Info($"[Aegis] Shaders loaded: {ShadersLoaded} (SwingTrail: {hasTrail}, Mark: {hasMark})");
        }
        public override void OnWorldLoad()
        {
            if (Main.dedServ)
                return;
        }
        public override void Unload()
        {
            TrailShader = null;
            MarkShader = null;
            ShadersLoaded = false;
        }
    }
    #region Bastion
    public class BastionShaders : ModSystem
    {
        public static Effect Flame { get; private set; }
        public static Effect Digit { get; private set; }
        public override void Load()
        {
            if (Main.dedServ)
                return;
            try
            {
                Flame = Mod.Assets.Request<Effect>("Shaders/BastionFlame", AssetRequestMode.ImmediateLoad).Value;
            }
            catch
            {
                Flame = null;
            }
            try
            {
                Digit = Mod.Assets.Request<Effect>("Shaders/BastionDigit", AssetRequestMode.ImmediateLoad).Value;
            }
            catch
            {
                Digit = null;
            }
        }
        public override void Unload()
        {
            Flame = null;
            Digit = null;
        }
    }
    #region Frost
    public class FrostShaders : ModSystem
    {
        public static Effect FrostStream;
        public static Texture2D FrostNoise;
        public override void Load()
        {
            if (Main.netMode == NetmodeID.Server)
                return;
            FrostStream = TryLoadEffect("FrostStreamTrail");
            FrostNoise = ModContent.Request<Texture2D>("jarkos/textures/FrostNoise", AssetRequestMode.ImmediateLoad).Value;
        }
        public override void Unload()
        {
            FrostStream = null;
            FrostNoise = null;
        }
        private static Effect TryLoadEffect(string name)
        {
            try
            {
                return ModContent.Request<Effect>($"jarkos/Shaders/{name}", AssetRequestMode.ImmediateLoad).Value;
            }
            catch
            {
                return null;
            }
        }
    }
    #region Devil
    public class DevilShaders : ModSystem
    {
        public static Effect AirSlice;
        public static Effect PointedTrail;
        public static Effect EnergyRing;
        public static Texture2D Noise;
        public override void Load()
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            AirSlice = TryLoadEffect("AirSliceTrail");
            PointedTrail = TryLoadEffect("PointedTrail");
            EnergyRing = TryLoadEffect("EnergyRing");
            Noise = ModContent.Request<Texture2D>("Jarkos/textures/EnergyNoise", AssetRequestMode.ImmediateLoad).Value;
        }
        public override void Unload()
        {
            AirSlice = null;
            PointedTrail = null;
            EnergyRing = null;
            Noise = null;
        }
        private static Effect TryLoadEffect(string name)
        {
            try
            {
                return ModContent.Request<Effect>($"Jarkos/Shaders/{name}", AssetRequestMode.ImmediateLoad).Value;
            }
            catch
            {
                return null;
            }
        }
        public static Matrix WorldViewProjection
        {
            get
            {
                Matrix view = Main.GameViewMatrix.TransformationMatrix;
                Matrix projection = Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, -1f, 1f);
                return view * projection;
            }
        }
    }
    #region Miasma
    public class MiasmaShaders : ModSystem
    {
        public static Effect Slash { get; private set; }
        public static Effect Shatter { get; private set; }
        public const string ShatterFilterName = "Jarkos:MiasmaShatter";
        public override void Load()
        {
            if (Main.dedServ)
                return;

            Asset<Effect> slashAsset = TryLoadAsset("jarkos/Shaders/MiasmaSlash");
            if (slashAsset != null) Slash = slashAsset.Value;

            Asset<Effect> shatterAsset = TryLoadAsset("jarkos/Shaders/MiasmaShatter");

            if (shatterAsset != null && shatterAsset.Value != null)
            {
                Filters.Scene[ShatterFilterName] = new Filter(new ScreenShaderData(shatterAsset, "P0"), EffectPriority.VeryHigh);
                Filters.Scene[ShatterFilterName].Load();
            }
        }
        public override void Unload()
        {
            Slash = null;
            Shatter = null;
        }
        private Asset<Effect> TryLoadAsset(string path)
        {
            try
            {
                return ModContent.Request<Effect>(path, AssetRequestMode.ImmediateLoad);
            }
            catch
            {
                return null;
            }
        }
    }
    #region Aqueous
    public class AqueousShaderSystem : ModSystem
    {
        private Asset<Effect> trailAsset;
        private Asset<Texture2D> perlinAsset;
        public static Effect TrailEffect { get; private set; }
        public static Texture2D PerlinNoise { get; private set; }
        public static bool CustomShaderLoaded => TrailEffect != null;
        public override void Load()
        {
            TrailEffect = null;
            PerlinNoise = null;

            if (Main.dedServ)
                return;

            perlinAsset = Main.Assets.Request<Texture2D>("Images/Misc/Perlin", AssetRequestMode.ImmediateLoad);
            PerlinNoise = perlinAsset.Value;

            string fxcPath = "Shaders/AqueousTrail.fxc";
            string xnbPath = "Shaders/AqueousTrail.xnb";
            string assetPath = "Shaders/AqueousTrail";

            bool hasFxc = Mod.FileExists(fxcPath);
            bool hasXnb = Mod.FileExists(xnbPath);

            if (hasFxc || hasXnb)
            {
                trailAsset = Mod.Assets.Request<Effect>(assetPath, AssetRequestMode.ImmediateLoad);
                TrailEffect = trailAsset.Value;
            }
        }
        public override void Unload()
        {
            TrailEffect = null;
            PerlinNoise = null;
        }
    }
    #region AqueousPath
    public class AqueousPathRenderer : ModSystem
    {
        private static readonly VertexStrip Strip = new();
        private static Asset<Effect> _effect;
        public override void Load()
        {
            if (Main.dedServ) return;
            _effect = ModContent.Request<Effect>("jarkos/Shaders/AqueousPath", AssetRequestMode.ImmediateLoad);
        }
    
        public override void Unload() => _effect = null;

        private static Vector2[] _tempPos = new Vector2[500]; //for all path 500 points
        private static Vector2[] _tempPos2 = new Vector2[500]; //for interpolate path but working not so good
        private static List<Vector2> _visualPath = new(500);
        public override void PostDrawTiles()
        {
            for (int playerIndex = 0; playerIndex < Main.maxPlayers; playerIndex++)
            {
                Player plr = Main.player[playerIndex];
                if (plr is null || !plr.active) continue;
                
                var mp = plr.GetModPlayer<AqueousPathPlayer>();

                bool drawing = mp.State == AqueousPathPlayer.PathState.Drawing;
                bool flying = mp.State == AqueousPathPlayer.PathState.Flying;
                bool dissolving = mp.State == AqueousPathPlayer.PathState.None && mp.DissolveTimer > 0;

                if ((!drawing && !flying && !dissolving) || mp.Path.Count < 2) continue;

                Effect fx = _effect?.Value;
                if (fx is null) continue;

                int count = mp.Path.Count;
                if (count > _tempPos.Length)
                {
                    _tempPos = new Vector2[count + 100];
                    _tempPos2 = new Vector2[count + 100];
                }

                for (int i = 0; i < count; i++)
                    _tempPos[i] = mp.Path[i]; //if mp.Path "longer"

                if (count >= 3)
                {
                    for (int q = 0; q < 20; q++)
                    {
                        _tempPos2[0] = _tempPos[0]; //fixation of the start and last points
                        _tempPos2[count - 1] = _tempPos[count - 1];
                        
                        for (int i = 1; i < count - 1; i++)
                            _tempPos2[i] = (_tempPos[i - 1] + _tempPos[i] * 2f + _tempPos[i + 1]) * 0.25f;

                        Array.Copy(_tempPos2, _tempPos, count);
                    }
                }

                _visualPath.Clear();
                _visualPath.Add(_tempPos[0]);
                
                float distAccum = 0f;
                float step = 8f;

                for (int i = 0; i < count - 1; i++)
                {
                    Vector2 p0 = _tempPos[i];
                    Vector2 p1 = _tempPos[i + 1];
                    float segLength = Vector2.Distance(p0, p1);

                    Vector2 dir = Vector2.Zero;

                    Vector2 rr = p1 - p0;

                    if(segLength > 0)
                    {
                        rr.Normalize();
                        dir = rr;
                    }

                    //Vector2 dir = segLength > 0 ? rr.Normalize() : Vector2.Zero;

                    while (distAccum + segLength >= step)
                    {
                        float d = step - distAccum;
                        Vector2 pt = p0 + dir * d;

                        _visualPath.Add(pt);

                        distAccum = 0f;
                        segLength -= d;
                        p0 = pt;
                    }
                    
                    distAccum += segLength;
                }

                var last = _visualPath.Count - 1;
                Vector2 lastp = _visualPath[last];

                if (Vector2.Distance(lastp, _tempPos[count - 1]) > 1f)
                    _visualPath.Add(_tempPos[count - 1]);
                    
                int visCount = _visualPath.Count;
                if (visCount < 2) continue;

                Vector2[] positions = new Vector2[visCount];
                float[] rotations = new float[visCount];

                for (int i = 0; i < visCount; i++)
                {
                    positions[i] = _visualPath[i];
                    Vector2 d = i < visCount - 1 ? _visualPath[i + 1] - _visualPath[i] : _visualPath[i] - _visualPath[i - 1];
                    rotations[i] = d.ToRotation();
                }

                float pathLen = Math.Max(mp.PathLength, 1f);
                float headT = drawing ? 1f : MathHelper.Clamp(mp.FlightDistance / pathLen, 0f, 1f);
                float dissolve = dissolving ? mp.DissolveTimer / (float)AqueousPathPlayer.DissolveTime : 1f;

                float time = Main.GlobalTimeWrappedHourly;

                float AlphaAt(float p)
                {
                    if (drawing)
                        return 0.5f + 0.15f * MathF.Sin(time * 8f + p * 25f);

                    float tail = 900f / pathLen;
                    float lead = 80f / pathLen;
                    float a;

                    if (p > headT + lead) a = 0f;
                    else if (p > headT) a = 1f - (p - headT) / lead;
                    else a = MathHelper.Clamp(1f - (headT - p) / tail, 0f, 1f);
                    return a * dissolve;
                }

                float PhaseAt(float p)
                {
                    if (mp.PointPhase.Length < 2) return 0f;
                    float fi = p * (mp.PointPhase.Length - 1);
                    int i0 = (int)fi;
                    int i1 = Math.Min(i0 + 1, mp.PointPhase.Length - 1);

                    return MathHelper.Lerp(mp.PointPhase[i0], mp.PointPhase[i1], fi - i0);
                }

                Color ColorFunc(float p)
                {
                    float headGlow = drawing ? 0f : MathF.Exp(-MathF.Abs(p - headT) * 18f);

                    return new Color(drawing ? 0.8f : 1f, PhaseAt(p), headGlow, AlphaAt(p));
                }
                float WidthFunc(float p)
                {
                    if (drawing)
                        return 7f + 2.5f * MathF.Sin(time * 7f + p * 30f);

                    float taper = MathF.Min(1f, MathF.Min(p, 1f - p) * 10f + 0.05f);
                    float pulse = 1f + 0.16f * MathF.Sin(time * 10f + p * 34f);
                    float headBoost = 1f + 1.3f * MathF.Exp(-MathF.Abs(p - headT) * 16f);

                    return 30f * taper * pulse * headBoost * (0.35f + 0.65f * dissolve);
                }

                GraphicsDevice gd = Main.instance.GraphicsDevice;
                gd.RasterizerState = RasterizerState.CullNone;
                gd.BlendState = BlendState.AlphaBlend;
                gd.SamplerStates[0] = SamplerState.LinearWrap;

                Matrix view = Main.GameViewMatrix.TransformationMatrix;
                Matrix proj = Matrix.CreateOrthographicOffCenter(0f, gd.Viewport.Width, gd.Viewport.Height, 0f, -1f, 1f);

                fx.Parameters["uTransform"]?.SetValue(view * proj);
                fx.Parameters["uTime"]?.SetValue(time);
                fx.Parameters["uGlobalPhase"]?.SetValue(mp.PhaseGlow);

                Strip.PrepareStripWithProceduralPadding(positions, rotations, ColorFunc, WidthFunc, -Main.screenPosition, includeBacksides: true);
                fx.CurrentTechnique.Passes[0].Apply();
                Strip.DrawTrail();
            }
        }
    }

    public class SporeShaders : ModSystem
    {
        public static Effect Orb { get; private set; }
        public static Effect Ring { get; private set; }
        public override void Load()
        {
            if (Main.dedServ)
                return;
            try
            {
                Orb = Mod.Assets.Request<Effect>("Shaders/SporeOrb", AssetRequestMode.ImmediateLoad).Value;
            }
            catch
            {
                Orb = null;
            }
            try
            {
                Ring = Mod.Assets.Request<Effect>("Shaders/SporeRing", AssetRequestMode.ImmediateLoad).Value;
            }
            catch
            {
                Ring = null;
            }
        }
        public override void Unload()
        {
            Orb = null;
            Ring = null;
        }
    }

    public sealed class PortalShaderTestSystem : ModSystem
    {
        public const string FilterKey = "Jarkos:PortalShaderTest";
        private static bool registered;
        public override void Load()
        {
            if (Main.dedServ)
                return;
            Asset<Effect> shaderAsset = ModContent.Request<Effect>("Jarkos/Shaders/PortalShaderTest", AssetRequestMode.ImmediateLoad);
            Filters.Scene[FilterKey] = new Filter(new ScreenShaderData(shaderAsset, "Pass1"), EffectPriority.High);
            Filters.Scene[FilterKey].Load();
            registered = true;
        }
        public override void Unload()
        {
            if (!Main.dedServ && registered && Filters.Scene[FilterKey] != null && Filters.Scene[FilterKey].IsActive())
            {
                Filters.Scene.Deactivate(FilterKey);
            }
            registered = false;
        }
        public static bool IsActive => registered && Filters.Scene[FilterKey] != null && Filters.Scene[FilterKey].IsActive();
        public static void Activate(Vector2 worldCenter)
        {
            if (Main.dedServ || !registered)
                return;
            if (!IsActive)
                Filters.Scene.Activate(FilterKey, worldCenter);
        }
        public static void Deactivate()
        {
            if (Main.dedServ || !registered)
                return;
            if (IsActive)
                Filters.Scene.Deactivate(FilterKey);
        }
        public static void Update(Vector2 worldCenter, float progress, float intensity, float opacity, Color primary, Color secondary)
        {
            if (Main.dedServ || !registered || !IsActive)
                return;
            Filters.Scene[FilterKey].GetShader()
                .UseTargetPosition(worldCenter)
                .UseProgress(progress)
                .UseIntensity(intensity)
                .UseOpacity(opacity)
                .UseColor(primary.ToVector3())
                .UseSecondaryColor(secondary.ToVector3())
                .UseImageOffset(new Vector2(Main.screenWidth, Main.screenHeight));
        }
        public static bool IsOnScreen(Vector2 worldPosition, float padding = 200f)
        {
            Vector2 screenPosition = worldPosition - Main.screenPosition;
            return screenPosition.X > -padding
                && screenPosition.X < Main.screenWidth + padding
                && screenPosition.Y > -padding
                && screenPosition.Y < Main.screenHeight + padding;
        }
    }

    public sealed class ZodiacFlowShader : ModSystem
	{
		public const string Key = "jarkos:ZodiacFlow";
		private const string AssetPath = "Jarkos/Shaders/ZodiacFlow";
		public override void Load()
		{
			if (Main.dedServ)
				return;
			Asset<Effect> effectAsset = ModContent.Request<Effect>(AssetPath, AssetRequestMode.ImmediateLoad);
			GameShaders.Misc[Key] = new MiscShaderData(effectAsset, "Pass1");
		}
		public override void Unload()
		{
			if (!Main.dedServ && GameShaders.Misc.ContainsKey(Key))
				GameShaders.Misc.Remove(Key);
		}
	}

    public sealed class MultiwaveShaderSystem : ModSystem
    {
        public static Effect CoreShader { get; private set; }
        public static Effect ExplosionShader { get; private set; }
        public const string AuraFilterKey = "Jarkos:MultiwaveAuraDistortion";
        private static bool registered;
        public override void Load()
        {
            if (Main.dedServ)
                return;
            try
            {
                CoreShader = Mod.Assets.Request<Effect>("Shaders/MultiwaveCore", AssetRequestMode.ImmediateLoad).Value;
            }
            catch
            {
                CoreShader = null;
            }
            try
            {
                ExplosionShader = Mod.Assets.Request<Effect>("Shaders/MultiwaveExplosion", AssetRequestMode.ImmediateLoad).Value;
            }
            catch
            {
                ExplosionShader = null;
            }
            try
            {
                Asset<Effect> auraAsset = Mod.Assets.Request<Effect>("Shaders/MultiwaveAuraDistortion", AssetRequestMode.ImmediateLoad);
                Asset<Texture2D> waveNoise = Mod.Assets.Request<Texture2D>("textures/Noises/WaveNoise", AssetRequestMode.ImmediateLoad);
                ScreenShaderData shaderData = new ScreenShaderData(auraAsset, "Pass1");

                shaderData.UseImage(waveNoise, 1);

                Filters.Scene[AuraFilterKey] = new Filter(shaderData, EffectPriority.VeryHigh);
                Filters.Scene[AuraFilterKey].Load();

                registered = true;
            }
            catch
            {
                registered = false;
            }
        }
        public override void Unload()
        {
            CoreShader = null;
            ExplosionShader = null;

            if (!Main.dedServ && registered && Filters.Scene[AuraFilterKey] != null && Filters.Scene[AuraFilterKey].IsActive())
            {
                Filters.Scene.Deactivate(AuraFilterKey);
            }

            registered = false;
        }
        public static bool IsActive => registered && Filters.Scene[AuraFilterKey] != null && Filters.Scene[AuraFilterKey].IsActive();
        public static void Activate(Vector2 worldCenter)
        {
            if (Main.dedServ || !registered)
                return;

            if (!IsActive)
                Filters.Scene.Activate(AuraFilterKey, worldCenter);
        }
        public static void Deactivate()
        {
            if (Main.dedServ || !registered)
                return;

            if (IsActive)
                Filters.Scene.Deactivate(AuraFilterKey);
        }
        public static void Update(Vector2 worldCenter, float radius, float opacity, float progress, Color glowColor)
        {
            if (Main.dedServ || !registered || !IsActive)
                return;
                
            Filters.Scene[AuraFilterKey].GetShader()
                .UseTargetPosition(worldCenter)
                .UseIntensity(radius)
                .UseOpacity(opacity)
                .UseProgress(progress)
                .UseColor(glowColor.ToVector3())
                .UseImageOffset(new Vector2(Main.screenWidth, Main.screenHeight));
        }
        public static bool IsOnScreen(Vector2 worldPosition, float padding = 500f)
        {
            Vector2 screenPosition = worldPosition - Main.screenPosition;
            return screenPosition.X > -padding
                && screenPosition.X < Main.screenWidth + padding
                && screenPosition.Y > -padding
                && screenPosition.Y < Main.screenHeight + padding;
        }
    }

    public sealed class PurgatoryShaders : ModSystem
    {
        public const string ArrowRainTelegraphKey = "Jarkos:ArrowRainTelegraph";
        private static bool _telegraphFailureLogged;
        public override void Load()
        {
            if (Main.dedServ)
                return;
            try
            {
                Asset<Effect> effect = ModContent.Request<Effect>("Jarkos/Shaders/ArrowRainShader", AssetRequestMode.ImmediateLoad);
                if (effect?.Value != null)
                {
                    GameShaders.Misc[ArrowRainTelegraphKey] = new MiscShaderData(effect, "Pass1");
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
            }
        }
        public override void Unload()
        {
            if (GameShaders.Misc.ContainsKey(ArrowRainTelegraphKey))
            {
                GameShaders.Misc.Remove(ArrowRainTelegraphKey);
            }
            _telegraphFailureLogged = false;
        }
        public static void ReportTelegraphFailure(Exception ex)
        {
            if (_telegraphFailureLogged)
                return;
            _telegraphFailureLogged = true;
            ModContent.GetInstance<PurgatoryShaders>()?.Mod?.Logger?.Warn($"ArrowRainTelegraph shader runtime failure ({ex.GetType().Name}: {ex.Message}). " + "Switching to non-shader fallback for the rest of the session.");
        }
    }

    public class HealShaders : ModSystem
    {
        public static Effect Stream { get; private set; }
        public static Effect BloodStream { get; private set; }
        public static Texture2D HealNoise => noiseAsset?.Value;
        private static Asset<Texture2D> noiseAsset;
        public override void Load()
        {
            if (Main.dedServ)
                return;
            noiseAsset = ModContent.Request<Texture2D>("jarkos/textures/HealNoise");
            try
            {
                Stream = Mod.Assets.Request<Effect>("Shaders/HealStream", AssetRequestMode.ImmediateLoad).Value;
            }
            catch
            {
                Stream = null;
            }
            
            try
            {
                BloodStream = Mod.Assets.Request<Effect>("Shaders/BloodStream", AssetRequestMode.ImmediateLoad).Value;
            }
            catch
            {
                BloodStream = null;
            }
        }
        public override void Unload()
        {
            Stream = null;
            BloodStream = null;
            noiseAsset = null;
        }
    }

    public class EtherealShaderSystem : ModSystem
    {
        public static string EtherealDomainKey = "jarkos:EtherealDomain";

        public override void Load()
        {
            if (Main.dedServ) return;

            Filters.Scene[EtherealDomainKey] = new Filter(new ScreenShaderData(ModContent.Request<Effect>("jarkos/Shaders/EtherealThundercloud", ReLogic.Content.AssetRequestMode.ImmediateLoad), "Thundercloud"), EffectPriority.VeryHigh);
            Filters.Scene[EtherealDomainKey].Load();
        }
    }

    public class CelestiaProjectileShaderSystem : ModSystem
	{
		private const string ShaderAssetPrefix = "Jarkos/Shaders/";
		public static Effect TrailFire { get; private set; }
		public static Effect Glow { get; private set; }
		public static Effect PixelTrail { get; private set; }
		public override void Load()
		{
			if (Main.dedServ)
				return;
			TrailFire  = TryLoad(ShaderAssetPrefix + "CelestiaTrailFire");
			Glow       = TryLoad(ShaderAssetPrefix + "CelestiaProjectileGlow");
			PixelTrail = TryLoad(ShaderAssetPrefix + "CelestiaPixelTrail");
		}
		public override void Unload()
		{
			TrailFire = null;
			Glow = null;
			PixelTrail = null;
		}
		private static Effect TryLoad(string path)
		{
			Asset<Effect> asset = ModContent.Request<Effect>(path, AssetRequestMode.ImmediateLoad);
			return asset?.Value;
		}
		public static void DrawTrailFire(Vector2 pos, float rot, Vector2 size, float time, float opacity, float intensity, float pixelSize, Color hot, Color mid, Color cool)
		{
			if (TrailFire == null)
				return;
			TrailFire.Parameters["uTime"]?.SetValue(time);
			TrailFire.Parameters["uOpacity"]?.SetValue(opacity);
			TrailFire.Parameters["uIntensity"]?.SetValue(intensity);
			TrailFire.Parameters["uPixelSize"]?.SetValue(pixelSize);
			TrailFire.Parameters["uHotColor"]?.SetValue(hot.ToVector3());
			TrailFire.Parameters["uMidColor"]?.SetValue(mid.ToVector3());
			TrailFire.Parameters["uCoolColor"]?.SetValue(cool.ToVector3());
			DrawQuad(TrailFire, pos, rot, size, BlendState.Additive);
		}
		public static void DrawPixelTrail(Vector2 pos, float rot, Vector2 size, float time, float opacity, float pixelSize, Color colorA, Color colorB, Color colorC)
		{
			if (PixelTrail == null)
				return;
			PixelTrail.Parameters["uTime"]?.SetValue(time);
			PixelTrail.Parameters["uOpacity"]?.SetValue(opacity);
			PixelTrail.Parameters["uPixelSize"]?.SetValue(pixelSize);
			PixelTrail.Parameters["uColorA"]?.SetValue(colorA.ToVector3());
			PixelTrail.Parameters["uColorB"]?.SetValue(colorB.ToVector3());
			PixelTrail.Parameters["uColorC"]?.SetValue(colorC.ToVector3());
			DrawQuad(PixelTrail, pos, rot, size, BlendState.Additive);
		}
		public static void DrawGlow(Vector2 pos, float radius, float time, float opacity, float pulse, Color core, Color halo)
		{
			if (Glow == null)
				return;
			Glow.Parameters["uTime"]?.SetValue(time);
			Glow.Parameters["uOpacity"]?.SetValue(opacity);
			Glow.Parameters["uPulse"]?.SetValue(pulse);
			Glow.Parameters["uCoreColor"]?.SetValue(core.ToVector3());
			Glow.Parameters["uGlowColor"]?.SetValue(halo.ToVector3());
			float diameter = radius * 2f;
			Texture2D pixel = TextureAssets.MagicPixel.Value;
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, Glow, Main.GameViewMatrix.TransformationMatrix);
			Main.EntitySpriteDraw(pixel, pos, new Rectangle(0, 0, 1, 1), Color.White, 0f, new Vector2(0.5f, 0.5f), new Vector2(diameter, diameter), SpriteEffects.None, 0);
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
		}
		private static void DrawQuad(Effect effect, Vector2 pos, float rot, Vector2 size, BlendState blend)
		{
			Texture2D pixel = TextureAssets.MagicPixel.Value;
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, blend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, effect, Main.GameViewMatrix.TransformationMatrix);
			Main.EntitySpriteDraw(pixel, pos, new Rectangle(0, 0, 1, 1), Color.White, rot, new Vector2(0f, 0.5f), size, SpriteEffects.None, 0);
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
		}
	}

    public class SunfateScreenSystem : ModSystem
    {
        public const string FilterKey = "Jarkos:SunfateContrastScreen";
        private static int effectTimer = 0;
        private const int MaxEffectDuration = 180;

        public override void Load()
        {
            if (Main.dedServ) return;

            Ref<Effect> filterRef = new Ref<Effect>(ModContent.Request<Effect>("jarkos/Shaders/SunfateContrastScreenFilter", AssetRequestMode.ImmediateLoad).Value);
            Filters.Scene[FilterKey] = new Filter(new ScreenShaderData(filterRef, "Pass1"), EffectPriority.VeryHigh);
            Filters.Scene[FilterKey].Load();
        }

        public static void TriggerContrastEffect()
        {
            if (Main.dedServ) return;

            effectTimer = MaxEffectDuration;
            if (!Filters.Scene[FilterKey].IsActive())
            {
                Filters.Scene.Activate(FilterKey, Main.LocalPlayer.Center);
            }
        }

        public override void PostUpdateEverything()
        {
            if (Main.dedServ) return;

            if (effectTimer > 0)
            {
                effectTimer--;
                float progress = effectTimer / (float)MaxEffectDuration;
                float smoothIntensity = progress * progress;

                if (!Filters.Scene[FilterKey].IsActive())
                {
                    Filters.Scene.Activate(FilterKey, Main.LocalPlayer.Center);
                }
                Filters.Scene[FilterKey].GetShader().UseIntensity(smoothIntensity);
            }
            else
            {
                if (Filters.Scene[FilterKey].IsActive())
                {
                    Filters.Scene.Deactivate(FilterKey);
                }
            }
        }

        public override void OnWorldUnload()
        {
            effectTimer = 0;
            if (!Main.dedServ && Filters.Scene[FilterKey].IsActive())
            {
                Filters.Scene.Deactivate(FilterKey);
            }
        }
    }
}
#endregion
#endregion
#endregion
#endregion
#endregion
#endregion
#endregion
#endregion
#endregion
#endregion
#endregion
#endregion
#endregion
#endregion
#endregion
#endregion
#endregion
#endregion
#endregion
