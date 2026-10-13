using jarkos.items.usetiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Jarkos.items.materials
{
	public class DiffusalBar : ModItem
	{
        public override void SetDefaults()
		{
            Item.width = 24;
            Item.height = 36;
			Item.maxStack = Item.CommonMaxStack;
			Item.material = true;
			Item.value = Item.buyPrice(silver: 35);
		}
		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<FulgurBar>(), 1)
			.AddIngredient(ModContent.ItemType<IgnisiumBar>(), 1)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
