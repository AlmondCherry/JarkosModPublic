using System;
using System.Collections.Generic;
using jarkos.particles;
using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.mecha
{
    public class Multiwave_Emitter_beam : ModProjectile
    {
        private const float INITIAL_SCALE = 0.5f;
        private const float MAX_SCALE = 4.0f;
        private const float EXPANSION_RATE = 0.08f;
        private const int MAX_LIFETIME = 120;
        private const float MIN_DAMAGE_MULTIPLIER = 0.15f;
        private List<Vector2> trailPositions = new List<Vector2>();
        private List<float> trailScales = new List<float>();
        private const int MAX_TRAIL_LENGTH = 20;
        private HashSet<int> hitNPCs = new HashSet<int>();

        private float CurrentScale => INITIAL_SCALE + (MAX_SCALE - INITIAL_SCALE) * (Projectile.ai[1] / MAX_LIFETIME);
        private float DamageMultiplier => MathHelper.Lerp(1f, MIN_DAMAGE_MULTIPLIER, Projectile.ai[1] / MAX_LIFETIME);

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 100;
            Projectile.height = 100;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = MAX_LIFETIME;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.light = 1.5f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.ai[1]++;
            float scale = CurrentScale;
            Projectile.scale = scale;
            int baseSize = 100;
            Projectile.width = (int)(baseSize * scale);
            Projectile.height = (int)(baseSize * scale);
            Projectile.Center = Projectile.Center;
            Projectile.velocity *= 0.985f;

            UpdateTrail();
            SpawnParticles();

            if (Projectile.ai[1] % 15 == 0)
            {
                SoundEngine.PlaySound(SoundID.DD2_BetsyFlameBreath with
                {
                    Volume = 0.3f * (1f - Projectile.ai[1] / MAX_LIFETIME),
                    Pitch = -0.5f
                }, Projectile.Center);
            }

            float lightIntensity = 1.5f * (1f - Projectile.ai[1] / MAX_LIFETIME * 0.5f);
            Lighting.AddLight(Projectile.Center, new Vector3(1f, 0.7f, 0.2f) * lightIntensity * scale);

            Projectile.alpha = (int)(255 * (Projectile.ai[1] / MAX_LIFETIME) * 0.7f);
            Projectile.rotation += 0.05f * (1f - Projectile.ai[1] / MAX_LIFETIME * 0.5f);

            if (Projectile.owner == Main.myPlayer && Main.netMode != NetmodeID.Server)
            {
                float auraRadius = scale * 105f;
                if (MultiwaveShaderSystem.IsOnScreen(Projectile.Center, auraRadius + 150f))
                {
                    MultiwaveShaderSystem.Activate(Projectile.Center);
                    float lifeProgress = Projectile.ai[1] / MAX_LIFETIME;
                    float distortionOpacity = 1f;
                    if (lifeProgress > 0.8f)
                    {
                        distortionOpacity = (1f - lifeProgress) / 0.2f;
                    }
                    MultiwaveShaderSystem.Update(Projectile.Center, auraRadius, distortionOpacity, Main.GlobalTimeWrappedHourly * 2.2f, new Color(255, 110, 15));
                }
                else
                {
                    MultiwaveShaderSystem.Deactivate();
                }
            }
        }

        private void UpdateTrail()
        {
            trailPositions.Insert(0, Projectile.Center);
            trailScales.Insert(0, CurrentScale);
            while (trailPositions.Count > MAX_TRAIL_LENGTH)
            {
                trailPositions.RemoveAt(trailPositions.Count - 1);
                trailScales.RemoveAt(trailScales.Count - 1);
            }
        }

        private void SpawnParticles()
        {
            float scale = CurrentScale;
            if (Main.rand.NextBool(2))
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(3f, 3f) * scale;
                new Spark(Projectile.Center, sparkVel, new Color(255, 180, 50), Main.rand.Next(15, 30), Main.rand.NextFloat(0.3f, 0.7f), 1f).Spawn();
            }
        }

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            float scale = CurrentScale;
            int size = (int)(100 * scale);
            hitbox = new Rectangle((int)Projectile.Center.X - size / 2, (int)Projectile.Center.Y - size / 2, size, size);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.FinalDamage *= DamageMultiplier;
            float distanceFromCenter = Vector2.Distance(target.Center, Projectile.Center);
            float maxDistance = Projectile.width / 2f;
            if (distanceFromCenter < maxDistance * 0.3f)
            {
                modifiers.FinalDamage *= 1.5f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 120);
            CreateHitEffect(target.Center);
            SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with
            {
                Volume = 0.4f,
                Pitch = 0.5f
            }, target.Center);
        }

        private void CreateHitEffect(Vector2 position)
        {

            for (int i = 0; i < 4; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(8f, 8f);
                Dust dust = Dust.NewDustPerfect(position, DustID.SolarFlare, velocity, 0, new Color(255, 200, 50), Main.rand.NextFloat(1.5f, 2.5f));
                dust.noGravity = true;
            }
            for (int i = 0; i < 3; i++)
            {
                float angle = MathHelper.TwoPi * i / 3f;
                Vector2 velocity = angle.ToRotationVector2() * 6f;
                Dust dust = Dust.NewDustPerfect(position, DustID.GoldFlame, velocity, 0, Color.White, 1.5f);
                dust.noGravity = true;
            }


            for (int i = 0; i < 8; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                new Spark(position, vel, new Color(255, 180, 40), Main.rand.Next(15, 30), Main.rand.NextFloat(0.4f, 0.8f), 1f).Spawn();
            }
            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(5f, 5f);
                new MagmaParticle(position, vel, new Color(255, 140, 20), Main.rand.Next(20, 35), Main.rand.NextFloat(0.4f, 0.7f), 1f).Spawn();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            float alpha = 1f - Projectile.alpha / 255f;
            float scale = CurrentScale;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;

            Color outerGlow = new Color(255, 80, 15) * 0.35f * alpha;
            spriteBatch.Draw(texture, drawPos, null, outerGlow, Projectile.rotation * 0.5f, origin, scale * 2.2f, SpriteEffects.None, 0f);
            spriteBatch.End();


            if (MultiwaveShaderSystem.CoreShader != null)
            {
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Texture2D noiseTex = ModContent.Request<Texture2D>("jarkos/textures/Noises/EnergyNoise", AssetRequestMode.ImmediateLoad).Value;
                Vector2 noiseOrigin = noiseTex.Size() / 2f;

                float drawScale = (200f * scale) / noiseTex.Width;

                MultiwaveShaderSystem.CoreShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 1.5f);
                MultiwaveShaderSystem.CoreShader.Parameters["uColor"]?.SetValue(new Vector3(1.0f, 0.72f, 0.18f));
                MultiwaveShaderSystem.CoreShader.Parameters["uSecondaryColor"]?.SetValue(new Vector3(0.95f, 0.12f, 0.02f));
                MultiwaveShaderSystem.CoreShader.Parameters["uOpacity"]?.SetValue(alpha);
                MultiwaveShaderSystem.CoreShader.CurrentTechnique.Passes[0].Apply();

                spriteBatch.Draw(noiseTex, drawPos, null, Color.White, Projectile.rotation, noiseOrigin, drawScale, SpriteEffects.None, 0f);
                spriteBatch.End();
            }
            else
            {
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                DrawMainWave(spriteBatch, texture, origin);
                spriteBatch.End();
            }

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }

        private void DrawMainWave(SpriteBatch spriteBatch, Texture2D texture, Vector2 origin)
        {
            float alpha = 1f - Projectile.alpha / 255f;
            float scale = CurrentScale;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            Color outerGlow = new Color(255, 80, 20) * 0.3f * alpha;
            spriteBatch.Draw(texture, drawPos, null, outerGlow, Projectile.rotation, origin, scale * 1.8f, SpriteEffects.None, 0f);

            Color middleGlow = new Color(255, 150, 50) * 0.5f * alpha;
            spriteBatch.Draw(texture, drawPos, null, middleGlow, Projectile.rotation * 0.8f, origin, scale * 1.5f, SpriteEffects.None, 0f);

            Color mainColor = new Color(255, 220, 100) * 0.7f * alpha;
            spriteBatch.Draw(texture, drawPos, null, mainColor, Projectile.rotation, origin, scale * 1.2f, SpriteEffects.None, 0f);

            Color coreColor = new Color(255, 255, 230) * 0.9f * alpha;

            spriteBatch.Draw(texture, drawPos, null, coreColor, -Projectile.rotation * 0.5f, origin, scale * 1.2f, SpriteEffects.None, 0f);

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.15f + 1f;
            Color pulseColor = new Color(255, 200, 150) * 0.3f * alpha;
            spriteBatch.Draw(texture, drawPos, null, pulseColor, Projectile.rotation * 1.5f, origin, scale * pulse, SpriteEffects.None, 0f);
        }

        public override void OnKill(int timeLeft)
        {
            float scale = CurrentScale;
            if (Projectile.owner == Main.myPlayer && Main.netMode != NetmodeID.Server)
            {
                MultiwaveShaderSystem.Deactivate();
                Main.LocalPlayer.GetModPlayer<Screenshake_system>().stronger_shake = 1;
                Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<MultiwaveExplosionFlash>(), 0, 0f, Projectile.owner);
                Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<MultiwaveExplosion>(), Projectile.damage, Projectile.knockBack * 1.5f, Projectile.owner);
            }
            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact with { Volume = 0.8f, Pitch = -0.2f }, Projectile.Center);


            int smokeCount = Math.Max(1, (int)(6 * scale));
            for (int i = 0; i < smokeCount; i++)
            {
                Vector2 smokeVel = Main.rand.NextVector2Circular(5f, 5f) * scale;
                Dust smoke = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, smokeVel.X, smokeVel.Y, 150, default, Main.rand.NextFloat(1.2f, 2.0f));
                smoke.noGravity = true;
            }

            int particleCount = Math.Max(1, (int)(14 * scale));
            for (int i = 0; i < particleCount; i++)
            {
                float angle = MathHelper.TwoPi * i / particleCount + Main.rand.NextFloat(-0.05f, 0.05f);
                float speed = Main.rand.NextFloat(4f, 11f) * scale;
                Vector2 velocity = angle.ToRotationVector2() * speed;
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.SolarFlare, velocity, 0, new Color(255, Main.rand.Next(120, 255), 0), Main.rand.NextFloat(1.2f, 2.0f));
                dust.noGravity = true;
            }


            if (Main.netMode != NetmodeID.Server)
            {

                new Circleedge(Projectile.Center, Vector2.Zero, new Color(255, 140, 30), 25, 0.35f * scale, 1.0f, 0.25f * scale).Spawn();


                int customSparkCount = (int)(25 * scale);
                for (int i = 0; i < customSparkCount; i++)
                {
                    Vector2 sparkVel = Main.rand.NextVector2CircularEdge(9f, 9f) * scale * Main.rand.NextFloat(0.4f, 1.2f);
                    Color sparkColor = Color.Lerp(new Color(255, 200, 50), new Color(255, 80, 10), Main.rand.NextFloat());
                    new Spark(Projectile.Center, sparkVel, sparkColor, Main.rand.Next(20, 45), Main.rand.NextFloat(0.5f, 1.3f), 1f, Main.rand.NextFloat(-0.2f, 0.2f)).Spawn();
                }


                int magmaCount = (int)(15 * scale);
                for (int i = 0; i < magmaCount; i++)
                {
                    Vector2 magmaVel = Main.rand.NextVector2Circular(7f, 7f) * scale;
                    new MagmaParticle(Projectile.Center, magmaVel, new Color(255, 160, 30), Main.rand.Next(25, 50), Main.rand.NextFloat(0.4f, 0.9f), 1f).Spawn();
                }


                int solidCount = (int)(18 * scale);
                for (int i = 0; i < solidCount; i++)
                {
                    Vector2 solidVel = Main.rand.NextVector2Circular(8f, 8f) * scale;
                    new Solid_particle(Projectile.Center, solidVel, new Color(255, 120, 20), Main.rand.Next(20, 40), Main.rand.NextFloat(0.4f, 0.8f), 1f).Spawn();
                }


                int starCount = (int)(8 * scale);
                for (int i = 0; i < starCount; i++)
                {
                    Vector2 starVel = Main.rand.NextVector2Circular(6f, 6f) * scale;
                    new StarParticleBigger(Projectile.Center, starVel, new Color(255, 210, 100), Main.rand.Next(20, 35), Main.rand.NextFloat(0.1f, 0.25f), 1f, Main.rand.NextFloat(-0.1f, 0.1f)).Spawn();
                }
            }

            for (int i = 0; i < 10; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(Projectile.width / 3f, Projectile.height / 3f);
                Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center + offset, Main.rand.NextVector2Circular(5f, 5f), ModContent.ProjectileType<SolarFlareParticle>(), Projectile.damage / 3, 0f, Projectile.owner);
            }
        }
    }

    #region SolarFlare
    public class SolarFlareParticle : ModProjectile
    {
        public override string Texture => "jarkos/projectiles/mecha/Astra_bomb_charged";
        private float initialScale;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 50;
            Projectile.light = 0.8f;
        }

        public override void AI()
        {
            if (Projectile.ai[0] == 0)
            {
                initialScale = Main.rand.NextFloat(0.5f, 1.2f);
                Projectile.ai[0] = 1;
            }
            float lifeProgress = 1f - Projectile.timeLeft / 60f;
            Projectile.scale = initialScale * (1f - lifeProgress * 0.7f);
            Projectile.velocity *= 0.96f;
            Projectile.rotation += 0.2f;
            Projectile.alpha = (int)(50 + 200 * lifeProgress);

            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.SolarFlare, Main.rand.NextVector2Circular(2f, 2f), 0, new Color(255, 180, 50), 0.8f * Projectile.scale);
                dust.noGravity = true;
            }
            Lighting.AddLight(Projectile.Center, new Vector3(1f, 0.6f, 0.2f) * Projectile.scale);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire, 120);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;
            float alpha = 1f - Projectile.alpha / 255f;

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float trailProgress = (float)i / Projectile.oldPos.Length;
                Color trailColor = new Color(255, 150, 50) * (1f - trailProgress) * alpha * 0.5f;
                float trailScale = Projectile.scale * (1f - trailProgress * 0.5f);
                spriteBatch.Draw(texture, Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
            }

            Color mainColor = new Color(255, 200, 100) * alpha;
            spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, mainColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);

            Color coreColor = Color.White * alpha * 0.8f;
            spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, coreColor, -Projectile.rotation, origin, Projectile.scale * 0.5f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
    }

    public class MultiwaveExplosionFlash : ModProjectile
    {
        public override string Texture => "jarkos/projectiles/mecha/Multiwave_Emitter_beam";

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 12;
            Projectile.alpha = 255;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            float progress = 1f - Projectile.timeLeft / 12f;
            Projectile.alpha = (int)(progress * 255);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);

            Texture2D pixel = TextureAssets.MagicPixel.Value;
            float progress = Projectile.timeLeft / 12f;
            float intensity = 0.45f * progress;
            Color flashColor = new Color(255, 195, 110) * intensity;
            spriteBatch.Draw(pixel, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), flashColor);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
    }
    #endregion

    public class MultiwaveExplosion : ModProjectile
    {
        public override string Texture => "jarkos/projectiles/mecha/Multiwave_Emitter_beam";

        public override void SetDefaults()
        {

            Projectile.width = 750;
            Projectile.height = 750;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 26;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            float progress = 1f - (float)Projectile.timeLeft / 26f;
            float light = (1f - progress) * 1.8f;
            Lighting.AddLight(Projectile.Center, new Vector3(1f, 0.45f, 0.08f) * light);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (MultiwaveShaderSystem.ExplosionShader == null)
                return false;

            SpriteBatch spriteBatch = Main.spriteBatch;
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Texture2D noiseTex = ModContent.Request<Texture2D>("jarkos/textures/Noises/EnergyNoise", AssetRequestMode.ImmediateLoad).Value;
            Vector2 origin = noiseTex.Size() / 2f;
            float progress = 1f - Projectile.timeLeft / 26f;
            float scale = Projectile.width / (float)noiseTex.Width;

            MultiwaveShaderSystem.ExplosionShader.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly * 2.5f);
            MultiwaveShaderSystem.ExplosionShader.Parameters["uProgress"]?.SetValue(progress);
            MultiwaveShaderSystem.ExplosionShader.Parameters["uColor"]?.SetValue(new Vector3(1.0f, 0.42f, 0.05f));
            MultiwaveShaderSystem.ExplosionShader.Parameters["uSeed"]?.SetValue(Projectile.identity * 0.13f);
            MultiwaveShaderSystem.ExplosionShader.CurrentTechnique.Passes[0].Apply();

            spriteBatch.Draw(noiseTex, Projectile.Center - Main.screenPosition, null, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            float progress = 1f - Projectile.timeLeft / 26f;
            modifiers.FinalDamage *= MathHelper.Lerp(1.2f, 0.5f, progress);
        }
    }
}
