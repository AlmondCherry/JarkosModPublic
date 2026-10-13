using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Jarkos.tiles;
using Jarkos.items.usetiles;

namespace jarkos.items.usetiles
{
	public class CleaningFurnaceItem : ModItem
	{
		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 1;
		}
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Orecleaner_tile>());
			Item.width = 30;
			Item.height = 32;
			Item.value = Item.buyPrice(silver: 50);
			Item.maxStack = Item.CommonMaxStack;
		}
		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<SteelBar>(), 5)
			.AddIngredient(ItemID.HellstoneBar, 10)
			.AddIngredient(ModContent.ItemType<GalvanicAlloy>(), 2)
			.AddIngredient(ItemID.WaterBucket, 1)
			.AddIngredient(ItemID.LavaBucket, 1)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
