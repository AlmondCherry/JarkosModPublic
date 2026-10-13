using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.GameContent;
using jarkos.particles;
namespace jarkos.projectiles.enemy.punisher
{
	public class FakeBomb : ModProjectile
	{
        private ref float BombVariant => ref Projectile.ai[0];
        private ref float FuseTime => ref Projectile.ai[1];
        private float wobbleOffset;
        private float rotationSpeed;
        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 28;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.scale = 1f;
        }
        public override void AI()
        {
            if (Projectile.localAI[0] == 0)
            {
                Initialize();
                Projectile.localAI[0] = 1;
            }
            FuseTime--;
            FloatingMovement();
            Projectile.rotation += rotationSpeed;
            CreateAmbientEffects();
            HandleBlinking();
            if (FuseTime <= 0)
            {
                FakeExplosion();
            }
        }
        private void Initialize()
        {
            wobbleOffset = Main.rand.NextFloat(MathHelper.TwoPi);
            rotationSpeed = Main.rand.NextFloat(0.02f, 0.06f) * (Main.rand.NextBool() ? 1 : -1);
            if (FuseTime <= 0)
            {
                FuseTime = Main.rand.Next(90, 150);
            }
            SoundEngine.PlaySound(SoundID.Item108 with { Volume = 0.5f, Pitch = 0.5f }, Projectile.Center);
        }
        private void FloatingMovement()
        {
            float time = (float)Main.GameUpdateCount * 0.05f + wobbleOffset;
            Projectile.velocity.Y = MathF.Sin(time) * 0.8f;
            Projectile.velocity.X = MathF.Cos(time * 0.7f) * 0.3f;
        }
        private void CreateAmbientEffects()
        {
            if (Main.rand.NextBool(8))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.PurpleTorch, 0f, -0.5f, 150, default, 0.8f);
                dust.noGravity = true;
                dust.velocity *= 0.5f;
            }
            if (Main.rand.NextBool(15))
            {
                Dust Spark = Dust.NewDustDirect(Projectile.Center + new Vector2(0, -Projectile.height / 2).RotatedBy(Projectile.rotation), 0, 0, DustID.Torch, Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-2f, 0f), 100, default, 1f);
                Spark.noGravity = true;
            }
            Lighting.AddLight(Projectile.Center, 0.4f, 0.2f, 0.5f);
        }
        private void HandleBlinking()
        {
            if (FuseTime < 60)
            {
                int blinkRate;
                if (FuseTime < 15) blinkRate = 2;
                else if (FuseTime < 30) blinkRate = 4;
                else if (FuseTime < 45) blinkRate = 6;
                else blinkRate = 8;
                Projectile.alpha = ((int)FuseTime % blinkRate < blinkRate / 2) ? 120 : 0;
                if (FuseTime < 30 && Main.rand.NextBool(4))
                {
                    Dust warning = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.RedTorch, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), 100, default, 1.2f);
                    warning.noGravity = true;
                }
            }
        }
        private void FakeExplosion()
        {
            SoundEngine.PlaySound(SoundID.Item54 with { Volume = 0.6f, Pitch = 0.8f }, Projectile.Center);
            for (int i = 0; i < 25; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(4f, 4f);
                Dust smoke = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Smoke, velocity.X, velocity.Y, 180, default, Main.rand.NextFloat(1.5f, 2.5f));
                smoke.noGravity = true;
            }
            for (int i = 0; i < 10; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(3f, 3f);
                Dust Spark = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Torch, velocity.X, velocity.Y, 100, default, 0.8f);
                Spark.noGravity = true;
                Spark.fadeIn = 0.5f;
            }
            for (int i = 0; i < 15; i++)
            {
                float angle = MathHelper.TwoPi * i / 15f;
                Vector2 velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 3f;
                Dust magic = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.PurpleTorch, velocity.X, velocity.Y, 100, default, 1.2f);
                magic.noGravity = true;
            }
            for (int i = 0; i < 8; i++)
            {
                Vector2 velocity = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-4f, -1f));
                Dust confetti = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.PinkTorch, velocity.X, velocity.Y, 100, default, 0.8f);
                confetti.noGravity = false;
            }
            Lighting.AddLight(Projectile.Center, 0.5f, 0.3f, 0.8f);
            Projectile.Kill();
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture;
            texture = TextureAssets.Projectile[ModContent.ProjectileType<TrapBomb>()].Value;
            Vector2 origin = texture.Size() / 2f;
            float pulse = 1f + MathF.Sin(Main.GameUpdateCount * 0.1f + wobbleOffset) * 0.1f;
            Color glowColor = Color.Purple * 0.4f;
            glowColor.A = 0;
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, glowColor, Projectile.rotation, origin, Projectile.scale * pulse * 1.3f, SpriteEffects.None, 0f);
            Color mainColor = Color.White;
            if (Projectile.alpha > 0)
            {
                mainColor *= 1f - (Projectile.alpha / 255f);
            }
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, mainColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
    }
}
