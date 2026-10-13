using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.accs.persts
{
	public class MagicalEcho : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 56;
			Item.height = 56;
			Item.accessory = true;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			var modPlayer = player.GetModPlayer<Jarkos_player>();
			modPlayer.gemstaffs_buff = true;

			player.GetDamage(DamageClass.Magic) += 0.12f;
			player.GetAttackSpeed(DamageClass.Magic) += 0.12f;

			player.manaRegen += 8;
			player.statManaMax2 += 100;
		}
        public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<AmethystPerst>(), 1)
			.AddIngredient(ModContent.ItemType<TobazPerst>(), 1)
			.AddIngredient(ModContent.ItemType<SapphirePerst>(), 1)
			.AddIngredient(ModContent.ItemType<EmeraldPerst>(), 1)
			.AddIngredient(ModContent.ItemType<RubyPerst>(), 1)
			.AddIngredient(ModContent.ItemType<DiamondPerst>(), 1)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
