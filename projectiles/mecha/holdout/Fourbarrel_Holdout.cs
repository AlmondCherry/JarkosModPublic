using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;
using System.Collections.Generic;
namespace jarkos.projectiles.mecha.holdout
{
	public class Fourbarrel_holdout : ModProjectile
	{
        public override string Texture => "jarkos/projectiles/mecha/holdout/Fourbarrel_Holdout";
        private const float MAX_CHARGE_TIME = 60f;
        private const float MIN_RADIUS = 10f;
        private const float MAX_RADIUS = 40f;
        private float ChargeTime
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        private float SoundTimer
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        private Player Owner => Main.player[Projectile.owner];
        private float currentPulse = 0f;
        private float glowIntensity = 0f;
        private List<SolarFlare> flares = new List<SolarFlare>();
        private List<TrailPoint> trailPoints = new List<TrailPoint>();
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.timeLeft = 999999;
        }
        public override void AI()
        {
        if (!Owner.active || Owner.dead || Owner.CCed || Owner.noItems)
        {
            Projectile.Kill();
            return;
        }
        bool hasEnoughMana = Owner.statMana >= 1 || Owner.channel;
        if (Owner.channel && hasEnoughMana)
        {
            UpdateCharging();
        }
        else
        {
            ReleaseSupernova();
            Projectile.Kill();
            return;
        }
            UpdatePosition();
            UpdateVisuals();
            UpdateSounds();
            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;
        }
        private void UpdateCharging()
        {
            if (ChargeTime < MAX_CHARGE_TIME)
            {
                ChargeTime++;
                if (ChargeTime >= MAX_CHARGE_TIME)
                {
                    for (int i = 0; i < 30; i++)
                    {
                        Vector2 dustVel = Main.rand.NextVector2Circular(8f, 8f);
                        Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.SolarFlare, dustVel, 0, new Color(255, 200, 50), 2f);
                        dust.noGravity = true;
                    }
                    SoundEngine.PlaySound(SoundID.Item29, Projectile.Center);
                }
            }
        }
        private void UpdatePosition()
        {
            Vector2 toMouse = Main.MouseWorld - Owner.Center;
            toMouse.Normalize();
            float offsetDistance = 60f + GetCurrentRadius() * 0.3f;
            Projectile.Center = Owner.Center + toMouse * offsetDistance;
            Owner.ChangeDir(Main.MouseWorld.X > Owner.Center.X ? 1 : -1);
            Projectile.rotation = toMouse.ToRotation();
            Owner.itemRotation = (float)Math.Atan2(toMouse.Y * Owner.direction, toMouse.X * Owner.direction);
        }
        private void UpdateVisuals()
        {
            float chargeProgress = ChargeTime / MAX_CHARGE_TIME;
            float currentRadius = GetCurrentRadius();
            currentPulse += 0.15f + chargeProgress * 0.1f;
            glowIntensity = 0.5f + (float)Math.Sin(currentPulse) * 0.3f + chargeProgress * 0.5f;
            SpawnChargeParticles(chargeProgress, currentRadius);
            UpdateFlares(chargeProgress);
            UpdateTrail();
            CreateGravityWell(currentRadius);
            float lightRadius = 1f + chargeProgress * 2f;
            Lighting.AddLight(Projectile.Center, new Vector3(1f, 0.7f, 0.2f) * lightRadius * glowIntensity);
        }
        private void SpawnChargeParticles(float progress, float radius)
        {
            int particleCount = (int)(2 + progress * 8);
            for (int i = 0; i < particleCount; i++)
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float distance = radius * (0.8f + Main.rand.NextFloat() * 0.4f);
                Vector2 offset = new Vector2((float)Math.Cos(angle) * distance, (float)Math.Sin(angle) * distance);
                Vector2 velocity = -offset.SafeNormalize(Vector2.Zero) * (2f + progress * 3f);
                velocity = velocity.RotatedBy(MathHelper.PiOver2 * 0.3f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, DustID.SolarFlare, velocity, 0, new Color(255, 150 + Main.rand.Next(100), Main.rand.Next(100)), 1f + progress * 1.5f);
                dust.noGravity = true;
                dust.fadeIn = 0.5f;
                if (Main.rand.NextBool(3))
                {
                    Dust fireDust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(radius * 0.5f, radius * 0.5f), DustID.Torch, Main.rand.NextVector2Circular(1f, 1f), 0, default, 2f);
                    fireDust.noGravity = true;
                }
            }
            if (progress > 0.5f && Main.rand.NextBool(5))
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 sparkPos = Projectile.Center + Main.rand.NextVector2Circular(radius, radius);
                    Vector2 sparkVel = Main.rand.NextVector2Circular(4f, 4f);
                    Dust Spark = Dust.NewDustPerfect(sparkPos, DustID.Electric, sparkVel, 0, new Color(255, 255, 200), 0.8f);
                    Spark.noGravity = true;
                }
            }
        }
        private void UpdateFlares(float progress)
        {
            if (Main.rand.NextBool(10 - (int)(progress * 8)))
            {
                flares.Add(new SolarFlare                {
                    Angle = Main.rand.NextFloat() * MathHelper.TwoPi, Length = 20f + Main.rand.NextFloat() * 30f * (1f + progress), Width = 3f + Main.rand.NextFloat() * 5f, Life = 20 + Main.rand.Next(20), MaxLife = 20 + Main.rand.Next(20), Color = new Color(255, 150 + Main.rand.Next(100), Main.rand.Next(50))
                });
            }
            for (int i = flares.Count - 1; i >= 0; i--)
            {
                flares[i].Life--;
                flares[i].Angle += 0.02f;
                if (flares[i].Life <= 0)
                    flares.RemoveAt(i);
            }
        }
        private void UpdateTrail()
        {
            trailPoints.Insert(0, new TrailPoint            {
                Position = Projectile.Center, Time = 0f
            });
            for (int i = trailPoints.Count - 1; i >= 0; i--)
            {
                trailPoints[i].Time += 1f / 20f;
                if (trailPoints[i].Time >= 1f)
                    trailPoints.RemoveAt(i);
            }
            if (trailPoints.Count > 30)
                trailPoints.RemoveAt(trailPoints.Count - 1);
        }
        private void CreateGravityWell(float radius)
        {
            foreach (Dust dust in Main.dust)
            {
                if (!dust.active) continue;
                float distance = Vector2.Distance(dust.position, Projectile.Center);
                if (distance < radius * 2f && distance > 5f)
                {
                    Vector2 pullDir = (Projectile.Center - dust.position).SafeNormalize(Vector2.Zero);
                    dust.velocity += pullDir * (0.5f / (distance / radius));
                }
            }
        }
        private void UpdateSounds()
        {
            SoundTimer++;
            if (SoundTimer % 20 == 0)
            {
                float pitch = -0.5f + (ChargeTime / MAX_CHARGE_TIME) * 1f;
                SoundEngine.PlaySound(SoundID.Item15 with
                {
                    Pitch = pitch, Volume = 0.5f + ChargeTime / MAX_CHARGE_TIME * 0.5f, MaxInstances = 3
                }, Projectile.Center);
            }
            if (ChargeTime > MAX_CHARGE_TIME * 0.7f && SoundTimer % 10 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item93 with
                {
                    Pitch = 0.5f, Volume = 0.3f
                }, Projectile.Center);
            }
        }
        private void ReleaseSupernova()
        {
            if (ChargeTime < 10) return;
            float chargeProgress = ChargeTime / MAX_CHARGE_TIME;
            float explosionRadius = MIN_RADIUS + (MAX_RADIUS - MIN_RADIUS) * chargeProgress;
            Vector2 toMouse = Main.MouseWorld - Owner.Center;
            toMouse.Normalize();
            float speed = 20f - chargeProgress * 10f;
            speed = Math.Max(speed, 8f);
            int proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, toMouse * speed, ModContent.ProjectileType<Fourbarrel_holdout>(), (int)(Projectile.damage * (0.5f + chargeProgress * 1.5f)), Projectile.knockBack * (1f + chargeProgress), Projectile.owner, explosionRadius, chargeProgress);
            SoundEngine.PlaySound(SoundID.Item14 with
            {
                Pitch = -0.3f, Volume = 1f + chargeProgress * 0.5f
            }, Projectile.Center);
            Owner.velocity -= toMouse * (3f + chargeProgress * 5f);
        }
        private float GetCurrentRadius()
        {
            float progress = ChargeTime / MAX_CHARGE_TIME;
            return MIN_RADIUS * 0.3f + progress * MIN_RADIUS * 0.7f;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            float chargeProgress = ChargeTime / MAX_CHARGE_TIME;
            float radius = GetCurrentRadius();
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            DrawTrail(spriteBatch);
            DrawOuterGlow(spriteBatch, radius, chargeProgress);
            DrawFlares(spriteBatch, radius);
            DrawCore(spriteBatch, radius, chargeProgress);
            DrawCorona(spriteBatch, radius, chargeProgress);
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
        private void DrawTrail(SpriteBatch spriteBatch)
        {
            if (trailPoints.Count < 2) return;
            Texture2D texture = TextureAssets.MagicPixel.Value;
            for (int i = 0; i < trailPoints.Count - 1; i++)
            {
                float progress = trailPoints[i].Time;
                float alpha = (1f - progress) * 0.5f;
                float width = (1f - progress) * 8f;
                Vector2 start = trailPoints[i].Position - Main.screenPosition;
                Vector2 end = trailPoints[i + 1].Position - Main.screenPosition;
                Vector2 diff = end - start;
                float rotation = diff.ToRotation();
                float length = diff.Length();
                Color trailColor = Color.Lerp(new Color(255, 200, 50), new Color(255, 100, 0), progress) * alpha;
                spriteBatch.Draw(texture, start, new Rectangle(0, 0, 1, 1), trailColor, rotation, Vector2.Zero, new Vector2(length, width), SpriteEffects.None, 0f);
            }
        }
        private void DrawOuterGlow(SpriteBatch spriteBatch, float radius, float progress)
        {
            Texture2D glowTex = TextureAssets.Extra[ExtrasID.SharpTears].Value;
            Vector2 center = Projectile.Center - Main.screenPosition;
            for (int i = 0; i < 4; i++)
            {
                float layerScale = (radius / glowTex.Width * 4f) * (1f + i * 0.3f);
                float layerAlpha = (0.3f - i * 0.05f) * glowIntensity;
                Color glowColor = Color.Lerp(new Color(255, 200, 100), new Color(255, 100, 50), i / 4f) * layerAlpha;
                spriteBatch.Draw(glowTex, center, null, glowColor, 0f, glowTex.Size() / 2f, layerScale * (1f + (float)Math.Sin(currentPulse + i) * 0.1f), SpriteEffects.None, 0f);
            }
        }
        private void DrawFlares(SpriteBatch spriteBatch, float radius)
        {
            Texture2D flareTex = TextureAssets.Extra[ExtrasID.SharpTears].Value;
            Vector2 center = Projectile.Center - Main.screenPosition;
            foreach (var flare in flares)
            {
                float lifeProgress = (float)flare.Life / flare.MaxLife;
                float alpha = lifeProgress * 0.8f;
                Vector2 flareDir = new Vector2((float)Math.Cos(flare.Angle), (float)Math.Sin(flare.Angle));
                Vector2 flarePos = center + flareDir * radius * 0.8f;
                spriteBatch.Draw(flareTex, flarePos, null, flare.Color * alpha, flare.Angle, new Vector2(0, flareTex.Height / 2f), new Vector2(flare.Length / flareTex.Width, flare.Width / flareTex.Height), SpriteEffects.None, 0f);
            }
        }
        private void DrawCore(SpriteBatch spriteBatch, float radius, float progress)
        {
            Texture2D coreTex = TextureAssets.Extra[ExtrasID.SharpTears].Value;
            Vector2 center = Projectile.Center - Main.screenPosition;
            float coreScale = radius / coreTex.Width * 2f;
            Color coreColor = Color.White * (0.8f + (float)Math.Sin(currentPulse * 2) * 0.2f);
            spriteBatch.Draw(coreTex, center, null, coreColor, 0f, coreTex.Size() / 2f, coreScale * 0.5f, SpriteEffects.None, 0f);
            spriteBatch.Draw(coreTex, center, null, new Color(255, 220, 100) * 0.6f, 0f, coreTex.Size() / 2f, coreScale * 0.7f, SpriteEffects.None, 0f);
        }
        private void DrawCorona(SpriteBatch spriteBatch, float radius, float progress)
        {
            Texture2D coronaTex = TextureAssets.Extra[ExtrasID.SharpTears].Value;
            Vector2 center = Projectile.Center - Main.screenPosition;
            int rays = 8 + (int)(progress * 8);
            for (int i = 0; i < rays; i++)
            {
                float angle = (float)i / rays * MathHelper.TwoPi + currentPulse * 0.5f;
                float rayLength = radius * (0.8f + (float)Math.Sin(currentPulse * 2 + i) * 0.3f);
                Vector2 rayDir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                Vector2 rayPos = center + rayDir * radius * 0.3f;
                Color rayColor = Color.Lerp(new Color(255, 200, 50), new Color(255, 100, 0), (float)i / rays) * (0.4f + progress * 0.3f);
                spriteBatch.Draw(coronaTex, rayPos, null, rayColor, angle, new Vector2(0, coronaTex.Height / 2f), new Vector2(rayLength / coronaTex.Width, 0.1f + progress * 0.1f), SpriteEffects.None, 0f);
            }
        }
        public override bool? CanDamage() => false;
    }
    public class SolarFlare
    {
        public float Angle;
        public float Length;
        public float Width;
        public int Life;
        public int MaxLife;
        public Color Color;
    }
    public class TrailPoint
    {
        public Vector2 Position;
        public float Time;
	}
}
