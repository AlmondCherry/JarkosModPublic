using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.projectiles.ranged.firearm;
using jarkos.items.weapons.ranged.firing;
using Microsoft.Xna.Framework.Graphics;
namespace jarkos.projectiles.mecha
{
    public class Dfsr01_Holdout : BaseRangedHoldout
    {
        public override string Texture => "jarkos/items/weapons/ranged/firing/Dfsr01";
        public override float MuzzleForwardOffset => 60f;
        public override float MuzzleVerticalOffset => -5f;
        public override int FirstShotDelay => 30;
        public override Vector2 HoldoutOrigin => new Vector2(42f, ModContent.Request<Texture2D>(Texture).Height() / 2f);
        public override bool KeepAliveCondition(Player player)
        {
            var mp = player.GetModPlayer<SST01Player>();
            return player.channel || mp.burstShotsLeft > 0;
        }
        public override Vector2 GetAimDirection(Player player, Vector2 mountedCenter)
        {
            var mp = player.GetModPlayer<SST01Player>();
            if (mp.burstShotsLeft > 0)
            {
                return mp.burstDirection.SafeNormalize(Vector2.UnitX);
            }
            return base.GetAimDirection(player, mountedCenter);
        }
        public override int GetUseTime(Player player)
        {
            var mp = player.GetModPlayer<SST01Player>();
            if (mp.fireMode == 0) return 9;
            if (mp.fireMode == 1) return 18;
            if (mp.fireMode == 2) return 22;
            return 9;
        }
        public override bool ShouldFire(Player player)
        {
            return player.GetModPlayer<SST01Player>().burstShotsLeft <= 0;
        }
        public override void OnFire(Player player, Vector2 muzzlePos, Vector2 velocity, float speed, int damage, float knockback, int projToShoot)
        {
            var mp = player.GetModPlayer<SST01Player>();
            Vector2 vel = velocity * speed;
            switch (mp.fireMode)
            {
                case 0:
                    ShootAuto(player, muzzlePos, vel, damage, knockback);
                    break;
                case 1:
                    ShootBurst(player, mp, muzzlePos, vel, damage, knockback);
                    break;
                case 2:
                    ShootSingle(player, muzzlePos, vel, damage, knockback);
                    break;
            }
        }
        public override void UpdateExtra(Player player)
        {
            var mp = player.GetModPlayer<SST01Player>();
            if (mp.burstShotsLeft > 0 && mp.burstTimer == 1 && Projectile.owner == Main.myPlayer)
            {
                recoil = 6f;
                Vector2 muzzlePos = Projectile.Center + Projectile.velocity * MuzzleForwardOffset + Projectile.velocity.RotatedBy(MathHelper.PiOver2) * MuzzleVerticalOffset * Projectile.direction;
                SoundEngine.PlaySound(SoundID.Item11 with { Volume = 0.5f, Pitch = 0.15f }, muzzlePos);
                SpawnMuzzleFlash(muzzlePos, Projectile.velocity, 1);
                mp.burstSpawnPos = muzzlePos;
            }
        }
        private void ShootAuto(Player player, Vector2 muzzle, Vector2 velocity, int damage, float knockback)
        {
            Vector2 vel = velocity.RotatedByRandom(MathHelper.ToRadians(3f));
            vel *= Main.rand.NextFloat(0.95f, 1.05f);
            int proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzle, vel, ModContent.ProjectileType<Dfsr01_bullet>(), damage, knockback, player.whoAmI);
            if (proj < Main.maxProjectiles)
            {
                Main.projectile[proj].ai[0] = Main.rand.NextFloat(MathHelper.TwoPi);
                Main.projectile[proj].ai[1] = 0f;
            }
            SoundEngine.PlaySound(SoundID.Item11 with { Volume = 0.55f, Pitch = Main.rand.NextFloat(-0.05f, 0.1f) }, muzzle);
            SpawnMuzzleFlash(muzzle, velocity.SafeNormalize(Vector2.UnitX), 0);
            recoil = 5f;
        }
        private void ShootBurst(Player player, SST01Player mp, Vector2 muzzle, Vector2 velocity, int damage, float knockback)
        {
            Vector2 vel = velocity.RotatedByRandom(MathHelper.ToRadians(0.8f));
            int proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzle, vel, ModContent.ProjectileType<Dfsr01_bullet>(), damage, knockback, player.whoAmI);
            if (proj < Main.maxProjectiles)
            {
                Main.projectile[proj].ai[0] = Main.rand.NextFloat(MathHelper.TwoPi);
                Main.projectile[proj].ai[1] = 1f;
            }
            mp.burstShotsLeft = 2;
            mp.burstTimer = 4;
            mp.burstDirection = velocity;
            mp.burstSpawnPos = muzzle;
            mp.burstDamage = damage;
            mp.burstKnockback = knockback;
            SoundEngine.PlaySound(SoundID.Item11 with { Volume = 0.5f, Pitch = 0.15f }, muzzle);
            SpawnMuzzleFlash(muzzle, velocity.SafeNormalize(Vector2.UnitX), 1);
            recoil = 6f;
        }
        private void ShootSingle(Player player, Vector2 muzzle, Vector2 velocity, int damage, float knockback)
        {
            Vector2 vel = velocity * 1.3f;
            int proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzle, vel, ModContent.ProjectileType<Dfsr01_bullet>(), (int)(damage * 1.8f), knockback * 2f, player.whoAmI);
            if (proj < Main.maxProjectiles)
            {
                Main.projectile[proj].ai[0] = Main.rand.NextFloat(MathHelper.TwoPi);
                Main.projectile[proj].ai[1] = 2f;
                Main.projectile[proj].penetrate = 3;
                Main.projectile[proj].usesLocalNPCImmunity = true;
                Main.projectile[proj].localNPCHitCooldown = 8;
            }
            SoundEngine.PlaySound(SoundID.Item40 with { Volume = 0.7f, Pitch = -0.15f }, muzzle);
            SoundEngine.PlaySound(SoundID.Item38 with { Volume = 0.4f, Pitch = -0.3f }, muzzle);
            SpawnMuzzleFlash(muzzle, velocity.SafeNormalize(Vector2.UnitX), 2);
            recoil = 14f;
        }
        private void SpawnMuzzleFlash(Vector2 pos, Vector2 dir, int mode)
        {
            int count;
            float scale;
            switch (mode)
            {
                case 2: count = 14; scale = 1.5f; break;
                case 1: count = 5; scale = 0.9f; break;
                default: count = 4; scale = 0.8f; break;
            }
            for (int i = 0; i < count; i++)
            {
                Vector2 vel = dir * Main.rand.NextFloat(2f, 6f) + Main.rand.NextVector2Circular(1.5f, 1.5f);
                Dust d = Dust.NewDustDirect(pos, 0, 0, DustID.Smoke, vel.X, vel.Y, 120, new Color(200, 200, 210), scale);
                d.noGravity = true;
                d.fadeIn = scale + 0.3f;
            }
            for (int i = 0; i < count / 2 + 1; i++)
            {
                Vector2 vel = dir * Main.rand.NextFloat(1f, 4f) + Main.rand.NextVector2Circular(1f, 1f);
                Dust d = Dust.NewDustDirect(pos, 0, 0, DustID.Torch, vel.X, vel.Y, 60, default, scale * 0.8f);
                d.noGravity = true;
            }
            if (mode == 2)
            {
                for (int i = 0; i < 8; i++)
                {
                    Vector2 vel = dir * Main.rand.NextFloat(3f, 7f) + Main.rand.NextVector2Circular(2f, 2f);
                    Dust d = Dust.NewDustDirect(pos, 0, 0, DustID.WhiteTorch, vel.X, vel.Y, 0, default, 1.2f);
                    d.noGravity = true;
                    d.fadeIn = 1.5f;
                }
            }
        }
    }
}
