using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.accs.persts
{
	public class EmeraldPerst : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 28;
			Item.height = 28;
			Item.accessory = true;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			var modPl = player.GetModPlayer<Jarkos_player>();
			modPl.emerald_shard = true;
			player.GetAttackSpeed(DamageClass.Magic) += 0.07f;
			player.manaRegen += 4;
		}
        public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ItemID.Emerald, 6)
			.AddIngredient(ItemID.FallenStar, 2)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
