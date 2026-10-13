using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.mecha
{
    public class Nucleon_ripper_Holdout : BaseRangedHoldout
    {
        public override string Texture => "jarkos/items/weapons/ranged/firing/Nucleon_ripper";
        public override float MuzzleForwardOffset => 115f;
        public override float MuzzleVerticalOffset => -14f;
        public override float BaseRecoil => 8f;
        public override int FirstShotDelay => 30;
        public override Vector2 HoldoutOrigin => new Vector2(48f, ModContent.Request<Texture2D>(Texture).Height() / 2f + 32f * Projectile.spriteDirection);
        public override bool ConsumeAmmo() => false;
        public override void OnFire(Player player, Vector2 muzzlePos, Vector2 velocity, float speed, int damage, float knockback, int projToShoot)
        {
            SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/Nucleon_shooting") with { Pitch = Main.rand.NextFloat(-0.35f, 0.35f) }, Projectile.position);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzlePos, Projectile.velocity * speed, ModContent.ProjectileType<Nucleon_ripper_orb>(), damage, knockback, Projectile.owner);
        }
    }
}
