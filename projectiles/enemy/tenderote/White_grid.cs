using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.enemy.tenderote
{
    public class White_grid : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_1";
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 360;
        }
        public override void AI()
        {
            Projectile.ai[0]++;
            if (Projectile.ai[0] == 30)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int spacing = 300;
                    int rows = 5;
                    int cols = 5;
                    Vector2 center = Projectile.Center;
                    center.X = (float)Math.Round(center.X / spacing) * spacing;
                    center.Y = (float)Math.Round(center.Y / spacing) * spacing;
                    int delay = 0;
                    for (int x = -cols; x <= cols; x++)
                    {
                        for (int y = -rows; y <= rows; y++)
                        {
                            Vector2 pos = center + new Vector2(x * spacing, y * spacing);
                            Projectile.NewProjectile(Projectile.GetSource_FromAI(), pos, Vector2.Zero, ModContent.ProjectileType<Grid_sphere>(), Projectile.damage, 0f, Main.myPlayer, 60 + delay);
                            delay += 2;
                        }
                    }
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            float alpha = 1f;
            if (Projectile.timeLeft < 30) alpha = Projectile.timeLeft / 30f;
            if (Projectile.ai[0] < 30) alpha = Projectile.ai[0] / 30f;
            Color gridColor = Color.White * alpha * 0.3f;
            int spacing = 300;
            Vector2 center = Projectile.Center;
            center.X = (float)Math.Round(center.X / spacing) * spacing;
            center.Y = (float)Math.Round(center.Y / spacing) * spacing;
            for (int x = -10; x <= 10; x++)
            {
                Vector2 pos = center + new Vector2(x * spacing, 0);
                Main.spriteBatch.Draw(pixel, pos - Main.screenPosition, new Rectangle(0, 0, 1, 1), gridColor, 0f, new Vector2(0.5f, 0.5f), new Vector2(2f, 6000f), SpriteEffects.None, 0f);
            }
            for (int y = -10; y <= 10; y++)
            {
                Vector2 pos = center + new Vector2(0, y * spacing);
                Main.spriteBatch.Draw(pixel, pos - Main.screenPosition, new Rectangle(0, 0, 1, 1), gridColor, 0f, new Vector2(0.5f, 0.5f), new Vector2(6000f, 2f), SpriteEffects.None, 0f);
            }
            return false;
        }
    }
}
