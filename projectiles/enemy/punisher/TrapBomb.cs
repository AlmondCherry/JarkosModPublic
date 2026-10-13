using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.GameContent;
using Terraria.DataStructures;

namespace jarkos.projectiles.enemy.punisher
{
	public class TrapBomb : ModProjectile
	{
        private ref float FuseTime => ref Projectile.ai[1];
        private float wobbleOffset;
        private float rotationSpeed;
        private float dangerPulse = 0f;
        private const float EXPLOSION_RADIUS = 150f;
        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 28;
            Projectile.hostile = true;
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
            dangerPulse += 0.15f;
            FloatingMovement();
            Projectile.rotation += rotationSpeed;
            CreateAmbientEffects();
            HandleBlinking();
            if (FuseTime < 60)
            {
                CreateDangerEffects();
            }
            if (FuseTime <= 0)
            {
                Explode();
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
            float time = Main.GameUpdateCount * 0.05f + wobbleOffset;
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
        private void CreateDangerEffects()
        {
            float danger = 1f - (FuseTime / 60f);
            Lighting.AddLight(Projectile.Center, 1f * danger, 0.3f * danger, 0.1f * danger);
            if (FuseTime % 20 == 0)
            {
                for (int i = 0; i < 20; i++)
                {
                    float angle = MathHelper.TwoPi * i / 20f;
                    Vector2 pos = Projectile.Center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 20f;
                    Dust ring = Dust.NewDustDirect(pos, 0, 0, DustID.RedTorch, 0f, 0f, 100, default, 1f);
                    ring.noGravity = true;
                    ring.velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 2f;
                }
            }
            if (Main.rand.NextBool((int)(5 - danger * 4)))
            {
                Dust Spark = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Torch, Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f), 100, default, 1.5f);
                Spark.noGravity = true;
            }
            if (FuseTime < 30 && FuseTime % (int)MathF.Max(5, FuseTime / 3) == 0)
            {
                SoundEngine.PlaySound(SoundID.Item11 with { Volume = 0.3f, Pitch = 0.5f + danger * 0.5f }, Projectile.Center);
            }
        }
        private void Explode()
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item62 with { Volume = 0.8f, Pitch = 0.2f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.NPCDeath14 with { Volume = 0.5f }, Projectile.Center);
            foreach (Player player in Main.player)
            {
                if (!player.active || player.dead) continue;
                float distance = Vector2.Distance(player.Center, Projectile.Center);
                if (distance < EXPLOSION_RADIUS)
                {
                    float damageMultiplier = 1f - distance / EXPLOSION_RADIUS * 0.5f;
                    int damage = (int)(Projectile.damage * damageMultiplier);
                    Vector2 knockbackDir = player.Center - Projectile.Center;
                    if (knockbackDir != Vector2.Zero)
                    {
                        knockbackDir.Normalize();
                        player.velocity += knockbackDir * 12f * damageMultiplier;
                    }
                    player.Hurt(PlayerDeathReason.ByProjectile(player.whoAmI, Projectile.whoAmI), damage, knockbackDir.X > 0 ? 1 : -1);
                    player.AddBuff(BuffID.OnFire, 180);
                    player.AddBuff(BuffID.Burning, 60);
                }
            }
            CreateEpicExplosion();
            Projectile.Kill();
        }
        private void CreateEpicExplosion()
        {
            for (int i = 0; i < 60; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(20f, 20f);
                Dust core = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.InfernoFork, velocity.X, velocity.Y, 50, default, Main.rand.NextFloat(2.5f, 4f));
                core.noGravity = true;
            }
            for (int i = 0; i < 80; i++)
            {
                float angle = MathHelper.TwoPi * i / 80f;
                float speed = Main.rand.NextFloat(8f, 18f);
                Vector2 velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * speed;
                Dust fire = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Torch, velocity.X, velocity.Y, 100, default, Main.rand.NextFloat(2f, 3.5f));
                fire.noGravity = true;
            }
            for (int i = 0; i < 50; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(10f, 10f);
                velocity.Y -= 3f;
                Dust smoke = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(30f, 30f), 0, 0, DustID.Smoke, velocity.X, velocity.Y, 180, default, Main.rand.NextFloat(2.5f, 4f));
            }
            for (int i = 0; i < 40; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(15f, 15f);
                Dust Spark = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Torch, velocity.X, velocity.Y, 100, default, Main.rand.NextFloat(0.8f, 1.5f));
                Spark.noGravity = false;
            }
            for (int ring = 0; ring < 5; ring++)
            {
                float radius = EXPLOSION_RADIUS * (0.2f + ring * 0.2f);
                int particleCount = (int)(radius / 4f);
                for (int i = 0; i < particleCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / particleCount;
                    Vector2 pos = Projectile.Center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius;
                    Dust ringDust = Dust.NewDustDirect(pos, 0, 0, ring % 2 == 0 ? DustID.Torch : DustID.InfernoFork, 0f, 0f, 100, default, 2f - ring * 0.3f);
                    ringDust.noGravity = true;
                    ringDust.velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * (5f - ring * 0.8f);
                }
            }
            int rayCount = 12;
            for (int r = 0; r < rayCount; r++)
            {
                float angle = MathHelper.TwoPi * r / rayCount;
                for (int p = 0; p < 15; p++)
                {
                    float dist = p * (EXPLOSION_RADIUS / 15f);
                    Vector2 pos = Projectile.Center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * dist;
                    Dust ray = Dust.NewDustDirect(pos, 0, 0, DustID.Torch, 0f, 0f, 100, default, 2f - p * 0.1f);
                    ray.noGravity = true;
                    ray.velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 8f;
                }
            }
            for (int i = 0; i < 25; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(5f, 12f);
                Vector2 velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * speed;
                velocity.Y -= 5f;
                Dust debris = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Torch, velocity.X, velocity.Y, 100, default, 2f);
                debris.noGravity = false;
            }
            for (int i = 0; i < 20; i++)
            {
                Dust electric = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(40f, 40f), 0, 0, DustID.Electric, Main.rand.NextFloat(-8f, 8f), Main.rand.NextFloat(-8f, 8f), 100, default, 1.2f);
                electric.noGravity = true;
            }
            for (int i = 0; i < 30; i++)
            {
                Dust flash = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.FireworksRGB, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f), 100, new Color(255, 200, 50), 2f);
                flash.noGravity = true;
            }
            Lighting.AddLight(Projectile.Center, 3f, 2f, 0.5f);
            if (Main.netMode != NetmodeID.Server)
            {
                foreach (Player player in Main.player)
                {
                    if (!player.active) continue;
                    float dist = Vector2.Distance(player.Center, Projectile.Center);
                    if (dist < EXPLOSION_RADIUS * 4f)
                    {
                        float intensity = 12f * (1f - dist / (EXPLOSION_RADIUS * 4f));
                        player.GetModPlayer<Screenshake_system>().stronger_shake++;
                    }
                }
            }
            for (int i = 0; i < 5; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(5f, 5f);
                velocity.Y -= 3f;
                Gore gore = Gore.NewGoreDirect(Projectile.GetSource_Death(), Projectile.Center, velocity, Main.rand.Next(61, 64));
                gore.timeLeft = 60;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;
            float pulse = 1f + MathF.Sin(Main.GameUpdateCount * 0.1f + wobbleOffset) * 0.1f;
            if (FuseTime < 60)
            {
                float danger = 1f - (FuseTime / 60f);
                Color dangerGlow = Color.Red * 0.6f * danger;
                dangerGlow.A = 0;
                Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, dangerGlow, -Projectile.rotation, origin, Projectile.scale * (1.5f + danger * 0.5f), SpriteEffects.None, 0f);
            }
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
