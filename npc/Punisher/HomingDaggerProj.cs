using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using jarkos.particles;
using Terraria.GameContent;

namespace jarkos.npc.Punisher
{
    public class HomingDaggerProj : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.MagicDagger;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 400;
            Projectile.alpha = 255;
            Projectile.aiStyle = -1;
        }
        public override void AI()
        {
            if (Projectile.ai[1] == 1f)
            {
                Projectile.alpha += 15;
                Projectile.hostile = false;

                if (Projectile.alpha >= 255)
                    Projectile.Kill();

                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
                return;
            }
            if (Projectile.alpha > 0)
                Projectile.alpha -= 15;

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.ai[0]++;

            if (Projectile.ai[0] < 45)
            {
                Projectile.velocity *= 0.95f;
            }
            else
            {
                Player player = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
                if (player.active && !player.dead)
                {
                    Vector2 targetDir = Vector2.Normalize(player.Center - Projectile.Center);
                    bool missedPlayer = Projectile.ai[0] > 60 && Vector2.Dot(Vector2.Normalize(Projectile.velocity), targetDir) < 0f && Projectile.Distance(player.Center) < 400f;
                    if (missedPlayer || Projectile.ai[0] > 200)
                    {
                        Projectile.ai[1] = 1f;
                    }
                    else
                    {
                        float speed = 10f;
                        Projectile.velocity = (Projectile.velocity * 30f + targetDir * speed) / 31f;
                    }
                }
            }
            if (Main.rand.NextBool(3))
            {
                new Spark(Projectile.Center, Vector2.Zero, Color.Purple, 20, Main.rand.NextFloat(0.5f, 1f), 1f).Spawn();
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);

            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = new Color(255, 50, 150, 0) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                
                Main.EntitySpriteDraw(texture, drawPos, null, color * ((255 - Projectile.alpha) / 255f), Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Color.White * ((255 - Projectile.alpha) / 255f), Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
}
