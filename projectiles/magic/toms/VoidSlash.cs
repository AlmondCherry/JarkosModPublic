using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.GameContent;
using System;
namespace jarkos.projectiles.magic.toms
{
	public class VoidSlash : ModProjectile
	{
		private VertexStrip _vertexStrip = new();
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 30;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 120;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
        }
        public override void AI()
        {
            Projectile.rotation += 0.25f;
            Lighting.AddLight(Projectile.Center, 0.1f, 0.5f, 0.8f);
            Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.ShimmerTorch);
            dust.noGravity = true;
            dust.scale = 1.3f;
            dust.velocity *= 0.2f;
        }
        private Color TrailColor(float progressOnStrip)
        {
            Color start = new Color(40, 255, 255);
            Color end = new Color(140, 0, 255);
            Color color = Color.Lerp(start, end, progressOnStrip);
            float alpha = 1f - progressOnStrip;
            return color * alpha;
        }
        private float TrailWidth(float progressOnStrip)
        {
            float width = MathHelper.Lerp(38f, 2f, progressOnStrip);
            float pulse = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 8f) * 3f;
            return width + pulse;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            DrawPrimitiveTrail();
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Main.EntitySpriteDraw(texture, drawPos, null, Color.White, Projectile.rotation, texture.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
        private void DrawPrimitiveTrail()
        {
            MiscShaderData miscShaderData = GameShaders.Misc["MagicMissile"];
            miscShaderData.UseSaturation(-2.8f);
            miscShaderData.UseOpacity(1.2f);
            Main.instance.GraphicsDevice.Textures[0] = TextureAssets.MagicPixel.Value;
            _vertexStrip.PrepareStrip(Projectile.oldPos, Projectile.oldRot, TrailColor, TrailWidth, -Main.screenPosition + Projectile.Size / 2f, Projectile.oldPos.Length, includeBacksides: true);
            _vertexStrip.DrawTrail();
        }
        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 35; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(6f, 6f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.ShimmerSpark, velocity);
                dust.noGravity = true;
                dust.scale = 1.6f;
            }
        }
    }
}
