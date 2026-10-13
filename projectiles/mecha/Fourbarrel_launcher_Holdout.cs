using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.projectiles.ranged.firearm;
namespace jarkos.projectiles.mecha
{
    public class Fourbarrel_launcher_Holdout : BaseRangedHoldout
    {
        public override string Texture => "jarkos/items/weapons/ranged/firing/Fourbarrel_launcher";
        public override float MuzzleForwardOffset => 65f;
        public override float MuzzleVerticalOffset => -10f;
        public override int FirstShotDelay => 30;
        private int burstCounter = 0;
        private int chargeCounter = 0;
        public override void OnFire(Player player, Vector2 muzzlePos, Vector2 velocity, float speed, int damage, float knockback, int projToShoot)
        {
            chargeCounter++;
            bool isOverload = chargeCounter >= 8;
            Vector2 direction = Projectile.velocity;
            Vector2 perpendicular = new Vector2(-direction.Y, direction.X) * Projectile.direction;
            if (isOverload)
            {
                chargeCounter = 0;
                OverloadShot(player, muzzlePos, Projectile.velocity * speed, direction, perpendicular, damage, knockback);
            }
            else
            {
                NormalShot(player, muzzlePos, Projectile.velocity * speed, direction, perpendicular, damage, knockback);
            }
        }
        private void NormalShot(Player player, Vector2 position, Vector2 velocity, Vector2 direction, Vector2 perpendicular, int damage, float knockback)
        {
            int barrel = burstCounter % 4;
            burstCounter++;
            float[] offsets = { -6f, -2f, 2f, 6f };
            float offset = offsets[barrel];
            Vector2 spawnPos = position + perpendicular * offset;
            Vector2 shotVel = velocity.RotatedByRandom(MathHelper.ToRadians(1.5f));
            int proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPos, shotVel, ModContent.ProjectileType<Glow_projectile>(), damage, knockback, player.whoAmI);
            if (proj < Main.maxProjectiles)
            {
                Main.projectile[proj].ai[0] = barrel * MathHelper.PiOver2 + Main.rand.NextFloat(0.5f);
                Main.projectile[proj].ai[1] = 0f;
            }
            SoundEngine.PlaySound(SoundID.Item91 with { Volume = 0.7f, Pitch = 0.1f + barrel * 0.05f }, spawnPos);
            SpawnMuzzleFlash(spawnPos, direction, false);
            recoil = 8f;
        }
        private void OverloadShot(Player player, Vector2 position, Vector2 velocity, Vector2 direction, Vector2 perpendicular, int damage, float knockback)
        {
            SoundEngine.PlaySound(SoundID.Item125 with { Volume = 0.9f, Pitch = -0.2f }, position);
            SoundEngine.PlaySound(SoundID.Item93 with { Volume = 0.5f }, position);
            float[] offsets = { -6f, -2f, 2f, 6f };
            for (int i = 0; i < 4; i++)
            {
                Vector2 spawnPos = position + perpendicular * offsets[i];
                Vector2 shotVel = velocity * 1.4f;
                shotVel = shotVel.RotatedBy(MathHelper.ToRadians((i - 1.5f) * 2f));
                int proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPos, shotVel, ModContent.ProjectileType<Glow_projectile>(), (int)(damage * 1.6f), knockback * 1.5f, player.whoAmI);
                if (proj < Main.maxProjectiles)
                {
                    Main.projectile[proj].ai[0] = i * MathHelper.PiOver2;
                    Main.projectile[proj].ai[1] = 1f;
                    Main.projectile[proj].penetrate = 3;
                    Main.projectile[proj].usesLocalNPCImmunity = true;
                    Main.projectile[proj].localNPCHitCooldown = 10;
                }
            }
            SpawnMuzzleFlash(position, direction, true);
            recoil = 16f;
        }
        private void SpawnMuzzleFlash(Vector2 pos, Vector2 dir, bool overload)
        {
            int count = overload ? 5 : 2;
            float baseScale = overload ? 1.6f : 0.9f;
            for (int i = 0; i < count; i++)
            {
                Vector2 vel = dir * Main.rand.NextFloat(2f, 7f) + Main.rand.NextVector2Circular(2f, 2f);
                int dustType = overload ? DustID.WhiteTorch : DustID.YellowStarDust;
                Dust d = Dust.NewDustDirect(pos, 0, 0, dustType, vel.X, vel.Y, 0, default, baseScale);
                d.noGravity = true;
                d.fadeIn = baseScale + 0.5f;
            }
            if (overload)
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector2 vel = dir * Main.rand.NextFloat(1f, 5f) + Main.rand.NextVector2Circular(3f, 3f);
                    Dust d = Dust.NewDustDirect(pos, 0, 0, DustID.GoldFlame, vel.X, vel.Y, 0, default, 1.3f);
                    d.noGravity = true;
                }
            }
        }
    }
}
