using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.projectiles.bullet;
using Terraria.Audio;
namespace jarkos.projectiles.ranged.bow
{
    public class EchoBowHoldout : BaseBowHoldout
    {
        public override string Texture => "jarkos/projectiles/ranged/bow/Echo_bow_Holdout";
        public override bool IsAnimated => true;
        public override int FrameCount => 4;
        public override int AnimSpeed => 4;
        public override int SpriteWidth => 30;
        public override int SpriteHeight => 52;
        public override int ShootDelay => 18;
        public override float MuzzleOffset => 25f;
        public override float HoldoutRotationOffset => 0f;
        public override void SafeSetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 52;
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
            IEntitySource source = Projectile.GetSource_FromThis();
            Projectile.NewProjectile(source, muzzlePos, velocity, type, damage, knockback, player.whoAmI);
            float spread = 0.15f;
            for (int i = 0; i < 2; i++)
            {
                float angleOffset = (i == 0) ? -spread : spread;
                Vector2 lightVel = velocity.RotatedBy(angleOffset) * 0.85f;
                Vector2 offset = new Vector2(Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f));
                Projectile.NewProjectile(source, muzzlePos + offset, lightVel, ModContent.ProjectileType<Echo_arrow>(), (int)(damage * 0.7f), knockback * 0.5f, player.whoAmI);
            }

            SoundEngine.PlaySound(SoundID.Item5, muzzlePos);

            for (int i = 0; i < 4; i++)
            {
                Dust d = Dust.NewDustDirect(muzzlePos, 1, 1, DustID.GoldFlame);
                d.velocity = velocity.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(1f, 3f)
                    + Main.rand.NextVector2Circular(1.5f, 1.5f);
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
                d.noGravity = true;
            }
        }
    }
}
