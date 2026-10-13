using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.systems;
using System;

namespace jarkos.projectiles.melee.unique_melee;
public sealed class CosmicVoidStarStormAxeProjectile : ModProjectile
{
    private const int FlyOutFrames = 30;
    private const int WindUpFrames = 7;
    private const int StrikeFrames = 6;
    private const int FreezeFrames = 6;
    private const int RecoveryFrames = 8;
    private const int SlashFrames = WindUpFrames + StrikeFrames + FreezeFrames + RecoveryFrames;
    private const int CastFrames = 116;
    private const int ReturnFrames = 32;
    private const float DrawScale = 0.42f;
    private const float MegaSlashScale = 1.75f;
    private const float RiseHeight = 145f;
    private const string RelativeWorldTexturePath = "textures/Helfeim/CosmicVoidAxe";
    private const string RelativeWorldCosmicMaskTexturePath = "textures/Helfeim/CosmicVoidAxe_CosmicMask";
    private bool initialized;
    private Vector2 launchPosition;
    private int castDirection = 1;
    private bool impactTriggered;
    public override string Texture => $"{Mod.Name}/{RelativeWorldTexturePath}";
    public override void SetDefaults()
    {
        Projectile.width = 2;
        Projectile.height = 2;
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = FlyOutFrames + SlashFrames + CastFrames + ReturnFrames + 20;
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

        Projectile.localAI[0]++;

        float timer = Projectile.localAI[0];
        int souls = (int)MathHelper.Clamp(Projectile.ai[0], 1, CosmicVoidAxePlayer.MaxSouls);
        float power = CosmicVoidAxePlayer.MaxSouls > 1 ? (souls - 1) / (float)(CosmicVoidAxePlayer.MaxSouls - 1) : 1f;

        Vector2 targetPosition = new Vector2(Projectile.ai[1], Projectile.ai[2]);
        if (targetPosition == Vector2.Zero)
            targetPosition = Projectile.Center + new Vector2(0f, -140f);

        Vector2 apexPosition = targetPosition + new Vector2(0f, -RiseHeight - power * 40f);
        Color color = CosmicVoidAxePlayer.GetSoulSecondaryColor(souls);

        if (!initialized)
        {
            initialized = true;
            launchPosition = player.Center;
            castDirection = player.direction != 0 ? player.direction : 1;

            Projectile.Center = launchPosition;

            SoundEngine.PlaySound(SoundID.Item117 with { Volume = 0.65f, Pitch = -0.25f }, player.Center);
            SoundEngine.PlaySound(SoundID.Item92 with { Volume = 0.55f, Pitch = -0.30f }, player.Center);
        }

        if (timer <= FlyOutFrames)
        {
            float t = MathSystem.EaseOutCubic(timer / FlyOutFrames);

            Vector2 along = Vector2.Lerp(launchPosition, apexPosition, t);
            Vector2 axis = apexPosition - launchPosition;
            Vector2 perp = (axis.Length() > 1f) ? new Vector2(-axis.Y, axis.X).SafeNormalize(Vector2.UnitX) : Vector2.UnitX;

            float arc = (float)Math.Sin(t * MathHelper.Pi) * (75f + power * 35f);
            Projectile.Center = along + perp * arc * castDirection;

            float spinAmount = (1f - t) * MathHelper.TwoPi * 1.6f * castDirection;
            float endRot = (-MathHelper.PiOver2 - 0.55f) * castDirection;
            Projectile.rotation = endRot + spinAmount;

            if (Main.rand.NextBool(2))
                SpawnFlightTrailDust(color, Projectile.Center, power);

            Lighting.AddLight(Projectile.Center, color.ToVector3() * (0.7f + 0.4f * t));
        }
        else if (timer <= FlyOutFrames + SlashFrames)
        {
            float local = timer - FlyOutFrames;

            float startRot = (-MathHelper.PiOver2 - 0.55f) * castDirection;
            float chargeRot = (-MathHelper.PiOver2 - 1.05f) * castDirection;
            float impactRot = (MathHelper.PiOver2 + 0.65f) * castDirection;
            float settleRot = (MathHelper.PiOver2 + 0.40f) * castDirection;

            Vector2 windUpOffset = new Vector2(-12f * castDirection, -55f);
            Vector2 impactOffset = new Vector2(20f * castDirection, 110f + power * 30f);
            Vector2 holdOffset = new Vector2(0f, -10f);

            if (local <= WindUpFrames)
            {
                float t = MathSystem.EaseInOutCubic(local / WindUpFrames);
                Projectile.rotation = MathHelper.Lerp(startRot, chargeRot, t);
                Projectile.Center = apexPosition + Vector2.Lerp(Vector2.Zero, windUpOffset, t);

                if (Main.rand.NextBool(2))
                    SpawnChargeDust(color, GetAxeHeadPosition(), power, t);

                Lighting.AddLight(Projectile.Center, color.ToVector3() * (0.85f + 0.4f * t));

                if (timer == FlyOutFrames + 1f)
                    SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.45f, Pitch = 0.30f }, Projectile.Center);
            }
            else if (local <= WindUpFrames + StrikeFrames)
            {
                float strikeT = (local - WindUpFrames) / StrikeFrames;
                float easedStrike = MathSystem.EaseInExpo(strikeT);

                Projectile.rotation = MathHelper.Lerp(chargeRot, impactRot, easedStrike);
                Projectile.Center = apexPosition + Vector2.Lerp(windUpOffset, impactOffset, easedStrike);

                if (timer == FlyOutFrames + WindUpFrames + 1f)
                {
                    SoundEngine.PlaySound(SoundID.Item71 with { Volume = 1.0f, Pitch = -0.50f }, Projectile.Center);
                    SoundEngine.PlaySound(SoundID.Item105 with { Volume = 0.65f, Pitch = -0.40f }, Projectile.Center);
                }

                for (int i = 0; i < 3; i++)
                    SpawnSlashDust(color, Projectile.Center, castDirection, power);

                Lighting.AddLight(Projectile.Center, color.ToVector3() * 1.55f);
            }
            else if (local <= WindUpFrames + StrikeFrames + FreezeFrames)
            {
                if (!impactTriggered)
                    TriggerImpact(player, power);

                float freezeT = (local - WindUpFrames - StrikeFrames) / FreezeFrames;
                Vector2 vibrate = Main.rand.NextVector2Circular(2.4f, 1.8f) * (1f - freezeT);
                Projectile.Center = apexPosition + impactOffset + vibrate;
                Projectile.rotation = impactRot + Main.rand.NextFloat(-0.022f, 0.022f) * (1f - freezeT);

                if (Main.rand.NextBool(2))
                    SpawnImpactDust(color, GetAxeHeadPosition(), power);

                Lighting.AddLight(Projectile.Center, color.ToVector3() * (1.65f - freezeT * 0.4f));
            }
            else
            {
                float recT = MathSystem.EaseOutCubic((local - WindUpFrames - StrikeFrames - FreezeFrames) / RecoveryFrames);

                Projectile.rotation = MathHelper.Lerp(impactRot, settleRot, recT);
                Projectile.Center = apexPosition + Vector2.Lerp(impactOffset, holdOffset, recT);

                Lighting.AddLight(Projectile.Center, color.ToVector3() * (1.05f - recT * 0.10f));
            }
        }
        else if (timer <= FlyOutFrames + SlashFrames + CastFrames)
        {
            Projectile.Center = apexPosition + new Vector2(0f, -10f);
            Projectile.rotation = (MathHelper.PiOver2 + 0.40f) * castDirection;

            if (Main.rand.NextBool(2))
                SpawnSoulDust(color, targetPosition, Projectile.Center, power);

            Lighting.AddLight(Projectile.Center, color.ToVector3() * 0.95f);
        }
        else
        {
            float returnProgress = MathHelper.Clamp((timer - FlyOutFrames - SlashFrames - CastFrames) / ReturnFrames, 0f, 1f);
            float t = MathSystem.EaseInCubic(returnProgress);

            Vector2 holdPosition = apexPosition + new Vector2(0f, -10f);
            Vector2 returnTarget = player.Center;
            Vector2 along = Vector2.Lerp(holdPosition, returnTarget, t);
            Vector2 axis = returnTarget - holdPosition;
            Vector2 perp = (axis.Length() > 1f) ? new Vector2(-axis.Y, axis.X).SafeNormalize(Vector2.UnitX) : Vector2.UnitX;

            float arc = (float)Math.Sin(t * MathHelper.Pi) * 80f;
            Projectile.Center = along + perp * arc * (-castDirection);
            Vector2 flightDir = (returnTarget - Projectile.Center).SafeNormalize(new Vector2(castDirection, -1f));
            float targetRot = ((float)Math.Atan2(flightDir.Y, flightDir.X) + MathHelper.PiOver2) * castDirection;
            float startRot = (MathHelper.PiOver2 + 0.40f) * castDirection;

            Projectile.rotation = MathHelper.Lerp(startRot, targetRot, MathSystem.EaseInOutCubic(returnProgress));

            if (Main.rand.NextBool(3))
                SpawnFlightTrailDust(color, Projectile.Center, power * 0.6f);

            if (returnProgress >= 0.96f || Vector2.Distance(Projectile.Center, returnTarget) < 18f)
                Projectile.Kill();
        }
        Projectile.spriteDirection = castDirection;
        player.GetModPlayer<CosmicVoidAxePlayer>().LockForStarStorm(6);
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = ModContent.Request<Texture2D>($"{Mod.Name}/{RelativeWorldTexturePath}").Value;
        Rectangle frame = texture.Frame();
        SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        Vector2 origin = frame.Size() * 0.5f;
        Vector2 position = Projectile.Center - Main.screenPosition;

        int souls = (int)MathHelper.Clamp(Projectile.ai[0], 1, CosmicVoidAxePlayer.MaxSouls);
        float power = CosmicVoidAxePlayer.MaxSouls > 1 ? (souls - 1) / (float)(CosmicVoidAxePlayer.MaxSouls - 1) : 1f;
        float timer = Projectile.localAI[0];
        float slashProgress = MathHelper.Clamp((timer - FlyOutFrames) / SlashFrames, 0f, 1f);
        bool megaSlash = timer > FlyOutFrames && timer <= FlyOutFrames + SlashFrames;
        float scale = megaSlash ? DrawScale * MathHelper.Lerp(1.25f, MegaSlashScale + power * 0.45f, (float)Math.Sin(slashProgress * MathHelper.Pi))
            : DrawScale;

        if (megaSlash)
            DrawMegaSlashTrail(texture, frame, origin, effects, power, slashProgress);

        Main.EntitySpriteDraw(texture, position, frame, lightColor, Projectile.rotation, origin, scale, effects, 0f);
        DrawCosmicMask(position, frame, origin, scale, effects);
        return false;
    }
    private void DrawCosmicMask(Vector2 position, Rectangle frame, Vector2 origin, float scale, SpriteEffects effects)
    {
        if (Main.dedServ || !GameShaders.Misc.ContainsKey(CosmicItemShaderSystem.CosmicItemShaderKey))
            return;
        Texture2D maskTexture = ModContent.Request<Texture2D>($"{Mod.Name}/{RelativeWorldCosmicMaskTexturePath}").Value;
        SpriteBatch spriteBatch = Main.spriteBatch;
        MiscShaderData shader = GameShaders.Misc[CosmicItemShaderSystem.CosmicItemShaderKey];

        int souls = (int)MathHelper.Clamp(Projectile.ai[0], 1, CosmicVoidAxePlayer.MaxSouls);
        shader.UseColor(CosmicVoidAxePlayer.GetSoulPrimaryColor(souls));
        shader.UseSecondaryColor(CosmicVoidAxePlayer.GetSoulSecondaryColor(souls));
        shader.UseOpacity(1f);
        shader.UseSaturation(Main.GlobalTimeWrappedHourly);

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

        shader.Apply(null);
        spriteBatch.Draw(maskTexture, position, frame, Color.White, Projectile.rotation, origin, scale, effects, 0f);

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
    }
    private static void SpawnSoulDust(Color color, Vector2 from, Vector2 to, float power)
    {
        Vector2 start = from + Main.rand.NextVector2Circular(42f, 36f);
        Vector2 velocity = (to - start).SafeNormalize(Vector2.UnitY).RotatedByRandom(0.35f) * Main.rand.NextFloat(2.2f, 4.4f + power * 2f);

        Dust dust = Dust.NewDustPerfect(start, DustID.SpectreStaff, velocity, 80, color, Main.rand.NextFloat(1.0f, 1.7f));
        dust.noGravity = true;
    }
    private static void SpawnSlashDust(Color color, Vector2 center, int direction, float power)
    {
        Vector2 slashDirection = Vector2.UnitY;
        Vector2 normal = new Vector2(-slashDirection.Y, slashDirection.X);

        for (int i = 0; i < 4; i++)
        {
            Vector2 position = center - slashDirection * Main.rand.NextFloat(20f, 90f) + normal * Main.rand.NextFloat(-26f, 26f);
            Vector2 velocity = slashDirection.RotatedByRandom(0.22f) * Main.rand.NextFloat(3.2f, 7.5f + power * 2f);
            Dust dust = Dust.NewDustPerfect(position, DustID.PurpleTorch, velocity, 60, color, Main.rand.NextFloat(1.0f, 1.8f));
            dust.noGravity = true;
        }
    }
    private void DrawMegaSlashTrail(Texture2D texture, Rectangle frame, Vector2 origin, SpriteEffects effects, float power, float slashProgress)
    {
        SpriteBatch spriteBatch = Main.spriteBatch;
        int souls = (int)MathHelper.Clamp(Projectile.ai[0], 1, CosmicVoidAxePlayer.MaxSouls);
        Color primary = CosmicVoidAxePlayer.GetSoulPrimaryColor(souls);
        Color secondary = CosmicVoidAxePlayer.GetSoulSecondaryColor(souls);
        Vector2 position = Projectile.Center - Main.screenPosition;

        float opacity = (float)Math.Sin(slashProgress * MathHelper.Pi);
        float midPulse = 1f - Math.Abs(slashProgress - 0.5f) * 2f;
        
        midPulse = MathHelper.Clamp(midPulse, 0f, 1f);
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

        for (int i = 0; i < 8; i++)
        {
            float progress = i / 7f;
            Vector2 offset = new Vector2(0f, -progress * (52f + power * 54f));
            float scale = DrawScale * (MegaSlashScale + power * 0.38f) * MathHelper.Lerp(1f, 0.52f, progress);
            Color color = Color.Lerp(secondary, primary, progress) * opacity * (1f - progress) * 0.46f;
            Main.EntitySpriteDraw(texture, position + offset, frame, color, Projectile.rotation + (progress - 0.5f) * 0.08f, origin, scale, effects, 0f);
        }

        float chromaOffset = 4.5f + power * 4.5f;
        float chromaOpacity = opacity * 0.55f;
        float trailScale = DrawScale * (MegaSlashScale + power * 0.38f);

        Color cyanGhost = new Color(120, 240, 255) * chromaOpacity * 0.55f;
        Color magentaGhost = new Color(255, 110, 220) * chromaOpacity * 0.55f;
        
        Main.EntitySpriteDraw(texture, position + new Vector2(-chromaOffset, 0f), frame, cyanGhost, Projectile.rotation, origin, trailScale * 1.04f, effects, 0f);
        Main.EntitySpriteDraw(texture, position + new Vector2(chromaOffset, 0f), frame, magentaGhost, Projectile.rotation, origin, trailScale * 1.04f, effects, 0f);

        if (midPulse > 0.05f)
        {
            float burstScale = trailScale * MathHelper.Lerp(1.4f, 2.6f + power * 0.7f, midPulse);
            float burstOpacity = midPulse * (0.35f + power * 0.25f);
            Color burstColor = Color.Lerp(secondary, Color.White, 0.4f) * burstOpacity;
            Main.EntitySpriteDraw(texture, position, frame, burstColor, Projectile.rotation + MathHelper.PiOver4, origin, burstScale, effects, 0f);
            Main.EntitySpriteDraw(texture, position, frame, burstColor, Projectile.rotation - MathHelper.PiOver4, origin, burstScale * 0.85f, effects, 0f);
            Main.EntitySpriteDraw(texture, position, frame, Color.White * burstOpacity * 0.55f, Projectile.rotation, origin, burstScale * 0.6f, effects, 0f);
        }

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
    }
    private static void SpawnFlightTrailDust(Color color, Vector2 center, float power)
    {
        Vector2 velocity = Main.rand.NextVector2Circular(1.6f, 1.6f);
        Dust dust = Dust.NewDustPerfect(center + Main.rand.NextVector2Circular(8f, 8f), DustID.SpectreStaff, velocity, 80, color, Main.rand.NextFloat(0.85f, 1.35f) + power * 0.25f);
        dust.noGravity = true;
        dust.fadeIn = 0.6f;
    }
    private static void SpawnChargeDust(Color color, Vector2 headPosition, float power, float t)
    {
        float radius = MathHelper.Lerp(46f, 12f, t);
        float angle = Main.rand.NextFloat(MathHelper.TwoPi);

        Vector2 spawn = headPosition + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
        Vector2 velocity = (headPosition - spawn).SafeNormalize(Vector2.UnitY) * (1.4f + t * 2.4f + power * 0.6f);

        Dust dust = Dust.NewDustPerfect(spawn, DustID.SpectreStaff, velocity, 90, color, 0.85f + t * 0.6f);
        dust.noGravity = true;
        dust.fadeIn = 0.8f;
    }
    private static void SpawnImpactDust(Color color, Vector2 headPosition, float power)
    {
        Vector2 velocity = Main.rand.NextVector2CircularEdge(2.6f, 2.6f) * Main.rand.NextFloat(0.6f, 1.4f);
        Dust dust = Dust.NewDustPerfect(headPosition + Main.rand.NextVector2Circular(14f, 14f), DustID.GemAmethyst, velocity, 60, color, Main.rand.NextFloat(1.1f, 1.7f) + power * 0.3f);
        dust.noGravity = true;
    }
    private void TriggerImpact(Player player, float power)
    {
        impactTriggered = true;
        Vector2 headPosition = GetAxeHeadPosition();
        player.GetModPlayer<CosmicVoidAxePlayer>().AddShake(MathHelper.Lerp(8f, 16f, power), 28);

        SoundEngine.PlaySound(SoundID.Item62 with { Volume = 0.95f, Pitch = -0.50f }, headPosition);
        SoundEngine.PlaySound(SoundID.Item92 with { Volume = 0.85f, Pitch = -0.30f }, headPosition);
        SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.65f, Pitch = -0.20f }, headPosition);

        int souls = (int)MathHelper.Clamp(Projectile.ai[0], 1, CosmicVoidAxePlayer.MaxSouls);
        Color primary = CosmicVoidAxePlayer.GetSoulPrimaryColor(souls);

        Lighting.AddLight(headPosition, primary.ToVector3() * 2.2f);
    }
    private Vector2 GetAxeHeadPosition()
    {
        Texture2D texture = ModContent.Request<Texture2D>($"{Mod.Name}/{RelativeWorldTexturePath}").Value;
        Rectangle frame = texture.Frame();

        Vector2 headLocal = new Vector2((0.70f - 0.10f) * frame.Width * castDirection, (0.10f - 0.84f) * frame.Height);

        float scale = DrawScale * MegaSlashScale;
        return Projectile.Center + headLocal.RotatedBy(Projectile.rotation) * scale;
    }
}
