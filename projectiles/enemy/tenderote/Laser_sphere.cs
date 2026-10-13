using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.enemy.tenderote
{
    public class Laser_sphere : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.ShadowBeamHostile;
        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
        }
        public override void AI()
        {
            Projectile.ai[2]++;
            Player player = Main.player[(int)Projectile.ai[0]];
            Projectile.scale = 1f + (float)Math.Sin(Projectile.ai[2] * 0.2f) * 0.15f;
            if (Projectile.ai[2] < 60)
            {
                if (player.active && !player.dead)
                {
                    Vector2 targetDir = player.Center - Projectile.Center;
                    targetDir.Normalize();
                    Projectile.rotation = targetDir.ToRotation();
                }
            }
            else if (Projectile.ai[2] == 60)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Projectile.rotation.ToRotationVector2(), ModContent.ProjectileType<Tenderote_laser_beam>(), Projectile.damage * 2, 0f, Main.myPlayer, Projectile.whoAmI);
                }
            }
            if (Projectile.ai[2] > 240)
            {
                Projectile.Kill();
            }
        }
        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 20; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.WhiteTorch, Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f));
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[ProjectileID.ShadowBeamHostile].Value;
            Vector2 origin = tex.Size() / 2f;
            if (Projectile.ai[2] < 60)
            {
                Texture2D pixel = TextureAssets.MagicPixel.Value;
                Vector2 lineOrigin = new Vector2(0, 0.5f);
                float alpha = Projectile.ai[2] / 60f;
                Main.spriteBatch.Draw(pixel, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, 1, 1), Color.White * alpha * 0.5f, Projectile.rotation, lineOrigin, new Vector2(2000f, 4f), SpriteEffects.None, 0f);
            }
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White, 0f, origin, Projectile.scale * 1.5f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.Black, 0f, origin, Projectile.scale * 1.2f, SpriteEffects.None, 0f);
            return false;
        }
    }
}
