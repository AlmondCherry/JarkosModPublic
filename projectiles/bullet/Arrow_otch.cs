using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.GameContent;
using System.Collections.Generic;
using jarkos.particles;
using ReLogic.Content;

namespace jarkos.projectiles.bullet
{
	public class Arrow_otch : ModProjectile
	{
        private const int MAX_DROPS = 12;
        private const int DROP_INTERVAL = 8;
        private const float MIN_SCALE = 0.3f;
        private int dropsSpawned = 0;
        private int dropTimer = 0;
        private float currentScale = 1f;
        private List<TrailPoint> trailPoints = new List<TrailPoint>();
        private const int TRAIL_LENGTH = 15;

        private struct TrailPoint
        {
            public Vector2 Position;
            public float Rotation;
            public float Scale;
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TRAIL_LENGTH;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = true;
            Projectile.arrow = true;
            Projectile.aiStyle = 0;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            UpdateTrail();
            Projectile.velocity.Y += 0.12f;
            if (Projectile.velocity.Y > 16f)
                Projectile.velocity.Y = 16f;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            currentScale = MathHelper.Lerp(1f, MIN_SCALE, (float)dropsSpawned / MAX_DROPS);
            Projectile.scale = currentScale;
            dropTimer++;
            if (dropTimer >= DROP_INTERVAL && dropsSpawned < MAX_DROPS)
            {
                SpawnLavaDrop();
                dropTimer = 0;
                dropsSpawned++;
                if (dropsSpawned % 3 == 0)
                {
                    Projectile.damage = (int)(Projectile.damage * 0.9f);
                }
            }
            CreateFireEffects();
            UpdateLighting();
            UpdateSounds();
            if (dropsSpawned >= MAX_DROPS)
            {
                TransformToFinalDrop();
            }
        }

        private void UpdateTrail()
        {
            trailPoints.Insert(0, new TrailPoint
            {
                Position = Projectile.Center, Rotation = Projectile.rotation, Scale = currentScale
            });
            while (trailPoints.Count > TRAIL_LENGTH)
            {
                trailPoints.RemoveAt(trailPoints.Count - 1);
            }
        }

        private void SpawnLavaDrop()
        {
            if (Projectile.owner != Main.myPlayer) return;
            Vector2 spawnOffset = -Vector2.Normalize(Projectile.velocity) * 8f;
            Vector2 spawnPos = Projectile.Center + spawnOffset + Main.rand.NextVector2Circular(4f, 4f);
            Vector2 dropVelocity = Projectile.velocity * Main.rand.NextFloat(0.1f, 0.3f);
            dropVelocity += new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-1f, 2f));
            float dropScale = Main.rand.NextFloat(0.6f, 1.2f);
            int dropDamage = (int)(Projectile.damage * Main.rand.NextFloat(0.4f, 0.7f));
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPos, dropVelocity, ModContent.ProjectileType<IgniziumLavaDrop>(), dropDamage, Projectile.knockBack * 0.5f, Projectile.owner, ai0: dropScale);
            CreateDropSpawnEffect(spawnPos);
            if (dropsSpawned % 2 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item45 with
                {
                    Volume = 0.3f, Pitch = Main.rand.NextFloat(0.5f, 1f), MaxInstances = 4
                }, spawnPos);
            }
        }

        private void CreateDropSpawnEffect(Vector2 position)
        {
            for (int i = 0; i < 5; i++)
            {
                Dust Spark = Dust.NewDustPerfect(position, DustID.Torch, Main.rand.NextVector2Circular(3f, 3f), 0, default, Main.rand.NextFloat(1f, 1.8f));
                Spark.noGravity = true;
            }
            for (int i = 0; i < 3; i++)
            {
                Dust lava = Dust.NewDustPerfect(position, DustID.Lava, Main.rand.NextVector2Circular(2f, 2f), 0, default, Main.rand.NextFloat(0.8f, 1.2f));
                lava.noGravity = false;
            }
        }

        private void CreateFireEffects()
        {
            if (Main.rand.NextBool(2))
            {
                Dust fire = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5 * currentScale, 5 * currentScale), DustID.Torch, -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(1f, 1f), 0, default, Main.rand.NextFloat(1.2f, 2f) * currentScale);
                fire.noGravity = true;
                fire.fadeIn = 0.5f;
            }
            if (Main.rand.NextBool(3))
            {
                Dust Spark = Dust.NewDustPerfect(Projectile.Center, DustID.InfernoFork, Projectile.velocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(0.1f, 0.4f), 0, default, Main.rand.NextFloat(0.8f, 1.3f) * currentScale);
                Spark.noGravity = true;
            }
            if (Main.rand.NextBool(4))
            {
                Dust smoke = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(3, 3), DustID.Smoke, new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-1.5f, -0.5f)), 100, Color.DarkGray, Main.rand.NextFloat(0.5f, 1f) * currentScale);
                smoke.noGravity = true;
            }
        }

        private void UpdateLighting()
        {
            float flicker = 0.8f + (float)Math.Sin(Main.GameUpdateCount * 0.4f + Projectile.whoAmI) * 0.2f;
            float intensity = currentScale * flicker;
            Lighting.AddLight(Projectile.Center, 1f * intensity, 0.5f * intensity, 0.1f * intensity);
        }

        private void UpdateSounds()
        {
            if (Main.rand.NextBool(40))
            {
                SoundEngine.PlaySound(SoundID.DD2_BetsyFlameBreath with
                {
                    Volume = 0.2f * currentScale, Pitch = Main.rand.NextFloat(0.5f, 1f), MaxInstances = 2
                }, Projectile.Center);
            }
        }

        private void TransformToFinalDrop()
        {
            if (Projectile.owner == Main.myPlayer)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity * 0.5f, ModContent.ProjectileType<IgniziumLavaDrop>(), Projectile.damage, Projectile.knockBack, Projectile.owner, ai0: 1.5f);
            }
            CreateDissolveEffect();
            SoundEngine.PlaySound(SoundID.Item74 with
            {
                Volume = 0.5f, Pitch = 0.3f
            }, Projectile.Center);
            Projectile.Kill();
        }

        private void CreateDissolveEffect()
        {
            Vector2 pos = Projectile.Center;
            for (int i = 0; i < 20; i++)
            {
                Dust fire = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(10, 10), DustID.Torch, Main.rand.NextVector2Circular(4f, 4f), 0, default, Main.rand.NextFloat(1.5f, 2.5f));
                fire.noGravity = true;
            }
            for (int i = 0; i < 12; i++)
            {
                Dust lava = Dust.NewDustPerfect(pos, DustID.Lava, Main.rand.NextVector2Circular(5f, 5f), 0, default, Main.rand.NextFloat(1.2f, 1.8f));
                lava.noGravity = false;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire, 180);
            if (Main.rand.NextBool(3))
            {
                target.AddBuff(BuffID.OnFire3, 90);
            }
            CreateHitEffects(target.Center);
            SpawnImpactDrops(target.Center);
        }

        private void CreateHitEffects(Vector2 position)
        {
            for (int i = 0; i < 15; i++)
            {
                Dust fire = Dust.NewDustPerfect(position + Main.rand.NextVector2Circular(15, 15), DustID.Torch, Main.rand.NextVector2Circular(5f, 5f), 0, default, Main.rand.NextFloat(1.5f, 2.5f));
                fire.noGravity = true;
            }

            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f) + new Vector2(0f, -3f);
                Color color = Color.Lerp(new Color(255, 110, 10), new Color(255, 225, 30), Main.rand.NextFloat());
                int lifetime = Main.rand.Next(20, 35);
                float scale = Main.rand.NextFloat(0.35f, 0.6f);
                new Spark_gravit(position, vel, color, lifetime, scale, 1f, Main.rand.NextFloat(-0.1f, 0.1f)).Spawn();
            }

            SoundEngine.PlaySound(SoundID.Item74 with
            {
                Volume = 0.7f, Pitch = 0f
            }, position);
        }

        private void SpawnImpactDrops(Vector2 position)
        {
            if (Projectile.owner != Main.myPlayer) return;
            int dropCount = Main.rand.Next(2, 5);
            for (int i = 0; i < dropCount; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                vel.Y = Math.Abs(vel.Y) * -1f - 2f;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), position + Main.rand.NextVector2Circular(10, 10), vel, ModContent.ProjectileType<IgniziumLavaDrop>(), (int)(Projectile.damage * 0.5f), Projectile.knockBack * 0.3f, Projectile.owner, ai0: Main.rand.NextFloat(0.5f, 0.9f));
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            CreateTileImpactEffect();
            return true;
        }

        private void CreateTileImpactEffect()
        {
            Vector2 pos = Projectile.Center;
            for (int i = 0; i < 15; i++)
            {
                float angle = MathHelper.TwoPi * i / 15f + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                vel *= Main.rand.NextFloat(2f, 5f);
                vel.Y = Math.Min(vel.Y, -1f);
                Dust splash = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(5, 5), DustID.Lava, vel, 0, default, Main.rand.NextFloat(1f, 1.8f));
                splash.noGravity = false;
            }
            for (int i = 0; i < 10; i++)
            {
                Dust fire = Dust.NewDustPerfect(pos, DustID.Torch, Main.rand.NextVector2Circular(4f, 4f), 0, default, Main.rand.NextFloat(1.5f, 2.2f));
                fire.noGravity = true;
            }

            for (int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f) + new Vector2(0f, -3f);
                Color color = Color.Lerp(new Color(255, 110, 10), new Color(255, 225, 30), Main.rand.NextFloat());
                int lifetime = Main.rand.Next(20, 35);
                float scale = Main.rand.NextFloat(0.35f, 0.6f);
                new Spark_gravit(pos, vel, color, lifetime, scale, 1f, Main.rand.NextFloat(-0.1f, 0.1f)).Spawn();
            }

            SoundEngine.PlaySound(SoundID.Item45 with
            {
                Volume = 0.6f, Pitch = 0.2f
            }, pos);

            if (Projectile.owner == Main.myPlayer)
            {
                int dropCount = Main.rand.Next(3, 6);
                for (int i = 0; i < dropCount; i++)
                {
                    Vector2 vel = new Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-5f, -1f));
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), pos, vel, ModContent.ProjectileType<IgniziumLavaDrop>(), (int)(Projectile.damage * 0.4f), Projectile.knockBack * 0.3f, Projectile.owner, ai0: Main.rand.NextFloat(0.6f, 1f));
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            Texture2D arrowTexture = TextureAssets.Projectile[Projectile.type].Value;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            DrawFireTrail(sb, pixel);
            DrawArrowGlow(sb, pixel);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            DrawArrow(sb, arrowTexture, lightColor);
            return false;
        }

        private void DrawFireTrail(SpriteBatch sb, Texture2D pixel)
        {
            if (trailPoints.Count < 2) return;
            Rectangle sourceRect = new Rectangle(0, 0, 1, 1);
            for (int i = 0; i < trailPoints.Count - 1; i++)
            {
                float progress = (float)i / trailPoints.Count;
                float alpha = (1f - progress) * 0.7f;
                TrailPoint point = trailPoints[i];
                TrailPoint nextPoint = trailPoints[Math.Min(i + 1, trailPoints.Count - 1)];
                Vector2 start = point.Position - Main.screenPosition;
                Vector2 end = nextPoint.Position - Main.screenPosition;
                Vector2 direction = end - start;
                float length = direction.Length();
                if (length < 0.1f) continue;
                float rotation = direction.ToRotation();
                float pulse = 0.8f + (float)Math.Sin(Main.GameUpdateCount * 0.3f + i * 0.5f) * 0.2f;
                float outerWidth = 20f * point.Scale * (1f - progress) * pulse;
                Color outerColor = new Color(255, 80, 0, 0) * alpha * 0.4f;
                sb.Draw(pixel, start, sourceRect, outerColor, rotation, new Vector2(0f, 0.5f), new Vector2(length + 2, outerWidth), SpriteEffects.None, 0f);
                float midWidth = 12f * point.Scale * (1f - progress) * pulse;
                Color midColor = new Color(255, 150, 30, 0) * alpha * 0.6f;
                sb.Draw(pixel, start, sourceRect, midColor, rotation, new Vector2(0f, 0.5f), new Vector2(length + 2, midWidth), SpriteEffects.None, 0f);
                float coreWidth = 5f * point.Scale * (1f - progress) * pulse;
                Color coreColor = new Color(255, 220, 100, 50) * alpha * 0.8f;
                sb.Draw(pixel, start, sourceRect, coreColor, rotation, new Vector2(0f, 0.5f), new Vector2(length + 2, coreWidth), SpriteEffects.None, 0f);
            }
        }

        private void DrawArrowGlow(SpriteBatch sb, Texture2D pixel)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/particles/Circular_glow", AssetRequestMode.ImmediateLoad).Value;
            Vector2 glowOrigin = glowTex.Size() * 0.5f;

            Color orangeGlow = new Color(255, 100, 15) * (0.65f * currentScale);
            Color brightOrangeGlow = new Color(255, 180, 40) * (0.4f * currentScale);

            sb.Draw(glowTex, drawPos, null, orangeGlow, 0f, glowOrigin, 0.55f * currentScale, SpriteEffects.None, 0f);
            sb.Draw(glowTex, drawPos, null, brightOrangeGlow, 0f, glowOrigin, 0.35f * currentScale, SpriteEffects.None, 0f);
        }

        private void DrawArrow(SpriteBatch sb, Texture2D texture, Color lightColor)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
            Color arrowColor = Color.Lerp(lightColor, new Color(255, 200, 150), 0.5f);
            arrowColor = Color.Lerp(arrowColor, Color.White, 0.3f);
            sb.Draw(texture, drawPos, null, arrowColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, Main.rand.NextVector2Circular(3f, 3f), 0, default, Main.rand.NextFloat(1f, 1.5f));
                dust.noGravity = true;
            }

            Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
        }
    }

	#region Lava Drop
	public class IgniziumLavaDrop : ModProjectile
    {
		public override string Texture => "jarkos/textures/MagicSphereProj";
        private float DropScale
        {
            get => Projectile.ai[0] == 0 ? 1f : Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        private float LifeProgress => (float)Projectile.timeLeft / 240f;
        private List<Vector2> trailPositions = new List<Vector2>();
        private const int MAX_TRAIL_LENGTH = 24;
        private float wobblePhase = 0f;
        private int timeAlive = 0;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = MAX_TRAIL_LENGTH;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.alpha = 0;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            timeAlive++;
            UpdateTrail();
            Projectile.velocity.Y += 0.35f;
            if (Projectile.velocity.Y > 18f)
                Projectile.velocity.Y = 18f;
            Projectile.velocity.X *= 0.99f;
            Projectile.rotation += Projectile.velocity.X * 0.03f;
            wobblePhase += 0.15f;
            CreateFireParticles();
            UpdateLighting();
            if (Projectile.wet)
            {
                HandleWaterInteraction();
            }
        }

        private void UpdateTrail()
        {
            trailPositions.Insert(0, Projectile.Center);
            float speed = Projectile.velocity.Length();

            float timeFactor = MathHelper.Clamp(timeAlive / 12f, 0.15f, 1f);
            int targetLength = (int)MathHelper.Clamp((3 + speed * 1.3f) * timeFactor, 3, MAX_TRAIL_LENGTH);

            while (trailPositions.Count > targetLength)
            {
                trailPositions.RemoveAt(trailPositions.Count - 1);
            }
        }

        private void CreateFireParticles()
        {
            if (Main.rand.NextBool(2))
            {
                Dust fire = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(4 * DropScale, 4 * DropScale), DustID.Torch, -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(0.8f, 0.8f), 0, default, Main.rand.NextFloat(1f, 1.8f) * DropScale);
                fire.noGravity = true;
                fire.fadeIn = 0.5f;
            }
            if (Main.rand.NextBool(5))
            {
                Dust smoke = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(3, 3), DustID.Smoke, new Vector2(0, Main.rand.NextFloat(-1.5f, -0.5f)), 100, Color.DarkGray, Main.rand.NextFloat(0.4f, 0.8f) * DropScale);
                smoke.noGravity = true;
            }
            if (Main.rand.NextBool(8))
            {
                Dust Spark = Dust.NewDustPerfect(Projectile.Center, DustID.Lava, Projectile.velocity.RotatedByRandom(0.6f) * Main.rand.NextFloat(0.2f, 0.5f), 0, default, Main.rand.NextFloat(0.6f, 1f));
                Spark.noGravity = false;
            }
        }

        private void UpdateLighting()
        {
            float flicker = 0.85f + (float)Math.Sin(Main.GameUpdateCount * 0.5f + Projectile.whoAmI * 0.7f) * 0.15f;
            float intensity = DropScale * flicker * LifeProgress;
            Lighting.AddLight(Projectile.Center, 0.9f * intensity, 0.45f * intensity, 0.1f * intensity);
        }

        private void HandleWaterInteraction()
        {
            for (int i = 0; i < 3; i++)
            {
                Dust steam = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8, 8), DustID.Cloud, new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-2.5f, -1f)), 150, Color.White, Main.rand.NextFloat(1f, 2f));
                steam.noGravity = true;
                steam.fadeIn = 1.5f;
            }
            Projectile.velocity *= 0.92f;
            Projectile.timeLeft -= 5;
            if (Main.rand.NextBool(8))
            {
                SoundEngine.PlaySound(SoundID.Item85 with
                {
                    Volume = 0.25f, Pitch = Main.rand.NextFloat(0.7f, 1.2f)
                }, Projectile.Center);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire, 120);
            CreateHitEffects(target.Center);
            SoundEngine.PlaySound(SoundID.Item74 with
            {
                Volume = 0.5f * DropScale, Pitch = Main.rand.NextFloat(-0.1f, 0.3f)
            }, target.Center);
        }

        private void CreateHitEffects(Vector2 position)
        {
            for (int i = 0; i < (int)(12 * DropScale); i++)
            {
                Dust fire = Dust.NewDustPerfect(position + Main.rand.NextVector2Circular(10 * DropScale, 10 * DropScale), DustID.Torch, Main.rand.NextVector2Circular(5f, 5f), 0, default, Main.rand.NextFloat(1.3f, 2.2f) * DropScale);
                fire.noGravity = true;
            }

            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f) + new Vector2(0f, -2.5f);
                Color color = Color.Lerp(new Color(255, 100, 10), new Color(255, 230, 20), Main.rand.NextFloat());
                int lifetime = Main.rand.Next(16, 28);
                float scale = Main.rand.NextFloat(0.25f, 0.45f) * DropScale;
                new Spark_gravit(position, vel, color, lifetime, scale, 1f, Main.rand.NextFloat(-0.1f, 0.1f)).Spawn();
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            CreateSplashEffect();
            return true;
        }

        private void CreateSplashEffect()
        {
            Vector2 pos = Projectile.Center;
            for (int i = 0; i < (int)(10 * DropScale); i++)
            {
                float angle = MathHelper.Pi + Main.rand.NextFloat(-0.8f, 0.8f);
                Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                vel *= Main.rand.NextFloat(2f, 5f) * DropScale;
                Dust splash = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(5, 3), DustID.Lava, vel, 0, default, Main.rand.NextFloat(0.8f, 1.4f) * DropScale);
                splash.noGravity = false;
            }

            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f) + new Vector2(0f, -3f);
                Color color = Color.Lerp(new Color(255, 100, 10), new Color(255, 230, 20), Main.rand.NextFloat());
                int lifetime = Main.rand.Next(16, 28);
                float scale = Main.rand.NextFloat(0.25f, 0.45f) * DropScale;
                new Spark_gravit(pos, vel, color, lifetime, scale, 1f, Main.rand.NextFloat(-0.1f, 0.1f)).Spawn();
            }

            SoundEngine.PlaySound(SoundID.Item45 with
            {
                Volume = 0.4f * DropScale, Pitch = Main.rand.NextFloat(0.3f, 0.7f)
            }, pos);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D pixel = TextureAssets.MagicPixel.Value;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            DrawLavaTrail(sb, pixel);
            DrawLavaDrop(sb, pixel);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }

        private void DrawLavaTrail(SpriteBatch sb, Texture2D pixel)
        {
            List<Vector2> renderPoints = new List<Vector2>(trailPositions.Count + 1);
            renderPoints.Add(Projectile.Center);
            renderPoints.AddRange(trailPositions);

            if (renderPoints.Count < 2) return;

            float spawnScale = MathHelper.Clamp(timeAlive / 10f, 0.2f, 1f);
            float speed = Projectile.velocity.Length();

            for (int i = 0; i < renderPoints.Count - 1; i++)
            {
                float progress = (float)i / (renderPoints.Count - 1);
                float alpha = (1f - progress * 0.75f) * LifeProgress * spawnScale;

                Vector2 start = renderPoints[i];
                Vector2 end = renderPoints[i + 1];

                Color outerColor = Color.Lerp(new Color(255, 75, 0), new Color(255, 235, 20), progress) * (0.6f * alpha);
                Color coreColor  = Color.Lerp(new Color(255, 140, 20), new Color(255, 250, 120), progress) * (0.9f * alpha);

                float baseWidth = MathHelper.Lerp(4f * DropScale, 1f, progress) * (0.6f + speed * 0.05f) * spawnScale;

                DrawSegmentLine(sb, pixel, start, end, baseWidth * 2.2f, outerColor);
                DrawSegmentLine(sb, pixel, start, end, baseWidth * 1.2f, coreColor);
            }
        }

        private void DrawLavaDrop(SpriteBatch sb, Texture2D pixel)
        {
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Texture2D dropTex = ModContent.Request<Texture2D>("jarkos/textures/MagicSphereProj", AssetRequestMode.ImmediateLoad).Value;
            Vector2 origin = dropTex.Size() * 0.5f;

            float time = Main.GameUpdateCount * 0.15f + Projectile.whoAmI;
            float pulse = 0.9f + (float)Math.Sin(time) * 0.1f;
            float wobbleX = 1f + (float)Math.Sin(wobblePhase) * 0.1f;
            float wobbleY = 1f + (float)Math.Sin(wobblePhase + 1.5f) * 0.1f;
            float scale = DropScale * pulse * 0.95f;
            Vector2 drawScale = new Vector2(scale * wobbleX, scale * wobbleY);

            sb.Draw(dropTex, drawPos, null, new Color(255, 80, 0, 0) * (0.5f * LifeProgress), Projectile.rotation, origin, drawScale * 1.8f, SpriteEffects.None, 0f);
            sb.Draw(dropTex, drawPos, null, new Color(255, 150, 20) * (0.85f * LifeProgress), Projectile.rotation, origin, drawScale * 1.25f, SpriteEffects.None, 0f);
            sb.Draw(dropTex, drawPos, null, Color.White * (0.95f * LifeProgress), Projectile.rotation, origin, drawScale * 0.7f, SpriteEffects.None, 0f);
        }

        private static void DrawSegmentLine(SpriteBatch sb, Texture2D pixel, Vector2 start, Vector2 end, float width, Color color)
        {
            Vector2 diff = end - start;
            float length = diff.Length();
            if (length < 0.1f) return;
            float rotation = diff.ToRotation();
            Vector2 origin = new Vector2(0f, 0.5f);
            Vector2 scale = new Vector2(length, width);
            sb.Draw(pixel, start - Main.screenPosition, new Rectangle(0, 0, 1, 1), color, rotation, origin, scale, SpriteEffects.None, 0f);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < (int)(6 * DropScale); i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, Main.rand.NextVector2Circular(2f, 2f), 0, default, Main.rand.NextFloat(0.8f, 1.3f));
                dust.noGravity = true;
            }
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
	}
	#endregion
}
