using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using ReLogic.Content;
using jarkos.particles;

namespace jarkos.projectiles.bullet
{
	public class Bullet_otch : ModProjectile
	{
		private List<TrailPoint> trailPoints = new List<TrailPoint>();
		private const int TRAIL_LENGTH = 15;
		private float pulseTimer = 0f;

		private struct TrailPoint
		{
			public Vector2 Position;
			public float Rotation;
		}

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = TRAIL_LENGTH;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.width = 6;
			Projectile.height = 14;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 600;
			Projectile.tileCollide = true;
			Projectile.aiStyle = 0;
			Projectile.extraUpdates = 2;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}

		public override void AI()
		{
			pulseTimer += 0.15f;
			UpdateTrail();
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

			CreateFireEffects();
			UpdateLighting();
			UpdateSounds();
		}

		private void UpdateTrail()
		{
			trailPoints.Insert(0, new TrailPoint
			{
				Position = Projectile.Center,
				Rotation = Projectile.rotation
			});
			while (trailPoints.Count > TRAIL_LENGTH)
			{
				trailPoints.RemoveAt(trailPoints.Count - 1);
			}
		}

		private void CreateFireEffects()
		{
			if (Main.rand.NextBool(2))
			{
				Dust fire = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(3, 3), DustID.Torch, -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(0.5f, 0.5f), 0, default, Main.rand.NextFloat(1.2f, 1.8f));
				fire.noGravity = true;
				fire.fadeIn = 0.4f;
			}
			if (Main.rand.NextBool(3))
			{
				Dust spark = Dust.NewDustPerfect(Projectile.Center, DustID.InfernoFork, Projectile.velocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(0.1f, 0.3f), 0, default, Main.rand.NextFloat(0.8f, 1.3f));
				spark.noGravity = true;
			}
			if (Main.rand.NextBool(5))
			{
				Dust smoke = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(2, 2), DustID.Smoke, new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), Main.rand.NextFloat(-1.2f, -0.4f)), 100, Color.DarkGray, Main.rand.NextFloat(0.4f, 0.8f));
				smoke.noGravity = true;
			}
		}

		private void UpdateLighting()
		{
			float flicker = 0.85f + (float)Math.Sin(Main.GameUpdateCount * 0.4f + Projectile.whoAmI) * 0.15f;
			Lighting.AddLight(Projectile.Center, 1f * flicker, 0.5f * flicker, 0.1f * flicker);
		}

		private void UpdateSounds()
		{
			if (Main.rand.NextBool(40))
			{
				SoundEngine.PlaySound(SoundID.DD2_BetsyFlameBreath with
				{
					Volume = 0.2f,
					Pitch = Main.rand.NextFloat(0.5f, 1f),
					MaxInstances = 2
				}, Projectile.Center);
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			target.AddBuff(BuffID.OnFire, 180);
			if (hit.Crit || Main.rand.NextBool(3))
			{
				target.AddBuff(BuffID.OnFire3, 90);
			}
			CreateHitEffects(target.Center);
		}

		private void CreateHitEffects(Vector2 position)
		{
			for (int i = 0; i < 15; i++)
			{
				Dust fire = Dust.NewDustPerfect(position + Main.rand.NextVector2Circular(12, 12), DustID.Torch, Main.rand.NextVector2Circular(5f, 5f), 0, default, Main.rand.NextFloat(1.5f, 2.5f));
				fire.noGravity = true;
			}

			for (int i = 0; i < 6; i++)
			{
				Vector2 vel = Main.rand.NextVector2Circular(6f, 6f) + new Vector2(0f, -3f);
				Color color = Color.Lerp(new Color(255, 110, 10), new Color(255, 225, 30), Main.rand.NextFloat());
				int lifetime = Main.rand.Next(20, 35);
				float scale = Main.rand.NextFloat(0.35f, 0.6f);
				new Spark_gravit(position, vel, color, lifetime, scale, 1f, Main.rand.NextFloat(-0.1f, 0.1f)).Spawn();
			}

			SoundEngine.PlaySound(SoundID.Item74 with
			{
				Volume = 0.7f,
				Pitch = 0.2f
			}, position);
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			CreateTileImpactEffect();
			return true;
		}

		private void CreateTileImpactEffect()
		{
			Vector2 pos = Projectile.Center;
			for (int i = 0; i < 12; i++)
			{
				float angle = MathHelper.TwoPi * i / 12f + Main.rand.NextFloat(-0.2f, 0.2f);
				Vector2 vel = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(2f, 5f);
				vel.Y = Math.Min(vel.Y, -1f);
				Dust splash = Dust.NewDustPerfect(pos + Main.rand.NextVector2Circular(5, 5), DustID.Lava, vel, 0, default, Main.rand.NextFloat(1f, 1.8f));
				splash.noGravity = false;
			}
			for (int i = 0; i < 10; i++)
			{
				Dust fire = Dust.NewDustPerfect(pos, DustID.Torch, Main.rand.NextVector2Circular(4f, 4f), 0, default, Main.rand.NextFloat(1.5f, 2.2f));
				fire.noGravity = true;
			}

			for (int i = 0; i < 6; i++)
			{
				Vector2 vel = Main.rand.NextVector2Circular(5f, 5f) + new Vector2(0f, -3f);
				Color color = Color.Lerp(new Color(255, 110, 10), new Color(255, 225, 30), Main.rand.NextFloat());
				int lifetime = Main.rand.Next(20, 35);
				float scale = Main.rand.NextFloat(0.35f, 0.6f);
				new Spark_gravit(pos, vel, color, lifetime, scale, 1f, Main.rand.NextFloat(-0.1f, 0.1f)).Spawn();
			}

			SoundEngine.PlaySound(SoundID.Item45 with
			{
				Volume = 0.6f,
				Pitch = 0.2f
			}, pos);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			SpriteBatch sb = Main.spriteBatch;
			Texture2D pixel = TextureAssets.MagicPixel.Value;
			Texture2D bulletTexture = TextureAssets.Projectile[Projectile.type].Value;

			sb.End();
			sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			DrawFireTrail(sb, pixel);
			DrawBulletGlow(sb, pixel);

			sb.End();
			sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			DrawBullet(sb, bulletTexture, lightColor);
			return false;
		}

		private void DrawFireTrail(SpriteBatch sb, Texture2D pixel)
		{
			if (trailPoints.Count < 2) return;
			Rectangle sourceRect = new Rectangle(0, 0, 1, 1);
			for (int i = 0; i < trailPoints.Count - 1; i++)
			{
				float progress = (float)i / trailPoints.Count;
				float alpha = (1f - progress) * 0.75f;
				TrailPoint point = trailPoints[i];
				TrailPoint nextPoint = trailPoints[Math.Min(i + 1, trailPoints.Count - 1)];
				Vector2 start = point.Position - Main.screenPosition;
				Vector2 end = nextPoint.Position - Main.screenPosition;
				Vector2 direction = end - start;
				float length = direction.Length();
				if (length < 0.1f) continue;
				float rotation = direction.ToRotation();
				float pulse = 0.85f + (float)Math.Sin(Main.GameUpdateCount * 0.35f + i * 0.4f) * 0.15f;

				float outerWidth = 16f * (1f - progress * 0.7f) * pulse;
				Color outerColor = new Color(255, 80, 0, 0) * alpha * 0.4f;
				sb.Draw(pixel, start, sourceRect, outerColor, rotation, new Vector2(0f, 0.5f), new Vector2(length + 2, outerWidth), SpriteEffects.None, 0f);

				float midWidth = 10f * (1f - progress * 0.6f) * pulse;
				Color midColor = new Color(255, 150, 30, 0) * alpha * 0.6f;
				sb.Draw(pixel, start, sourceRect, midColor, rotation, new Vector2(0f, 0.5f), new Vector2(length + 2, midWidth), SpriteEffects.None, 0f);

				float coreWidth = 4.5f * (1f - progress * 0.5f) * pulse;
				Color coreColor = new Color(255, 220, 100, 50) * alpha * 0.8f;
				sb.Draw(pixel, start, sourceRect, coreColor, rotation, new Vector2(0f, 0.5f), new Vector2(length + 2, coreWidth), SpriteEffects.None, 0f);
			}
		}

		private void DrawBulletGlow(SpriteBatch sb, Texture2D pixel)
		{
			Vector2 drawPos = Projectile.Center - Main.screenPosition;
			Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/particles/Circular_glow", AssetRequestMode.ImmediateLoad).Value;
			Vector2 glowOrigin = glowTex.Size() * 0.5f;

			Color orangeGlow = new Color(255, 100, 15) * 0.65f;
			Color brightOrangeGlow = new Color(255, 180, 40) * 0.4f;

			sb.Draw(glowTex, drawPos, null, orangeGlow, 0f, glowOrigin, 0.45f, SpriteEffects.None, 0f);
			sb.Draw(glowTex, drawPos, null, brightOrangeGlow, 0f, glowOrigin, 0.3f, SpriteEffects.None, 0f);
		}

		private void DrawBullet(SpriteBatch sb, Texture2D texture, Color lightColor)
		{
			Vector2 drawPos = Projectile.Center - Main.screenPosition;
			Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
			Color bulletColor = Color.Lerp(lightColor, new Color(255, 200, 150), 0.5f);
			bulletColor = Color.Lerp(bulletColor, Color.White, 0.3f);
			sb.Draw(texture, drawPos, null, bulletColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
		}

		public override void OnKill(int timeLeft)
		{
			for (int i = 0; i < 8; i++)
			{
				Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, Main.rand.NextVector2Circular(3f, 3f), 0, default, Main.rand.NextFloat(1f, 1.5f));
				dust.noGravity = true;
			}

			Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
		}
	}
}
