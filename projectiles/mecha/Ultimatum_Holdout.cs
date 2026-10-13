using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.projectiles.ranged.firearm;
using Microsoft.Xna.Framework.Graphics;
namespace jarkos.projectiles.mecha
{
    public class Ultimatum_Holdout : BaseRangedHoldout
    {
        public override string Texture => "jarkos/items/weapons/ranged/firing/Ultimatum";
        public override float MuzzleForwardOffset => 65f;
        public override float MuzzleVerticalOffset => -6f;
        public override float BaseRecoil => 8f;
        public override Vector2 HoldoutOrigin => new Vector2(24f, ModContent.Request<Texture2D>(Texture).Height() / 2f);
        public override int FirstShotDelay => 30;
        private int shotCounter = 0;
        public override void OnFire(Player player, Vector2 muzzlePos, Vector2 velocity, float speed, int damage, float knockback, int projToShoot)
        {
            shotCounter++;
            bool isEmpowered = shotCounter >= 5;
            if (isEmpowered)
            {
                shotCounter = 0;
                SoundEngine.PlaySound(SoundID.Item125, player.Center);
                recoil = 15f;
                for (int i = -1; i <= 1; i++)
                {
                    Vector2 spreadVel = Projectile.velocity.RotatedBy(MathHelper.ToRadians(i * 6f)) * speed * 1.3f;
                    int proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzlePos, spreadVel, ModContent.ProjectileType<Ultimatum_bullet>(), (int)(damage * 1.5f), knockback * 1.5f, player.whoAmI);
                    if (proj < Main.maxProjectiles)
                    {
                        Main.projectile[proj].ai[0] = 1f;
                    }
                }
                SpawnMuzzleFlash(muzzlePos, Projectile.velocity, true);
            }
            else
            {
                SoundEngine.PlaySound(SoundID.Item91, player.Center);
                recoil = 8f;
                Vector2 spreadVel = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(2f)) * speed;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzlePos, spreadVel, ModContent.ProjectileType<Ultimatum_bullet>(), damage, knockback, player.whoAmI);
                SpawnMuzzleFlash(muzzlePos, Projectile.velocity, false);
            }
        }
        private void SpawnMuzzleFlash(Vector2 muzzlePos, Vector2 velocity, bool empowered)
        {
            Vector2 muzzleDir = velocity.SafeNormalize(Vector2.UnitX);
            int count = empowered ? 6 : 2;
            float scale = empowered ? 0.8f : 0.2f;
            int dustType = empowered ? DustID.Clentaminator_Cyan : DustID.Electric;
            for (int i = 0; i < count; i++)
            {
                Vector2 vel = muzzleDir * Main.rand.NextFloat(2f, 6f) + Main.rand.NextVector2Circular(2f, 2f);
                Dust d = Dust.NewDustDirect(muzzlePos, 0, 0, dustType, vel.X, vel.Y, 0, default, scale);
                d.noGravity = true;
                d.fadeIn = scale + 0.1f;
            }
            if (empowered)
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 vel = muzzleDir * Main.rand.NextFloat(1f, 4f) + Main.rand.NextVector2Circular(3f, 3f);
                    Dust d = Dust.NewDustDirect(muzzlePos, 0, 0, DustID.BlueTorch, vel.X, vel.Y, 0, default, 1.4f);
                    d.noGravity = true;
                }
            }
        }
    }
}
