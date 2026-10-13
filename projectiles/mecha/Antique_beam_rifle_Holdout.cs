using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.projectiles.ranged.firearm.beamrifle;
using Microsoft.Xna.Framework.Graphics;
namespace jarkos.projectiles.mecha
{
    public class Antique_beam_rifle_Holdout : BaseRangedHoldout
    {
        public override string Texture => "jarkos/items/weapons/ranged/firing/Antique_beam_rifle";
        public override Vector2 HoldoutOrigin => new Vector2(25f, ModContent.Request<Texture2D>(Texture).Height() / 2f);
        public override float MuzzleForwardOffset => 60f;
        public override float MuzzleVerticalOffset => -2f;
        public override float BaseRecoil => 10f;
        public override int FirstShotDelay => 30;
        public override int GetUseTime(Player player)
        {
            return 45;
        }
        public override void OnFire(Player player, Vector2 muzzlePos, Vector2 velocity, float speed, int damage, float knockback, int projToShoot)
        {
            SoundEngine.PlaySound(SoundID.Item40, Projectile.position);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzlePos, velocity * 16f, projToShoot, damage, knockback, player.whoAmI);
            for (int i = 0; i < 5; i++)
            {
                Dust d = Dust.NewDustDirect(muzzlePos, 0, 0, DustID.YellowTorch, velocity.X * Main.rand.NextFloat(0.5f, 1.5f), velocity.Y * Main.rand.NextFloat(0.5f, 1.5f), 0, default, 1.5f);
                d.noGravity = true;
            }
            Vector2 perpendicular = Vector2.Normalize(velocity).RotatedBy(MathHelper.PiOver2);
            float sideOffset = 15f;
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzlePos + perpendicular * sideOffset, velocity * 14f, ModContent.ProjectileType<Antique_beam_side_proj>(), damage, knockback, player.whoAmI);
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzlePos - perpendicular * sideOffset, velocity * 14f, ModContent.ProjectileType<Antique_beam_side_proj>(), damage, knockback, player.whoAmI);
        }
    }
}
