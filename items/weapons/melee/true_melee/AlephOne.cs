using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.projectiles.melee.true_melee;

namespace jarkos.items.weapons.melee.true_melee
{
	public class AlephOne : ModItem
	{
		public override void SetDefaults()
		{
			Item.damage = 19;
			Item.DamageType = DamageClass.Melee;
			Item.width = 52;
			Item.height = 52;
			Item.useTime = 24;
			Item.useAnimation = 24;
			Item.scale = 1f;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 2;
			Item.shoot = ModContent.ProjectileType<AlephOne_Proj>();
			Item.shootSpeed = 14f;
			Item.autoReuse = true;

			Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 20, 0);
		}
		public override void AddRecipes() 
		{
			CreateRecipe()
			.AddIngredient(ItemID.WoodenSword, 1)
			.AddIngredient(ItemID.Glass, 50)
			.AddIngredient(ItemID.Amethyst, 3)
			.AddIngredient(ItemID.Ruby, 3)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
