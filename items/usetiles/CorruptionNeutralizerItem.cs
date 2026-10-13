using jarkos.tiles.CradleTiles;
using Terraria;
using Terraria.Enums;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;

namespace jarkos.items.usetiles
{
	public class CorruptionNeutralizerItem : ModItem
	{
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}
		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(ModContent.TileType<CorruptionNeutralizerTile>());
			Item.SetShopValues(ItemRarityColor.Pink5, Item.buyPrice(gold: 5));
			Item.width = 22;
			Item.height = 30;
			Item.value = Item.buyPrice(silver: 50);
		}
	}
}
