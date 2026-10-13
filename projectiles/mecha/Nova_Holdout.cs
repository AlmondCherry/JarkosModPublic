using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
namespace jarkos.projectiles.mecha
{
    public class Nova_Holdout : BaseRangedHoldout
    {
        public override string Texture => "jarkos/items/weapons/ranged/firing/Nova";
        public override float MuzzleForwardOffset => 130f;
        public override float MuzzleVerticalOffset => 2f;
        public override Vector2 HoldoutOrigin => new Vector2(40f, ModContent.Request<Texture2D>(Texture).Height() / 2f);
        public override float BaseRecoil => 12f;
        public override float RecoilDecay => 0.85f;
        public override int FirstShotDelay => 30;
        public override float CustomShootSpeed => 24f;
        public override int GetUseTime(Player player)
        {
            int useTime = player.HeldItem.useTime;
            return useTime <= 0 ? 72 : useTime;
        }
        public override void OnFire(Player player, Vector2 muzzlePos, Vector2 velocity, float speed, int damage, float knockback, int projToShoot)
        {
            SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/NovaShot") with { Pitch = Main.rand.NextFloat(-0.1f, 0.1f) }, Projectile.position);
            for (int i = 0; i < 6; i++)
            {
                Vector2 smokeVel = Projectile.velocity.RotatedByRandom(0.4f) * Main.rand.NextFloat(2f, 5f);
                Dust smoke = Dust.NewDustPerfect(muzzlePos, DustID.Smoke, smokeVel, 100, Color.Gray, Main.rand.NextFloat(0.8f, 1.4f));
                smoke.noGravity = true;
            }
            for (int i = 0; i < 8; i++)
            {
                Vector2 sparkVel = Projectile.velocity.RotatedByRandom(0.6f) * Main.rand.NextFloat(3f, 8f);
                new Spark(muzzlePos, sparkVel, new Color(255, 60, 200), 18, Main.rand.NextFloat(0.8f, 1.3f), 1f).Spawn();
            }
            Vector2 projectileVel = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(2)) * speed;
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzlePos, projectileVel, ModContent.ProjectileType<Nova_proj>(), damage, knockback, Projectile.owner);
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
