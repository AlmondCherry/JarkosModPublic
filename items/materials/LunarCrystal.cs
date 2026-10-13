using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.materials
{
	public class LunarCrystal : ModItem
	{
        public override void SetDefaults()
		{
            Item.width = 26;
            Item.height = 30;
			Item.maxStack = Item.CommonMaxStack;
			Item.material = true;
			Item.value = Item.buyPrice(gold: 5);
		}
        public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ItemID.FragmentVortex, 2)
			.AddIngredient(ItemID.FragmentNebula, 2)
            .AddIngredient(ItemID.FragmentSolar, 2)
			.AddIngredient(ItemID.FragmentStardust, 2)
            .AddIngredient(ItemID.LunarBar, 1)
            .AddTile(TileID.LunarCraftingStation)
			.Register();
		}
    }
}
