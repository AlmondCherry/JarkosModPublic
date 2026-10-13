using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using jarkos.items.usetiles;

namespace jarkos.items.tools
{
	public class FulgurPickaxe : ModItem
	{
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}
		public override void SetDefaults()
		{
			Item.damage = 11;
			Item.DamageType = DamageClass.Melee;
			Item.width = 38;
			Item.height = 38;
			Item.useTime = 8;
			Item.useAnimation = 16;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 5;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;
			Item.pick = 59;
		}
		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ModContent.ItemType<FulgurBar>(), 8)
				.AddRecipeGroup(nameof(ItemID.GoldPickaxe), 1)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
