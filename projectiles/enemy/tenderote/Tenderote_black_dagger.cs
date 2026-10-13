using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.enemy.tenderote
{
    public class Tenderote_black_dagger : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_625";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 36;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 400;
            Projectile.tileCollide = false;
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;
                Projectile.localAI[1] = Projectile.velocity.Length();
            }
            if (Projectile.ai[0] == 1f)
            {
                float time = 400 - Projectile.timeLeft;
                float baseSpeed = Projectile.localAI[1];
                float speedMult = 1.1f + (float)Math.Sin(time * 0.15f) * 0.9f;
                Projectile.velocity = Vector2.Normalize(Projectile.velocity) * (baseSpeed * speedMult);
            }
            else
            {
                if (Projectile.velocity.Length() < 24f)
                {
                    Projectile.velocity *= 1.02f;
                }
            }
            if (Main.rand.NextBool(4))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Wraith);
                dust.noGravity = true;
                dust.scale = 1.2f;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                float alpha = ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Color trailColor = Color.Black * alpha;
                Main.spriteBatch.Draw(texture, drawPos + new Vector2(2, 2), null, Color.White * (alpha * 0.5f), Projectile.oldRot[k], drawOrigin, Projectile.scale, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(texture, drawPos - new Vector2(2, 2), null, Color.White * (alpha * 0.5f), Projectile.oldRot[k], drawOrigin, Projectile.scale, SpriteEffects.None, 0f);
                Main.spriteBatch.Draw(texture, drawPos, null, trailColor, Projectile.oldRot[k], drawOrigin, Projectile.scale, SpriteEffects.None, 0f);
            }
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, Color.Black, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
    }
}
