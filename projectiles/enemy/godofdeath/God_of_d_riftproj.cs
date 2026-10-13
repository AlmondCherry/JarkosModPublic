using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.npc.godofdeath;
namespace jarkos.projectiles.enemy.godofdeath
{
	public class God_of_d_riftproj : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_0";
		public float Opacity
		{
			get => 1f - Projectile.alpha / 255f;
			set => Projectile.alpha = (int)((1f - value) * 255f);
		}
		public override void SetDefaults()
		{
			Projectile.netImportant = true;
			Projectile.width = 90;
			Projectile.height = 90;
			Projectile.hostile = false;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 180;
			Projectile.alpha = 255;
		}
		private int Timer = 0;
		private const int FadeTime = 25;
		public override void AI()
		{
			Timer++;
			if (Projectile.timeLeft > FadeTime)
			{
				if (Opacity < 1f)
				{
					float progress = MathHelper.Clamp(Timer / (float)FadeTime, 0f, 1f);
					Opacity = 1f - MathF.Pow(1f - progress, 2f);
				}
			}
			else
			{
				float progress = MathHelper.Clamp(Projectile.timeLeft / (float)FadeTime, 0f, 1f);
				Opacity = progress * progress;
			}
			if (Timer > FadeTime && Projectile.timeLeft > FadeTime)
			{
				if (Timer % 35 == 0)
				{
					int targetIndex = Player.FindClosest(Projectile.Center, 1, 1);
					if (targetIndex != -1)
					{
						Player target = Main.player[targetIndex];
						Vector2 shootVel = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY) * 8.5f;
						if (Main.netMode != NetmodeID.Server)
						{
							SoundEngine.PlaySound(SoundID.Item8 with { Volume = 0.5f, Pitch = -0.3f }, Projectile.Center);
						}
						if (Main.netMode != NetmodeID.MultiplayerClient)
						{
							int proj = Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, shootVel, ModContent.ProjectileType<Smokeproj>(), 18, 1f, Main.myPlayer);
							if (proj >= 0 && proj < Main.maxProjectiles)
							{
								Main.projectile[proj].netUpdate = true;
							}
						}
						if (Main.netMode != NetmodeID.Server)
						{
							for (int i = 0; i < 8; i++)
							{
								Vector2 particleSpeed = shootVel.RotatedByRandom(0.4f) * Main.rand.NextFloat(0.3f, 0.8f);
								new jarkos.particles.VoidShardParticle(Projectile.Center, particleSpeed, Color.Black, 25, 0.9f, 0.93f).Spawn();
							}
						}
					}
				}
			}
			if (Main.rand.NextBool(4))
			{
				Vector2 particlePos = Projectile.Center + Main.rand.NextVector2Circular(20f, 20f);
				Vector2 particleSpeed = Main.rand.NextVector2Circular(1f, 1f);
				Dust d = Dust.NewDustDirect(particlePos, 1, 1, DustID.Shadowflame, particleSpeed.X, particleSpeed.Y, 150, default, 0.8f);
				d.noGravity = true;
			}
		}
		public override bool PreDraw(ref Color lightColor)
		{
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			var shader = GameShaders.Misc["Jarkos:GoDVoidRift"];
			shader.UseOpacity(Opacity);
			shader.Shader.Parameters["uTime"].SetValue((float)Main.timeForVisualEffects * 0.04f);
			shader.Apply();
			Texture2D texture = TextureAssets.MagicPixel.Value;
			Rectangle drawRect = new Rectangle((int)(Projectile.Center.X - Main.screenPosition.X - Projectile.width / 2),
											   (int)(Projectile.Center.Y - Main.screenPosition.Y - Projectile.height / 2),
											   Projectile.width,
											   Projectile.height);
			Main.spriteBatch.Draw(texture, drawRect, Color.White * Opacity);
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			return false;
		}
	}
}
