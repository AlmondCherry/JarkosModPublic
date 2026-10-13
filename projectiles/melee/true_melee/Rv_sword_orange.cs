using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.true_melee
{
	public class Rv_sword_orange : ModProjectile
	{
        public enum SolarAttackType
        {
            SolarExplosion, SolarSpear
        }
        public SolarAttackType AttackType
        {
            get => (SolarAttackType)(int)Projectile.ai[0];
            set => Projectile.ai[0] = (float)value;
        }
        public float Timer
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        private Vector2 targetPosition;
        private bool initialized = false;
        private bool hasExploded = false;
        private float chargeLevel = 0f;
        private float pulseTimer = 0f;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 25;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            Timer++;
            pulseTimer += 0.15f;
            if (!initialized)
            {
                targetPosition = Main.MouseWorld;
                initialized = true;
                if (AttackType == SolarAttackType.SolarSpear)
                {
                    Vector2 direction = targetPosition - Projectile.Center;
                    direction.Normalize();
                    Projectile.velocity = direction * 22f;
                    Projectile.rotation = direction.ToRotation();
                }
            }
            if (Timer < 20 && Projectile.owner == Main.myPlayer)
            {
                targetPosition = Main.MouseWorld;
            }
            float pulse = 1f + MathF.Sin(pulseTimer) * 0.1f;
            Projectile.scale = pulse * GetBaseScale();
            switch (AttackType)
            {
                case SolarAttackType.SolarExplosion:
                    AI_SolarExplosion();
                    break;
                case SolarAttackType.SolarSpear:
                    AI_SolarSpear();
                    break;
            }
            SpawnParticles();
            UpdateLighting();
        }
        private void AI_SolarExplosion()
        {
            Vector2 toTarget = targetPosition - Projectile.Center;
            float distance = toTarget.Length();
            Projectile.width = 60;
            Projectile.height = 60;
            if (distance > 40f && !hasExploded)
            {
                float speed = MathHelper.Clamp(10f + Timer * 0.3f, 10f, 20f);
                Vector2 desiredVel = Vector2.Normalize(toTarget) * speed;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVel, 0.15f);
                Projectile.rotation += 0.2f;
                chargeLevel = MathHelper.Clamp(chargeLevel + 0.02f, 0f, 1f);
            }
            else if (!hasExploded)
            {
                hasExploded = true;
                TriggerExplosion();
                Projectile.Kill();
            }
            if (Timer % 15 == 0)
            {
                SoundEngine.PlaySound(SoundID.Item20 with { Pitch = 0.3f, Volume = 0.5f }, Projectile.Center);
            }
        }
        private void AI_SolarSpear()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.penetrate = -1;
            if (Timer < 30)
            {
                Vector2 toTarget = targetPosition - Projectile.Center;
                if (toTarget.Length() > 10f)
                {
                    toTarget.Normalize();
                    Vector2 desiredVel = toTarget * Projectile.velocity.Length();
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVel, 0.08f);
                }
            }
            Projectile.rotation = Projectile.velocity.ToRotation();
            if (Projectile.velocity.Length() < 28f)
            {
                Projectile.velocity *= 1.02f;
            }
            if (Timer % 3 == 0)
            {
                SpawnFireTrail();
            }
        }
        private float GetBaseScale()
        {
            return AttackType switch
            {
                SolarAttackType.SolarExplosion => 1.2f + chargeLevel * 0.3f, SolarAttackType.SolarSpear => 1.0f, _ => 1.0f
            };
        }
        private void TriggerExplosion()
        {
            SoundEngine.PlaySound(SoundID.Item74, Projectile.Center);
            for (int ring = 0; ring < 3; ring++)
            {
                int count = 12 + ring * 6;
                float speed = 4 + ring * 3;
                for (int i = 0; i < count; i++)
                {
                    float angle = MathHelper.TwoPi / count * i;
                    Vector2 vel = new Vector2(speed, 0).RotatedBy(angle);
                    Dust d = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.SolarFlare, vel.X, vel.Y, 100, default, 2.5f - ring * 0.4f);
                    d.noGravity = true;
                }
            }
            for (int i = 0; i < 25; i++)
            {
                Dust flash = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Torch, Main.rand.NextFloat(-8, 8), Main.rand.NextFloat(-8, 8), 50, default, 3f);
                flash.noGravity = true;
            }
            DamageEnemiesInRadius(120f);
        }
        private void DamageEnemiesInRadius(float radius)
        {
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                if (Vector2.Distance(Projectile.Center, npc.Center) < radius)
                {
                    npc.AddBuff(BuffID.OnFire3, 180);
                    npc.AddBuff(BuffID.Daybreak, 120);
                }
            }
        }
        private void SpawnFireTrail()
        {
            Vector2 trailPos = Projectile.Center - Projectile.velocity * 0.5f;
            for (int i = 0; i < 3; i++)
            {
                Dust trail = Dust.NewDustDirect(trailPos + Main.rand.NextVector2Circular(10, 10), 0, 0, DustID.SolarFlare, -Projectile.velocity.X * 0.1f, -Projectile.velocity.Y * 0.1f, 100, default, 1.8f);
                trail.noGravity = true;
            }
        }
        private void SpawnParticles()
        {
            if (AttackType == SolarAttackType.SolarExplosion)
            {
                float angle = pulseTimer * 2;
                Vector2 offset = new Vector2(20 * Projectile.scale, 0).RotatedBy(angle);
                Dust orb = Dust.NewDustDirect(Projectile.Center + offset, 0, 0, DustID.SolarFlare, 0, 0, 100, default, 1.5f);
                orb.noGravity = true;
                orb.velocity *= 0.1f;
                if (Main.rand.NextBool(2))
                {
                    Dust center = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Torch, Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-2, 2), 100, default, 1.3f);
                    center.noGravity = true;
                }
            }
            else
            {
                if (Main.rand.NextBool(2))
                {
                    Dust fire = Dust.NewDustDirect(Projectile.Center - Projectile.velocity * 0.3f + Main.rand.NextVector2Circular(8, 8), 0, 0, DustID.SolarFlare, -Projectile.velocity.X * 0.2f, -Projectile.velocity.Y * 0.2f, 100, default, 1.6f);
                    fire.noGravity = true;
                }
            }
        }
        private void UpdateLighting()
        {
            float intensity = 1f + MathF.Sin(pulseTimer * 2) * 0.2f;
            Lighting.AddLight(Projectile.Center, 1.2f * intensity, 0.7f * intensity, 0.2f * intensity);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 180);
            if (AttackType == SolarAttackType.SolarSpear)
            {
                target.AddBuff(BuffID.Daybreak, 120);
            }
            for (int i = 0; i < 8; i++)
            {
                Dust d = Dust.NewDustDirect(target.Center, 0, 0, DustID.SolarFlare, Main.rand.NextFloat(-4, 4), Main.rand.NextFloat(-4, 4), 100, default, 1.8f);
                d.noGravity = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D glow = TextureAssets.Extra[98].Value;
            Texture2D flare = TextureAssets.Extra[89].Value;
            Texture2D proj = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 pos = Projectile.Center - Main.screenPosition;
            Vector2 glowOrigin = glow.Size() / 2;
            Vector2 flareOrigin = flare.Size() / 2;
            Vector2 projOrigin = proj.Size() / 2;
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                float alpha = (1f - progress) * 0.6f;
                float scale = Projectile.scale * (1f - progress * 0.5f);
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition;
                Color trailColor = Color.Lerp(new Color(255, 220, 100), new Color(255, 100, 20), progress) * alpha;
                sb.Draw(glow, trailPos, null, trailColor, 0, glowOrigin, scale * 1.2f, SpriteEffects.None, 0);
            }
            float pulse = 1f + MathF.Sin(pulseTimer * 2) * 0.2f;
            for (int i = 4; i >= 0; i--)
            {
                float layerScale = (2f + i * 0.6f) * Projectile.scale * pulse;
                Color layerColor = new Color(255, 150, 50) * (0.15f - i * 0.025f);
                sb.Draw(glow, pos, null, layerColor, 0, glowOrigin, layerScale, SpriteEffects.None, 0);
            }
            if (AttackType == SolarAttackType.SolarExplosion)
            {
                float flareScale = Projectile.scale * (0.8f + chargeLevel * 0.5f);
                sb.Draw(flare, pos, null, new Color(255, 180, 80) * 0.7f, pulseTimer, flareOrigin, flareScale, SpriteEffects.None, 0);
            }
            sb.Draw(glow, pos, null, new Color(255, 220, 150) * 0.9f, 0, glowOrigin, Projectile.scale * 0.5f, SpriteEffects.None, 0);
            sb.Draw(glow, pos, null, Color.White * 0.8f, 0, glowOrigin, Projectile.scale * 0.25f, SpriteEffects.None, 0);
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
        public override Color? GetAlpha(Color lightColor) => Color.White;
	}
}
