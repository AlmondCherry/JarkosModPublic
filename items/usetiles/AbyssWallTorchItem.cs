using jarkos.tiles.CradleTiles;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.usetiles
{
	public class AbyssWallTorchItem : ModItem
	{
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;
		}
		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(ModContent.TileType<AbyssWallTorchTile>());
			Item.width = 12;
			Item.height = 16;
			Item.maxStack = Item.CommonMaxStack;
			Item.value = Item.sellPrice(copper: 50);
			Item.holdStyle = ItemHoldStyleID.HoldFront;
		}
		public override void HoldItem(Player player)
		{
			Lighting.AddLight(player.Center, 0.55f, 0.20f, 0.40f);
		}
		public override void AddRecipes()
		{
			CreateRecipe(3)
				.AddIngredient(ItemID.Torch, 3)
				.Register();
		}
	}
}
