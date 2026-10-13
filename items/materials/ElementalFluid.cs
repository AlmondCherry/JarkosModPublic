using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.materials
{
	public class ElementalFluid : ModItem
	{
        public override void SetDefaults()
		{
            Item.width = 38;
            Item.height = 32;
			Item.maxStack = Item.CommonMaxStack;
			Item.material = true;
		}
        public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ItemID.SummoningPotion, 1)
			.AddIngredient(ItemID.WrathPotion, 1)
			.AddIngredient(ItemID.RagePotion, 1)
			.AddIngredient(ItemID.LifeforcePotion, 1)
			.AddIngredient(ItemID.EndurancePotion, 1)
			.AddIngredient(ItemID.SwiftnessPotion, 1)
			.AddIngredient(ItemID.RegenerationPotion, 1)
			.AddIngredient(ItemID.IronskinPotion, 1)
			.AddIngredient(ItemID.LunarBar, 1)
            .AddTile(TileID.LunarCraftingStation)
			.Register();
		}
    }
}
