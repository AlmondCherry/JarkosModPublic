using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
using jarkos.projectiles.mecha;
namespace Jarkos.projectiles.mecha
{
    public class Plasma_wave_Holdout : BaseRangedHoldout
    {
        public override string Texture => "jarkos/items/weapons/ranged/firing/Plasma_wave";
        public override float MuzzleForwardOffset => 95f;
        public override float MuzzleVerticalOffset => 2f;
        public override Vector2 HoldoutOrigin => new Vector2(46f, ModContent.Request<Texture2D>(Texture).Height() / 2f);
        public override float BaseRecoil => 12f;
        public override int FirstShotDelay => 30;
        public override void OnFire(Player player, Vector2 muzzlePos, Vector2 velocity, float speed, int damage, float knockback, int projToShoot)
        {
            SoundEngine.PlaySound(SoundID.Item91 with { Pitch = 0.3f, Volume = 0.8f }, Projectile.position);
            SoundEngine.PlaySound(SoundID.Item125 with { Pitch = -0.2f, Volume = 0.6f }, Projectile.position);
            for (int i = 0; i < 6; i++)
            {
                Vector2 smokeVel = Projectile.velocity.RotatedByRandom(0.4f) * Main.rand.NextFloat(2f, 5f);
                Dust smoke = Dust.NewDustPerfect(muzzlePos, DustID.Smoke, smokeVel, 100, Color.Gray, Main.rand.NextFloat(0.8f, 1.4f));
                smoke.noGravity = true;
            }
            for (int i = 0; i < 5; i++)
            {
                Vector2 sparkVel = Projectile.velocity.RotatedByRandom(0.6f) * Main.rand.NextFloat(4f, 8f);
                new Spark(muzzlePos, sparkVel, new Color(0, 255, 180), 20, Main.rand.NextFloat(0.8f, 1.3f), 1f, 0f).Spawn();
            }
            new Nova_particle(muzzlePos, Projectile.velocity * 2f, new Color(0, 255, 180), 15, Main.rand.NextFloat(0.7f, 1.0f), 1f, 0f).Spawn();
            Vector2 projectileVel = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(3)) * speed;
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzlePos, projectileVel, ModContent.ProjectileType<Plasma_proj>(), damage, knockback, Projectile.owner);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteEffects spriteEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            float currentRecoil = recoil;
            Vector2 shakeOffset = Vector2.Zero;
            if (recoil > 0.5f)
            {
                shakeOffset = Main.rand.NextVector2Circular(1.5f, 1.5f);
            }
            Vector2 drawPos = Projectile.Center - Main.screenPosition - Projectile.velocity * currentRecoil + shakeOffset;
            float rotation = Projectile.velocity.ToRotation();
            Main.EntitySpriteDraw(texture, drawPos, null, lightColor, rotation, HoldoutOrigin, Projectile.scale, spriteEffects, 0);
            return false;
        }
    }
}
