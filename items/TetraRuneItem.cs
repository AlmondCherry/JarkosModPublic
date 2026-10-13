using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items
{
	public class TetraRuneItem : BaseRuneItem
	{
		public override string Texture => "jarkos/items/HighRuneItem";
		public override int MaxCapacity => 1000000;
		public override int MaxSlots => 16;

		public override void SafeSetDefaults()
		{
			Item.width = 32;
			Item.height = 32;
			Item.maxStack = 1;
			Item.value = Item.buyPrice(gold: 20);
			Item.rare = ItemRarityID.Cyan;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1)
				.AddIngredient(ModContent.ItemType<AlphaRuneItem>(), 1)
				.AddIngredient(ItemID.SpectreBar, 10)
				.AddIngredient(ItemID.LunarBar, 5)
				.AddTile(TileID.LunarCraftingStation)
				.Register();
		}
	}
}
