using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;
using jarkos.projectiles.ranged.firearm;
using jarkos.systems;
using jarkos.projectiles.bullet;
namespace jarkos.projectiles.mecha
{
    public class Electric_arrestor_Holdout : BaseRangedHoldout
    {
        public override string Texture => "jarkos/items/weapons/ranged/firing/Electric_arrestor";
        public override float MuzzleForwardOffset => 50f;
        public override float MuzzleVerticalOffset => 8f;
        public override bool ConsumeAmmo() => false;
        public override void OnFire(Player player, Vector2 muzzlePos, Vector2 velocity, float speed, int damage, float knockback, int projToShoot)
        {
            SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/Electric_arrestor") with { Pitch = Main.rand.NextFloat(-0.35f, 0.35f) }, Projectile.position);
            LightningSystemLaser.MakeProjectileLighting(muzzlePos, Main.MouseWorld, ModContent.ProjectileType<Lighting_beam>(), damage, knockback, 0.75f, 120f, 1);
        }
    }
}
