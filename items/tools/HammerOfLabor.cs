using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using jarkos.items.usetiles;

namespace jarkos.items.tools
{
	public class HammerOfLabor : ModItem
	{
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}
		public override void SetDefaults()
		{
			Item.damage = 26;
			Item.DamageType = DamageClass.Melee;
			Item.width = 60;
			Item.height = 62;
			Item.useTime = 2;
			Item.useAnimation = 46;
			Item.scale = 1f;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 2;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;
			Item.hammer = 85;
			Item.pick = 105;
		}
		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ModContent.ItemType<IgnisiumBar>(), 6)
				.AddIngredient(ModContent.ItemType<FulgurBar>(), 6)
				.AddRecipeGroup(nameof(ItemID.GoldBar), 10)
				.AddIngredient(ItemID.WoodenHammer, 1)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
