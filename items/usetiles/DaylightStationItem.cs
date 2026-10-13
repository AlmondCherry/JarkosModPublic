using jarkos.tiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.usetiles
{
	public class DaylightStationItem : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 48;
			Item.height = 32;
			Item.maxStack = 99;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 15;
			Item.useAnimation = 15;
			Item.createTile = ModContent.TileType<DaylightStationTile>();
			Item.value = Item.sellPrice(0, 5, 0, 0);
			Item.rare = ItemRarityID.Orange;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ModContent.ItemType<DaylightGeneratorItem>(), 1)
				.AddIngredient(ItemID.SunplateBlock, 30)
				.AddIngredient(ItemID.GoldBar, 15)
				.AddIngredient(ItemID.FallenStar, 10)
				.AddTile(TileID.Anvils)
				.Register();

			CreateRecipe()
				.AddIngredient(ModContent.ItemType<DaylightGeneratorItem>(), 1)
				.AddIngredient(ItemID.SunplateBlock, 30)
				.AddIngredient(ItemID.PlatinumBar, 15)
				.AddIngredient(ItemID.FallenStar, 10)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
