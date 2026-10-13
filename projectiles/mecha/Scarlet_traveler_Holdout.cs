using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
using jarkos.projectiles.mecha;
namespace Jarkos.projectiles.mecha
{
    public class Scarlet_traveler_Holdout : BaseRangedHoldout
    {
        public override string Texture => "jarkos/items/weapons/ranged/firing/Scarlet_traveler";
        public override float MuzzleForwardOffset => 135f;
        public override float MuzzleVerticalOffset => 2f;
        public override Vector2 HoldoutOrigin => new Vector2(36f, ModContent.Request<Texture2D>(Texture).Height() / 2f);
        private const int BurstEnd = 38;
        private const int ChargeEnd = 78;
        private const int CycleEnd = 93;
        public override int GetUseTime(Player player) => CycleEnd + 1;
        public override bool KeepAliveCondition(Player player)
        {
            if (shootTimer < ChargeEnd && !player.channel)
                return false;
            return true;
        }
        public override bool ShouldFire(Player player) => false;
        public override void UpdateExtra(Player player)
        {
            Vector2 muzzlePos = Projectile.Center + Projectile.velocity * MuzzleForwardOffset + Projectile.velocity.RotatedBy(MathHelper.PiOver2) * MuzzleVerticalOffset * Projectile.direction;
            if (shootTimer < BurstEnd)
            {
                int localTime = shootTimer;
                if (localTime == 21 || localTime == 27 || localTime == 33)
                {
                    if (Projectile.owner == Main.myPlayer)
                    {
                        recoil = 5f;
                        SoundEngine.PlaySound(SoundID.Item38 with { Volume = 0.7f, Pitch = 0.2f }, Projectile.position);
                        for (int i = 0; i < 3; i++)
                        {
                            Vector2 smokeVel = Projectile.velocity.RotatedByRandom(0.4f) * Main.rand.NextFloat(1.5f, 4f);
                            Dust smoke = Dust.NewDustPerfect(muzzlePos, DustID.Smoke, smokeVel, 100, Color.Gray, Main.rand.NextFloat(0.7f, 1.2f));
                            smoke.noGravity = true;
                        }
                        for (int i = 0; i < 4; i++)
                        {
                            Vector2 sparkVel = Projectile.velocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(3f, 6f);
                            new Spark(muzzlePos, sparkVel, new Color(255, 50, 50), 15, Main.rand.NextFloat(0.7f, 1.1f), 1f).Spawn();
                        }
                        int damage = player.GetWeaponDamage(player.HeldItem);
                        float knockback = player.HeldItem.knockBack;
                        float speed = player.HeldItem.shootSpeed;
                        Vector2 projectileVel = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(5)) * speed;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzlePos, projectileVel, ModContent.ProjectileType<Scarlet_proj>(), damage, knockback, Projectile.owner);
                    }
                }
            }
            else if (shootTimer >= BurstEnd && shootTimer < ChargeEnd)
            {
                if (shootTimer == BurstEnd + 2)
                {
                    SoundEngine.PlaySound(SoundID.Item15 with { Volume = 0.4f, Pitch = 0.4f }, Projectile.position);
                }
                if (Main.rand.NextBool(3))
                {
                    Vector2 spawnOffset = Main.rand.NextVector2Circular(50f, 50f) + Projectile.velocity * 30f;
                    Vector2 spawnPos = muzzlePos + spawnOffset;
                    new Stretching_particle(spawnPos, muzzlePos, new Color(255, 50, 50), 20, Main.rand.NextFloat(20f, 40f), Main.rand.NextFloat(6f, 10f), 1.0f).Spawn();
                }
            }
            else if (shootTimer == ChargeEnd)
            {
                if (Projectile.owner == Main.myPlayer)
                {
                    recoil = 14f;
                    SoundEngine.PlaySound(SoundID.Item122 with { Volume = 1.2f, Pitch = -0.2f }, Projectile.position);
                    for (int i = 0; i < 10; i++)
                    {
                        Vector2 smokeVel = Projectile.velocity.RotatedByRandom(0.6f) * Main.rand.NextFloat(3f, 7f);
                        Dust smoke = Dust.NewDustPerfect(muzzlePos, DustID.Smoke, smokeVel, 100, Color.Gray, Main.rand.NextFloat(0.9f, 1.5f));
                        smoke.noGravity = true;
                    }
                    int damage = (int)(player.GetWeaponDamage(player.HeldItem) * 2.5f);
                    float knockback = player.HeldItem.knockBack * 2f;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzlePos, Projectile.velocity, ModContent.ProjectileType<Scarlet_laser_proj>(), damage, knockback, Projectile.owner, Projectile.whoAmI);
                }
            }
            else if (shootTimer >= CycleEnd)
            {
                if (player.channel)
                {
                    shootTimer = 0;
                }
                else
                {
                    Projectile.Kill();
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteEffects spriteEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            float currentRecoil = recoil;
            Vector2 shakeOffset = Vector2.Zero;
            if (recoil > 0.5f)
            {
                shakeOffset = Main.rand.NextVector2Circular(1.5f, 1.5f);
            }
            Vector2 drawPos = Projectile.Center - Main.screenPosition - Projectile.velocity * currentRecoil + shakeOffset;
            float rotation = Projectile.velocity.ToRotation();
            Main.EntitySpriteDraw(texture, drawPos, null, lightColor, rotation, HoldoutOrigin, Projectile.scale, spriteEffects, 0);
            if (shootTimer >= BurstEnd && shootTimer < ChargeEnd)
            {
                float chargeProgress = (float)(shootTimer - BurstEnd) / (ChargeEnd - BurstEnd);
                Vector2 muzzleDrawPos = drawPos + Projectile.velocity * 38f + Projectile.velocity.RotatedBy(MathHelper.PiOver2) * 2f * Projectile.direction;
                Texture2D pixel = TextureAssets.MagicPixel.Value;
                float sphereSize = chargeProgress * 14f + (float)Math.Sin(Main.GameUpdateCount * 0.4f) * 2f;
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }
            return false;
        }
    }
}
