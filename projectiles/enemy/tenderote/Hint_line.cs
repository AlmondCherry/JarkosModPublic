using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.GameContent;
namespace jarkos.projectiles.enemy.tenderote
{
	public class Hint_line : ModProjectile
	{
		public override void SetDefaults() {
			Projectile.width = 20;
			Projectile.height = 20;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.ignoreWater = true;
			Projectile.light = 1f;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 300;
			Projectile.penetrate = 1;
		}
        public override void AI()
        {
			Player player = Main.player[Projectile.owner];
			Projectile.velocity *= 0;
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }
		public override bool PreDraw(ref Color lightcolor) {
			Main.spriteBatch.End();
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            var origin = texture.Size() / 2f;
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, origin, 1f, SpriteEffects.None, 0f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.Camera.Sampler, DepthStencilState.None, Main.Camera.Rasterizer, null, Main.Camera.GameViewMatrix.TransformationMatrix);
			return false;
        }
    }
}
