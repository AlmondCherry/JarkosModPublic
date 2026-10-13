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
	public class Feather_arrow_proj : ModProjectile
	{
		private Vector2 stuckOffset = Vector2.Zero;
		private float stuckRotation = 0f;
		private bool isStuck = false;

		private List<Vector2> trailPositions = new List<Vector2>();
		private const int TRAIL_LENGTH = 10;

		private const int StuckDuration = 300;
		private const int DisappearDuration = 40;

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = TRAIL_LENGTH;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
			ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 2000;
		}

		public override void SetDefaults()
		{
			Projectile.width = 14;
			Projectile.height = 14;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.ignoreWater = true;
			Projectile.light = 0.4f;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 600;
			Projectile.penetrate = -1;
			Projectile.arrow = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			int stuckCount = 0;
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if (p.active && p.type == Projectile.type && p.ai[0] == 1f && (int)p.ai[1] == target.whoAmI)
				{
					stuckCount++;
				}
			}

			if (stuckCount > 0)
			{
				modifiers.SourceDamage += stuckCount * 0.15f;
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (!isStuck)
			{
				isStuck = true;
				Projectile.ai[0] = 1f;
				Projectile.ai[1] = target.whoAmI;

				stuckOffset = Projectile.Center - target.Center;
				stuckRotation = Projectile.rotation;

				Projectile.velocity = Vector2.Zero;
				Projectile.friendly = false;
				Projectile.tileCollide = false;
				Projectile.timeLeft = StuckDuration;
				Projectile.localAI[0] = 0f;
				Projectile.netUpdate = true;

				for (int i = 0; i < 5; i++) 
				{
					Vector2 speed = Main.rand.NextVector2CircularEdge(2f, 2f);
					Dust d = Dust.NewDustPerfect(Projectile.position, DustID.WhiteTorch, speed * 2.5f, Scale: 2.5f);
					d.noGravity = true;
				}
			}
		}

		public override void AI()
		{
			if (Projectile.ai[0] == 1f)
			{
				int targetIndex = (int)Projectile.ai[1];
				if (targetIndex < 0 || targetIndex >= Main.maxNPCs)
				{
					Projectile.Kill();
					return;
				}

				NPC target = Main.npc[targetIndex];
				if (!target.active)
				{
					Projectile.Kill();
					return;
				}

				Projectile.Center = target.Center + stuckOffset;
				Projectile.velocity = Vector2.Zero;
				Projectile.rotation = stuckRotation;
				Projectile.friendly = false;

				Projectile.localAI[0]++;
				if (Projectile.localAI[0] >= StuckDuration)
				{
					Projectile.Kill();
				}
			}
			else
			{
				UpdateTrail();

				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

				float currentSpeed = Projectile.velocity.Length();
				if (currentSpeed > 0.1f && currentSpeed < 35f)
				{
					Projectile.velocity *= 1.05f;
				}
			}
		}

		private void UpdateTrail()
		{
			trailPositions.Insert(0, Projectile.Center);
			while (trailPositions.Count > TRAIL_LENGTH)
			{
				trailPositions.RemoveAt(trailPositions.Count - 1);
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			DrawTrail();

			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Vector2 drawPos = Projectile.Center - Main.screenPosition;
			Vector2 origin = texture.Size() / 2f;

			float alpha = 1f;
			float glowScaleFactor = 0f;

			if (Projectile.ai[0] == 1f)
			{
				float remainingTime = StuckDuration - Projectile.localAI[0];
				if (remainingTime <= DisappearDuration)
				{
					float progress = 1f - MathHelper.Clamp(remainingTime / DisappearDuration, 0f, 1f);
					alpha = 1f - progress;
					glowScaleFactor = 1f - progress;
				}
			}

			Color drawColor = lightColor * alpha;
			Main.EntitySpriteDraw(texture, drawPos, null, drawColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);

			if (glowScaleFactor > 0f)
			{
				Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/particles/Circular_glow", AssetRequestMode.ImmediateLoad).Value;
				if (glowTex != null)
				{
					SpriteBatch sb = Main.spriteBatch;
					sb.End();
					sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

					Vector2 glowOrigin = glowTex.Size() / 2f;
					float finalScale = 0.65f * glowScaleFactor;
					Color whiteGlow = Color.White * (glowScaleFactor * 0.9f);
					Color softCyanCore = new Color(220, 245, 255) * (glowScaleFactor * 0.6f);

					sb.Draw(glowTex, drawPos, null, whiteGlow, Projectile.rotation, glowOrigin, finalScale, SpriteEffects.None, 0f);
					sb.Draw(glowTex, drawPos, null, softCyanCore, Projectile.rotation, glowOrigin, finalScale * 0.5f, SpriteEffects.None, 0f);

					sb.End();
					sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
				}
			}

			return false;
		}

		private void DrawTrail()
		{
			if (trailPositions.Count < 2) return;

			SpriteBatch sb = Main.spriteBatch;
			Texture2D pixel = TextureAssets.MagicPixel.Value;

			sb.End();
			sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			for (int i = 0; i < trailPositions.Count - 1; i++)
			{
				float t0 = (float)i / trailPositions.Count;
				Vector2 start = trailPositions[i];
				Vector2 end = trailPositions[i + 1];

				float glowWidth = MathHelper.Lerp(14f, 1f, t0);
				float coreWidth = MathHelper.Lerp(4.5f, 0.5f, t0);

				Color glowColor = new Color(180, 190, 205) * ((1f - t0) * 0.45f);
				Color coreColor = Color.Lerp(Color.White, new Color(220, 225, 235), t0) * ((1f - t0) * 0.85f);

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

		public override void OnKill(int timeLeft)
		{
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);

			int numStars = Main.rand.Next(2, 4);
			for (int i = 0; i < numStars; i++)
			{
				Vector2 vel = Main.rand.NextVector2Circular(2.5f, 2.5f);
				Color starColor = Color.Lerp(Color.White, new Color(210, 240, 255), Main.rand.NextFloat());
				new StarParticle(Projectile.Center, vel, starColor, Main.rand.Next(20, 35), Main.rand.NextFloat(0.2f, 0.35f), 1f, Main.rand.NextFloat(-0.1f, 0.1f)).Spawn();
			}
		}
	}
}
