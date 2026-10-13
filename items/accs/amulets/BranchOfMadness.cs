using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.accs.amulets
{
	public class BranchOfMadness : ModItem 
	{
		public override void SetDefaults()
		{
			Item.width = 24;
			Item.height = 28;
			Item.accessory = true;
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.sellPrice(silver: 85);
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetModPlayer<BranchMadnessPlayer>().amulet8spawnrate = true;
			player.GetCritChance(DamageClass.Generic) += 10;
		}

		public override void AddRecipes() {
			CreateRecipe(1)
			.AddIngredient(ItemID.Wood, 48)
			.AddIngredient(ItemID.HellstoneBar, 4)
			.AddRecipeGroup(nameof(ItemID.CrimtaneBar), 4)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
	}

	public class BranchMadnessPlayer : ModPlayer
	{
		public bool amulet8spawnrate = false;

        public override void ResetEffects()
        {
            amulet8spawnrate = false;
        }
	}
}
