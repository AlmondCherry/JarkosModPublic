using jarkos.tiles;
using jarkos.tiles.CradleTiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.usetiles
{
	public class PetrifiedAltarItem : ModItem
	{
		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(ModContent.TileType<PetrifiedAltarTile>());
			Item.width = 32;
			Item.height = 28;
			Item.value = Item.sellPrice(copper: 50);
			Item.rare = ItemRarityID.Blue;
		}
		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient<PetrifiedDepthstoneItem>(50)
				.AddIngredient(ItemID.Obsidian, 10)
				.AddIngredient(ItemID.Diamond, 3)
				.AddTile(TileID.HeavyWorkBench)
				.Register();
		}
	}
}
