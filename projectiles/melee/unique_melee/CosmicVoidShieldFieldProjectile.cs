using System;
using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.unique_melee;
public sealed class CosmicVoidShieldFieldProjectile : ModProjectile
{
    private const int FadeInFrames = 18;
    private const float ReflectionHitRadius = 100f;
    private const int ReflectionCooldownFrames = 6;
    private const int ReflectionWaveFrames = 22;
    private const string ShieldNoiseTexturePath = "textures/Helfeim/CosmicVoidShieldNoise";
    public override string Texture => "Terraria/Images/MagicPixel";
    private int reflectionCooldown;
    private readonly Vector2[] waveOrigins = new Vector2[4];
    private readonly int[] waveTimers = new int[4];
    private int waveSlotCursor;
    public override void SetDefaults()
    {
        Projectile.width = 2;
        Projectile.height = 2;
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 2;
    }
    public override bool ShouldUpdatePosition() => false;
    public override void AI()
    {
        Player player = Main.player[Projectile.owner];
        if (!player.active || player.dead)
        {
            Projectile.Kill();
            return;
        }
        CosmicVoidAxePlayer modPlayer = player.GetModPlayer<CosmicVoidAxePlayer>();
        if (modPlayer.ShieldTimer <= 0)
        {
            Projectile.Kill();
            return;
        }
        Projectile.timeLeft = 2;
        Projectile.localAI[0]++;
        Projectile.Center = Vector2.Lerp(Projectile.Center == Vector2.Zero ? player.Center : Projectile.Center, player.Center, 0.35f);
        if (reflectionCooldown > 0)
            reflectionCooldown--;
        for (int i = 0; i < waveTimers.Length; i++)
        {
            if (waveTimers[i] > 0)
                waveTimers[i]--;
        }
        if (Projectile.owner == Main.myPlayer)
            TryReflectProjectiles(player, modPlayer);
    }
    private float GetCurrentReflectionRadius(CosmicVoidAxePlayer modPlayer)
    {
        float activationProgress = 1f;
        if (modPlayer.ShieldActivationTimer > 0)
            activationProgress = 1f - modPlayer.ShieldActivationTimer / (float)CosmicVoidAxePlayer.ShieldActivationFrames;
        float ease = 1f - (float)Math.Pow(1f - MathHelper.Clamp(activationProgress, 0f, 1f), 3.0);
        return ReflectionHitRadius * MathHelper.Lerp(0.05f, 1f, ease);
    }
    private void TryReflectProjectiles(Player player, CosmicVoidAxePlayer modPlayer)
    {
        if (reflectionCooldown > 0)
            return;
        if (modPlayer.ShieldActivationTimer >= CosmicVoidAxePlayer.ShieldActivationFrames - 4)
            return;
        float effectiveRadius = GetCurrentReflectionRadius(modPlayer);
        float radiusSq = effectiveRadius * effectiveRadius;
        for (int i = 0; i < Main.maxProjectiles; i++)
        {
            Projectile other = Main.projectile[i];
            if (!other.active || !other.hostile || other.friendly)
                continue;
            if (other.damage <= 0)
                continue;
            if (other.localAI[1] >= 1f && other.owner == Projectile.owner)
                continue;
            if (Vector2.DistanceSquared(other.Center, player.Center) > radiusSq)
                continue;
            ReflectProjectile(other, player);
            modPlayer.ConsumeShieldTime(CosmicVoidAxePlayer.ShieldReflectionTimeCost);
            SpawnReflectionWave(other.Center, modPlayer);
            reflectionCooldown = ReflectionCooldownFrames;
            return;
        }
    }
    private void ReflectProjectile(Projectile hostile, Player player)
    {
        Vector2 fromShield = (hostile.Center - player.Center).SafeNormalize(Vector2.UnitX);
        float speed = Math.Max(hostile.velocity.Length(), 6f);
        hostile.velocity = fromShield * speed;
        hostile.hostile = false;
        hostile.friendly = true;
        hostile.owner = Projectile.owner;
        hostile.localAI[1] = 1f;
        SoundEngine.PlaySound(SoundID.NPCHit4 with { Pitch = 0.4f, Volume = 0.55f }, hostile.Center);
        CosmicVoidAxePlayer modPlayer = player.GetModPlayer<CosmicVoidAxePlayer>();
        modPlayer.OnShieldHit();
        Color color = CosmicVoidAxePlayer.GetSoulSecondaryColor(modPlayer.ShieldSouls);
        for (int i = 0; i < 12; i++)
        {
            Vector2 vel = Main.rand.NextVector2CircularEdge(3.6f, 3.6f) * Main.rand.NextFloat(0.6f, 1.2f);
            Dust dust = Dust.NewDustPerfect(hostile.Center, DustID.SpectreStaff, vel, 60, color, Main.rand.NextFloat(1.0f, 1.6f));
            dust.noGravity = true;
        }
    }
    private void SpawnReflectionWave(Vector2 origin, CosmicVoidAxePlayer modPlayer)
    {
        int slot = waveSlotCursor % waveOrigins.Length;
        waveOrigins[slot] = origin;
        waveTimers[slot] = ReflectionWaveFrames;
        waveSlotCursor++;
    }
    public override bool PreDraw(ref Color lightColor)
    {
        if (Main.dedServ || !GameShaders.Misc.ContainsKey(CosmicItemShaderSystem.CosmicShieldShaderKey))
            return false;
        Player player = Main.player[Projectile.owner];
        CosmicVoidAxePlayer modPlayer = player.GetModPlayer<CosmicVoidAxePlayer>();
        Texture2D noiseTexture = ModContent.Request<Texture2D>($"{Mod.Name}/{ShieldNoiseTexturePath}").Value;
        Vector2 position = Projectile.Center - Main.screenPosition + new Vector2(0f, -10f);
        Rectangle frame = noiseTexture.Frame();
        Vector2 origin = frame.Size() * 0.5f;
        float power = MathHelper.Clamp(modPlayer.ShieldSouls / (float)CosmicVoidAxePlayer.MaxSouls, 0f, 1f);
        float fadeIn = MathHelper.Clamp(Projectile.localAI[0] / FadeInFrames, 0f, 1f);
        float baseRadius = MathHelper.Lerp(92f, 116f, power);
        float castProgress = 1f;
        if (modPlayer.ShieldActivationTimer > 0)
            castProgress = 1f - modPlayer.ShieldActivationTimer / (float)CosmicVoidAxePlayer.ShieldActivationFrames;
        float castEase = 1f - (float)Math.Pow(1f - MathHelper.Clamp(castProgress, 0f, 1f), 3.0);
        float castOvershoot = (float)Math.Sin(castProgress * MathHelper.Pi) * 8f;
        float reflectImpulse = 0f;
        for (int i = 0; i < waveTimers.Length; i++)
        {
            if (waveTimers[i] > 0)
            {
                float t = waveTimers[i] / (float)ReflectionWaveFrames;
                reflectImpulse += (float)Math.Sin(t * MathHelper.Pi) * 6f;
            }
        }
        float radius = baseRadius * MathHelper.Lerp(0.18f, 1f, castEase) + castOvershoot + reflectImpulse;
        Vector2 scale = new Vector2(radius * 2.08f / frame.Width, radius * 2.08f / frame.Height);
        float opacity = fadeIn * 0.82f * MathHelper.Lerp(0.35f, 1f, castEase);
        MiscShaderData shader = GameShaders.Misc[CosmicItemShaderSystem.CosmicShieldShaderKey];
        Color baseColor = CosmicVoidAxePlayer.GetSoulPrimaryColor(modPlayer.ShieldSouls);
        Color baseSecondary = CosmicVoidAxePlayer.GetSoulSecondaryColor(modPlayer.ShieldSouls);
        float flash = modPlayer.ShieldHitFlashIntensity;
        Color hurtPrimary = new Color(255, 30, 35);
        Color hurtSecondary = new Color(255, 90, 90);
        Color tintedPrimary = Color.Lerp(baseColor, hurtPrimary, flash * 0.85f);
        Color tintedSecondary = Color.Lerp(baseSecondary, hurtSecondary, flash);
        shader.UseColor(tintedPrimary);
        shader.UseSecondaryColor(tintedSecondary);
        shader.UseOpacity(MathHelper.Clamp(opacity + flash * 0.18f, 0f, 1f));
        shader.UseSaturation(Main.GlobalTimeWrappedHourly);
        SpriteBatch spriteBatch = Main.spriteBatch;
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        shader.Apply(null);
        spriteBatch.Draw(noiseTexture, position, frame, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        DrawReflectionWaves(spriteBatch, noiseTexture, frame, origin, modPlayer);
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        return false;
    }
    private void DrawReflectionWaves(SpriteBatch spriteBatch, Texture2D noiseTexture, Rectangle frame, Vector2 origin, CosmicVoidAxePlayer modPlayer)
    {
        Color tint = CosmicVoidAxePlayer.GetSoulSecondaryColor(modPlayer.ShieldSouls);
        Player player = Main.player[Projectile.owner];
        for (int i = 0; i < waveTimers.Length; i++)
        {
            if (waveTimers[i] <= 0)
                continue;
            float lifeProgress = 1f - waveTimers[i] / (float)ReflectionWaveFrames;
            float waveRadius = MathHelper.Lerp(38f, 150f, lifeProgress);
            float waveOpacity = (1f - lifeProgress) * 0.65f;
            Vector2 wavePosition = waveOrigins[i] - Main.screenPosition + new Vector2(0f, -10f);
            Vector2 waveScale = new Vector2(waveRadius * 2.08f / frame.Width, waveRadius * 2.08f / frame.Height);
            spriteBatch.Draw(noiseTexture, wavePosition, frame, tint * waveOpacity, 0f, origin, waveScale, SpriteEffects.None, 0f);
        }
    }
}
