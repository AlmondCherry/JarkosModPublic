using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.systems;
namespace jarkos.projectiles.melee.unique_melee;
public sealed class CosmicVoidAxeShieldProjectile : ModProjectile
{
    private const float DrawScale = 0.36f;
    private const float ReturnFrames = 48f;
    private const string RelativeWorldTexturePath = "textures/Helfeim/CosmicVoidAxe";
    private const string RelativeWorldCosmicMaskTexturePath = "textures/Helfeim/CosmicVoidAxe_CosmicMask";
    private const float OrbitVerticalSquash = 0.82f;
    private const float CastOrbitInitialRadius = 64f;
    private const float CastOrbitFinalRadius = 224f;
    private const float CastOrbitInitialAngularSpeed = 0.40f;
    private const float CastOrbitFinalAngularSpeed = 0.095f;
    private const float SteadyOrbitRadius = 215f;
    private const float SteadyAngularSpeed = 0.045f;
    private const int TrailLength = 9;
    public override string Texture => $"{Mod.Name}/{RelativeWorldTexturePath}";
    private float orbitAngle;
    private float spinAngle;
    private Vector2[] trailPositions = new Vector2[TrailLength];
    private float[] trailRotations = new float[TrailLength];
    private int trailFilled;
    private bool initialized;
    private Vector2 returnStart;
    private Vector2 returnControl1;
    private Vector2 returnControl2;
    private bool returnInitialized;
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
        Projectile.timeLeft = 2;
        Projectile.localAI[0]++;
        if (!initialized)
        {
            Vector2 hand = player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, -MathHelper.PiOver2);
            Projectile.Center = hand;
            orbitAngle = player.direction == -1 ? MathHelper.Pi : 0f;
            spinAngle = Projectile.rotation;
            initialized = true;
        }
        if (modPlayer.ShieldTimer <= 0)
        {
            AIReturnToHand(player);
            return;
        }
        AIOrbit(player, modPlayer);
        EmitOrbitFx(player, modPlayer);
    }
    private void AIReturnToHand(Player player)
    {
        float side = player.direction == 0 ? 1f : player.direction;
        Projectile.localAI[2]++;
        Vector2 hand = player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, -MathHelper.PiOver2);
        if (!returnInitialized)
        {
            returnStart = Projectile.Center;
            Vector2 dir = (hand - returnStart).SafeNormalize(new Vector2(side, 0f));
            Vector2 perp = new Vector2(-dir.Y, dir.X);
            float dist = Vector2.Distance(returnStart, hand);
            returnControl1 = returnStart + dir * (dist * 0.25f) + perp * 36f * side;
            returnControl2 = hand + dir * -(dist * 0.25f) + new Vector2(-side * 22f, 30f);
            returnInitialized = true;
        }
        float t = MathHelper.Clamp(Projectile.localAI[2] / ReturnFrames, 0f, 1f);
        float ease = t < 0.5f ? 8f * t * t * t * t : 1f - (float)Math.Pow(-2f * t + 2f, 4f) / 2f;
        Vector2 bezier = CubicBezier(returnStart, returnControl1, returnControl2, hand, ease);
        Projectile.Center = Vector2.Lerp(Projectile.Center, bezier, 0.55f);
        float targetRotation = MathHelper.ToRadians(-62f * side);
        float spin = MathHelper.ToRadians(38f * side) * (1f - ease);
        Projectile.rotation = MathHelper.Lerp(Projectile.rotation, targetRotation + spin, 0.18f);
        Projectile.spriteDirection = side < 0f ? -1 : 1;
        if (t >= 1f || Vector2.Distance(Projectile.Center, hand) < 8f)
            Projectile.Kill();
    }
    private static Vector2 CubicBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        float u = 1f - t;
        return u * u * u * p0
             + 3f * u * u * t * p1
             + 3f * u * t * t * p2
             + t * t * t * p3;
    }
    private void AIOrbit(Player player, CosmicVoidAxePlayer modPlayer)
    {
        bool casting = modPlayer.ShieldActivationTimer > 0;
        float castProgress = casting
            ? 1f - modPlayer.ShieldActivationTimer / (float)CosmicVoidAxePlayer.ShieldActivationFrames
            : 1f;
        float castEase = EaseOutQuint(castProgress);
        float angularSpeed = casting
            ? MathHelper.Lerp(CastOrbitInitialAngularSpeed, CastOrbitFinalAngularSpeed, castEase)
            : SteadyAngularSpeed;
        orbitAngle += angularSpeed;
        if (orbitAngle > MathHelper.TwoPi * 8f)
            orbitAngle -= MathHelper.TwoPi * 8f;
        float radius;
        if (casting)
        {
            radius = MathHelper.Lerp(CastOrbitInitialRadius, CastOrbitFinalRadius, castEase);
        }
        else
        {
            float breathSlow = (float)Math.Sin(Projectile.localAI[0] * 0.06f) * 8f;
            float breathFast = (float)Math.Sin(Projectile.localAI[0] * 0.18f + 1.7f) * 3.5f;
            radius = SteadyOrbitRadius + breathSlow + breathFast;
        }
        float orbitTilt = Projectile.localAI[0] * 0.012f;
        float cosT = (float)Math.Cos(orbitTilt);
        float sinT = (float)Math.Sin(orbitTilt);
        float rawX = (float)Math.Cos(orbitAngle) * radius;
        float rawY = (float)Math.Sin(orbitAngle) * radius * OrbitVerticalSquash;
        float tiltedX = rawX * cosT - rawY * sinT;
        float tiltedY = rawX * sinT + rawY * cosT;
        float bob = (float)Math.Sin(orbitAngle * 2f + Projectile.localAI[0] * 0.04f) * 6f;
        Vector2 orbitOffset = new Vector2(tiltedX, tiltedY + bob - 8f);
        Vector2 target = player.MountedCenter + orbitOffset;
        float follow = casting ? MathHelper.Lerp(0.55f, 0.32f, castEase) : 0.30f;
        Projectile.Center = Vector2.Lerp(Projectile.Center, target, follow);
        float baseSpin = casting
            ? MathHelper.Lerp(0.42f, 0.22f, castEase)
            : 0.16f;
        float spinPulse = 1f + (float)Math.Sin(Projectile.localAI[0] * 0.09f) * 0.18f;
        spinAngle += baseSpin * spinPulse;
        if (spinAngle > MathHelper.TwoPi * 8f)
            spinAngle -= MathHelper.TwoPi * 8f;
        Projectile.rotation = spinAngle;
        Projectile.spriteDirection = 1;
        PushTrailSample(Projectile.Center, Projectile.rotation);
    }
    private void PushTrailSample(Vector2 position, float rotation)
    {
        for (int i = TrailLength - 1; i > 0; i--)
        {
            trailPositions[i] = trailPositions[i - 1];
            trailRotations[i] = trailRotations[i - 1];
        }
        trailPositions[0] = position;
        trailRotations[0] = rotation;
        if (trailFilled < TrailLength)
            trailFilled++;
    }
    private void EmitOrbitFx(Player player, CosmicVoidAxePlayer modPlayer)
    {
        bool casting = modPlayer.ShieldActivationTimer > 0;
        float castIntensity = casting
            ? modPlayer.ShieldActivationTimer / (float)CosmicVoidAxePlayer.ShieldActivationFrames
            : 0f;
        float breath = 0.75f + (float)Math.Sin(Projectile.localAI[0] * 0.08f) * 0.25f;
        Color color = CosmicVoidAxePlayer.GetSoulSecondaryColor(modPlayer.ShieldSouls);
        Lighting.AddLight(Projectile.Center, color.ToVector3() * (0.45f + breath * 0.35f + castIntensity * 0.6f));
        bool dustChance = Main.rand.NextBool(casting ? 1 : 4);
        if (dustChance)
        {
            float spread = casting ? 28f : 22f;
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(spread, spread), DustID.GemAmethyst, Main.rand.NextVector2Circular(0.8f, 0.8f), 80, color, Main.rand.NextFloat(0.65f, 1.1f) + castIntensity * 0.4f);
            dust.noGravity = true;
        }
        if (Projectile.owner == Main.myPlayer)
        {
            int interval = casting ? 6 : 18;
            if ((int)Projectile.localAI[0] % interval == 0)
                CosmicItemShaderSystem.SpawnMiniSpaceDistortion(Projectile.Center, 0.38f + breath * 0.28f + castIntensity * 0.4f);
        }
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = ModContent.Request<Texture2D>($"{Mod.Name}/{RelativeWorldTexturePath}").Value;
        Rectangle frame = texture.Frame();
        SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        Vector2 origin = frame.Size() * 0.5f;
        Vector2 position = Projectile.Center - Main.screenPosition;
        float breath = 0.94f + (float)Math.Sin(Projectile.localAI[0] * 0.08f) * 0.06f;
        float scale = DrawScale * breath;
        Player player = Main.player[Projectile.owner];
        CosmicVoidAxePlayer modPlayer = player.GetModPlayer<CosmicVoidAxePlayer>();
        bool casting = modPlayer.ShieldActivationTimer > 0;
        DrawMotionTrail(texture, frame, origin, scale, effects, modPlayer);
        if (casting)
            DrawShieldGhostTrail(texture, frame, origin, scale, effects, modPlayer);
        float opacity = 0.82f;
        Main.EntitySpriteDraw(texture, position, frame, lightColor * opacity, Projectile.rotation, origin, scale, effects, 0f);
        DrawCosmicMask(position, frame, origin, scale, effects, opacity);
        return false;
    }
    private void DrawMotionTrail(Texture2D texture, Rectangle frame, Vector2 origin, float scale, SpriteEffects effects, CosmicVoidAxePlayer modPlayer)
    {
        if (trailFilled <= 1)
            return;
        SpriteBatch spriteBatch = Main.spriteBatch;
        bool casting = modPlayer.ShieldActivationTimer > 0;
        float intensityBoost = casting ? 1.35f : 1f;
        Color baseColor = CosmicVoidAxePlayer.GetSoulSecondaryColor(modPlayer.ShieldSouls);
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        for (int i = trailFilled - 1; i >= 1; i--)
        {
            float t = i / (float)trailFilled;
            float fade = (1f - t) * (1f - t) * (1f - t);
            float trailAlpha = fade * 0.55f * intensityBoost;
            float trailScale = scale * (1f - t * 0.30f);
            Vector2 trailPos = trailPositions[i] - Main.screenPosition;
            Color color = baseColor * trailAlpha;
            color.A = 0;
            Main.EntitySpriteDraw(texture, trailPos, frame, color, trailRotations[i], origin, trailScale, effects, 0f);
        }
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
    }
    private void DrawShieldGhostTrail(Texture2D texture, Rectangle frame, Vector2 origin, float scale, SpriteEffects effects, CosmicVoidAxePlayer modPlayer)
    {
        SpriteBatch spriteBatch = Main.spriteBatch;
        Vector2 position = Projectile.Center - Main.screenPosition;
        float castIntensity = modPlayer.ShieldActivationTimer / (float)CosmicVoidAxePlayer.ShieldActivationFrames;
        float intensity = MathHelper.Clamp(castIntensity * 0.85f, 0f, 1f);
        float pulse = 0.55f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 13.5f) * 0.45f;
        float ghostOpacity = intensity * 0.62f * pulse;
        float offset = MathHelper.Lerp(2.5f, 6.5f, intensity);
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        Color cyanGhost = new Color(110, 255, 255) * ghostOpacity * 0.42f;
        Color magentaGhost = new Color(255, 95, 220) * ghostOpacity * 0.42f;
        Vector2 cyanOffset = new Vector2(-offset, -offset * 0.5f);
        Vector2 magentaOffset = new Vector2(offset, offset * 0.5f);
        Main.EntitySpriteDraw(texture, position + cyanOffset, frame, cyanGhost, Projectile.rotation, origin, scale * 1.02f, effects, 0f);
        Main.EntitySpriteDraw(texture, position + magentaOffset, frame, magentaGhost, Projectile.rotation, origin, scale * 1.02f, effects, 0f);
        if (intensity > 0.4f)
        {
            float auraOpacity = (intensity - 0.4f) / 0.6f * 0.55f * pulse;
            Color aura = new Color(255, 245, 255) * auraOpacity * 0.55f;
            Main.EntitySpriteDraw(texture, position, frame, aura, Projectile.rotation, origin, scale * 1.18f, effects, 0f);
        }
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
    }
    private void DrawCosmicMask(Vector2 position, Rectangle frame, Vector2 origin, float scale, SpriteEffects effects, float opacity)
    {
        if (Main.dedServ || !GameShaders.Misc.ContainsKey(CosmicItemShaderSystem.CosmicItemShaderKey))
            return;
        Texture2D maskTexture = ModContent.Request<Texture2D>($"{Mod.Name}/{RelativeWorldCosmicMaskTexturePath}").Value;
        SpriteBatch spriteBatch = Main.spriteBatch;
        MiscShaderData shader = GameShaders.Misc[CosmicItemShaderSystem.CosmicItemShaderKey];
        int souls = Main.player[Projectile.owner].GetModPlayer<CosmicVoidAxePlayer>().ShieldSouls;
        shader.UseColor(CosmicVoidAxePlayer.GetSoulPrimaryColor(souls));
        shader.UseSecondaryColor(CosmicVoidAxePlayer.GetSoulSecondaryColor(souls));
        shader.UseOpacity(opacity);
        shader.UseSaturation(Main.GlobalTimeWrappedHourly);
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        shader.Apply(null);
        spriteBatch.Draw(maskTexture, position, frame, Color.White, Projectile.rotation, origin, scale, effects, 0f);
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
    }
    private static float EaseOutQuint(float x)
    {
        x = MathHelper.Clamp(x, 0f, 1f);
        float inv = 1f - x;
        return 1f - inv * inv * inv * inv * inv;
    }
}
