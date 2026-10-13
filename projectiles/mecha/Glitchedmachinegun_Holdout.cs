using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.mecha
{
    public class Glitchedmachinegun_Holdout : BaseRangedHoldout
    {
        public override string Texture => "jarkos/items/weapons/ranged/firing/Glitchedmachinegun";
        public override float MuzzleForwardOffset => 42f;
        public override float MuzzleVerticalOffset => -5f;
        public override float BaseRecoil => 8f;
        public override int FirstShotDelay => 30;
        public override void OnFire(Player player, Vector2 muzzlePos, Vector2 velocity, float speed, int damage, float knockback, int projToShoot)
        {
            if (projToShoot == ProjectileID.Bullet)
            {
                projToShoot = ProjectileID.BulletHighVelocity;
            }
            SoundEngine.PlaySound(SoundID.Item41, Projectile.position);
            Vector2 shotVel = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(4f)) * speed;
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzlePos, shotVel, projToShoot, damage, knockback, Projectile.owner);
        }
         }
}
