using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.unique_melee;
public sealed class CosmicVoidRiftShaderProjectile : ModProjectile
{
    private const int Lifetime = 132;
    private const string RiftMaskTexturePath = "textures/Heilfeim/CosmicVoidRiftMask";
    private const string RiftEffectPath = "Shaders/Heilfeim/CosmicRift";
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
        Projectile.timeLeft = Lifetime;
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
        float power = MathHelper.Clamp(Projectile.ai[1], 0.1f, 1f);
        if (Projectile.localAI[0] == 0f)
            Projectile.localAI[1] = Main.rand.NextFloat(10f, 999f);
        Projectile.localAI[0]++;
        if (Main.rand.NextBool(2))
        {
            Color color = CosmicVoidAxePlayer.GetSoulSecondaryColor((int)MathHelper.Clamp(Projectile.ai[0], 1, CosmicVoidAxePlayer.MaxSouls));
            Vector2 dustPosition = Projectile.Center + new Vector2(Main.rand.NextFloat(-170f, 170f), Main.rand.NextFloat(110f, 210f));
            Dust dust = Dust.NewDustPerfect(dustPosition, DustID.PurpleTorch, Main.rand.NextVector2Circular(1.4f, 1.4f), 80, color, Main.rand.NextFloat(0.75f, 1.25f));
            dust.noGravity = true;
        }
    }
    public override bool PreDraw(ref Color lightColor)
    {
        if (Main.dedServ)
            return false;
        int souls = (int)MathHelper.Clamp(Projectile.ai[0], 1, CosmicVoidAxePlayer.MaxSouls);
        float power = MathHelper.Clamp(Projectile.ai[1], 0.1f, 1f);
        float progress = MathHelper.Clamp(Projectile.localAI[0] / Lifetime, 0f, 1f);
        float opacity = MathHelper.Clamp(progress / 0.18f, 0f, 1f) * (1f - MathHelper.Clamp((progress - 0.90f) / 0.10f, 0f, 1f));
        if (opacity <= 0f)
            return false;
        Asset<Texture2D> maskAsset = ModContent.Request<Texture2D>($"{Mod.Name}/{RiftMaskTexturePath}");
        Texture2D maskTexture = maskAsset.Value;
        Vector2 position = Projectile.Center - Main.screenPosition;
        Rectangle frame = maskTexture.Frame();
        Vector2 origin = frame.Size() * 0.5f;
        Vector2 scale = new Vector2(MathHelper.Lerp(460f, 620f, power) / frame.Width, MathHelper.Lerp(220f, 310f, power) / frame.Height);
        Color primary = CosmicVoidAxePlayer.GetSoulPrimaryColor(souls);
        Color secondary = CosmicVoidAxePlayer.GetSoulSecondaryColor(souls);
        Main.EntitySpriteDraw(maskTexture, position, frame, new Color(18, 0, 42) * (0.42f * opacity), 0f, origin, scale * 1.08f, SpriteEffects.None, 0f);
        Main.EntitySpriteDraw(maskTexture, position, frame, secondary * (0.50f * opacity), 0f, origin, scale, SpriteEffects.None, 0f);
        Main.EntitySpriteDraw(maskTexture, position, frame, Color.White * (0.16f * opacity), 0f, origin, scale * 0.88f, SpriteEffects.None, 0f);
        Effect effect;
        try
        {
            effect = ModContent.Request<Effect>($"{Mod.Name}/{RiftEffectPath}").Value;
        }
        catch
        {
            return false;
        }
        effect.Parameters["uColor"]?.SetValue(primary.ToVector3());
        effect.Parameters["uSecondaryColor"]?.SetValue(secondary.ToVector3());
        effect.Parameters["uOpacity"]?.SetValue(opacity);
        effect.Parameters["uProgress"]?.SetValue(progress);
        effect.Parameters["uIntensity"]?.SetValue(power);
        effect.Parameters["uSaturation"]?.SetValue(Projectile.localAI[1]);
        SpriteBatch spriteBatch = Main.spriteBatch;
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        effect.CurrentTechnique.Passes[0].Apply();
        spriteBatch.Draw(maskTexture, position, frame, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        return false;
    }
}
