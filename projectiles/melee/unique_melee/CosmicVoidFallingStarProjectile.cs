using System;
using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.unique_melee;
public sealed class CosmicVoidFallingStarProjectile : ModProjectile
{
    public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Starfury;
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Type] = 14;
        ProjectileID.Sets.TrailingMode[Type] = 2;
    }
    public override void SetDefaults()
    {
        Projectile.width = 22;
        Projectile.height = 22;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Melee;
        Projectile.penetrate = 5;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.timeLeft = 200;
    }
    private const float LaunchPhaseEnd = 22f;
    private const float ArcPhaseEnd = 56f;
    public override void AI()
    {
        Projectile.localAI[0]++;
        float frame = Projectile.localAI[0];
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        if (frame < LaunchPhaseEnd)
        {
            Projectile.velocity.Y = MathHelper.Clamp(Projectile.velocity.Y + 0.20f, -38f, 38f);
            Projectile.velocity.X *= 0.992f;
        }
        else if (frame < ArcPhaseEnd)
        {
            Projectile.velocity.Y = MathHelper.Clamp(Projectile.velocity.Y + 0.34f, -38f, 38f);
            HomeIntoStarStormArea(0.055f);
        }
        else
        {
            Projectile.velocity.Y = MathHelper.Clamp(Projectile.velocity.Y + 0.45f, -42f, 42f);
            HomeIntoStarStormArea(0.16f);
        }
        int souls = (int)Projectile.ai[0];
        Color color = CosmicVoidAxePlayer.GetSoulSecondaryColor(souls);
        Lighting.AddLight(Projectile.Center, color.ToVector3() * 0.9f);
        if (Main.rand.NextBool(2))
        {
            Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), DustID.Enchanted_Pink, -Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedByRandom(0.28f) * Main.rand.NextFloat(0.8f, 2.2f), 60, color, Main.rand.NextFloat(0.8f, 1.25f));
            dust.noGravity = true;
        }
        if (Main.rand.NextBool(3))
        {
            Dust Spark = Dust.NewDustPerfect(Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.UnitY) * Main.rand.NextFloat(10f, 30f), DustID.GemAmethyst, Main.rand.NextVector2Circular(0.7f, 0.7f) - Projectile.velocity * 0.04f, 80, CosmicVoidAxePlayer.GetSoulPrimaryColor(souls), Main.rand.NextFloat(0.55f, 1.05f));
            Spark.noGravity = true;
            Spark.fadeIn = 0.9f;
        }
    }
    private void HomeIntoStarStormArea(float homingStrength)
    {
        int areaIndex = (int)Projectile.ai[1];
        if (areaIndex < 0 || areaIndex >= Main.maxProjectiles)
            return;
        Projectile area = Main.projectile[areaIndex];
        if (!area.active || area.type != ModContent.ProjectileType<CosmicVoidStarStormAreaProjectile>())
            return;
        NPC closest = null;
        float closestDistance = 1100f;
        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC npc = Main.npc[i];
            if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.lifeMax <= 5)
                continue;
            float distanceToArea = Vector2.Distance(npc.Center, area.Center);
            if (distanceToArea > 1240f)
                continue;
            float distance = Vector2.Distance(npc.Center, Projectile.Center);
            if (distance < closestDistance && Collision.CanHitLine(Projectile.Center, 1, 1, npc.Center, 1, 1))
            {
                closest = npc;
                closestDistance = distance;
            }
        }
        if (closest == null)
            return;
        Vector2 desiredVelocity = (closest.Center - Projectile.Center).SafeNormalize(Projectile.velocity.SafeNormalize(Vector2.UnitY)) * MathHelper.Clamp(Projectile.velocity.Length() + 0.42f, 14f, 30f);
        Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, homingStrength);
    }
    public override void OnKill(int timeLeft)
    {
        SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.32f, Pitch = 0.45f }, Projectile.Center);
        SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.18f, Pitch = 0.65f }, Projectile.Center);
        int souls = (int)MathHelper.Clamp(Projectile.ai[0], 1, CosmicVoidAxePlayer.MaxSouls);
        Color color = CosmicVoidAxePlayer.GetSoulSecondaryColor(souls);
        Color primary = CosmicVoidAxePlayer.GetSoulPrimaryColor(souls);
        for (int i = 0; i < 16; i++)
        {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Enchanted_Pink, Main.rand.NextVector2Circular(3.6f, 3.6f), 40, color, Main.rand.NextFloat(0.9f, 1.5f));
            dust.noGravity = true;
        }
        const int ringCount = 18;
        for (int i = 0; i < ringCount; i++)
        {
            float angle = i * MathHelper.TwoPi / ringCount;
            Vector2 dir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
            Vector2 velocity = dir * Main.rand.NextFloat(3.5f, 5.8f);
            Color ringColor = (i % 2 == 0) ? primary : Color.Lerp(color, Color.White, 0.35f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center + dir * 6f, DustID.GemAmethyst, velocity, 30, ringColor, Main.rand.NextFloat(0.95f, 1.45f));
            dust.noGravity = true;
            dust.fadeIn = 1.0f;
        }
        for (int i = 0; i < 6; i++)
        {
            Dust Spark = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch, Main.rand.NextVector2Circular(1.2f, 1.2f), 40, Color.Lerp(color, Color.White, 0.55f), Main.rand.NextFloat(1.0f, 1.45f));
            Spark.noGravity = true;
        }
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        Main.player[Projectile.owner].GetModPlayer<CosmicVoidAxePlayer>().RegisterAxeHit(target, damageDone);
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D texture = TextureAssets.Projectile[ProjectileID.Starfury].Value;
        Rectangle frame = texture.Frame();
        Vector2 origin = frame.Size() * 0.5f;
        int souls = (int)MathHelper.Clamp(Projectile.ai[0], 1, CosmicVoidAxePlayer.MaxSouls);
        Color mainColor = CosmicVoidAxePlayer.GetSoulPrimaryColor(souls);
        for (int i = 0; i < Projectile.oldPos.Length; i++)
        {
            Vector2 oldPosition = Projectile.oldPos[i];
            if (oldPosition == Vector2.Zero)
                continue;
            float progress = 1f - i / (float)Projectile.oldPos.Length;
            float scale = Projectile.scale * MathHelper.Lerp(0.20f, 0.65f, progress);
            Color trailColor = Color.Lerp(mainColor, Color.White, progress * 0.45f) * (progress * 0.28f);
            float trailRotation = Projectile.oldRot[i];
            Main.EntitySpriteDraw(texture, oldPosition + Projectile.Size * 0.5f - Main.screenPosition, frame, trailColor, trailRotation, origin, scale, SpriteEffects.None, 0f);
        }
        Color auraColor = Color.Lerp(mainColor, Color.White, 0.25f) * 0.20f;
        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, auraColor, Projectile.rotation, origin, Projectile.scale * 2.05f, SpriteEffects.None, 0f);
        Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frame, Color.Lerp(mainColor, Color.White, 0.45f), Projectile.rotation, origin, Projectile.scale * 1.18f, SpriteEffects.None, 0f);
        return false;
    }
}
