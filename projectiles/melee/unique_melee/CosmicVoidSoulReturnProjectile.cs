using System;
using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.melee.unique_melee;
public sealed class CosmicVoidSoulReturnProjectile : ModProjectile
{
    private const float StartRadius = 320f;
    private const int TailFadeOutFrames = 12;
    private static readonly int[] RingSlots = { 5, 4, 4 };
    private static readonly float[] RingRadii = { 38f, 64f, 92f };
    private static readonly float[] RingSquash = { 0.45f, 0.45f, 0.45f };
    private float[] startAngles = Array.Empty<float>();
    private bool[] landedSouls = Array.Empty<bool>();
    public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Starfury;
    public override void SetDefaults()
    {
        Projectile.width = 2;
        Projectile.height = 2;
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = CosmicVoidAxePlayer.SoulReturnFrames + TailFadeOutFrames + 4;
        Projectile.alpha = 0;
    }
    public override bool ShouldUpdatePosition() => false;
    public override void OnSpawn(IEntitySource source)
    {
        int totalSouls = (int)MathHelper.Clamp(Projectile.ai[0], 1, CosmicVoidAxePlayer.MaxSouls);
        startAngles = new float[totalSouls];
        landedSouls = new bool[totalSouls];
        for (int i = 0; i < totalSouls; i++)
        {
            startAngles[i] = i * MathHelper.TwoPi / totalSouls + Main.rand.NextFloat(-0.6f, 0.6f);
        }
    }
    public override void AI()
    {
        Player player = Main.player[Projectile.owner];
        if (!player.active || player.dead)
        {
            Projectile.Kill();
            return;
        }
        Projectile.Center = player.MountedCenter;
        Projectile.localAI[0]++;
        int totalSouls = (int)MathHelper.Clamp(Projectile.ai[0], 1, CosmicVoidAxePlayer.MaxSouls);
        if (startAngles.Length != totalSouls)
        {
            startAngles = new float[totalSouls];
            landedSouls = new bool[totalSouls];
            for (int i = 0; i < totalSouls; i++)
                startAngles[i] = i * MathHelper.TwoPi / totalSouls;
        }
        CosmicVoidAxePlayer modPlayer = player.GetModPlayer<CosmicVoidAxePlayer>();
        float t = Projectile.localAI[0];
        for (int i = 0; i < totalSouls; i++)
        {
            if (landedSouls[i])
                continue;
            float landingTime = MathHelper.Lerp(0.55f, 1f, totalSouls > 1 ? i / (float)(totalSouls - 1) : 1f) * CosmicVoidAxePlayer.SoulReturnFrames;
            if (t < landingTime)
                continue;
            landedSouls[i] = true;
            if (player.whoAmI == Main.myPlayer)
            {
                Vector2 landingPos = GetSoulLandingPosition(player, i);
                Color color = CosmicVoidAxePlayer.GetSoulSecondaryColor(modPlayer.Souls);
                for (int j = 0; j < 14; j++)
                {
                    float angle = j / 14f * MathHelper.TwoPi;
                    Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(1.2f, 2.4f);
                    Dust dust = Dust.NewDustPerfect(landingPos, DustID.SpectreStaff, velocity, 60, color, Main.rand.NextFloat(0.85f, 1.25f));
                    dust.noGravity = true;
                }
                for (int j = 0; j < 4; j++)
                {
                    Vector2 velocity = Main.rand.NextVector2Circular(0.9f, 0.9f);
                    Dust dust = Dust.NewDustPerfect(landingPos, DustID.GemDiamond, velocity, 30, Color.White, Main.rand.NextFloat(0.75f, 1f));
                    dust.noGravity = true;
                }
                SoundEngine.PlaySound(SoundID.Item28 with { Volume = 0.32f, Pitch = 0.45f + i * 0.05f, MaxInstances = 4 }, landingPos);
            }
        }
        bool allLanded = true;
        for (int i = 0; i < totalSouls; i++)
            if (!landedSouls[i]) { allLanded = false; break; }
        if (allLanded && Projectile.timeLeft > TailFadeOutFrames)
            Projectile.timeLeft = TailFadeOutFrames;
    }
    private Vector2 GetSoulLandingPosition(Player player, int index)
    {
        int placed = 0;
        for (int ring = 0; ring < RingSlots.Length; ring++)
        {
            int slotsOnRing = RingSlots[ring];
            if (index < placed + slotsOnRing)
            {
                int slot = index - placed;
                float radius = RingRadii[ring];
                float squash = RingSquash[ring];
                float time = Main.GlobalTimeWrappedHourly;

                float[] spinSpeeds = { 0.32f, 0.20f, 0.13f };
                float direction = (ring % 2 == 0) ? 1f : -1f;
                float spinPhase = time * spinSpeeds[ring] * MathHelper.TwoPi * direction + ring * 0.7f;
                float angle = spinPhase + slot * MathHelper.TwoPi / slotsOnRing;

                Vector2 offset = new Vector2((float)Math.Cos(angle) * radius, (float)Math.Sin(angle) * radius * squash);
                return player.MountedCenter - new Vector2(0f, 8f) + offset;
            }
            placed += slotsOnRing;
        }
        return player.MountedCenter;
    }
    private Vector2 GetSoulStartPosition(Player player, int index)
    {
        if (index < 0 || index >= startAngles.Length)
            return player.MountedCenter;
        float angle = startAngles[index];
        return player.MountedCenter - new Vector2(0f, 8f) + new Vector2((float)Math.Cos(angle) * StartRadius, (float)Math.Sin(angle) * StartRadius * 0.7f);
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Player player = Main.player[Projectile.owner];
        CosmicVoidAxePlayer modPlayer = player.GetModPlayer<CosmicVoidAxePlayer>();
        int totalSouls = startAngles.Length;

        if (totalSouls <= 0)
            return false;
        SpriteBatch sb = Main.spriteBatch;
        Texture2D star = TextureAssets.Projectile[ProjectileID.Starfury].Value;
        Rectangle frame = star.Frame();
        Vector2 origin = frame.Size() * 0.5f;

        sb.End();
        sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        float t = Projectile.localAI[0];
        for (int i = 0; i < totalSouls; i++)
        {
            float landingFrame = MathHelper.Lerp(0.55f, 1f, totalSouls > 1 ? i / (float)(totalSouls - 1) : 1f) * CosmicVoidAxePlayer.SoulReturnFrames;
            float firstFrame = MathHelper.Lerp(0f, 0.45f, totalSouls > 1 ? i / (float)(totalSouls - 1) : 0f) * CosmicVoidAxePlayer.SoulReturnFrames;
            if (t < firstFrame)
                continue;
            if (landedSouls[i] && t > landingFrame + 4f)
                continue;
            float progress = MathHelper.Clamp((t - firstFrame) / Math.Max(1f, landingFrame - firstFrame), 0f, 1f);
            float eased = progress < 0.5f ? 4f * progress * progress * progress : 1f - (float)Math.Pow(-2f * progress + 2f, 3f) / 2f;
            Vector2 startWorld = GetSoulStartPosition(player, i);
            Vector2 endWorld = GetSoulLandingPosition(player, i);
            Vector2 currentWorld = Vector2.Lerp(startWorld, endWorld, eased);
            Vector2 currentScreen = currentWorld - Main.screenPosition;

            int colorSouls = Math.Max(modPlayer.Souls, 1);
            Color primary = CosmicVoidAxePlayer.GetSoulPrimaryColor(colorSouls);
            Color secondary = CosmicVoidAxePlayer.GetSoulSecondaryColor(colorSouls);

            Color soulColor = Color.Lerp(primary, secondary, 0.5f);
            float intensity = 0.45f + 0.55f * eased;
            Color cyanGhost = new Color(80, 220, 255) * intensity;
            Color magentaGhost = new Color(255, 90, 200) * intensity;

            float scale = MathHelper.Lerp(0.10f, 0.26f, eased);
            float rotation = (float)Math.Atan2(endWorld.Y - startWorld.Y, endWorld.X - startWorld.X) + MathHelper.PiOver2;
            int tailSteps = 8;

            for (int s = tailSteps; s >= 1; s--)
            {
                float echoProgress = MathHelper.Clamp(eased - s * 0.045f, 0f, 1f);
                if (echoProgress <= 0f)
                    continue;
                float echoEased = echoProgress < 0.5f ? 4f * echoProgress * echoProgress * echoProgress : 1f - (float)Math.Pow(-2f * echoProgress + 2f, 3f) / 2f;
                Vector2 echoWorld = Vector2.Lerp(startWorld, endWorld, echoEased);
                Vector2 echoScreen = echoWorld - Main.screenPosition;
                float echoFade = (1f - s / (float)(tailSteps + 1)) * intensity;
                sb.Draw(star, echoScreen, frame, soulColor * echoFade * 0.55f, rotation, origin, scale * 0.85f, SpriteEffects.None, 0f);
            }
            sb.Draw(star, currentScreen + new Vector2(-1.4f, -0.6f), frame, cyanGhost, rotation, origin, scale * 1.04f, SpriteEffects.None, 0f);
            sb.Draw(star, currentScreen + new Vector2(1.4f, 0.6f), frame, magentaGhost, rotation, origin, scale * 1.04f, SpriteEffects.None, 0f);
            sb.Draw(star, currentScreen, frame, soulColor * (0.45f + 0.55f * eased), rotation, origin, scale * 1.85f, SpriteEffects.None, 0f);
            sb.Draw(star, currentScreen, frame, soulColor, rotation, origin, scale, SpriteEffects.None, 0f);
            sb.Draw(star, currentScreen, frame, Color.White * (0.55f + 0.4f * eased), rotation, origin, scale * 0.55f, SpriteEffects.None, 0f);
        }
        sb.End();
        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        return false;
    }
}
