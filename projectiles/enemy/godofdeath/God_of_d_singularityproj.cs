using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.npc.godofdeath;
namespace jarkos.projectiles.enemy.godofdeath
{
	public class God_of_d_singularityproj : ModProjectile
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
			Projectile.width = 160;
			Projectile.height = 160;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 260;
			Projectile.alpha = 255;
			CooldownSlot = 1;
		}
		private int Timer = 0;
		private const int FadeTime = 40;
		public override void AI()
		{
			Timer++;
			if (Projectile.timeLeft > FadeTime)
			{
				if (Opacity < 1f)
				{
					float progress = MathHelper.Clamp(Timer / (float)FadeTime, 0f, 1f);
					Opacity = 1f - MathF.Pow(1f - progress, 3f);
				}
			}
			else
			{
				float progress = MathHelper.Clamp(Projectile.timeLeft / (float)FadeTime, 0f, 1f);
				Opacity = progress * progress;
			}
			if (Main.netMode != NetmodeID.Server)
			{
				if (!Filters.Scene["Jarkos:GoDSingularity"].Active)
				{
					Filters.Scene.Activate("Jarkos:GoDSingularity", Projectile.Center);
				}
				Filters.Scene["Jarkos:GoDSingularity"].GetShader()
					.UseTargetPosition(Projectile.Center)
					.UseProgress(Opacity)
					.UseOpacity(Opacity);
			}
			if (Main.netMode != NetmodeID.Server)
			{
				Player player = Main.LocalPlayer;
				if (player.active && !player.dead)
				{
					Vector2 toCenter = Projectile.Center - player.Center;
					float distance = toCenter.Length();
					if (distance < 1400f)
					{
						float pullForce = MathHelper.Lerp(7.5f, 0.4f, distance / 1400f) * Opacity;
						player.velocity += toCenter.SafeNormalize(Vector2.Zero) * pullForce;
					}
				}
			}
			if (Timer > FadeTime && Projectile.timeLeft > FadeTime)
			{
				if (Timer % 10 == 0)
				{
					if (Main.netMode != NetmodeID.MultiplayerClient)
					{
						float baseAngle = Timer * 0.08f;
						for (int i = 0; i < 2; i++)
						{
							float angle = baseAngle + i * MathF.PI;
							Vector2 velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 4.5f;
							int proj = Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, velocity, ModContent.ProjectileType<Smokeproj>(), 20, 1f, Main.myPlayer);
							if (proj >= 0 && proj < Main.maxProjectiles)
							{
								Main.projectile[proj].netUpdate = true;
							}
						}
					}
					if (Main.netMode != NetmodeID.Server && Timer % 30 == 0)
					{
						SoundEngine.PlaySound(SoundID.Item103 with { Volume = 0.4f, Pitch = -0.4f }, Projectile.Center);
					}
				}
			}
			if (Main.netMode != NetmodeID.Server && Main.rand.NextBool(2))
			{
				Vector2 startOffset = Main.rand.NextVector2CircularEdge(220f, 220f);
				Vector2 startPos = Projectile.Center + startOffset;
				Vector2 speed = -startOffset.SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(4f, 7f);
				new jarkos.particles.VoidShardParticle(startPos, speed, Color.Black, 35, 0.9f, 0.95f).Spawn();
			}
		}
		public override void OnKill(int timeLeft)
		{
			if (Main.netMode != NetmodeID.Server)
			{
				if (Filters.Scene["Jarkos:GoDSingularity"].Active)
				{
					Filters.Scene.Deactivate("Jarkos:GoDSingularity");
				}
			}
		}
		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.MagicPixel.Value;
			Vector2 drawPos = Projectile.Center - Main.screenPosition;
			Vector2 origin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
			Main.EntitySpriteDraw(texture, drawPos, null, Color.Black * Opacity * 0.6f, 0f, origin, Projectile.scale * 40f, SpriteEffects.None, 0);
			return false;
		}
	}
}
