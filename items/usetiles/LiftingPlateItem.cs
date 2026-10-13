using jarkos.tiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.usetiles
{
	public class LiftingPlateItem : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 16;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 15;
			Item.useAnimation = 15;
			Item.maxStack = 99;
			Item.consumable = true;
			Item.createTile = ModContent.TileType<LiftingPlateTile>();
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(0, 0, 20, 0);
		}
	}
}
