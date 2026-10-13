using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.projectiles.magic.staffs;
using jarkos.items.materials;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;

namespace jarkos.items.weapons.magic.staffs
{
	public class ShadowVolleyStaff : ModItem
	{
		public override void SetStaticDefaults()
		{
			Item.staff[Item.type] = true;
		}

		public override void SetDefaults()
		{
			Item.damage = 160;
			Item.DamageType = DamageClass.Magic;
			Item.width = 42;
			Item.height = 42;
			Item.useTime = 2;
			Item.useAnimation = 2;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.channel = true;
			Item.knockBack = 4f;
			Item.mana = 5;
			Item.autoReuse = true;
			Item.shoot = ModContent.ProjectileType<ShadowVolleyStaffHoldout>();
			Item.shootSpeed = 14f;

			Item.rare = ItemRarityID.Red;
			Item.value = Item.sellPrice(0, 10, 0, 0);
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			foreach (Projectile proj in Main.projectile)
			{
				if (proj.active && proj.owner == player.whoAmI && proj.type == type)
					return false;
			}
			Projectile.NewProjectile(source, player.Center, velocity, type, damage, knockback, player.whoAmI);
			return false;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<FragmentOfRage>(), 75)
			.AddIngredient(ItemID.LunarBar, 20)
			.AddTile(TileID.LunarCraftingStation)
			.Register();
		}
	}
}
