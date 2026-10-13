using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.items.materials;
namespace jarkos.items.accs.emblems
{
	public class EmblemOfMisery : ModItem 
	{
		public override void SetDefaults()
		{
			Item.width = 28;
			Item.height = 28;
			Item.accessory = true;
			Item.rare = ItemRarityID.Red;
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.endurance -= 1.50f;
			player.GetDamage(DamageClass.Generic) += 1.00f;
			player.statDefense -= player.statDefense / 2;
			player.lifeRegen -= player.lifeRegen / 2;
		}
        public override void AddRecipes() 
		{
			CreateRecipe(1)
			.AddIngredient(ItemID.AvengerEmblem, 1)
			.AddIngredient(ModContent.ItemType<NitoziumPlate>(), 1)
			.AddIngredient(ModContent.ItemType<ElementalFluid>(), 25)
			.AddIngredient(ModContent.ItemType<FragmentOfFear>(), 15)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
}
