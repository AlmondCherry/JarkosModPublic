using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
namespace Jarkos.projectiles.mecha
{
    public class High_toxic_laser_Holdout : jarkos.projectiles.mecha.BaseRangedHoldout
    {
        public override string Texture => "jarkos/items/weapons/ranged/firing/High_toxic_laser";
        public override float MuzzleForwardOffset => 112f;
        public override float MuzzleVerticalOffset => 2f;
        public override Vector2 HoldoutOrigin => new Vector2(52f, ModContent.Request<Texture2D>(Texture).Height() / 2f);
        public override float BaseRecoil => 8.5f;
        public override float RecoilDecay => 0.83f;
        public override int GetUseTime(Player player)
        {
            int useTime = player.HeldItem.useTime;
            return useTime <= 0 ? 21 : useTime;
        }
        public override void OnFire(Player player, Vector2 muzzlePos, Vector2 velocity, float speed, int damage, float knockback, int projToShoot)
        {
            SoundEngine.PlaySound(SoundID.Item91 with { Pitch = 0.3f, Volume = 0.8f }, Projectile.position);
            SoundEngine.PlaySound(SoundID.Item34 with { Pitch = -0.5f, Volume = 0.4f }, Projectile.position);
            for (int i = 0; i < 4; i++)
            {
                Vector2 smokeVel = Projectile.velocity.RotatedByRandom(0.4f) * Main.rand.NextFloat(1.5f, 4f);
                Dust smoke = Dust.NewDustPerfect(muzzlePos, DustID.Smoke, smokeVel, 100, Color.Gray, Main.rand.NextFloat(0.7f, 1.2f));
                smoke.noGravity = true;
            }
            for (int i = 0; i < 6; i++)
            {
                Vector2 sparkVel = Projectile.velocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(3f, 7f);
                Color sparkColor = Main.rand.NextBool(2) ? new Color(80, 255, 100) : new Color(120, 255, 80);
                new Spark(muzzlePos, sparkVel, sparkColor, 15, Main.rand.NextFloat(0.7f, 1.2f), 1f).Spawn();
            }
            int numLasers = Main.rand.Next(2, 4);
            float spreadAngle = MathHelper.ToRadians(12f);
            for (int i = 0; i < numLasers; i++)
            {
                Vector2 perturbedVelocity = Projectile.velocity.RotatedByRandom(spreadAngle);
                perturbedVelocity *= speed * (1f - Main.rand.NextFloat(0.15f));
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzlePos, perturbedVelocity, ModContent.ProjectileType<Toxic_blast>(), damage, knockback, Projectile.owner, Main.rand.Next(3));
            }
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
