using System;
using jarkos.items.materials;
using Jarkos.projectiles.summon.staff;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.weapons.summon.staff.Vigilante
{
	#region Buff
	public class Vigilante_buff : ModBuff
	{
		public override void SetStaticDefaults() {
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
		}
		public override void Update(Player player, ref int buffIndex) {
			if (player.ownedProjectileCounts[ModContent.ProjectileType<Vigilante_proj>()] > 0) {
				player.buffTime[buffIndex] = 2;
			}
			else {
				player.DelBuff(buffIndex);
				buffIndex--;
			}
		}
	}
	#endregion
	#region Item
	public class Vigilante_staff : ModItem
	{
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
			ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
			ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
		}
		public override void SetDefaults()
		{
			Item.damage = 9;
			Item.crit = 5;
			Item.knockBack = 1f;
			Item.mana = 15;
			Item.width = 60;
			Item.height = 60;
			Item.useTime = 32;
			Item.useAnimation = 32;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.UseSound = SoundID.Item44;
			Item.noMelee = true;
			Item.DamageType = DamageClass.Summon;
			Item.buffType = ModContent.BuffType<Vigilante_buff>();
			Item.shoot = ModContent.ProjectileType<Vigilante_proj>();

			Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 20, 0);
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			position = Main.MouseWorld;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			player.AddBuff(ModContent.BuffType<Vigilante_buff>(), 180);
			var projectile = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, Main.myPlayer);
			projectile.originalDamage = Item.damage;
			return false;
		}
		public override void AddRecipes()
		{
			CreateRecipe()
			.AddTile(TileID.Anvils)
			.AddIngredient(ItemID.Topaz, 3)
			.AddIngredient(ItemID.Diamond, 2)
			.AddIngredient(ModContent.ItemType<FragmentOfFear>(), 5)
			.AddRecipeGroup(nameof(ItemID.TissueSample), 12)
			.Register();
		}
	}
	#endregion
	#region Projectile
	public class Vigilante_proj : ModProjectile
	{
		private float orbitAngle = 0f;
		private float orbitRadiusTimer = 0f;
		private int shootSequenceCount = 0;
		private int shootTimer = 20;
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
			Main.projPet[Projectile.type] = true;
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}
		public sealed override void SetDefaults()
		{
			Projectile.width = 18;
			Projectile.height = 28;
			Projectile.tileCollide = false;
			Projectile.friendly = true;
			Projectile.minion = true;
			Projectile.minionSlots = 1;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.penetrate = -1;
		}
		public override bool? CanCutTiles()
		{
			return false;
		}
		public override bool MinionContactDamage()
		{
			return false;
		}
		public override bool? CanDamage()
		{
			return false;
		}
		public override void AI()
		{
			Player owner = Main.player[Projectile.owner];
			if (!CheckActive(owner))
			{
				return;
			}
			GeneralBehavior(owner, out Vector2 vectorToIdlePosition, out float distanceToIdlePosition);
			SearchForTargets(owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter);
			Movement(foundTarget, distanceFromTarget, targetCenter, distanceToIdlePosition, vectorToIdlePosition);
			Visuals();
			float targetRotation = Projectile.velocity.X * 0.045f;
			targetRotation = MathHelper.Clamp(targetRotation, -0.4f, 0.4f);
			Projectile.rotation = MathHelper.Lerp(Projectile.rotation, targetRotation, 0.12f);
			Projectile.gfxOffY = MathF.Sin(Main.GameUpdateCount * 0.05f + Projectile.whoAmI * 0.7f) * 5f;
		}
		private bool CheckActive(Player owner)
		{
			if (owner.dead || !owner.active)
			{
				owner.ClearBuff(ModContent.BuffType<Vigilante_buff>());
				return false;
			}
			if (owner.HasBuff(ModContent.BuffType<Vigilante_buff>()))
			{
				Projectile.timeLeft = 2;
			}
			return true;
		}
		#region General AI
		private void GeneralBehavior(Player owner, out Vector2 vectorToIdlePosition, out float distanceToIdlePosition)
		{
			int count = owner.ownedProjectileCounts[Projectile.type];
			if (count <= 0) count = 1;
			float orbitSpeed = 0.015f;
			float angle = (Main.GameUpdateCount * orbitSpeed) + (Projectile.minionPos * (MathHelper.TwoPi / count));
			float radius = 75f;
			Vector2 idlePosition = owner.Center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius;
			vectorToIdlePosition = idlePosition - Projectile.Center;
			distanceToIdlePosition = vectorToIdlePosition.Length();
			if (Main.myPlayer == owner.whoAmI && distanceToIdlePosition > 2000f)
			{
				Projectile.position = idlePosition;
				Projectile.velocity *= 0.1f;
				Projectile.netUpdate = true;
			}
		}
		private void SearchForTargets(Player owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter)
		{
			distanceFromTarget = 900f;
			targetCenter = Projectile.position;
			foundTarget = false;
			if (owner.HasMinionAttackTargetNPC)
			{
				NPC npc = Main.npc[owner.MinionAttackTargetNPC];
				float distance_tonpc = Vector2.Distance(npc.Center, Projectile.Center);
				if (distance_tonpc < 2000f)
				{
					distanceFromTarget = distance_tonpc;
					targetCenter = npc.Center;
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
						float distance_tonpc = Vector2.Distance(npc.Center, Projectile.Center);
						bool closest = Vector2.Distance(Projectile.Center, targetCenter) > distance_tonpc;
						bool inRange = distance_tonpc < distanceFromTarget;
						bool lineOfSight = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height);
						bool closeThroughWall = distance_tonpc < 100f;
						if (((closest && inRange) || !foundTarget) && (lineOfSight || closeThroughWall))
						{
							distanceFromTarget = distance_tonpc;
							targetCenter = npc.Center;
							foundTarget = true;
						}
					}
				}
			}
			Projectile.friendly = foundTarget;
		}
		private void Movement(bool foundTarget, float distanceFromTarget, Vector2 targetCenter, float distanceToIdlePosition, Vector2 vectorToIdlePosition)
		{
			Player owner = Main.player[Projectile.owner];
			int count = owner.ownedProjectileCounts[Projectile.type];
			if (count <= 0) count = 1;

			int index = Projectile.minionPos;
			int id = Projectile.identity;

			if (foundTarget)
			{
				Projectile.spriteDirection = targetCenter.X > Projectile.Center.X ? 1 : -1;

				float speedMult = 0.022f + index % 3 * 0.014f + id % 5 * 0.004f;
				orbitAngle += speedMult;

				float baseRadius = 110f + ((index * 53 + id * 29) % 130);
				orbitRadiusTimer += 0.015f + index % 4 * 0.007f;
				float radiusPulse = MathF.Sin(orbitRadiusTimer + index * 1.7f) * 35f;
				float finalRadius = baseRadius + radiusPulse;

				float phaseOffset = index * (MathHelper.TwoPi / count);
				Vector2 targetPos = targetCenter + new Vector2(MathF.Cos(orbitAngle + phaseOffset), MathF.Sin(orbitAngle + phaseOffset)) * finalRadius;

				Vector2 toTargetPos = targetPos - Projectile.Center;
				float distToTargetPos = toTargetPos.Length();
				if (distToTargetPos > 1f)
				{
					toTargetPos.Normalize();
					float targetSpeed = Math.Min(distToTargetPos * 0.14f, 24f);
					Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTargetPos * targetSpeed, 0.09f);
				}

				shootTimer--;
				if (shootTimer <= 0)
				{
					if (shootSequenceCount == 0)
					{
						shootSequenceCount = 2 + ((index + id) % 3);
						shootTimer = 0;
					}
					if (shootSequenceCount > 0)
					{
						Vector2 shootDir = targetCenter - Projectile.Center;
						shootDir.Normalize();
						if (Projectile.owner == Main.myPlayer)
						{
							Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, shootDir * 16f, ModContent.ProjectileType<Vigilante_shot_proj>(), Projectile.damage, Projectile.knockBack, Main.myPlayer);
						}
						SoundEngine.PlaySound(SoundID.Item11 with { Pitch = Main.rand.NextFloat(-0.15f, 0.25f), Volume = 0.5f }, Projectile.Center);
						for (int i = 0; i < 5; i++)
						{
							Dust d = Dust.NewDustDirect(Projectile.Center, 4, 4, DustID.CopperCoin, shootDir.X * 3f, shootDir.Y * 3f, 0, default, 1f);
							d.noGravity = true;
						}
						shootSequenceCount--;
						if (shootSequenceCount == 0)
						{
							shootTimer = 30 + ((index * 23 + id * 11) % 35);
						}
						else
						{
							shootTimer = 10 + index % 3 * 2;
						}
					}
				}
			}
			else
			{
				Projectile.spriteDirection = owner.Center.X > Projectile.Center.X ? 1 : -1;
				shootSequenceCount = 0;
				shootTimer = 15 + (index * 17 + id) % 30;

				if (distanceToIdlePosition > 12f)
				{
					Vector2 dir = vectorToIdlePosition;
					dir.Normalize();
					float targetSpeed = MathHelper.Clamp(distanceToIdlePosition * 0.1f, 2f, 16f);
					Projectile.velocity = Vector2.Lerp(Projectile.velocity, dir * targetSpeed, 0.08f);
				}
				else
				{
					Projectile.velocity *= 0.85f;
				}
			}
		}
		private void Visuals()
		{
			Lighting.AddLight(Projectile.Center, Color.White.ToVector3() * 0.78f);
		}
		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			int num156 = TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type];
			int y3 = num156 * Projectile.frame;

			Rectangle rectangle = new(0, y3, texture.Width, num156);
			Vector2 origin2 = rectangle.Size() / 2f;
			Color color26 = lightColor;
			color26 = Projectile.GetAlpha(color26);
			SpriteEffects fx = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

			for (float i = 0; i < ProjectileID.Sets.TrailCacheLength[Projectile.type]; i += 0.5f)
			{
				Color color27 = new Color(255, 177, 131) * Projectile.Opacity * 0.33f;
				color27.A = (byte)(color26.A / 2);
				int max0 = Math.Max((int)i - 1, 0);

				if (max0 < 0)
					continue;

				float rot = Projectile.rotation;
				Vector2 center = Vector2.Lerp(Projectile.oldPos[(int)i], Projectile.oldPos[max0], 1 - i % 1);
				center += Projectile.Size / 2;
				
				Main.EntitySpriteDraw(texture, center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), color27, rot, origin2, Projectile.scale, fx, 0);
			}
			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), color26, Projectile.rotation, origin2, Projectile.scale, fx, 0);
			return false;
		}
		#endregion
		#endregion
	}
}
