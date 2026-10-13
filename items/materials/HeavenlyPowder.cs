using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.materials
{
	public class HeavenlyPowder : ModItem
	{
		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 32;
			Item.maxStack = Item.CommonMaxStack;
			Item.value = Item.buyPrice(gold: 5);
		}
		public override void PostUpdate()
        {
            Lighting.AddLight(Item.Center, 1f, 0.9f, 0.4f);
        }

		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<ElementalPowder>(), 1)
			.AddIngredient(ItemID.FragmentStardust, 2)
            .AddTile(TileID.LunarCraftingStation)
			.Register();
		}
	}
}
