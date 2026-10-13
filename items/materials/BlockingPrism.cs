using jarkos.items.materials;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Jarkos.items.materials
{
	public class BlockingPrism : ModItem
	{
		public override void SetStaticDefaults()
		{
			Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(4, 8));
			ItemID.Sets.AnimatesAsSoul[Item.type] = true;
		}
        public override void SetDefaults()
		{
            Item.width = 38;
            Item.height = 38;
			Item.maxStack = Item.CommonMaxStack;
			Item.material = true;
			Item.value = Item.buyPrice(gold: 3);
		}
		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<PulsatingFragment>(), 1)
			.AddIngredient(ModContent.ItemType<DiffusalBar>(), 1)
			.AddIngredient(ModContent.ItemType<LunarCrystal>(), 1)
			.AddTile(TileID.LunarCraftingStation)
			.Register();
		}
	}
}
