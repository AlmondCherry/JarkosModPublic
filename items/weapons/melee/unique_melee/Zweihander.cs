using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.projectiles.melee.unique_melee;
using jarkos.items.usetiles;
using jarkos.items.materials;

namespace jarkos.items.weapons.melee.unique_melee
{
	public class Zweihander : ModItem
	{
		public int comboStep = 0;
		public int comboExpireTimer = 0;

		public override void SetDefaults()
		{
			Item.damage = 1004;
			Item.crit = 15;
			Item.DamageType = DamageClass.Melee;
			Item.autoReuse = true;
			Item.knockBack = 6f;
			Item.useTime = 22;
			Item.useAnimation = 22;
			Item.width = 60;
			Item.height = 60;
			Item.scale = 1f;
			Item.rare = ModContent.RarityType<jarkos.rare.SpeedRarity>();
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.channel = true;
			Item.shoot = ModContent.ProjectileType<Degeneracy_sword_proj>();
			Item.shootSpeed = 1f;

            Item.value = Item.sellPrice(0, 70, 50, 0);
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			int dir = comboStep % 2 == 0 ? 1 : -1;
			Projectile.NewProjectile(source, player.MountedCenter, velocity, type, damage, knockback, player.whoAmI, comboStep, dir);

			comboStep = (comboStep + 1) % 5;
			comboExpireTimer = 0;
			return false;
		}

		public override void UpdateInventory(Player player) {
			comboExpireTimer++;
			if (comboExpireTimer >= 150)
			{
				comboStep = 0;
			}
		}

		public override void AddRecipes()
		{
			CreateRecipe()
			.AddIngredient(ModContent.ItemType<Sunfate>(), 1)
			.AddIngredient(ModContent.ItemType<AntiqueBar>(), 6)
			.AddIngredient(ModContent.ItemType<AncientFragment>(), 4)
			.AddTile(TileID.LunarCraftingStation)
			.Register();
		}
	}
}
