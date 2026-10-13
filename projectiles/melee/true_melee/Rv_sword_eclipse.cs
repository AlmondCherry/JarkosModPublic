using System;
using System.Collections.Generic;
using jarkos.buffs.debuff;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.true_melee
{
	public class Rv_sword_eclipse : ModProjectile
	{
        public float Timer
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        public float Phase
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        private const int PHASE_CHARGE = 0;
        private const int PHASE_EXPAND = 1;
        private const int PHASE_EXPLODE = 2;
        private Vector2 targetPosition;
        private bool initialized = false;
        private float chargeLevel = 0f;
        private float energyRotation = 0f;
        private float pulseIntensity = 1f;
        private float expansionTimer = 0f;
        private float[] orbitalAngles = new float[12];
        private float[] orbitalDistances = new float[12];
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 35;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 100;
            Projectile.height = 100;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 5;
        }
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            Timer++;
            energyRotation += 0.12f + chargeLevel * 0.08f;
            if (!initialized)
            {
                initialized = true;
                targetPosition = Main.MouseWorld;
                Phase = PHASE_CHARGE;
                for (int i = 0; i < 12; i++)
                {
                    orbitalAngles[i] = MathHelper.TwoPi / 12 * i;
                    orbitalDistances[i] = 40f + Main.rand.NextFloat(20f);
                }
                SoundEngine.PlaySound(SoundID.Item117 with { Pitch = -0.3f }, Projectile.Center);
            }
            if (Timer < 25 && Projectile.owner == Main.myPlayer)
            {
                targetPosition = Main.MouseWorld;
            }
            float basePulse = 1f + MathF.Sin(Timer * 0.12f) * 0.12f;
            float chargePulse = 1f + chargeLevel * 0.4f;
            pulseIntensity = basePulse * chargePulse;
            switch ((int)Phase)
            {
                case PHASE_CHARGE:
                    AI_ChargePhase();
                    break;
                case PHASE_EXPAND:
                    AI_ExpandPhase();
                    break;
                case PHASE_EXPLODE:
                    AI_ExplodePhase();
                    break;
            }
            UpdateOrbitals();
            SpawnParticles();
            UpdateLighting();
        }
        private void AI_ChargePhase()
        {
            Vector2 toTarget = targetPosition - Projectile.Center;
            float distance = toTarget.Length();
            if (distance > 50f)
            {
                float speed = MathHelper.Clamp(8f + chargeLevel * 12f, 8f, 22f);
                Vector2 desiredVel = Vector2.Normalize(toTarget) * speed;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVel, 0.1f + chargeLevel * 0.05f);
                Projectile.rotation = Projectile.velocity.ToRotation();
                chargeLevel = MathHelper.Clamp(chargeLevel + 0.015f, 0f, 1f);
                Projectile.scale = 1f + chargeLevel * 0.6f;
                if (chargeLevel > 0.3f)
                {
                    PullEnemies(200f * chargeLevel, 3f * chargeLevel);
                }
            }
            else
            {
                Phase = PHASE_EXPAND;
                expansionTimer = 0f;
                chargeLevel = 1f;
                Projectile.velocity *= 0.1f;
                SoundEngine.PlaySound(SoundID.Item82 with { Pitch = 0.2f }, Projectile.Center);
            }
            if (Timer % 12 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item15 with {
                    Pitch = -0.5f + chargeLevel * 0.8f, Volume = 0.4f + chargeLevel * 0.3f
                }, Projectile.Center);
            }
            int size = (int)(80 + 40 * chargeLevel);
            Projectile.width = size;
            Projectile.height = size;
        }
        private void AI_ExpandPhase()
        {
            expansionTimer++;
            Projectile.velocity *= 0.92f;
            float expandProgress = expansionTimer / 45f;
            Projectile.scale = 1.6f + expandProgress * 1.2f;
            float pullRadius = 300f + expandProgress * 200f;
            float pullStrength = 8f + expandProgress * 6f;
            PullEnemies(pullRadius, pullStrength);
            if (expansionTimer % 4 == 0)
            {
                DamageNearbyEnemies(150f + expandProgress * 100f);
            }
            energyRotation += expandProgress * 0.15f;
            int size = (int)(120 + expandProgress * 80);
            Projectile.width = size;
            Projectile.height = size;
            if (expansionTimer % 15 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item29 with { Pitch = -0.3f + expandProgress * 0.5f }, Projectile.Center);
            }
            if (expansionTimer >= 45)
            {
                Phase = PHASE_EXPLODE;
                TriggerMegaExplosion();
            }
        }
        private void AI_ExplodePhase()
        {
            Projectile.alpha += 15;
            Projectile.scale *= 1.08f;
            if (Projectile.alpha >= 255)
            {
                Projectile.Kill();
            }
        }
        private void TriggerMegaExplosion()
        {
            SoundEngine.PlaySound(SoundID.Item162 with { Pitch = -0.5f, Volume = 1.5f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item119 with { Volume = 1.2f }, Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.7f }, Projectile.Center);
            for (int ring = 0; ring < 5; ring++)
            {
                int count = 16 + ring * 8;
                float speed = 6 + ring * 4;
                float delay = ring * 0.1f;
                for (int i = 0; i < count; i++)
                {
                    float angle = MathHelper.TwoPi / count * i + ring * 0.15f;
                    Vector2 vel = new Vector2(speed, 0).RotatedBy(angle);
                    int dustType;
                    float scale;
                    if ((i + ring) % 3 == 0)
                    {
                        dustType = DustID.SolarFlare;
                        scale = 3f - ring * 0.3f;
                    }
                    else if ((i + ring) % 3 == 1)
                    {
                        dustType = DustID.IceTorch;
                        scale = 2.8f - ring * 0.3f;
                    }
                    else
                    {
                        dustType = DustID.PurpleTorch;
                        scale = 2.5f - ring * 0.25f;
                    }
                    Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, dustType, vel.X, vel.Y, 100, default, scale);
                    d.noGravity = true;
                }
            }
            for (int i = 0; i < 80; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(18, 18);
                int dustType = i % 3 == 0 ? DustID.SolarFlare :
                               i % 3 == 1 ? DustID.IceTorch : DustID.PurpleTorch;
                Dust flash = Dust.NewDustDirect(Projectile.Center, 0, 0, dustType, vel.X, vel.Y, 50, default, 3.5f);
                flash.noGravity = true;
            }
            for (int ray = 0; ray < 8; ray++)
            {
                float rayAngle = MathHelper.TwoPi / 8 * ray + energyRotation;
                float rayLength = 300f;
                for (int p = 0; p < 20; p++)
                {
                    float dist = rayLength * (p / 20f);
                    Vector2 rayPos = Projectile.Center + new Vector2(dist, 0).RotatedBy(rayAngle);
                    int dustType = ray % 2 == 0 ? DustID.SolarFlare : DustID.IceTorch;
                    Dust rayDust = Dust.NewDustDirect(rayPos, 0, 0, dustType, 0, 0, 100, default, 2f - p * 0.08f);
                    rayDust.noGravity = true;
                    rayDust.velocity = new Vector2(2, 0).RotatedBy(rayAngle);
                }
            }
            float explosionRadius = 350f;
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < explosionRadius)
                {
                    npc.AddBuff(BuffID.OnFire3, 300);
                    npc.AddBuff(BuffID.Daybreak, 240);
                    npc.AddBuff(BuffID.Frostburn2, 240);
                    npc.AddBuff(ModContent.BuffType<FrostSlowDebuff>(), 180);
                    if (Main.rand.NextBool(3))
                        npc.AddBuff(BuffID.Frozen, 90);
                    Vector2 knockback = npc.Center - Projectile.Center;
                    knockback.Normalize();
                    npc.velocity += knockback * 12f;
                }
            }
            if (Main.LocalPlayer.Distance(Projectile.Center) < 1000f)
            {
                Main.LocalPlayer.GetModPlayer<Screenshake_system>().stronger_shake++;
            }
        }
        private void UpdateOrbitals()
        {
            float orbitSpeed = 0.08f + chargeLevel * 0.12f;
            for (int i = 0; i < 12; i++)
            {
                float direction = i % 2 == 0 ? 1f : -1f;
                orbitalAngles[i] += orbitSpeed * direction;
                float baseDist = 40f + 30f * Projectile.scale;
                orbitalDistances[i] = baseDist + MathF.Sin(Timer * 0.15f + i * 0.5f) * 15f;
            }
        }
        private void PullEnemies(float radius, float strength)
        {
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                if (npc.boss && strength < 5f) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < radius && dist > 30f)
                {
                    Vector2 pull = Projectile.Center - npc.Center;
                    pull.Normalize();
                    float force = strength * (1f - dist / radius);
                    npc.velocity += pull * force * 0.15f;
                }
            }
        }
        private void DamageNearbyEnemies(float radius)
        {
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                if (Vector2.Distance(Projectile.Center, npc.Center) < radius)
                {
                    npc.AddBuff(BuffID.OnFire3, 30);
                    npc.AddBuff(ModContent.BuffType<FrostSlowDebuff>(), 30);
                }
            }
        }
        private void SpawnParticles()
        {
            for (int i = 0; i < 12; i++)
            {
                if (Main.rand.NextBool(3)) continue;
                Vector2 orbPos = Projectile.Center + new Vector2(orbitalDistances[i], 0).RotatedBy(orbitalAngles[i]);
                int dustType = i % 2 == 0 ? DustID.SolarFlare : DustID.IceTorch;
                Dust orb = Dust.NewDustDirect(orbPos, 0, 0, dustType, 0, 0, 100, default, 1.3f * pulseIntensity);
                orb.noGravity = true;
                orb.velocity *= 0.1f;
            }
            for (int spiral = 0; spiral < 2; spiral++)
            {
                float spiralAngle = energyRotation * 3 + MathHelper.Pi * spiral;
                float radius = 20f * Projectile.scale;
                Vector2 spiralPos = Projectile.Center + new Vector2(radius, 0).RotatedBy(spiralAngle);
                int dustType = spiral == 0 ? DustID.SolarFlare : DustID.IceTorch;
                Dust spDust = Dust.NewDustDirect(spiralPos, 0, 0, dustType, 0, 0, 100, default, 1.8f * pulseIntensity);
                spDust.noGravity = true;
                spDust.velocity = new Vector2(2, 0).RotatedBy(spiralAngle + MathHelper.PiOver2);
            }
            if (Main.rand.NextBool(2))
            {
                Dust core = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(10, 10), 0, 0, DustID.PurpleTorch, Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-1.5f, 1.5f), 100, default, 2f * pulseIntensity);
                core.noGravity = true;
            }
            if (chargeLevel > 0.5f && Main.rand.NextBool(4))
            {
                float rayAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                float rayLength = 60f * chargeLevel * Projectile.scale;
                for (int p = 0; p < 6; p++)
                {
                    float dist = rayLength * (p / 6f);
                    Vector2 rayPos = Projectile.Center + new Vector2(dist, 0).RotatedBy(rayAngle);
                    Dust ray = Dust.NewDustDirect(rayPos, 0, 0, DustID.PurpleTorch, 0, 0, 150, default, 1.2f);
                    ray.noGravity = true;
                }
            }
            if (Phase == PHASE_EXPAND && Main.rand.NextBool(2))
            {
                float inAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                float inDist = 150f + Main.rand.NextFloat(100f);
                Vector2 inPos = Projectile.Center + new Vector2(inDist, 0).RotatedBy(inAngle);
                Vector2 toCenter = Projectile.Center - inPos;
                toCenter.Normalize();
                int dustType = Main.rand.NextBool() ? DustID.SolarFlare : DustID.IceTorch;
                Dust inDust = Dust.NewDustDirect(inPos, 0, 0, dustType, toCenter.X * 8, toCenter.Y * 8, 100, default, 1.5f);
                inDust.noGravity = true;
            }
        }
        private void UpdateLighting()
        {
            float intensity = pulseIntensity;
            Lighting.AddLight(Projectile.Center, 1.5f * intensity, 1f * intensity, 1.5f * intensity);
            for (int i = 0; i < 12; i += 3)
            {
                Vector2 orbPos = Projectile.Center + new Vector2(orbitalDistances[i], 0).RotatedBy(orbitalAngles[i]);
                if (i % 2 == 0)
                    Lighting.AddLight(orbPos, 1f, 0.5f, 0.2f);
                else
                    Lighting.AddLight(orbPos, 0.2f, 0.5f, 1f);
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 240);
            target.AddBuff(BuffID.Daybreak, 180);
            target.AddBuff(BuffID.Frostburn2, 180);
            target.AddBuff(ModContent.BuffType<FrostSlowDebuff>(), 150);
            if (Main.rand.NextBool(4))
                target.AddBuff(BuffID.Frozen, 60);
            CreateHitEffect(target.Center);
            SoundEngine.PlaySound(SoundID.Item89 with { Pitch = 0.1f, Volume = 0.6f }, target.Center);
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.ArmorPenetration += 40;
            modifiers.CritDamage += 0.6f;
        }
        private void CreateHitEffect(Vector2 position)
        {
            for (int i = 0; i < 15; i++)
            {
                int dustType = i % 3 == 0 ? DustID.SolarFlare :
                               i % 3 == 1 ? DustID.IceTorch : DustID.PurpleTorch;
                Dust d = Dust.NewDustDirect(position, 0, 0, dustType, Main.rand.NextFloat(-6, 6), Main.rand.NextFloat(-6, 6), 100, default, 2f);
                d.noGravity = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = TextureAssets.Extra[98].Value;
            Texture2D flare = TextureAssets.Extra[89].Value;
            Vector2 pos = Projectile.Center - Main.screenPosition;
            Vector2 glowOrigin = glow.Size() / 2;
            Vector2 flareOrigin = flare.Size() / 2;
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                float alpha = (1f - progress) * 0.5f;
                float scale = Projectile.scale * (1f - progress * 0.6f);
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition;
                Color trailColor;
                if (i % 3 == 0)
                    trailColor = new Color(255, 150, 50) * alpha;
                else if (i % 3 == 1)
                    trailColor = new Color(100, 180, 255) * alpha;
                else
                    trailColor = new Color(200, 100, 255) * alpha;
                sb.Draw(glow, trailPos, null, trailColor, 0, glowOrigin, scale * 1.3f, SpriteEffects.None, 0);
            }
            for (int i = 6; i >= 0; i--)
            {
                float layerScale = (3f + i * 0.7f) * Projectile.scale * pulseIntensity;
                Color layerColor = new Color(255, 150, 50) * (0.1f - i * 0.012f);
                sb.Draw(glow, pos, null, layerColor, energyRotation * 0.15f, glowOrigin, layerScale, SpriteEffects.None, 0);
            }
            for (int i = 5; i >= 0; i--)
            {
                float layerScale = (2f + i * 0.5f) * Projectile.scale * pulseIntensity;
                Color layerColor = new Color(100, 180, 255) * (0.12f - i * 0.018f);
                sb.Draw(glow, pos, null, layerColor, -energyRotation * 0.2f, glowOrigin, layerScale, SpriteEffects.None, 0);
            }
            for (int i = 0; i < 12; i++)
            {
                Vector2 orbPos = pos + new Vector2(orbitalDistances[i], 0).RotatedBy(orbitalAngles[i]);
                Color orbColor = i % 2 == 0 ? new Color(255, 200, 100) * 0.7f : new Color(150, 200, 255) * 0.7f;
                float orbScale = 0.4f * pulseIntensity;
                sb.Draw(glow, orbPos, null, orbColor, 0, glowOrigin, orbScale, SpriteEffects.None, 0);
            }
            float flareScale = Projectile.scale * (0.8f + MathF.Sin(Timer * 0.25f) * 0.3f) * pulseIntensity;
            sb.Draw(flare, pos, null, new Color(255, 180, 80) * 0.8f, energyRotation, flareOrigin, flareScale, SpriteEffects.None, 0);
            sb.Draw(flare, pos, null, new Color(100, 180, 255) * 0.6f, energyRotation + MathHelper.PiOver4, flareOrigin, flareScale * 0.8f, SpriteEffects.None, 0);
            sb.Draw(flare, pos, null, new Color(200, 100, 255) * 0.5f, energyRotation - MathHelper.PiOver4, flareOrigin, flareScale * 0.6f, SpriteEffects.None, 0);
            sb.Draw(glow, pos, null, new Color(180, 100, 220) * 0.9f, 0, glowOrigin, Projectile.scale * 0.8f * pulseIntensity, SpriteEffects.None, 0);
            sb.Draw(glow, pos, null, new Color(220, 180, 255) * 0.95f, 0, glowOrigin, Projectile.scale * 0.4f * pulseIntensity, SpriteEffects.None, 0);
            sb.Draw(glow, pos, null, Color.White * 0.9f, 0, glowOrigin, Projectile.scale * 0.2f * pulseIntensity, SpriteEffects.None, 0);
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
        public override Color? GetAlpha(Color lightColor) => Color.White;
        public override void OnKill(int timeLeft)
        {
            if (Phase != PHASE_EXPLODE)
            {
                for (int i = 0; i < 30; i++)
                {
                    int dustType = i % 3 == 0 ? DustID.SolarFlare :
                                   i % 3 == 1 ? DustID.IceTorch : DustID.PurpleTorch;
                    Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, dustType, Main.rand.NextFloat(-8, 8), Main.rand.NextFloat(-8, 8), 100, default, 2f);
                    d.noGravity = true;
                }
            }
        }
	}
}
