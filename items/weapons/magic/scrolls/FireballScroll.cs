using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.projectiles.magic.scrolls;
using jarkos.items.materials;
namespace jarkos.items.weapons.magic.scrolls
{
	public class FireballScroll : ModItem
	{
		public override void SetDefaults()
		{
			Item.damage = 31;
            Item.maxStack = 9999;
			Item.DamageType = DamageClass.Magic;
			Item.width = 32;
			Item.height = 32;
			Item.useTime = 15;
			Item.useAnimation = 15;
			Item.scale = 1f;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 2;
			Item.noMelee = true;
      		Item.noUseGraphic = true;
			Item.consumable = true;
			Item.shoot = ModContent.ProjectileType<Fire_ball01>();
			Item.shootSpeed = 20f;

			Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 0, 50);
		}
		public override void AddRecipes() {
			CreateRecipe(10)
			.AddIngredient(ModContent.ItemType<FragmentOfFear>(), 1)
			.Register();
		}
	}
}
