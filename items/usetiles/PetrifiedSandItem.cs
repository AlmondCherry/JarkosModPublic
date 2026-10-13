using jarkos.tiles.CradleTiles;
using Jarkos.items.materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.usetiles
{
	public class PetrifiedSandItem : ModItem
	{
		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(ModContent.TileType<PetrifiedSandTile>());
			Item.width = 16;
			Item.height = 16;
		}
		public override void AddRecipes()
		{
			CreateRecipe(25)
				.AddIngredient(ItemID.SandBlock, 25)
				.AddIngredient(ModContent.ItemType<ResidualShadow>(), 1)
				.AddTile(TileID.HeavyWorkBench)
				.Register();
		}
	}
}
