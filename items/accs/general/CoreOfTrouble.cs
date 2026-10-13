using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.items.materials;

namespace jarkos.items.accs.general
{
	public class CoreOfTrouble : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 36;
			Item.height = 30;
			Item.accessory = true;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			var Jarkos_player = player.GetModPlayer<Jarkos_player>();
			Jarkos_player.core_of_trouble = true;
		}
		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<FeatherBlack>(), 8)
			.AddIngredient(ItemID.Obsidian, 60)
			.AddIngredient(ModContent.ItemType<EnchantedFragment>(), 12)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
