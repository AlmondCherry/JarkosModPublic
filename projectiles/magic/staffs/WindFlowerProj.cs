using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using System;
using jarkos.particles;

namespace jarkos.projectiles.magic.staffs
{
    public class WindFlowerProj : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Typhoon;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 60;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = false;
            Projectile.extraUpdates = 1;
        }

        private bool initialized = false;
        private Vector2 baseVelocity;
        private float frequency;
        private float amplitude;
        private int timer = 0;

        public override void AI()
        {
            if (!initialized)
            {
                baseVelocity = Projectile.velocity;
                frequency = Main.rand.NextFloat(0.04f, 0.12f);
                amplitude = Main.rand.NextFloat(2f, 4f) * (Main.rand.NextBool() ? 1f : -1f);
                initialized = true;
            }
            timer++;
            Vector2 perp = new Vector2(-baseVelocity.Y, baseVelocity.X);
            if (perp != Vector2.Zero)
                perp.Normalize();
            float wave = (float)Math.Sin(timer * frequency) * amplitude;
            Projectile.velocity = baseVelocity + perp * wave;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D magicPixel = TextureAssets.MagicPixel.Value;
            for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero || Projectile.oldPos[i + 1] == Vector2.Zero)
                    continue;
                Vector2 startPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 endPos = Projectile.oldPos[i + 1] + Projectile.Size / 2f - Main.screenPosition;
                float progress = (float)i / (Projectile.oldPos.Length - 1);
                float taper = (float)Math.Sin(progress * Math.PI);
                float width = 16f * taper;
                Color color = new Color(160, 255, 200) * taper * 0.9f;
                Vector2 direction = startPos - endPos;
                float length = direction.Length();
                float rotation = direction.ToRotation();
                Main.EntitySpriteDraw(magicPixel, endPos, new Rectangle(0, 0, 1, 1), color, rotation, new Vector2(0f, 0.5f), new Vector2(length, width), SpriteEffects.None, 0);
            }
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                Color particleColor = new Color(160, 255, 200);
                if (Main.rand.NextBool())
                {
                    new StarParticle(target.Center + Main.rand.NextVector2Circular(10f, 10f), vel, particleColor, Main.rand.Next(30, 45), Main.rand.NextFloat(0.05f, 0.15f), 0.8f, Main.rand.NextFloat(-0.2f, 0.2f)).Spawn();
                }
                else
                {
                    new StarParticleBigger(target.Center + Main.rand.NextVector2Circular(15f, 15f), vel * 0.7f, Color.White * 0.8f, Main.rand.Next(25, 40), Main.rand.NextFloat(0.05f, 0.1f), 0.7f, Main.rand.NextFloat(-0.1f, 0.1f)).Spawn();
                }
            }
            for (int j = 0; j < 10; j++)
            {
                Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, DustID.JungleSpore, 0f, 0f, 100, default, 1.2f);
                dust.velocity *= 1.5f;
                dust.noGravity = true;
            }
        }
    }
}
