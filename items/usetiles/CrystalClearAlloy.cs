using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using Terraria;
using Terraria.DataStructures;
using jarkos.items.materials;

namespace jarkos.items.usetiles
{
	public class CrystalClearAlloy : ModItem
	{
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 100;
			ItemID.Sets.SortingPriorityMaterials[Item.type] = 59;
			Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(8, 5));
			ItemID.Sets.AnimatesAsSoul[Item.type] = true;
		}
		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 24;
			Item.maxStack = Item.CommonMaxStack;
			Item.value = Item.buyPrice(gold: 1);
		}
		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<CrystalBlack>(), 5)
			.AddIngredient(ModContent.ItemType<NoxtiliumBar>(), 5)
			.AddIngredient(ItemID.LunarBar, 5)
			.AddTile(TileID.Hellforge)
			.Register();
		}
	}
}
