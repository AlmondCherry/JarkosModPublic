using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.enemy.celestia
{
	public class CelestiaStarRay : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.Starfury;
		private const float EmergeTicks = 5f;
		private const float PauseTicks = 13f;
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 22;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}
		public override void SetDefaults()
		{
			Projectile.width = 14;
			Projectile.height = 14;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 360;
			Projectile.extraUpdates = 1;
		}
		public override void OnSpawn(IEntitySource source)
		{
			if (Projectile.ai[0] > 0f && Projectile.ai[1] > 0f)
			{
				Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitY);
				Projectile.localAI[1] = dir.ToRotation();
				Projectile.velocity = dir * (Projectile.ai[0] * 0.6f);
			}
		}
		public override void AI()
		{
			Projectile.localAI[0]++;
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			float targetSpeed = Projectile.ai[0];
			float rampTicks = Projectile.ai[1];
			bool burstMode = targetSpeed > 0f && rampTicks > 0f;
			if (burstMode)
			{
				Vector2 dir = Projectile.localAI[1].ToRotationVector2();
				float age = Projectile.localAI[0];
				float factor;
				if (age <= EmergeTicks)
				{
					float t = age / EmergeTicks;
					factor = MathHelper.Lerp(0.6f, 0.4f, t);
				}
				else if (age <= PauseTicks)
				{
					float t = (age - EmergeTicks) / (PauseTicks - EmergeTicks);
					factor = MathHelper.Lerp(0.4f, 0.04f, MathHelper.SmoothStep(0f, 1f, t));
				}
				else if (age <= rampTicks)
				{
					float window = Math.Max(1f, rampTicks - PauseTicks);
					float t = MathHelper.Clamp((age - PauseTicks) / window, 0f, 1f);
					float ease = t * t * t;
					factor = MathHelper.Lerp(0.04f, 1f, ease);
				}
				else
				{
					factor = 1f;
				}
				Projectile.velocity = dir * (targetSpeed * factor);
			}
			float fadeIn = Math.Min(1f, Projectile.localAI[0] / 16f);
			float fadeOut = 1f;
			if (Projectile.timeLeft < 70)
			{
				fadeOut = Projectile.timeLeft / 70f;
				if (!burstMode)
					Projectile.velocity *= 0.985f;
				else
					Projectile.velocity *= 0.992f;
			}
			float overall = fadeIn * fadeOut;
			Projectile.alpha = (int)MathHelper.Clamp((1f - overall) * 255f, 0f, 255f);
			Projectile.scale = 0.85f + overall * 0.25f;
			float speedFactorLight = ComputeSpeedFactor();
			Lighting.AddLight(Projectile.Center, (0.35f + speedFactorLight * 0.45f) * overall, (0.28f + speedFactorLight * 0.22f) * overall, (0.85f + speedFactorLight * 0.45f) * overall);
			if (!Main.dedServ && overall > 0.2f)
			{
				if (Main.rand.NextBool(2))
				{
					Dust trailDust = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch, -Projectile.velocity * 0.12f, (int)((1f - overall) * 200f), default, 1.35f * overall);
					trailDust.noGravity = true;
				}
				if (Main.rand.NextBool(4))
				{
					float pulse = (float)Math.Sin(Projectile.localAI[0] * 0.08f) * 0.5f + 0.5f;
					Vector2 offset = Main.rand.NextVector2CircularEdge(10f + pulse * 12f, 10f + pulse * 12f);
					Dust auraDust = Dust.NewDustPerfect(Projectile.Center + offset, DustID.PurpleTorch, -offset * 0.08f, (int)((1f - overall) * 200f), default, (1.05f + pulse * 0.6f) * overall);
					auraDust.noGravity = true;
				}
				if (burstMode && Projectile.localAI[0] > PauseTicks && Projectile.localAI[0] < PauseTicks + 4f)
				{
					for (int j = 0; j < 6; j++)
					{
						Vector2 burstOff = Main.rand.NextVector2Circular(8f, 8f);
						Dust burst = Dust.NewDustPerfect(Projectile.Center + burstOff, DustID.PinkTorch, Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedByRandom(0.7f) * Main.rand.NextFloat(2f, 5f), 0, default, 1.2f);
						burst.noGravity = true;
					}
				}
			}
		}
		private float ComputeSpeedFactor()
		{
			float currentSpeed = Projectile.velocity.Length();
			float reference;
			if (Projectile.ai[0] > 0f)
				reference = Projectile.ai[0];
			else
				reference = 11f;
			return MathHelper.Clamp(currentSpeed / Math.Max(1f, reference), 0f, 1.25f);
		}
		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Texture2D pixel = TextureAssets.MagicPixel.Value;
			Vector2 origin = texture.Size() / 2f;
			float time = Main.GameUpdateCount * 0.075f + Projectile.identity * 0.11f;
			float pulse = (float)Math.Sin(time * 2f) * 0.5f + 0.5f;
			float fade = 1f - Projectile.alpha / 255f;
			float speedFactor = ComputeSpeedFactor();
			Vector2 velNorm = Projectile.velocity.SafeNormalize(Vector2.UnitY);
			Vector2 trailDir = -velNorm;
			float trailRot = trailDir.ToRotation();
			Vector2 trailStart = Projectile.Center - Main.screenPosition;
			Rectangle pixSrc = new Rectangle(0, 0, 1, 1);
			float trailLen = 12f + speedFactor * 130f;
			float trailPulse = (float)Math.Sin(time * 4f + Projectile.identity * 0.3f) * 0.5f + 0.5f;
			float baseTrailAlpha = (0.35f + speedFactor * 0.55f) * fade;
			Color outerHalo = new Color(120, 70, 255, 0);
			Color midGlow = new Color(190, 130, 255, 0);
			Color innerCore = new Color(255, 235, 255, 0);
			if (speedFactor > 0.15f)
			{
				float shaderTime = Main.GameUpdateCount * 0.018f + Projectile.identity * 0.17f;
				float shaderTrailWidth = 22f + trailPulse * 4f + speedFactor * 6f;
				float shaderTrailLen = trailLen * 1.10f;
				float shaderTrailOpacity = baseTrailAlpha * (0.85f + speedFactor * 0.25f);
				CelestiaProjectileShaderSystem.DrawTrailFire(trailStart, trailRot, new Vector2(shaderTrailLen, shaderTrailWidth), shaderTime, shaderTrailOpacity, 0.85f + speedFactor * 0.6f, 0.025f, new Color(255, 240, 255), new Color(180, 130, 255), new Color(80, 60, 200));
				CelestiaProjectileShaderSystem.DrawPixelTrail(trailStart, trailRot, new Vector2(shaderTrailLen * 0.92f, shaderTrailWidth * 0.7f), shaderTime + 0.5f, shaderTrailOpacity * 0.55f, 0.05f, new Color(60, 40, 130), new Color(160, 110, 240), new Color(255, 245, 255));
			}
			
			Main.EntitySpriteDraw(pixel, trailStart, pixSrc, outerHalo * baseTrailAlpha * 0.40f, trailRot, new Vector2(0f, 0.5f), new Vector2(trailLen, 18f + trailPulse * 4f + speedFactor * 5f), SpriteEffects.None, 0);
			Main.EntitySpriteDraw(pixel, trailStart, pixSrc, midGlow * baseTrailAlpha * 0.65f, trailRot, new Vector2(0f, 0.5f), new Vector2(trailLen * 0.92f, 9f + trailPulse * 2f + speedFactor * 3f), SpriteEffects.None, 0);
			Main.EntitySpriteDraw(pixel, trailStart, pixSrc, innerCore * baseTrailAlpha, trailRot, new Vector2(0f, 0.5f), new Vector2(trailLen * 0.85f, 3.2f + trailPulse * 1.5f + speedFactor * 1.5f), SpriteEffects.None, 0);
			Main.EntitySpriteDraw(pixel, trailStart, pixSrc, Color.White * baseTrailAlpha * (0.5f + speedFactor * 0.5f), trailRot, new Vector2(0f, 0.5f), new Vector2(trailLen * 0.28f + 8f, 1.8f + trailPulse * 0.8f + speedFactor * 1.2f), SpriteEffects.None, 0);
			if (speedFactor > 0.5f)
			{
				float glossAlpha = MathHelper.Clamp((speedFactor - 0.5f) / 0.5f, 0f, 1f) * fade;
				Main.EntitySpriteDraw(pixel, trailStart, pixSrc, new Color(255, 180, 230, 0) * glossAlpha * 0.75f, trailRot, new Vector2(0f, 0.5f), new Vector2(trailLen * 0.65f, 5f + trailPulse * 1.5f), SpriteEffects.None, 0);
			}

			float trailIntensity = 0.30f + speedFactor * 0.55f;
			for (int i = 0; i < Projectile.oldPos.Length; i++)
			{
				float progress = (float)(Projectile.oldPos.Length - i) / Projectile.oldPos.Length;
				Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
				Color color = Color.Lerp(new Color(90, 80, 255, 0), Color.White, progress) * progress * trailIntensity * fade;
				Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, origin, Projectile.scale * (0.6f + progress * 0.35f), SpriteEffects.None, 0);
			}
			float glowTime = Main.GameUpdateCount * 0.024f + Projectile.identity * 0.19f;
			float glowRadius = 38f + speedFactor * 38f + pulse * 8f;
			float glowOpacity = (0.45f + speedFactor * 0.45f) * fade;
			CelestiaProjectileShaderSystem.DrawGlow(Projectile.Center - Main.screenPosition, glowRadius, glowTime, glowOpacity, 0.4f + speedFactor * 0.6f, new Color(255, 235, 255), new Color(150, 100, 255));
			float auraStrength = MathHelper.Clamp(speedFactor, 0.2f, 1f) * fade;
			int innerCopies = 4 + (int)Math.Round(speedFactor * 3f);
			int outerCopies = 6 + (int)Math.Round(speedFactor * 4f);

			for (int layer = 0; layer < 2; layer++)
			{
				int copies = layer == 0 ? innerCopies : outerCopies;
				float baseRadius = layer == 0 ? 8f : 14f;
				float radius = baseRadius * pulse + layer * 3f + speedFactor * 4f;
				float spin = time * (layer == 0 ? 1.4f : -0.9f) * (0.5f + speedFactor * 0.8f);
				for (int i = 0; i < copies; i++)
				{
					float angle = spin + MathHelper.TwoPi / copies * i;
					Vector2 offset = angle.ToRotationVector2() * radius;
					Color auraColor = layer == 0 ? new Color(170, 110, 255, 0) : new Color(255, 255, 255, 0);
					float scale = Projectile.scale * (0.55f + pulse * 0.28f - layer * 0.08f) * (0.7f + speedFactor * 0.4f);
					float alpha = (0.32f + pulse * 0.22f) * auraStrength;
					Main.EntitySpriteDraw(texture, Projectile.Center + offset - Main.screenPosition, null, auraColor * alpha, Projectile.rotation + angle * 0.18f, origin, scale, SpriteEffects.None, 0);
				}
			}
			for (int i = 0; i < 4; i++)
			{
				Vector2 glowOffset = (MathHelper.PiOver2 * i + time).ToRotationVector2() * (3f + pulse * 5f);
				Main.EntitySpriteDraw(texture, Projectile.Center + glowOffset - Main.screenPosition, null, new Color(140, 90, 255, 0) * (0.45f + speedFactor * 0.3f) * fade, Projectile.rotation, origin, Projectile.scale * 1.22f, SpriteEffects.None, 0);
			}
			if (speedFactor > 0.6f)
			{
				float burstAlpha = MathHelper.Clamp((speedFactor - 0.6f) / 0.4f, 0f, 1f) * fade;
				int rays = 6;
				for (int i = 0; i < rays; i++)
				{
					Vector2 dir = (MathHelper.TwoPi / rays * i + time * 0.4f).ToRotationVector2();
					Vector2 offset = dir * (12f + pulse * 6f);
					Main.EntitySpriteDraw(texture, Projectile.Center + offset - Main.screenPosition, null, Color.White * 0.35f * burstAlpha, dir.ToRotation(), origin, Projectile.scale * 0.85f, SpriteEffects.None, 0);
				}
			}
			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Color.White * fade, Projectile.rotation, origin, Projectile.scale * (1f + pulse * 0.08f), SpriteEffects.None, 0);
			return false;
		}
	}
}
