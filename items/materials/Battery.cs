using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Jarkos.items.materials
{
	public class Battery : ModItem
	{
        public override void SetDefaults()
		{
            Item.width = 24;
            Item.height = 36;
			Item.maxStack = Item.CommonMaxStack;
			Item.material = true;
			Item.value = Item.buyPrice(silver: 2);
		}
		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddRecipeGroup(nameof(ItemID.IronBar), 1)
			.AddRecipeGroup(nameof(ItemID.CopperBar), 1)
			.AddIngredient(ItemID.Gel, 2)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
