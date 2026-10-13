using System;
using jarkos.particles;
using Jarkos.trails;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.weapons.summon.staff.PhantomDagger
{
	#region Buff
	public class PhantomDagger_buff : ModBuff
	{
		public override void SetStaticDefaults() {
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
		}
		public override void Update(Player player, ref int buffIndex) {
			if (player.ownedProjectileCounts[ModContent.ProjectileType<PhantomDagger_proj>()] > 0) {
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
	public class PhantomDagger_item : ModItem
	{
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
			ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true;
			ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
		}
		public override void SetDefaults()
		{
			Item.damage = 32;
			Item.crit = 6;
			Item.knockBack = 2f;
			Item.mana = 12;
			Item.width = 38;
			Item.height = 82;
			Item.useTime = 21;
			Item.rare = ModContent.RarityType<jarkos.rare.StormRarity>();
			Item.useAnimation = 21;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.UseSound = SoundID.Item20;
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.DamageType = DamageClass.Summon;
			Item.buffType = ModContent.BuffType<PhantomDagger_buff>();
			Item.shoot = ModContent.ProjectileType<PhantomDagger_proj>();

            Item.value = Item.sellPrice(0, 5, 0, 0);
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			position = Main.MouseWorld;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			player.AddBuff(ModContent.BuffType<PhantomDagger_buff>(), 180);
			var projectile = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, Main.myPlayer);
			projectile.originalDamage = Item.damage;
			return false;
		}
	}
	#endregion
	#region Projectile
	public class PhantomDagger_proj : ModProjectile
	{
		private bool hitted = false;
		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
			Main.projPet[Projectile.type] = true;
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
			ProjectileID.Sets.AllowsContactDamageFromJellyfish[Type] = true;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}
		public sealed override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 9;
			Projectile.tileCollide = false;
			Projectile.friendly = true;
			Projectile.minion = true;
			Projectile.minionSlots = 1;
			Projectile.DamageType = DamageClass.Summon;
			Projectile.penetrate = -1;
			Projectile.light = 0.75f;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 1;
		}
		public override bool? CanCutTiles()
		{
			return false;
		}
		public override bool MinionContactDamage()
		{
			if (!hitted)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		public override void AI()
		{
			Player owner = Main.player[Projectile.owner];
			if (hitted)
			{
				Projectile.ai[0]++;
				if (Projectile.ai[0] > 25f)
				{
					hitted = false;
					Projectile.ai[0]= 0f;
				}
			}
			if (!CheckActive(owner))
			{
				return;
			}
			GeneralBehavior(owner, out Vector2 vectorToIdlePosition, out float distanceToIdlePosition);
			SearchForTargets(owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter);
			Movement(foundTarget, distanceFromTarget, targetCenter, distanceToIdlePosition, vectorToIdlePosition);
			Visuals();
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			hitted = true;
			for (int i = 0; i < 2; i++)
			{
				float modifier = Main.rand.NextFloat(0.25f, 2.25f);
				float size = Main.rand.NextFloat(0.5f, 0.75f);
				Vector2 vel = new Vector2(0, Main.rand.NextFloat(-15f, -10f));
				new Spark_gravit(Projectile.Center, vel.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-35f, 35f))) * modifier, Color.HotPink, 300, size * 2f, 2f, 0f).Spawn();
			}
			SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/something") with { Pitch = Main.rand.NextFloat(-0.25f, 0.25f) }, Projectile.position);
			Projectile.netUpdate = true;
		}
		private bool CheckActive(Player owner)
		{
			if (owner.dead || !owner.active)
			{
				owner.ClearBuff(ModContent.BuffType<PhantomDagger_buff>());
				return false;
			}
			if (owner.HasBuff(ModContent.BuffType<PhantomDagger_buff>()))
			{
				Projectile.timeLeft = 2;
			}
			return true;
		}
		private void GeneralBehavior(Player owner, out Vector2 vectorToIdlePosition, out float distanceToIdlePosition)
		{
			Vector2 idlePosition = owner.Center;
			idlePosition.Y -= 60f;
			float minionPositionOffsetX = 0;
			idlePosition.X += minionPositionOffsetX;
			vectorToIdlePosition = idlePosition - Projectile.Center;
			distanceToIdlePosition = vectorToIdlePosition.Length();
			if (Main.myPlayer == owner.whoAmI && distanceToIdlePosition > 2000f)
			{
				Projectile.position = idlePosition;
				Projectile.velocity *= 0.1f;
				Projectile.netUpdate = true;
			}
			float overlapVelocity = 0.00001f;
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile other = Main.projectile[i];
				if (i != Projectile.whoAmI && other.active && other.owner == Projectile.owner && Math.Abs(Projectile.position.X - other.position.X) + Math.Abs(Projectile.position.Y - other.position.Y) < Projectile.width)
				{
					if (Projectile.position.X < other.position.X)
					{
						Projectile.velocity.X -= overlapVelocity;
					}
					else
					{
						Projectile.velocity.X += overlapVelocity;
					}
					if (Projectile.position.Y < other.position.Y)
					{
						Projectile.velocity.Y -= overlapVelocity;
					}
					else
					{
						Projectile.velocity.Y += overlapVelocity;
					}
				}
			}
		}
		private void SearchForTargets(Player owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter)
		{
			distanceFromTarget = 700f;
			targetCenter = Projectile.position;
			foundTarget = false;
			if (owner.HasMinionAttackTargetNPC)
			{
				NPC npc = Main.npc[owner.MinionAttackTargetNPC];
				float between = Vector2.Distance(npc.Center, Projectile.Center);
				if (between < 2000f)
				{
					distanceFromTarget = between;
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
						float between = Vector2.Distance(npc.Center, Projectile.Center);
						bool closest = Vector2.Distance(Projectile.Center, targetCenter) > between;
						bool inRange = between < distanceFromTarget;
						bool lineOfSight = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height);
						bool closeThroughWall = between < 100f;
						if (((closest && inRange) || !foundTarget) && (lineOfSight || closeThroughWall))
						{
							distanceFromTarget = between;
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
			float speed = 18f;
			float inertia = 1f;
			if (foundTarget)
			{
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
				if (distanceFromTarget > 175f)
				{
					Vector2 direction = targetCenter - Projectile.Center;
					direction.Normalize();
					direction *= speed;
					Projectile.velocity = direction;
					hitted = false;
				}
				else
				{
					Projectile.velocity = Projectile.velocity.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(0.15f, 1.5f)));
					Vector2 vel = new Vector2(0, -15);
					if (hitted)
					{
						Projectile.velocity += vel.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-75f, 75f)));
					}
					if (Projectile.velocity == Vector2.Zero)
					{
						Vector2 direction = targetCenter - Projectile.Center;
						direction.Normalize();
						direction *= speed;
						Projectile.velocity = direction;
					}
				}
			}
			else
			{
				Projectile.rotation = 0;
				if (distanceToIdlePosition > 200f)
				{
					speed = 35f;
					inertia = 10f;
				}
				else
				{
					speed = 4f;
					inertia = 8f;
				}
				if (distanceToIdlePosition > 20f)
				{
					vectorToIdlePosition.Normalize();
					vectorToIdlePosition *= speed;
					Projectile.velocity = (Projectile.velocity * (inertia - 1) + vectorToIdlePosition) / inertia;
				}
				else if (Projectile.velocity == Vector2.Zero)
				{
					Projectile.velocity.X = -0.000005f;
					Projectile.velocity.Y = -0.000005f;
				}
				if (distanceToIdlePosition < 30f && !foundTarget)
				{
					Projectile.velocity = Projectile.velocity.RotatedBy(MathHelper.ToRadians(0.5f) * 2f);
				}
			}
		}
		private void Visuals()
		{
			Lighting.AddLight(Projectile.Center, Color.White.ToVector3() * 0.78f);
		}
		public Primitivedraw TrailDrawer { get; private set; } = null;
        public float WidthFunction(float completionRatio)
        {
            float width_trail = 3f;
            float baseWidth = Projectile.scale * (Projectile.width / 4) * width_trail;
            return MathHelper.SmoothStep(baseWidth, 3.5f, completionRatio);
        }
        public Color ColorFunction(float completionRatio)
        {
            return Color.Lerp(Color.HotPink, Color.Transparent, completionRatio) * 0.7f;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            TrailDrawer ??= new Primitivedraw(WidthFunction, ColorFunction, GameShaders.Misc["Jarkos:LaserShader"]);
            GameShaders.Misc["Jarkos:LaserShader"].SetShaderTexture(Trailreg.test);
            TrailDrawer.DrawPrims(Projectile.oldPos, Projectile.Size * 0.5f - Main.screenPosition, 155);
            return true;
        }
	}
	#endregion
}
