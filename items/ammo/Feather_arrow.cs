using jarkos.items.materials;
using jarkos.projectiles.bullet;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.ammo
{
	public class Feather_arrow : ModItem
	{
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;
		}
		public override void SetDefaults()
		{
			Item.width = 14;
			Item.height = 37;
			Item.damage = 12;
			Item.DamageType = DamageClass.Ranged;
			Item.maxStack = Item.CommonMaxStack;
			Item.consumable = true;
			Item.knockBack = 1f;
			Item.shoot = ModContent.ProjectileType<Feather_arrow_proj>();
            Item.ammo = AmmoID.Arrow;
		}
		public override void AddRecipes()
		{
			CreateRecipe(50)
				.AddIngredient(ModContent.ItemType<FeatherBlack>(), 1)
				.AddIngredient(ItemID.WoodenArrow, 50)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
