using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.systems;
using jarkos.projectiles.melee.unique_melee;
using jarkos.items.weapons.melee.unique_melee;
using jarkos.particles;
using Humanizer;
namespace sdd.topor.Content.Projectiles;
public sealed class CosmicVoidAxeSwingProjectile : ModProjectile
{
    public const int AttackGroundSlam = 1;
    public const int AttackAirSpin = 2;
    public const int AttackSoulChannel = 3;
    public const int GroundWaveCooldownFrames = 120;
    private const int ComboWindow = 52;
    private const float DrawScale = 0.54f;
    public const float GroundSlamStartAnglePi = -0.55f;
    public const float GroundSlamEndAnglePi = -0.10f;
    public const float GroundSlamImpactProgress = 0.82f;
    public const float GroundSlamWindupProgress = 0.42f;
    public const float GroundSlamWindupAngleProgress = 0.12f;
    public const float AxeHoldOriginX = 0.10f;
    public const float AxeHoldOriginY = 0.84f;
    public const float AxeHoldForwardOffset = 0f;
    public const float AxeHoldVerticalOffset = 0f;
    public const float AxeHitboxLineWidth = 34f;
    public const float AxeHitboxGroundSlamActiveStart = 0.46f;
    public const float AxeHitboxAirSpinActiveStart = 0.12f;
    public const float AxeHitboxInnerTopX = 0.48f;
    public const float AxeHitboxInnerTopY = 0.66f;
    public const float AxeHitboxOuterX = 0.90f;
    public const float AxeHitboxOuterY = 0.54f;
    public const float AxeHitboxTipX = 0.96f;
    public const float AxeHitboxTipY = 0.78f;
    public const float AxeHitboxLowerX = 0.52f;
    public const float AxeHitboxLowerY = 0.94f;
    public const float AxeGroundContactHandleX = 0.48f;
    public const float AxeGroundContactHandleY = 0.66f;
    public const float AxeGroundContactInnerTopX = 0.90f;
    public const float AxeGroundContactInnerTopY = 0.54f;
    public const float AxeGroundContactLowerX = 0.52f;
    public const float AxeGroundContactLowerY = 0.94f;
    public const float AxeGroundContactTipX = 0.96f;
    public const float AxeGroundContactTipY = 0.78f;
    public const float AxeGroundContactBottomX = 0.54f;
    public const float AxeGroundContactBottomY = 0.98f;
    public const int GroundSlamFrames = 34;
    private const int RecoveryFrames = 6;
    private const int AirSpinFrames = 44;
    public const int ChannelLongFrames = 64;
    private const int ChannelReleaseGraceFrames = 8;
    private const int ChannelMaxFrames = 150;
    private const int StarStormMinStars = 12;
    private const int StarStormMaxStars = 52;
    private const float StarStormMinDamageMultiplier = 1.25f;
    private const float StarStormMaxDamageMultiplier = 3.2f;
    private const float StarStormMinSpread = 190f;
    private const float StarStormMaxSpread = 620f;
    private const string RelativeWorldTexturePath = "textures/Helfeim/CosmicVoidAxe";
    private const string RelativeWorldCosmicMaskTexturePath = "textures/Helfeim/CosmicVoidAxe_CosmicMask";
    public static readonly SoundStyle SwingSound = new SoundStyle("jarkos/sounds/HelfeimSlash")
    {
        Volume = 0.85f, PitchVariance = 0.4f, MaxInstances = 4, SoundLimitBehavior = SoundLimitBehavior.IgnoreNew
    };
    public static readonly SoundStyle ImpactSound = new SoundStyle("jarkos/sounds/HelfeimImpact")
    {
        Volume = 0.6f, PitchVariance = 0.12f, MaxInstances = 3, SoundLimitBehavior = SoundLimitBehavior.IgnoreNew
    };
    public static readonly SoundStyle ShieldSound = new SoundStyle("jarkos/sounds/HelfeimShi")
    {
        Volume = 0.8f, PitchVariance = 0.08f, MaxInstances = 2, SoundLimitBehavior = SoundLimitBehavior.IgnoreNew
    };
    private bool impactPoseLocked;
    private int impactHoldTimer;
    private int impactDirection;
    private float impactSpriteRotation;
    private Vector2 impactCenter;
    private float chargePushVelocity;
    private float chargePushAngle;
    private int starHitFlashFrames;
    private float starHitFlashIntensity;
    private float channelDrawScale = 1f;
    private int effectiveChannelFrames = ChannelLongFrames;
    private float channelBlinkPhase;
    private float channelBlinkAlpha;
    private int AttackType => (int)Projectile.ai[0];
    private bool IsChannel => AttackType == AttackSoulChannel;
    private bool WasAltUse => Projectile.ai[2] == 1f;
    private bool WaveReleased
    {
        get => Projectile.localAI[1] == 1f;
        set => Projectile.localAI[1] = value ? 1f : 0f;
    }
    public override string Texture => $"{Mod.Name}/{RelativeWorldTexturePath}";
    public static int GetNextCombo(Player player)
    {
        CosmicVoidAxePlayer mp = player.GetModPlayer<CosmicVoidAxePlayer>();
        mp.ComboStep = 1;
        mp.ComboTimer = ComboWindow;
        return 1;
    }
    public override void SetDefaults()
    {
        Projectile.width = 2;
        Projectile.height = 2;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.ownerHitCheck = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.timeLeft = 999;
    }
    public override bool ShouldUpdatePosition() => false;
    public override void AI()
    {
        Player player = Main.player[Projectile.owner];
        if (!player.active || player.dead || player.HeldItem.type != ModContent.ItemType<Helfeim>())
        {
            Projectile.Kill();
            return;
        }
        if (IsChannel)
        {
            AIChannel(player);
            return;
        }
        if (AttackType == AttackGroundSlam && impactPoseLocked)
        {
            AIImpactHold(player);
            return;
        }
        int swingTime = GetTotalTime();
        Projectile.localAI[0]++;
        float progress = GetSwingProgress();
        float eased = AttackType == AttackAirSpin ? EaseInOutCubic(progress) : GetGroundSlamEase(progress);
        float localRotation = MathHelper.Lerp(GetStartAngle(), GetEndAngle(), eased);
        int direction = Projectile.ai[1] == 0f ? player.direction : Math.Sign(Projectile.ai[1]);
        float worldRotation = direction == -1 ? MathHelper.Pi - localRotation : localRotation;
        Vector2 weaponPosition = GetHandPosition(player, worldRotation);
        Projectile.Center = weaponPosition;
        Projectile.rotation = GetSpriteRotation(worldRotation);
        Projectile.spriteDirection = direction;
        player.direction = direction;
        player.heldProj = Projectile.whoAmI;
        player.itemTime = 2;
        player.itemAnimation = 2;
        player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, worldRotation - MathHelper.PiOver2);
        if (Projectile.localAI[0] == 1f)
        {
            SoundEngine.PlaySound(SwingSound, player.Center);
        }
        if (AttackType == AttackGroundSlam && !WaveReleased && progress >= GroundSlamImpactProgress)
            ReleaseGroundWave(player, direction, worldRotation, FindGroundImpact(player, direction, GetBladeGroundContact(worldRotation)));
        if (progress > 0.44f && progress < 1f && Main.rand.NextBool(2))
        {
            Vector2 bladeTip = GetBladeTip(worldRotation);
            Vector2 dustVelocity = (weaponPosition - bladeTip).SafeNormalize(Vector2.UnitY).RotatedByRandom(0.5f) * Main.rand.NextFloat(0.4f, 1.6f);
            Dust dust = Dust.NewDustPerfect(bladeTip, DustID.PurpleTorch, dustVelocity);
            dust.noGravity = true;
            dust.scale = Main.rand.NextFloat(0.75f, 1.25f);
        }
        Lighting.AddLight(GetBladeTip(worldRotation), 0.34f, 0.16f, 0.62f);
        if (Projectile.localAI[0] >= swingTime)
            Projectile.Kill();
    }
    private void AIImpactHold(Player player)
    {
        impactHoldTimer++;
        float bounce = impactHoldTimer < 6 ? -(float)Math.Sin(impactHoldTimer / 6f * MathHelper.Pi) * 8f : 0f;
        Projectile.Center = impactCenter + new Vector2(0f, bounce);
        Projectile.rotation = impactSpriteRotation;
        Projectile.spriteDirection = impactDirection;
        player.direction = impactDirection;
        player.heldProj = Projectile.whoAmI;
        player.itemTime = 2;
        player.itemAnimation = 2;
        Vector2 armDirection = (impactCenter - player.MountedCenter).SafeNormalize(new Vector2(impactDirection, 0f));
        player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armDirection.ToRotation() - MathHelper.PiOver2);
        if (impactHoldTimer >= RecoveryFrames + 2)
            Projectile.Kill();
    }
    private void AIChannel(Player player)
    {
        CosmicVoidAxePlayer modPlayer = player.GetModPlayer<CosmicVoidAxePlayer>();
        int currentSouls = (int)MathHelper.Clamp(modPlayer.Souls, 1, CosmicVoidAxePlayer.MaxSouls);
        effectiveChannelFrames = ChannelLongFrames + (currentSouls - 1) * 8;
        Projectile.localAI[0]++;
        float chargeProgress = MathHelper.Clamp(Projectile.localAI[0] / (float)effectiveChannelFrames, 0f, 1f);
        bool ultReady = Projectile.localAI[0] >= effectiveChannelFrames;
        float lift = EaseInOutCubic(chargeProgress);
        float swayPhase = Projectile.localAI[0] * 0.055f;
        float xSway = (float)Math.Sin(swayPhase) * MathHelper.Lerp(2.6f, 0.8f, chargeProgress);
        Vector2 overheadHoldPosition = player.MountedCenter + new Vector2(player.direction * 8f + xSway, MathHelper.Lerp(-30f, -52f, lift));
        float jitterPower = chargeProgress * chargeProgress;
        if (ultReady)
            jitterPower = 1f;
        float t = Projectile.localAI[0];
        float jitterX = (float)Math.Sin(t * 0.83f)
            + (float)Math.Sin(t * 1.71f + 1.3f) * 0.55f
            + (float)Math.Sin(t * 2.49f + 2.7f) * 0.30f;
        float jitterY = (float)Math.Sin(t * 0.97f + 0.4f)
            + (float)Math.Sin(t * 1.83f + 2.1f) * 0.55f
            + (float)Math.Sin(t * 2.61f + 0.9f) * 0.30f;
        Vector2 jitter = new Vector2(jitterX, jitterY) * 1.5f * jitterPower;
        float rotJitter = (float)Math.Sin(t * 1.31f + 0.55f) * 0.028f * jitterPower;
        const int RecoilFrames = 18;
        int framesSinceReady = (int)(Projectile.localAI[0] - effectiveChannelFrames);
        float recoilLift = 0f;
        float blinkScale = 1f;
        if (framesSinceReady >= 0 && framesSinceReady < RecoilFrames)
        {
            float recoilT = framesSinceReady / (float)RecoilFrames;
            float recoilCurve = (float)Math.Sin(recoilT * MathHelper.Pi);
            recoilLift = -recoilCurve * 4f;
            blinkScale = 1f + recoilCurve * 0.24f;
        }
        chargePushAngle += chargePushVelocity;
        chargePushVelocity *= 0.78f;
        chargePushAngle *= 0.86f;
        chargePushAngle = MathHelper.Clamp(chargePushAngle, -0.10f, 0.10f);
        if (Math.Abs(chargePushVelocity) < 0.0003f) chargePushVelocity = 0f;
        if (Math.Abs(chargePushAngle) < 0.0005f) chargePushAngle = 0f;
        float starFlashScaleBoost = 0f;
        if (starHitFlashFrames > 0)
        {
            float flashT = starHitFlashFrames / 7f;
            float flashCurve = flashT * (2f - flashT);
            starFlashScaleBoost = flashCurve * 0.10f * starHitFlashIntensity;
            starHitFlashFrames--;
            if (starHitFlashFrames <= 0)
                starHitFlashIntensity = 0f;
        }
        float heartbeat = 1f;
        if (chargeProgress > 0.5f && !ultReady)
        {
            float heartT = (chargeProgress - 0.5f) / 0.5f;
            heartbeat = 1f + (float)Math.Sin(t * 0.28f) * 0.045f * heartT;
        }
        float blinkFreq = 0f;
        float blinkAmp = 0f;
        if (chargeProgress > 0.55f)
        {
            float blinkT = MathHelper.Clamp((chargeProgress - 0.55f) / 0.45f, 0f, 1f);
            blinkFreq = MathHelper.Lerp(0.26f, 0.78f, blinkT);
            blinkAmp = blinkT;
        }
        channelBlinkPhase += blinkFreq;
        float pulse = (float)Math.Sin(channelBlinkPhase);
        float blinkPulse = MathHelper.Clamp(pulse, 0f, 1f);
        channelBlinkAlpha = blinkPulse * blinkAmp;
        if (ultReady)
        {
            float readyT = MathHelper.Clamp(framesSinceReady / (float)RecoilFrames, 0f, 1f);
            float readyCurve = (float)Math.Sin(readyT * MathHelper.Pi);
            channelBlinkAlpha = Math.Max(channelBlinkAlpha * 0.3f, readyCurve);
        }
        Projectile.Center = overheadHoldPosition + jitter + new Vector2(0f, recoilLift);
        Projectile.rotation = -MathHelper.PiOver2 * player.direction + rotJitter + chargePushAngle;
        channelDrawScale = blinkScale * heartbeat + starFlashScaleBoost;
        Projectile.spriteDirection = player.direction;
        Projectile.damage = 0;
        Projectile.friendly = false;
        Projectile.hide = false;
        Projectile.timeLeft = 2;
        player.heldProj = Projectile.whoAmI;
        player.itemTime = 2;
        player.itemAnimation = 2;
        player.direction = Projectile.spriteDirection;
        Vector2 armDirection = (overheadHoldPosition - player.MountedCenter).SafeNormalize(new Vector2(player.direction, -1f));
        player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armDirection.ToRotation() - MathHelper.PiOver2);
        modPlayer.SoulChannelTimer = (int)Projectile.localAI[0];
        modPlayer.SoulChannelMaxTimer = effectiveChannelFrames;
        if (Projectile.localAI[0] == 1f)
            SoundEngine.PlaySound(SoundID.Item4 with { Volume = 0.28f, Pitch = 0.25f + modPlayer.SoulPower * 0.35f }, player.Center);
        if (Main.myPlayer == Projectile.owner && !ultReady)
        {
            float soulFactor = (currentSouls - 1) / (float)(CosmicVoidAxePlayer.MaxSouls - 1);
            float intervalStart = MathHelper.Lerp(5f, 3f, soulFactor);
            float intervalEnd = MathHelper.Lerp(3f, 1f, soulFactor);
            int spawnInterval = Math.Max(1, (int)MathHelper.Lerp(intervalStart, intervalEnd, chargeProgress));
            if ((int)Projectile.localAI[0] % spawnInterval == 0)
                SpawnChannelStar(player, 1);
        }
        if (Projectile.localAI[0] == effectiveChannelFrames)
            TriggerUltReadyFlash(player, modPlayer);
        float lightPower = ultReady ? 1.1f : MathHelper.Lerp(0.7f, 1f, chargeProgress);
        Lighting.AddLight(player.Center, CosmicVoidAxePlayer.GetSoulPrimaryColor(modPlayer.Souls).ToVector3() * lightPower);
        bool ownerHoldingRightClick = Main.myPlayer == Projectile.owner && Main.mouseRight;
        bool remoteChanneling = Main.myPlayer != Projectile.owner && player.channel;
        bool channeling = WasAltUse && !player.noItems && !player.CCed && (ownerHoldingRightClick || remoteChanneling);
        int effectiveMaxFrames = effectiveChannelFrames + (ChannelMaxFrames - ChannelLongFrames);
        if ((channeling || Projectile.localAI[0] < ChannelReleaseGraceFrames) && Projectile.localAI[0] < effectiveMaxFrames)
            return;
        int souls = modPlayer.ConsumeSouls();
        if (souls <= 0)
        {
            SoundEngine.PlaySound(SoundID.Item8 with { Volume = 0.25f, Pitch = -0.3f }, player.Center);
            Projectile.Kill();
            return;
        }
        if (Projectile.localAI[0] >= effectiveChannelFrames)
            ReleaseStarStorm(player, souls);
        else
            ActivateSoulShield(player, souls);
        Projectile.Kill();
    }
    private void ReleaseGroundWave(Player player, int direction, float worldRotation, Vector2 impactPosition)
    {
        WaveReleased = true;
        player.GetModPlayer<CosmicVoidAxePlayer>().AddShake(7f, 16);
        if (Main.myPlayer == Projectile.owner)
        {
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), impactPosition + new Vector2(direction * 2f, -2f), Vector2.Zero, ModContent.ProjectileType<CosmicVoidGroundWaveProjectile>(), (int)(Projectile.damage * 2.4f), Projectile.knockBack * 1.15f, Projectile.owner, direction);
        }
        player.GetModPlayer<CosmicVoidAxePlayer>().StartGroundWaveCooldown(GroundWaveCooldownFrames);
        SoundEngine.PlaySound(ImpactSound, impactPosition);
        impactPoseLocked = true;
        impactHoldTimer = 0;
        impactDirection = direction;
        impactSpriteRotation = Projectile.rotation;
        impactCenter = Projectile.Center;
        for (int i = 0; i < 28; i++)
        {
            Vector2 velocity = new Vector2(direction * Main.rand.NextFloat(0.5f, 4.6f), Main.rand.NextFloat(-5.2f, -0.8f));
            Dust dust = Dust.NewDustPerfect(impactPosition + new Vector2(Main.rand.NextFloat(-22f, 22f), Main.rand.NextFloat(-6f, 4f)), DustID.PurpleTorch, velocity);
            dust.noGravity = true;
            dust.scale = Main.rand.NextFloat(0.8f, 1.45f);
        }
    }
    private static Vector2 FindGroundImpact(Player player, int direction, Vector2 bladeContact)
    {
        float minX = player.Center.X + direction * 8f;
        float maxX = player.Center.X + direction * 72f;
        if (direction == -1)
        {
            float left = player.Center.X + direction * 72f;
            float right = player.Center.X + direction * 8f;
            minX = left;
            maxX = right;
        }
        float contactX = MathHelper.Clamp(bladeContact.X, minX, maxX);
        Vector2 start = new Vector2(contactX, Math.Min(bladeContact.Y, player.Bottom.Y) - 24f);
        if (TryFindGroundBelow(start, 240f, out Vector2 groundImpact))
            return groundImpact;
        return new Vector2(contactX, player.Bottom.Y);
    }
    private static bool TryFindGroundBelow(Vector2 start, float maxDistance, out Vector2 impactPosition)
    {
        int tileX = (int)(start.X / 16f);
        int startTileY = (int)(start.Y / 16f);
        int maxTiles = (int)(maxDistance / 16f);
        for (int i = 0; i <= maxTiles; i++)
        {
            int tileY = startTileY + i;
            if (!WorldGen.InWorld(tileX, tileY, 10))
                continue;
            Tile tile = Framing.GetTileSafely(tileX, tileY);
            if (!tile.HasTile || !Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType])
                continue;
            impactPosition = new Vector2(start.X, tileY * 16f);
            return true;
        }
        for (int side = 1; side <= 3; side++)
        {
            for (int sign = -1; sign <= 1; sign += 2)
            {
                int testX = tileX + side * sign;
                for (int i = 0; i <= maxTiles; i++)
                {
                    int tileY = startTileY + i;
                    if (!WorldGen.InWorld(testX, tileY, 10))
                        continue;
                    Tile tile = Framing.GetTileSafely(testX, tileY);
                    if (!tile.HasTile || !Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType])
                        continue;
                    impactPosition = new Vector2(testX * 16f + 8f, tileY * 16f);
                    return true;
                }
            }
        }
        impactPosition = start + Vector2.UnitY * maxDistance;
        return false;
    }
    private void ActivateSoulShield(Player player, int souls)
    {
        player.GetModPlayer<CosmicVoidAxePlayer>().ActivateShield(souls);
        SoundEngine.PlaySound(ShieldSound, player.Center);
        Color color = CosmicVoidAxePlayer.GetSoulSecondaryColor(souls);
        for (int i = 0; i < 36; i++)
        {
            Vector2 velocity = Main.rand.NextVector2CircularEdge(3.6f, 3.6f) * Main.rand.NextFloat(0.65f, 1.25f);
            Dust dust = Dust.NewDustPerfect(player.Center, DustID.GemAmethyst, velocity, 40, color, Main.rand.NextFloat(1f, 1.65f));
            dust.noGravity = true;
        }
        const int RingDustCount = 28;
        const float MarkerRadius = 100f;
        for (int i = 0; i < RingDustCount; i++)
        {
            float angle = i / (float)RingDustCount * MathHelper.TwoPi;
            Vector2 ringPos = player.Center + new Vector2((float)Math.Cos(angle) * MarkerRadius, (float)Math.Sin(angle) * MarkerRadius * 0.78f);
            Vector2 outward = (ringPos - player.Center).SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(0.4f, 1.2f);
            Dust dust = Dust.NewDustPerfect(ringPos, DustID.SpectreStaff, outward, 40, color, Main.rand.NextFloat(0.85f, 1.3f));
            dust.noGravity = true;
        }
        if (Main.myPlayer == Projectile.owner)
            CosmicItemShaderSystem.SpawnMiniSpaceDistortion(player.Center, 0.85f);
    }
    private void ReleaseStarStorm(Player player, int souls)
    {
        souls = (int)MathHelper.Clamp(souls, 1, CosmicVoidAxePlayer.MaxSouls);
        float power = CosmicVoidAxePlayer.MaxSouls > 1 ? (souls - 1) / (float)(CosmicVoidAxePlayer.MaxSouls - 1) : 1f;
        int starCount = (int)MathHelper.Lerp(StarStormMinStars, StarStormMaxStars, power);
        int baseDamage = Projectile.originalDamage > 0 ? Projectile.originalDamage : player.HeldItem.damage;
        int starDamage = (int)(baseDamage * MathHelper.Lerp(StarStormMinDamageMultiplier, StarStormMaxDamageMultiplier, power));
        Vector2 target = Main.myPlayer == Projectile.owner ? Main.MouseWorld : player.Center;
        if (Main.myPlayer == Projectile.owner)
        {
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), target + new Vector2(0f, 140f), Vector2.Zero, ModContent.ProjectileType<CosmicVoidStarStormAxeProjectile>(), 0, 0f, Projectile.owner, souls, target.X, target.Y);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), target, Vector2.Zero, ModContent.ProjectileType<CosmicVoidStarStormAreaProjectile>(), 0, 0f, Projectile.owner, souls, starCount, starDamage);
        }
        SoundEngine.PlaySound(SoundID.Item9 with { Volume = 0.75f, Pitch = -0.15f }, player.Center);
        CosmicVoidAxePlayer modPlayer = player.GetModPlayer<CosmicVoidAxePlayer>();
        modPlayer.AddShake(MathHelper.Lerp(5.5f, 12f, power), 34);
        modPlayer.LockForStarStorm(168);
        int stormCooldown = (int)MathHelper.Lerp(CosmicVoidAxePlayer.StarStormCooldownMinFrames, CosmicVoidAxePlayer.StarStormCooldownMaxFrames, power);
        modPlayer.BeginUltCooldown(stormCooldown);
    }
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {
        Player player = Main.player[Projectile.owner];
        float progress = GetSwingProgress();
        if (IsChannel || Projectile.localAI[0] > GetSwingTime() || progress < GetActiveStart())
            return false;
        int direction = Projectile.ai[1] == 0f ? player.direction : Math.Sign(Projectile.ai[1]);
        float worldRotation;
        if (AttackType == AttackGroundSlam)
        {
            worldRotation = Projectile.rotation - GetRotationOffset(GetSpriteEffects());
        }
        else
        {
            float localRotation = MathHelper.Lerp(GetStartAngle(), GetEndAngle(), EaseInOutCubic(progress));
            worldRotation = direction == -1 ? MathHelper.Pi - localRotation : localRotation;
        }
        float width = AxeHitboxLineWidth;
        float collisionPoint = 0f;
        foreach ((Vector2 start, Vector2 end) in GetBladeHitLines(worldRotation))
        {
            collisionPoint = 0f;
            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, width, ref collisionPoint))
                return true;
        }
        return false;
    }
    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        if (AttackType == AttackAirSpin)
            modifiers.SourceDamage *= 1.55f;
        else if (AttackType == AttackGroundSlam)
            modifiers.SourceDamage *= 1.85f;
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        Main.player[Projectile.owner].GetModPlayer<CosmicVoidAxePlayer>().RegisterAxeHit(target, damageDone);
        for (int i = 0; i < 10; i++)
        {
            Dust dust = Dust.NewDustPerfect(target.Center, DustID.PurpleTorch, Main.rand.NextVector2Circular(2.8f, 2.8f));
            dust.noGravity = true;
            dust.scale = Main.rand.NextFloat(0.9f, 1.35f);
            Color[] hz = new Color[] {Color.MediumPurple, Color.Purple, Color.White};
            Vector2 toCursor = Main.MouseWorld - Projectile.Center;
            toCursor.Normalize();
            new Spark(target.Center, toCursor.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-90f, 90f))) * 16f, hz[Main.rand.Next(0, 3)], 200, Main.rand.NextFloat(1f, 3.75f), 1.5f, 0f).Spawn();
        }
        SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/HelfeimBlood") with { Pitch = Main.rand.NextFloat(-0.25f, 0.25f) }, Projectile.position);
        SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/HelfeimHitNPC") with { Pitch = Main.rand.NextFloat(-0.25f, 0.25f) }, Projectile.position);
    }
    public override bool PreDraw(ref Color lightColor)
    {
        if (Projectile.hide)
            return false;
        Texture2D texture = ModContent.Request<Texture2D>($"{Mod.Name}/{RelativeWorldTexturePath}").Value;
        Rectangle frame = texture.Frame();
        SpriteEffects effects = GetSpriteEffects();
        Vector2 origin = GetTextureOrigin(frame, effects);
        Vector2 position = Projectile.Center - Main.screenPosition;
        float scale = DrawScale * GetVisualScale();
        float opacity = GetDrawOpacity();
        if (IsChannel)
            DrawChannelChromaticGhosts(texture, frame, origin, scale, effects);
        Main.EntitySpriteDraw(texture, position, frame, lightColor * opacity, Projectile.rotation, origin, scale, effects, 0f);
        DrawCosmicMask(position, frame, origin, scale, effects, opacity);
        return false;
    }
    private void DrawChannelChromaticGhosts(Texture2D texture, Rectangle frame, Vector2 origin, float scale, SpriteEffects effects)
    {
        float chargeProgress = GetSwingProgress();
        SpriteBatch spriteBatch = Main.spriteBatch;
        Vector2 position = Projectile.Center - Main.screenPosition;
        bool ultReady = Projectile.localAI[0] >= effectiveChannelFrames;
        Player player = Main.player[Projectile.owner];
        CosmicVoidAxePlayer modPlayer = player.GetModPlayer<CosmicVoidAxePlayer>();
        int souls = (int)MathHelper.Clamp(modPlayer.Souls, 1, CosmicVoidAxePlayer.MaxSouls);
        Color soulPrimary = CosmicVoidAxePlayer.GetSoulPrimaryColor(souls);
        Color soulSecondary = CosmicVoidAxePlayer.GetSoulSecondaryColor(souls);
        float haloIntensity = MathHelper.Clamp(chargeProgress * 1.05f, 0f, 1f);
        float haloBreathe = 0.78f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 4.3f) * 0.22f;
        float haloOpacity = haloIntensity * 0.42f * haloBreathe;
        int framesSinceReady = (int)(Projectile.localAI[0] - effectiveChannelFrames);
        float bloomBoost = 0f;
        if (framesSinceReady >= 0 && framesSinceReady < 24)
        {
            float bloomT = framesSinceReady / 24f;
            float bloomCurve = (float)Math.Sin(bloomT * MathHelper.Pi);
            bloomBoost = bloomCurve * 0.65f;
        }
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        if (haloOpacity > 0.001f || bloomBoost > 0.001f)
        {
            Color halo = soulSecondary * (haloOpacity + bloomBoost * 0.55f);
            halo.A = 0;
            Main.EntitySpriteDraw(texture, position, frame, halo, Projectile.rotation, origin, scale * (1.36f + bloomBoost * 0.18f), effects, 0f);
            Color innerHalo = soulPrimary * (haloOpacity * 1.05f + bloomBoost * 0.6f);
            innerHalo.A = 0;
            Main.EntitySpriteDraw(texture, position, frame, innerHalo, Projectile.rotation, origin, scale * (1.18f + bloomBoost * 0.10f), effects, 0f);
        }
        if (chargeProgress >= 0.20f)
        {
            float chromIntensity = ultReady ? 1f : (chargeProgress - 0.20f) / 0.80f;
            chromIntensity = MathHelper.Clamp(chromIntensity, 0f, 1f);
            float chromPulse = 0.65f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 9.5f) * 0.35f;
            float ghostOpacity = chromIntensity * (ultReady ? 0.85f : 0.55f) * chromPulse;
            float offsetMag = MathHelper.Lerp(2.5f, 5.5f, chromIntensity);
            float offsetAngle = Main.GlobalTimeWrappedHourly * 2.7f;
            Vector2 cyanOffset = new Vector2((float)Math.Cos(offsetAngle), (float)Math.Sin(offsetAngle)) * offsetMag;
            Vector2 magentaOffset = -cyanOffset;
            Vector2 whiteOffset = new Vector2(-cyanOffset.Y, cyanOffset.X) * 0.65f;
            Color cyanGhost = new Color(110, 255, 255) * ghostOpacity * 0.42f;
            Color magentaGhost = new Color(255, 95, 220) * ghostOpacity * 0.42f;
            Color whiteGhost = new Color(245, 240, 255) * ghostOpacity * 0.28f;
            Main.EntitySpriteDraw(texture, position + cyanOffset, frame, cyanGhost, Projectile.rotation, origin, scale * 1.02f, effects, 0f);
            Main.EntitySpriteDraw(texture, position + magentaOffset, frame, magentaGhost, Projectile.rotation, origin, scale * 1.02f, effects, 0f);
            Main.EntitySpriteDraw(texture, position + whiteOffset, frame, whiteGhost, Projectile.rotation, origin, scale * 1.01f, effects, 0f);
        }
        if (ultReady)
        {
            float auraOpacity = 0.55f * (0.6f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 11.5f) * 0.4f);
            Color aura = new Color(255, 245, 255) * auraOpacity * 0.55f;
            Main.EntitySpriteDraw(texture, position, frame, aura, Projectile.rotation, origin, scale * 1.20f, effects, 0f);
        }
        if (channelBlinkAlpha > 0.01f)
        {
            Color blinkSoft = new Color(255, 250, 255) * channelBlinkAlpha * 0.55f;
            blinkSoft.A = 0;
            Main.EntitySpriteDraw(texture, position, frame, blinkSoft, Projectile.rotation, origin, scale * (1.06f + channelBlinkAlpha * 0.08f), effects, 0f);
            Color blinkSharp = Color.White * channelBlinkAlpha * 0.85f;
            blinkSharp.A = 0;
            Main.EntitySpriteDraw(texture, position, frame, blinkSharp, Projectile.rotation, origin, scale, effects, 0f);
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
        CosmicVoidAxePlayer modPlayer = Main.player[Projectile.owner].GetModPlayer<CosmicVoidAxePlayer>();
        int souls = modPlayer.ShieldTimer > 0 ? modPlayer.ShieldSouls : modPlayer.Souls;
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
    private int GetSwingTime()
    {
        return AttackType == AttackAirSpin ? AirSpinFrames : GroundSlamFrames;
    }
    private int GetTotalTime()
    {
        return GetSwingTime() + RecoveryFrames;
    }
    private float GetStartAngle()
    {
        return AttackType == AttackAirSpin ? -MathHelper.Pi * 1.1f : MathHelper.Pi * GroundSlamStartAnglePi;
    }
    private float GetEndAngle()
    {
        return AttackType == AttackAirSpin ? MathHelper.Pi * 0.4f : MathHelper.Pi * GroundSlamEndAnglePi;
    }
    private float GetSwingPitch()
    {
        return AttackType == AttackAirSpin ? 0.08f : -0.28f;
    }
    private static float GetGroundSlamEase(float progress)
    {
        progress = MathHelper.Clamp(progress, 0f, 1f);
        if (progress < GroundSlamWindupProgress)
            return EaseInOutCubic(progress / GroundSlamWindupProgress) * GroundSlamWindupAngleProgress;
        float slamProgress = (progress - GroundSlamWindupProgress) / (1f - GroundSlamWindupProgress);
        return GroundSlamWindupAngleProgress + EaseInOutCubic(slamProgress) * (1f - GroundSlamWindupAngleProgress);
    }
    private float GetVisualScale()
    {
        if (IsChannel)
            return 1.0f * channelDrawScale;
        float progress = GetSwingProgress();
        float grow = 1f - (float)Math.Pow(1f - progress, 3f);
        float scale = MathHelper.Lerp(0.82f, 1.08f, grow);
        if (AttackType == AttackAirSpin)
            scale *= 0.96f + (float)Math.Sin(progress * MathHelper.Pi) * 0.08f;
        return scale;
    }
    private float GetSwingProgress()
    {
        if (IsChannel)
            return MathHelper.Clamp(Projectile.localAI[0] / (float)Math.Max(effectiveChannelFrames, 1), 0f, 1f);
        return MathHelper.Clamp(Projectile.localAI[0] / GetSwingTime(), 0f, 1f);
    }
    private float GetDrawOpacity()
    {
        if (IsChannel)
            return 1f;
        int swingTime = GetSwingTime();
        if (Projectile.localAI[0] <= swingTime)
            return 1f;
        float fadeProgress = (Projectile.localAI[0] - swingTime) / RecoveryFrames;
        return 1f - MathHelper.Clamp(fadeProgress, 0f, 1f);
    }
    private float GetActiveStart()
    {
        return AttackType == AttackAirSpin ? AxeHitboxAirSpinActiveStart : AxeHitboxGroundSlamActiveStart;
    }
    private static float EaseInOutCubic(float x)
    {
        x = MathHelper.Clamp(x, 0f, 1f);
        return x < 0.5f
            ? 4f * x * x * x
            : 1f - (float)Math.Pow(-2f * x + 2f, 3f) * 0.5f;
    }
    private void SpawnChannelStar(Player player, int count)
    {
        if (Main.myPlayer != Projectile.owner)
            return;
        for (int i = 0; i < count; i++)
        {
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            if (count > 1)
                angle = (i / (float)count) * MathHelper.TwoPi + Main.rand.NextFloat(-0.25f, 0.25f);
            float horizontalDist = Main.rand.NextFloat(95f, 155f);
            float verticalDist = Main.rand.NextFloat(70f, 120f);
            Vector2 spawnPosition = player.MountedCenter + new Vector2((float)Math.Cos(angle) * horizontalDist, (float)Math.Sin(angle) * verticalDist);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPosition, Vector2.Zero, ModContent.ProjectileType<CosmicVoidChannelStarProjectile>(), 0, 0f, Projectile.owner, Projectile.whoAmI, spawnPosition.X, spawnPosition.Y);
        }
    }
    public void ApplyChannelStarImpulse(Vector2 starVelocity, Vector2 hitPosition)
    {
        if (!IsChannel)
            return;
        float tangential = MathHelper.Clamp(starVelocity.X * 0.00018f, -0.006f, 0.006f);
        chargePushVelocity += tangential;
        float intensity = MathHelper.Clamp(starVelocity.Length() / 22f, 0.4f, 1.1f);
        if (intensity > starHitFlashIntensity)
            starHitFlashIntensity = intensity;
        starHitFlashFrames = Math.Max(starHitFlashFrames, 7);
        Player owner = Main.player[Projectile.owner];
        int souls = owner.GetModPlayer<CosmicVoidAxePlayer>().Souls;
        Color flashColor = CosmicVoidAxePlayer.GetSoulSecondaryColor(Math.Max(souls, 1));
        Lighting.AddLight(hitPosition, flashColor.ToVector3() * (1.2f + intensity * 0.4f));
        if (Main.rand.NextBool(3))
        {
            SoundEngine.PlaySound(SoundID.Item25 with
            {
                Volume = 0.10f + intensity * 0.05f, Pitch = 0.55f + Main.rand.NextFloat(-0.1f, 0.1f)
            }, hitPosition);
        }
        for (int i = 0; i < 5; i++)
        {
            Vector2 dustVel = starVelocity * 0.12f + Main.rand.NextVector2Circular(1.6f, 1.6f);
            Dust Spark = Dust.NewDustPerfect(hitPosition, DustID.SpectreStaff, dustVel, 80, flashColor, Main.rand.NextFloat(0.55f, 0.95f));
            Spark.noGravity = true;
            Spark.fadeIn = 0.7f;
        }
    }
    private void TriggerUltReadyFlash(Player player, CosmicVoidAxePlayer modPlayer)
    {
        SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.55f, Pitch = 0.35f }, player.Center);
        SoundEngine.PlaySound(SoundID.Item29 with { Volume = 0.45f, Pitch = 0.35f }, player.Center);
        int souls = (int)MathHelper.Clamp(modPlayer.Souls, 1, CosmicVoidAxePlayer.MaxSouls);
        Color primary = CosmicVoidAxePlayer.GetSoulPrimaryColor(souls);
        SpawnChannelStar(player, 8);
        modPlayer.AddShake(3.5f, 14);
        Lighting.AddLight(player.Center, primary.ToVector3() * 1.4f);
    }
    private float GetSpriteRotation(float worldRotation)
    {
        return worldRotation + GetRotationOffset(GetSpriteEffects());
    }
    private static Vector2 GetHandPosition(Player player, float worldRotation)
    {
        Vector2 hand = player.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, worldRotation - MathHelper.PiOver2);
        hand.X += player.direction * AxeHoldForwardOffset;
        hand.Y += player.gfxOffY + AxeHoldVerticalOffset;
        return hand;
    }
    private Vector2 GetBladeTip(float worldRotation)
    {
        Rectangle frame = GetWorldFrame();
        Vector2 localTip = new Vector2(frame.Width * AxeHitboxTipX, frame.Height * AxeHitboxTipY);
        SpriteEffects effects = GetSpriteEffects();
        Vector2 origin = GetTextureOrigin(frame, effects);
        return TransformTexturePoint(Projectile.Center, localTip, origin, DrawScale * GetVisualScale(), GetSpriteRotation(worldRotation), effects, frame);
    }
    private Vector2 GetBladeGroundContact(float worldRotation)
    {
        Rectangle frame = GetWorldFrame();
        Vector2 hand = Projectile.Center;
        float scale = DrawScale * GetVisualScale();
        float rotation = GetSpriteRotation(worldRotation);
        SpriteEffects effects = GetSpriteEffects();
        Vector2 origin = GetTextureOrigin(frame, effects);
        Vector2[] localPoints = {
            new(frame.Width * AxeGroundContactHandleX, frame.Height * AxeGroundContactHandleY), new(frame.Width * AxeGroundContactInnerTopX, frame.Height * AxeGroundContactInnerTopY), new(frame.Width * AxeGroundContactLowerX, frame.Height * AxeGroundContactLowerY), new(frame.Width * AxeGroundContactTipX, frame.Height * AxeGroundContactTipY), new(frame.Width * AxeGroundContactBottomX, frame.Height * AxeGroundContactBottomY)
        };
        Vector2 contact = TransformTexturePoint(hand, localPoints[0], origin, scale, rotation, effects, frame);
        for (int i = 1; i < localPoints.Length; i++)
        {
            Vector2 point = TransformTexturePoint(hand, localPoints[i], origin, scale, rotation, effects, frame);
            if (point.Y > contact.Y)
                contact = point;
        }
        return contact;
    }
    private (Vector2 Start, Vector2 End)[] GetBladeHitLines(float worldRotation)
    {
        Rectangle frame = GetWorldFrame();
        Vector2 hand = Projectile.Center;
        float scale = DrawScale * GetVisualScale();
        float rotation = GetSpriteRotation(worldRotation);
        SpriteEffects effects = GetSpriteEffects();
        Vector2 origin = GetTextureOrigin(frame, effects);
        Vector2 innerTop = TransformTexturePoint(hand, new Vector2(frame.Width * AxeHitboxInnerTopX, frame.Height * AxeHitboxInnerTopY), origin, scale, rotation, effects, frame);
        Vector2 outer = TransformTexturePoint(hand, new Vector2(frame.Width * AxeHitboxOuterX, frame.Height * AxeHitboxOuterY), origin, scale, rotation, effects, frame);
        Vector2 tip = TransformTexturePoint(hand, new Vector2(frame.Width * AxeHitboxTipX, frame.Height * AxeHitboxTipY), origin, scale, rotation, effects, frame);
        Vector2 lower = TransformTexturePoint(hand, new Vector2(frame.Width * AxeHitboxLowerX, frame.Height * AxeHitboxLowerY), origin, scale, rotation, effects, frame);
        return new[]
        {
            (innerTop, outer), (outer, tip), (tip, lower)
        };
    }
    private static Vector2 GetTextureOrigin(Rectangle frame, SpriteEffects effects)
    {
        float originX = frame.Width * AxeHoldOriginX;
        if ((effects & SpriteEffects.FlipHorizontally) != 0)
            originX = frame.Width * (1f - AxeHoldOriginX);
        return new Vector2(originX, frame.Height * AxeHoldOriginY);
    }
    private static Rectangle GetWorldFrame()
    {
        Texture2D texture = ModContent.Request<Texture2D>("jarkos/textures/Helfeim/CosmicVoidAxe").Value;
        return texture.Frame();
    }
    private SpriteEffects GetSpriteEffects()
    {
        return Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
    }
    private static float GetRotationOffset(SpriteEffects effects)
    {
        return (effects & SpriteEffects.FlipHorizontally) != 0
            ? MathHelper.ToRadians(135f)
            : MathHelper.ToRadians(45f);
    }
    private static Vector2 TransformTexturePoint(Vector2 drawPosition, Vector2 localPoint, Vector2 origin, float scale, float rotation, SpriteEffects effects, Rectangle frame)
    {
        if ((effects & SpriteEffects.FlipHorizontally) != 0)
            localPoint.X = frame.Width - localPoint.X;
        return drawPosition + (localPoint - origin).RotatedBy(rotation) * scale;
    }
}
