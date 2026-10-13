using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.Audio;
namespace jarkos.projectiles.ranged.bow
{
    public class FrostWeakBowHoldout : BaseBowHoldout
    {
        public override string Texture => "jarkos/projectiles/ranged/bow/FrostWeakBowHoldout";
        public override bool IsAnimated => true;
        public override int FrameCount => 4;
        public override int AnimSpeed => 5;
        public override int SpriteWidth => 38;
        public override int SpriteHeight => 110;
        public override int ShootDelay => 25;
        public override float MuzzleOffset => 40f;
        public override float HoldoutRotationOffset => 0f;
        public override void SafeSetDefaults()
        {
            Projectile.width = 38;
            Projectile.height = 110;
        }
        public override void OnFire(Player player, Vector2 muzzlePos, Vector2 velocity, int damage, float knockback)
        {
            Item weapon = player.inventory[player.selectedItem];
            int type = ProjectileID.WoodenArrowFriendly;
            bool canShoot = true;
            if (weapon.useAmmo > 0)
            {
                float speed = 0f;
                canShoot = player.PickAmmo(weapon, out type, out speed, out damage, out knockback, out _);
            }
            if (!canShoot)
            {
                Projectile.Kill();
                return;
            }

            SoundEngine.PlaySound(SoundID.Item5 with { Volume = 0.3f, Pitch = 0.6f}, muzzlePos);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzlePos, velocity, ModContent.ProjectileType<FrostWeakArrowProj>(), damage, knockback, player.whoAmI);
        }
    }
}
