using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
using jarkos.systems;
using jarkos.projectiles.mecha;
namespace Jarkos.projectiles.mecha
{
    public class Rollback_Holdout : BaseRangedHoldout
    {
        public override string Texture => "jarkos/items/weapons/ranged/firing/Rollback";
        public override float MuzzleForwardOffset => 146f;
        public override float MuzzleVerticalOffset => 2f;
        public override Vector2 HoldoutOrigin => new Vector2(42f, ModContent.Request<Texture2D>(Texture).Height() / 2f);
        public override int FirstShotDelay => 30;
        private int chargeTimer = 0;
        private ReLogic.Utilities.SlotId chargeSoundSlot = ReLogic.Utilities.SlotId.Invalid;
        public override bool KeepAliveCondition(Player player) => player.controlUseItem || player.controlUseTile;
        public override bool ShouldFire(Player player) => false;
        public override void UpdateExtra(Player player)
        {
            var modPlayer = player.GetModPlayer<PurgatoryPlayer>();
            Vector2 muzzlePos = Projectile.Center + Projectile.velocity * MuzzleForwardOffset + Projectile.velocity.RotatedBy(MathHelper.PiOver2) * MuzzleVerticalOffset * Projectile.direction;
            if (player.controlUseTile)
            {
                if (chargeTimer > 0)
                {
                    chargeTimer = 0;
                    if (chargeSoundSlot.IsValid)
                    {
                        if (SoundEngine.TryGetActiveSound(chargeSoundSlot, out ActiveSound activeSound))
                        {
                            activeSound.Stop();
                        }
                        chargeSoundSlot = ReLogic.Utilities.SlotId.Invalid;
                    }
                }
                if (modPlayer.RollbackRMBTimer >= 25)
                {
                    modPlayer.RollbackRMBTimer = 0;
                    if (Projectile.owner == Main.myPlayer)
                    {
                        SoundEngine.PlaySound(SoundID.Item91 with { Volume = 0.75f, Pitch = 0.1f }, Projectile.position);
                        SoundEngine.PlaySound(SoundID.Item24 with { Volume = 0.5f, Pitch = 0.2f }, Projectile.position);
                        recoil = 5f;
                        for (int i = 0; i < 6; i++)
                        {
                            Vector2 smokeVel = Projectile.velocity.RotatedByRandom(0.4f) * Main.rand.NextFloat(2f, 5f);
                            Dust smoke = Dust.NewDustPerfect(muzzlePos, DustID.OrangeTorch, smokeVel, 100, default, 1.2f);
                            smoke.noGravity = true;
                        }
                        int damage = player.GetWeaponDamage(player.HeldItem);
                        float knockback = player.HeldItem.knockBack;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzlePos, Projectile.velocity * 9f, ModContent.ProjectileType<Rollback_sphere_proj>(), damage, knockback, Projectile.owner);
                    }
                }
            }
            else if (chargeTimer > 0)
            {
                if (!player.controlUseItem)
                {
                    Projectile.Kill();
                    return;
                }
                chargeTimer++;
                float chargeProgress = (float)chargeTimer / 45f;
                if (chargeTimer == 2)
                {
                    chargeSoundSlot = SoundEngine.PlaySound(SoundID.Item15 with { Volume = 0.85f, Pitch = -0.6f }, Projectile.position);
                }
                if (chargeSoundSlot.IsValid && SoundEngine.TryGetActiveSound(chargeSoundSlot, out ActiveSound activeSound))
                {
                    activeSound.Pitch = MathHelper.Lerp(-0.6f, 0.4f, chargeProgress);
                    activeSound.Position = muzzlePos;
                }
                recoil = MathHelper.Lerp(0f, 3.5f, chargeProgress);
                if (Main.rand.NextBool(2))
                {
                    Vector2 spawnOffset = Main.rand.NextVector2Circular(60f, 60f) + Projectile.velocity * Main.rand.NextFloat(-25f, 25f);
                    Vector2 spawnPos = muzzlePos + spawnOffset;
                    new Stretching_particle(spawnPos, muzzlePos, new Color(255, 110, 20) * 0.9f, 16, Main.rand.NextFloat(12f, 24f), Main.rand.NextFloat(3f, 7f), 1.0f).Spawn();
                }
                if (Main.rand.NextBool(3))
                {
                    new Circular_glow(muzzlePos, Vector2.Zero, new Color(255, 150, 30) * 0.7f, 10, Main.rand.NextFloat(0.25f, 0.55f), 0.8f, Main.rand.NextFloat(-0.06f, 0.06f)).Spawn();
                }
                if (chargeTimer >= 45)
                {
                    chargeTimer = 0;
                    modPlayer.RollbackLMBTimer = 0;
                    if (chargeSoundSlot.IsValid && SoundEngine.TryGetActiveSound(chargeSoundSlot, out ActiveSound activeSound2))
                    {
                        activeSound2.Stop();
                        chargeSoundSlot = ReLogic.Utilities.SlotId.Invalid;
                    }
                    if (Projectile.owner == Main.myPlayer)
                    {
                        SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/RecoillessBurst") with { Volume = 0.95f, Pitch = Main.rand.NextFloat(-0.05f, 0.05f) }, Projectile.position);
                        SoundEngine.PlaySound(SoundID.Item38 with { Volume = 0.6f, Pitch = -0.2f }, Projectile.position);
                        recoil = 15f;
                        for (int i = 0; i < 15; i++)
                        {
                            Vector2 smokeVel = Projectile.velocity.RotatedByRandom(0.4f) * Main.rand.NextFloat(3f, 8f);
                            Dust smoke = Dust.NewDustPerfect(muzzlePos, DustID.Smoke, smokeVel, 100, Color.Orange, Main.rand.NextFloat(0.8f, 1.5f));
                            smoke.noGravity = true;
                        }
                        for (int i = 0; i < 12; i++)
                        {
                            Vector2 sparkVel = Projectile.velocity.RotatedByRandom(0.6f) * Main.rand.NextFloat(4f, 10f);
                            new Spark(muzzlePos, sparkVel, new Color(255, 60, 20), 24, Main.rand.NextFloat(0.9f, 1.5f), 1f).Spawn();
                        }
                        int damage = player.GetWeaponDamage(player.HeldItem);
                        float knockback = player.HeldItem.knockBack;
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzlePos, Projectile.velocity * player.HeldItem.shootSpeed, ModContent.ProjectileType<Rollback_laser_proj>(), damage, knockback, Projectile.owner);
                    }
                }
            }
            else if (player.controlUseItem)
            {
                if (chargeTimer == 0 && modPlayer.RollbackLMBTimer >= 150)
                {
                    chargeTimer = 1;
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
                float shakeAmt = MathHelper.Clamp(recoil * 0.6f, 1.5f, 8f);
                shakeOffset = Main.rand.NextVector2Circular(shakeAmt, shakeAmt);
            }
            Vector2 drawPos = Projectile.Center - Main.screenPosition - Projectile.velocity * currentRecoil + shakeOffset;
            float rotation = Projectile.velocity.ToRotation();
            Main.EntitySpriteDraw(texture, drawPos, null, lightColor, rotation, HoldoutOrigin, Projectile.scale, spriteEffects, 0);
            return false;
        }
        public override void OnKillCondition(Player player)
        {
            if (chargeSoundSlot.IsValid)
            {
                if (SoundEngine.TryGetActiveSound(chargeSoundSlot, out ActiveSound activeSound))
                {
                    activeSound.Stop();
                }
            }
        }
        public override void OnKill(int timeLeft)
        {
            OnKillCondition(Owner);
        }
    }
}
