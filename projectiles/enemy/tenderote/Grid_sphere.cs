using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.enemy.tenderote
{
    public class Grid_sphere : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.ShadowBeamHostile;
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
        }
        public override void AI()
        {
            Projectile.ai[1]++;
            Projectile.scale = 0.5f + (float)Math.Sin(Projectile.ai[1] * 0.2f) * 0.1f;
            if (Projectile.ai[1] >= Projectile.ai[0])
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    bool diagonal = Main.rand.NextBool();
                    float baseAngle = diagonal ? MathHelper.PiOver4 : 0f;
                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 dir = (baseAngle + i * MathHelper.PiOver2).ToRotationVector2();
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, dir * 8f, ModContent.ProjectileType<Grid_sphere_projectile>(), Projectile.damage, 0f, Main.myPlayer);
                    }
                }
                Projectile.Kill();
            }
        }
        public override void Kill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
            for (int i = 0; i < 10; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.WhiteTorch);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[ProjectileID.ShadowBeamHostile].Value;
            Vector2 origin = tex.Size() / 2f;
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White, 0f, origin, Projectile.scale * 1.5f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.Black, 0f, origin, Projectile.scale * 1.2f, SpriteEffects.None, 0f);
            return false;
        }
    }
}
