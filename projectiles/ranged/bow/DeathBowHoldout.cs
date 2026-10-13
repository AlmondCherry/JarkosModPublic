using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.projectiles.bullet;
using Terraria.Audio;

namespace jarkos.projectiles.ranged.bow
{
    public class DeathBowHoldout : BaseBowHoldout
    {
        public override string Texture => "jarkos/projectiles/ranged/bow/Death_bow_swing";
        public override bool IsAnimated => true;
        public override int FrameCount => 4;
        public override int AnimSpeed => 4;
        public override int SpriteWidth => 48;
        public override int SpriteHeight => 26;
        public override int ShootDelay => 20;
        public override float MuzzleOffset => 25f;
        public override float HoldoutRotationOffset => 0f;

        public override void SafeSetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 26;
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

            Vector2 extraVel = velocity.RotatedBy(Main.rand.NextFloat(-0.06f, 0.06f)) * 0.9f;
            Projectile.NewProjectile(source, muzzlePos, extraVel, ModContent.ProjectileType<Arrow_death>(), (int)(damage * 0.8f), knockback * 0.5f, player.whoAmI, 6, Projectile.GetByUUID(player.whoAmI, Projectile.whoAmI));

            SoundEngine.PlaySound(SoundID.Item5 with { Volume = 0.4f, Pitch = -0.2f }, muzzlePos);

            for (int i = 0; i < 4; i++)
            {
                Dust d = Dust.NewDustDirect(muzzlePos, 1, 1, DustID.PurpleTorch);
                d.velocity = velocity.SafeNormalize(Vector2.UnitX) * Main.rand.NextFloat(1f, 3f) + Main.rand.NextVector2Circular(1.5f, 1.5f);
                d.scale = Main.rand.NextFloat(0.7f, 1.2f);
                d.noGravity = true;
            }
        }
    }
}
