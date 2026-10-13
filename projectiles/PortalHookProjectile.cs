using System;
using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles;
public sealed class PortalHookProjectile : ModProjectile
{
    public const float MaxRange = 580f;
    public const float PullSpeed = 48f;
    public const float ArriveDistance = 14f;
    public const int LaunchTimeoutTicks = 240;
    private enum Phase { Launch = 0, PortalLatched = 1, Arrived = 2, }
    public override string Texture => "Jarkos/textures/PortalHook/PortalHookProjectile";
    public override void SetStaticDefaults()
    {
        Main.projHook[Type] = true;
        ProjectileID.Sets.SingleGrappleHook[Type] = true;
        ProjectileID.Sets.TrailCacheLength[Type] = 12;
        ProjectileID.Sets.TrailingMode[Type] = 0;
    }
    public override bool? CanUseGrapple(Player player)
    {
        int myType = Type;
        for (int i = 0; i < Main.maxProjectiles; i++)
        {
            Projectile p = Main.projectile[i];
            if (p.active && p.owner == player.whoAmI && p.type == myType)
                return false;
        }
        return true;
    }
    public override void NumGrappleHooks(Player player, ref int numHooks)
    {
        numHooks = 1;
    }
    public override void GrapplePullSpeed(Player player, ref float speed)
    {
        speed = PullSpeed;
    }
    public override void GrappleRetreatSpeed(Player player, ref float speed)
    {
        speed = 48f;
    }
    public override void SetDefaults()
    {
        Projectile.width = 18;
        Projectile.height = 18;
        Projectile.aiStyle = -1;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 7200;
        Projectile.tileCollide = true;
        Projectile.netImportant = true;
        Projectile.ignoreWater = true;
    }
    private Phase CurrentPhase
    {
        get => (Phase)(int)Projectile.ai[0];
        set => Projectile.ai[0] = (float)(int)value;
    }
    private int PortalIndexPlusOne
    {
        get => (int)Projectile.ai[1];
        set => Projectile.ai[1] = value;
    }
    private float TravelTimer
    {
        get => Projectile.localAI[0];
        set => Projectile.localAI[0] = value;
    }
    public override bool? CanDamage() => false;
    public override bool ShouldUpdatePosition()
        => CurrentPhase == Phase.Launch;
    public override void OnKill(int timeLeft)
    {
        Projectile portal = TryGetPortal();
        if (portal != null && portal.ModProjectile is PortalProjectile portalMod)
        {
            portalMod.BeginCollapse();
        }
    }
    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        if (CurrentPhase == Phase.Launch)
        {
            int tileX = (int)(Projectile.Center.X / 16f);
            int tileY = (int)(Projectile.Center.Y / 16f);

            if (oldVelocity.X != Projectile.velocity.X)
            {
                if (oldVelocity.X > 0) tileX = (int)((Projectile.position.X + Projectile.width + 2f) / 16f);
                else if (oldVelocity.X < 0) tileX = (int)((Projectile.position.X - 2f) / 16f);
            }
            if (oldVelocity.Y != Projectile.velocity.Y)
            {
                if (oldVelocity.Y > 0) tileY = (int)((Projectile.position.Y + Projectile.height + 2f) / 16f);
                else if (oldVelocity.Y < 0) tileY = (int)((Projectile.position.Y - 2f) / 16f);
            }

            Projectile.Center = new Vector2(tileX * 16f + 8f, tileY * 16f + 8f);
            Projectile.velocity = Vector2.Zero;
            
            CurrentPhase = Phase.PortalLatched;
            PortalIndexPlusOne = 0;

            Projectile.netUpdate = true;
            SoundEngine.PlaySound(SoundID.Item7, Projectile.Center);
        }
        return false;
    }
    public override void AI()
    {
        Player owner = Main.player[Projectile.owner];
        if (!owner.active || owner.dead || owner.outOfRange)
        {
            Projectile.Kill();
            return;
        }
        owner.heldProj = Projectile.whoAmI;
        switch (CurrentPhase)
        {
            case Phase.Launch:
                AILaunch(owner);
                break;
            case Phase.PortalLatched:
                AIPortalLatched(owner);
                break;
            case Phase.Arrived:
                Projectile.Kill();
                break;
        }

        Vector2 toOwner = owner.MountedCenter - Projectile.Center;
        Projectile.rotation = (-toOwner).ToRotation() + MathHelper.PiOver2;
    }
    private void AILaunch(Player owner)
    {
        TravelTimer += 1f;
        float distanceFromOwner = Vector2.Distance(Projectile.Center, owner.MountedCenter);

        bool overRange = distanceFromOwner >= MaxRange;
        bool timedOut = TravelTimer >= LaunchTimeoutTicks;

        if (overRange || timedOut)
        {
            SpawnPortalAndLatch(owner);
        }
        else if (LatchTileOrPlatform())
        {
            SpawnPortalAndLatch(owner);
            if (distanceFromOwner < 40f)
            {
                OnPlayerArrived(owner);
                Projectile.Kill();
                return;
            }
        }
    }
    private void SpawnPortalAndLatch(Player owner)
    {
        Vector2 portalCenter = Projectile.Center + (Projectile.velocity == Vector2.Zero ? Vector2.Zero
                : Vector2.Normalize(Projectile.velocity) * 6f);

        Projectile.velocity = Vector2.Zero;
        Projectile.tileCollide = false;
        Projectile.position = portalCenter - Projectile.Size * 0.5f;

        if (Projectile.owner == Main.myPlayer)
        {
            int portalIndex = Projectile.NewProjectile(Projectile.GetSource_FromAI("portal_hook_open"), portalCenter, Vector2.Zero, ModContent.ProjectileType<PortalProjectile>(), Damage: 0, KnockBack: 0f, Owner: Projectile.owner);
            if (portalIndex >= 0 && portalIndex < Main.maxProjectiles)
            {
                PortalIndexPlusOne = portalIndex + 1;
            }
        }

        CurrentPhase = Phase.PortalLatched;
        Projectile.netUpdate = true;
        SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.6f, Volume = 0.6f }, portalCenter);
    }
    private void AIPortalLatched(Player owner)
    {
        owner.GoingDownWithGrapple = true;
        Vector2 hookCenter = Projectile.Center;
        Vector2 toHook = hookCenter - owner.MountedCenter;

        if (toHook.Y > 0f)
        {
            owner.GoingDownWithGrapple = true;
            owner.releaseDown = true;
            if (owner.velocity.Y >= 0f)
            {
                owner.position.Y += 2f;
            }
        }

        float dist = toHook.Length();

        if (dist < 40f)
        {
            OnPlayerArrived(owner);
            Projectile.Kill();
            return;
        }

        Vector2 dir = toHook / Math.Max(dist, 0.0001f);
        float speed = MathHelper.Lerp(PullSpeed * 0.6f, PullSpeed, MathHelper.Clamp(dist / 240f, 0f, 1f));

        owner.velocity = dir * speed;
        owner.fallStart = (int)(owner.position.Y / 16f);
        owner.ChangeDir(hookCenter.X > owner.Center.X ? 1 : -1);

        Projectile portal = TryGetPortal();
        if (portal != null && Jarkos_config.SomeShadersEnabled)
            PortalScreenSystem.Activate(portal.Center);
    }

    private bool LatchTileOrPlatform()
    {
        Point tileCoords = Projectile.Center.ToTileCoordinates();
        int startX = (int)MathHelper.Clamp(tileCoords.X - 1, 0, Main.maxTilesX - 1);
        int endX = (int)MathHelper.Clamp(tileCoords.X + 1, 0, Main.maxTilesX - 1);
        int startY = (int)MathHelper.Clamp(tileCoords.Y - 1, 0, Main.maxTilesY - 1);
        int endY = (int)MathHelper.Clamp(tileCoords.Y + 1, 0, Main.maxTilesY - 1);

        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                Tile tile = Main.tile[x, y];
                if (tile.HasTile && !tile.IsActuated)
                {
                    bool isSolid = Main.tileSolid[tile.TileType];
                    bool isPlatform = TileID.Sets.Platforms[tile.TileType];

                    if (isSolid || isPlatform)
                    {
                        Rectangle tileRect = new Rectangle(x * 16, y * 16, 16, 16);
                        if (Projectile.Hitbox.Intersects(tileRect))
                        {
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }
    private void OnPlayerArrived(Player owner)
    {
        Projectile portal = TryGetPortal();
        Vector2 target = portal != null ? portal.Center : Projectile.Center;

        owner.Center = target;
        owner.velocity = Vector2.Zero;
        owner.fallStart = (int)(owner.position.Y / 16f);

        if (portal != null && portal.ModProjectile is PortalProjectile portalMod)
        {
            portalMod.BeginCollapse();
        }

        SoundEngine.PlaySound(SoundID.Item104 with { Pitch = -0.2f, Volume = 0.7f }, target);
        CurrentPhase = Phase.Arrived;
        Projectile.netUpdate = true;
    }
    private Projectile TryGetPortal()
    {
        int idxPlus = PortalIndexPlusOne;
        if (idxPlus <= 0)
            return null;

        int idx = idxPlus - 1;
        if (idx < 0 || idx >= Main.maxProjectiles)
            return null;

        Projectile p = Main.projectile[idx];
        int portalType = ModContent.ProjectileType<PortalProjectile>();
        if (!p.active || p.type != portalType)
            return null;
        return p;
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Player owner = Main.player[Projectile.owner];
        DrawChain(owner);

        Asset<Texture2D> textureAsset = ModContent.Request<Texture2D>("Jarkos/textures/PortalHook/PortalHookProjectile", AssetRequestMode.ImmediateLoad);
        Texture2D tex = textureAsset.Value;
        Vector2 origin = tex.Size() * 0.5f;

        Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
        return false;
    }
    private void DrawChain(Player owner)
    {
        Asset<Texture2D> chainAsset = ModContent.Request<Texture2D>("Jarkos/textures/PortalHook/PortalChain", AssetRequestMode.ImmediateLoad);
        Texture2D chain = chainAsset.Value;
        Vector2 playerCenter = owner.MountedCenter;
        Vector2 hookCenter = Projectile.Center;
        Vector2 segment = hookCenter - playerCenter;

        float distance = segment.Length();
        if (distance < 1f)
            return;

        float rotation = segment.ToRotation() - MathHelper.PiOver2;
        Vector2 step = Vector2.Normalize(segment) * chain.Height;
        Vector2 pos = playerCenter;
        int safety = 256;

        while (distance > chain.Height && safety-- > 0)
        {
            Color color = Lighting.GetColor(pos.ToTileCoordinates());
            
            Main.EntitySpriteDraw(chain, pos - Main.screenPosition, null, color, rotation, chain.Size() * 0.5f, 1f, SpriteEffects.None, 0);
            pos += step;
            distance -= chain.Height;
        }
    }
}
