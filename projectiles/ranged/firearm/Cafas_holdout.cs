using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.Audio;
using jarkos.projectiles.mecha;
using jarkos.systems;
using jarkos.projectiles.bullet;
using Terraria.GameContent;

namespace jarkos.projectiles.ranged.firearm
{
    public class Cafas_holdout : BaseRangedHoldout
    {
        public override string Texture => "jarkos/items/weapons/ranged/firing/Cafas";
        public override void SafeSetDefaults()
        {
            Main.projFrames[Projectile.type] = 8;
        }
        public override float BaseRecoil => 6f;
        public override float RecoilDecay => 0.85f;
        public override float MuzzleForwardOffset => 35f;
        public override float MuzzleVerticalOffset => -4f;
        public override void UpdateExtra(Player player)
        {
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 4)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= 8)
                {
                    Projectile.frame = 0;
                }
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            int startY = frameHeight * Projectile.frame;
            Rectangle sourceRectangle = new Rectangle(0, startY, texture.Width, frameHeight);
            SpriteEffects spriteEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipVertically : SpriteEffects.None;
            float currentRecoil = recoil;
            Vector2 drawPos = Projectile.Center - Main.screenPosition - Projectile.velocity * currentRecoil;
            float rotation = Projectile.velocity.ToRotation();
            Vector2 origin = new Vector2(16f, frameHeight / 2f);
            Main.EntitySpriteDraw(texture, drawPos, sourceRectangle, lightColor, rotation, origin, Projectile.scale, spriteEffects, 0);
            PostDrawHoldout(drawPos, rotation, spriteEffects);
            return false;
        }
        public override void OnFire(Player player, Vector2 muzzlePos, Vector2 velocity, float speed, int damage, float knockback, int projToShoot)
        {
			SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/cafas_sound") with { Pitch = Main.rand.NextFloat(-0.45f, 0.45f) }, player.position);
			LightningSystemLaser.MakeProjectileLighting(muzzlePos, Main.MouseWorld, ModContent.ProjectileType<Lighting_beam_cafas>(), damage, knockback, 0.75f, 120f, 1);
        }
    }
}
