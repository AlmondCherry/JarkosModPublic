using System;
using jarkos.items.weapons.melee.unique_melee;
using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.unique_melee;
public sealed class CosmicVoidSoulAstrolabeProjectile : ModProjectile
{
    private static readonly int[] RingSlots = { 5, 4, 4 };
    private static readonly float[] RingRadii = { 38f, 64f, 92f };
    private static readonly float[] RingSquash = { 0.45f, 0.45f, 0.45f };
    private static readonly float[] RingSpinSpeed = { 0.32f, 0.20f, 0.13f };
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
        Projectile.timeLeft = 2;
        Projectile.alpha = 0;
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
        Projectile.timeLeft = 2;
        Projectile.Center = player.MountedCenter;
        Projectile.localAI[0]++;
    }
    public override bool PreDraw(ref Color lightColor)
    {
        Player player = Main.player[Projectile.owner];
        CosmicVoidAxePlayer modPlayer = player.GetModPlayer<CosmicVoidAxePlayer>();
        if (modPlayer.SoulsHidden)
            return false;
        int souls = (int)MathHelper.Clamp(modPlayer.Souls, 0, CosmicVoidAxePlayer.MaxSouls);
        if (souls <= 0)
            return false;
        bool ready = souls >= CosmicVoidAxePlayer.MaxSouls;
        Color primary = CosmicVoidAxePlayer.GetSoulPrimaryColor(souls);
        Color secondary = CosmicVoidAxePlayer.GetSoulSecondaryColor(souls);
        float time = Main.GlobalTimeWrappedHourly;
        float pulse = 0.5f + (float)Math.Sin(time * 3.4f) * 0.5f;
        SpriteBatch spriteBatch = Main.spriteBatch;
        Texture2D markerTexture = TextureAssets.Projectile[ProjectileID.Starfury].Value;
        Rectangle markerFrame = markerTexture.Frame();
        Vector2 markerOrigin = markerFrame.Size() * 0.5f;
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        Vector2 center = player.MountedCenter - Main.screenPosition - new Vector2(0f, 8f);
        int placedSouls = 0;
        for (int ring = 0; ring < RingSlots.Length; ring++)
        {
            int slotsOnRing = RingSlots[ring];
            float radius = RingRadii[ring];
            float squash = RingSquash[ring];
            float direction = (ring % 2 == 0) ? 1f : -1f;
            float spinPhase = time * RingSpinSpeed[ring] * MathHelper.TwoPi * direction + ring * 0.7f;
            for (int slot = 0; slot < slotsOnRing; slot++)
            {
                int globalSoulIndex = placedSouls + slot;
                bool active = globalSoulIndex < souls;
                float angle = spinPhase + slot * MathHelper.TwoPi / slotsOnRing;
                Vector2 offset = new Vector2((float)Math.Cos(angle) * radius, (float)Math.Sin(angle) * radius * squash);
                Vector2 markerPos = center + offset;
                if (!active)
                {
                    continue;
                }
                Color markerColor = (slot + ring) % 2 == 0 ? secondary : Color.Lerp(primary, secondary, 0.5f);
                if (ready)
                    markerColor = Color.Lerp(markerColor, Color.White, pulse * 0.55f);
                float markerScale = 0.20f + (ready ? pulse * 0.06f : 0f);
                float markerRotation = angle + MathHelper.PiOver2;
                spriteBatch.Draw(markerTexture, markerPos, markerFrame, markerColor * 0.35f, markerRotation, markerOrigin, markerScale * 1.85f, SpriteEffects.None, 0f);
                spriteBatch.Draw(markerTexture, markerPos, markerFrame, markerColor, markerRotation, markerOrigin, markerScale, SpriteEffects.None, 0f);
                spriteBatch.Draw(markerTexture, markerPos, markerFrame, Color.White * 0.55f, markerRotation, markerOrigin, markerScale * 0.55f, SpriteEffects.None, 0f);
            }
            placedSouls += slotsOnRing;
        }
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        return false;
    }
}
