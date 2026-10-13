using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.accs.amulets
{
	public class MedallionOfMalice : ModItem {
		public override void SetDefaults()
		{
			Item.width = 24;
			Item.height = 28;
			Item.accessory = true;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 90);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			
			player.statDefense -= 5;
		}
		public override void AddRecipes() {
			CreateRecipe(1)
			.AddIngredient(ItemID.Wood, 20)
			.AddIngredient(ItemID.Gel, 12)
			.AddIngredient(ItemID.Waterleaf, 1)
			.AddIngredient(ItemID.Fireblossom, 1)
			.AddRecipeGroup(nameof(ItemID.GoldBar), 12)
			.AddRecipeGroup(nameof(ItemID.CrimsonSeeds), 4)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
