using jarkos.items.weapons.melee.unique_melee;
using jarkos.projectiles.melee.unique_melee;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.systems;
public sealed class CosmicVoidAxePlayer : ModPlayer
{
    public const int MaxSouls = 13;
    private const int ShieldMinFrames = 180;
    private const int ShieldMaxFrames = 540;
    public const int ShieldReflectionTimeCost = 24;
    public const int ShieldActivationFrames = 48;
    public const int ShieldReflectLungeFrames = 18;
    public const int ShieldHitFlashFrames = 22;
    public const int ShieldCooldownPostFrames = 360;
    public const int StarStormCooldownMinFrames = 540;
    public const int StarStormCooldownMaxFrames = 900;
    public const int SoulReturnFrames = 70;
    public const int ShieldCooldownDenialFrames = 24;
    public int ComboStep;
    public int ComboTimer;
    public int Souls;
    public int ShieldTimer;
    public int ShieldMaxTimer;
    public int ShieldSouls;
    public int SoulChannelTimer;
    public int SoulChannelMaxTimer;
    public int StarStormLockTimer;
    public int ShakeTimer;
    public int ShakeMaxTimer;
    public int GroundWaveCooldown;
    public float ShakeIntensity;
    public int ShieldActivationTimer;
    public int ShieldReflectLungeTimer;
    public Vector2 ShieldReflectLungeTarget;
    public int ShieldReflectionsCount;
    public int ShieldHitFlashTimer;
    public int ShieldCooldownTimer;
    public int ShieldCooldownMaxTimer;
    public int SoulsHiddenTimer;
    public int ShieldCooldownDenialTimer;
    private readonly int[] survivingHitCounts = new int[Main.maxNPCs];
    public float SoulPower => Souls / (float)MaxSouls;
    public bool IsShieldOnCooldown => ShieldCooldownTimer > 0;
    public bool SoulsHidden => SoulsHiddenTimer > 0;
    public float ShieldCooldownProgress =>
        ShieldCooldownMaxTimer > 0 ? 1f - ShieldCooldownTimer / (float)ShieldCooldownMaxTimer : 0f;
    public float ShieldCooldownDenialIntensity =>
        ShieldCooldownDenialTimer <= 0 ? 0f : ShieldCooldownDenialTimer / (float)ShieldCooldownDenialFrames;
    public static Color GetSoulPrimaryColor(int souls)
    {
        float power = MathHelper.Clamp(souls / (float)MaxSouls, 0f, 1f);
        return Color.Lerp(new Color(92, 34, 180), new Color(255, 82, 130), power);
    }
    public static Color GetSoulSecondaryColor(int souls)
    {
        float power = MathHelper.Clamp(souls / (float)MaxSouls, 0f, 1f);
        return Color.Lerp(new Color(58, 190, 255), new Color(255, 219, 96), power);
    }
    public void AddSoul()
    {
        AddSoul(Player.Center);
    }
    public void AddSoul(Vector2 sourcePosition)
    {
        if (Souls < MaxSouls)
        {
            Souls++;
            SpawnSoulCollectVisual(sourcePosition);
        }
    }
    public void RegisterAxeHit(NPC target, int damageDone)
    {
        if (target.life <= 0)
        {
            AddSoul(target.Center);
            survivingHitCounts[target.whoAmI] = 0;
            return;
        }
        int hits = survivingHitCounts[target.whoAmI] + 1;
        if (hits >= 3)
        {
            AddSoul(target.Center);
            survivingHitCounts[target.whoAmI] = 0;
            return;
        }
        survivingHitCounts[target.whoAmI] = hits;
    }
    public int ConsumeSouls()
    {
        int souls = Souls;
        if (souls <= 0)
            return 0;
        SpawnSoulAbsorbVisual(souls);
        Souls = 0;
        return souls;
    }
    public void ActivateShield(int souls)
    {
        souls = (int)MathHelper.Clamp(souls, 1, MaxSouls);
        float power = MaxSouls > 1 ? (souls - 1) / (float)(MaxSouls - 1) : 1f;
        ShieldTimer = (int)MathHelper.Lerp(ShieldMinFrames, ShieldMaxFrames, power);
        ShieldMaxTimer = ShieldTimer;
        ShieldSouls = souls;
        ShieldActivationTimer = ShieldActivationFrames;
        ShieldReflectLungeTimer = 0;
        ShieldReflectionsCount = 0;
        BeginUltCooldown(ShieldTimer + ShieldCooldownPostFrames);
    }
    public void BeginUltCooldown(int totalFrames)
    {
        if (totalFrames <= 0)
            return;
        if (totalFrames > ShieldCooldownTimer)
        {
            ShieldCooldownTimer = totalFrames;
            ShieldCooldownMaxTimer = totalFrames;
        }
        int hideFrames = totalFrames + SoulReturnFrames;
        if (hideFrames > SoulsHiddenTimer)
            SoulsHiddenTimer = hideFrames;
    }
    public void TryDenyShieldActivation()
    {
    }
    private void SpawnSoulAbsorbVisual(int soulsConsumed)
    {
        if (Player.whoAmI != Main.myPlayer)
            return;
        Color color = GetSoulSecondaryColor(soulsConsumed);
        Vector2 center = Player.MountedCenter - new Vector2(0f, 8f);
        for (int i = 0; i < 28 + soulsConsumed * 3; i++)
        {
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            float speed = Main.rand.NextFloat(1.4f, 4.8f);
            Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
            Dust dust = Dust.NewDustPerfect(center, DustID.SpectreStaff, velocity, 60, color, Main.rand.NextFloat(0.9f, 1.55f));
            dust.noGravity = true;
        }
        for (int i = 0; i < 10; i++)
        {
            Vector2 velocity = Main.rand.NextVector2Circular(1.4f, 1.4f);
            Dust dust = Dust.NewDustPerfect(center, DustID.GemDiamond, velocity, 30, Color.White, Main.rand.NextFloat(0.85f, 1.2f));
            dust.noGravity = true;
        }
    }
    public void ConsumeShieldTime(int frames)
    {
        if (frames <= 0 || ShieldTimer <= 0)
            return;
        ShieldTimer = Math.Max(0, ShieldTimer - frames);
        ShieldReflectionsCount++;
    }
    public void BeginShieldReflectLunge(Vector2 target)
    {
        ShieldReflectLungeTarget = target;
        ShieldReflectLungeTimer = ShieldReflectLungeFrames;
    }
    public void OnShieldHit()
    {
        ShieldHitFlashTimer = ShieldHitFlashFrames;
    }
    public float ShieldHitFlashIntensity =>
        ShieldHitFlashTimer <= 0 ? 0f : ShieldHitFlashTimer / (float)ShieldHitFlashFrames;
    public void AddShake(float intensity, int duration)
    {
        if (Player.whoAmI != Main.myPlayer)
            return;
        ShakeTimer = Math.Max(ShakeTimer, duration);
        ShakeMaxTimer = Math.Max(ShakeMaxTimer, duration);
        ShakeIntensity = Math.Max(ShakeIntensity, intensity);
    }
    public void StartGroundWaveCooldown(int duration)
    {
        GroundWaveCooldown = Math.Max(GroundWaveCooldown, duration);
    }
    public void LockForStarStorm(int duration)
    {
        StarStormLockTimer = Math.Max(StarStormLockTimer, duration);
    }
    private void SpawnSoulCollectVisual(Vector2 sourcePosition)
    {
        if (Player.whoAmI != Main.myPlayer)
            return;
        int projectileIndex = Projectile.NewProjectile(Player.GetSource_Misc("CosmicVoidSoulCollect"), sourcePosition, Vector2.Zero, ModContent.ProjectileType<CosmicVoidSoulCollectProjectile>(), 0, 0f, Player.whoAmI, Souls);
        if (projectileIndex >= 0 && projectileIndex < Main.maxProjectiles)
            Main.projectile[projectileIndex].netUpdate = true;
        Color color = GetSoulSecondaryColor(Souls);
        for (int i = 0; i < 18; i++)
        {
            Dust dust = Dust.NewDustPerfect(sourcePosition, DustID.SpectreStaff, Main.rand.NextVector2Circular(4.2f, 4.2f), 60, color, Main.rand.NextFloat(1.1f, 1.85f));
            dust.noGravity = true;
        }
    }
    public override void ResetEffects()
    {
        SoulChannelTimer = 0;
        SoulChannelMaxTimer = 0;
        if (ShieldTimer <= 0)
            return;
        float power = MathHelper.Clamp(ShieldSouls / (float)MaxSouls, 0f, 1f);
        Player.moveSpeed += 0.22f + power * 0.34f;
        Player.accRunSpeed += 1.2f + power * 1.6f;
        Player.runAcceleration += 0.025f + power * 0.035f;
        Player.immune = true;
        Player.immuneTime = Math.Max(Player.immuneTime, 2);
    }
    public override bool FreeDodge(Player.HurtInfo info)
    {
        return ShieldTimer > 0;
    }
    public override void ModifyScreenPosition()
    {
        if (Player.whoAmI != Main.myPlayer || ShakeTimer <= 0)
            return;
        float progress = ShakeMaxTimer > 0 ? ShakeTimer / (float)ShakeMaxTimer : 0f;
        float falloff = progress * progress;
        Main.screenPosition += Main.rand.NextVector2Circular(ShakeIntensity, ShakeIntensity * 0.7f) * falloff;
    }
    public override void PostUpdate()
    {
        if (ComboTimer > 0)
        {
            ComboTimer--;
            if (ComboTimer <= 0)
                ComboStep = 0;
        }
        if (StarStormLockTimer > 0)
            StarStormLockTimer--;
        if (Player.whoAmI == Main.myPlayer && Souls > 0
            && Player.HeldItem != null && Player.HeldItem.type == ModContent.ItemType<Helfeim>()
            && Player.ownedProjectileCounts[ModContent.ProjectileType<CosmicVoidSoulAstrolabeProjectile>()] <= 0)
        {
            Projectile.NewProjectile(Player.GetSource_Misc("CosmicVoidSoulAstrolabe"), Player.Center, Vector2.Zero, ModContent.ProjectileType<CosmicVoidSoulAstrolabeProjectile>(), 0, 0f, Player.whoAmI);
        }
        if (ShieldCooldownTimer > 0)
        {
            ShieldCooldownTimer--;
            if (ShieldCooldownTimer == 0 && Player.whoAmI == Main.myPlayer && Souls > 0)
            {
                Projectile.NewProjectile(Player.GetSource_Misc("CosmicVoidSoulReturn"), Player.MountedCenter, Vector2.Zero, ModContent.ProjectileType<CosmicVoidSoulReturnProjectile>(), 0, 0f, Player.whoAmI, Souls);
            }
        }
        if (SoulsHiddenTimer > 0)
            SoulsHiddenTimer--;
        if (ShieldCooldownDenialTimer > 0)
            ShieldCooldownDenialTimer--;
        if (ShieldTimer > 0)
        {
            if (Player.whoAmI == Main.myPlayer && Player.ownedProjectileCounts[ModContent.ProjectileType<CosmicVoidAxeShieldProjectile>()] <= 0)
            {
                Projectile.NewProjectile(Player.GetSource_Misc("CosmicVoidAxeShield"), Player.Center, Vector2.Zero, ModContent.ProjectileType<CosmicVoidAxeShieldProjectile>(), 0, 0f, Player.whoAmI);
            }
            if (Player.whoAmI == Main.myPlayer && Player.ownedProjectileCounts[ModContent.ProjectileType<CosmicVoidShieldFieldProjectile>()] <= 0)
            {
                Projectile.NewProjectile(Player.GetSource_Misc("CosmicVoidShieldField"), Player.Center, Vector2.Zero, ModContent.ProjectileType<CosmicVoidShieldFieldProjectile>(), 0, 0f, Player.whoAmI);
            }
            ShieldTimer--;
            if (ShieldActivationTimer > 0)
                ShieldActivationTimer--;
            if (ShieldReflectLungeTimer > 0)
                ShieldReflectLungeTimer--;
            if (ShieldHitFlashTimer > 0)
                ShieldHitFlashTimer--;
            Color color = GetSoulSecondaryColor(ShieldSouls);
            float castGlow = ShieldActivationTimer > 0
                ? 1f + ShieldActivationTimer / (float)ShieldActivationFrames * 0.6f
                : 1f;
            Lighting.AddLight(Player.Center, color.ToVector3() * 0.65f * castGlow);
            if (Main.rand.NextBool(4))
            {
                Vector2 position = Player.Center + Main.rand.NextVector2Circular(Player.width * 0.8f, Player.height * 0.8f);
                Dust dust = Dust.NewDustPerfect(position, DustID.GemAmethyst, Main.rand.NextVector2Circular(1.4f, 1.4f), 80, color, Main.rand.NextFloat(0.7f, 1.15f));
                dust.noGravity = true;
            }
        }
        else
        {
            ShieldMaxTimer = 0;
            ShieldSouls = 0;
            ShieldActivationTimer = 0;
            ShieldReflectLungeTimer = 0;
            ShieldReflectionsCount = 0;
        }
        if (GroundWaveCooldown > 0)
            GroundWaveCooldown--;
        for (int i = Main.maxNPCs - 1; i >= 0; i--)
        {
            if (survivingHitCounts[i] > 0 && (!Main.npc[i].active || Main.npc[i].life <= 0))
                survivingHitCounts[i] = 0;
        }
        if (ShakeTimer > 0)
            ShakeTimer--;
        else
        {
            ShakeMaxTimer = 0;
            ShakeIntensity = 0f;
        }
    }
}
