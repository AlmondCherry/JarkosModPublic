using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.accs.summon
{
	public class Branch : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 42;
			Item.height = 26;
			Item.accessory = true;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 5);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetDamage(DamageClass.Summon) += 0.04f;
			player.maxMinions += 1;
		}
        public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddRecipeGroup(RecipeGroupID.Wood, 8)
			.AddIngredient(ItemID.Daybloom, 1)
			.AddIngredient(ItemID.Gel, 24)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
	}
}
