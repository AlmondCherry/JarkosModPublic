using System;
using jarkos.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.npc.Celestia
{
    public class CelestiaStar : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Starfury;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1; 
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.hostile = true; 
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false; 
            Projectile.timeLeft = 300; 
            Projectile.penetrate = -1;
            Projectile.scale = 0.1f;
        }

        public override void AI()
        {
            Projectile.rotation += 0.2f;

            if (Projectile.scale < 1.5f)
            {
                Projectile.scale += 0.02f;
            }

            if (Projectile.timeLeft > 230)
            {
                if (Projectile.velocity.Length() > 0.3f)
                {
                    Projectile.velocity *= 0.94f;
                }
            }
            else if (Projectile.timeLeft < 180)
            {
                if (Projectile.velocity.Length() < 16f)
                {
                    Projectile.velocity *= 1.06f;
                }
            }

            float waveFrequency = 0.06f;
            float waveAmplitude = 0.035f;
            Projectile.velocity = Projectile.velocity.RotatedBy((float)Math.Cos(Projectile.timeLeft * waveFrequency) * waveAmplitude);

            Lighting.AddLight(Projectile.Center, 0.8f * Projectile.scale, 0.4f * Projectile.scale, 1f * Projectile.scale);

            if (Main.rand.NextBool(8)) 
            {
                new StarParticle(Projectile.Center + Main.rand.NextVector2Circular(5, 5), Projectile.velocity * 0.1f, Color.HotPink, 15, 0.2f * Projectile.scale, 0.8f, Main.rand.NextFloat(-0.1f, 0.1f)).Spawn();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Main.EntitySpriteDraw(texture, drawPos, null, lightColor, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }
    }
}
