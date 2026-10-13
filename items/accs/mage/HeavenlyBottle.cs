using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.accs.mage
{
	public class HeavenlyBottle : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 32;
			Item.accessory = true;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			var modPl = player.GetModPlayer<Jarkos_player>();
			modPl.celestial_flask = true;
			player.GetDamage(DamageClass.Magic) += 0.08f;
			player.manaCost -= 0.15f;
		}
        public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<WoodenMageEmblem>(), 1)
			.AddRecipeGroup(nameof(ItemID.GoldBar), 15)
			.AddIngredient(ItemID.FallenStar, 5)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
