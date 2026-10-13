using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.items.accs.amulets
{
	public class HollowManaCharm : ModItem {
		public override void SetDefaults()
		{
			Item.width = 24;
			Item.height = 28;
			Item.accessory = true;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 20);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.manaCost += 0.10f;
			player.manaRegen += player.manaRegen * 50 / 100;
		}
		public override void AddRecipes() {
			CreateRecipe(1)
			.AddIngredient(ItemID.ManaCrystal, 2)
			.AddRecipeGroup(nameof(ItemID.GoldBar), 8)
			.AddIngredient(ItemID.Marble, 4)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
