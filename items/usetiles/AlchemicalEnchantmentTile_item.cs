using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Jarkos.tiles;

namespace jarkos.items.usetiles
{
	public class AlchemicalEnchantmentTile_item : ModItem
	{
		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(ModContent.TileType<AlchemicalEnchantmentTile>());
			Item.width = 40;
			Item.height = 32;
			Item.maxStack = Item.CommonMaxStack;
		}
		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ItemID.AlchemyTable, 1)
			.AddIngredient(ModContent.ItemType<NoxtiliumBar>(), 12)
			.AddIngredient(ItemID.Glass, 25)
			.AddRecipeGroup(nameof(ItemID.GoldBar), 10)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
