using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using jarkos.items.usetiles;

namespace jarkos.items.tools
{
	public class NoxtiliumPickaxe : ModItem
	{
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}
		public override void SetDefaults()
		{
			Item.damage = 23;
			Item.DamageType = DamageClass.Melee;
			Item.width = 38;
			Item.height = 38;
			Item.useTime = 8;
			Item.useAnimation = 16;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 4;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;
			Item.pick = 195;
		}
		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ModContent.ItemType<NoxtiliumBar>(), 6)
				.AddIngredient(ItemID.Amethyst, 8)
				.AddRecipeGroup(nameof(ItemID.MythrilBar), 16)
				.AddTile(TileID.MythrilAnvil)
				.Register();
		}
	}
}
