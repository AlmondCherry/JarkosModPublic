using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.materials
{
	public class NitoziumPlate : ModItem
	{
        public override void SetDefaults()
		{
            Item.width = 26;
            Item.height = 34;
			Item.maxStack = Item.CommonMaxStack;
			Item.material = true;
		}
		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<Nitozium>(), 5)
            .AddIngredient(ItemID.LunarBar, 5)
			.AddIngredient(ItemID.BeetleHusk, 5)
            .AddTile(TileID.LunarCraftingStation)
			.Register();
		}
    }
}
