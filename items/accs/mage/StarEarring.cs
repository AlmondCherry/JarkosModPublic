using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.accs.mage
{
	public class StarEarring : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 22;
			Item.height = 32;
			Item.accessory = true;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetModPlayer<Jarkos_player>().sofs01equiped = true;
			var modPlayer = player.GetModPlayer<Jarkos_player>();

			if(!modPlayer.TaranisAccessoryEquipped)
			{
				player.GetDamage(DamageClass.Magic) += 0.02f;
				player.GetAttackSpeed(DamageClass.Magic) += 0.02f;
				player.manaRegen += 1;
			}
		}
        public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ItemID.FallenStar, 30)
			.AddIngredient(ItemID.Chain, 5)
			.AddRecipeGroup(nameof(ItemID.TissueSample), 10)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
