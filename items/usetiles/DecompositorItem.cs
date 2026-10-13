using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.tiles;
using Jarkos.items.usetiles;

namespace jarkos.items.usetiles
{
	public class DecompositorItem : ModItem
	{
		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(ModContent.TileType<Decompositor>());
			Item.width = 78;
			Item.height = 64;
			Item.maxStack = 1;
			Item.value = Item.buyPrice(silver: 50);
		}
		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ItemID.Bottle, 4)
			.AddIngredient(ItemID.Obsidian, 50)
			.AddIngredient(ModContent.ItemType<SteelBar>(), 5)
			.AddIngredient(ItemID.Ruby, 3)
			.AddIngredient(ItemID.Wire, 150)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
