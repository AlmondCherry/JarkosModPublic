using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.materials
{
	public class Nitozium : ModItem
	{
        public override void SetDefaults()
		{
            Item.width = 24;
            Item.height = 20;
			Item.maxStack = Item.CommonMaxStack;
			Item.material = true;
			Item.value = Item.buyPrice(gold: 5);
		}
		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<TenderoteMechanism>(), 15)
            .AddIngredient(ItemID.LavaBucket, 1)
            .AddTile(TileID.LunarCraftingStation)
			.Register();
		}
    }
}
