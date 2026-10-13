using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.items.potions;
using jarkos.items.materials;

namespace jarkos.items.boost
{
	public class Brokenpotion : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 42;
			Item.height = 40;
			Item.useTime = 32;
			Item.useAnimation = 32;
            Item.useStyle = ItemUseStyleID.HoldUp;
			Item.scale = 1f;
			Item.consumable = true;
            Item.maxStack = 1;
        }
		public override void AddRecipes()
		{
    		CreateRecipe()
        	.AddIngredient(ModContent.ItemType<Potion_of_s>(), 1)
			.AddIngredient(ModContent.ItemType<FragmentOfNature>(), 100)
			.AddIngredient(ModContent.ItemType<CrystalBlack>(), 10)
            .AddTile(TileID.Anvils)
        	.Register();
		}
	}
}
