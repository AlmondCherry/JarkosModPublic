using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.materials
{
	public class SmallBlackCrystal : ModItem
	{
		public override void SetStaticDefaults()
		{
			ItemID.Sets.ItemNoGravity[Item.type] = true;
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults()
		{
			Item.width = 24;
			Item.height = 32;
			Item.maxStack = Item.CommonMaxStack;
			Item.value = Item.buyPrice(silver: 1);
		}
	}
}
