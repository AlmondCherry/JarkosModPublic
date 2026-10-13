using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using jarkos.items.usetiles;

namespace jarkos.items.materials
{
	public class Toxin : ModItem
	{
        public override void SetDefaults()
		{
            Item.width = 28;
            Item.height = 28;
			Item.maxStack = Item.CommonMaxStack;
			Item.material = true;
		}
        public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ItemType<GalvanicAlloy>(), 1)
			.Register();
		}
    }
}
