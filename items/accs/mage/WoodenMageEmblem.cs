using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.accs.mage
{
	public class WoodenMageEmblem : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 32;
			Item.accessory = true;
			Item.rare = ItemRarityID.White;
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetDamage(DamageClass.Magic) += 0.05f;
		}
        public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ItemID.Wood, 32)
			.AddIngredient(ItemID.StoneBlock, 24)
			.AddRecipeGroup(nameof(ItemID.IronBar), 2)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
