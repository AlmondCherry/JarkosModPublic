using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.accs.mage
{
	public class EyeOfMage : ModItem
	{
        public override void SetDefaults()
		{
			Item.width = 50;
			Item.height = 34;
			Item.accessory = true;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			var Jarkos_player = player.GetModPlayer<Jarkos_player>();
			if (player.statMana < player.statManaMax2 / 2)
			{
				Jarkos_player.eom_equiped = true;
			}

			float unrestored_mana = player.statManaMax2 - player.statMana;
			float damage_multiplier = 0.005f * unrestored_mana;

			if (damage_multiplier > 0.35f)
			{
				damage_multiplier = 0.35f;
			}
			
			player.GetDamage(DamageClass.Magic) += damage_multiplier;
		}
        public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ItemID.LesserManaPotion, 50)
			.AddIngredient(ItemID.Amethyst, 15)
			.AddIngredient(ItemID.Glass, 40)
			.AddIngredient(ItemID.FallenStar, 12)
			.AddRecipeGroup(nameof(ItemID.CrimtaneBar), 20)
			.AddTile(TileID.TinkerersWorkbench)
			.Register();
		}
	}
}
