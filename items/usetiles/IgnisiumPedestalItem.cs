using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.tiles;
namespace jarkos.items.usetiles
{
	public class IgnisiumPedestalItem : ModItem
	{
		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(ModContent.TileType<Auriteh_statue>());
			Item.width = 52;
			Item.height = 34;
			Item.maxStack = Item.CommonMaxStack;
			Item.value = Item.buyPrice(silver: 30);
		}
		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<IgnisiumBar>(), 8)
			.AddIngredient(ItemID.StoneBlock, 90)
			.AddIngredient(ItemID.Emerald, 8)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
