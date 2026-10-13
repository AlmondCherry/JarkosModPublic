using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.enemy.godofdeath
{
	public class God_of_d_voidpoolproj : ModProjectile
	{
		public float Opacity
		{
			get => 1f - Projectile.alpha / 255f;
			set => Projectile.alpha = (int)((1f - value) * 255f);
		}
		public override string Texture => "Terraria/Images/Projectile_708";
		public override void SetDefaults()
		{
			Projectile.netImportant = true;
			Projectile.width = 70;
			Projectile.height = 20;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 240;
			Projectile.alpha = 0;
			CooldownSlot = 1;
		}
		private int Timer = 0;
		private const int FadeOutTime = 30;
		public override void AI()
		{
			Timer++;
			Projectile.velocity.Y += 0.3f;
			Projectile.velocity.X *= 0.95f;
			if (Projectile.timeLeft <= FadeOutTime)
			{
				float progress = MathHelper.Clamp(Projectile.timeLeft / (float)FadeOutTime, 0f, 1f);
				Opacity = progress * progress;
			}
			if (Main.rand.NextBool(8))
			{
				Vector2 dustPos = Projectile.position + new Vector2(Main.rand.NextFloat(Projectile.width), Main.rand.NextFloat(Projectile.height));
				Dust d = Dust.NewDustDirect(dustPos, 1, 1, DustID.Shadowflame, 0f, -Main.rand.NextFloat(0.8f, 2.2f), 120, default, 1f);
				d.noGravity = true;
			}
		}
		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterModifier)
		{
			fallThrough = false;
			return true;
		}
		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			Projectile.velocity = Vector2.Zero;
			return false;
		}
		public override bool PreDraw(ref Color lightColor)
		{
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			var shader = GameShaders.Misc["Jarkos:GoDVoidPool"];

			shader.UseOpacity(Opacity);
			shader.Shader.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * 0.04f);
			shader.Apply();

			Texture2D texture = TextureAssets.Projectile[Type].Value;
			int frameHeight = texture.Height;

			Rectangle sourceRect = new Rectangle(0, 0, texture.Width, frameHeight);
			Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, frameHeight * 0.5f);
			Vector2 drawPos = Projectile.Center - Main.screenPosition;
			Vector2 scale = new Vector2(Projectile.width / (float)texture.Width, Projectile.height / (float)frameHeight) * Projectile.scale * 1.5f;

			Main.spriteBatch.Draw(texture, drawPos, sourceRect, Color.White * Opacity, 0f, drawOrigin, scale, SpriteEffects.None, 0);
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			return false;
		}
	}
}
