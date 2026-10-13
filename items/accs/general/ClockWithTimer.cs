using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.items.materials;

namespace jarkos.items.accs.general
{
	public class ClockWithTimer : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 28;
			Item.height = 24;
			Item.accessory = true;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			var modpl = player.GetModPlayer<Jarkos_player>();
			modpl.clockwithtimer = true;
			player.statLifeMax2 += 100;
			player.statManaMax2 += 50;
		}
		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddRecipeGroup(nameof(ItemID.GoldBar), 8)
			.AddIngredient(ModContent.ItemType<PulsatingFragment>(), 2)
			.AddIngredient(ItemID.Obsidian, 25)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
