using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace jarkos.items.accs.fishing
{
	public class Fisher_emblem_first_tier : ModItem
	{
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}
		public override void SetDefaults()
		{
			Item.width = 34;
			Item.height = 30;
			Item.accessory = true;
			Item.rare = ItemRarityID.White;
			Item.value = Item.sellPrice(copper: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.fishingSkill += 5;
			player.luck += 0.05f;
		}
        public override void AddRecipes() {
			CreateRecipe(1)
			.AddIngredient(ItemID.Wood, 40)
			.AddRecipeGroup(nameof(ItemID.CopperBar), 2)
			.AddIngredient(ItemID.Chain, 1)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
	}
}
