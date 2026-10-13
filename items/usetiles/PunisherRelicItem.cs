using Jarkos.tiles.Relics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Jarkos.items.usetiles
{
	public class PunisherRelicItem : ModItem
	{
		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(ModContent.TileType<PunisherRelic>(), 0);
			Item.width = 30;
			Item.height = 40;
			Item.rare = ItemRarityID.Master;
			Item.master = true;
			Item.value = Item.buyPrice(0, 5);
		}
	}
}
