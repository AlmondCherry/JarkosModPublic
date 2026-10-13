using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.tiles;
using Terraria.GameContent.Creative;

namespace jarkos.items.usetiles
{
	public class IgnisiumOre : ModItem
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
			Item.value = Item.buyPrice(silver: 5);
			Item.DefaultToPlaceableTile(ModContent.TileType<AuritehOre>());
		}
	}
}
