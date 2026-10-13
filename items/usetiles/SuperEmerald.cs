using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.tiles;
using jarkos.items.materials;
using jarkos.rare;

namespace jarkos.items.usetiles
{
	public class SuperEmerald : ModItem
	{
		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(ModContent.TileType<superemerald>());
			Item.width = 40;
			Item.height = 32;
			Item.maxStack = Item.CommonMaxStack;
			Item.rare = ModContent.RarityType<SpeedRarity>();
		}
		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<AntiqueBar>(), 8)
			.AddIngredient(ModContent.ItemType<AncientFragment>(), 4)
			.AddIngredient(ModContent.ItemType<NitoziumPlate>(), 2)
			.AddIngredient(ItemID.StoneBlock, 750)
			.AddTile(TileID.LunarCraftingStation)
			.Register();
		}
	}
}
