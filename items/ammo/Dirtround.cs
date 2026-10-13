using jarkos.projectiles.bullet;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.ammo
{
	public class Dirtround : ModItem
	{
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;
		}
		public override void SetDefaults()
		{
			Item.width = 18;
			Item.height = 40;
			Item.damage = 4;
			Item.DamageType = DamageClass.Ranged;
			Item.maxStack = Item.CommonMaxStack;
			Item.consumable = true;
			Item.knockBack = 1f;
			Item.shoot = ModContent.ProjectileType<Dirtammo>();
			Item.shootSpeed = 5f;
            Item.ammo = AmmoID.Bullet;
		}
		public override void AddRecipes()
		{
			CreateRecipe(25)
				.AddIngredient(ItemID.DirtBlock, 4)
				.AddIngredient(ItemID.StoneBlock, 2)
				.AddTile(TileID.Dirt)
				.Register();
		}
	}
}
