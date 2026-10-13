using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.npc.Punisher
{
    public class DespairSphere : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 500;
        }
        public override void AI()
        {
            Projectile.rotation += 0.05f;
            if (Main.player[Player.FindClosest(Projectile.position, Projectile.width, Projectile.height)].Distance(Projectile.Center) > 3000f)
            {
                Projectile.Kill();
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.MagicPixel.Value;
            Effect sphereEffect = ModContent.Request<Effect>("jarkos/npc/Punisher/Effects/DespairSphere", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            sphereEffect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);

            Texture2D noiseTex = TextureAssets.Extra[ExtrasID.PhantasmalDeathrayBody].Value;
            Main.instance.GraphicsDevice.Textures[1] = noiseTex;
            
            Rectangle rect = new Rectangle((int)(Projectile.Center.X - Main.screenPosition.X - Projectile.width), (int)(Projectile.Center.Y - Main.screenPosition.Y - Projectile.height), Projectile.width * 2, Projectile.height * 2);
            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, sphereEffect, Main.GameViewMatrix.ZoomMatrix);
            Main.spriteBatch.Draw(tex, rect, null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.ZoomMatrix);
            return false;
        }
    }
}
