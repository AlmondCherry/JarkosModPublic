using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.enemy.celestia
{
    public class CelestiaRisingStar : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Starfury;

        
        
        
        

        public ref float WaveAmplitude => ref Projectile.ai[0];
        public ref float WaveFrequency => ref Projectile.ai[1];

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 420;
            Projectile.penetrate = -1;
            Projectile.scale = 1f;
        }

        public override void AI()
        {
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = Main.rand.NextFloat(6f, 11f); 
                Projectile.localAI[1] = Main.rand.NextFloat(0f, MathHelper.TwoPi); 
            }

            
            Projectile.rotation += 0.15f * (WaveAmplitude > 0.04f ? 1.5f : 1f);

            
            float speedY = -Projectile.localAI[0];
            
            
            float phase = (420 - Projectile.timeLeft) * (WaveFrequency > 0f ? WaveFrequency : 0.05f) + Projectile.localAI[1];
            float waveX = MathF.Sin(phase) * WaveAmplitude * 120f;

            Projectile.velocity = new Vector2(waveX, speedY);

            
            Lighting.AddLight(Projectile.Center, 0.7f * Projectile.scale, 0.35f * Projectile.scale, 0.9f * Projectile.scale);

            
            if (Main.rand.NextBool(6))
            {
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    Main.rand.NextBool(2) ? DustID.PurpleTorch : DustID.PinkTorch,
                    -Projectile.velocity * 0.15f,
                    100,
                    default,
                    1.2f * Projectile.scale
                );
                dust.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = texture.Size() * 0.5f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Color starColor = Color.Lerp(Color.White, new Color(255, 180, 240), (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.5f + 0.5f);

            
            Main.EntitySpriteDraw(
                texture,
                drawPos,
                null,
                new Color(180, 80, 255, 0) * 0.5f,
                Projectile.rotation,
                drawOrigin,
                Projectile.scale * 1.35f,
                SpriteEffects.None,
                0
            );

            
            Main.EntitySpriteDraw(
                texture,
                drawPos,
                null,
                starColor,
                Projectile.rotation,
                drawOrigin,
                Projectile.scale,
                SpriteEffects.None,
                0
            );

            return false;
        }
    }
}
