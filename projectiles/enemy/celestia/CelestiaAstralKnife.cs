using jarkos.npc.Celestia;
using jarkos.systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.projectiles.enemy.celestia
{
	public class CelestiaAstralKnife : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.MagicDagger;
		private const float DefaultDelay = 54f;
		private const float KnifeSpeed = 56f;
		public const float FixedDirSentinel = -1000f;
		public static float EncodeFixedDirection(float angleRadians) =>
			angleRadians + FixedDirSentinel;
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 26;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}
		public override void SetDefaults()
		{
			Projectile.width = 20;
			Projectile.height = 48;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 300;
		}
		private bool IsFixedDirection => Projectile.ai[0] < -100f;
		private Vector2 GetDirection()
		{
			if (IsFixedDirection)
			{
				float angle = Projectile.ai[0] - FixedDirSentinel;
				return angle.ToRotationVector2();
			}
			int targetIndex = (int)Projectile.ai[0];
			if (targetIndex < 0 || targetIndex >= Main.maxPlayers)
				return Vector2.UnitY;
			Player target = Main.player[targetIndex];
			return target.active && !target.dead
				? (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY)
				: Vector2.UnitY;
		}
		public override void AI()
		{
			float delay = Projectile.ai[1] <= 0f ? DefaultDelay : Projectile.ai[1];
			Projectile.localAI[0]++;
			Lighting.AddLight(Projectile.Center, 0.55f, 0.38f, 1.05f);
			if (Projectile.localAI[1] == 0f)
			{
				Projectile.velocity = Vector2.Zero;
				Vector2 direction = GetDirection();
				Projectile.rotation = direction.ToRotation() + MathHelper.PiOver2;
				Projectile.alpha = (int)MathHelper.Clamp(180f - Projectile.localAI[0] * 4f, 0f, 180f);
				Projectile.scale = 0.85f + MathHelper.Clamp(Projectile.localAI[0] / delay, 0f, 1f) * 0.32f;
				if (Projectile.localAI[0] >= delay)
				{
					Projectile.velocity = direction * KnifeSpeed;
					Projectile.localAI[1] = 1f;
					Projectile.netUpdate = true;
					SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/CelestiaDaggerSwoosh") with { Pitch = Main.rand.NextFloat(-0.2f, 0.2f) }, Projectile.position);
					if (!Main.dedServ)
					{
						for (int j = 0; j < 18; j++)
						{
							Vector2 burstDir = (MathHelper.TwoPi / 18f * j).ToRotationVector2();
							Dust burstDust = Dust.NewDustPerfect(Projectile.Center, DustID.PurpleTorch, burstDir * Main.rand.NextFloat(2f, 5f), 0, default, 1.5f);
							burstDust.noGravity = true;
						}
						for (int j = 0; j < 6; j++)
						{
							Dust burstDust = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch, direction * Main.rand.NextFloat(4f, 9f), 0, default, 1.7f);
							burstDust.noGravity = true;
						}
					}
				}
			}
			else
			{
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
				Projectile.scale = 1.18f;
			}
			if (!Main.dedServ && Projectile.localAI[1] == 0f)
			{
				float charge = MathHelper.Clamp(Projectile.localAI[0] / delay, 0f, 1f);
				for (int i = 0; i < 3; i++)
				{
					Vector2 offset = Main.rand.NextVector2CircularEdge(28f + charge * 22f, 28f + charge * 22f);
					Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, i == 0 ? DustID.WhiteTorch : DustID.PurpleTorch, -offset * 0.12f, 0, default, 1.0f + charge * 0.9f);
					dust.noGravity = true;
				}
			}
			else if (!Main.dedServ && Projectile.localAI[1] == 1f)
			{
				if (Main.rand.NextBool(2))
				{
					Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f), DustID.PurpleTorch, -Projectile.velocity * 0.1f, 0, default, 1.35f);
					dust.noGravity = true;
				}
				if (Main.rand.NextBool(3))
				{
					Dust Spark = Dust.NewDustPerfect(Projectile.Center, DustID.WhiteTorch, -Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedByRandom(0.6f) * Main.rand.NextFloat(2f, 5f), 0, default, 1.1f);
					Spark.noGravity = true;
				}
				if (Main.rand.NextBool(6))
				{
					Dust shimmer = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(20f, 20f), DustID.PinkTorch, Vector2.Zero, 0, default, 0.9f);
					shimmer.noGravity = true;
				}
			}
		}
		public override bool? CanDamage()
		{
			return Projectile.localAI[1] == 1f;
		}
		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Texture2D pixel = TextureAssets.MagicPixel.Value;
			Vector2 origin = texture.Size() / 2f;
			Rectangle pixSrc = new Rectangle(0, 0, 1, 1);
			if (Projectile.localAI[1] == 0f)
			{
				Vector2 telegraphStart = Projectile.Center - Main.screenPosition;
				Vector2 telegraphEnd;
				bool drawTelegraph = false;
				if (IsFixedDirection)
				{
					float angle = Projectile.ai[0] - FixedDirSentinel;
					telegraphEnd = telegraphStart + angle.ToRotationVector2() * 880f;
					drawTelegraph = true;
				}
				else
				{
					int targetIndex = (int)Projectile.ai[0];
					if (targetIndex >= 0 && targetIndex < Main.maxPlayers)
					{
						Player target = Main.player[targetIndex];
						if (target.active && !target.dead)
						{
							telegraphEnd = target.Center - Main.screenPosition;
							drawTelegraph = true;
						}
						else telegraphEnd = telegraphStart;
					}
					else telegraphEnd = telegraphStart;
				}
				if (drawTelegraph)
				{
						Vector2 start = telegraphStart;
						Vector2 end = telegraphEnd;
						Vector2 edge = end - start;
						float beamLength = edge.Length() + 60f;
						float delay = Projectile.ai[1] <= 0f ? DefaultDelay : Projectile.ai[1];
						float charge = MathHelper.Clamp(Projectile.localAI[0] / delay, 0f, 1f);
						float pulse = (float)Math.Sin(Main.GameUpdateCount * (0.18f + charge * 0.55f)) * 0.5f + 0.5f;
						float taper = 1f - MathHelper.SmoothStep(0f, 1f, charge);
						float innerWidth = 11f * taper;
						float outerWidth = (17f * taper + pulse * 2f * taper);
						Color outerColor = Color.Lerp(new Color(140, 70, 255), new Color(255, 230, 255), charge);
						Color innerColor = Color.Lerp(new Color(190, 130, 255), Color.White, charge);
						float baseAlpha = (0.55f + pulse * 0.18f) * taper;
						const int segments = 14;
						Vector2 dir = edge.SafeNormalize(Vector2.UnitX);
						float beamRot = edge.ToRotation();
						float segLen = beamLength / segments;
						float drawSegLen = segLen * 1.05f;
						for (int s = 0; s < segments; s++)
						{
							float tFromKnife = s / (float)(segments - 1);
							float alphaFactor = (float)Math.Pow(1f - tFromKnife, 1.4f);
							alphaFactor = MathHelper.Lerp(0.18f, 1f, alphaFactor);
							Vector2 segStart = start + dir * (s * segLen);
							float segAlpha = baseAlpha * alphaFactor;
							Main.EntitySpriteDraw(pixel, segStart, pixSrc, outerColor * segAlpha * 0.42f, beamRot, new Vector2(0f, 0.5f), new Vector2(drawSegLen, outerWidth), SpriteEffects.None, 0);
							Main.EntitySpriteDraw(pixel, segStart, pixSrc, outerColor * segAlpha * 0.65f, beamRot, new Vector2(0f, 0.5f), new Vector2(drawSegLen, innerWidth * 1.4f), SpriteEffects.None, 0);
							Main.EntitySpriteDraw(pixel, segStart, pixSrc, innerColor * segAlpha, beamRot, new Vector2(0f, 0.5f), new Vector2(drawSegLen, innerWidth * 0.55f), SpriteEffects.None, 0);
						}
						float ringRadius = 26f + charge * 14f + pulse * 6f;
						for (int i = 0; i < 6; i++)
						{
							Vector2 ray = (MathHelper.TwoPi / 6f * i + Main.GameUpdateCount * 0.05f).ToRotationVector2();
							DrawLine(pixel, start + ray * (ringRadius - 10f), start + ray * ringRadius, innerColor * (0.45f * (0.4f + 0.6f * (1f - charge))), 3f);
						}
				}
			}
			float fadeKnife = 1f - Projectile.alpha / 255f;
			float knifePulse = (float)Math.Sin(Main.GameUpdateCount * 0.3f + Projectile.identity * 0.4f) * 0.5f + 0.5f;
			if (Projectile.localAI[1] == 1f)
			{
				Vector2 velNorm = Projectile.velocity.SafeNormalize(Vector2.UnitY);
				Vector2 trailStart = Projectile.Center - Main.screenPosition;
				float trailRot = (-velNorm).ToRotation();
				float velMag = Projectile.velocity.Length();
				float trailLen = MathHelper.Clamp(velMag * 5.5f, 24f, 220f);
				float trailAlpha = fadeKnife * (0.6f + knifePulse * 0.25f);
				float shaderTime = (float)Main.GameUpdateCount * 0.016f + Projectile.identity * 0.13f;
				float shaderTrailWidth = 26f + knifePulse * 6f;
				float shaderTrailLen = trailLen * 1.10f;
				CelestiaProjectileShaderSystem.DrawTrailFire(trailStart, trailRot, new Vector2(shaderTrailLen, shaderTrailWidth), shaderTime, trailAlpha * 0.85f, 1.0f + knifePulse * 0.3f, 0.025f, new Color(255, 235, 255), new Color(255, 130, 230), new Color(140, 80, 255));
				CelestiaProjectileShaderSystem.DrawPixelTrail(trailStart, trailRot, new Vector2(shaderTrailLen * 0.92f, shaderTrailWidth * 0.7f), shaderTime + 0.7f, trailAlpha * 0.55f, 0.05f, new Color(80, 40, 140), new Color(220, 110, 230), new Color(255, 240, 255));
				Main.EntitySpriteDraw(pixel, trailStart, pixSrc, new Color(150, 80, 255, 0) * trailAlpha * 0.45f, trailRot, new Vector2(0f, 0.5f), new Vector2(trailLen, 18f + knifePulse * 5f), SpriteEffects.None, 0);
				Main.EntitySpriteDraw(pixel, trailStart, pixSrc, new Color(220, 140, 255, 0) * trailAlpha * 0.7f, trailRot, new Vector2(0f, 0.5f), new Vector2(trailLen * 0.92f, 10f + knifePulse * 2.5f), SpriteEffects.None, 0);
				Main.EntitySpriteDraw(pixel, trailStart, pixSrc, new Color(255, 235, 255, 0) * trailAlpha, trailRot, new Vector2(0f, 0.5f), new Vector2(trailLen * 0.85f, 3.5f + knifePulse * 1.2f), SpriteEffects.None, 0);
				Main.EntitySpriteDraw(pixel, trailStart, pixSrc, Color.White * trailAlpha * 0.85f, trailRot, new Vector2(0f, 0.5f), new Vector2(trailLen * 0.32f + 12f, 2.2f + knifePulse * 0.8f), SpriteEffects.None, 0);
			}
			for (int i = 0; i < Projectile.oldPos.Length; i++)
			{
				float progress = (float)(Projectile.oldPos.Length - i) / Projectile.oldPos.Length;
				Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
				Color trailColor = Color.Lerp(new Color(255, 100, 220, 0), new Color(170, 90, 255, 0), progress) * progress * 0.55f * fadeKnife;
				Main.EntitySpriteDraw(texture, drawPos, null, trailColor, Projectile.rotation, origin, Projectile.scale * (0.6f + progress * 0.4f), SpriteEffects.None, 0);
			}
			if (Projectile.localAI[1] == 1f)
			{
				float glowTime = (float)Main.GameUpdateCount * 0.022f + Projectile.identity * 0.21f;
				float glowRadius = 70f + knifePulse * 14f;
				CelestiaProjectileShaderSystem.DrawGlow(Projectile.Center - Main.screenPosition, glowRadius, glowTime, fadeKnife * 0.85f, 0.5f + knifePulse * 0.5f, new Color(255, 230, 255), new Color(180, 110, 255));
				for (int i = 0; i < 4; i++)
				{
					Vector2 haloOffset = (MathHelper.PiOver2 * i + Main.GameUpdateCount * 0.08f).ToRotationVector2() * (5f + knifePulse * 3f);
					Main.EntitySpriteDraw(texture, Projectile.Center + haloOffset - Main.screenPosition, null, new Color(180, 120, 255, 0) * (0.55f + knifePulse * 0.25f) * fadeKnife, Projectile.rotation, origin, Projectile.scale * (1.05f + knifePulse * 0.12f), SpriteEffects.None, 0);
				}
				Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, new Color(255, 230, 255, 0) * (0.6f + knifePulse * 0.3f) * fadeKnife, Projectile.rotation, origin, Projectile.scale * 1.45f, SpriteEffects.None, 0);
			}
			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Color.White * fadeKnife, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
			return false;
		}
		private void DrawLine(Texture2D pixel, Vector2 start, Vector2 end, Color color, float width)
		{
			Vector2 edge = end - start;
			Main.EntitySpriteDraw(pixel, start, new Rectangle(0, 0, 1, 1), color, edge.ToRotation(), new Vector2(0f, 0.5f), new Vector2(edge.Length(), width), SpriteEffects.None, 0);
		}
	}
}
