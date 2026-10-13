using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.projectiles.mecha;
namespace Jarkos.projectiles.mecha
{
    public class M_blaster_Holdout : BaseRangedHoldout
    {
        public override string Texture => "jarkos/items/weapons/ranged/firing/M_blaster";
        public override float MuzzleForwardOffset => 102f;
        public override float MuzzleVerticalOffset => -4f;
        public override Vector2 HoldoutOrigin => new Vector2(40f, ModContent.Request<Texture2D>(Texture).Height() / 2f);
        public override float BaseRecoil => 4f;
        public override int FirstShotDelay => 30;
        public override float CustomShootSpeed => 14f;
        public override void OnFire(Player player, Vector2 muzzlePos, Vector2 velocity, float speed, int damage, float knockback, int projToShoot)
        {
            SoundEngine.PlaySound(SoundID.Item91 with { Pitch = -0.3f, Volume = 1.2f }, Projectile.position);
            for (int i = 0; i < 6; i++)
            {
                Vector2 smokeVel = Projectile.velocity.RotatedByRandom(0.4f) * Main.rand.NextFloat(2f, 5f);
                Dust smoke = Dust.NewDustPerfect(muzzlePos, DustID.GreenTorch, smokeVel, 100, default, Main.rand.NextFloat(1.2f, 1.8f));
                smoke.noGravity = true;
            }
            for (int i = 0; i < 2; i++)
            {
                Vector2 projectileVel = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(6f)) * speed;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzlePos, projectileVel, ModContent.ProjectileType<M_blast_proj_green>(), damage, knockback, Projectile.owner);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteEffects spriteEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            float currentRecoil = recoil;
            Vector2 drawPos = Projectile.Center - Main.screenPosition - Projectile.velocity * currentRecoil;
            float rotation = Projectile.velocity.ToRotation();
            Main.EntitySpriteDraw(texture, drawPos, null, lightColor, rotation, HoldoutOrigin, Projectile.scale * 0.5f, spriteEffects, 0);
            return false;
        }
    }
}
