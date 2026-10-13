using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.projectiles.melee.unique_melee;

namespace jarkos.items.weapons.melee.unique_melee
{
	public class Brewmaster_weapon : ModItem
	{
		public override void SetDefaults()
		{
			Item.damage = 20;
			Item.DamageType = DamageClass.Melee;
			Item.width = 108;
			Item.height = 54;
			Item.useTime = 2;
			Item.crit = 10;
			Item.useAnimation = 2;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 9.5f;
			Item.autoReuse = true;
			Item.shootSpeed = 30;
			Item.shoot = ModContent.ProjectileType<Brewmaster_weapon_slash>();
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.channel = true;

			Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 10, 0);
		}

		public override void AddRecipes()
		{
			CreateRecipe()
			.AddIngredient(ItemID.Wood, 150)
			.AddIngredient(ItemID.Chain, 20)
			.AddIngredient(ItemID.Silk, 5)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
