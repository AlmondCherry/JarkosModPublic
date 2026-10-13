using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using jarkos.tiles;
using jarkos.rare;

namespace jarkos.items.usetiles
{
	public class AntiqueOre : ModItem
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
			Item.rare = ModContent.RarityType<SpeedRarity>();
			Item.autoReuse = true;
			Item.consumable = true;
			Item.value = Item.buyPrice(gold: 3);
			Item.DefaultToPlaceableTile(ModContent.TileType<AnticvarOre>());
		}
	}
}
