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
	public class BombBarrier : ModProjectile
	{
        private Vector2 TrapCenter => new Vector2(Projectile.ai[0], Projectile.ai[1]);
        private enum BarrierState
        {
            Spawning, Active, Dying
        }
        private float spawnProgress = 0f;
        private float orbitAngle = 0f;
        private float pulseTimer = 0f;
        private float glowIntensity = 0f;
        private float electricTimer = 0f;
        private bool spawnSoundPlayed = false;
        private BarrierState currentState = BarrierState.Spawning;
        private Vector2 spawnStartPos;
        private Vector2 targetPos;
        private float initialRotation;
        private const float SPAWN_DURATION = 45f;
        private const float BARRIER_RADIUS = 500f;
        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 36;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 420;
            Projectile.light = 0f;
            Projectile.alpha = 255;
            Projectile.scale = 0f;
        }
        public override void AI()
        {
            if (Projectile.localAI[0] == 0)
            {
                Initialize();
                Projectile.localAI[0] = 1;
            }
            pulseTimer += 0.1f;
            electricTimer++;
            switch (currentState)
            {
                case BarrierState.Spawning:
                    SpawningBehavior();
                    break;
                case BarrierState.Active:
                    ActiveBehavior();
                    break;
                case BarrierState.Dying:
                    DyingBehavior();
                    break;
            }
            Projectile.rotation += 0.05f;
            float lightIntensity = glowIntensity * (0.8f + MathF.Sin(pulseTimer) * 0.2f);
            Lighting.AddLight(Projectile.Center, 1f * lightIntensity, 0.2f * lightIntensity, 0.3f * lightIntensity);
        }
        private void Initialize()
        {
            Vector2 currentOffset = Projectile.Center - TrapCenter;
            float radius = currentOffset.Length();
            orbitAngle = currentOffset.ToRotation();
            targetPos = Projectile.Center;
            spawnStartPos = targetPos + new Vector2(Main.rand.NextFloat(-100f, 100f), -800f - Main.rand.NextFloat(200f));
            Projectile.Center = spawnStartPos;
            initialRotation = Main.rand.NextFloat(MathHelper.TwoPi);
            Projectile.rotation = initialRotation;
            CreateSpawnWarning();
        }
        private void CreateSpawnWarning()
        {
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                Vector2 pos = targetPos + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 30f;
                Dust warning = Dust.NewDustDirect(pos, 0, 0, DustID.RedTorch, 0f, 0f, 100, default, 1.5f);
                warning.noGravity = true;
                warning.velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 2f;
            }
        }
        private void SpawningBehavior()
        {
            spawnProgress += 1f / SPAWN_DURATION;
            if (spawnProgress >= 1f)
            {
                spawnProgress = 1f;
                currentState = BarrierState.Active;
                OnSpawnComplete();
                return;
            }
            float easedProgress = EaseOutBack(spawnProgress);
            Projectile.Center = Vector2.Lerp(spawnStartPos, targetPos, easedProgress);
            Projectile.Center += new Vector2(MathF.Sin(spawnProgress * 20f) * (1f - spawnProgress) * 30f, 0f);
            Projectile.scale = EaseOutElastic(spawnProgress) * 1.2f;
            if (Projectile.scale > 1f && spawnProgress > 0.7f)
            {
                Projectile.scale = MathHelper.Lerp(Projectile.scale, 1f, 0.1f);
            }
            Projectile.alpha = (int)(255 * (1f - spawnProgress));
            glowIntensity = spawnProgress;
            float rotationSpeed = 0.5f * (1f - spawnProgress) + 0.05f;
            Projectile.rotation += rotationSpeed;
            CreateSpawningEffects();
            if (!spawnSoundPlayed && spawnProgress > 0.3f)
            {
                SoundEngine.PlaySound(SoundID.Item93 with { Volume = 0.7f, Pitch = -0.2f }, Projectile.Center);
                spawnSoundPlayed = true;
            }
        }
        private void CreateSpawningEffects()
        {
            if (Main.rand.NextBool(2))
            {
                Dust trail = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Torch, -Projectile.velocity.X * 0.5f, -Projectile.velocity.Y * 0.5f, 100, default, 2f * spawnProgress);
                trail.noGravity = true;
            }
            if (Main.rand.NextBool(3))
            {
                Dust Spark = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.RedTorch, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f), 100, default, 1.5f * spawnProgress);
                Spark.noGravity = true;
            }
            if (Main.rand.NextBool(4))
            {
                Dust electric = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f), 0, 0, DustID.Electric, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), 100, default, 0.8f);
                electric.noGravity = true;
            }
            if (spawnProgress > 0.3f && Main.rand.NextBool(3))
            {
                for (int i = 0; i < 2; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    float dist = Main.rand.NextFloat(10f, 40f);
                    Vector2 pos = Projectile.Center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * dist;
                    Dust magic = Dust.NewDustDirect(pos, 0, 0, DustID.FireworksRGB, 0f, 0f, 100, new Color(255, 50, 50), 1.2f);
                    magic.noGravity = true;
                    magic.velocity = (Projectile.Center - pos) * 0.1f;
                }
            }
            if (spawnProgress < 0.5f && Main.rand.NextBool(2))
            {
                Vector2 linePos = Vector2.Lerp(Projectile.Center, targetPos, Main.rand.NextFloat());
                Dust line = Dust.NewDustDirect(linePos, 0, 0, DustID.RedTorch, 0f, 0f, 150, default, 0.5f);
                line.noGravity = true;
            }
        }
        private void OnSpawnComplete()
        {
            SoundEngine.PlaySound(SoundID.Item94 with { Volume = 0.6f }, Projectile.Center);
            for (int i = 0; i < 30; i++)
            {
                float angle = MathHelper.TwoPi * i / 30f;
                Vector2 velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 5f;
                Dust burst = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Torch, velocity.X, velocity.Y, 100, default, 2f);
                burst.noGravity = true;
            }
            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                Vector2 pos = Projectile.Center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 50f;
                Dust ring = Dust.NewDustDirect(pos, 0, 0, DustID.Electric, 0f, 0f, 100, default, 1.2f);
                ring.noGravity = true;
                ring.velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 3f;
            }
            for (int i = 0; i < 15; i++)
            {
                Dust wave = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Smoke, Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f), 150, default, 1.5f);
            }
            Projectile.scale = 1f;
            Projectile.alpha = 0;
            glowIntensity = 1f;
        }
        private void ActiveBehavior()
        {
            orbitAngle += 0.003f;
            Vector2 currentOffset = Projectile.Center - TrapCenter;
            float radius = currentOffset.Length();
            Projectile.Center = TrapCenter + new Vector2(MathF.Cos(orbitAngle), MathF.Sin(orbitAngle)) * radius;
            float pulse = 1f + MathF.Sin(pulseTimer) * 0.1f;
            Projectile.scale = pulse;
            if (electricTimer % 15 == 0)
            {
                CreateElectricArcs();
            }
            CreateActiveEffects();
            if (Projectile.timeLeft < 60)
            {
                currentState = BarrierState.Dying;
            }
        }
        private void CreateElectricArcs()
        {
            foreach (Projectile other in Main.projectile)
            {
                if (!other.active || other.type != Type || other.whoAmI == Projectile.whoAmI)
                    continue;
                float dist = Vector2.Distance(Projectile.Center, other.Center);
                if (dist < 250f && Main.rand.NextBool(3))
                {
                    CreateLightningBetween(Projectile.Center, other.Center);
                }
            }
        }
        private void CreateLightningBetween(Vector2 start, Vector2 end)
        {
            int segments = 12;
            Vector2 direction = end - start;
            float length = direction.Length();
            direction.Normalize();
            Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
            Vector2 currentPos = start;
            for (int i = 0; i < segments; i++)
            {
                float progress = (float)i / segments;
                Vector2 basePos = start + direction * length * progress;
                float offset = MathF.Sin(progress * MathHelper.Pi) * Main.rand.NextFloat(-20f, 20f);
                Vector2 pos = basePos + perpendicular * offset;
                Dust lightning = Dust.NewDustDirect(pos, 0, 0, DustID.Electric, 0f, 0f, 100, default, 0.6f);
                lightning.noGravity = true;
                lightning.velocity = Vector2.Zero;
                currentPos = pos;
            }
            if (Main.rand.NextBool(5))
            {
                SoundEngine.PlaySound(SoundID.Item93 with { Volume = 0.2f, Pitch = 0.5f }, start);
            }
        }
        private void CreateActiveEffects()
        {
            if (Main.rand.NextBool(5))
            {
                Dust glow = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.RedTorch, 0f, 0f, 150, default, 1f);
                glow.noGravity = true;
                glow.velocity *= 0.3f;
            }
            if (Main.rand.NextBool(8))
            {
                Dust Spark = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(15f, 15f), 0, 0, DustID.Electric, Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f), 100, default, 0.7f);
                Spark.noGravity = true;
            }
        }
        private void DyingBehavior()
        {
            float deathProgress = 1f - (Projectile.timeLeft / 60f);
            Projectile.alpha = (int)(255 * deathProgress * MathF.Abs(MathF.Sin(deathProgress * 30f)));
            Projectile.scale = 1f - deathProgress * 0.5f;
            if (Main.rand.NextBool(2))
            {
                Dust unstable = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Electric, Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f), 100, default, 1f);
                unstable.noGravity = true;
            }
            glowIntensity = 1f - deathProgress;
        }
        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            Vector2 pushDir = TrapCenter - target.Center;
            if (pushDir != Vector2.Zero)
            {
                pushDir.Normalize();
                target.velocity = pushDir * 25f;
            }
            target.AddBuff(BuffID.Electrified, 90);
            for (int i = 0; i < 15; i++)
            {
                Dust hit = Dust.NewDustDirect(target.Center, 0, 0, DustID.Electric, Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f), 100, default, 1.2f);
                hit.noGravity = true;
            }
            SoundEngine.PlaySound(SoundID.Item94 with { Volume = 0.5f }, target.Center);
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item93 with { Volume = 0.4f }, Projectile.Center);
            for (int i = 0; i < 25; i++)
            {
                Dust death = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Electric, Main.rand.NextFloat(-8f, 8f), Main.rand.NextFloat(-8f, 8f), 100, default, 1.5f);
                death.noGravity = true;
            }
            for (int i = 0; i < 15; i++)
            {
                Dust fire = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Torch, Main.rand.NextFloat(-5f, 5f), Main.rand.NextFloat(-5f, 5f), 100, default, 1.5f);
                fire.noGravity = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;
            float glowPulse = 1f + MathF.Sin(pulseTimer * 2f) * 0.2f;
            Color outerGlow = Color.Red * 0.3f * glowIntensity;
            outerGlow.A = 0;
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, outerGlow, Projectile.rotation, origin, Projectile.scale * glowPulse * 2f, SpriteEffects.None, 0f);
            Color middleGlow = Color.OrangeRed * 0.5f * glowIntensity;
            middleGlow.A = 0;
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, middleGlow, -Projectile.rotation * 0.5f, origin, Projectile.scale * glowPulse * 1.5f, SpriteEffects.None, 0f);
            Color innerGlow = Color.Yellow * 0.4f * glowIntensity;
            innerGlow.A = 0;
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, innerGlow, Projectile.rotation * 2f, origin, Projectile.scale * 1.2f, SpriteEffects.None, 0f);
            Color mainColor = Color.White;
            if (Projectile.alpha > 0)
            {
                mainColor *= 1f - (Projectile.alpha / 255f);
            }
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, mainColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
            if (glowIntensity > 0.5f)
            {
                Color highlight = Color.White * 0.3f * glowIntensity;
                highlight.A = 0;
                Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition + new Vector2(-5, -5) * Projectile.scale, null, highlight, Projectile.rotation, origin, Projectile.scale * 0.5f, SpriteEffects.None, 0f);
            }
            return false;
        }
        #region Easing Functions
        private float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * MathF.Pow(t - 1f, 3f) + c1 * MathF.Pow(t - 1f, 2f);
        }
        private float EaseOutElastic(float t)
        {
            const float c4 = (2f * MathF.PI) / 3f;
            if (t == 0) return 0;
            if (t == 1) return 1;
            return MathF.Pow(2f, -10f * t) * MathF.Sin((t * 10f - 0.75f) * c4) + 1f;
        }
        #endregion
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
    }
}
