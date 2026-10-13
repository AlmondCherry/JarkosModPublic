using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.projectiles.melee.unique_melee;
using jarkos.items.materials;
using jarkos.items.usetiles;
using jarkos.systems;
using Terraria.Audio;

namespace jarkos.items.weapons.melee.unique_melee
{
	public class Thermal_scythe : ModItem, IRMBCooldown
	{
		public bool GetRMBCooldown(Player player, out int currentCooldown, out int maxCooldown)
		{
			var modPl = player.GetModPlayer<Jarkos_player>();
			currentCooldown = modPl.thermalScytheCooldown;
			maxCooldown = 300;
			return currentCooldown > 0;
		}

		public override void SetDefaults()
		{
			Item.damage = 132;
			Item.DamageType = DamageClass.Melee;
			Item.knockBack = 2;
			Item.useTime = 24;
			Item.useAnimation = 24;
			Item.width = 38;
			Item.height = 38;
			Item.scale = 1f;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.shoot = ModContent.ProjectileType<Thermal_scythe_proj>();
			Item.shootSpeed = 1f;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.shootsEveryUse = true;
			Item.autoReuse = true;

			Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 2, 20, 0);
		}

		public override bool AltFunctionUse(Player player)
		{
			return true;
		}

		public override bool CanUseItem(Player player)
		{
			var modPl = player.GetModPlayer<Jarkos_player>();
			if (player.altFunctionUse == 2)
			{
				if (modPl.thermalScytheCooldown > 0 || modPl.thermalScytheDashing)
					return false;

				NPC target = FindNearestEnemy(player, 1000f);
				if (target == null)
					return false;

				return true;
			}
			return true;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			var modPl = player.GetModPlayer<Jarkos_player>();
			if (player.altFunctionUse == 2)
			{
				NPC target = FindNearestEnemy(player, 1000f);
				if (target != null)
				{
					Vector2 dir = Vector2.Normalize(target.Center - player.Center);
					if (dir == Vector2.Zero) dir = new Vector2(player.direction, 0);

					modPl.thermalScytheDashing = true;
					modPl.thermalScytheStartPos = player.Center;
					modPl.thermalScytheTargetPos = target.Center + dir * 64f;
					modPl.thermalScytheDashTimer = 0;
					modPl.thermalScytheDashDuration = 14;
					modPl.thermalScytheTargetNPC = target;
					modPl.thermalScytheDamage = (int)(damage * 1.20f);
					modPl.thermalScytheCooldown = 300;

					modPl.thermalDashTrailPoints.Clear();
					modPl.thermalDashTrailPoints.Add(player.Center);
					modPl.thermalDashTrailAlpha = 1f;

					SoundEngine.PlaySound(SoundID.Item71, player.Center);
				}
				return false;
			}
			return true;
		}

		public override void UpdateInventory(Player player)
		{
			var modPl = player.GetModPlayer<Jarkos_player>();
			if (Main.player[Main.myPlayer].HeldItem.ModItem == this)
			{
				modPl.thermal_scythe_held = true;
			}
		}

		private NPC FindNearestEnemy(Player player, float maxRange)
		{
			NPC nearest = null;
			float minDistanceSq = maxRange * maxRange;
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.dontTakeDamage && npc.CanBeChasedBy())
				{
					float distSq = Vector2.DistanceSquared(player.Center, npc.Center);
					if (distSq < minDistanceSq)
					{
						minDistanceSq = distSq;
						nearest = npc;
					}
				}
			}
			return nearest;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
			.AddIngredient(ModContent.ItemType<BloodyAlloy>(), 12)
			.AddIngredient(ItemID.Obsidian, 20)
			.AddRecipeGroup(nameof(ItemID.TitaniumBar), 18)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
}
