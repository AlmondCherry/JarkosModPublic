using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.materials
{
	public class ThickSilk : ModItem
	{
        public override void SetDefaults() 
		{
            Item.width = 28;
            Item.height = 28;
			Item.maxStack = Item.CommonMaxStack;
			Item.material = true;
			Item.value = Item.buyPrice(silver: 1);
		}
        public override void AddRecipes() 
		{
			CreateRecipe(5)
			.AddIngredient(ItemID.Silk, 5)
			.AddIngredient(ModContent.ItemType<RemainsOfMoth>(), 1)
            .AddTile(TileID.Loom)
			.Register();
		}
    }
}
