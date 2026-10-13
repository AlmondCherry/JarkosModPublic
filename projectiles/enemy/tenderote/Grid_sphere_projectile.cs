using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Jarkos.trails;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;

namespace jarkos.projectiles.enemy.tenderote
{
    public class Grid_sphere_projectile : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.ShadowBeamHostile;
        private Primitivedraw TrailDrawer;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (TrailDrawer == null)
            {
                TrailDrawer = new Primitivedraw(WidthFunction, ColorFunction, GameShaders.Misc["Jarkos:LaserShader"]);
                GameShaders.Misc["Jarkos:LaserShader"].SetShaderTexture(TextureAssets.Extra[ExtrasID.MagicMissileTrailErosion]);
            }
            GameShaders.Misc["Jarkos:LaserShader"].UseColor(Color.White);
            GameShaders.Misc["Jarkos:LaserShader"].UseSecondaryColor(Color.Gray);
            GameShaders.Misc["Jarkos:LaserShader"].UseShaderSpecificData(new Vector4(1.5f, 1f, 0, 0));
            TrailDrawer.DrawPrims(Projectile.oldPos, -Main.screenPosition + Projectile.Size / 2f, 15);
            Texture2D tex = TextureAssets.Projectile[ProjectileID.ShadowBeamHostile].Value;
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, tex.Size() / 2f, 1f, SpriteEffects.None, 0f);
            return false;
        }
        private float WidthFunction(float completionRatio)
        {
            return 20f * (1f - completionRatio);
        }
        private Color ColorFunction(float completionRatio)
        {
            return Color.White;
        }
    }
}
