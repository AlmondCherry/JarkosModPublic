using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;

namespace Jarkos.items.materials
{
	public class CosmicFlame : ModItem
	{
		public override void SetStaticDefaults()
		{
			Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(4, 4));
			ItemID.Sets.AnimatesAsSoul[Item.type] = true;
		}
        public override void SetDefaults()
		{
            Item.width = 32;
            Item.height = 90;
			Item.maxStack = Item.CommonMaxStack;
			Item.material = true;
			Item.value = Item.buyPrice(gold: 8);
		}
		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ItemID.LunarBar, 12)
			.AddIngredient(ItemID.FallenStar, 20)
			.AddIngredient(ItemID.Ectoplasm, 4)
            .AddTile(TileID.LunarCraftingStation)
			.Register();
		}
	}
}
