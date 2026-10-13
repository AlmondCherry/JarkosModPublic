using System;
using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.unique_melee;
public sealed class CosmicVoidSoulCollectProjectile : ModProjectile
{
    private const int TravelFrames = 46;
    private const float DrawScale = 0.34f;
    private const string RelativeSoulFlameTexturePath = "textures/Helfeim/CosmicVoidSoulFlameMask";
    private Vector2 startPosition;
    private float curveSide = 1f;
    private bool initialized;
    public override string Texture => $"{Mod.Name}/{RelativeSoulFlameTexturePath}";
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Type] = 22;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }
    public override void SetDefaults()
    {
        Projectile.width = 10;
        Projectile.height = 10;
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = TravelFrames + 18;
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
        if (!initialized)
        {
            initialized = true;
            startPosition = Projectile.Center;
            curveSide = Main.rand.NextBool() ? 1f : -1f;
            Projectile.localAI[0] = 0f;
            Projectile.localAI[1] = Main.rand.NextFloat(MathHelper.TwoPi);
            SoundEngine.PlaySound(SoundID.Item8 with { Volume = 0.18f, Pitch = 0.58f }, Projectile.Center);
            int spawnSouls = (int)MathHelper.Clamp(Projectile.ai[0], 1, CosmicVoidAxePlayer.MaxSouls);
            Color spawnColor = CosmicVoidAxePlayer.GetSoulSecondaryColor(spawnSouls);
            for (int i = 0; i < 5; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.SpectreStaff, Main.rand.NextVector2Circular(1.7f, 1.7f), 70, spawnColor, Main.rand.NextFloat(0.55f, 0.85f));
                dust.noGravity = true;
            }
        }
        float time = Projectile.localAI[0]++;
        float progress = MathHelper.Clamp(time / TravelFrames, 0f, 1f);
        float eased = progress * progress * (3f - 2f * progress);
        Vector2 end = player.MountedCenter + new Vector2(0f, -18f);
        Vector2 straight = Vector2.Lerp(startPosition, end, eased);
        Vector2 direction = (end - startPosition).SafeNormalize(Vector2.UnitY);
        Vector2 normal = new Vector2(-direction.Y, direction.X) * curveSide;
        float arcHeight = MathHelper.Lerp(120f, 28f, progress) * (float)Math.Sin(progress * MathHelper.Pi);
        Projectile.Center = straight + normal * arcHeight;
        Projectile.rotation += 0.18f + progress * 0.25f;
        Projectile.scale = MathHelper.Lerp(0.9f, 0.55f, eased);
        int souls = (int)MathHelper.Clamp(Projectile.ai[0], 1, CosmicVoidAxePlayer.MaxSouls);
        Color color = CosmicVoidAxePlayer.GetSoulSecondaryColor(souls);
        Lighting.AddLight(Projectile.Center, color.ToVector3() * (0.32f + (1f - progress) * 0.22f));
        if (Main.rand.NextBool(2))
        {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(3f, 3f), DustID.SpectreStaff, -direction.RotatedByRandom(0.55f) * Main.rand.NextFloat(0.45f, 1.25f), 80, color, Main.rand.NextFloat(0.38f, 0.7f));
            dust.noGravity = true;
        }
        if (progress >= 1f || Vector2.Distance(Projectile.Center, end) < 12f)
            Projectile.Kill();
    }
    public override void OnKill(int timeLeft)
    {
        Player player = Main.player[Projectile.owner];
        Color color = CosmicVoidAxePlayer.GetSoulSecondaryColor((int)MathHelper.Clamp(Projectile.ai[0], 1, CosmicVoidAxePlayer.MaxSouls));
        for (int i = 0; i < 5; i++)
        {
            Dust dust = Dust.NewDustPerfect(player.MountedCenter + Main.rand.NextVector2Circular(5f, 5f), DustID.GemAmethyst, Main.rand.NextVector2Circular(1.2f, 1.2f), 90, color, Main.rand.NextFloat(0.42f, 0.75f));
            dust.noGravity = true;
        }
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = ModContent.Request<Texture2D>($"{Mod.Name}/{RelativeSoulFlameTexturePath}").Value;
        Rectangle frame = texture.Frame();
        Vector2 origin = frame.Size() * 0.5f;
        int souls = (int)MathHelper.Clamp(Projectile.ai[0], 1, CosmicVoidAxePlayer.MaxSouls);
        Color primaryColor = CosmicVoidAxePlayer.GetSoulPrimaryColor(souls);
        Color secondaryColor = CosmicVoidAxePlayer.GetSoulSecondaryColor(souls);
        for (int i = Projectile.oldPos.Length - 1; i >= 0; i--)
        {
            Vector2 oldCenter = Projectile.oldPos[i] + Projectile.Size * 0.5f;
            float progress = 1f - i / (float)Projectile.oldPos.Length;
            Color trailColor = Color.Lerp(Color.Transparent, secondaryColor, progress) * (progress * 0.62f);
            Main.EntitySpriteDraw(texture, oldCenter - Main.screenPosition, frame, trailColor, Projectile.rotation, origin, Projectile.scale * DrawScale * (0.32f + progress * 0.44f), SpriteEffects.None, 0f);
        }
        DrawShaderTrail(texture, frame, origin, primaryColor, secondaryColor);
        DrawSoulFlame(texture, frame, origin, primaryColor, secondaryColor);
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        Main.EntitySpriteDraw(texture, drawPosition, frame, Color.White * 0.9f, Projectile.rotation, origin, Projectile.scale * DrawScale * 0.72f, SpriteEffects.None, 0f);
        return false;
    }
    private void DrawShaderTrail(Texture2D texture, Rectangle frame, Vector2 origin, Color primaryColor, Color secondaryColor)
    {
        if (Main.dedServ || !GameShaders.Misc.ContainsKey(CosmicItemShaderSystem.CosmicSoulFlameShaderKey))
            return;
        SpriteBatch spriteBatch = Main.spriteBatch;
        MiscShaderData shader = GameShaders.Misc[CosmicItemShaderSystem.CosmicSoulFlameShaderKey];
        shader.UseColor(primaryColor);
        shader.UseSecondaryColor(secondaryColor);
        shader.UseOpacity(0.72f);
        shader.UseSaturation(Main.GlobalTimeWrappedHourly);
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        shader.Apply(null);
        for (int i = Projectile.oldPos.Length - 1; i >= 0; i--)
        {
            Vector2 oldCenter = Projectile.oldPos[i] + Projectile.Size * 0.5f;
            float progress = 1f - i / (float)Projectile.oldPos.Length;
            spriteBatch.Draw(texture, oldCenter - Main.screenPosition, frame, Color.White * (progress * 0.42f), Projectile.rotation, origin, Projectile.scale * DrawScale * (0.42f + progress * 0.55f), SpriteEffects.None, 0f);
        }
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
    }
    private void DrawSoulFlame(Texture2D texture, Rectangle frame, Vector2 origin, Color primaryColor, Color secondaryColor)
    {
        if (Main.dedServ || !GameShaders.Misc.ContainsKey(CosmicItemShaderSystem.CosmicSoulFlameShaderKey))
        {
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Color.Lerp(primaryColor, Color.White, 0.25f), Projectile.rotation, origin, Projectile.scale * DrawScale, SpriteEffects.None, 0f);
            return;
        }
        SpriteBatch spriteBatch = Main.spriteBatch;
        MiscShaderData shader = GameShaders.Misc[CosmicItemShaderSystem.CosmicSoulFlameShaderKey];
        shader.UseColor(primaryColor);
        shader.UseSecondaryColor(secondaryColor);
        shader.UseOpacity(1f);
        shader.UseSaturation(Main.GlobalTimeWrappedHourly + Projectile.whoAmI * 0.07f);
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        shader.Apply(null);
        Vector2 drawPosition = Projectile.Center - Main.screenPosition;
        spriteBatch.Draw(texture, drawPosition, frame, Color.White, Projectile.rotation, origin, Projectile.scale * DrawScale * 1.35f, SpriteEffects.None, 0f);
        spriteBatch.Draw(texture, drawPosition + new Vector2(0f, -2f * Projectile.scale), frame, Color.White * 0.72f, Projectile.rotation + 0.28f, origin, Projectile.scale * DrawScale * 0.86f, SpriteEffects.None, 0f);
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
    }
}
