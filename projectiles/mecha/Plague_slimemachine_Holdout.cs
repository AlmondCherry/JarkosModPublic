using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.projectiles.ranged.firearm;
namespace jarkos.projectiles.mecha
{
    public class Plague_slimemachine_Holdout : BaseRangedHoldout
    {
        public override string Texture => "jarkos/items/weapons/ranged/firing/Plague_slimemachine";
        public override float MuzzleForwardOffset => 42f;
        public override float MuzzleVerticalOffset => -5f;
        public override float BaseRecoil => 12f;
        public override int FirstShotDelay => 30;
        public override float CustomShootSpeed => 16f;
        public override void OnFire(Player player, Vector2 muzzlePos, Vector2 velocity, float speed, int damage, float knockback, int projToShoot)
        {
            SoundEngine.PlaySound(SoundID.Item41, Projectile.position);
            Vector2 shotVel = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(5f)) * speed;
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzlePos, shotVel, ModContent.ProjectileType<Plague_slimebomb>(), damage, knockback, Projectile.owner);
            for (int i = 0; i < 6; i++)
            {
                Dust d = Dust.NewDustDirect(muzzlePos, 0, 0, DustID.Venom, shotVel.X * 0.2f, shotVel.Y * 0.2f, 100, default, 1.5f);
                d.noGravity = true;
            }
        }
    }
}
