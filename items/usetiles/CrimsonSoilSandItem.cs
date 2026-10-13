using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using jarkos.tiles;

namespace jarkos.items.usetiles
{
	public class CrimsonSoilSandItem : ModItem
	{
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 100;
			ItemID.Sets.SortingPriorityMaterials[Item.type] = 58;
		}
		public override void SetDefaults()
		{
			Item.width = 16;
			Item.height = 16;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.consumable = true;
			Item.DefaultToPlaceableTile(ModContent.TileType<Crimson_soil_sand>());
		}
	}
}
