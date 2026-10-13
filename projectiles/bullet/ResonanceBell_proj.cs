using System;
using jarkos.particles;
using jarkos.projectiles.mecha;
using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.bullet
{
    public class ResonanceBell_proj : ModProjectile
    {
        private const int TotalLifetime = 300;
        private const int RingInterval = 50;
        private float swingAngle = 0f;
        private float swingVelocity = 0f;
        private float maxAmplitude = 0.5f;
        private const float Damping = 0.992f;
        private const float SwingSpeed = 0.08f;
        private int ringTimer = 0;
        private int ringCount = 0;
        private const int MaxRings = 8;
        private float alpha = 1f;
        private bool fadingOut = false;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 50;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = TotalLifetime;
            Projectile.damage = 0;
        }
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            Vector2 targetPos = owner.Center + new Vector2(0, -70);
            Projectile.Center = Vector2.Lerp(Projectile.Center, targetPos, 0.15f);
            float springForce = -SwingSpeed * swingAngle;
            swingVelocity += springForce;
            swingVelocity *= Damping;
            swingAngle += swingVelocity;
            maxAmplitude *= 0.998f;
            swingAngle = MathHelper.Clamp(swingAngle, -maxAmplitude, maxAmplitude);
            Projectile.rotation = swingAngle;
            ringTimer++;
            if (ringCount == 0 && ringTimer >= 5)
            {
                RingBell();
            }
            else if (ringCount > 0 && ringCount < MaxRings)
            {
                if (ringTimer >= RingInterval)
                {
                    RingBell();
                }
            }
            int fadeStartTime = 80;
            if (Projectile.timeLeft <= fadeStartTime)
            {
                fadingOut = true;
                alpha = (float)Projectile.timeLeft / fadeStartTime;
            }
            if (Main.rand.NextBool(3))
            {
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(25, 30);
                Dust dust = Dust.NewDustDirect(dustPos, 2, 2, DustID.GoldCoin, 0, -0.5f, 150, default, 0.8f);
                dust.noGravity = true;
                dust.alpha = (int)(255 * (1f - alpha));
            }
        }
        private void RingBell()
        {
            ringTimer = 0;
            ringCount++;
            float direction = (ringCount % 2 == 0) ? 1f : -1f;
            float impulseStrength = maxAmplitude * 0.8f;
            swingVelocity = direction * impulseStrength;
            float volume = MathHelper.Lerp(1f, 0.15f, (float)(ringCount - 1) / (MaxRings - 1));
            SoundEngine.PlaySound(SoundID.Item35 with
            {
                Volume = volume, Pitch = -0.3f, PitchVariance = 0.05f
            }, Projectile.Center);
            SpawnRingBurst(volume);
            SpawnShockwave(volume);
        }
        private void SpawnRingBurst(float intensity)
        {
            int particleCount = (int)(20 * intensity);
            for (int i = 0; i < particleCount; i++)
            {
                float angle = MathHelper.TwoPi / particleCount * i;
                Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (3f * intensity);
                Dust dust = Dust.NewDustDirect(Projectile.Center, 4, 4, DustID.GoldFlame, velocity.X, velocity.Y, 100, default, 1.5f * intensity);
                dust.noGravity = true;
                dust.fadeIn = 1.2f;
            }
        }
        private void SpawnShockwave(float intensity)
        {
            if (Main.myPlayer != Projectile.owner)
                return;
            Player owner = Main.player[Projectile.owner];
            int damage = (int)(80 * intensity);
            if (damage < 10) damage = 10;
            Projectile.NewProjectile(owner.GetSource_Accessory(owner.armor[0]), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<ResonanceShockwave>(), damage, 4f, Projectile.owner, ai0: intensity);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture_two = ModContent.Request<Texture2D>("jarkos/projectiles/bullet/ResonanceBell_core").Value;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width / 2, 4);
            Color drawColor = lightColor * alpha;
            Color glowColor = new Color(255, 215, 80) * alpha * 0.4f;
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, glowColor, Projectile.rotation, drawOrigin, Projectile.scale * 1.15f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, drawColor, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture_two, Projectile.Center - Main.screenPosition, null, drawColor, Projectile.rotation * 1.9f, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
        public override bool ShouldUpdatePosition()
        {
            return false;
        }
    }
}
