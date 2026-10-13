using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.systems;
using jarkos.projectiles.mecha;

namespace Jarkos.projectiles.mecha
{
    public class Twobarrel_atomgun_Holdout : BaseRangedHoldout
    {
        public override string Texture => "jarkos/items/weapons/ranged/firing/Twobarrel_atomgun";
        public override float MuzzleForwardOffset => 125f;
        public override float MuzzleVerticalOffset => -7f;
        public override Vector2 HoldoutOrigin => new Vector2(30f, ModContent.Request<Texture2D>(Texture).Height() / 2f);
        public override int FirstShotDelay => 20;

        private int spoolTimer = 0;

        public override int GetUseTime(Player player)
        {
            float spoolProgress = MathHelper.Clamp(spoolTimer / 300f, 0f, 1f);
            int baseUseTime = player.HeldItem.useTime > 0 ? player.HeldItem.useTime : 12;


            int currentUseTime = (int)MathHelper.Lerp(baseUseTime, Math.Max(2, baseUseTime / 2), spoolProgress);
            return currentUseTime;
        }

        public override void UpdateExtra(Player player)
        {

            spoolTimer++;
            if (spoolTimer > 300)
            {
                spoolTimer = 300;
            }
        }

        public override void OnFire(Player player, Vector2 muzzlePos, Vector2 velocity, float speed, int damage, float knockback, int projToShoot)
        {
            SoundEngine.PlaySound(SoundID.Item91 with { Volume = 0.70f, Pitch = Main.rand.NextFloat(-0.15f, 0.15f) }, Projectile.position);
            recoil = 3.5f;

            for (int i = 0; i < 3; i++)
            {
                Vector2 smokeVel = Projectile.velocity.RotatedByRandom(0.4f) * Main.rand.NextFloat(1.5f, 3.5f);
                Dust smoke = Dust.NewDustPerfect(muzzlePos, DustID.Smoke, smokeVel, 100, new Color(255, 240, 120), Main.rand.NextFloat(0.7f, 1.1f));
                smoke.noGravity = true;
            }
            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkVel = Projectile.velocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(3f, 7f);
                Dust Spark = Dust.NewDustPerfect(muzzlePos, DustID.YellowTorch, sparkVel, 0, default, Main.rand.NextFloat(1.2f, 1.7f));
                Spark.noGravity = true;
            }


            Vector2 projectileVel = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(2f)) * speed;
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzlePos, projectileVel, ModContent.ProjectileType<Atomic_two_yellow_proj>(), damage, knockback, Projectile.owner);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteEffects spriteEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

            float currentRecoil = recoil;
            Vector2 shakeOffset = Vector2.Zero;
            if (recoil > 0.2f)
            {
                shakeOffset = Main.rand.NextVector2Circular(0.8f, 0.8f);
            }

            Vector2 drawPos = Projectile.Center - Main.screenPosition - Projectile.velocity * currentRecoil + shakeOffset;
            float rotation = Projectile.velocity.ToRotation();
            Main.EntitySpriteDraw(texture, drawPos, null, lightColor, rotation, HoldoutOrigin, Projectile.scale, spriteEffects, 0);

            return false;
        }
    }
}
