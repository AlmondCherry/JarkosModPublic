using System;
using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using sdd.topor.Content.Projectiles;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.unique_melee;
public sealed class CosmicVoidChannelStarProjectile : ModProjectile
{
    private const int LifetimeFrames = 28;
    public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Starfury;
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Type] = 8;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }
    public override void SetDefaults()
    {
        Projectile.width = 4;
        Projectile.height = 4;
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = LifetimeFrames + 4;
        Projectile.alpha = 0;
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
        int channelIndex = (int)Projectile.ai[0];
        if (channelIndex < 0 || channelIndex >= Main.maxProjectiles)
        {
            Projectile.Kill();
            return;
        }
        Projectile channel = Main.projectile[channelIndex];
        if (!channel.active)
        {
            Projectile.Kill();
            return;
        }
        Projectile.localAI[0]++;
        float rawT = MathHelper.Clamp(Projectile.localAI[0] / (float)LifetimeFrames, 0f, 1f);
        float t = rawT * rawT;
        Vector2 spawnPosition = new Vector2(Projectile.ai[1], Projectile.ai[2]);
        Vector2 axeHead = channel.Center + new Vector2(0f, -170f);
        Vector2 toAxe = axeHead - spawnPosition;
        Vector2 perp = new Vector2(-toAxe.Y, toAxe.X).SafeNormalize(Vector2.UnitX);
        float swingPhase = (Projectile.ai[1] * 0.0173f + Projectile.ai[2] * 0.0291f);
        float swing = (float)Math.Sin(swingPhase) * 0.6f;
        Vector2 control = spawnPosition + toAxe * 0.5f + perp * (toAxe.Length() * 0.55f * swing);
        float omt = 1f - t;
        Vector2 pos = spawnPosition * (omt * omt) + control * (2f * omt * t) + axeHead * (t * t);
        Projectile.Center = pos;
        Vector2 vel = (control - spawnPosition) * (2f * omt) + (axeHead - control) * (2f * t);
        Projectile.velocity = vel;
        Projectile.rotation = vel.ToRotation() + MathHelper.PiOver2;
        Projectile.scale = 0.18f + 0.22f * (float)Math.Sin(rawT * MathHelper.Pi);
        int souls = (int)MathHelper.Clamp(player.GetModPlayer<CosmicVoidAxePlayer>().Souls, 1, CosmicVoidAxePlayer.MaxSouls);
        Color color = CosmicVoidAxePlayer.GetSoulSecondaryColor(souls);
        Lighting.AddLight(Projectile.Center, color.ToVector3() * 0.30f);
        if (rawT >= 1f)
        {
            if (channel.ModProjectile is CosmicVoidAxeSwingProjectile axeSwing)
                axeSwing.ApplyChannelStarImpulse(vel, axeHead);
            Projectile.Kill();
        }
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = TextureAssets.Projectile[ProjectileID.Starfury].Value;
        Rectangle frame = texture.Frame();
        Vector2 origin = frame.Size() * 0.5f;
        Player player = Main.player[Projectile.owner];
        int souls = (int)MathHelper.Clamp(player.GetModPlayer<CosmicVoidAxePlayer>().Souls, 1, CosmicVoidAxePlayer.MaxSouls);
        Color baseColor = CosmicVoidAxePlayer.GetSoulSecondaryColor(souls);
        Color primary = CosmicVoidAxePlayer.GetSoulPrimaryColor(souls);
        for (int i = 0; i < Projectile.oldPos.Length; i++)
        {
            Vector2 oldPosition = Projectile.oldPos[i];
            if (oldPosition == Vector2.Zero)
                continue;
            float progress = 1f - i / (float)Projectile.oldPos.Length;
            float scale = Projectile.scale * MathHelper.Lerp(0.30f, 0.95f, progress);
            Color trailColor = Color.Lerp(baseColor, primary, 0.4f) * (progress * 0.50f);
            float trailRotation = Projectile.oldRot[i];
            Main.EntitySpriteDraw(texture, oldPosition + Projectile.Size * 0.5f - Main.screenPosition, frame, trailColor, trailRotation, origin, scale, SpriteEffects.None, 0f);
        }
        Color drawColor = Color.Lerp(baseColor, Color.White, 0.35f);
        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, drawColor * 0.55f, Projectile.rotation, origin, Projectile.scale * 1.85f, SpriteEffects.None, 0f);
        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, drawColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Color.White * 0.65f, Projectile.rotation, origin, Projectile.scale * 0.50f, SpriteEffects.None, 0f);
        return false;
    }
}
