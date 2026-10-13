using jarkos.items.weapons.summon.whip;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
namespace jarkos.projectiles.summon.whip
{
    public class AncientNatureChargeIndicator : ModProjectile
    {
        private float opacity = 0f;
        private float currentScale = 0.3f;
        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 999999;
            Projectile.alpha = 255;
        }
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            bool stillCharging = owner.active && !owner.dead && owner.channel && owner.HeldItem.type == ModContent.ItemType<Ancient_whip>();
            if (!stillCharging)
            {
                opacity -= 0.08f;
                if (opacity <= 0f)
                {
                    Projectile.Kill();
                    return;
                }
            }
            else
            {
                opacity = MathHelper.Lerp(opacity, 0.7f, 0.06f);
            }
            if (stillCharging)
            {
                Projectile.ai[0] += 1f / Ancient_whip.MAX_CHARGE_TICKS;
                if (Projectile.ai[0] > 1f) Projectile.ai[0] = 1f;
            }

            float progress = MathHelper.Clamp(Projectile.ai[0], 0f, 1f);
            float targetScale = MathHelper.Lerp(Ancient_whip.MIN_RANGE_MULT, Ancient_whip.MAX_RANGE_MULT, progress);
            currentScale = MathHelper.Lerp(currentScale, targetScale, 0.08f);
            Vector2 toMouse = Main.MouseWorld - owner.Center;
            Vector2 direction = toMouse.SafeNormalize(Vector2.UnitX);

            float fixedDist = 60f * currentScale;
            Projectile.Center = owner.Center + direction * fixedDist;
            Projectile.rotation = direction.ToRotation();

            if (progress > 0.3f)
            {
                Lighting.AddLight(Projectile.Center, new Vector3(0.15f, 0.4f, 0.1f) * opacity * progress);
            }
        }
        public override bool ShouldUpdatePosition() => false;
        public override bool PreDraw(ref Color lightColor)
        {
            if (opacity <= 0.01f) return false;
            SpriteBatch sb = Main.spriteBatch;
            float progress = MathHelper.Clamp(Projectile.ai[0], 0f, 1f);
            Texture2D texture = ModContent.Request<Texture2D>("jarkos/textures/CircleLight").Value;
            Vector2 origin = new(texture.Width / 2f, texture.Height / 2f);
            float drawScale = currentScale * 0.25f;
            Color tintColor;
            if (progress < 0.3f)
                tintColor = new Color(100, 180, 80);
            else if (progress < 0.7f)
                tintColor = new Color(150, 210, 70);
            else if (progress < 0.95f)
                tintColor = new Color(120, 255, 80);
            else
                tintColor = new Color(200, 255, 150);
                
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            sb.Draw(texture, Projectile.Center - Main.screenPosition, null, tintColor * opacity, Projectile.rotation + MathHelper.PiOver2, origin, drawScale, SpriteEffects.None, 0f);
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            Color glowColor = tintColor * opacity * 0.35f;
            sb.Draw(texture, Projectile.Center - Main.screenPosition, null, glowColor, Projectile.rotation + MathHelper.PiOver2, origin, drawScale * 1.15f, SpriteEffects.None, 0f);
            if (progress > 0.6f)
            {
                float extraGlow = (progress - 0.6f) / 0.4f;
                sb.Draw(texture, Projectile.Center - Main.screenPosition, null, tintColor * opacity * extraGlow * 0.2f, Projectile.rotation + MathHelper.PiOver2, origin, drawScale * 1.3f, SpriteEffects.None, 0f);
            }
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
	}
}
