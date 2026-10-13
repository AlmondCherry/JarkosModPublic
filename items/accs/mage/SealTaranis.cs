using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using jarkos.items.materials;

namespace jarkos.items.accs.mage
{
	public class SealTaranis : ModItem
	{
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}
		public override void SetDefaults()
		{
			Item.width = 36;
			Item.height = 36;
			Item.accessory = true;
			Item.defense = 20;
			Item.rare = ItemRarityID.Red;
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			var modPlayer = player.GetModPlayer<Jarkos_player>();
			modPlayer.seal_taranis_equiped = true;
		}
        public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<LunarCrystal>(), 30)
			.AddIngredient(ItemID.LunarBar, 40)
			.AddIngredient(ItemID.NebulaMonolith, 1)
			.AddIngredient(ModContent.ItemType<ElementalFluid>(), 30)
			.AddTile(TileID.LunarCraftingStation)
			.Register();
		}
	}
}
