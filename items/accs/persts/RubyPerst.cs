using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.accs.persts
{
	public class RubyPerst : ModItem
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
			modPl.ruby_shard = true;

			player.GetDamage(DamageClass.Magic) += 0.10f;
			player.statManaMax2 += 60;
		}
        public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ItemID.Ruby, 6)
			.AddIngredient(ItemID.FallenStar, 2)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
