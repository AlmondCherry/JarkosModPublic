using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.tiles;

namespace jarkos.items.usetiles
{
	public class FulgurPedestalItem : ModItem
	{
		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(ModContent.TileType<Lazatep_statue>());
			Item.width = 52;
			Item.height = 34;
			Item.maxStack = Item.CommonMaxStack;
		}
		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<FulgurBar>(), 8)
			.AddIngredient(ItemID.StoneBlock, 90)
			.AddIngredient(ItemID.Sapphire, 8)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
