using System;
using jarkos.items.materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.weapons.summon.staff.Plague
{
	public class Voron_buff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			if (player.ownedProjectileCounts[ModContent.ProjectileType<Voron>()] > 0)
			{
				player.buffTime[buffIndex] = 2;
			}
			else
			{
				player.DelBuff(buffIndex);
				buffIndex--;
			}
		}
	}

	public class Plague_staff : ModItem
	{
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
			ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
			ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
		}

		public override void SetDefaults()
		{
			Item.damage = 26;
			Item.crit = 4;
			Item.knockBack = 2f;
			Item.mana = 10;
			Item.width = 60;
			Item.height = 60;
			Item.useTime = 28;
			Item.useAnimation = 28;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.UseSound = Jarkos_sounds.RavenSummon;
			Item.noMelee = true;
			Item.DamageType = DamageClass.Summon;
			Item.buffType = ModContent.BuffType<Voron_buff>();
			Item.shoot = ModContent.ProjectileType<Voron>();

			Item.rare = ItemRarityID.Green;
			Item.value = Item.sellPrice(0, 1, 0, 0);
		}

		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			position = Main.MouseWorld;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			player.AddBuff(ModContent.BuffType<Voron_buff>(), 180);
			var projectile = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, Main.myPlayer);
			projectile.originalDamage = Item.damage;
			return false;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
			.AddTile(TileID.Anvils)
			.AddIngredient(ModContent.ItemType<FeatherBlack>(), 12)
			.AddIngredient(ItemID.Diamond, 4)
			.Register();
		}
	}

	public class Voron : ModProjectile
	{
		private enum AttackState
		{
			HoverAboveTarget,
			ArcSwoop
		}

		private AttackState State
		{
			get => (AttackState)Projectile.ai[0];
			set => Projectile.ai[0] = (float)value;
		}

		private float SwoopTimer
		{
			get => Projectile.ai[1];
			set => Projectile.ai[1] = value;
		}

		private int SwoopPhase
		{
			get => (int)Projectile.ai[2];
			set => Projectile.ai[2] = value;
		}

		private float swoopSide = 1f;
		private float trailAlpha = 0f;

		public override void SetStaticDefaults()
		{
			Main.projFrames[Projectile.type] = 5;
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
			Main.projPet[Projectile.type] = true;

			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}

		public sealed override void SetDefaults()
		{
			Projectile.width = 40;
			Projectile.height = 24;
			Projectile.tileCollide = false;
			Projectile.friendly = true;
			Projectile.minion = true;
			Projectile.minionSlots = 1f;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.penetrate = -1;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 10;
		}

		public override bool? CanCutTiles() => false;

		public override bool MinionContactDamage()
		{
			return State == AttackState.ArcSwoop;
		}

		public override void AI()
		{
			Player owner = Main.player[Projectile.owner];
			if (!CheckActive(owner))
				return;

			SearchForTargets(owner, out bool foundTarget, out float distanceFromTarget, out NPC targetNPC);

			if (foundTarget && targetNPC != null)
			{
				AttackBehavior(targetNPC);
			}
			else
			{
				IdleBehavior(owner);
			}

			Visuals();
		}

		private bool CheckActive(Player owner)
		{
			if (owner.dead || !owner.active)
			{
				owner.ClearBuff(ModContent.BuffType<Voron_buff>());
				return false;
			}
			if (owner.HasBuff(ModContent.BuffType<Voron_buff>()))
			{
				Projectile.timeLeft = 2;
			}
			return true;
		}

		private void SearchForTargets(Player owner, out bool foundTarget, out float distanceFromTarget, out NPC targetNPC)
		{
			distanceFromTarget = 900f;
			targetNPC = null;
			foundTarget = false;

			if (owner.HasMinionAttackTargetNPC)
			{
				NPC npc = Main.npc[owner.MinionAttackTargetNPC];
				float between = Vector2.Distance(npc.Center, Projectile.Center);
				if (between < 2000f)
				{
					distanceFromTarget = between;
					targetNPC = npc;
					foundTarget = true;
				}
			}

			if (!foundTarget)
			{
				for (int i = 0; i < Main.maxNPCs; i++)
				{
					NPC npc = Main.npc[i];
					if (npc.CanBeChasedBy())
					{
						float between = Vector2.Distance(npc.Center, Projectile.Center);
						if (between < distanceFromTarget)
						{
							distanceFromTarget = between;
							targetNPC = npc;
							foundTarget = true;
						}
					}
				}
			}

			Projectile.friendly = foundTarget && State == AttackState.ArcSwoop;
		}

		private void AttackBehavior(NPC target)
		{
			switch (State)
			{
				case AttackState.HoverAboveTarget:
					{
						float hoverOffsetX = (float)Math.Cos(Main.GameUpdateCount * 0.05f + Projectile.minionPos * 1.4f) * (90f + Projectile.minionPos * 20f);
						float hoverOffsetY = -230f + (float)Math.Sin(Main.GameUpdateCount * 0.08f + Projectile.minionPos) * 20f;
						Vector2 hoverTargetPos = target.Center + new Vector2(hoverOffsetX, hoverOffsetY);

						Vector2 toHover = hoverTargetPos - Projectile.Center;
						float distToHover = toHover.Length();

						if (distToHover > 16f)
						{
							Vector2 desiredVel = toHover.SafeNormalize(Vector2.Zero) * MathHelper.Clamp(distToHover * 0.12f + 4f, 6f, 22f);
							Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVel, 0.18f);
						}

						SwoopTimer--;
						if (SwoopTimer <= 0)
						{
							StartArcSwoop();
						}
						break;
					}

				case AttackState.ArcSwoop:
					{
						if (SwoopPhase == 0)
						{
							Vector2 lowPoint = target.Center + new Vector2(swoopSide * 120f, 40f);
							Vector2 toLow = lowPoint - Projectile.Center;
							float distToLow = toLow.Length();

							Vector2 desiredVel = toLow.SafeNormalize(Vector2.Zero) * 22f;
							Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVel, 0.15f);

							if (distToLow < 70f || Projectile.Center.Y > target.Center.Y - 10f)
							{
								SwoopPhase = 1;
							}
						}
						else
						{
							Vector2 highPoint = target.Center + new Vector2(-swoopSide * 160f, -230f);
							Vector2 toHigh = highPoint - Projectile.Center;
							float distToHigh = toHigh.Length();

							Vector2 desiredVel = toHigh.SafeNormalize(Vector2.Zero) * 22f;
							Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVel, 0.15f);

							if (distToHigh < 70f)
							{
								FinishSwoop();
							}
						}
						break;
					}
			}
		}

		private void StartArcSwoop()
		{
			State = AttackState.ArcSwoop;
			SwoopPhase = 0;

			swoopSide = (Projectile.minionPos % 2 == 0) ? 1f : -1f;
			if (Main.rand.NextBool()) swoopSide *= -1f;

			SoundEngine.PlaySound(Jarkos_sounds.RavenSummon with { Pitch = Main.rand.NextFloat(-0.1f, 0.1f) }, Projectile.Center);
		}

		private void FinishSwoop()
		{
			State = AttackState.HoverAboveTarget;
			SwoopPhase = 0;
			SwoopTimer = Main.rand.Next(45, 90);
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (State == AttackState.ArcSwoop)
			{
				SoundEngine.PlaySound(Jarkos_sounds.RavenHit with { Pitch = Main.rand.NextFloat(0f, 0.2f) }, Projectile.Center);
			}
		}

		private void IdleBehavior(Player owner)
		{
			State = AttackState.HoverAboveTarget;
			SwoopTimer = 20 + (int)(Projectile.minionPos * 20);

			float orbitAngle = Main.GameUpdateCount * 0.04f + Projectile.minionPos * (MathHelper.TwoPi / Math.Max(1, owner.ownedProjectileCounts[Projectile.type]));
			Vector2 orbitOffset = new Vector2((float)Math.Cos(orbitAngle) * 100f, (float)Math.Sin(orbitAngle) * 50f - 40f);
			Vector2 idlePosition = owner.Center + orbitOffset;

			Vector2 toIdle = idlePosition - Projectile.Center;
			float dist = toIdle.Length();

			if (dist > 1500f)
			{
				Projectile.Center = idlePosition;
				Projectile.velocity = Vector2.Zero;
			}
			else if (dist > 16f)
			{
				Vector2 desiredVel = toIdle.SafeNormalize(Vector2.Zero) * MathHelper.Clamp(dist * 0.1f + 4f, 6f, 20f);
				Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVel, 0.12f);
			}
		}

		private void Visuals()
		{
			if (State == AttackState.ArcSwoop)
			{
				trailAlpha = MathHelper.Clamp(trailAlpha + 0.15f, 0f, 1f);
			}
			else
			{
				trailAlpha = MathHelper.Clamp(trailAlpha - 0.08f, 0f, 1f);
			}

			if (Projectile.velocity.LengthSquared() > 0.1f)
			{
				float targetRot = Projectile.velocity.ToRotation() + MathHelper.Pi;
				Projectile.rotation = Terraria.Utils.AngleLerp(Projectile.rotation, targetRot, 0.35f);
			}

			int frameSpeed = 4;
			Projectile.frameCounter++;
			if (Projectile.frameCounter >= frameSpeed)
			{
				Projectile.frameCounter = 0;
				Projectile.frame = (Projectile.frame + 1) % 5;
			}

			Lighting.AddLight(Projectile.Center, Color.Purple.ToVector3() * 0.4f);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (trailAlpha > 0f)
			{
				Texture2D trailTex = ModContent.Request<Texture2D>("jarkos/items/weapons/summon/staff/Plague/VoronTrail").Value;
				Vector2 trailOrigin = trailTex.Size() / 2f;

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

				for (int i = 0; i < Projectile.oldPos.Length; i++)
				{
					if (Projectile.oldPos[i] == Vector2.Zero) continue;

					Vector2 oldDrawPos = Projectile.oldPos[i] + new Vector2(Projectile.width, Projectile.height) * 0.5f - Main.screenPosition;
					float factor = 1f - (float)i / Projectile.oldPos.Length;
					Color trailColor = Color.Red * factor * trailAlpha * 0.85f;
					float oldRot = Projectile.oldRot[i];

					Main.spriteBatch.Draw(trailTex, oldDrawPos, null, trailColor, oldRot, trailOrigin, Projectile.scale * MathHelper.Lerp(0.6f, 1.0f, factor), SpriteEffects.None, 0f);
				}

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			}

			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

			int frameWidth = 52;
			int frameHeight = 26;
			Rectangle sourceRect = new Rectangle(0, Projectile.frame * frameHeight, frameWidth, frameHeight);

			Vector2 origin = new Vector2(frameWidth / 2f, frameHeight / 2f);
			Vector2 drawPos = Projectile.Center - Main.screenPosition;

			Main.spriteBatch.Draw(texture, drawPos, sourceRect, lightColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
			return false;
		}
	}
}
