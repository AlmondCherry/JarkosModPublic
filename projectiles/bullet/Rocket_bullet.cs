using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.GameContent;
using ReLogic.Content;
using jarkos.particles;

namespace jarkos.projectiles.bullet
{
	public class Rocket_bullet : ModProjectile
	{
		public override string Texture => "jarkos/textures/MagicSphereProj";

		private List<Vector2> trailPositions = new List<Vector2>();
		private const int TRAIL_LENGTH = 10;

		private const int ExplosionDelay = 45;
		private const int ShrapnelCount = 6;
		private const float ShrapnelSpeed = 7f;
		private Vector2 PreviousPosition
		{
			get => new Vector2(Projectile.localAI[0], Projectile.localAI[1]);
			set
			{
				Projectile.localAI[0] = value.X;
				Projectile.localAI[1] = value.Y;
			}
		}

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = TRAIL_LENGTH;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}

		public override void SetDefaults()
		{
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.aiStyle = 0;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 300;
			Projectile.ignoreWater = false;
			Projectile.tileCollide = true;
			Projectile.extraUpdates = 1;
			Projectile.light = 0.3f;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if (PreviousPosition == Vector2.Zero)
			{
				return null;
			}
			if (projHitbox.Intersects(targetHitbox))
			{
				return true;
			}
			float collisionPoint = 0f;
			Vector2 lineStart = PreviousPosition;
			Vector2 lineEnd = Projectile.Center;
			Rectangle expandedHitbox = new Rectangle(targetHitbox.X - 2, targetHitbox.Y - 2, targetHitbox.Width + 4, targetHitbox.Height + 4);
			bool lineCollision = Collision.CheckAABBvLineCollision(expandedHitbox.TopLeft(), expandedHitbox.Size(), lineStart, lineEnd, Projectile.width * 0.5f, ref collisionPoint);
			if (lineCollision)
			{
				return true;
			}
			return false;
		}

		public override void AI()
		{
			UpdateTrail();

			if (Projectile.ai[0] == 0)
			{
				PreviousPosition = Projectile.Center;
			}
			
			Projectile.ai[0]++;
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

			int ticksRemaining = ExplosionDelay - (int)Projectile.ai[0];
			if (ticksRemaining <= 30 && ticksRemaining > 0)
			{
				int blinkRate = ticksRemaining > 15 ? 6 : 3;
				if (Projectile.ai[0] % blinkRate == 0)
				{
					for (int i = 0; i < 2; i++)
					{
						Dust torch = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f);
						torch.noGravity = true;
						torch.scale = 0.7f + Main.rand.NextFloat(0.3f);
						torch.velocity = Main.rand.NextVector2Circular(1.5f, 1.5f);
					}
				}
				if (ticksRemaining == 20 || ticksRemaining == 10)
				{
					SoundEngine.PlaySound(SoundID.Item94 with { Volume = 0.3f, Pitch = 0.5f }, Projectile.Center);
				}
			}
			if (Projectile.ai[0] >= ExplosionDelay)
			{
				Explode();
				Projectile.Kill();
				return;
			}
			if (Main.rand.NextBool(16))
			{
				Dust gear = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Iron, -Projectile.velocity.X * 0.1f, -Projectile.velocity.Y * 0.1f);
				gear.noGravity = true;
				gear.scale = 0.5f;
			}
			PreviousPosition = Projectile.Center;
		}

		private void UpdateTrail()
		{
			trailPositions.Insert(0, Projectile.Center);
			while (trailPositions.Count > TRAIL_LENGTH)
			{
				trailPositions.RemoveAt(trailPositions.Count - 1);
			}
		}

		public override void OnKill(int timeLeft)
		{
			if (Projectile.ai[1] == 0)
			{
				Explode();
			}

			Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
		}

		private void Explode()
		{
			Projectile.ai[1] = 1;


			SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.8f, Pitch = 0.2f }, Projectile.Center);



			new Circleedge_reducing(Projectile.Center, Vector2.Zero, new Color(255, 160, 40), 20, 1.2f, 1f, 0f).Spawn();


			for (int i = 0; i < 6; i++)
			{
				Vector2 vel = Main.rand.NextVector2Circular(6f, 6f);
				Color sparkCol = Color.Lerp(new Color(255, 150, 20), new Color(255, 210, 60), Main.rand.NextFloat());
				new Spark(Projectile.Center, vel, sparkCol, Main.rand.Next(15, 26), Main.rand.NextFloat(0.4f, 0.7f), 1f, Main.rand.NextFloat(-0.1f, 0.1f)).Spawn();
			}


			for (int i = 0; i < 2; i++)
			{
				Vector2 smokeVel = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-4f, -1.5f));
				new High_smoke(Projectile.Center, smokeVel, Color.White, 35, Main.rand.NextFloat(0.08f, 0.15f), Main.rand.NextFloat(0.6f, 1f), 0f).Spawn();
			}


			for (int i = 0; i < 2; i++)
			{
				Vector2 velocity = Main.rand.NextVector2Circular(4f, 4f);
				Dust fire = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.Torch, velocity.X, velocity.Y);
				fire.noGravity = true;
				fire.scale = 1.3f;
			}
			Dust smokeDust = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.Smoke, 0f, -1f);
			smokeDust.scale = 1.1f;

			if (Main.netMode != NetmodeID.Server)
			{
				for (int i = 0; i < 2; i++)
				{
					Gore.NewGore(Projectile.GetSource_Death(), Projectile.Center, Main.rand.NextVector2Circular(2f, 2f), Main.rand.Next(GoreID.Smoke1, GoreID.Smoke3 + 1));
				}
			}
			SpawnShrapnel();
		}

		private void SpawnShrapnel()
		{
			if (Main.myPlayer != Projectile.owner)
				return;
			for (int i = 0; i < ShrapnelCount; i++)
			{
				float baseAngle = MathHelper.TwoPi / ShrapnelCount * i;
				float angle = baseAngle + Main.rand.NextFloat(-0.3f, 0.3f);
				float speed = ShrapnelSpeed + Main.rand.NextFloat(-2f, 2f);
				Vector2 velocity = new Vector2((float)Math.Cos(angle) * speed, (float)Math.Sin(angle) * speed);
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<BulletShrapnel>(), Projectile.damage / 2, Projectile.knockBack * 0.3f, Projectile.owner);
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			DrawTrail();

			Texture2D mainTex = TextureAssets.Projectile[Projectile.type].Value;
			Vector2 origin = mainTex.Size() / 2f;
			float currentAlpha = (255 - Projectile.alpha) / 255f;


			Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/particles/Circular_glow", AssetRequestMode.ImmediateLoad).Value;
			if (glowTex != null)
			{
				SpriteBatch sb = Main.spriteBatch;
				sb.End();
				sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

				Vector2 glowOrigin = glowTex.Size() / 2f;
				Vector2 drawPos = Projectile.Center - Main.screenPosition;

				Color softOrangeGlow = new Color(255, 120, 20) * (0.7f * currentAlpha);
				Color innerYellowCore = new Color(255, 210, 60) * (0.45f * currentAlpha);
				float glowScale = 0.45f * Projectile.scale;

				sb.Draw(glowTex, drawPos, null, softOrangeGlow, Projectile.rotation, glowOrigin, glowScale, SpriteEffects.None, 0f);
				sb.Draw(glowTex, drawPos, null, innerYellowCore, Projectile.rotation, glowOrigin, glowScale * 0.5f, SpriteEffects.None, 0f);

				sb.End();
				sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			}


			Vector2 currentDrawPos = Projectile.Center - Main.screenPosition;
			Color mainColor = new Color(255, 200, 120) * currentAlpha;
			Main.EntitySpriteDraw(mainTex, currentDrawPos, null, mainColor, Projectile.rotation, origin, Projectile.scale * 0.45f, SpriteEffects.None, 0);

			return false;
		}

		private void DrawTrail()
		{
			if (trailPositions.Count < 2) return;

			SpriteBatch sb = Main.spriteBatch;
			Texture2D pixel = TextureAssets.MagicPixel.Value;
			float currentAlpha = (255 - Projectile.alpha) / 255f;

			sb.End();
			sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			for (int i = 0; i < trailPositions.Count - 1; i++)
			{
				float t0 = (float)i / trailPositions.Count;
				Vector2 start = Projectile.Center;
				Vector2 end = trailPositions[i + 1];

				float glowWidth = MathHelper.Lerp(12f, 1f, t0);
				float coreWidth = MathHelper.Lerp(4f, 0.5f, t0);

				Color glowColor = new Color(255, 120, 20) * ((1f - t0) * 0.45f * currentAlpha);
				Color coreColor = Color.Lerp(new Color(255, 230, 120), new Color(255, 160, 40), t0) * ((1f - t0) * 0.85f * currentAlpha);

				DrawSegmentLine(sb, pixel, start, end, glowWidth, glowColor);
				DrawSegmentLine(sb, pixel, start, end, coreWidth, coreColor);
			}

			sb.End();
			sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
		}

		private static void DrawSegmentLine(SpriteBatch sb, Texture2D pixel, Vector2 start, Vector2 end, float width, Color color)
		{
			Vector2 diff = end - start;
			float length = diff.Length();
			if (length < 0.1f) return;
			float rotation = diff.ToRotation();
			Vector2 origin = new Vector2(0f, 0.5f);
			Vector2 scale = new Vector2(length, width);
			sb.Draw(pixel, start - Main.screenPosition, new Rectangle(0, 0, 1, 1), color, rotation, origin, scale, SpriteEffects.None, 0f);
		}
	}

	public class BulletShrapnel : ModProjectile
	{
		public override string Texture => "jarkos/textures/MagicSphereProj";
		private const float Gravity = 0.2f;
		private const float MaxFallSpeed = 14f;
		private const float AirResistance = 0.98f;

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}

		public override void SetDefaults()
		{
			Projectile.width = 6;
			Projectile.height = 6;
			Projectile.aiStyle = 0;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 120;
			Projectile.ignoreWater = false;
			Projectile.tileCollide = true;
			Projectile.alpha = 0;
		}

		public override void AI()
		{
			Projectile.ai[0]++;
			Projectile.velocity.Y += Gravity;
			if (Projectile.velocity.Y > MaxFallSpeed)
			{
				Projectile.velocity.Y = MaxFallSpeed;
			}
			Projectile.velocity.X *= AirResistance;
			Projectile.rotation += Projectile.velocity.X * 0.08f + Projectile.velocity.Y * 0.02f;
			
			Lighting.AddLight(Projectile.Center, 0.4f, 0.2f, 0.05f);

			if (Projectile.timeLeft < 30)
			{
				Projectile.alpha += 8;
				if (Projectile.alpha > 255)
					Projectile.alpha = 255;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D mainTex = TextureAssets.Projectile[Projectile.type].Value;
			Vector2 origin = mainTex.Size() / 2f;
			float currentAlpha = (255 - Projectile.alpha) / 255f;


			Color trailColor = new Color(255, 140, 30);
			for (int i = 0; i < Projectile.oldPos.Length; i++)
			{
				if (Projectile.oldPos[i] == Vector2.Zero) continue;
				float progress = (float)(Projectile.oldPos.Length - i) / Projectile.oldPos.Length;
				float alpha = progress * 0.5f * currentAlpha;
				Color color = trailColor * alpha;
				float scale = Projectile.scale * 0.3f * (0.5f + progress * 0.5f);
				Vector2 oldDrawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
				Main.EntitySpriteDraw(mainTex, oldDrawPos, null, color, Projectile.oldRot[i], origin, scale, SpriteEffects.None, 0);
			}


			Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/particles/Circular_glow", AssetRequestMode.ImmediateLoad).Value;
			if (glowTex != null)
			{
				SpriteBatch sb = Main.spriteBatch;
				sb.End();
				sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

				Vector2 glowOrigin = glowTex.Size() / 2f;
				Vector2 drawPos = Projectile.Center - Main.screenPosition;

				Color softOrangeGlow = new Color(255, 120, 20) * (0.6f * currentAlpha);
				float glowScale = 0.35f * Projectile.scale;

				sb.Draw(glowTex, drawPos, null, softOrangeGlow, Projectile.rotation, glowOrigin, glowScale, SpriteEffects.None, 0f);

				sb.End();
				sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			}


			Vector2 currentDrawPos = Projectile.Center - Main.screenPosition;
			Color shrapnelColor = new Color(255, 200, 120) * currentAlpha;
			Main.EntitySpriteDraw(mainTex, currentDrawPos, null, shrapnelColor, Projectile.rotation, origin, Projectile.scale * 0.3f, SpriteEffects.None, 0);

			return false;
		}

		public override void OnKill(int timeLeft)
		{
			SoundEngine.PlaySound(SoundID.Item10 with { Pitch = 0.5f, Volume = 0.5f }, Projectile.Center);
			for (int i = 0; i < 2; i++)
			{
				Vector2 velocity = Main.rand.NextVector2Circular(3f, 3f);
				Dust fire = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, velocity);
				fire.noGravity = true;
				fire.scale = 0.9f;
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			target.AddBuff(BuffID.OnFire, 180);
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			if (Projectile.velocity.X != oldVelocity.X)
			{
				Projectile.velocity.X = -oldVelocity.X * 0.4f;
			}
			if (Projectile.velocity.Y != oldVelocity.Y)
			{
				Projectile.velocity.Y = -oldVelocity.Y * 0.4f;
			}
			SoundEngine.PlaySound(SoundID.Dig with { Volume = 0.3f }, Projectile.Center);
			if (Projectile.velocity.Length() < 1f)
			{
				return true;
			}
			Projectile.timeLeft -= 30;
			return false;
		}
	}
}
