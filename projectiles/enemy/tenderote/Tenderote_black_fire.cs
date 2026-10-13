using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.enemy.tenderote
{
    public class Tenderote_black_fire : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_1";
        private Effect fireShader;
        public override void SetStaticDefaults()
        {
        }
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }
        public override void AI()
        {
            if (Main.netMode != NetmodeID.Server && fireShader == null)
            {
                fireShader = ModContent.Request<Effect>("jarkos/Shaders/TenderoteBlackFire", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            if (fireShader != null)
            {
                spriteBatch.End();
                fireShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, fireShader, Main.GameViewMatrix.TransformationMatrix);
                Texture2D tex = TextureAssets.MagicPixel.Value;
                Vector2 drawPos = Projectile.Center - Main.screenPosition;
                float alpha = 1f;
                if (Projectile.timeLeft < 30)
                {
                    alpha = Projectile.timeLeft / 30f;
                }
                else if (Projectile.timeLeft > 270)
                {
                    alpha = (300 - Projectile.timeLeft) / 30f;
                }
                spriteBatch.Draw(tex, drawPos, null, Color.White * alpha, Projectile.rotation, new Vector2(tex.Width * 0.5f, tex.Height * 0.8f), new Vector2(60f / tex.Width, 100f / tex.Height), SpriteEffects.None, 0f);
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            }
            return false;
        }
    }
}
