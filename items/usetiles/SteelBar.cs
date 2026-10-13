using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using Jarkos.tiles.bars;

namespace Jarkos.items.usetiles
{
	public class SteelBar : ModItem
	{
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 100;
			ItemID.Sets.SortingPriorityMaterials[Item.type] = 59;
		}
        public override void SetDefaults()
		{
			Item.maxStack = 9999;
			Item.width = 30;
			Item.height = 24;
			Item.autoReuse = true;
			Item.consumable = true;
			Item.useAnimation = 8;
			Item.useTime = 8;
			Item.placeStyle = 0;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTurn = true;
			Item.value = Item.buyPrice(silver: 3);
			Item.createTile = ModContent.TileType<Steel_bar_tile>();
		}
		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddRecipeGroup(nameof(ItemID.IronBar), 2)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
