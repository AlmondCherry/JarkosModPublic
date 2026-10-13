using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using jarkos;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;

namespace Jarkos.projectiles.bullet
{
    public class PhaseBloodExplosion : ModProjectile
    {
        private float maxRadius;
        private float currentRadius;
        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 30;
            Projectile.alpha = 0;
        }
        public override void AI()
        {
            maxRadius = Projectile.ai[0];
            if (maxRadius <= 0) maxRadius = 120f;
            
            float progress = 1f - Projectile.timeLeft / 30f;
            currentRadius = maxRadius * MathSystem.EaseOutCubic(progress);
            Projectile.alpha = (int)(255 * progress);

            if (Projectile.timeLeft % 2 == 0)
            {
                int count = 24;
                for (int i = 0; i < count; i++)
                {
                    float angle = MathHelper.TwoPi / count * i;
                    Vector2 pos = Projectile.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * currentRadius;
                    Dust d = Dust.NewDustDirect(pos, 2, 2, DustID.LifeDrain);

                    d.velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 1f;
                    d.noGravity = true;
                    d.scale = 1.2f * (1f - progress);
                    d.color = Color.Lerp(new Color(255, 50, 50), new Color(255, 200, 100), progress);
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.End();
            Texture2D pixel = TextureAssets.MagicPixel.Value;

            float progress = 1f - Projectile.timeLeft / 30f;
            float alpha = (1f - progress) * 0.4f;
            int segments = 60;
            float ringThickness = 3f * (1f - progress);

            Color ringColor = Color.Lerp(new Color(255, 50, 50), new Color(255, 180, 80), progress) * alpha;
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);

            for (int i = 0; i < segments; i++)
            {
                float angle = MathHelper.TwoPi / segments * i;
                Vector2 pos = Projectile.Center - Main.screenPosition + new Vector2((float)Math.Cos(angle) * currentRadius, (float)Math.Sin(angle) * currentRadius);
                Rectangle rect = new Rectangle((int)pos.X - (int)(ringThickness / 2), (int)pos.Y - (int)(ringThickness / 2), (int)ringThickness + 1, (int)ringThickness + 1);
                Main.spriteBatch.Draw(pixel, rect, ringColor);
            }

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
            return false;
        }
        public override bool? CanDamage() => false;
    }
}
