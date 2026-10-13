using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.materials
{
	public class ReinforcedShell : ModItem
	{
        public override void SetDefaults()
		{
            Item.width = 32;
            Item.height = 32;
			Item.maxStack = Item.CommonMaxStack;
			Item.material = true;
			Item.value = Item.buyPrice(gold: 1);
		}
        public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<ChitinBeetle>(), 1)
			.AddIngredient(ItemID.BeetleHusk, 2)
            .AddTile(TileID.MythrilAnvil)
			.Register();
		}
    }
}
