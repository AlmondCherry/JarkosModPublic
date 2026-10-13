using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.true_melee
{
	public class Incineration_proj : ModProjectile {
        private const int MaxBounces = 3;
        private const int TrailLength = 30;
        private Vector2[] trailPositions = new Vector2[TrailLength];
        private float[] trailRotations = new float[TrailLength];
        private bool trailInitialized = false;
        private int BounceCount
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        private bool Returning
        {
            get => Projectile.ai[1] == 1f;
            set => Projectile.ai[1] = value ? 1f : 0f;
        }
        private float LiveTimer
        {
            get => Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = TrailLength;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.extraUpdates = 1;
        }
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            LiveTimer++;
            if (!trailInitialized)
            {
                for (int i = 0; i < TrailLength; i++)
                {
                    trailPositions[i] = Projectile.Center;
                    trailRotations[i] = Projectile.rotation;
                }
                trailInitialized = true;
            }
            for (int i = TrailLength - 1; i > 0; i--)
            {
                trailPositions[i] = trailPositions[i - 1];
                trailRotations[i] = trailRotations[i - 1];
            }
            trailPositions[0] = Projectile.Center;
            trailRotations[0] = Projectile.rotation;
            float spinSpeed = 0.35f + BounceCount * 0.15f;
            Projectile.rotation += spinSpeed * Projectile.direction;
            if (Returning)
            {
                Projectile.tileCollide = false;
                Vector2 toOwner = owner.Center - Projectile.Center;
                float distance = toOwner.Length();
                if (distance < 30f)
                {
                    Projectile.Kill();
                    return;
                }
                float returnSpeed = 18f + BounceCount * 2f;
                Vector2 desiredVelocity = toOwner.SafeNormalize(Vector2.Zero) * returnSpeed;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVelocity, 0.08f);
            }
            else
            {
                if (LiveTimer > 10)
                {
                    NPC closest = FindClosestNPC(400f);
                    if (closest != null)
                    {
                        Vector2 toTarget = (closest.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), 0.02f + BounceCount * 0.01f);
                    }
                }
                if (LiveTimer > 180)
                {
                    Returning = true;
                }
            }
            SpawnFlightParticles();
            float intensity = 0.6f + BounceCount * 0.15f;
            Lighting.AddLight(Projectile.Center, 1f * intensity, 0.5f * intensity, 0.1f * intensity);
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            BounceCount++;
            if (BounceCount >= MaxBounces)
            {
                Returning = true;
                SpawnBounceExplosion(Projectile.Center, true);
                SoundEngine.PlaySound(SoundID.Item74 with { Pitch = 0.3f, Volume = 0.9f }, Projectile.Center);
            }
            else
            {
                if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
                    Projectile.velocity.X = -oldVelocity.X;
                if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
                    Projectile.velocity.Y = -oldVelocity.Y;
                Projectile.velocity *= 1.15f;
                SpawnBounceExplosion(Projectile.Center, false);
                SoundEngine.PlaySound(SoundID.Item10 with { Pitch = 0.2f + BounceCount * 0.15f }, Projectile.Center);
            }
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire, 180 + BounceCount * 60);
            SpawnHitParticles(target.Center);
            SoundEngine.PlaySound(SoundID.Item45 with { Pitch = -0.2f, Volume = 0.7f }, target.Center);
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.FinalDamage *= 1f + BounceCount * 0.2f;
        }
        private NPC FindClosestNPC(float maxDistance)
        {
            NPC closest = null;
            float closestDist = maxDistance;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage)
                    continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc;
                }
            }
            return closest;
        }
        private void SpawnFlightParticles()
        {
            if (Main.rand.NextBool(2))
            {
                Vector2 offset = Main.rand.NextVector2Circular(8f, 8f);
                Dust fire = Dust.NewDustDirect(Projectile.Center + offset, 0, 0, DustID.Torch, -Projectile.velocity.X * 0.2f, -Projectile.velocity.Y * 0.2f, 100, default, 1.8f + BounceCount * 0.3f);
                fire.noGravity = true;
                fire.fadeIn = 1.2f;
            }
            if (Main.rand.NextBool(4))
            {
                Dust ember = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Smoke, Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-2f, 0f), 150, new Color(80, 40, 10), 1.5f);
                ember.noGravity = false;
                ember.fadeIn = 0.8f;
            }
            if (Projectile.velocity.Length() > 12f && Main.rand.NextBool(3))
            {
                Dust Spark = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.OrangeTorch, Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f), 0, default, 1.2f);
                Spark.noGravity = true;
                Spark.velocity += -Projectile.velocity * 0.1f;
            }
        }
        private void SpawnHitParticles(Vector2 position)
        {
            for (int i = 0; i < 15; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
                int dustType = Main.rand.NextBool() ? DustID.Torch : DustID.OrangeTorch;
                Dust d = Dust.NewDustDirect(position, 0, 0, dustType, vel.X, vel.Y, 80, default, Main.rand.NextFloat(1.5f, 2.5f));
                d.noGravity = true;
                d.fadeIn = 1.5f;
            }
        }
        private void SpawnBounceExplosion(Vector2 position, bool isFinal)
        {
            int count = isFinal ? 30 : 15;
            float speed = isFinal ? 10f : 6f;
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi / count * i;
                Vector2 vel = angle.ToRotationVector2() * speed * Main.rand.NextFloat(0.5f, 1.2f);
                int dustType = Main.rand.NextBool(3) ? DustID.Smoke : DustID.Torch;
                Dust d = Dust.NewDustDirect(position, 0, 0, dustType, vel.X, vel.Y, 100, default, Main.rand.NextFloat(2f, 3.5f));
                d.noGravity = true;
                d.fadeIn = 1.5f;
            }
            if (isFinal)
            {
                for (int i = 0; i < 12; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                    Dust smoke = Dust.NewDustDirect(position, 0, 0, DustID.Smoke, vel.X, vel.Y, 200, new Color(60, 30, 10), 4f);
                    smoke.noGravity = true;
                    smoke.fadeIn = 3f;
                }
            }
            Lighting.AddLight(position, 1.5f, 0.8f, 0.2f);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
            DrawMagicPixelTrail();
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
            DrawGlow(lightColor);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
            DrawDisc(lightColor);
            return false;
        }
        private void DrawMagicPixelTrail()
        {
            Texture2D magicPixel = TextureAssets.MagicPixel.Value;
            for (int i = TrailLength - 1; i > 0; i--)
            {
                Vector2 current = trailPositions[i];
                Vector2 next = trailPositions[i - 1];

                if (current == Vector2.Zero || next == Vector2.Zero)
                    continue;

                Vector2 diff = next - current;
                float length = diff.Length();
                if (length < 1f)
                    continue;

                float rotation = diff.ToRotation();
                float progress = 1f - (float)i / TrailLength;
                float waveTime = LiveTimer * 0.15f + i * 0.3f;
                float wave = (float)Math.Sin(waveTime) * 3f;
                float baseWidth = MathHelper.Lerp(1f, 16f + BounceCount * 4f, progress);
                float width = Math.Max(baseWidth + wave, 1f);
                Color trailColor;
                float alpha;
                if (progress > 0.8f)
                {
                    trailColor = Color.Lerp(new Color(255, 200, 60), new Color(255, 255, 200), (progress - 0.8f) / 0.2f);
                    alpha = 0.95f;
                }
                else if (progress > 0.4f)
                {
                    trailColor = Color.Lerp(new Color(255, 100, 20), new Color(255, 200, 60), (progress - 0.4f) / 0.4f);
                    alpha = MathHelper.Lerp(0.7f, 0.9f, (progress - 0.4f) / 0.4f);
                }
                else
                {
                    trailColor = Color.Lerp(new Color(40, 5, 0), new Color(255, 100, 20), progress / 0.4f);
                    alpha = MathHelper.Lerp(0f, 0.7f, progress / 0.4f);
                }
                trailColor *= alpha;
                Vector2 scale = new Vector2(length / magicPixel.Width, width / magicPixel.Height);
                Vector2 origin = new Vector2(0, magicPixel.Height / 2f);
                Main.EntitySpriteDraw(magicPixel, current - Main.screenPosition, null, trailColor, rotation, origin, scale, SpriteEffects.None, 0);
                if (progress > 0.3f)
                {
                    float coreWidth = width * 0.4f;
                    Vector2 coreScale = new Vector2(length / magicPixel.Width, coreWidth / magicPixel.Height);
                    Color coreColor = Color.Lerp(new Color(255, 220, 100), new Color(255, 255, 220), progress) * (alpha * 0.6f);
                    Main.EntitySpriteDraw(magicPixel, current - Main.screenPosition, null, coreColor, rotation, origin, coreScale, SpriteEffects.None, 0);
                }
                if (progress > 0.2f && i % 3 == 0)
                {
                    float flameOffset = (float)Math.Sin(waveTime * 2f) * width * 0.6f;
                    Vector2 perpendicular = new Vector2(-diff.Y, diff.X).SafeNormalize(Vector2.Zero);
                    Vector2 flamePos = current + perpendicular * flameOffset;
                    float flameWidth = width * 0.3f;
                    float flameLength = length * 0.5f;
                    Vector2 flameScale = new Vector2(flameLength / magicPixel.Width, flameWidth / magicPixel.Height);
                    Color flameColor = new Color(255, 150, 30) * (alpha * 0.4f);
                    float flameRot = rotation + (float)Math.Sin(waveTime * 3f) * 0.3f;
                    Main.EntitySpriteDraw(magicPixel, flamePos - Main.screenPosition, null, flameColor, flameRot, origin, flameScale, SpriteEffects.None, 0);
                }
            }
        }
        private void DrawGlow(Color lightColor)
        {
            Texture2D glowTexture2 = ModContent.Request<Texture2D>("jarkos/projectiles/bullet/Biomechanical_heart_proj_glowtwo").Value;

            float pulse = 1f + (float)Math.Sin(LiveTimer * 0.12f) * 0.2f;
            float glowSize = (0.5f + BounceCount * 0.25f) * pulse;
            float layerScale = glowSize;

            Vector2 glowScale = new Vector2(layerScale / glowTexture2.Width, layerScale / glowTexture2.Height);
            Vector2 glowOrigin = new Vector2(glowTexture2.Width / 2f, glowTexture2.Height / 2f);
            Main.EntitySpriteDraw(glowTexture2, Projectile.Center - Main.screenPosition, null, new Color(255, 255, 200) * 1f, 0f, glowOrigin, glowSize, SpriteEffects.None, 0);
        }
        private void DrawDisc(Color lightColor)
        {
            Main.instance.LoadProjectile(Type);
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2f;

            float brightness = 1f + BounceCount * 0.15f;
            Color drawColor = lightColor * brightness;

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, drawColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);

            float additivePulse = 0.3f + (float)Math.Sin(LiveTimer * 0.15f) * 0.15f;
            Color additiveColor = new Color(255, 150, 50) * (additivePulse + BounceCount * 0.1f);

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, additiveColor, Projectile.rotation, origin, Projectile.scale * 1.05f, SpriteEffects.None, 0);
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Torch, vel.X, vel.Y, 100, default, 2f);
                d.noGravity = true;
            }
        }
	}
	public class ScorchedGroundFire : ModProjectile
    {
		public override string Texture => "jarkos/projectiles/Empty_proj";
        private float IntensityScale
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.alpha = 255;
        }
        public override void AI()
        {
            float lifeProgress = 1f - Projectile.timeLeft / 180f;
            if (Main.rand.NextBool(3))
            {
                float xOffset = Main.rand.NextFloat(-30f, 30f) * (1f + IntensityScale * 0.3f);
                Vector2 pos = Projectile.Center + new Vector2(xOffset, Main.rand.NextFloat(-5f, 5f));
                Dust fire = Dust.NewDustDirect(pos, 0, 0, DustID.Torch, Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-4f, -1f), 100, default, 2f * (1f - lifeProgress));
                fire.noGravity = true;
                fire.fadeIn = 1.5f;
            }
            if (Main.rand.NextBool(5))
            {
                Dust smoke = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(20f, 5f), 0, 0, DustID.Smoke, 0f, Main.rand.NextFloat(-2f, -0.5f), 180, default, 2f);
                smoke.noGravity = true;
            }
            float lightIntensity = (1f - lifeProgress) * (0.5f + IntensityScale * 0.2f);
            Lighting.AddLight(Projectile.Center, 1f * lightIntensity, 0.5f * lightIntensity, 0.1f * lightIntensity);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire, 120);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D magicPixel = TextureAssets.MagicPixel.Value;
            float lifeProgress = 1f - Projectile.timeLeft / 180f;
            float alpha = 1f - lifeProgress;
            float scale = 1f + IntensityScale * 0.3f;
            for (int layer = 0; layer < 3; layer++)
            {
                float layerWidth = (60f + layer * 20f) * scale;
                float layerHeight = (8f + layer * 4f) * scale;
                float layerAlpha = alpha * (0.4f / (1f + layer * 0.5f));
                float wave = (float)Math.Sin(Main.GameUpdateCount * 0.1f + layer * 2f) * 3f;
                
                Color color = layer switch
                {
                    0 => new Color(255, 220, 100) * layerAlpha, 1 => new Color(255, 130, 30) * layerAlpha, _ => new Color(180, 50, 10) * layerAlpha
                };

                Vector2 drawScale = new Vector2(layerWidth / magicPixel.Width, (layerHeight + wave) / magicPixel.Height);
                Main.EntitySpriteDraw(magicPixel, Projectile.Center - Main.screenPosition + new Vector2(0, wave * 0.3f), null, color, 0f, new Vector2(magicPixel.Width / 2f, magicPixel.Height / 2f), drawScale, SpriteEffects.None, 0);
            }
            return false;
        }
    }
}
