using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Jarkos.projectiles.mecha
{
    public class Ignis_shot_Holdout : jarkos.projectiles.mecha.BaseRangedHoldout
    {
        public override string Texture => "jarkos/items/weapons/ranged/firing/Ignis_shot";
        public override float MuzzleForwardOffset => 135f;
        public override float MuzzleVerticalOffset => -8f;
        public override int FirstShotDelay => 30;
        public override float CustomShootSpeed => 22f;
        public override Vector2 HoldoutOrigin => new Vector2(48f, ModContent.Request<Texture2D>(Texture).Height() / 2f);

        public override int GetUseTime(Player player)
        {
            int useTime = player.HeldItem.useTime;
            return useTime <= 0 ? 45 : useTime;
        }

        public override void OnFire(Player player, Vector2 muzzlePos, Vector2 velocity, float speed, int damage, float knockback, int projToShoot)
        {
            recoil = 8f;
            SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/standart_mechashot") with { Pitch = Main.rand.NextFloat(-0.35f, 0.35f) }, Projectile.position);
            for (int i = 0; i < 15; i++)
            {
                Vector2 dustVel = velocity.RotatedByRandom(MathHelper.ToRadians(25)) * Main.rand.NextFloat(0.3f, 0.8f);
                Dust dust = Dust.NewDustDirect(muzzlePos, 0, 0, DustID.Lava, dustVel.X, dustVel.Y, 100, default, 2f);
                dust.noGravity = true;
            }
            for (int i = 0; i < 8; i++)
            {
                Dust flash = Dust.NewDustDirect(muzzlePos, 0, 0, DustID.Torch, 0, 0, 0, default, 3f);
                flash.velocity = Main.rand.NextVector2Circular(8f, 8f);
                flash.noGravity = true;
            }
            if (Projectile.owner == Main.myPlayer)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzlePos, velocity * speed, ModContent.ProjectileType<Ignis_shot_proj>(), damage, knockback, Projectile.owner);
            }
        }
    }
}
