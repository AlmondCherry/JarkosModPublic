using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.accs.mage
{
	public class StarAmpoule : ModItem
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
			player.GetDamage(DamageClass.Magic) += 0.06f;
			player.manaCost -= 0.10f;
		}
        public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ItemID.NaturesGift, 1)
			.AddIngredient(ItemID.FallenStar, 5)
			.AddIngredient(ItemID.Bottle, 1)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
