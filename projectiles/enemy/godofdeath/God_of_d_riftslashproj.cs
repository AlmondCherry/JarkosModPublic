using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.enemy.godofdeath
{
	public class God_of_d_riftslashproj : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_0";
		public const int TelegraphTime = 45;
		public const int SlashTime = 20;
		public override void SetDefaults()
		{
			Projectile.netImportant = true;
			Projectile.width = 40;
			Projectile.height = 40;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = TelegraphTime + SlashTime;
			Projectile.scale = 1f;
			CooldownSlot = 1;
		}
		private int Timer = 0;
		public override void AI()
		{
			Timer++;
			if (Timer == TelegraphTime)
			{
				if (Main.netMode != NetmodeID.Server)
				{
					SoundEngine.PlaySound(SoundID.Item71 with { Volume = 1f, Pitch = -0.2f }, Projectile.Center);
				}
				if (Main.netMode != NetmodeID.Server)
				{
					Main.LocalPlayer.GetModPlayer<jarkos.Screenshake_system>().stronger_shake = 1f;
				}
			}
			if (Timer >= TelegraphTime && Main.netMode != NetmodeID.Server && Main.rand.NextBool(2))
			{
				float dist = Main.rand.NextFloat(0f, 2000f);
				Vector2 pos = Projectile.Center + new Vector2(MathF.Cos(Projectile.rotation), MathF.Sin(Projectile.rotation)) * dist;
				Vector2 shardSpeed = new Vector2(MathF.Cos(Projectile.rotation + MathF.PI/2f), MathF.Sin(Projectile.rotation + MathF.PI/2f)).RotatedByRandom(0.2f) * Main.rand.NextFloat(3f, 6f);
				new jarkos.particles.VoidShardParticle(pos, shardSpeed, Color.Black, 25, 0.8f, 0.94f).Spawn();
			}
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if (Timer < TelegraphTime)
				return false;
			float point = 0f;
			Vector2 end = Projectile.Center + new Vector2(MathF.Cos(Projectile.rotation), MathF.Sin(Projectile.rotation)) * 2400f;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, end, 32f * Projectile.scale, ref point);
		}
		public override bool PreDraw(ref Color lightColor)
		{
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			Texture2D texture = TextureAssets.MagicPixel.Value;
			Vector2 drawPos = Projectile.Center - Main.screenPosition;
			Vector2 origin = new Vector2(0f, 0.5f);
			if (Timer < TelegraphTime)
			{
				float progress = Timer / (float)TelegraphTime;
				float alpha = MathHelper.Lerp(0.15f, 0.75f, progress);
				float width = MathHelper.Lerp(1.5f, 7f, progress);
				Color warningColor = new Color(180, 30, 255) * alpha;
				Main.spriteBatch.Draw(texture, drawPos, new Rectangle(0, 0, 1, 1), warningColor, Projectile.rotation, origin, new Vector2(2400f, width), SpriteEffects.None, 0);
			}
			else
			{
				var shader = GameShaders.Misc["Jarkos:GoDRiftSlash"];
				float progress = (Timer - TelegraphTime) / (float)SlashTime;
				float alpha = 1f - progress;
				shader.UseOpacity(alpha);
				shader.Shader.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * 0.04f);
				shader.Apply();
				Main.spriteBatch.Draw(texture, drawPos, new Rectangle(0, 0, 1, 1), Color.White, Projectile.rotation, origin, new Vector2(2400f, 60f * Projectile.scale), SpriteEffects.None, 0);
			}
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			return false;
		}
	}
}
