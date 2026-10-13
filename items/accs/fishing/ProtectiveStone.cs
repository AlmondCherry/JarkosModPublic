using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace jarkos.items.accs.fishing
{
	public class ProtectiveStone : ModItem
	{
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}
		public override void SetDefaults()
		{
			Item.width = 38;
			Item.height = 34;
			Item.accessory = true;
			Item.rare = ItemRarityID.Green;
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			float missing_hp_ratio = 1f - (player.statLife / (float)player.statLifeMax2);
			float final_def_bonus = 20f * missing_hp_ratio;
			player.statDefense += (int)final_def_bonus;
		}
        public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ItemID.Bone, 80)
			.AddRecipeGroup(nameof(ItemID.GoldBar), 16)
			.AddIngredient(ItemID.Diamond, 5)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
