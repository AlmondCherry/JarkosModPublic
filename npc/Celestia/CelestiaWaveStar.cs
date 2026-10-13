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
    public class CelestiaWaveStar : ModProjectile
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
            Projectile.timeLeft = 400;
            Projectile.penetrate = -1;
            Projectile.scale = 1f;
        }

        public ref float Timer => ref Projectile.localAI[0];

        private float baseSpeed;
        private float baseAngle;
        private bool init;

        public override void AI()
        {
            if (!init)
            {
                init = true;
                baseSpeed = Projectile.velocity.Length();
                if (baseSpeed < 0.01f) baseSpeed = 9f;
                baseAngle = Projectile.velocity.ToRotation();
                Timer = 0f;
            }

            Timer++;

            float waveFrequency = 0.018f;
            float waveAmplitude = MathHelper.ToRadians(55f);
            float phase = Projectile.ai[1];

            float angle = baseAngle + (float)Math.Sin(Timer * waveFrequency + phase) * waveAmplitude;
            Projectile.velocity = angle.ToRotationVector2() * baseSpeed;
            Projectile.rotation += 0.2f;

            Lighting.AddLight(Projectile.Center, 0.8f, 0.4f, 1f);
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
