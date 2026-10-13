using jarkos.tiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.usetiles
{
	public class DaylightGeneratorItem : ModItem
	{
		public override string Texture => "jarkos/items/materials/GoldenCore";

		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 32;
			Item.maxStack = 99;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 15;
			Item.useAnimation = 15;
			Item.createTile = ModContent.TileType<DaylightGeneratorTile>();
			Item.value = Item.sellPrice(0, 1, 0, 0);
			Item.rare = ItemRarityID.Green;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.SunplateBlock, 10)
				.AddIngredient(ItemID.FallenStar, 5)
				.AddIngredient(ItemID.IronBar, 5)
				.AddTile(TileID.Anvils)
				.Register();

			CreateRecipe()
				.AddIngredient(ItemID.Glass, 10)
				.AddIngredient(ItemID.FallenStar, 5)
				.AddIngredient(ItemID.LeadBar, 5)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
