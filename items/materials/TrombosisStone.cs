using jarkos.items.materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Jarkos.items.materials
{
	public class TrombosisStone : ModItem
	{
        public override void SetDefaults()
		{
            Item.width = 24;
            Item.height = 36;
			Item.maxStack = Item.CommonMaxStack;
			Item.material = true;
			Item.value = Item.buyPrice(silver: 5);
		}
		public override void AddRecipes() 
		{
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<BloodDroplet>(), 5)
			.AddIngredient(ItemID.StoneBlock, 20)
			.AddIngredient(ItemID.Obsidian, 2)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
