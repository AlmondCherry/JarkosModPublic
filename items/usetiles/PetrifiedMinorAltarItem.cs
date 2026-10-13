using jarkos.tiles.CradleTiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.usetiles
{
	public class PetrifiedMinorAltarItem : ModItem
	{
		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(ModContent.TileType<PetrifiedMinorAltarTile>());
			Item.width = 16;
			Item.height = 28;
			Item.value = Item.sellPrice(silver: 20);
			Item.rare = ItemRarityID.Blue;
		}
		public override void AddRecipes()
		{
			CreateRecipe(2)
				.AddIngredient<PetrifiedDepthstoneItem>(10)
				.AddIngredient(ItemID.Obsidian, 5)
				.AddTile(TileID.HeavyWorkBench)
				.Register();
		}
	}
}
