using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Jarkos.tiles.bars;
using Terraria.GameContent.Creative;
using jarkos.items.materials;

namespace jarkos.items.usetiles
{
	public class GalvanicAlloy : ModItem
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
			Item.createTile = ModContent.TileType<Uranium_bar_tile>();
		}
		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<FulgurBar>(), 1)
			.AddIngredient(ModContent.ItemType<PulsatingFragment>(), 2)
			.AddIngredient(ItemID.Obsidian, 3)
			.AddRecipeGroup(nameof(ItemID.Ichor), 2)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
}
