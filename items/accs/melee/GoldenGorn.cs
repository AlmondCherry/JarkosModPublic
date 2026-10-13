using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.accs.melee
{
	public class GoldenGorn : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 40;
			Item.height = 40;
			Item.accessory = true;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 25);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			var modPl = player.GetModPlayer<Jarkos_player>();
			player.GetModPlayer<Jarkos_player>().goldengorn = true;

			player.GetAttackSpeed(DamageClass.Melee) += 0.05f;
			player.GetAttackSpeed(DamageClass.Melee) += 0.02f * modPl.goldengornstrength;
		}
        public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddRecipeGroup(nameof(ItemID.IronBar), 4)
			.AddRecipeGroup(nameof(ItemID.GoldBar), 8)
			.AddIngredient(ItemID.Ruby, 2)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
