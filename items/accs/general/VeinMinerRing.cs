using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.systems;
using jarkos.GameplayChanges;

namespace jarkos.items.accs.general
{
	public class VeinMinerRing : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 28;
			Item.height = 28;
			Item.accessory = true;
			Item.rare = ItemRarityID.Green;
			Item.value = Item.sellPrice(gold: 1, silver: 50);
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			var veinPlayer = player.GetModPlayer<VeinMinerPlayer>();
			if (veinPlayer != null)
			{
				veinPlayer.veinMinerEquipped = true;
			}
		}

		public override void AddRecipes()
		{
			CreateRecipe(1)
				.AddRecipeGroup(nameof(ItemID.GoldBar), 8)
				.AddIngredient(ItemID.Obsidian, 5)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
