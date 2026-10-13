using jarkos.dusts;
using jarkos.particles;
using jarkos.projectiles.melee.other_melee;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.other_melee
{
    public class Darkpedproj : ModProjectile
    {
        private enum AxeState
        {
            FlyingOut = 0, Returning = 1
        }
        private ref float SpreadIndex => ref Projectile.ai[0];
        private ref float StateTimer => ref Projectile.ai[1];
        private ref float CurrentState => ref Projectile.localAI[0];
        private ref float PulseTimer => ref Projectile.localAI[1];
        private const float MAX_DISTANCE = 1350f;
        private const float RETURN_SPEED = 18f;
        private const float PULSE_SPEED = 0.55f;
        private const int TRAIL_LENGTH = 24;
        private Vector2[] trailPositions = new Vector2[TRAIL_LENGTH];
        private float[] trailAlphas = new float[TRAIL_LENGTH];
        private bool trailInitialized = false;
        private int pulseSoundTimer = 0;
        private bool returnSoundPlayed = false;
        private float random_degree_returning = 2f;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TRAIL_LENGTH;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            Main.projFrames[Projectile.type] = 1;
        }
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Throwing;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
            Projectile.alpha = 50;
            random_degree_returning = Math.Sign(Main.rand.NextFloat(-1f, 1f)) * 3f;
        }
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead)
            {
                Projectile.Kill();
                return;
            }
            if (!trailInitialized)
            {
                for (int i = 0; i < TRAIL_LENGTH; i++)
                {
                    trailPositions[i] = Projectile.Center;
                    trailAlphas[i] = 1f - (i / (float)TRAIL_LENGTH);
                }
                trailInitialized = true;
            }
            UpdateTrail();
            UpdatePulse();
            Projectile.rotation += (CurrentState == (float)AxeState.Returning ? 0.4f : 0.25f)
                * Projectile.direction;
            StateTimer++;
            if (CurrentState == (float)AxeState.FlyingOut)
            {
                FlyingOutBehavior(owner);
            }
            else
            {
                ReturningBehavior(owner);
            }
            SpawnParticles();
            float lightIntensity = CurrentState == (float)AxeState.Returning ? 1.2f : 0.7f;
            float pulse = (float)Math.Sin(PulseTimer) * 0.3f + 0.7f;
            Lighting.AddLight(Projectile.Center, 0.4f * lightIntensity * pulse, 0.1f * lightIntensity * pulse, 0.8f * lightIntensity * pulse);
        }
        private void UpdateTrail()
        {
            for (int i = TRAIL_LENGTH - 1; i > 0; i--)
            {
                trailPositions[i] = trailPositions[i - 1];
                trailAlphas[i] = trailAlphas[i - 1] * 0.9f;
            }
            trailPositions[0] = Projectile.Center;
            trailAlphas[0] = 1f;
        }
        private void UpdatePulse()
        {
            PulseTimer += PULSE_SPEED;
            pulseSoundTimer++;
            if (pulseSoundTimer >= 20)
            {
                pulseSoundTimer = 0;
                SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/toxic_pistol_rotating") with
                {
                    Volume = 0.3f, Pitch = CurrentState == (float)AxeState.Returning ? 0.3f : 0f, MaxInstances = 3
                }, Projectile.Center);
            }
        }
        private void FlyingOutBehavior(Player owner)
        {
            Projectile.velocity *= 0.985f;
            float distanceFromPlayer = Vector2.Distance(Projectile.Center, owner.Center);
            if (distanceFromPlayer > MAX_DISTANCE || Projectile.velocity.Length() < 3f || StateTimer > 80)
            {
                CurrentState = (float)AxeState.Returning;
                StateTimer = 0;
                if (!returnSoundPlayed)
                {
                    returnSoundPlayed = true;
                    SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/toxic_pistol_rotating") with
                    {
                        Volume = 0.9f, Pitch = 0.2f
                    }, Projectile.Center);
                    for (int i = 0; i < 15; i++)
                    {
                        Vector2 dustVel = Main.rand.NextVector2Circular(5f, 5f);
                        Dust dust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Square_dust>(), dustVel);
                        dust.noGravity = true;
                        dust.scale = Main.rand.NextFloat(0.1f, 0.2f);
                    }
                }
            }
        }
        private void ReturningBehavior(Player owner)
        {
            Vector2 returnDirection = owner.Center - Projectile.Center;
            float distance = returnDirection.Length();
            if (distance < 30f)
            {
                Projectile.Kill();
                SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/toxic_pistol_rotating") with
                {
                    Volume = 0.6f, Pitch = 0.5f
                }, owner.Center);
                return;
            }
            returnDirection.Normalize();
            float speed = MathHelper.Clamp(RETURN_SPEED + (StateTimer * 0.05f), RETURN_SPEED, 28f);
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, returnDirection * speed, 0.15f).RotatedBy(MathHelper.ToRadians(random_degree_returning));
            if (Main.rand.NextBool(2))
            {
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(10f, 10f);
                Dust dust = Dust.NewDustPerfect(dustPos, ModContent.DustType<Square_dust>(), -Projectile.velocity * 0.3f);
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.1f, 0.2f);
            }
        }
        private void SpawnParticles()
        {
            bool isReturning = CurrentState == (float)AxeState.Returning;
            int particleChance = isReturning ? 4 : 6;
            if (Main.rand.NextBool(particleChance))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(15f, 15f), ModContent.DustType<Square_dust>(), -Projectile.velocity * 0.2f, 0, new Color(150, 50, 200), isReturning ? 0.2f : 0.1f);
                dust.noGravity = true;
                dust.fadeIn = 1.2f;
                if (Main.rand.NextBool(4))
                {
                    Dust glowDust = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<Square_dust>(), Main.rand.NextVector2Circular(2f, 2f), 0, new Color(150, 50, 200), isReturning ? 0.2f : 0.1f);
                    glowDust.noGravity = true;
                }
            }
            if (isReturning && Main.rand.NextBool(2))
            {
                Vector2 trailPos = Projectile.Center - Projectile.velocity.SafeNormalize(Vector2.Zero) * 20f;
                Dust trailDust = Dust.NewDustPerfect(trailPos, ModContent.DustType<Square_dust>(), Vector2.Zero, 100, new Color(150, 50, 200), 0.3f);
                trailDust.noGravity = true;
            }
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (CurrentState == (float)AxeState.Returning)
            {
                modifiers.FinalDamage *= 1.75f;
                modifiers.Knockback *= 1.5f;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SpawnExplosion(target.Center);
            SoundEngine.PlaySound(SoundID.NPCHit1 with { Pitch = -0.9f }, Projectile.position);
            SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.5f, Volume = 0.5f }, Projectile.position);
            if (CurrentState == (float)AxeState.Returning)
            {
                SpawnExplosion(target.Center, true);
                SoundEngine.PlaySound(SoundID.NPCHit1 with { Pitch = -0.6f }, Projectile.position);
                SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.7f, Volume = 0.7f }, Projectile.position);
            }
        }
        private void SpawnExplosion(Vector2 position, bool enhanced = false)
        {
             int dustCount = enhanced ? 30 : 15;
            float dustScale = enhanced ? 2.5f : 1.8f;
            for (int i = 0; i < dustCount; i++)
            {
                float angle = MathHelper.TwoPi * i / dustCount;
                Vector2 dustVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 8f);
                Dust dust = Dust.NewDustPerfect(position, DustID.ShadowbeamStaff, dustVel);
                dust.noGravity = true;
                dust.scale = dustScale * Main.rand.NextFloat(0.8f, 1.2f);
                dust.fadeIn = 1.5f;
                new Spark(position, dustVel, Color.MediumPurple, 250, 0.5f, 1.4f, 0f).Spawn();
                if (enhanced && Main.rand.NextBool(2))
                {
                    Dust dust2 = Dust.NewDustPerfect(position, DustID.Shadowflame, dustVel * 0.7f);
                    dust2.noGravity = true;
                    dust2.scale = dustScale * 1.3f;
                }
            }
             }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Texture2D glowTexture = ModContent.Request<Texture2D>("jarkos/projectiles/melee/other_melee/darkpedproj_glow").Value;
            Texture2D trailTexture = ModContent.Request<Texture2D>("jarkos/projectiles/melee/other_melee/darkpedproj_trail").Value;
            bool isReturning = CurrentState == (float)AxeState.Returning;
            float pulse = (float)Math.Sin(PulseTimer) * 0.15f + 1f;
            float baseScale = isReturning ? 0.9f : 0.9f;
            float scale = baseScale * pulse;
            Vector2 drawOrigin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Color baseColor = new Color(80, 20, 120);
            Color glowColor = isReturning
                ? new Color(180, 80, 255, 0)
                : new Color(120, 40, 180, 0);
            Color trailColor = isReturning
                ? new Color(150, 50, 220, 100)
                : new Color(100, 30, 150, 80);
            DrawTrail(spriteBatch, trailTexture, trailColor, isReturning);
            DrawBackGlow(spriteBatch, glowTexture, glowColor, scale, isReturning);
            for (int i = 0; i < 4; i++)
            {
                float layerScale = scale * (1f + i * 0.15f);
                float layerAlpha = (1f - i * 0.2f) * (isReturning ? 0.7f : 0.5f);
                Color layerColor = glowColor * layerAlpha;
                spriteBatch.Draw(texture, drawPos, null, layerColor, Projectile.rotation, drawOrigin, layerScale, SpriteEffects.None, 0f);
            }
            Color mainColor = Color.Lerp(baseColor, Color.White, 0.2f * pulse);
            mainColor.A = (byte)(isReturning ? 220 : 180);
            spriteBatch.Draw(texture, drawPos, null, mainColor, Projectile.rotation, drawOrigin, scale, SpriteEffects.None, 0f);
            if (isReturning)
            {
                Color coreColor = new Color(255, 200, 255, 0) * (0.5f + pulse * 0.3f);
                spriteBatch.Draw(texture, drawPos, null, coreColor, Projectile.rotation, drawOrigin, scale * 0.7f, SpriteEffects.None, 0f);
            }
            return false;
        }
        private void DrawTrail(SpriteBatch spriteBatch, Texture2D trailTexture, Color trailColor, bool isReturning)
        {
            if (Projectile.oldPos.Length < 2) return;
            Vector2 drawOrigin = trailTexture.Size() / 2f;
            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = 1f - (i / (float)Projectile.oldPos.Length);
                float trailScale = progress * (isReturning ? 0.9f : 0.9f);
                float trailAlpha = progress * (isReturning ? 0.8f : 0.5f);
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Color color = trailColor * trailAlpha;
                float trailPulse = (float)Math.Sin(PulseTimer + i * 0.3f) * 0.2f + 1f;
                trailScale *= trailPulse;
                spriteBatch.Draw(trailTexture, trailPos, null, color, Projectile.oldRot[i], drawOrigin, trailScale, SpriteEffects.None, 0f);
            }
        }
        private void DrawBackGlow(SpriteBatch spriteBatch, Texture2D glowTexture, Color glowColor, float scale, bool isReturning)
        {
            Vector2 drawOrigin = glowTexture.Size() / 2f;
            Vector2 backOffset = -Projectile.velocity.SafeNormalize(Vector2.Zero) * 25f;
            Vector2 glowPos = Projectile.Center + backOffset - Main.screenPosition;
            float glowPulse = (float)Math.Sin(PulseTimer * 2.5f) * 0.3f + 1f;
            float glowScale = (isReturning ? 0.95f : 0.95f) * glowPulse;
            float glowAlpha = isReturning ? 0.2f : 0.2f;
            for (int i = 0; i < 3; i++)
            {
                float layerScale = glowScale * (1f + i * 0.4f);
                float layerAlpha = glowAlpha / (i + 1);
                Color layerColor = glowColor * layerAlpha;
                spriteBatch.Draw(glowTexture, glowPos, null, layerColor, Projectile.oldRot[i], drawOrigin, layerScale, SpriteEffects.None, 0f);
            }
            if (isReturning)
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    Vector2 sideOffset = Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2) * 15f * side;
                    Vector2 sideGlowPos = Projectile.Center + backOffset + sideOffset - Main.screenPosition;
                    spriteBatch.Draw(glowTexture, sideGlowPos, null, glowColor * 0.3f, 0f, drawOrigin, glowScale * 0.6f, SpriteEffects.None, 0f);
                }
            }
        }
        public override Color? GetAlpha(Color lightColor)
        {
            bool isReturning = CurrentState == (float)AxeState.Returning;
            float pulse = (float)Math.Sin(PulseTimer) * 0.2f + 0.8f;
            if (isReturning)
            {
                return new Color(200, 120, 255) * pulse;
            }
            return new Color(150, 80, 200) * pulse;
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.ShadowbeamStaff, Main.rand.NextVector2Circular(4f, 4f));
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(1f, 2f);
            }
        }
    }
}
