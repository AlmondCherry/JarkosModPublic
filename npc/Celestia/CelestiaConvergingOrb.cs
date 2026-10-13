using System;
using jarkos.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.npc.Celestia
{
    public class CelestiaConvergingOrb : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.ShadowBeamHostile;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 360;
            Projectile.penetrate = -1;
            Projectile.scale = 1.5f;
            Projectile.alpha = 255;
        }

        public ref float Timer => ref Projectile.ai[0];
        
        public override void AI()
        {
            if (Timer == 0f)
            {
                Projectile.localAI[0] = Projectile.velocity.X;
                Projectile.localAI[1] = Projectile.velocity.Y;
                Projectile.velocity = Vector2.Zero;
            }

            Timer++;

            if (Timer <= 90)
            {
                if (Projectile.alpha > 0)
                {
                    Projectile.alpha -= 5;
                    if (Projectile.alpha < 0) Projectile.alpha = 0;
                }

                Projectile.rotation += 0.1f;
                Projectile.scale = 1.5f + (float)Math.Sin(Timer * 0.1f) * 0.2f;

                if (Main.rand.NextBool(3))
                {
                    new Circular_glow(Projectile.Center + Main.rand.NextVector2Circular(20, 20), Vector2.Zero, Color.HotPink, 20, 0.5f, 1f, 0f).Spawn();
                    new Spark(Projectile.Center, Main.rand.NextVector2Circular(2, 2), Color.Magenta, 30, 0.4f, 1f, 0f).Spawn();
                }
            }
            else
            {
                if (Timer == 91)
                {
                    Projectile.velocity = new Vector2(Projectile.localAI[0], Projectile.localAI[1]);
                    SoundEngine.PlaySound(SoundID.Item8, Projectile.Center);
                    
                    for (int i = 0; i < 5; i++)
                    {
                        new Circleedge(Projectile.Center, Main.rand.NextVector2Circular(4, 4), Color.HotPink, 30, 0.8f, 1f, 0f).Spawn();
                    }
                }
                
                if (Projectile.velocity.Length() < 14f)
                {
                    Projectile.velocity *= 1.05f;
                }

                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

                Lighting.AddLight(Projectile.Center, 0.8f, 0.2f, 0.8f);
                if (Main.rand.NextBool(2))
                {
                    new Stretching_particle(Projectile.Center, -Projectile.velocity * 0.3f, Color.Magenta, 20, 0.6f, 1f, 0f).Spawn();
                }

                Projectile.localAI[2] += Projectile.velocity.Length();

                if (Projectile.localAI[2] >= 550f || Timer > 350)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        new Circleedge(Projectile.Center, Main.rand.NextVector2Circular(6, 6), Color.Magenta, 20, 1.2f, 1f, 0f).Spawn();
                        new Spark(Projectile.Center, Main.rand.NextVector2Circular(8, 8), Color.HotPink, 30, 0.6f, 1f, 0f).Spawn();
                    }
                    SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
                    Projectile.Kill();
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Color drawColor = Color.Magenta * ((255 - Projectile.alpha) / 255f);
            drawColor.A = 150;

            Main.EntitySpriteDraw(texture, drawPos, null, drawColor, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, drawPos, null, Color.White * ((255 - Projectile.alpha) / 255f) * 0.8f, Projectile.rotation, drawOrigin, Projectile.scale * 0.6f, SpriteEffects.None, 0);

            return false;
        }
    }
}
