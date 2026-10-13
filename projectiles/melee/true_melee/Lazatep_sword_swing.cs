using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.particles;
using ReLogic.Content;

namespace jarkos.projectiles.melee.true_melee
{
	public class Lazatep_sword_swing : ModProjectile
	{
		public override string Texture => "jarkos/items/weapons/melee/true_melee/Lazatep_sword";

		private Player Owner => Main.player[Projectile.owner];
		private const int PrepDuration = 6;
		private const int ActiveSwingDuration = 18;
		private const int FadeTime = 8;
		private const float BladeLength = 35f;

		private float startAngle;
		private float endAngle;
		private int swingDirection = 1;
		private bool projectileFired = false;

		private List<Vector2> lightningPoints = new List<Vector2>();
		private int lightningTimer = 0;

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}

		public override void SetDefaults()
		{
			Projectile.width = 64;
			Projectile.height = 64;
			Projectile.friendly = false;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ownerHitCheck = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 12;
		}

		public override void OnSpawn(IEntitySource source)
		{
			Projectile.timeLeft = PrepDuration + ActiveSwingDuration + FadeTime;

			if (Projectile.owner == Main.myPlayer)
			{
				Vector2 aim = Projectile.velocity.SafeNormalize(Vector2.UnitX);
				swingDirection = aim.X >= 0f ? 1 : -1;
				float aimAngle = aim.ToRotation();

				float halfArc = MathHelper.ToRadians(100f);
				startAngle = aimAngle - swingDirection * halfArc;
				endAngle = aimAngle + swingDirection * halfArc;
				Projectile.netUpdate = true;
			}
		}

		public override void AI()
		{
			if (!Owner.active || Owner.dead || Owner.noItems || Owner.CCed)
			{
				Projectile.Kill();
				return;
			}

			Owner.heldProj = Projectile.whoAmI;
			int totalDuration = PrepDuration + ActiveSwingDuration + FadeTime;
			int progressTick = totalDuration - Projectile.timeLeft;

			float currentAngle;

			if (progressTick < PrepDuration)
			{
				Projectile.friendly = false;
				float prepProgress = progressTick / (float)PrepDuration;


				currentAngle = startAngle - swingDirection * MathHelper.ToRadians(12f) * prepProgress;


				if (Main.rand.NextBool(6))
				{
					Vector2 dirVec = currentAngle.ToRotationVector2();
					Vector2 tipPos = Owner.MountedCenter + dirVec * (BladeLength * 0.85f);
					Vector2 sparkVel = Main.rand.NextVector2Circular(1.2f, 1.2f);
					Color cyanColor = Color.Lerp(new Color(40, 180, 255), new Color(180, 240, 255), Main.rand.NextFloat());


					new Electric_particle(tipPos, sparkVel, cyanColor, Main.rand.Next(10, 16), Main.rand.NextFloat(0.02f, 0.08f), 0.9f, 0.90f).Spawn();
				}
			}
			else
			{

				Projectile.friendly = true;
				int swingTick = progressTick - PrepDuration;
				float swingProgress = MathHelper.Clamp(swingTick / (float)ActiveSwingDuration, 0f, 1f);


				float easeOutProgress = 1f - (float)Math.Pow(1f - swingProgress, 3);
				currentAngle = MathHelper.Lerp(startAngle, endAngle, easeOutProgress);


				if (!projectileFired && swingTick >= 2 && Projectile.owner == Main.myPlayer)
				{
					projectileFired = true;
					Vector2 shootVel = Projectile.velocity.SafeNormalize(Vector2.UnitX) * 14f;
					Projectile.NewProjectile(
						Projectile.GetSource_FromThis(),
						Owner.MountedCenter,
						shootVel,
						ModContent.ProjectileType<Lazatep_sword_proj>(),
						Projectile.damage,
						Projectile.knockBack * 0.6f,
						Projectile.owner
					);
					SoundEngine.PlaySound(SoundID.Item15 with { Volume = 0.7f, Pitch = 0.3f }, Owner.MountedCenter);
				}


				if (swingTick < ActiveSwingDuration && Main.rand.NextBool(4))
				{
					Vector2 dirVec = currentAngle.ToRotationVector2();
					Vector2 tipPos = Owner.MountedCenter + dirVec * (BladeLength * Main.rand.NextFloat(0.5f, 0.95f));
					Vector2 sparkVel = dirVec.RotatedBy(swingDirection * MathHelper.PiOver2) * Main.rand.NextFloat(1.5f, 3.5f);
					Color cyanColor = Color.Lerp(new Color(40, 180, 255), new Color(160, 230, 255), Main.rand.NextFloat());

					new Electric_particle(tipPos, sparkVel, cyanColor, Main.rand.Next(10, 18), Main.rand.NextFloat(0.02f, 0.06f), 0.85f, 0.92f).Spawn();
				}
			}

			Vector2 dirVector = currentAngle.ToRotationVector2();
			Projectile.Center = Owner.MountedCenter + dirVector * (BladeLength * 0.45f);
			Projectile.rotation = currentAngle;

			Owner.direction = swingDirection;
			Owner.itemRotation = currentAngle;
			if (Owner.direction < 0)
			{
				Owner.itemRotation += MathHelper.Pi;
			}

			if (lightningPoints.Count > 0)
			{
				lightningTimer++;
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{

			Vector2 strikeStart = target.Center + new Vector2(Main.rand.NextFloat(-40f, 40f), -200f);
			Vector2 strikeEnd = target.Center;

			GenerateLightningPath(strikeStart, strikeEnd);


			int particleCount = 3;
			float baseAngle = Main.rand.NextFloat(MathHelper.TwoPi);
			for (int i = 0; i < particleCount; i++)
			{
				float angle = baseAngle + (i / (float)particleCount) * MathHelper.TwoPi + Main.rand.NextFloat(-0.2f, 0.2f);
				Vector2 particleVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
				Color cyanColor = Color.Lerp(new Color(40, 180, 255), new Color(180, 240, 255), Main.rand.NextFloat());

				new Electric_particle(target.Center, particleVel, cyanColor, Main.rand.Next(15, 24), Main.rand.NextFloat(0.14f, 0.22f), 0.85f, 0.91f).Spawn();
			}


			new Circular_glow(target.Center, Vector2.Zero, new Color(40, 180, 255), 14, 0.35f, 0.85f).Spawn();


			SoundEngine.PlaySound(SoundID.Item122 with { Volume = 0.6f, Pitch = 0.3f }, target.Center);
		}

		private void GenerateLightningPath(Vector2 start, Vector2 end)
		{
			lightningPoints.Clear();
			lightningPoints.Add(start);

			Vector2 dir = end - start;
			Vector2 normalDir = Vector2.Normalize(dir);
			Vector2 perpendicular = new Vector2(-normalDir.Y, normalDir.X);
			int segments = 8;
			float jaggedness = 20f;

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
			Texture2D swordTex = TextureAssets.Projectile[Projectile.type].Value;

			int totalDuration = PrepDuration + ActiveSwingDuration + FadeTime;
			int progressTick = totalDuration - Projectile.timeLeft;
			float alpha = 1f;

			if (Projectile.timeLeft < FadeTime)
			{
				alpha = Projectile.timeLeft / (float)FadeTime;
			}


			float drawRot = Projectile.rotation + MathHelper.PiOver4;
			Vector2 origin = new Vector2(0f, swordTex.Height);

			SpriteEffects fx = SpriteEffects.None;
			if (swingDirection * Owner.direction < 0)
			{
				fx = SpriteEffects.FlipHorizontally;
				drawRot = Projectile.rotation + MathHelper.PiOver4 + MathHelper.PiOver2;
				origin = new Vector2(swordTex.Width, swordTex.Height);
			}

			Vector2 dirVector = Projectile.rotation.ToRotationVector2();

			sb.End();
			sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);


			Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/particles/Circular_glow", AssetRequestMode.ImmediateLoad).Value;
			if (glowTex != null)
			{
				Vector2 glowOrigin = glowTex.Size() / 2f;
				Vector2 tipDrawPos = Owner.MountedCenter + dirVector * (BladeLength * 0.85f) - Main.screenPosition;
				Color cyanGlow = new Color(40, 180, 255) * (0.5f * alpha);
				sb.Draw(glowTex, tipDrawPos, null, cyanGlow, Projectile.rotation, glowOrigin, 0.28f, SpriteEffects.None, 0f);
			}


			if (lightningPoints.Count > 1 && lightningTimer < 16)
			{
				float lightningAlpha = (1f - (lightningTimer / 16f)) * alpha;
				Texture2D pixel = TextureAssets.MagicPixel.Value;

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

					Color outerCyan = new Color(40, 180, 255) * (0.75f * lightningAlpha);
					Color innerWhite = Color.White * (0.9f * lightningAlpha);

					sb.Draw(pixel, start - Main.screenPosition, new Rectangle(0, 0, 1, 1), outerCyan, rot, lineOrigin, new Vector2(length, outerThickness), SpriteEffects.None, 0f);
					sb.Draw(pixel, start - Main.screenPosition, new Rectangle(0, 0, 1, 1), innerWhite, rot, lineOrigin, new Vector2(length, innerThickness), SpriteEffects.None, 0f);
				}
			}

			sb.End();
			sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);


			Color swordColor = lightColor * alpha;
			sb.Draw(swordTex, Owner.MountedCenter - Main.screenPosition, null, swordColor, drawRot, origin, Projectile.scale, fx, 0f);

			return false;
		}
	}
}
