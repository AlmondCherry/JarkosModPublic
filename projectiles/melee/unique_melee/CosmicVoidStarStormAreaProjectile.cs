using System;
using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.unique_melee;
public sealed class CosmicVoidStarStormAreaProjectile : ModProjectile
{
    private const int DurationFrames = 168;
    private const int RiftOpenFrames = 40;
    private const int FinalBurstFrame = DurationFrames - 14;
    private const int FlightDelayFrames = 43;
    private const int RiftSoundFadeFrames = 60;
    private const float PullStartProgress = 0.18f;
    private const float PullEndProgress = 0.86f;
    private const string RiftMaskTexturePath = "textures/Helfeim/CosmicVoidRiftMask";
    private const string RiftEffectPath = "Shaders/Helfeim/CosmicRift";
    public static readonly SoundStyle RiftSound = new SoundStyle("Jarkos/sounds/HelfeimFault")
    {
        Volume = 1.1f, Pitch = -0.05f, PitchVariance = 0.06f, MaxInstances = 2, SoundLimitBehavior = SoundLimitBehavior.IgnoreNew
    };
    public static readonly SoundStyle OpenSound = new SoundStyle("Jarkos/sounds/HelfeimOpen")
    {
        Volume = 0.85f, PitchVariance = 0.08f, MaxInstances = 2, SoundLimitBehavior = SoundLimitBehavior.IgnoreNew
    };
    private bool finalBurstDone;
    private int delayCounter;
    private int totalStarsAtStart;
    private SlotId riftSoundSlot;
    private float riftSoundBaseVolume;
    public override string Texture => "Terraria/Images/MagicPixel";
    public override void SetDefaults()
    {
        Projectile.width = 2;
        Projectile.height = 2;
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = DurationFrames + FlightDelayFrames + 8;
    }
    public override bool ShouldUpdatePosition() => false;
    public override void AI()
    {
        if (delayCounter < FlightDelayFrames)
        {
            delayCounter++;
            return;
        }
        Projectile.localAI[0]++;
        int souls = (int)MathHelper.Clamp(Projectile.ai[0], 1, CosmicVoidAxePlayer.MaxSouls);
        int starsLeft = (int)Projectile.ai[1];
        int damage = (int)Projectile.ai[2];
        float power = CosmicVoidAxePlayer.MaxSouls > 1 ? (souls - 1) / (float)(CosmicVoidAxePlayer.MaxSouls - 1) : 1f;
        float areaRadius = MathHelper.Lerp(620f, 1180f, power);
        Vector2 portalPosition = GetPortalPosition(power);
        if (Projectile.localAI[0] == 1f && Main.myPlayer == Projectile.owner)
        {
            totalStarsAtStart = starsLeft;
            SoundEngine.PlaySound(OpenSound, portalPosition);
            riftSoundSlot = SoundEngine.PlaySound(RiftSound, portalPosition);
            riftSoundBaseVolume = RiftSound.Volume;
            CosmicItemShaderSystem.StartStarStormUltimate(Projectile.Center, power, DurationFrames + 44);
            CosmicItemShaderSystem.SpawnMiniSpaceDistortion(portalPosition, MathHelper.Lerp(0.6f, 1f, power));
            SpawnOpeningShockwave(portalPosition, souls, power);
            ApplyImpactStrike(portalPosition, areaRadius, damage, power);
        }
        UpdateRiftSound(portalPosition);
        if (Main.myPlayer == Projectile.owner && starsLeft > 0)
        {
            int interval = (int)MathHelper.Lerp(4f, 2f, power);
            int burst = Projectile.localAI[0] < 60f ? 2 : 1;
            if (Projectile.localAI[0] >= RiftOpenFrames && Projectile.localAI[0] % interval == 0f)
            {
                for (int i = 0; i < burst && starsLeft > 0; i++)
                {
                    int starIndex = totalStarsAtStart - starsLeft;
                    SpawnStar(souls, damage, areaRadius, power, portalPosition, i, starIndex, totalStarsAtStart);
                    starsLeft--;
                }
                Projectile.ai[1] = starsLeft;
                Projectile.netUpdate = true;
            }
        }
        PullEnemies(areaRadius, power);
        SpawnRiftDust(portalPosition, areaRadius, souls, power);
        if (!finalBurstDone && Projectile.localAI[0] >= FinalBurstFrame)
            TriggerFinalBurst(areaRadius, souls, damage, power);
        if (Main.rand.NextBool(4))
        {
            Color color = CosmicVoidAxePlayer.GetSoulSecondaryColor(souls);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(areaRadius * 0.72f, 120f), DustID.PurpleTorch, Main.rand.NextVector2Circular(0.8f, 0.8f), 100, color, Main.rand.NextFloat(0.75f, 1.2f));
            dust.noGravity = true;
        }
    }
    private void SpawnStar(int souls, int damage, float areaRadius, float power, Vector2 portalPosition, int burstIndex, int starIndex, int totalStars)
    {
        float t = (totalStars > 1) ? starIndex / (float)(totalStars - 1) : 0.5f;
        t = MathHelper.Clamp(t, 0f, 1f);
        float launchAngle = MathHelper.Lerp(-MathHelper.Pi * 0.92f, -MathHelper.Pi * 0.08f, t);
        launchAngle += (float)Math.Sin(t * MathHelper.Pi * 3.2f + burstIndex * 0.55f) * 0.14f;
        launchAngle += Main.rand.NextFloat(-0.05f, 0.05f);
        float speedShape = 1.0f + 0.55f * (1f - (float)Math.Sin(t * MathHelper.Pi));
        float speed = (16f + power * 9f) * speedShape * Main.rand.NextFloat(0.94f, 1.06f);
        Vector2 velocity = new Vector2((float)Math.Cos(launchAngle), (float)Math.Sin(launchAngle)) * speed;
        Vector2 spawnPosition = portalPosition + Main.rand.NextVector2Circular(14f, 8f);
        Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPosition, velocity, ModContent.ProjectileType<CosmicVoidFallingStarProjectile>(), damage, 5f + souls * 0.22f, Projectile.owner, souls, Projectile.whoAmI);
        Color color = CosmicVoidAxePlayer.GetSoulSecondaryColor(souls);
        Vector2 backDir = -velocity.SafeNormalize(Vector2.UnitY);
        for (int i = 0; i < 8; i++)
        {
            Dust dust = Dust.NewDustPerfect(spawnPosition, DustID.PurpleTorch, backDir.RotatedByRandom(0.55f) * Main.rand.NextFloat(0.8f, 2.8f), 70, color, Main.rand.NextFloat(0.8f, 1.35f));
            dust.noGravity = true;
        }
    }
    private Vector2 GetPortalPosition(float power)
    {
        return Projectile.Center;
    }
    private void UpdateRiftSound(Vector2 portalPosition)
    {
        if (!SoundEngine.TryGetActiveSound(riftSoundSlot, out ActiveSound activeSound) || activeSound == null)
            return;
        activeSound.Position = portalPosition;
        int life = (int)Projectile.localAI[0];
        int totalLife = DurationFrames + 8;
        float envelope;
        if (life <= 12)
            envelope = MathHelper.Clamp(life / 12f, 0f, 1f);
        else if (life >= totalLife - RiftSoundFadeFrames)
        {
            int framesIntoFade = life - (totalLife - RiftSoundFadeFrames);
            envelope = MathHelper.Clamp(1f - framesIntoFade / (float)RiftSoundFadeFrames, 0f, 1f);
        }
        else
        {
            envelope = 1f;
        }
        activeSound.Volume = riftSoundBaseVolume * envelope;
        if (envelope <= 0.001f)
        {
            activeSound.Stop();
            riftSoundSlot = SlotId.Invalid;
        }
    }
    public override void OnKill(int timeLeft)
    {
        if (SoundEngine.TryGetActiveSound(riftSoundSlot, out ActiveSound activeSound) && activeSound != null)
        {
            activeSound.Stop();
            riftSoundSlot = SlotId.Invalid;
        }
    }
    private void PullEnemies(float areaRadius, float power)
    {
        float progress = MathHelper.Clamp(Projectile.localAI[0] / DurationFrames, 0f, 1f);
        if (progress < PullStartProgress || progress > PullEndProgress)
            return;
        float pullProgress = (progress - PullStartProgress) / (PullEndProgress - PullStartProgress);
        float strength = MathHelper.Lerp(0.035f, 0.125f, power) * (float)Math.Sin(pullProgress * MathHelper.Pi);
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC npc = Main.npc[i];
            if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.lifeMax <= 5 || npc.boss)
                continue;
            Vector2 toCenter = Projectile.Center - npc.Center;
            float distance = toCenter.Length();
            if (distance <= 32f || distance > areaRadius)
                continue;
            Vector2 pullVelocity = toCenter.SafeNormalize(Vector2.Zero) * MathHelper.Lerp(2.2f, 7.5f, 1f - distance / areaRadius) * strength;
            npc.velocity = Vector2.Lerp(npc.velocity, npc.velocity + pullVelocity, 0.72f);
        }
    }
    private void SpawnRiftDust(Vector2 portalPosition, float areaRadius, int souls, float power)
    {
        Color primary = CosmicVoidAxePlayer.GetSoulPrimaryColor(souls);
        Color secondary = CosmicVoidAxePlayer.GetSoulSecondaryColor(souls);
        float timer = Projectile.localAI[0];
        if (Main.rand.NextBool(2))
        {
            float angle = timer * 0.16f + Main.rand.NextFloat(MathHelper.TwoPi);
            Vector2 offset = new Vector2((float)Math.Cos(angle) * Main.rand.NextFloat(18f, 36f + power * 20f), (float)Math.Sin(angle) * Main.rand.NextFloat(46f, 86f + power * 32f));
            Dust dust = Dust.NewDustPerfect(portalPosition + offset, DustID.SpectreStaff, (-offset).SafeNormalize(Vector2.UnitY).RotatedByRandom(0.35f) * Main.rand.NextFloat(0.5f, 1.9f), 70, secondary, Main.rand.NextFloat(0.75f, 1.45f));
            dust.noGravity = true;
        }
        if (timer % 5f == 0f)
        {
            Vector2 ringPosition = Projectile.Center + Main.rand.NextVector2CircularEdge(areaRadius * Main.rand.NextFloat(0.36f, 0.88f), 72f);
            Vector2 velocity = (Projectile.Center - ringPosition).SafeNormalize(Vector2.UnitY).RotatedByRandom(0.35f) * Main.rand.NextFloat(0.35f, 1.4f);
            Dust dust = Dust.NewDustPerfect(ringPosition, DustID.GemAmethyst, velocity, 90, primary, Main.rand.NextFloat(0.55f, 1.05f));
            dust.noGravity = true;
        }
    }
    private void SpawnOpeningShockwave(Vector2 portalPosition, int souls, float power)
    {
        Color primary = CosmicVoidAxePlayer.GetSoulPrimaryColor(souls);
        Color secondary = CosmicVoidAxePlayer.GetSoulSecondaryColor(souls);
        int ringDustCount = 56;
        for (int i = 0; i < ringDustCount; i++)
        {
            float angle = i * MathHelper.TwoPi / ringDustCount;
            Vector2 dir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle) * 0.55f);
            Vector2 spawnPos = portalPosition + dir * 36f;
            Vector2 velocity = dir * Main.rand.NextFloat(7.5f, 12.5f + power * 4f);
            Color color = (i % 2 == 0) ? secondary : Color.Lerp(primary, Color.White, 0.25f);
            Dust dust = Dust.NewDustPerfect(spawnPos, DustID.GemAmethyst, velocity, 30, color, Main.rand.NextFloat(1.2f, 1.85f));
            dust.noGravity = true;
            dust.fadeIn = 1.2f;
        }
        for (int i = 0; i < 26; i++)
        {
            Vector2 velocity = Main.rand.NextVector2CircularEdge(3f, 3f) * Main.rand.NextFloat(0.7f, 1.3f);
            Dust dust = Dust.NewDustPerfect(portalPosition, DustID.PurpleTorch, velocity, 40, Color.Lerp(secondary, Color.White, 0.45f), Main.rand.NextFloat(1.0f, 1.55f));
            dust.noGravity = true;
        }
        Player player = Main.player[Projectile.owner];
        player.GetModPlayer<CosmicVoidAxePlayer>().AddShake(4.5f + power * 3f, 18);
    }
    private void ApplyImpactStrike(Vector2 portalPosition, float areaRadius, int damage, float power)
    {
        int strikeDamage = (int)(damage * MathHelper.Lerp(10f, 20f, power));
        float strikeKnockback = 14f + power * 10f;
        float impactRadius = MathHelper.Lerp(360f, 540f, power);
        float impactRadiusSq = impactRadius * impactRadius;
        int slowTicks = 60 * 3 + (int)(power * 60f);
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC npc = Main.npc[i];
            if (!npc.active || npc.dontTakeDamage || npc.friendly || npc.lifeMax <= 5 || npc.CountsAsACritter)
                continue;
            if (Vector2.DistanceSquared(npc.Center, portalPosition) > impactRadiusSq)
                continue;
            int hitDirection = npc.Center.X >= portalPosition.X ? 1 : -1;
            npc.SimpleStrikeNPC(strikeDamage, hitDirection, false, strikeKnockback, DamageClass.Melee);
            if (!npc.buffImmune[BuffID.Slow])
                npc.AddBuff(BuffID.Slow, slowTicks);
        }
    }
    private void TriggerFinalBurst(float areaRadius, int souls, int damage, float power)
    {
        finalBurstDone = true;
        Player player = Main.player[Projectile.owner];
        player.GetModPlayer<CosmicVoidAxePlayer>().AddShake(10f + power * 10f, 28);
        SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.68f, Pitch = -0.25f }, Projectile.Center);
        SoundEngine.PlaySound(SoundID.Item74 with { Volume = 0.58f, Pitch = -0.38f }, Projectile.Center);
        SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.7f, Pitch = -0.55f }, Projectile.Center);
        CosmicItemShaderSystem.SpawnMiniSpaceDistortion(Projectile.Center, MathHelper.Lerp(0.7f, 1f, power));
        if (Main.myPlayer == Projectile.owner)
        {
            int burstCount = 8 + (int)(power * 8f);
            for (int i = 0; i < burstCount; i++)
            {
                float angle = MathHelper.TwoPi * i / burstCount + Main.rand.NextFloat(-0.16f, 0.16f);
                Vector2 spawnPosition = Projectile.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(80f, 150f);
                Vector2 velocity = (Projectile.Center - spawnPosition).SafeNormalize(Vector2.UnitY).RotatedByRandom(1.35f) * Main.rand.NextFloat(9f, 16f + power * 5f);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPosition + new Vector2(0f, -120f), velocity, ModContent.ProjectileType<CosmicVoidFallingStarProjectile>(), (int)(damage * MathHelper.Lerp(0.72f, 1.10f, power)), 6.5f + souls * 0.22f, Projectile.owner, souls, Projectile.whoAmI);
            }
        }
        Color color = CosmicVoidAxePlayer.GetSoulSecondaryColor(souls);
        for (int i = 0; i < 72; i++)
        {
            Vector2 velocity = Main.rand.NextVector2CircularEdge(7f + power * 5f, 7f + power * 5f) * Main.rand.NextFloat(0.55f, 1.15f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(areaRadius * 0.12f, 72f), DustID.PurpleTorch, velocity, 60, color, Main.rand.NextFloat(1.0f, 2.1f));
            dust.noGravity = true;
        }
        Color primary = CosmicVoidAxePlayer.GetSoulPrimaryColor(souls);
        int ringDustCount = 84;
        for (int i = 0; i < ringDustCount; i++)
        {
            float angle = i * MathHelper.TwoPi / ringDustCount;
            Vector2 dir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
            Vector2 spawnPos = Projectile.Center + dir * 64f;
            Vector2 velocity = dir * (10f + power * 6f);
            Color ringColor = (i % 3 == 0) ? Color.White : (i % 3 == 1 ? primary : color);
            Dust dust = Dust.NewDustPerfect(spawnPos, DustID.GemAmethyst, velocity, 30, ringColor, Main.rand.NextFloat(1.4f, 1.95f));
            dust.noGravity = true;
            dust.fadeIn = 1.3f;
        }
        Projectile.localAI[1] = Projectile.localAI[0];
    }
    private Vector2 PickAimPoint(float areaRadius)
    {
        NPC closest = null;
        float closestDistance = areaRadius;
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC npc = Main.npc[i];
            if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.lifeMax <= 5)
                continue;
            float distance = Vector2.Distance(npc.Center, Projectile.Center);
            if (distance < closestDistance && Collision.CanHitLine(Projectile.Center, 1, 1, npc.Center, 1, 1))
            {
                closest = npc;
                closestDistance = distance;
            }
        }
        if (closest != null)
            return closest.Center + closest.velocity * 12f + Main.rand.NextVector2Circular(34f, 26f);
        return Projectile.Center + new Vector2(Main.rand.NextFloat(-areaRadius, areaRadius), Main.rand.NextFloat(-140f, 190f));
    }
    public override bool PreDraw(ref Color lightColor)
    {
        DrawCosmicRift();
        DrawFinalBurstFlash();
        return false;
    }
    private void DrawFinalBurstFlash()
    {
        if (!finalBurstDone || Projectile.localAI[1] <= 0f)
            return;
        float framesSinceBurst = Projectile.localAI[0] - Projectile.localAI[1];
        const float flashFrames = 18f;
        if (framesSinceBurst < 0f || framesSinceBurst > flashFrames)
            return;
        float t = framesSinceBurst / flashFrames;
        float opacity = 1f - t;
        opacity = opacity * opacity;
        if (opacity <= 0.01f)
            return;
        int souls = (int)MathHelper.Clamp(Projectile.ai[0], 1, CosmicVoidAxePlayer.MaxSouls);
        Color secondary = CosmicVoidAxePlayer.GetSoulSecondaryColor(souls);
        Vector2 position = Projectile.Center - Main.screenPosition;
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Rectangle frame = new Rectangle(0, 0, 1, 1);
        Vector2 origin = new Vector2(0.5f, 0.5f);
        SpriteBatch spriteBatch = Main.spriteBatch;
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        float ringRadius = MathHelper.Lerp(80f, 360f, t);
        float ringThickness = MathHelper.Lerp(40f, 8f, t);
        Color ringColor = secondary * (opacity * 0.55f);
        Color ringOuter = Color.White * (opacity * 0.30f);
        DrawRing(pixel, position, ringRadius, ringThickness, ringColor, frame);
        DrawRing(pixel, position, ringRadius * 1.18f, ringThickness * 0.6f, ringOuter, frame);
        float coreScale = MathHelper.Lerp(140f, 360f, t);
        Color coreColor = Color.Lerp(secondary, Color.White, 0.55f) * (opacity * 0.85f);
        spriteBatch.Draw(pixel, position, frame, coreColor, 0f, origin, coreScale, SpriteEffects.None, 0f);
        spriteBatch.Draw(pixel, position, frame, Color.White * (opacity * 0.55f), 0f, origin, coreScale * 0.45f, SpriteEffects.None, 0f);
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
    }
    private static void DrawRing(Texture2D pixel, Vector2 center, float radius, float thickness, Color color, Rectangle frame)
    {
        const int segments = 64;
        float circumference = MathHelper.TwoPi * radius;
        float segmentLength = circumference / segments + 2f;
        Vector2 origin = new Vector2(0.5f, 0.5f);
        SpriteBatch spriteBatch = Main.spriteBatch;
        for (int i = 0; i < segments; i++)
        {
            float angle = i * MathHelper.TwoPi / segments;
            Vector2 dir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
            Vector2 position = center + dir * radius;
            Vector2 scale = new Vector2(thickness, segmentLength);
            spriteBatch.Draw(pixel, position, frame, color, angle, origin, scale, SpriteEffects.None, 0f);
        }
    }
    private void DrawCosmicRift()
    {
        if (Main.dedServ)
            return;
        int souls = (int)MathHelper.Clamp(Projectile.ai[0], 1, CosmicVoidAxePlayer.MaxSouls);
        float power = CosmicVoidAxePlayer.MaxSouls > 1 ? (souls - 1) / (float)(CosmicVoidAxePlayer.MaxSouls - 1) : 1f;
        float progress = MathHelper.Clamp(Projectile.localAI[0] / DurationFrames, 0f, 1f);
        float opacity = MathHelper.Clamp(progress / 0.16f, 0f, 1f) * (1f - MathHelper.Clamp((progress - 0.92f) / 0.08f, 0f, 1f));
        if (opacity <= 0f)
            return;
        Asset<Texture2D> maskAsset = ModContent.Request<Texture2D>($"{Mod.Name}/{RiftMaskTexturePath}", AssetRequestMode.ImmediateLoad);
        Texture2D maskTexture = maskAsset.Value;
        Rectangle frame = maskTexture.Frame();
        Vector2 origin = frame.Size() * 0.5f;
        Vector2 position = GetPortalPosition(power) - Main.screenPosition;
        float baseWidth = MathHelper.Lerp(520f, 720f, power);
        float baseHeight = MathHelper.Lerp(220f, 340f, power);
        Vector2 scale = new Vector2(baseWidth / frame.Width, baseHeight / frame.Height);
        Color primary = CosmicVoidAxePlayer.GetSoulPrimaryColor(souls);
        Color secondary = CosmicVoidAxePlayer.GetSoulSecondaryColor(souls);
        Main.EntitySpriteDraw(maskTexture, position, frame, new Color(4, 0, 14) * (0.55f * opacity), 0f, origin, scale * 1.04f, SpriteEffects.None, 0f);
        Effect effect;
        try
        {
            effect = ModContent.Request<Effect>($"{Mod.Name}/{RiftEffectPath}", AssetRequestMode.ImmediateLoad).Value;
        }
        catch
        {
            return;
        }
        float shaderTime = Projectile.identity * 0.37f + 17.5f + Projectile.localAI[0] * 0.06f;
        effect.Parameters["uColor"]?.SetValue(primary.ToVector3());
        effect.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
        effect.Parameters["uOpacity"]?.SetValue(opacity);
        effect.Parameters["uProgress"]?.SetValue(progress);
        effect.Parameters["uIntensity"]?.SetValue(power);
        effect.Parameters["uSaturation"]?.SetValue(shaderTime);
        SpriteBatch spriteBatch = Main.spriteBatch;
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        effect.Parameters["uOpacity"]?.SetValue(opacity * 0.55f);
        effect.CurrentTechnique.Passes[0].Apply();
        spriteBatch.Draw(maskTexture, position, frame, Color.White, 0f, origin, scale * 1.18f, SpriteEffects.None, 0f);
        effect.Parameters["uOpacity"]?.SetValue(opacity);
        effect.CurrentTechnique.Passes[0].Apply();
        spriteBatch.Draw(maskTexture, position, frame, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
        effect.Parameters["uOpacity"]?.SetValue(opacity * 0.85f);
        effect.CurrentTechnique.Passes[0].Apply();
        spriteBatch.Draw(maskTexture, position, frame, Color.White, 0f, origin, scale * 0.62f, SpriteEffects.None, 0f);
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
    }
}
