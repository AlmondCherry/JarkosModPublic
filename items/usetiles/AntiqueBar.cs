using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using Jarkos.tiles.bars;
using jarkos.items.materials;

namespace jarkos.items.usetiles
{
	public class AntiqueBar : ModItem
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
			Item.rare = ModContent.RarityType<jarkos.rare.SpeedRarity>();
			Item.consumable = true;
			Item.useAnimation = 8;
			Item.useTime = 8;
			Item.useTurn = true;
			Item.value = Item.buyPrice(gold: 50);
			Item.createTile = ModContent.TileType<Antique_bar_tile>();
		}
			public override void AddRecipes()
			{
			CreateRecipe(20)
			.AddIngredient(ModContent.ItemType<AntiqueOre>(), 20)
			.AddIngredient(ModContent.ItemType<FragmentOfRage>(), 2)
			.AddIngredient(ModContent.ItemType<LunarCrystal>(), 1)
			.AddTile(TileID.LunarCraftingStation)
			.Register();
}
	}
}
