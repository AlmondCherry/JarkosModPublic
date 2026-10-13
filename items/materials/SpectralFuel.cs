using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.materials
{
	public class SpectralFuel : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 20;
			Item.height = 20;
			Item.maxStack = 999;
			Item.value = Item.sellPrice(0, 2, 0, 0);
			Item.rare = ItemRarityID.LightRed;
		}

		public override void AddRecipes()
		{
			CreateRecipe(5)
				.AddIngredient(ItemID.Ectoplasm, 2)
				.AddIngredient(ItemID.FallenStar, 2)
				.AddTile(TileID.MythrilAnvil)
				.Register();

			CreateRecipe(5)
				.AddIngredient(ModContent.ItemType<CoolingFuel>(), 5)
				.AddIngredient(ItemID.SoulofLight, 2)
				.AddTile(TileID.MythrilAnvil)
				.Register();
		}
	}
}
