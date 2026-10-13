using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.usable
{
	public class RuneEnchantmentScroll : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 28;
			Item.height = 28;
			Item.maxStack = 99;
			Item.value = Item.sellPrice(0, 2, 0, 0);
			Item.rare = ItemRarityID.LightRed;
		}
	}
}
