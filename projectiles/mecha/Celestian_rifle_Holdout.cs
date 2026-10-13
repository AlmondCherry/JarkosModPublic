using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.projectiles.ranged.firearm;
using System;
namespace jarkos.projectiles.mecha
{
    public class Celestian_rifle_Holdout : BaseRangedHoldout
    {
        public override string Texture => "jarkos/items/weapons/ranged/firing/Celestian_rifle";
        public override float MuzzleForwardOffset => 33f;
        public override float MuzzleVerticalOffset => -5f;
        public override int FirstShotDelay => 30;
        public override int GetUseTime(Player player)
        {
            return 8;
        }
        public override void OnFire(Player player, Vector2 muzzlePos, Vector2 velocity, float speed, int damage, float knockback, int projToShoot)
        {
            SoundEngine.PlaySound(SoundID.Item75, Projectile.position);
            recoil = 6f;
            float spread = MathHelper.ToRadians(2f);
            for (int i = 0; i < 2; i++)
            {
                Vector2 perturbedVel = velocity.RotatedByRandom(spread);
                perturbedVel *= Main.rand.NextFloat(0.95f, 1.05f) * speed;
                Vector2 offset = Vector2.Normalize(velocity).RotatedBy(MathHelper.PiOver2) * (i == 0 ? -4f : 4f);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzlePos + offset, perturbedVel, ModContent.ProjectileType<Celestian_vortex_laser>(), damage, knockback, player.whoAmI);
            }
        }
        public override void PostDrawHoldout(Vector2 drawPos, float rotation, SpriteEffects spriteEffects)
        {
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.3f + 0.7f;
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Main.EntitySpriteDraw(texture, drawPos, null, new Color(100, 180, 255, 0) * 0.6f * pulse, rotation, HoldoutOrigin, Projectile.scale * 1.05f, spriteEffects, 0f);
        }
    }
}
