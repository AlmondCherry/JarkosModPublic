using jarkos.items.materials;
using jarkos.items.usetiles;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace Jarkos.items.materials
{
	public class ResidualShadow : ModItem
	{
		public override void SetStaticDefaults()
		{
			Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 4));
			ItemID.Sets.AnimatesAsSoul[Item.type] = true;
		}
        public override void SetDefaults()
		{
            Item.width = 32;
            Item.height = 36;
			Item.maxStack = Item.CommonMaxStack;
			Item.material = true;
			Item.value = Item.buyPrice(silver: 25);
		}
		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<SmallBlackCrystal>(), 1)
			.AddIngredient(ModContent.ItemType<NoxtiliumOre>(), 3)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
