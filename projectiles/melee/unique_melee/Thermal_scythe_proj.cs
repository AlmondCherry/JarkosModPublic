using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
using jarkos.buffs.debuff;

namespace jarkos.projectiles.melee.unique_melee
{
    public class Thermal_scythe_proj : ModProjectile
    {
        private const int TotalSwingTime = 40;
        private const float BladeHalfLength = 82f;
        private const float CollisionWidth = 18f;

        private float AimAngle => Projectile.velocity.ToRotation();

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 16;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 48;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = TotalSwingTime;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 6;
            Projectile.scale = 1.15f;
        }

        public override bool ShouldUpdatePosition() => false;

        public override void OnSpawn(IEntitySource source)
        {
            SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.2f, Volume = 1.0f }, Projectile.position);
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead)
            {
                Projectile.Kill();
                return;
            }

            float progress = MathHelper.Clamp(1f - (float)Projectile.timeLeft / TotalSwingTime, 0f, 1f);

            float easeOutProgress = MathSystem.EaseOutCubic(progress);

            int spinDir = player.direction;
            float currentAngle = AimAngle + MathHelper.TwoPi * spinDir * easeOutProgress;
            Projectile.rotation = currentAngle;

            Vector2 centerPos = player.MountedCenter;
            Projectile.Center = centerPos;

            player.heldProj = Projectile.whoAmI;
            player.itemTime = 2;
            player.itemAnimation = 2;
            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, currentAngle - MathHelper.PiOver2);

            Vector2 rotDir = currentAngle.ToRotationVector2();
            Vector2 tip1 = centerPos + rotDir * BladeHalfLength;
            Vector2 tip2 = centerPos - rotDir * BladeHalfLength;

            Lighting.AddLight(tip1, 1.0f, 0.35f, 0.15f);
            Lighting.AddLight(tip2, 1.0f, 0.35f, 0.15f);

            if (Main.rand.NextBool(14))
            {
                Vector2 tipVel1 = (currentAngle + MathHelper.PiOver2 * spinDir).ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                Dust d1 = Dust.NewDustPerfect(tip1, DustID.Firework_Red, tipVel1, Scale: Main.rand.NextFloat(0.4f, 0.8f));
                d1.noGravity = true;

                Vector2 tipVel2 = (currentAngle - MathHelper.PiOver2 * spinDir).ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
                new Spark(tip2, tipVel2, Color.IndianRed, 90, Main.rand.NextFloat(0.45f, 0.9f), 2f, 0f).Spawn();
            }

            if (Main.rand.NextBool(8))
            {
                Vector2 midPos = Vector2.Lerp(tip1, tip2, Main.rand.NextFloat());
                Dust spark = Dust.NewDustPerfect(midPos, DustID.GoldFlame, rotDir.RotatedByRandom(0.4f) * Main.rand.NextFloat(0.5f, 2f), Scale: 0.8f);
                spark.noGravity = true;
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 center = Projectile.Center;
            Vector2 rotDir = Projectile.rotation.ToRotationVector2();
            Vector2 tip1 = center + rotDir * BladeHalfLength;
            Vector2 tip2 = center - rotDir * BladeHalfLength;

            float point1 = 0f;
            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), center, tip1, CollisionWidth, ref point1))
            {
                return true;
            }

            float point2 = 0f;
            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), center, tip2, CollisionWidth, ref point2))
            {
                return true;
            }

            return null;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/ThermalScytheHit") with { Pitch = Main.rand.NextFloat(-0.2f, 0.2f)}, Projectile.position);

            target.AddBuff(ModContent.BuffType<BleedingMobDebuff>(), 360);

            if(Main.rand.NextBool(4))
            {
                Main.LocalPlayer.GetModPlayer<Screenshake_system>().regular_shake++;
                for (int i = 0; i < 4; i++)
                {
                    Vector2 Vel = Main.rand.NextVector2Circular(8f, 12f);
                    new Spark(target.Center, Vel, Color.Red, 90, Main.rand.NextFloat(1f, 1.75f), 2f, 0f).Spawn();
                }
                target.AddBuff(ModContent.BuffType<ThermalStageDebuff>(), 360);
                SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/ThermalScytheBleeding") with { Pitch = Main.rand.NextFloat(-0.2f, 0.2f)}, Projectile.position);
            }

            for (int i = 0; i < 3; i++)
            {
                Vector2 dustVel = Projectile.rotation.ToRotationVector2().RotatedByRandom(0.8f) * Main.rand.NextFloat(2f, 6f);
                Dust dust = Dust.NewDustDirect(target.Center - new Vector2(10), 20, 20, DustID.Firework_Red, dustVel.X, dustVel.Y, 0, default, 0.4f);
                dust.noGravity = true;
            }

            var upParticle = new UpGlow_particle(
                target,
                Color.OrangeRed,
                18,
                upDistance: 70f,
                scale: 0.75f,
                widthScale: 1.0f,
                initialVerticalScale: 0.05f
            );
            upParticle.Spawn();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            if (texture == null)
                return false;

            Vector2 origin = texture.Size() * 0.5f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            float rotation = Projectile.rotation + MathHelper.PiOver4;

            float progress = MathSystem.GetProgressValue(Projectile.timeLeft, TotalSwingTime);
            float easeOutProgress = 1f - MathF.Pow(1f - progress, 3f);
            float alpha = MathHelper.Clamp(1.5f - progress * 1.3f, 0f, 1f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            float totalSpin = MathHelper.TwoPi * Main.player[Projectile.owner].direction;
            float aimAngle = AimAngle;

            for (int i = 1; i <= 6; i++)
            {
                float trailProgress = MathHelper.Clamp(progress - i * 0.04f, 0f, 1f);
                float trailEase = MathSystem.EaseOutCubic(trailProgress);
                float trailAngle = aimAngle + totalSpin * trailEase + MathHelper.PiOver4;
                float trailAlpha = (1f - (i / 7f)) * 0.45f * alpha;

                Color trailColor = Color.Lerp(Color.OrangeRed, Color.DarkRed, i / 6f) * trailAlpha;
                Main.spriteBatch.Draw(texture, drawPos, null, trailColor, trailAngle, origin, Projectile.scale * (1f + i * 0.015f), SpriteEffects.None, 0f);
            }

            Color auraColor = Color.Orange * (0.35f * alpha);
            Main.spriteBatch.Draw(texture, drawPos, null, auraColor, rotation, origin, Projectile.scale * 1.18f, SpriteEffects.None, 0f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            Color mainColor = Projectile.GetAlpha(lightColor) * alpha;
            Main.EntitySpriteDraw(texture, drawPos, null, mainColor, rotation, origin, Projectile.scale, SpriteEffects.None, 0);

            return false;
        }
    }
}
