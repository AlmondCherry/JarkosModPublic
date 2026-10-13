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
	public class God_of_d_scytheproj : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.DeathSickle;
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
		}
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
			Projectile.timeLeft = 120;
			CooldownSlot = 1;
		}
		private Player Target => Main.player[(int)Projectile.ai[0]];
		private float StartAngle => Projectile.ai[1];
		private int Timer = 0;
		private const int Lifetime = 100;
		public override void AI()
		{
			Player player = Target;
			if (!player.active || player.dead)
			{
				Projectile.Kill();
				return;
			}
			Timer++;
			Projectile.timeLeft = 2;
			float progress = MathHelper.Clamp(Timer / (float)Lifetime, 0f, 1f);
			float easeOut = 1f - MathF.Pow(1f - progress, 3f);
			float currentRadius = MathHelper.Lerp(450f, 0f, easeOut);
			float rotationSpeed = 0.05f + (progress * 0.05f);
			float angle = StartAngle + Timer * rotationSpeed;
			Projectile.Center = player.Center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * currentRadius;
			Projectile.rotation += 0.25f;
			if (Main.rand.NextBool(3))
			{
				Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame, 0f, 0f, 150, default, 1.2f);
				d.noGravity = true;
				d.velocity *= 0.3f;
			}
			if (Timer >= Lifetime)
			{
				Projectile.Kill();
			}
		}
		public override void OnKill(int timeLeft)
		{
			SoundEngine.PlaySound(SoundID.Item74 with { Volume = 0.6f, Pitch = 0.2f }, Projectile.Center);
			if (Main.netMode != NetmodeID.Server)
			{
				for (int i = 0; i < 20; i++)
				{
					Vector2 speed = Main.rand.NextVector2Circular(5f, 5f);
					new jarkos.particles.VoidShardParticle(Projectile.Center, speed, Color.Black, 30, 1.1f, 0.95f).Spawn();
					Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame, speed.X, speed.Y, 100, default, 1.5f);
					d.noGravity = true;
				}
			}
		}
		public override bool PreDraw(ref Color lightColor)
		{
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			var shader = GameShaders.Misc["Jarkos:GoDScytheGlow"];
			shader.UseOpacity(1f);
			shader.Shader.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * 0.04f);
			shader.Apply();
			Texture2D texture = TextureAssets.Projectile[Type].Value;
			Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
			Vector2 drawPos = Projectile.Center - Main.screenPosition;
			for (int k = 0; k < Projectile.oldPos.Length; k++)
			{
				Vector2 oldDrawPos = Projectile.oldPos[k] + Projectile.Size / 2f - Main.screenPosition;
				Color color = new Color(120, 30, 200) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length) * 0.35f;
				Main.spriteBatch.Draw(texture, oldDrawPos, null, color, Projectile.rotation - k * 0.08f, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
			}
			Main.spriteBatch.Draw(texture, drawPos, null, Color.White, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			return false;
		}
	}
}
