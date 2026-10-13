using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using Jarkos.tiles.bars;

namespace jarkos.items.usetiles
{
	public class FulgurBar : ModItem
	{
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 100;
			ItemID.Sets.SortingPriorityMaterials[Item.type] = 59;
		}
        public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 24;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.maxStack = Item.CommonMaxStack;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.consumable = true;
			Item.useTurn = true;
			Item.useAnimation = 8;
			Item.useTime = 8;
			Item.value = Item.buyPrice(silver: 30);
			Item.createTile = ModContent.TileType<Lazatep_bar_tile>();
		}
			public override void AddRecipes()
			{
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<FulgurOre>(), 6)
			.AddTile(TileID.Hellforge)
			.Register();
		}
	}
}
