using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items
{
	public class AlphaRuneItem : BaseRuneItem
	{
		public override string Texture => "jarkos/items/HighRuneItem";
		public override int MaxCapacity => 100000;
		public override int MaxSlots => 8;

		public override void SafeSetDefaults()
		{
			Item.width = 32;
			Item.height = 32;
			Item.maxStack = 1;
			Item.value = Item.buyPrice(gold: 5);
			Item.rare = ItemRarityID.Pink;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1)
				.AddIngredient(ModContent.ItemType<HighRuneItem>(), 1)
				.AddIngredient(ItemID.ChlorophyteBar, 10)
				.AddIngredient(ItemID.SoulofMight, 5)
				.AddTile(TileID.MythrilAnvil)
				.Register();
		}
	}
}
