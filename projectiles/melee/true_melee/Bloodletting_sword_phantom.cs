using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.melee.true_melee
{
	public class Bloodletting_sword_phantom : ModProjectile {
		private enum PhantomState
        {
            PullBack, Rotating, Flying
        }
        private ref float InitialAngle => ref Projectile.ai[0];
        private ref float StateTimer => ref Projectile.ai[1];
        private PhantomState currentState = PhantomState.PullBack;
        private Vector2 pullBackDirection;
        private float targetAngle;
        private float currentRotation;
        private float rotationSpeed = 0f;
        private Vector2 flyDirection;
        private bool initialized = false;
        private const float PULL_BACK_DISTANCE = 80f;
        private const int PULL_BACK_TIME = 15;
        private const float ROTATION_SPEED_MAX = 0.5f;
        private const float FLY_SPEED = 25f;
        private const float ANGLE_THRESHOLD = 0.1f;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
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
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 300;
            Projectile.alpha = 50;
            Projectile.scale = 1.3f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!initialized)
            {
                Initialize(player);
                initialized = true;
            }
            Lighting.AddLight(Projectile.Center, new Vector3(0.8f, 0.15f, 0.15f));
            switch (currentState)
            {
                case PhantomState.PullBack:
                    UpdatePullBack(player);
                    break;
                case PhantomState.Rotating:
                    UpdateRotating(player);
                    break;
                case PhantomState.Flying:
                    UpdateFlying();
                    break;
            }
            CreatePhantomEffects();
            StateTimer++;
        }
        private void Initialize(Player player)
        {
            currentRotation = InitialAngle;
            Projectile.rotation = currentRotation;
            pullBackDirection = -InitialAngle.ToRotationVector2();
            SoundEngine.PlaySound(SoundID.Item103 with { Volume = 0.6f, Pitch = 0.5f }, Projectile.Center);
        }
        private void UpdatePullBack(Player player)
        {
            float progress = StateTimer / PULL_BACK_TIME;
            float easedProgress = EaseOutQuad(progress);
            Vector2 pullOffset = pullBackDirection * PULL_BACK_DISTANCE * easedProgress;
            Projectile.velocity = pullBackDirection * (PULL_BACK_DISTANCE / PULL_BACK_TIME) * (1f - progress * 0.5f);
            Projectile.Center += new Vector2(Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-2, 2));
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(40, 40), DustID.Blood);
                dust.velocity = (Projectile.Center - dust.position).SafeNormalize(Vector2.Zero) * 3f;
                dust.scale = Main.rand.NextFloat(1.5f, 2.5f);
                dust.noGravity = true;
            }
            if (StateTimer >= PULL_BACK_TIME)
            {
                currentState = PhantomState.Rotating;
                StateTimer = 0;
                Projectile.velocity = Vector2.Zero;
                targetAngle = (Main.MouseWorld - Projectile.Center).ToRotation();
                SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.7f, Pitch = 0.2f }, Projectile.Center);
            }
        }
        private void UpdateRotating(Player player)
        {
            targetAngle = (Main.MouseWorld - Projectile.Center).ToRotation();
            float angleDiff = MathHelper.WrapAngle(targetAngle - currentRotation);
            rotationSpeed = MathHelper.Lerp(rotationSpeed, ROTATION_SPEED_MAX * Math.Sign(angleDiff), 0.15f);
            currentRotation += rotationSpeed;
            currentRotation = MathHelper.WrapAngle(currentRotation);
            Projectile.rotation = currentRotation;
            for (int i = 0; i < 2; i++)
            {
                Vector2 dustPos = Projectile.Center + currentRotation.ToRotationVector2() * Main.rand.NextFloat(20, 60);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.GemRuby);
                dust.velocity = currentRotation.ToRotationVector2().RotatedBy(MathHelper.PiOver2 * Math.Sign(rotationSpeed)) * 2f;
                dust.scale = Main.rand.NextFloat(1f, 1.5f);
                dust.noGravity = true;
            }
            if (Math.Abs(angleDiff) < ANGLE_THRESHOLD || StateTimer > 60)
            {
                currentState = PhantomState.Flying;
                StateTimer = 0;
                flyDirection = targetAngle.ToRotationVector2();
                Projectile.velocity = flyDirection * FLY_SPEED;
                Projectile.tileCollide = true;
                SoundEngine.PlaySound(SoundID.Item74 with { Volume = 1f, Pitch = 0f }, Projectile.Center);
                for (int i = 0; i < 20; i++)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Blood);
                    dust.velocity = flyDirection.RotatedByRandom(0.5f) * Main.rand.NextFloat(5, 12);
                    dust.scale = Main.rand.NextFloat(1.5f, 2.5f);
                    dust.noGravity = true;
                }
            }
        }
        private void UpdateFlying()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            if (Projectile.velocity.Length() < FLY_SPEED * 1.5f)
            {
                Projectile.velocity *= 1.02f;
            }
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center - Projectile.velocity * 0.5f, DustID.Blood);
                dust.velocity = -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(2, 2);
                dust.scale = Main.rand.NextFloat(1.2f, 1.8f);
                dust.noGravity = true;
            }
        }
        private void CreatePhantomEffects()
        {
            if (Main.rand.NextBool(3))
            {
                Vector2 offset = Main.rand.NextVector2Circular(Projectile.width / 2, Projectile.height / 2);
                Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, DustID.RedTorch);
                dust.velocity = Vector2.Zero;
                dust.scale = Main.rand.NextFloat(0.8f, 1.2f);
                dust.noGravity = true;
                dust.fadeIn = 1.2f;
            }
        }
        private float EaseOutQuad(float t) => 1 - (1 - t) * (1 - t);
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 15; i++)
            {
                Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Blood);
                dust.velocity = Main.rand.NextVector2Circular(8, 8);
                dust.scale = Main.rand.NextFloat(1.5f, 2.5f);
                dust.noGravity = true;
            }
            SoundEngine.PlaySound(SoundID.NPCHit18 with { Volume = 0.8f }, target.Center);
            Player player = Main.player[Projectile.owner];
            int healAmount = Main.rand.Next(1, 2);
            player.Heal(healAmount);
        }
        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.5f, Pitch = 0.5f }, Projectile.Center);
            for (int i = 0; i < 30; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool() ? DustID.Blood : DustID.GemRuby);
                dust.velocity = Main.rand.NextVector2Circular(10, 10);
                dust.scale = Main.rand.NextFloat(1.5f, 2.5f);
                dust.noGravity = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 origin = texture.Size() / 2;
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float trailProgress = 1f - (i / (float)Projectile.oldPos.Length);
                float trailAlpha = trailProgress * 0.7f;
                float trailScale = Projectile.scale * (0.3f + trailProgress * 0.7f);
                Color trailColor = new Color(150, 0, 0, 0) * trailAlpha;
                spriteBatch.Draw(texture, Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition, null, trailColor, Projectile.oldRot[i] + MathHelper.PiOver4, origin, trailScale, SpriteEffects.None, 0);
            }
            Color glowColor = new Color(255, 30, 30, 0);
            for (int i = 0; i < 6; i++)
            {
                Vector2 offset = new Vector2(6, 0).RotatedBy(MathHelper.TwoPi / 6 * i);
                spriteBatch.Draw(texture, Projectile.Center + offset - Main.screenPosition, null, glowColor * 0.4f, Projectile.rotation + MathHelper.PiOver4, origin, Projectile.scale * 1.1f, SpriteEffects.None, 0);
            }
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 0.1f + 0.9f;
            spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, new Color(255, 100, 100, 0) * 0.5f * pulse, Projectile.rotation + MathHelper.PiOver4, origin, Projectile.scale * 1.2f * pulse, SpriteEffects.None, 0);
            spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, Color.White * ((255 - Projectile.alpha) / 255f), Projectile.rotation + MathHelper.PiOver4, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
	}
}
