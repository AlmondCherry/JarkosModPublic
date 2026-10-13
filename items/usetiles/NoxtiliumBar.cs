using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using Terraria;
using Jarkos.tiles.bars;
using jarkos.items.materials;

namespace jarkos.items.usetiles
{
	public class NoxtiliumBar : ModItem
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
			Item.autoReuse = true;
			Item.consumable = true;
			Item.useAnimation = 8;
			Item.useTime = 8;
			Item.useTurn = true;
			Item.value = Item.buyPrice(silver: 50);
			Item.createTile = ModContent.TileType<Etherionite_bar_tile>();
		}
		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<NoxtiliumOre>(), 3)
			.AddIngredient(ModContent.ItemType<FragmentOfFear>(), 1)
			.AddTile(TileID.Hellforge)
			.Register();
		}
	}
}
