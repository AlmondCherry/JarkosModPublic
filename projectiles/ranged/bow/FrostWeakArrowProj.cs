using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
namespace jarkos.projectiles.ranged.bow
{
    public class FrostWeakArrowProj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.aiStyle = 1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.arrow = true;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 1200;
            Projectile.light = 1f;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn, 180);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D magicPixel = TextureAssets.MagicPixel.Value;
            for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero || Projectile.oldPos[i + 1] == Vector2.Zero)
                    continue;
                Vector2 startPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 endPos = Projectile.oldPos[i + 1] + Projectile.Size / 2f - Main.screenPosition;
                float progress = 1f - (float)i / Projectile.oldPos.Length;
                float width = 6f * progress;
                Color color = Color.Lerp(Color.Cyan, Color.Blue, 1f - progress) * progress;
                Vector2 direction = endPos - startPos;
                float length = direction.Length();
                float rotation = direction.ToRotation();
                Main.EntitySpriteDraw(magicPixel, startPos, new Rectangle(0, 0, 1, 1), color, rotation, new Vector2(0f, 0.5f), new Vector2(length, width), SpriteEffects.None, 0);
            }
            return true;
        }
    }
}
