using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.tiles;

namespace jarkos.items.usetiles
{
	public class ArcaneForgeItem : ModItem
	{
		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(ModContent.TileType<Arcane_forge>());
			Item.width = 52;
			Item.height = 34;
			Item.maxStack = Item.CommonMaxStack;
			Item.value = Item.buyPrice(gold: 20);
		}
		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ItemID.LunarBar, 12)
			.AddRecipeGroup(nameof(ItemID.MythrilBar), 16)
			.AddIngredient(ItemID.FragmentStardust, 10)
			.AddTile(TileID.LunarCraftingStation)
			.Register();
		}
	}
}
