using jarkos.buffs.debuff;
using jarkos.dusts;
using jarkos.particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.other_melee
{
    public class Darkped_trueproj : ModProjectile
    {
        private enum DiscState
        {
            Flying = 0, Returning = 1
        }
        private ref float StateValue => ref Projectile.ai[0];
        private ref float Timer => ref Projectile.ai[1];
        private DiscState CurrentState
        {
            get => (DiscState)StateValue;
            set => StateValue = (float)value;
        }
        private bool hitOnReturn = false;
        private float shakeIntensity = 0f;
        private float glowPulse = 0f;
        private int soundTimer = 0;
        private int hitCount = 0;
        private List<int> hitNPCs = new List<int>();
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 24;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
        }
        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 900;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = -1;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }
            Timer++;
            soundTimer++;
            glowPulse += 0.15f;
            float rotationSpeed = CurrentState == DiscState.Returning && hitOnReturn ? 0.6f : 0.35f;
            Projectile.rotation += rotationSpeed * Math.Sign(Projectile.velocity.X + 0.01f);
            switch (CurrentState)
            {
                case DiscState.Flying:
                    HandleFlyingState(owner);
                    break;
                case DiscState.Returning:
                    HandleReturningState(owner);
                    break;
            }
            SpawnAmbientParticles();
            SpreadInfectionAura();
            EmitLight();
        }
        private void HandleFlyingState(Player owner)
        {
            if (Timer > 25)
            {
                Projectile.velocity *= 0.975f;
            }
            if (Timer < 20 && Main.myPlayer == Projectile.owner)
            {
                Vector2 toMouse = Main.MouseWorld - Projectile.Center;
                if (toMouse.Length() > 50f)
                {
                    toMouse = toMouse.SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toMouse * Projectile.velocity.Length(), 0.02f);
                }
            }
            bool tooSlow = Projectile.velocity.Length() < 3f;
            bool tooFar = Vector2.Distance(Projectile.Center, owner.Center) > 800f;
            bool timedOut = Timer > 50;
            if (tooSlow || tooFar || timedOut)
            {
                TransitionToReturning();
            }
        }
        private void HandleReturningState(Player owner)
        {
            Vector2 returnDirection = owner.Center - Projectile.Center;
            float distance = returnDirection.Length();
            if (distance < 40f)
            {
                CatchDisc(owner);
                return;
            }
            returnDirection.Normalize();
            float returnSpeed = hitOnReturn ? 28f : 20f;
            float lerpFactor = hitOnReturn ? 0.15f : 0.08f;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, returnDirection * returnSpeed, lerpFactor);
            if (hitOnReturn)
            {
                ApplyShakeEffect();
            }
        }
        private void ApplyShakeEffect()
        {
            shakeIntensity = MathHelper.Lerp(shakeIntensity, 8f, 0.08f);
            Vector2 shakeOffset = new Vector2(Main.rand.NextFloat(-shakeIntensity, shakeIntensity), Main.rand.NextFloat(-shakeIntensity, shakeIntensity));
            Projectile.position += shakeOffset;
            if (Main.rand.NextBool(16))
            {
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(20f, 20f);
                Dust shakeDust = Dust.NewDustDirect(dustPos, 0, 0, ModContent.DustType<Square_dust>(), shakeOffset.X * 0.5f, shakeOffset.Y * 0.5f, 150, Color.Crimson, 0.4f);
                shakeDust.noGravity = true;
                shakeDust.fadeIn = 1.5f;
            }
        }
        private void TransitionToReturning()
        {
            CurrentState = DiscState.Returning;
            Projectile.tileCollide = false;
            hitNPCs.Clear();

            for (int i = 0; i < 2; i++)
            {
                float angle = MathHelper.TwoPi / 20f * i;
                Vector2 dustVel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 4f;
                Dust turnDust = Dust.NewDustDirect(Projectile.Center, 0, 0, ModContent.DustType<Square_dust>(), dustVel.X, dustVel.Y, 100, Color.Red, 0.5f);
                turnDust.noGravity = true;
            }
        }
        private void CatchDisc(Player owner)
        {
            for (int i = 0; i < 3; i++)
            {
                Dust catchDust = Dust.NewDustDirect(owner.Center, 0, 0, ModContent.DustType<Square_dust>(), 0, 0, 100, Color.Red, 0.4f);
                catchDust.velocity = Main.rand.NextVector2Circular(4f, 4f);
                catchDust.noGravity = true;
            }
            Projectile.Kill();
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            hitCount++;
            int debuffDuration = CurrentState == DiscState.Returning ? 600 : 420;
            target.AddBuff(ModContent.BuffType<True_plague>(), debuffDuration);
            target.GetGlobalNPC<SpreaderPlagueNPC>().darkpedSlashTimer = 70;
            if (CurrentState == DiscState.Flying)
            {
                CreateExplosion(100f, 1.0f, false);
                PlayHitSounds(false);
            }
            else
            {
                if (!hitOnReturn)
                {
                    hitOnReturn = true;
                    shakeIntensity = 3f;
                    Projectile.damage = (int)(Projectile.damage * 1.6f);
                }
                CreateExplosion(180f, 2.2f, true);
                PlayHitSounds(true);
            }

            int count = Main.rand.Next(1, 3);
                for (int i = 0; i < count; i++)
                {
                    float radius = MathF.Max(target.width, target.height) * 0.7f + Main.rand.NextFloat(45f, 85f);
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 spawnOffset = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius;
                    Vector2 spawnPos = target.Center + spawnOffset;

                    Vector2 dirToCenter = -spawnOffset.SafeNormalize(Vector2.UnitX);
                    float speed = Main.rand.NextFloat(8f, 12f);
                    Vector2 velocity = dirToCenter * speed;

                    new SlashSparkParticle(spawnPos, velocity, Color.Red, 35, Main.rand.NextFloat(0.7f, 0.7f), 3.5f).Spawn();
                }
        }
        private void PlayHitSounds(bool powerfulHit)
        {
            if (powerfulHit)
            {
                SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/DPhit") with { Volume = 0.6f, Pitch = Main.rand.NextFloat(-0.15f, 0.15f)}, Projectile.position);
                SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Volume = 0.25f }, Projectile.position);
            }
            else
            {
                SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/DPhit") with { Volume = 0.6f, Pitch = Main.rand.NextFloat(-0.15f, 0.15f)}, Projectile.position);
            }
        }
        private void CreateExplosion(float radius, float intensity, bool isPowerful)
        {
            int particleCount = (int)(2 * intensity);
            for (int i = 0; i < particleCount; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(radius * 0.15f, radius * 0.15f);
                Dust explosionDust = Dust.NewDustDirect(Projectile.Center - new Vector2(4, 4), 8, 8, ModContent.DustType<Square_dust>(), vel.X, vel.Y, 80, Color.Crimson, 0.2f * intensity);
                explosionDust.noGravity = true;
                explosionDust.fadeIn = 1.8f;
            }
            int ringCount = isPowerful ? 8 : 4;
            for (int i = 0; i < ringCount; i++)
            {
                float angle = MathHelper.TwoPi / ringCount * i;
                float speed = (6f + Main.rand.NextFloat(2f)) * intensity;
                Vector2 ringVel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
                Dust ringDust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Blood, ringVel.X, ringVel.Y, 0, default, 1.2f * intensity);
                ringDust.noGravity = true;
                ringDust.fadeIn = 1.5f;
            }
            if (isPowerful)
            {
                for (int i = 0; i < 20; i++)
                {
                    float angle = MathHelper.TwoPi / 16 * i + MathHelper.PiOver4;
                    Vector2 innerVel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 10f;
                    Dust innerDust = Dust.NewDustDirect(Projectile.Center, 0, 0, DustID.Torch, innerVel.X, innerVel.Y, 0, Color.OrangeRed, 2f);
                    innerDust.noGravity = true;
                    new Spark(Projectile.Center, innerVel, Color.IndianRed, 250, 0.6f, 1.8f, 0f).Spawn();
                }
            }
            SpreadInfectionInRadius(radius, intensity);
            if (isPowerful && Main.myPlayer == Projectile.owner)
            {
                Main.player[Projectile.owner].GetModPlayer<Screenshake_system>().stronger_shake++;
                float explosionRadius = 250f * Projectile.scale;
                foreach (NPC npc in Main.npc)
                {
                    if (npc.active && !npc.friendly && !npc.dontTakeDamage)
                    {
                        float distance = Vector2.Distance(npc.Center, Projectile.Center);
                        if (distance < explosionRadius)
                        {
                            float damageMultiplier = 1f - (distance / explosionRadius) * 0.5f;
                            int damage = (int)(Projectile.damage * damageMultiplier);
                            Vector2 hitDirection = (npc.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
                            npc.SimpleStrikeNPC(damage, hitDirection.X > 0 ? 1 : -1, false, 15f);
                        }
                    }
                }
            }
        }
        private void SpreadInfectionInRadius(float radius, float intensity)
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.lifeMax <= 5) continue;
                float distance = Vector2.Distance(Projectile.Center, npc.Center);
                if (distance < radius)
                {
                    float distanceFactor = 1f - (distance / radius);
                    int duration = (int)(300 * intensity * distanceFactor);
                    npc.AddBuff(ModContent.BuffType<True_plague>(), duration);
                    int dustCount = (int)(8 * distanceFactor);
                    for (int j = 0; j < dustCount; j++)
                    {
                        float progress = (float)j / dustCount;
                        Vector2 dustPos = Vector2.Lerp(Projectile.Center, npc.Center, progress);
                        dustPos += Main.rand.NextVector2Circular(16f, 16f);
                        Dust lineDust = Dust.NewDustDirect(dustPos, 0, 0, ModContent.DustType<Square_dust>(), 0, 0, 120, Color.Red, 0.4f);
                        lineDust.noGravity = true;
                        lineDust.fadeIn = 1.2f;
                    }
                }
            }
        }
        private void SpreadInfectionAura()
        {
            if ((int)Timer % 12 != 0) return;
            float auraRadius = 100f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly || npc.lifeMax <= 5) continue;
                float distance = Vector2.Distance(Projectile.Center, npc.Center);
                if (distance < auraRadius)
                {
                    npc.AddBuff(ModContent.BuffType<True_plague>(), 120);
                    if (Main.rand.NextBool(3))
                    {
                        Vector2 dustPos = Vector2.Lerp(Projectile.Center, npc.Center, Main.rand.NextFloat(0.3f, 0.7f));
                        Dust auraDust = Dust.NewDustDirect(dustPos, 0, 0, ModContent.DustType<Square_dust>(), 0, 0, 150, Color.DarkRed, 0.6f);
                        auraDust.noGravity = true;
                    }
                }
            }
        }
        private void SpawnAmbientParticles()
        {
            if (Main.rand.NextBool(12))
            {
                Dust trailDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<Square_dust>(), Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f, 100, Color.Crimson, 0.4f);
                trailDust.noGravity = true;
                trailDust.fadeIn = 1.3f;
            }
            if (Main.rand.NextBool(14))
            {
                Dust bloodDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Blood, 0, 0, 120, default, 1f);
                bloodDust.velocity = Main.rand.NextVector2Circular(2f, 2f);
                bloodDust.noGravity = true;
            }
            if (Main.rand.NextBool(16))
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(15f, 25f);
                Dust orbitDust = Dust.NewDustDirect(Projectile.Center + offset, 0, 0, DustID.RedTorch, 0, 0, 0, Color.Red, 0.9f);
                orbitDust.noGravity = true;
                orbitDust.velocity = new Vector2(-offset.Y, offset.X) * 0.05f;
            }
        }
        private void EmitLight()
        {
            float pulse = 0.85f + (float)Math.Sin(glowPulse) * 0.15f;
            float intensity = hitOnReturn ? 1.3f : 1f;
            Lighting.AddLight(Projectile.Center, 1.2f * pulse * intensity, 0.25f * pulse * intensity, 0.2f * pulse * intensity);
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (CurrentState == DiscState.Flying)
            {
                Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
                SoundEngine.PlaySound(SoundID.Dig with {
                    Volume = 0.7f, Pitch = -0.1f
                }, Projectile.position);
                CreateExplosion(50f, 0.5f, false);
                TransitionToReturning();
            }
            return false;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;

            DrawTrail(texture, origin);
            DrawGlow(texture, origin);

            Color drawColor = Color.White;
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, drawColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
        private void DrawTrail(Texture2D texture, Vector2 origin)
        {
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = 1f - (float)i / Projectile.oldPos.Length;
                Color trailColor = Color.Red * progress * 0.6f;
                trailColor.A = 0;
                float scale = Projectile.scale * progress * (hitOnReturn ? 1.1f : 0.9f);
                Vector2 drawPos = Projectile.oldPos[i] + origin - Main.screenPosition;
                Main.EntitySpriteDraw(texture, drawPos, null, trailColor, Projectile.oldRot[i], origin, scale, SpriteEffects.None, 0);
            }
        }
        private void DrawGlow(Texture2D texture, Vector2 origin)
        {
            float glowIntensity = hitOnReturn ? 0.5f : 0.3f;
            float pulseScale = 1f + (float)Math.Sin(glowPulse * 2) * 0.1f;
            Color glowColor = Color.Red * glowIntensity;
            glowColor.A = 0;
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi / 6 * i + glowPulse * 0.5f;
                Vector2 offset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * (3f * pulseScale);
                Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + offset, null, glowColor, Projectile.rotation, origin, Projectile.scale * 1.1f * pulseScale, SpriteEffects.None, 0);
            }
        }
    }
}
