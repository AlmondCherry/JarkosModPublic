using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.unique_melee;
public sealed class CosmicVoidGroundWaveProjectile : ModProjectile
{
    private const int LifeTime = 132;
    private const int SpikeCount = 12;
    private const float SpikeStartDistance = 36f;
    private const float SpikeEndDistance = 650f;
    private const float SpikeGroundSearchStartOffset = 96f;
    private const float SpikeGroundSearchDistance = 320f;
    private const float SpikeBaseYOffset = 0f;
    private const int SpikeGroundSideSearchTiles = 2;
    private const float SpikeDelayFrames = 4.5f;
    private const float SpikeAppearFrames = 20f;
    private const float SpikeFadeOutFrames = 28f;
    private const float SpikeScreenDistortionIntensity = 0.72f;
    private const float SpikeScreenDistortionYOffset = -4f;
    private const float SpikePortalYOffset = -28f;
    private const float SpikePortalTriggerAppear = 0.08f;
    private const float SpikeFireWidthScale = 1.78f;
    private const float SpikeFireHeightScale = 1.42f;
    private const float SpikeFireYOffsetScale = 0f;
    private const string RelativeSpikeTexturePath = "textures/Helfeim/CosmicVoidGroundSpike";
    private const string RelativeSpikeCosmicMaskTexturePath = "textures/Helfeim/CosmicVoidGroundSpike_CosmicMask";
    private readonly bool[] portalTriggered = new bool[SpikeCount];
    public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.StardustTowerMark;
    public override void SetDefaults()
    {
        Projectile.width = 560;
        Projectile.height = 170;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 6;
        Projectile.timeLeft = LifeTime;
    }
    public override void AI()
    {
        Projectile.localAI[0]++;
        Projectile.velocity = Vector2.Zero;
        if (Projectile.localAI[0] % 4f == 0f)
        {
            float direction = GetDirection();
            int index = Main.rand.Next(SpikeCount);
            if (TryGetSpikeBasePosition(direction, index, out Vector2 basePosition))
            {
                float appear = GetSpikeAppear(index);
                if (appear > 0f)
                {
                    float t = GetSpikeProgress(index);
                    Vector2 position = basePosition + new Vector2(Main.rand.NextFloat(-GetSpikeWidth(t, appear) * 0.35f, GetSpikeWidth(t, appear) * 0.35f), -Main.rand.NextFloat(0f, GetSpikeHeight(t, appear) * 0.75f));
                    Dust dust = Dust.NewDustPerfect(position, DustID.PurpleTorch, new Vector2(direction * Main.rand.NextFloat(0.2f, 0.8f), Main.rand.NextFloat(-3.2f, -0.8f)), 30, new Color(130, 24, 220), Main.rand.NextFloat(1.0f, 1.8f));
                    dust.noGravity = true;
                    dust.scale = Main.rand.NextFloat(0.8f, 1.35f);
                }
            }
        }
        UpdateSpikeScreenDistortion();
        Lighting.AddLight(Projectile.Center, 0.28f, 0.08f, 0.45f);
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        Main.player[Projectile.owner].GetModPlayer<CosmicVoidAxePlayer>().RegisterAxeHit(target, damageDone);
        SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.25f, Pitch = 0.25f }, target.Center);
    }
    public override bool PreDraw(ref Color lightColor)
    {
        float fadeOut = MathHelper.Clamp(Projectile.timeLeft / SpikeFadeOutFrames, 0f, 1f);
        float opacity = MathHelper.SmoothStep(0f, 1f, fadeOut);
        DrawSpikes(opacity);
        return false;
    }
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        float direction = GetDirection();
        for (int i = 0; i < SpikeCount; i++)
        {
            float appear = GetSpikeAppear(i);
            if (appear <= 0.18f)
                continue;
            float t = GetSpikeProgress(i);
            if (!TryGetSpikeBasePosition(direction, i, out Vector2 basePosition))
                continue;
            float height = GetSpikeHeight(t, appear);
            float width = GetSpikeWidth(t, appear);
            Rectangle spikeBox = new Rectangle((int)(basePosition.X - width * 0.5f), (int)(basePosition.Y - height), (int)width, (int)height);
            if (spikeBox.Intersects(targetHitbox))
                return true;
        }
        return false;
    }
    private void UpdateSpikeScreenDistortion()
    {
        if (Main.dedServ)
            return;
        float direction = GetDirection();
        for (int i = 0; i < SpikeCount; i++)
        {
            if (portalTriggered[i])
                continue;
            float appear = GetSpikeAppear(i);
            if (appear < SpikePortalTriggerAppear)
                continue;
            if (!TryGetSpikeBasePosition(direction, i, out Vector2 baseWorldPosition))
                continue;
            Vector2 portalWorldPosition = baseWorldPosition + new Vector2(0f, SpikePortalYOffset);
            if (IsOnScreen(portalWorldPosition, 120f))
                CosmicItemShaderSystem.SpawnMiniSpaceDistortion(portalWorldPosition, SpikeScreenDistortionIntensity);
            portalTriggered[i] = true;
        }
    }
    private static bool IsOnScreen(Vector2 worldPosition, float padding)
    {
        Vector2 screenPosition = worldPosition - Main.screenPosition;
        return screenPosition.X > -padding
            && screenPosition.X < Main.screenWidth + padding
            && screenPosition.Y > -padding
            && screenPosition.Y < Main.screenHeight + padding;
    }
    private void DrawSpikes(float opacity)
    {
        Texture2D texture = ModContent.Request<Texture2D>($"{Mod.Name}/{RelativeSpikeTexturePath}").Value;
        DrawSpikePass(texture, opacity, false);
        DrawSpikeFirePass(texture, opacity);
        if (Main.dedServ || !GameShaders.Misc.ContainsKey(CosmicItemShaderSystem.CosmicItemShaderKey))
            return;
        Texture2D maskTexture = ModContent.Request<Texture2D>($"{Mod.Name}/{RelativeSpikeCosmicMaskTexturePath}").Value;
        DrawSpikePass(maskTexture, opacity, true);
    }
    private void DrawSpikePass(Texture2D texture, float opacity, bool useShader)
    {
        float direction = GetDirection();
        SpriteBatch spriteBatch = Main.spriteBatch;
        if (useShader)
        {
            MiscShaderData shader = GameShaders.Misc[CosmicItemShaderSystem.CosmicItemShaderKey];
            shader.UseColor(new Color(12, 4, 34));
            shader.UseSecondaryColor(new Color(115, 32, 230));
            shader.UseOpacity(opacity * 0.65f);
            shader.UseSaturation(Main.GlobalTimeWrappedHourly);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            shader.Apply(null);
        }
        for (int i = 0; i < SpikeCount; i++)
        {
            float t = GetSpikeProgress(i);
            float appear = GetSpikeAppear(i);
            if (appear <= 0f)
                continue;
            if (!TryGetSpikeBasePosition(direction, i, out Vector2 baseWorldPosition))
                continue;
            float height = GetSpikeHeight(t, appear);
            float width = GetSpikeWidth(t, appear);
            float lean = direction * MathHelper.Lerp(0.08f, -0.04f, t);
            Vector2 basePosition = baseWorldPosition - Main.screenPosition;
            DrawTriangleSpike(texture, basePosition, width, height, lean, Color.White * opacity);
        }
        if (useShader)
        {
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
    private void DrawSpikeFirePass(Texture2D texture, float opacity)
    {
        if (Main.dedServ || !GameShaders.Misc.ContainsKey(CosmicItemShaderSystem.CosmicSpikeFireShaderKey))
            return;
        float direction = GetDirection();
        SpriteBatch spriteBatch = Main.spriteBatch;
        MiscShaderData shader = GameShaders.Misc[CosmicItemShaderSystem.CosmicSpikeFireShaderKey];
        shader.UseColor(new Color(12, 0, 28));
        shader.UseSecondaryColor(new Color(185, 42, 255));
        shader.UseOpacity(opacity * 0.95f);
        shader.UseSaturation(Main.GlobalTimeWrappedHourly);
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        shader.Apply(null);
        for (int i = 0; i < SpikeCount; i++)
        {
            float t = GetSpikeProgress(i);
            float appear = GetSpikeAppear(i);
            if (appear <= 0f)
                continue;
            if (!TryGetSpikeBasePosition(direction, i, out Vector2 baseWorldPosition))
                continue;
            float baseHeight = GetSpikeHeight(t, appear);
            float height = baseHeight * SpikeFireHeightScale;
            float width = GetSpikeWidth(t, appear) * SpikeFireWidthScale;
            float lean = direction * MathHelper.Lerp(0.08f, -0.04f, t);
            Vector2 basePosition = baseWorldPosition - Main.screenPosition - new Vector2(0f, baseHeight * SpikeFireYOffsetScale);
            DrawTriangleSpike(texture, basePosition, width, height, lean, Color.White * opacity);
        }
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
    }
    private float GetSpikeAppear(int index)
    {
        float growTicks = Projectile.localAI[0] - index * SpikeDelayFrames;
        return MathHelper.Clamp(growTicks / SpikeAppearFrames, 0f, 1f);
    }
    private float GetDirection()
    {
        return Projectile.ai[0] == 0f ? 1f : Math.Sign(Projectile.ai[0]);
    }
    private static float GetSpikeProgress(int index)
    {
        return index / (float)(SpikeCount - 1);
    }
    private static float GetSpikeSpacingProgress(int index)
    {
        return (float)Math.Pow(GetSpikeProgress(index), 1.35);
    }
    private Vector2 GetSpikeSearchStart(float direction, int index)
    {
        float x = direction * MathHelper.Lerp(SpikeStartDistance, SpikeEndDistance, GetSpikeSpacingProgress(index));
        return Projectile.Center + new Vector2(x, -SpikeGroundSearchStartOffset);
    }
    private bool TryGetSpikeBasePosition(float direction, int index, out Vector2 basePosition)
    {
        Vector2 start = GetSpikeSearchStart(direction, index);
        if (TryFindGroundBelow(start, SpikeGroundSearchDistance, out Vector2 groundPosition))
        {
            basePosition = groundPosition + new Vector2(0f, SpikeBaseYOffset);
            return true;
        }
        basePosition = Vector2.Zero;
        return false;
    }
    private static bool TryFindGroundBelow(Vector2 start, float maxDistance, out Vector2 groundPosition)
    {
        int tileX = (int)(start.X / 16f);
        int startTileY = (int)(start.Y / 16f);
        int maxTiles = (int)(maxDistance / 16f);
        for (int side = 0; side <= SpikeGroundSideSearchTiles; side++)
        {
            int signStart = side == 0 ? 0 : -1;
            int signEnd = side == 0 ? 0 : 1;
            for (int sign = signStart; sign <= signEnd; sign += 2)
            {
                int testX = tileX + side * sign;
                for (int i = 0; i <= maxTiles; i++)
                {
                    int tileY = startTileY + i;
                    if (!IsValidSpikeGround(testX, tileY))
                        continue;
                    float groundX = side == 0 ? start.X : testX * 16f + 8f;
                    groundPosition = new Vector2(groundX, tileY * 16f);
                    return true;
                }
            }
        }
        groundPosition = Vector2.Zero;
        return false;
    }
    private static bool IsValidSpikeGround(int tileX, int tileY)
    {
        if (!WorldGen.InWorld(tileX, tileY, 10))
            return false;
        Tile tile = Framing.GetTileSafely(tileX, tileY);
        if (!tile.HasTile || !Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType])
            return false;
        Tile above = Framing.GetTileSafely(tileX, tileY - 1);
        return !above.HasTile || !Main.tileSolid[above.TileType] || Main.tileSolidTop[above.TileType];
    }
    private static float GetSpikeHeight(float t, float appear)
    {
        float sizeProgress = t * t;
        return MathHelper.Lerp(34f, 150f, sizeProgress) * MathHelper.SmoothStep(0f, 1f, appear);
    }
    private static float GetSpikeWidth(float t, float appear)
    {
        float sizeProgress = t * t;
        return MathHelper.Lerp(26f, 78f, sizeProgress) * MathHelper.Lerp(0.55f, 1f, appear);
    }
    private static void DrawTriangleSpike(Texture2D texture, Vector2 basePosition, float width, float height, float rotation, Color color)
    {
        Rectangle frame = texture.Frame();
        Vector2 origin = new Vector2(frame.Width * 0.5f, frame.Height);
        Vector2 scale = new Vector2(width / frame.Width, height / frame.Height);
        Main.EntitySpriteDraw(texture, basePosition, frame, color, rotation, origin, scale, SpriteEffects.None, 0f);
    }
}
