using jarkos.items.materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.accs.summon
{
	public class CelestialShell : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 34;
			Item.accessory = true;
			Item.rare = ItemRarityID.Red;
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			var modPlayer = player.GetModPlayer<Jarkos_player>();
			modPlayer.nature_dar = true;
			player.GetDamage(DamageClass.Summon) += 0.12f;
			player.maxMinions += 2;
		}
		public override bool CanEquipAccessory(Player player, int slot, bool modded)
		{
			var jarkosplayer = player.GetModPlayer<Jarkos_player>();
			if (!jarkosplayer.Radiant_Generator && !jarkosplayer.Evil_Gods_Parchment_equip)
			{
				return true;
			}
			return false;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<RadiantGenerator>(), 1)
			.AddIngredient(ModContent.ItemType<EvilGodsParchment>(), 1)
			.AddIngredient(ModContent.ItemType<GalaxyPearl>(), 1)
			.AddIngredient(ModContent.ItemType<SpookynightSeal>(), 1)
			.AddIngredient(ModContent.ItemType<BulbSunEnergy>(), 2)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
}
