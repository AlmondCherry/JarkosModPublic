using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.projectiles.melee.true_melee
{
	public class Uranium_sword_laser : ModProjectile
	{
		public override string Texture => "jarkos/items/weapons/melee/true_melee/Uranium_sword";

		private const int CHARGE_TIME = 42;
		private const int BEAM_TIME = 78;
		private const int TOTAL_TIME = CHARGE_TIME + BEAM_TIME;
		private const float MAX_DIST = 700f;

		private float aimAngle;
		private int timer;
		private bool soundPlayed;

		public override void SetDefaults()
		{
			Projectile.netImportant = true;
			Projectile.width = 30;
			Projectile.height = 30;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.aiStyle = 0;
			Projectile.timeLeft = TOTAL_TIME;
			Projectile.ownerHitCheck = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 8;
		}

		public override void SendExtraAI(BinaryWriter writer) => writer.Write(aimAngle);
		public override void ReceiveExtraAI(BinaryReader reader) => aimAngle = reader.ReadSingle();

		public override void OnSpawn(IEntitySource source)
		{
			Player player = Main.player[Projectile.owner];
			if (Projectile.owner == Main.myPlayer)
			{
				Vector2 mousePos = Main.MouseWorld;
				aimAngle = (mousePos - player.MountedCenter).ToRotation();
				Projectile.netUpdate = true;
			}
			Projectile.rotation = aimAngle;
		}

		public override void AI()
		{
			Player player = Main.player[Projectile.owner];
			if (!player.active || player.dead)
			{
				Projectile.Kill();
				return;
			}

			Projectile.Center = player.MountedCenter;
			Projectile.timeLeft = 2;


			if (Projectile.owner == Main.myPlayer)
			{
				Vector2 mousePos = Main.MouseWorld;
				aimAngle = (mousePos - player.MountedCenter).ToRotation();
				Projectile.netUpdate = true;
			}

			timer++;
			Projectile.rotation = aimAngle;


			HoldPlayer(player);

			Vector2 dir = aimAngle.ToRotationVector2();
			Vector2 tipPos = player.MountedCenter + dir * 65f;


			if (timer <= CHARGE_TIME)
			{
				float chargeProgress = timer / (float)CHARGE_TIME;
				if (Main.rand.NextFloat() < chargeProgress)
				{
					Vector2 dustVel = -dir.RotatedByRandom(0.8f) * Main.rand.NextFloat(2f, 6f);
					Dust d = Dust.NewDustPerfect(tipPos + Main.rand.NextVector2Circular(20f, 20f), DustID.Electric, dustVel, 0, new Color(0, 255, 255), 1.1f);
					d.noGravity = true;
				}
				Lighting.AddLight(tipPos, new Color(0, 255, 255).ToVector3() * chargeProgress);
			}

			else if (timer <= TOTAL_TIME)
			{
				if (!soundPlayed)
				{
					soundPlayed = true;
					SoundEngine.PlaySound(SoundID.Item93 with { Volume = 0.85f, Pitch = -0.1f }, tipPos);
				}

				int beamTimer = timer - CHARGE_TIME;

				float currentReach = MathHelper.Lerp(0f, MAX_DIST, MathHelper.Clamp(beamTimer / 15f, 0f, 1f));

				float fade = MathHelper.Clamp((TOTAL_TIME - timer) / 15f, 0f, 1f);
				for (float d = 0; d < currentReach; d += 80f)
				{
					Lighting.AddLight(tipPos + dir * d, new Color(0, 255, 255).ToVector3() * 0.8f * fade);
				}

				if (Main.rand.NextBool(2))
				{
					float dist = Main.rand.NextFloat(0f, currentReach);
					Dust d = Dust.NewDustPerfect(tipPos + dir * dist, DustID.Electric, dir * 2f + Main.rand.NextVector2Circular(1f, 1f), 0, new Color(0, 255, 255), 1.1f);
					d.noGravity = true;
				}
			}
			else
			{
				Projectile.Kill();
			}
		}

		private void HoldPlayer(Player player)
		{
			player.heldProj = Projectile.whoAmI;
			player.SetDummyItemTime(2);
			player.ChangeDir(Math.Cos(aimAngle) >= 0 ? 1 : -1);
			player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, aimAngle - MathHelper.PiOver2);
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			if (timer <= CHARGE_TIME) return false;

			int beamTimer = timer - CHARGE_TIME;
			float currentReach = MathHelper.Lerp(0f, MAX_DIST, MathHelper.Clamp(beamTimer / 15f, 0f, 1f));

			Vector2 dir = aimAngle.ToRotationVector2();
			Vector2 start = Projectile.Center + dir * 65f;
			Vector2 end = start + dir * currentReach;
			float point = 0f;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), start, end, 24f, ref point);
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			modifiers.HitDirectionOverride = Math.Sign(target.Center.X - Main.player[Projectile.owner].Center.X);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Player player = Main.player[Projectile.owner];
			Texture2D swordTex = TextureAssets.Projectile[Type].Value;

			Vector2 dir = aimAngle.ToRotationVector2();
			Vector2 drawPos = player.MountedCenter - Main.screenPosition;
			bool flip = Math.Cos(aimAngle) < 0;
			SpriteEffects fxFlip = flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

			Vector2 origin = flip
				? new Vector2(swordTex.Width - 6f, swordTex.Height - 6f)
				: new Vector2(6f, swordTex.Height - 6f);

			float rotOffset = flip ? MathHelper.Pi * 0.75f : MathHelper.PiOver4;

			SpriteBatch sb = Main.spriteBatch;


			sb.Draw(swordTex, drawPos, null, lightColor, aimAngle + rotOffset, origin, Projectile.scale * 1.15f, fxFlip, 0f);

			if (timer <= CHARGE_TIME) return false;

			int beamTimer = timer - CHARGE_TIME;
			float currentReach = MathHelper.Lerp(0f, MAX_DIST, MathHelper.Clamp(beamTimer / 15f, 0f, 1f));
			float fade = MathHelper.Clamp((TOTAL_TIME - timer) / 15f, 0f, 1f);

			if (currentReach <= 5f || fade <= 0.01f) return false;

			Vector2 tipPos = player.MountedCenter + dir * 65f - Main.screenPosition;
			Vector2 perpDir = new Vector2(-dir.Y, dir.X);

			Texture2D pixel = TextureAssets.MagicPixel.Value;


			int segments = 32;
			Vector2[] pts = new Vector2[segments + 1];
			pts[0] = tipPos;

			int seed = (int)(timer * 7) + Projectile.identity * 31;
			Random r = new Random(seed);

			for (int i = 1; i <= segments; i++)
			{
				float fraction = i / (float)segments;
				Vector2 basePt = tipPos + dir * (currentReach * fraction);

				if (i < segments)
				{

					float offsetAmp = (float)Math.Sin(fraction * MathHelper.Pi) * 11f * (1f - fraction * 0.5f);
					float offset = (float)(r.NextDouble() * 2.0 - 1.0) * offsetAmp;
					basePt += perpDir * offset;
				}
				pts[i] = basePt;
			}

			sb.End();
			sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);


			Color tipGlowColor = new Color(0, 255, 255) * 0.9f * fade;
			sb.Draw(pixel, tipPos, new Rectangle(0, 0, 1, 1), tipGlowColor, 0f, new Vector2(0.5f, 0.5f), new Vector2(16f, 16f), SpriteEffects.None, 0f);


			for (int pass = 0; pass < 3; pass++)
			{
				Color passColor;
				float baseThickness;

				if (pass == 0)
				{
					passColor = new Color(0, 180, 255) * 0.45f;
					baseThickness = 12f;
				}
				else if (pass == 1)
				{
					passColor = new Color(0, 255, 255) * 0.85f;
					baseThickness = 5.5f;
				}
				else
				{
					passColor = new Color(245, 255, 255) * 0.98f;
					baseThickness = 2.2f;
				}

				passColor *= fade;

				for (int i = 0; i < segments; i++)
				{
					Vector2 p1 = pts[i];
					Vector2 p2 = pts[i + 1];
					Vector2 diff = p2 - p1;
					float len = diff.Length();
					if (len <= 0.01f) continue;

					float segFraction = (i + 0.5f) / segments;

					float taper = MathHelper.Lerp(1.0f, 0.08f, (float)Math.Pow(segFraction, 0.75));
					float thick = baseThickness * taper;

					float segRot = diff.ToRotation();
					Vector2 scale = new Vector2(len, thick);
					Vector2 segOrigin = new Vector2(0f, 0.5f);

					sb.Draw(pixel, p1, new Rectangle(0, 0, 1, 1), passColor, segRot, segOrigin, scale, SpriteEffects.None, 0f);


					if (pass == 1 && i > 3 && i < segments - 3 && (i + seed) % 5 == 0)
					{
						float branchDirSign = ((i * 3 + seed) % 2 == 0) ? 1f : -1f;
						Vector2 branchDir = dir.RotatedBy(branchDirSign * (0.4f + (float)r.NextDouble() * 0.3f));
						float branchLen = (1f - segFraction) * (18f + (float)r.NextDouble() * 14f);
						Vector2 branchEnd = p1 + branchDir * branchLen;

						Vector2 branchDiff = branchEnd - p1;
						float bLen = branchDiff.Length();
						if (bLen > 0.01f)
						{
							Vector2 bScale = new Vector2(bLen, thick * 0.6f);
							sb.Draw(pixel, p1, new Rectangle(0, 0, 1, 1), passColor * 0.7f, branchDiff.ToRotation(), segOrigin, bScale, SpriteEffects.None, 0f);
						}
					}
				}
			}

			sb.End();
			sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			return false;
		}
	}
}
