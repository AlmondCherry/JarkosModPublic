using Terraria;
using Terraria.ModLoader;
using Jarkos.tiles;

namespace jarkos.items.usetiles
{
	public class CE_bigger_flower_placeable : ModItem
	{
		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(ModContent.TileType<Crimson_eden_bigger_flower>());
			Item.width = 40;
			Item.height = 32;
			Item.maxStack = Item.CommonMaxStack;
		}
	}
}
