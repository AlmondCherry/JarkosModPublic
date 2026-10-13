using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using jarkos.particles;
using Terraria.GameContent;

namespace jarkos.npc.Punisher
{
    public class PunisherSlashProj : ModProjectile
    {
        public override string Texture => "jarkos/npc/Punisher/PunisherSlashProj";
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.alpha = 200;
            Projectile.extraUpdates = 1;
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (Main.rand.NextBool(5))
            {
                new Spark(Projectile.Center, Main.rand.NextVector2Circular(1f, 1f), Color.Plum, 15, Main.rand.NextFloat(0.5f, 1f), 1f).Spawn();
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
            Color drawColor = new Color(255, 100, 255, 0) * 0.2f;
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, drawColor, Projectile.rotation, drawOrigin, new Vector2(0.5f, 2f), SpriteEffects.None, 0);
            return false;
        }
    }
}
