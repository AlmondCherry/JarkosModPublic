using System;
using jarkos.systems;
using Jarkos;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles;
public sealed class PortalProjectile : ModProjectile
{
    public const int OpenDuration = 22;
    public const int CloseDuration = 28;
    public const int MaxStableDuration = 600;
    public const float MaxRadius = 32f;
    public enum Phase : int { Open = 0, Stable = 1, Close = 2, }
    private static readonly Color PrimaryColor = new(80, 30, 200);
    private static readonly Color SecondaryColor = new(120, 220, 255);
    public override string Texture => "Jarkos/textures/PortalHook/PortalDisk";
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Type] = 1;
        ProjectileID.Sets.TrailingMode[Type] = 0;
    }
    public override void SetDefaults()
    {
        Projectile.width = (int)(MaxRadius * 2f);
        Projectile.height = (int)(MaxRadius * 2f);
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.netImportant = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = MaxStableDuration + OpenDuration + CloseDuration;
        Projectile.aiStyle = -1;
        Projectile.alpha = 0;
    }
    public Phase CurrentPhase
    {
        get => (Phase)(int)Projectile.ai[0];
        set => Projectile.ai[0] = (int)value;
    }
    public float PhaseTimer
    {
        get => Projectile.ai[1];
        set => Projectile.ai[1] = value;
    }
    private float TotalTime
    {
        get => Projectile.localAI[0];
        set => Projectile.localAI[0] = value;
    }
    public void BeginCollapse()
    {
        if (CurrentPhase == Phase.Close)
            return;

        CurrentPhase = Phase.Close;
        PhaseTimer = 0f;
        Projectile.netUpdate = true;
    }
    public override bool ShouldUpdatePosition() => false;
    public override void AI()
    {
        TotalTime += 1f;
        PhaseTimer += 1f;

        switch (CurrentPhase)
        {
            case Phase.Open:
                if (PhaseTimer >= OpenDuration)
                {
                    CurrentPhase = Phase.Stable;
                    PhaseTimer = 0f;
                }
                break;
            case Phase.Stable:
                if (PhaseTimer >= MaxStableDuration)
                    BeginCollapse();
                break;
            case Phase.Close:
                if (PhaseTimer >= CloseDuration)
                {
                    Projectile.Kill();
                    return;
                }
                break;
        }
        UpdateScreenDistortion();
        EmitAmbientLight();
    }
    public override void OnKill(int timeLeft)
    {
        if (CountActivePortals() <= 1)
            PortalScreenSystem.Deactivate();
    }
    private static int CountActivePortals()
    {
        int type = ModContent.ProjectileType<PortalProjectile>();
        int n = 0;

        for (int i = 0; i < Main.maxProjectiles; i++)
        {
            Projectile p = Main.projectile[i];
            if (p.active && p.type == type)
                n++;
        }
        return n;
    }
    private void UpdateScreenDistortion()
    {
        if (!Jarkos_config.SomeShadersEnabled)
        {
            PortalScreenSystem.Deactivate();
            return;
        }
        if (!PortalScreenSystem.IsOnScreen(Projectile.Center, padding: MaxRadius * 6f))
        {
            PortalScreenSystem.Deactivate();
            return;
        }
        PortalScreenSystem.Activate(Projectile.Center);

        float openT = MathHelper.Clamp(PhaseTimer / OpenDuration, 0f, 1f);
        float closeT = MathHelper.Clamp(PhaseTimer / CloseDuration, 0f, 1f);

        float intensity;
        float opacity;
        float progress;

        switch (CurrentPhase)
        {
            default:
            case Phase.Open:
                intensity = SmoothStep(openT) * 0.55f;
                opacity = SmoothStep(openT);
                progress = 0.15f + openT * 0.2f;
                break;
            case Phase.Stable:
                intensity = 0.55f + (float)Math.Sin(TotalTime * 0.05f) * 0.05f;
                opacity = 1f;
                progress = 0.4f;
                break;
            case Phase.Close:
                intensity = 0.6f + closeT * 0.9f;
                opacity = 1f - closeT * 0.5f;
                progress = 0.6f + closeT * 0.4f;
                break;
        }
        
        PortalScreenSystem.Update(Projectile.Center, progress: progress, intensity: intensity, opacity: opacity, primary: PrimaryColor, secondary: SecondaryColor);
    }
    private void EmitAmbientLight()
    {
        float pulse = 0.7f + (float)Math.Sin(TotalTime * 0.18f) * 0.15f;
        float scale = GetVisualScale();

        Vector3 baseColor = SecondaryColor.ToVector3();
        Lighting.AddLight(Projectile.Center, baseColor.X * pulse * scale * 0.9f, baseColor.Y * pulse * scale * 0.9f, baseColor.Z * pulse * scale);
    }
    public float GetVisualScale()
    {
        switch (CurrentPhase)
        {
            default:
            case Phase.Open:
                return SmoothStep(MathHelper.Clamp(PhaseTimer / OpenDuration, 0f, 1f));
            case Phase.Stable:
                return 1f;
            case Phase.Close:
            {
                float t = MathHelper.Clamp(PhaseTimer / CloseDuration, 0f, 1f);
                float collapse = 1f - t * t;
                return MathHelper.Clamp(collapse, 0f, 1f);
            }
        }
    }
    private static float SmoothStep(float x)
    {
        x = MathHelper.Clamp(x, 0f, 1f);
        return x * x * (3f - 2f * x);
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Asset<Texture2D> textureAsset = ModContent.Request<Texture2D>("Jarkos/textures/PortalHook/PortalDisk", AssetRequestMode.ImmediateLoad);
        Texture2D texture = textureAsset.Value;
        Vector2 origin = texture.Size() * 0.5f;
        Vector2 drawPos = Projectile.Center - Main.screenPosition;
        float scale = GetVisualScale();
        if (scale <= 0.001f)
            return false;

        SpriteBatch sb = Main.spriteBatch;

        if (!Jarkos_config.SomeShadersEnabled)
        {
            Color color = PrimaryColor * GetOpacity();
            sb.Draw(texture, drawPos, null, color, Projectile.rotation, origin, scale, SpriteEffects.None, 0f);
            return false;
        }

        sb.End();
        sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
        
        ApplyVortexShader();
        sb.Draw(texture, drawPos, null, Color.White, Projectile.rotation, origin, scale, SpriteEffects.None, 0f);

        sb.End();
        sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

        ApplyGlowShader();
        float glowScale = scale * 1.08f;
        sb.Draw(texture, drawPos, null, Color.White, 0f, origin, glowScale, SpriteEffects.None, 0f);

        sb.End();
        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
        return false;
    }
    private void ApplyVortexShader()
    {
        if (!GameShaders.Misc.TryGetValue(Jarkos_mod.VortexShaderKey, out MiscShaderData shader) || shader == null)
        {
            return;
        }
        shader.UseColor(PrimaryColor)
            .UseSecondaryColor(SecondaryColor)
            .UseOpacity(GetOpacity())
            .UseSaturation(GetVisualScale());
            
        Effect fx = shader.Shader;
        fx.Parameters["uIntensity"]?.SetValue(0.85f);
        fx.Parameters["uProgress"]?.SetValue(GetPhaseProgress());
        shader.Apply();
    }
    private void ApplyGlowShader()
    {
        if (!GameShaders.Misc.TryGetValue(Jarkos_mod.GlowShaderKey, out MiscShaderData shader) || shader == null)
        {
            return;
        }
        shader.UseColor(SecondaryColor)
            .UseSecondaryColor(PrimaryColor)
            .UseOpacity(GetOpacity())
            .UseSaturation(GetVisualScale());

        Effect fx = shader.Shader;
        fx.Parameters["uIntensity"]?.SetValue(GetVisualScale());
        fx.Parameters["uProgress"]?.SetValue(GetPhaseProgress());
        shader.Apply();
    }
    private float GetOpacity()
    {
        switch (CurrentPhase)
        {
            default:
            case Phase.Open:
                return SmoothStep(MathHelper.Clamp(PhaseTimer / OpenDuration, 0f, 1f));
            case Phase.Stable:
                return 1f;
            case Phase.Close:
            {
                float t = MathHelper.Clamp(PhaseTimer / CloseDuration, 0f, 1f);
                return MathHelper.Clamp(1f - t * 0.6f, 0f, 1f);
            }
        }
    }
    private float GetPhaseProgress()
    {
        switch (CurrentPhase)
        {
            default:
            case Phase.Open:
                return MathHelper.Clamp(PhaseTimer / OpenDuration, 0f, 1f) * 0.5f;
            case Phase.Stable:
                return 0.5f;
            case Phase.Close:
                return 0.5f + MathHelper.Clamp(PhaseTimer / CloseDuration, 0f, 1f) * 0.5f;
        }
    }
}
