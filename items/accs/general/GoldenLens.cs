using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.accs.general
{
	public class GoldenLens : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 28;
			Item.height = 24;
			Item.accessory = true;
			Item.rare = ItemRarityID.White;
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetDamage(DamageClass.Generic) += 0.05f;
			player.statLifeMax2 += 50;
			player.statManaMax2 += 50;
		}
		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddRecipeGroup(nameof(ItemID.GoldBar), 6)
			.AddIngredient(ItemID.Glass, 20)
			.AddIngredient(ItemID.Obsidian, 2)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
