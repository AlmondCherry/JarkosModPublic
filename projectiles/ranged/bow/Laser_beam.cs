using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.GameContent;
using jarkos.particles;

namespace jarkos.projectiles.ranged.bow
{
	public class Laser_beam : ModProjectile
	{
        private bool hasSpawned = false;
        private float glowPulse = 0f;
        private float Delay => Projectile.ai[0];
        private float SizeMultiplier => Projectile.ai[1] == 0 ? 1f : Projectile.ai[1];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 28;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 360;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
			Projectile.scale = 0.75f;
            Projectile.light = 0f;
            Projectile.alpha = 255;
            
            Projectile.extraUpdates = 3;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            if (Projectile.ai[0] > 0)
            {
                Projectile.ai[0]--;
                Projectile.alpha = 255;
                Projectile.velocity = Vector2.Zero;
                return;
            }

            if (!hasSpawned)
            {
                SpawnEffects();
                hasSpawned = true;
                if (Projectile.velocity == Vector2.Zero)
                {
                    Projectile.velocity = new Vector2(0, 18f);
                }
            }

            if (Projectile.alpha > 0)
            {
                Projectile.alpha -= 25;
                if (Projectile.alpha < 0) Projectile.alpha = 0;
            }

            glowPulse += 0.15f;
            float pulseIntensity = 0.7f + MathF.Sin(glowPulse) * 0.3f;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Projectile.velocity.Y < 20f)
            {
                Projectile.velocity.Y += 0.18f;
            }

            float lightIntensity = (1f - Projectile.alpha / 255f) * pulseIntensity;
            Lighting.AddLight(Projectile.Center, 0.9f * lightIntensity, 0.15f * lightIntensity, 0.15f * lightIntensity);
            SpawnTrailDust();
        }

        private void SpawnEffects()
        {
            SoundEngine.PlaySound(SoundID.Item60 with
            {
                Pitch = Main.rand.NextFloat(0.2f, 0.6f), Volume = 0.5f, MaxInstances = 5
            }, Projectile.Center);

            for (int i = 0; i < 6; i++)
            {
                Vector2 dustVel = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-3f, 0f));
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, dustVel.X, dustVel.Y, 150, default, 1.3f);
                dust.noGravity = true;
                dust.fadeIn = 1.2f;
            }


            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4.5f, 4.5f);
                Color col = Color.Lerp(new Color(255, 60, 60), new Color(200, 10, 10), Main.rand.NextFloat());
                new Spark(Projectile.Center, vel, col, Main.rand.Next(15, 25), Main.rand.NextFloat(0.35f, 0.6f), 1f, Main.rand.NextFloat(-0.1f, 0.1f)).Spawn();
            }

            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                Color col = Color.Lerp(new Color(255, 80, 80), new Color(220, 20, 20), Main.rand.NextFloat());
                new Solid_particle(Projectile.Center, vel, col, Main.rand.Next(12, 20), Main.rand.NextFloat(0.3f, 0.6f), 1f, Main.rand.NextFloat(-0.1f, 0.1f)).Spawn();
            }
        }

        private void SpawnTrailDust()
        {
            if (Main.rand.NextBool(6))
            {
                Dust blood = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f, 100, default, Main.rand.NextFloat(0.8f, 1.2f));
                blood.noGravity = true;
                blood.velocity *= 0.3f;
            }


            if (Main.rand.NextBool())
            {
                Color col = Color.Lerp(new Color(255, 80, 80), new Color(220, 20, 20), Main.rand.NextFloat());
                Vector2 particleVel = -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(0.8f, 0.8f);
                new Spark(Projectile.Center, particleVel, col, 16, Main.rand.NextFloat(0.3f, 0.55f), 1f, Main.rand.NextFloat(-0.1f, 0.1f)).Spawn();
            }

            if (Main.rand.NextBool(3))
            {
                Color col = Color.Lerp(new Color(255, 50, 50), new Color(200, 20, 20), Main.rand.NextFloat());
                new Solid_particle(Projectile.Center, -Projectile.velocity * 0.05f + Main.rand.NextVector2Circular(0.5f, 0.5f), col, 12, Main.rand.NextFloat(0.3f, 0.5f), 1f, Main.rand.NextFloat(-0.08f, 0.08f)).Spawn();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;

            float scale = Projectile.scale * SizeMultiplier;
            float alphaFactor = 1f - (Projectile.alpha / 255f);

            DrawTrail(texture, origin, scale, alphaFactor);
            DrawMainNeedle(texture, origin, scale, alphaFactor);
            return false;
        }

        private void DrawTrail(Texture2D texture, Vector2 origin, float scale, float alphaFactor)
        {

            for (int i = Projectile.oldPos.Length - 1; i >= 0; i--)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = 1f - (float)i / Projectile.oldPos.Length;
                float trailAlpha = progress * 0.55f * alphaFactor;
                float trailScale = scale * (0.35f + progress * 0.65f);
                Color trailColor = Color.Lerp(new Color(120, 0, 0, 30), new Color(240, 40, 40, 100), progress) * trailAlpha;
                Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Main.EntitySpriteDraw(texture, drawPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0);
            }
        }

        private void DrawMainNeedle(Texture2D texture, Vector2 origin, float scale, float alphaFactor)
        {
            Color needleColor = Color.Lerp(new Color(255, 200, 200, 255), Color.White, 0.5f + MathF.Sin(glowPulse * 2) * 0.2f) * alphaFactor;
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, needleColor, Projectile.rotation, origin, scale * 0.45f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, new Color(255, 255, 255, 0) * 0.3f * alphaFactor, Projectile.rotation, origin, scale * 1.05f, SpriteEffects.None, 0);
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.NPCHit18 with
            {
                Pitch = Main.rand.NextFloat(-0.2f, 0.3f), Volume = 0.7f
            }, Projectile.Center);

            for (int i = 0; i < 4; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(4f, 4f);
                dustVel.Y -= 2f;
                Dust blood = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, dustVel.X, dustVel.Y, 50, default, Main.rand.NextFloat(1.2f, 1.8f));
                blood.noGravity = i % 3 == 0;
            }


            for (int i = 0; i < 12; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                Color col = Color.Lerp(new Color(255, 70, 70), new Color(210, 15, 15), Main.rand.NextFloat());
                new Spark(Projectile.Center, vel, col, Main.rand.Next(15, 28), Main.rand.NextFloat(0.35f, 0.6f), 1f, Main.rand.NextFloat(-0.1f, 0.1f)).Spawn();
            }

            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f) + new Vector2(0f, -2f);
                Color col = Color.Lerp(new Color(255, 40, 40), new Color(180, 0, 0), Main.rand.NextFloat());
                new Spark_gravit(Projectile.Center, vel, col, Main.rand.Next(20, 35), Main.rand.NextFloat(0.35f, 0.6f), 1f).Spawn();
            }

            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Color col = Color.Lerp(new Color(255, 90, 90), new Color(230, 30, 30), Main.rand.NextFloat());
                new Solid_particle(Projectile.Center, vel, col, Main.rand.Next(14, 22), Main.rand.NextFloat(0.35f, 0.6f), 1f, Main.rand.NextFloat(-0.1f, 0.1f)).Spawn();
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(SoundID.NPCHit9 with
            {
                Pitch = -0.2f, Volume = 0.9f
            }, target.Center);


            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5.5f, 5.5f);
                Color col = Color.Lerp(new Color(255, 60, 60), new Color(200, 10, 10), Main.rand.NextFloat());
                new Spark(target.Center, vel, col, Main.rand.Next(15, 28), Main.rand.NextFloat(0.35f, 0.6f), 1f, Main.rand.NextFloat(-0.1f, 0.1f)).Spawn();
            }

            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4.5f, 4.5f) + new Vector2(0f, -1.5f);
                Color col = Color.Lerp(new Color(255, 40, 40), new Color(180, 0, 0), Main.rand.NextFloat());
                new Spark_gravit(target.Center, vel, col, Main.rand.Next(18, 30), Main.rand.NextFloat(0.35f, 0.6f), 1f).Spawn();
            }

            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3.5f, 3.5f);
                Color col = Color.Lerp(new Color(255, 90, 90), new Color(230, 30, 30), Main.rand.NextFloat());
                new Solid_particle(target.Center, vel, col, Main.rand.Next(14, 22), Main.rand.NextFloat(0.35f, 0.6f), 1f, Main.rand.NextFloat(-0.1f, 0.1f)).Spawn();
            }

            if (Main.rand.NextBool(8))
            {
                Player player = Main.player[Projectile.owner];
                int healAmount = Main.rand.Next(1, 4);
                player.Heal(healAmount);
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            SoundEngine.PlaySound(SoundID.Dig with
            {
                Pitch = 0.5f, Volume = 0.4f
            }, Projectile.Center);
            return true;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            float alpha = 1f - (Projectile.alpha / 255f);
            return new Color(255, 220, 220) * alpha;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (Projectile.velocity.Y > 15f)
            {
                modifiers.FinalDamage *= 1.25f;
            }
        }
    }
}
