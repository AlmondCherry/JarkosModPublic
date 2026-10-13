using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.materials
{
	public class CausticSubstance : ModItem
	{
        public override void SetDefaults()
		{
            Item.width = 32;
            Item.height = 32;
			Item.maxStack = Item.CommonMaxStack;
			Item.material = true;
		}
        public override void AddRecipes()
		{
			CreateRecipe(3)
			.AddIngredient(ItemID.Stinger, 1)
			.AddIngredient(ItemID.RottenChunk, 2)
			.AddIngredient(ItemID.Gel, 5)
            .AddTile(TileID.Bottles)
			.Register();

			CreateRecipe(3)
			.AddIngredient(ItemID.Stinger, 1)
			.AddIngredient(ItemID.Vertebrae, 2)
			.AddIngredient(ItemID.Gel, 5)
            .AddTile(TileID.Bottles)
			.Register();
		}
    }
}
