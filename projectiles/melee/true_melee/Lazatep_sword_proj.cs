using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
using ReLogic.Content;

namespace jarkos.projectiles.melee.true_melee
{
	public class Lazatep_sword_proj : ModProjectile
	{
		public override string Texture => "jarkos/textures/MagicSphereProj";

		private List<Vector2> lightningPoints = new List<Vector2>();
		private int lightningTimer = 0;
		private bool struck = false;

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 300;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.light = 0.5f;
			Projectile.extraUpdates = 1;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;


			if (Main.rand.NextBool(5))
			{
				Color cyanColor = Color.Lerp(new Color(40, 180, 255), new Color(160, 240, 255), Main.rand.NextFloat());
				Vector2 sparkVel = Main.rand.NextVector2Circular(2f, 2f) - Projectile.velocity * 0.15f;
				new Electric_particle(Projectile.Center, sparkVel, cyanColor, Main.rand.Next(15, 25), Main.rand.NextFloat(0.05f, 0.2f), 1f, 0.92f).Spawn();
			}

			if (struck)
			{
				lightningTimer++;
				if (lightningTimer > 20)
				{
					Projectile.Kill();
				}
			}

			Lighting.AddLight(Projectile.Center, 0.2f, 0.6f, 1f);
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			TriggerLightningImpact(target.Center);
		}

		public override void OnKill(int timeLeft)
		{
			if (!struck)
			{
				TriggerLightningImpact(Projectile.Center);
			}
		}

		private void TriggerLightningImpact(Vector2 pos)
		{
			if (struck) return;
			struck = true;


			Vector2 strikeStart = pos + new Vector2(Main.rand.NextFloat(-50f, 50f), -260f);
			GenerateLightningPath(strikeStart, pos);


			int count = Main.rand.Next(3, 6);
			float baseAngle = Main.rand.NextFloat(MathHelper.TwoPi);
			for (int i = 0; i < count; i++)
			{
				float angle = baseAngle + (i / (float)count) * MathHelper.TwoPi + Main.rand.NextFloat(-0.25f, 0.25f);
				Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 11f);
				Color cyanColor = Color.Lerp(new Color(40, 180, 255), new Color(180, 240, 255), Main.rand.NextFloat());

				new Electric_particle(pos, vel, cyanColor, Main.rand.Next(20, 35), Main.rand.NextFloat(0.15f, 0.3f), 1f, 0.91f).Spawn();
			}


			new Circular_glow(pos, Vector2.Zero, new Color(40, 180, 255), 22, 0.75f, 0.95f).Spawn();


			SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.85f, Pitch = 0.2f }, pos);
		}

		private void GenerateLightningPath(Vector2 start, Vector2 end)
		{
			lightningPoints.Clear();
			lightningPoints.Add(start);

			Vector2 dir = end - start;
			Vector2 normalDir = Vector2.Normalize(dir);
			Vector2 perpendicular = new Vector2(-normalDir.Y, normalDir.X);
			int segments = 9;
			float jaggedness = 28f;

			for (int i = 1; i < segments; i++)
			{
				float progress = (float)i / segments;
				Vector2 basePoint = start + dir * progress;
				float offset = Main.rand.NextFloat(-jaggedness, jaggedness);
				float envelope = (float)Math.Sin(progress * Math.PI);
				Vector2 point = basePoint + perpendicular * (offset * envelope);
				lightningPoints.Add(point);
			}
			lightningPoints.Add(end);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			SpriteBatch sb = Main.spriteBatch;
			Texture2D mainTex = TextureAssets.Projectile[Projectile.type].Value;
			Vector2 origin = mainTex.Size() / 2f;
			Vector2 drawPos = Projectile.Center - Main.screenPosition;


			Color trailColor = new Color(40, 180, 255);
			for (int i = 0; i < Projectile.oldPos.Length; i++)
			{
				if (Projectile.oldPos[i] == Vector2.Zero) continue;
				float progress = (float)(Projectile.oldPos.Length - i) / Projectile.oldPos.Length;
				Color col = trailColor * (progress * 0.6f);
				float scale = Projectile.scale * 0.5f * (0.5f + progress * 0.5f);
				Vector2 oldDrawPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
				sb.Draw(mainTex, oldDrawPos, null, col, Projectile.oldRot[i], origin, scale, SpriteEffects.None, 0f);
			}


			Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/particles/Circular_glow", AssetRequestMode.ImmediateLoad).Value;
			if (glowTex != null)
			{
				sb.End();
				sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

				Vector2 glowOrigin = glowTex.Size() / 2f;
				Color softCyanGlow = new Color(40, 180, 255) * 0.8f;
				Color innerWhite = Color.White * 0.5f;

				sb.Draw(glowTex, drawPos, null, softCyanGlow, Projectile.rotation, glowOrigin, 0.55f, SpriteEffects.None, 0f);
				sb.Draw(glowTex, drawPos, null, innerWhite, Projectile.rotation, glowOrigin, 0.28f, SpriteEffects.None, 0f);

				sb.End();
				sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			}


			if (lightningPoints.Count > 1 && lightningTimer < 20)
			{
				float lightningAlpha = 1f - (lightningTimer / 20f);
				Texture2D pixel = TextureAssets.MagicPixel.Value;

				sb.End();
				sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

				for (int i = 0; i < lightningPoints.Count - 1; i++)
				{
					Vector2 start = lightningPoints[i];
					Vector2 end = lightningPoints[i + 1];

					Vector2 diff = end - start;
					float length = diff.Length();
					if (length < 0.1f) continue;

					float rot = diff.ToRotation();
					Vector2 lineOrigin = new Vector2(0f, 0.5f);


					float tSeg = i / (float)(lightningPoints.Count - 1);
					float outerThickness = MathHelper.Lerp(1.0f, 7.5f, tSeg);
					float innerThickness = MathHelper.Lerp(0.4f, 2.8f, tSeg);

					Color outerCyan = new Color(40, 180, 255) * (0.85f * lightningAlpha);
					Color innerWhiteLine = Color.White * (0.95f * lightningAlpha);

					sb.Draw(pixel, start - Main.screenPosition, new Rectangle(0, 0, 1, 1), outerCyan, rot, lineOrigin, new Vector2(length, outerThickness), SpriteEffects.None, 0f);
					sb.Draw(pixel, start - Main.screenPosition, new Rectangle(0, 0, 1, 1), innerWhiteLine, rot, lineOrigin, new Vector2(length, innerThickness), SpriteEffects.None, 0f);
				}

				sb.End();
				sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			}


			sb.Draw(mainTex, drawPos, null, Color.White, Projectile.rotation, origin, Projectile.scale * 0.5f, SpriteEffects.None, 0f);

			return false;
		}
	}
}
