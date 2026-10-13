using Terraria;
using Terraria.ModLoader;
using Jarkos.tiles;

namespace jarkos.items.usetiles
{
	public class CE_bigger_stone_placeable : ModItem
	{
		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(ModContent.TileType<CE_bigger_stone>());
			Item.width = 40;
			Item.height = 32;
			Item.maxStack = Item.CommonMaxStack;
		}
	}
}
