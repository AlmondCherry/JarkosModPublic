using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using jarkos;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.Audio;
namespace Jarkos.projectiles.bullet
{
    public class PhaseBloodMarker : ModProjectile
    {
       private enum MarkerState
        {
            Idle, WaitingDetonate, Detonating, Dead
        }
        private MarkerState state = MarkerState.Idle;
        private float pulseScale = 1f;
        private float pulseTimer = 0f;
        private float glowAlpha = 0f;
        private float spawnScale = 0f;
        private float rotation = 0f;
        private int detonateTimer = 0;
        private const int DETONATE_DURATION = 12;
        private const float EXPLOSION_RADIUS = 120f;
        private float detonateScale = 1f;
        private bool hasDealtDamage = false;
        private int detonationDelay = 0;
        private int detonationDelayTimer = 0;
        private const float HEAL_PER_HIT = 5f;
        private const float HEAL_PER_HIT_LOW_HP = 10f;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;
        }
        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 300;
            Projectile.DamageType = DamageClass.Generic;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.alpha = 255;
        }
        public void SetDetonationDelay(int delay)
        {
            if (state == MarkerState.Idle)
            {
                state = MarkerState.WaitingDetonate;
                detonationDelay = delay;
                detonationDelayTimer = 0;
            }
        }
        public void Detonate()
        {
            if (state != MarkerState.Detonating && state != MarkerState.Dead)
            {
                state = MarkerState.Detonating;
                detonateTimer = 0;
                hasDealtDamage = false;
                SoundEngine.PlaySound(SoundID.Item74 with { Volume = 0.6f, Pitch = 0.5f }, Projectile.Center);
            }
        }
        public override void AI()
        {
            switch (state)
            {
                case MarkerState.Idle:
                    AI_Idle();
                    break;
                case MarkerState.WaitingDetonate:
                    detonationDelayTimer++;
                    AI_Idle();
                    pulseTimer += 0.15f;
                    if (detonationDelayTimer >= detonationDelay)
                        Detonate();
                    break;
                case MarkerState.Detonating:
                    AI_Detonating();
                    break;
                case MarkerState.Dead:
                    Projectile.Kill();
                    break;
            }
        }
        private void AI_Idle()
        {
            spawnScale = MathHelper.Lerp(spawnScale, 1f, 0.15f);
            Projectile.alpha = (int)(255 * (1f - spawnScale));
            pulseTimer += 0.12f;
            pulseScale = 1f + (float)Math.Sin(pulseTimer) * 0.15f;
            glowAlpha = MathHelper.Lerp(glowAlpha, 0.6f, 0.05f);
            rotation += 0.02f;
            if (Main.rand.NextBool(3))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float dist = Main.rand.NextFloat(15f, 25f);
                Vector2 dustPos = Projectile.Center + new Vector2((float)Math.Cos(angle) * dist, (float)Math.Sin(angle) * dist);
                Dust d = Dust.NewDustDirect(dustPos, 4, 4, DustID.Blood);
                d.velocity = (Projectile.Center - dustPos) * 0.08f;
                d.noGravity = true;
                d.scale = 0.8f * spawnScale;
                d.color = new Color(255, 80, 80, 150);
            }
            if (Main.rand.NextBool(5))
            {
                Dust center = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(4f, 4f), 2, 2, DustID.GoldFlame);
                center.velocity = Vector2.Zero;
                center.noGravity = true;
                center.scale = 0.6f;
                center.color = new Color(255, 200, 100);
            }
            if (Projectile.timeLeft < 30)
                Projectile.alpha = (int)(255 * (1f - (float)Projectile.timeLeft / 30f));
        }
        private void AI_Detonating()
        {
            detonateTimer++;
            float progress = (float)detonateTimer / DETONATE_DURATION;
            detonateScale = 1f + EaseOutQuad(progress) * 4f;
            if (detonateTimer == DETONATE_DURATION / 3 && !hasDealtDamage)
            {
                hasDealtDamage = true;
                DealExplosionDamage();
            }
            SpawnExplosionParticles(progress);
            if (progress > 0.5f)
                Projectile.alpha = (int)(255 * ((progress - 0.5f) / 0.5f));
            if (detonateTimer >= DETONATE_DURATION)
                state = MarkerState.Dead;
        }
        public bool AnyBossAlive()
        {
            foreach(NPC n in Main.ActiveNPCs)
            {
                if(n.boss)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }
        private void DealExplosionDamage()
        {
            Player owner = Main.player[Projectile.owner];
            float healthRatio = (float)owner.statLife / owner.statLifeMax2;
            bool isLowHP = healthRatio < 0.25f;
            int totalHits = 0;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.immortal)
                    continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist > EXPLOSION_RADIUS)
                    continue;
                float damageMult = 1f - (dist / EXPLOSION_RADIUS) * 0.5f;
                int damage = (int)(Projectile.damage * damageMult);
                Vector2 hitDir = (npc.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
                owner.ApplyDamageToNPC(npc, damage, Projectile.knockBack, (int)hitDir.X, false);
                totalHits++;
                SpawnHitEffect(npc.Center);
            }
            if (totalHits > 0)
            {
                float healPerHit = isLowHP ? HEAL_PER_HIT_LOW_HP : HEAL_PER_HIT;
                int totalHeal = (int)(healPerHit * totalHits);
                totalHeal = Math.Min(totalHeal, owner.statLifeMax2 - owner.statLife);
                if (totalHeal > 0 && owner.statLife < owner.statLifeMax2)
                {
                    owner.Heal(totalHeal);
                    SpawnHealParticles(owner.Center, totalHits);
                }
            }
        }
        private void SpawnExplosionParticles(float progress)
        {
            float radius = EXPLOSION_RADIUS * EaseOutQuad(progress);
            int count = (int)(16 * (1f - progress));
            for (int i = 0; i < count; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float dist = radius * Main.rand.NextFloat(0.5f, 1f);
                Vector2 pos = Projectile.Center + new Vector2((float)Math.Cos(angle) * dist, (float)Math.Sin(angle) * dist);
                Dust blood = Dust.NewDustDirect(pos, 6, 6, DustID.Blood);
                blood.velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(2f, 6f);
                blood.noGravity = true;
                blood.scale = Main.rand.NextFloat(1.2f, 2.5f) * (1f - progress);
                blood.color = Color.Lerp(new Color(255, 30, 30), new Color(255, 150, 50), progress);
                blood.fadeIn = 1.5f;
            }
            if (detonateTimer % 2 == 0)
            {
                int ringCount = 20;
                for (int i = 0; i < ringCount; i++)
                {
                    float angle = MathHelper.TwoPi / ringCount * i;
                    Vector2 pos = Projectile.Center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                    Dust ring = Dust.NewDustDirect(pos, 2, 2, DustID.GoldFlame);
                    ring.velocity = Vector2.Zero;
                    ring.noGravity = true;
                    ring.scale = 1f * (1f - progress);
                    ring.color = new Color(255, 200, 100);
                }
            }
            if (detonateTimer <= 3)
            {
                for (int i = 0; i < 10; i++)
                {
                    Dust flash = Dust.NewDustDirect(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), 4, 4, DustID.MagicMirror);
                    flash.velocity = Main.rand.NextVector2Circular(4f, 4f);
                    flash.noGravity = true;
                    flash.scale = 2f;
                    flash.color = new Color(255, 100, 100);
                }
            }
        }
        private void SpawnHitEffect(Vector2 hitPos)
        {
            for (int i = 0; i < 12; i++)
            {
                Dust d = Dust.NewDustDirect(hitPos, 6, 6, DustID.Blood);
                d.velocity = Main.rand.NextVector2Circular(4f, 4f);
                d.noGravity = true;
                d.scale = 1.5f;
            }
            for (int i = 0; i < 6; i++)
            {
                Dust flash = Dust.NewDustDirect(hitPos, 4, 4, DustID.GoldFlame);
                flash.velocity = Main.rand.NextVector2Circular(2f, 2f);
                flash.noGravity = true;
                flash.scale = 1f;
                flash.color = new Color(255, 80, 80);
            }
        }
        private void SpawnHealParticles(Vector2 playerPos, int hitCount)
        {
            int count = Math.Min(hitCount * 4, 20);
            for (int i = 0; i < count; i++)
            {
                Vector2 startPos = Projectile.Center + Main.rand.NextVector2Circular(30f, 30f);
                Vector2 toPlayer = (playerPos - startPos).SafeNormalize(Vector2.Zero);
                Dust heal = Dust.NewDustDirect(startPos, 4, 4, DustID.GreenFairy);
                heal.velocity = toPlayer * Main.rand.NextFloat(6f, 12f);
                heal.noGravity = true;
                heal.scale = 1.3f;
                heal.color = new Color(100, 255, 100);
                heal.fadeIn = 1.2f;
            }
        }
        private float EaseOutQuad(float t)
        {
            return 1f - (1f - t) * (1f - t);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.End();
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float currentAlpha = 1f - Projectile.alpha / 255f;
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
            if (state == MarkerState.Idle || state == MarkerState.WaitingDetonate)
            {
                float scale = spawnScale * pulseScale;
                Color outerGlow = new Color(255, 30, 30, 0) * glowAlpha * currentAlpha * 0.3f;
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi / 6f * i + rotation;
                    Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 5f;
                    Main.EntitySpriteDraw(texture, drawPos + offset, null, outerGlow, rotation, origin, scale * 1.5f, SpriteEffects.None, 0);
                }
                if (state == MarkerState.WaitingDetonate)
                {
                    Color waitGlow = new Color(255, 200, 50, 0) * currentAlpha * 0.4f;
                    Main.EntitySpriteDraw(texture, drawPos, null, waitGlow, rotation, origin, scale * 1.4f, SpriteEffects.None, 0);
                }
                Color midGlow = new Color(255, 80, 50, 0) * glowAlpha * currentAlpha * 0.5f;
                Main.EntitySpriteDraw(texture, drawPos, null, midGlow, rotation, origin, scale * 1.3f, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(texture, drawPos, null, Color.White * currentAlpha, rotation, origin, scale, SpriteEffects.None, 0);
                Color coreColor = new Color(255, 220, 200, 0) * currentAlpha * 0.7f;
                Main.EntitySpriteDraw(texture, drawPos, null, coreColor, -rotation * 0.5f, origin, scale * 0.5f, SpriteEffects.None, 0);
                DrawOrbitingDots(drawPos, scale, currentAlpha);
            }
            else if (state == MarkerState.Detonating)
            {
                float progress = (float)detonateTimer / DETONATE_DURATION;
                Color explosionColor = Color.Lerp(new Color(255, 50, 30), new Color(255, 200, 50), progress) * currentAlpha * (1f - progress);
                Main.EntitySpriteDraw(texture, drawPos, null, explosionColor, rotation + progress * MathHelper.Pi, origin, detonateScale, SpriteEffects.None, 0);
                if (progress < 0.5f)
                {
                    Color coreFlash = Color.White * currentAlpha * (1f - progress * 2f);
                    Main.EntitySpriteDraw(texture, drawPos, null, coreFlash, 0f, origin, detonateScale * 0.3f, SpriteEffects.None, 0);
                }
            }
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
            return false;
        }
        private void DrawOrbitingDots(Vector2 center, float scale, float alpha)
        {
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            for (int i = 0; i < 4; i++)
            {
                float angle = rotation * 2f + MathHelper.TwoPi / 4f * i;
                float orbitRadius = 18f * scale;
                Vector2 dotPos = center + new Vector2((float)Math.Cos(angle) * orbitRadius, (float)Math.Sin(angle) * orbitRadius);
                Rectangle dotRect = new Rectangle((int)dotPos.X - 2, (int)dotPos.Y - 2, 4, 4);
                Main.spriteBatch.Draw(pixel, dotRect, new Color(255, 150, 100) * alpha * 0.8f);
            }
        }
        public override bool? CanDamage() => false;
    }
}
