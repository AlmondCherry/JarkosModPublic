using jarkos.tiles.CradleTiles;
using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;

namespace jarkos.items.usetiles
{
	public class PetrifiedAbyssPylonItem : ModItem
	{
		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(ModContent.TileType<PetrifiedAbyssPylonTile>());
			Item.SetShopValues(ItemRarityColor.Blue1, Item.buyPrice(gold: 10));
		}
	}
}
