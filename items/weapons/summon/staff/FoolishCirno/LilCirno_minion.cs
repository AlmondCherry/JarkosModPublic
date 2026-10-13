using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using ReLogic.Content;

namespace jarkos.items.weapons.summon.staff.FoolishCirno
{
	public class LilCirno_minion : ModProjectile
	{
		public override string Texture => "jarkos/items/weapons/summon/staff/FoolishCirno/LilCirno_Minion";

		private float floatTimer = 0f;
		private int attackTimer = 0;
		private int attackPhase = 0;
		private float colorHueTimer = 0f;
		private bool isInitialized = false;


		private float recoilTimer = 0f;
		private int tiltDirection = 1;

		private static readonly Color[] RainbowPalette = new Color[]
		{
			new Color(255, 40, 40),
			new Color(255, 140, 0),
			new Color(255, 235, 0),
			new Color(40, 230, 60),
			new Color(0, 220, 255),
			new Color(30, 80, 255),
			new Color(180, 50, 255)
		};

		public override void SetStaticDefaults()
		{
			Main.projPet[Projectile.type] = false;
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.width = 24;
			Projectile.height = 30;
			Projectile.friendly = true;
			Projectile.minion = true;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.minionSlots = 1f;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 18000;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.scale = 0.5f;
		}

		public override bool? CanCutTiles() => false;
		public override bool MinionContactDamage() => false;

		public override void AI()
		{
			Player player = Main.player[Projectile.owner];

			if (!isInitialized)
			{
				isInitialized = true;
				colorHueTimer = Main.rand.NextFloat(0f, 7f);
			}

			colorHueTimer += 1.0f / 90f;
			if (colorHueTimer >= 7f)
			{
				colorHueTimer -= 7f;
			}

			if (Projectile.active)
			{
				player.ownedProjectileCounts[Projectile.type]++;
			}

			if (player.dead || !player.active)
			{
				player.ClearBuff(ModContent.BuffType<LilCirno_buff>());
			}

			if (recoilTimer > 0f)
			{
				recoilTimer *= 0.86f;
				if (recoilTimer < 0.01f) recoilTimer = 0f;
			}

			if (player.HasBuff(ModContent.BuffType<LilCirno_buff>()))
			{
				Projectile.timeLeft = 18000;
			}
			else
			{
				Projectile.Kill();
				return;
			}

			floatTimer += 0.05f;
			NPC target = FindTarget(player);

			int minionIndex = 0;
			int totalMinions = 0;
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if (p.active && p.owner == player.whoAmI && p.type == Projectile.type)
				{
					if (i == Projectile.identity)
					{
						minionIndex = totalMinions;
					}
					totalMinions++;
				}
			}
			if (totalMinions == 0) totalMinions = 1;

			if (target == null)
			{
				float orbitRadiusX = 55f;
				float orbitRadiusY = 28f;
				float angle = floatTimer * 1.2f + (minionIndex * MathHelper.TwoPi / totalMinions);

				Vector2 orbitOffset = new Vector2(
					(float)Math.Cos(angle) * orbitRadiusX,
					(float)Math.Sin(angle) * orbitRadiusY - 30f
				);

				Vector2 idleTargetPos = player.Center + orbitOffset;
				Vector2 moveVel = idleTargetPos - Projectile.Center;
				float dist = moveVel.Length();

				if (dist > 1200f)
				{
					Projectile.Center = idleTargetPos;
				}
				else if (dist > 4f)
				{
					float moveSpeed = MathHelper.Clamp(dist * 0.2f, 3f, 18f);
					Projectile.velocity = Vector2.Lerp(Projectile.velocity, moveVel.SafeNormalize(Vector2.Zero) * moveSpeed, 0.2f);
				}
				else
				{
					Projectile.velocity *= 0.85f;
				}

				Projectile.spriteDirection = player.direction;
			}
			else
			{
				float combatOffsetX = (minionIndex % 2 == 0 ? -40f : 40f) - (minionIndex * 10f * player.direction);
				Vector2 combatTargetPos = player.Center + new Vector2(combatOffsetX, -36f + (float)Math.Sin(floatTimer + minionIndex) * 6f);

				Vector2 moveVel = combatTargetPos - Projectile.Center;
				float dist = moveVel.Length();

				if (dist > 1200f)
				{
					Projectile.Center = combatTargetPos;
				}
				else if (dist > 4f)
				{
					float moveSpeed = MathHelper.Clamp(dist * 0.22f, 4f, 20f);
					Projectile.velocity = Vector2.Lerp(Projectile.velocity, moveVel.SafeNormalize(Vector2.Zero) * moveSpeed, 0.25f);
				}
				else
				{
					Projectile.velocity *= 0.8f;
				}

				Projectile.spriteDirection = target.Center.X > Projectile.Center.X ? 1 : -1;

				if (Projectile.owner == Main.myPlayer)
				{
					attackTimer++;
					if (attackTimer >= Main.rand.Next(40, 90))
					{
						attackTimer = 0;
						ExecuteAttack(target);
					}
				}
			}

			if (Main.rand.NextBool(5))
			{
				Color currentColor = GetCurrentMinionColor();
				Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.IceTorch, 0f, 0f, 100, currentColor, 0.75f);
				d.noGravity = true;
				d.velocity = Projectile.velocity * 0.2f;
			}
		}

		private NPC FindTarget(Player player)
		{
			float maxDetectDist = 800f;
			NPC target = null;

			if (player.HasMinionAttackTargetNPC)
			{
				NPC npc = Main.npc[player.MinionAttackTargetNPC];
				if (npc.CanBeChasedBy() && Vector2.Distance(Projectile.Center, npc.Center) < maxDetectDist)
				{
					return npc;
				}
			}

			float closestDist = maxDetectDist;
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (npc.active && !npc.friendly && npc.CanBeChasedBy() && !npc.dontTakeDamage)
				{
					float dist = Vector2.Distance(Projectile.Center, npc.Center);
					if (dist < closestDist)
					{
						closestDist = dist;
						target = npc;
					}
				}
			}
			return target;
		}

		private void ExecuteAttack(NPC target)
		{
			Vector2 shootOrigin = Projectile.Center;
			Vector2 aimDir = (target.Center - shootOrigin).SafeNormalize(Vector2.UnitX);

			recoilTimer = 1.0f;
			tiltDirection = -tiltDirection;

			if (attackPhase == 0)
			{
				Vector2 vel = aimDir * 11f;
				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(),
					shootOrigin,
					vel,
					ModContent.ProjectileType<LilCirno_Snowflake_proj>(),
					Projectile.damage,
					Projectile.knockBack,
					Projectile.owner
				);
				SoundEngine.PlaySound(SoundID.Item28 with { Volume = 0.5f, Pitch = 0.4f }, shootOrigin);
			}
			else if (attackPhase == 1)
			{
				Vector2 vel = aimDir * 3f;
				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(),
					shootOrigin,
					vel,
					ModContent.ProjectileType<LilCirno_BiggerSnowflake_proj>(),
					(int)(Projectile.damage * 1.15f),
					Projectile.knockBack * 1.1f,
					Projectile.owner
				);
				SoundEngine.PlaySound(SoundID.Item30 with { Volume = 0.55f, Pitch = 0.3f }, shootOrigin);
			}
			else if (attackPhase == 2)
			{
				bool isGiant = Main.rand.NextFloat() < 0.04f;
				float giantParam = isGiant ? 1f : 0f;

				Vector2 vel = aimDir * 10f;
				Projectile.NewProjectile(
					Projectile.GetSource_FromThis(),
					shootOrigin,
					vel,
					ModContent.ProjectileType<LilCirno_IceShard_proj>(),
					Projectile.damage,
					Projectile.knockBack * 1.2f,
					Projectile.owner,
					ai0: giantParam
				);
				SoundEngine.PlaySound(SoundID.Item28 with { Volume = 0.6f, Pitch = 0.1f }, shootOrigin);
			}

			attackPhase = (attackPhase + 1) % 3;
		}

		public Color GetCurrentMinionColor()
		{
			float t = colorHueTimer % 7f;
			if (t < 0f) t += 7f;

			int currentIndex = (int)t;
			int nextIndex = (currentIndex + 1) % 7;
			float factor = t - currentIndex;

			return Color.Lerp(RainbowPalette[currentIndex], RainbowPalette[nextIndex], factor);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			SpriteBatch sb = Main.spriteBatch;
			Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
			Vector2 origin = tex.Size() / 2f;
			Vector2 drawPos = Projectile.Center - Main.screenPosition;

			SpriteEffects fx = Projectile.spriteDirection < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

			float spinScaleX = 1.0f;
			float squashYFactor = 1.0f;

			float tiltAngle = MathHelper.ToRadians(25f) * tiltDirection * recoilTimer;
			float recoilSquashY = MathHelper.Lerp(1.0f, 0.70f, recoilTimer);
			float recoilStretchX = MathHelper.Lerp(1.0f, 1.25f, recoilTimer);

			Color minionColor = GetCurrentMinionColor();
			Color drawColor = Color.Lerp(lightColor, minionColor, 0.70f);

			Vector2 drawScale = new Vector2(spinScaleX * recoilStretchX, squashYFactor * recoilSquashY) * Projectile.scale;

			Texture2D glowTex = ModContent.Request<Texture2D>("jarkos/particles/Circular_glow", AssetRequestMode.ImmediateLoad).Value;
			if (glowTex != null)
			{
				sb.End();
				sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

				Color glowColor = minionColor * 0.40f;
				sb.Draw(glowTex, drawPos, null, glowColor, 0f, glowTex.Size() / 2f, 0.38f, SpriteEffects.None, 0f);

				sb.End();
				sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			}

			float drawRotation = Projectile.rotation + tiltAngle;

			sb.Draw(tex, drawPos, null, drawColor, drawRotation, origin, drawScale, fx, 0f);

			return false;
		}
	}
}
