using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos;
using jarkos.projectiles.mecha;
namespace Jarkos.projectiles.mecha
{
    public class Piercer_Holdout : BaseRangedHoldout
    {
        public override string Texture => "jarkos/items/weapons/ranged/firing/Piercer";
        public override float MuzzleForwardOffset => 40f;
        public override float MuzzleVerticalOffset => -2f;
        public override Vector2 HoldoutOrigin => new Vector2(4f, ModContent.Request<Texture2D>(Texture).Height() / 2f);
        public override float CustomShootSpeed => 14f;
        public override int FirstShotDelay => 30;
        public bool IsAltFire => Projectile.ai[0] == 1f;
        public float CurrentRecoil => recoil;
        public int ShootTimer => shootTimer;
        private const int PrimaryBurstCount = 3;
        private const int PrimaryBurstDelay = 6;
        private int currentBurst = 0;
        public const int SpinDuration = 40;
        private const int AltFireEnd = 60;
        public override int GetUseTime(Player player) => 9999;
        public override bool ShouldFire(Player player) => false;
        public override bool KeepAliveCondition(Player player)
        {
            if (IsAltFire) return true;
            return player.channel;
        }
        public override void SafeSetDefaults()
        {
            Projectile.scale = 0.65f;
        }
        public override void UpdateExtra(Player player)
        {
            if (!IsAltFire)
            {
                if (shootTimer % PrimaryBurstDelay == 0 && currentBurst < PrimaryBurstCount)
                {
                    currentBurst++;
                    if (Projectile.owner == Main.myPlayer)
                    {
                        Vector2 muzzlePos = Projectile.Center + Projectile.velocity * MuzzleForwardOffset + Projectile.velocity.RotatedBy(MathHelper.PiOver2) * MuzzleVerticalOffset * Projectile.direction;
                        SoundEngine.PlaySound(Jarkos_sounds.pierce, Projectile.position);
                        recoil = 4f;
                        int damage = player.GetWeaponDamage(player.HeldItem);
                        float knockback = player.HeldItem.knockBack;
                        float speed = 8f;
                        Vector2 projectileVel = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(2)) * speed;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzlePos, projectileVel, ModContent.ProjectileType<Pierc_proj>(), damage, knockback, Projectile.owner);
                        for (int i = 0; i < 3; i++)
                        {
                            Dust.NewDust(muzzlePos, 0, 0, DustID.YellowTorch, projectileVel.X * 0.2f, projectileVel.Y * 0.2f);
                        }
                    }
                }
                if (shootTimer >= PrimaryBurstDelay * PrimaryBurstCount + 10)
                {
                    Projectile.Kill();
                }
            }
            else
            {
                if (shootTimer == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item15 with { Pitch = 0.5f, Volume = 0.6f }, Projectile.position);
                }
                if (shootTimer < SpinDuration)
                {
                    if (Main.rand.NextBool(3))
                    {
                        Vector2 muzzlePos = Projectile.Center + Projectile.velocity * MuzzleForwardOffset + Projectile.velocity.RotatedBy(MathHelper.PiOver2) * MuzzleVerticalOffset * Projectile.direction;
                        Dust.NewDustPerfect(muzzlePos + Main.rand.NextVector2Circular(10f, 10f), DustID.MagicMirror, Vector2.Zero, 0, default, 1.2f).noGravity = true;
                    }
                }
                else if (shootTimer == SpinDuration)
                {
                    if (Projectile.owner == Main.myPlayer)
                    {
                        Vector2 muzzlePos = Projectile.Center + Projectile.velocity * MuzzleForwardOffset + Projectile.velocity.RotatedBy(MathHelper.PiOver2) * MuzzleVerticalOffset * Projectile.direction;
                        SoundEngine.PlaySound(Jarkos_sounds.pierce_alt, Projectile.position);
                        recoil = 12f;
                        int damage = player.GetWeaponDamage(player.HeldItem) * 3;
                        float knockback = player.HeldItem.knockBack * 2f;
                        float speed = 25f;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzlePos, Projectile.velocity * speed, ModContent.ProjectileType<Pierc_proj_alt>(), damage, knockback, Projectile.owner);
                        for (int i = 0; i < 15; i++)
                        {
                            Vector2 sparkVel = Projectile.velocity.RotatedByRandom(0.4f) * Main.rand.NextFloat(3f, 8f);
                            Dust Spark = Dust.NewDustPerfect(muzzlePos, DustID.MagicMirror, sparkVel, 100, default, Main.rand.NextFloat(1.5f, 2.5f));
                            Spark.noGravity = true;
                        }
                    }
                }
                else if (shootTimer >= AltFireEnd)
                {
                    Projectile.Kill();
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }
        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            SpriteEffects spriteEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Texture2D texture2 = ModContent.Request<Texture2D>("jarkos/items/weapons/ranged/firing/piercer_light", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            float currentRecoil = recoil;
            Vector2 shakeOffset = Vector2.Zero;
            if (currentRecoil > 1f)
            {
                shakeOffset = Main.rand.NextVector2Circular(currentRecoil * 0.4f, currentRecoil * 0.4f);
            }
            Vector2 drawPos = Projectile.Center - Main.screenPosition - Projectile.velocity * currentRecoil + shakeOffset;
            float rotation = Projectile.velocity.ToRotation();
            if (IsAltFire && shootTimer < SpinDuration)
            {
                float progress = (float)shootTimer / SpinDuration;
                float easeOut = 1f - (float)Math.Pow(1f - progress, 3);
                float extraRotation = easeOut * MathHelper.TwoPi * -1f;
                rotation += extraRotation * Projectile.direction;
                for (int i = 0; i < 6; i++)
                {
                    Vector2 blurPos = drawPos + Main.rand.NextVector2Circular(4f, 4f);
                    Main.EntitySpriteDraw(texture, blurPos, null, Color.LightCyan * 0.6f * (1f - progress), rotation - (i * 0.15f * Projectile.direction), HoldoutOrigin, Projectile.scale, spriteEffects, 0);
                }
            }
            float outlinePulse = 1f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 5f) * 0.2f;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = (MathHelper.PiOver2 * i).ToRotationVector2() * 2f * outlinePulse;
                Main.EntitySpriteDraw(texture2, drawPos + offset, null, Color.White * 0.8f, rotation, HoldoutOrigin, Projectile.scale, spriteEffects, 0);
            }
        }
        public void DrawAlphaBlend(SpriteBatch spriteBatch, Color lightColor)
        {
            SpriteEffects spriteEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            float currentRecoil = recoil;
            Vector2 shakeOffset = Vector2.Zero;
            if (currentRecoil > 1f)
            {
                shakeOffset = Main.rand.NextVector2Circular(currentRecoil * 0.4f, currentRecoil * 0.4f);
            }
            Vector2 drawPos = Projectile.Center - Main.screenPosition - Projectile.velocity * currentRecoil + shakeOffset;
            float rotation = Projectile.velocity.ToRotation();
            if (IsAltFire && shootTimer < SpinDuration)
            {
                float progress = (float)shootTimer / SpinDuration;
                float easeOut = 1f - (float)Math.Pow(1f - progress, 3);
                float extraRotation = easeOut * MathHelper.TwoPi * -1f;
                rotation += extraRotation * Projectile.direction;
            }
            Main.EntitySpriteDraw(texture, drawPos, null, lightColor, rotation, HoldoutOrigin, Projectile.scale, spriteEffects, 0);
        }
    }
}
