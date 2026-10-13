using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.materials
{
	public class CrystalBlack : ModItem
	{
		public override void SetStaticDefaults()
		{
			ItemID.Sets.ItemNoGravity[Item.type] = true;
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults()
		{
			Item.width = 22;
			Item.height = 18;
			Item.maxStack = Item.CommonMaxStack;
			Item.value = Item.buyPrice(silver: 20);
		}
		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<SmallBlackCrystal>(), 10)
			.AddIngredient(ModContent.ItemType<FragmentOfFear>(), 2)
            .AddTile(TileID.Anvils)
			.Register();
		}
	}
}
