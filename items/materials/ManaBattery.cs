using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.items.usetiles;

namespace jarkos.items.materials
{
	public class ManaBattery : ModItem
	{
        public override void SetDefaults()
		{
            Item.width = 32;
            Item.height = 13;
			Item.maxStack = Item.CommonMaxStack;
			Item.material = true;
			Item.value = Item.buyPrice(silver: 5);
		}
        public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ItemID.ManaCrystal, 1)
			.AddIngredient(ModContent.ItemType<FulgurBar>(), 4)
			.AddIngredient(ModContent.ItemType<FragmentOfFear>(), 2)
            .AddTile(TileID.Anvils)
			.Register();
		}
    }
}
