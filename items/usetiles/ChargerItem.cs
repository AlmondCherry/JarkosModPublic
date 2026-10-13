using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Jarkos.tiles;
using jarkos.items.materials;

namespace jarkos.items.usetiles
{
	public class ChargerItem : ModItem
	{
		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(ModContent.TileType<Uranium_charger_tile>());
			Item.width = 40;
			Item.height = 32;
			Item.value = Item.buyPrice(silver: 25);
			Item.maxStack = Item.CommonMaxStack;
		}
		public override void AddRecipes() 
		{
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<FragmentOfFear>(), 10)
			.AddIngredient(ItemID.Glass, 50)
			.AddIngredient(ItemID.Wire, 200)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
