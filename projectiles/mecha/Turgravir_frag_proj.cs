using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.mecha
{
    public class Turgravir_frag_proj : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_1";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }
        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = true;
        }
        public override void AI()
        {
            Projectile.velocity.Y += 0.2f;
            if (Projectile.velocity.Y > 12f)
            {
                Projectile.velocity.Y = 12f;
            }
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (Main.rand.NextBool(5))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.GoldFlame, Vector2.Zero, 100, default, 0.8f);
                dust.noGravity = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            float progress = Projectile.timeLeft / 90f;
            Color baseColor = new Color(255, 200, 50) * progress;
            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                if (Projectile.oldPos[k] != Vector2.Zero)
                {
                    Vector2 drawPos = Projectile.oldPos[k] + Projectile.Size / 2f - Main.screenPosition;
                    float trailProgress = 1f - (k / (float)Projectile.oldPos.Length);
                    Color trailColor = baseColor * trailProgress;
                    float trailScale = 4f * trailProgress;
                    Main.spriteBatch.Draw(pixel, drawPos, new Rectangle(0, 0, 1, 1), trailColor, Projectile.rotation, new Vector2(0.5f, 0.5f), trailScale, SpriteEffects.None, 0f);
                }
            }
            Main.spriteBatch.Draw(pixel, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), Color.White * progress, Projectile.rotation, new Vector2(0.5f, 0.5f), 4f, SpriteEffects.None, 0f);
            return false;
        }
    }
}
