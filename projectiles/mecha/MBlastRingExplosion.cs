using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using System;
namespace Jarkos.projectiles.mecha
{
    public class MBlastRingExplosion : ModProjectile
    {
        public override string Texture => "jarkos/particles/Accurate_circle_particle";
        public override void SetDefaults()
        {
            Projectile.width = 160;
            Projectile.height = 160;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 20;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.DamageType = DamageClass.Ranged;
        }
        public override void AI()
        {
            Projectile.scale += 0.1f;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            float progress = 1f - (Projectile.timeLeft / 20f);
            Texture2D ringTexture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
            for (int i = 0; i < 2; i++)
            {
                float ringProgress = MathHelper.Clamp(progress * 1.5f - i * 0.3f, 0f, 1f);
                if (ringProgress > 0 && ringProgress < 1f)
                {
                    float scale = MathHelper.Lerp(0.1f, 3f, (float)Math.Pow(ringProgress, 0.5f));
                    float alpha = (1f - ringProgress);
                    Color color = i == 0 ? new Color(150, 255, 150) : new Color(50, 200, 100);
                    spriteBatch.Draw(ringTexture, drawPos, null, color * alpha, 0f, ringTexture.Size() / 2f, scale, SpriteEffects.None, 0f);
                }
            }
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);
            return false;
        }
    }
}
