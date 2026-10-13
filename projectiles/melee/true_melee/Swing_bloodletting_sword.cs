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
	public class Swing_bloodletting_sword : ModProjectile {
        private const float SWING_ANGLE_DEGREES = 110f;
        private const float SWORD_LENGTH = 100f;
        private const int SWING_DURATION = 20;
        private Player Owner => Main.player[Projectile.owner];
        private float Timer
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        private float startAngle;
        private float endAngle;
        private int swingDir;
        private bool initialized = false;
        private bool phantomSpawned = false;
        private List<float> trailAngles = new List<float>();
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = SWING_DURATION;
            Projectile.scale = 1.2f;
            Projectile.timeLeft = SWING_DURATION + 15;
        }
        public override void AI()
        {
            if (!initialized)
            {
                Initialize();
                initialized = true;
            }
            Projectile.Center = Owner.Center;
            Owner.heldProj = Projectile.whoAmI;
            Owner.itemTime = 2;
            Owner.itemAnimation = 2;
            float progress = Timer / SWING_DURATION;
            if (progress >= 1f)
            {
                FinishSwing();
                return;
            }
            float smoothProgress = MathSystem.EaseOutCubic(progress);
            float currentAngle = MathHelper.Lerp(startAngle, endAngle, smoothProgress);
            Projectile.rotation = currentAngle;
            Owner.direction = swingDir;
            trailAngles.Insert(0, currentAngle);
            if (trailAngles.Count > 12)
                trailAngles.RemoveAt(trailAngles.Count - 1);
            CreateEffects(currentAngle);
            if (Timer == 0)
            {
                SoundEngine.PlaySound(SoundID.Item71 with { Volume = 0.8f, Pitch = -0.2f }, Owner.Center);
            }
            Timer++;
        }
        private void Initialize()
        {
            Vector2 toCursor = Main.MouseWorld - Owner.Center;
            float aimAngle = toCursor.ToRotation();
            bool cursorOnRight = Main.MouseWorld.X >= Owner.Center.X;
            swingDir = cursorOnRight ? 1 : -1;
            float halfSwing = MathHelper.ToRadians(SWING_ANGLE_DEGREES / 2f);
            if (cursorOnRight)
            {
                startAngle = aimAngle + halfSwing;
                endAngle = aimAngle - halfSwing;
            }
            else
            {
                startAngle = aimAngle - halfSwing;
                endAngle = aimAngle + halfSwing;
            }
            Projectile.rotation = startAngle;
            Projectile.spriteDirection = swingDir;
            for (int i = 0; i < 10; i++)
            {
                Vector2 dustPos = Owner.Center + startAngle.ToRotationVector2() * Main.rand.NextFloat(30, SWORD_LENGTH);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.Blood);
                dust.velocity = startAngle.ToRotationVector2() * Main.rand.NextFloat(2, 5);
                dust.scale = Main.rand.NextFloat(1.2f, 1.8f);
                dust.noGravity = true;
            }
        }
        private void FinishSwing()
        {
            if (!phantomSpawned && Main.myPlayer == Projectile.owner)
            {
                phantomSpawned = true;
                Vector2 spawnPos = Owner.Center + Projectile.rotation.ToRotationVector2() * (SWORD_LENGTH * 0.5f);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPos, Vector2.Zero, ModContent.ProjectileType<Bloodletting_sword_phantom>(), (int)(Projectile.damage * 1.5f), Projectile.knockBack * 1.5f, Projectile.owner, Projectile.rotation);
                SoundEngine.PlaySound(SoundID.Item74 with { Volume = 1f, Pitch = -0.3f }, Owner.Center);
                for (int i = 0; i < 20; i++)
                {
                    Vector2 dustPos = Owner.Center + Projectile.rotation.ToRotationVector2() * Main.rand.NextFloat(20, SWORD_LENGTH);
                    Dust dust = Dust.NewDustPerfect(dustPos, DustID.GemRuby);
                    dust.velocity = Projectile.rotation.ToRotationVector2().RotatedByRandom(0.5f) * Main.rand.NextFloat(4, 10);
                    dust.scale = Main.rand.NextFloat(1.5f, 2.2f);
                    dust.noGravity = true;
                }
            }
            Projectile.Kill();
        }
         private void CreateEffects(float angle)
        {
            Vector2 lightPos = Owner.Center + angle.ToRotationVector2() * (SWORD_LENGTH * 0.5f);
            Lighting.AddLight(lightPos, 0.9f, 0.2f, 0.2f);
            if (Main.rand.NextBool(2))
            {
                Vector2 dustPos = Owner.Center + angle.ToRotationVector2() * Main.rand.NextFloat(30, SWORD_LENGTH);
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.Blood);
                dust.velocity = angle.ToRotationVector2().RotatedBy(-MathHelper.PiOver2 * swingDir) * Main.rand.NextFloat(3, 7);
                dust.scale = Main.rand.NextFloat(1f, 1.6f);
                dust.noGravity = true;
            }
            if (Main.rand.NextBool(3))
            {
                Vector2 tipPos = Owner.Center + angle.ToRotationVector2() * SWORD_LENGTH;
                Dust Spark = Dust.NewDustPerfect(tipPos, DustID.GemRuby);
                Spark.velocity = Main.rand.NextVector2Circular(2, 2);
                Spark.scale = Main.rand.NextFloat(0.8f, 1.3f);
                Spark.noGravity = true;
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 start = Owner.Center;
            Vector2 end = Owner.Center + Projectile.rotation.ToRotationVector2() * SWORD_LENGTH;
            float point = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 40f, ref point);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            int heal = Main.rand.Next(1, 2);
            Owner.Heal(heal);
            for (int i = 0; i < 10; i++)
            {
                Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, DustID.Blood);
                dust.velocity = Main.rand.NextVector2Circular(6, 6);
                dust.scale = Main.rand.NextFloat(1.5f, 2.2f);
                dust.noGravity = true;
            }
            SoundEngine.PlaySound(SoundID.NPCHit18 with { Volume = 0.6f }, target.Center);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPos = Owner.Center - Main.screenPosition;
            Vector2 origin;
            SpriteEffects effects;
            float rotOffset;
            if (swingDir >= 0)
            {
                origin = new Vector2(0, texture.Height);
                effects = SpriteEffects.None;
                rotOffset = MathHelper.ToRadians(0f);
            }
            else
            {
                origin = new Vector2(texture.Width, texture.Height);
                effects = SpriteEffects.FlipHorizontally;
                rotOffset = MathHelper.ToRadians(180f);
            }
            for (int i = 0; i < trailAngles.Count; i++)
            {
                float trailProgress = 1f - (i / (float)trailAngles.Count);
                float trailAlpha = trailProgress * 0.5f;
                float trailScale = Projectile.scale * (0.4f + trailProgress * 0.6f);
                Color trailColor = new Color(180, 30, 30, 0) * trailAlpha;
                spriteBatch.Draw(texture, drawPos, null, trailColor, trailAngles[i] + rotOffset, origin, trailScale, effects, 0);
            }
            Color glowColor = new Color(255, 50, 50, 0);
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(5, 0).RotatedBy(MathHelper.TwoPi / 4 * i);
                spriteBatch.Draw(texture, drawPos + offset, null, glowColor * 0.35f, Projectile.rotation + rotOffset, origin, Projectile.scale * 1.1f, effects, 0);
            }
            float pulse = 0.8f + (float)Math.Sin(Main.GameUpdateCount * 0.3f) * 0.2f;
            spriteBatch.Draw(texture, drawPos, null, new Color(255, 150, 150, 0) * 0.3f * pulse, Projectile.rotation + rotOffset, origin, Projectile.scale * 1.15f * pulse, effects, 0);
            spriteBatch.Draw(texture, drawPos, null, Color.White, Projectile.rotation + rotOffset, origin, Projectile.scale, effects, 0);
            spriteBatch.Draw(texture, drawPos, null, new Color(255, 220, 220, 100) * 0.25f, Projectile.rotation + rotOffset, origin, Projectile.scale * 1.02f, effects, 0);
            return false;
        }
	}
}
