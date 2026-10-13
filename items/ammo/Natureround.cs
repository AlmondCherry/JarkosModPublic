using jarkos.items.materials;
using jarkos.projectiles.bullet;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.ammo
{
	public class Natureround : ModItem
	{
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;
		}
		public override void SetDefaults()
		{
			Item.width = 16;
			Item.height = 44;
			Item.damage = 6;
			Item.DamageType = DamageClass.Ranged;
			Item.maxStack = Item.CommonMaxStack;
			Item.consumable = true;
			Item.knockBack = 1f;
			Item.shoot = ModContent.ProjectileType<Nature_round>();
            Item.ammo = AmmoID.Bullet;
			Item.shootSpeed = 8f;
		}
		public override void AddRecipes()
		{
			CreateRecipe(100)
				.AddIngredient(ModContent.ItemType<FragmentOfNature>(), 1)
				.AddIngredient(ItemID.MusketBall, 100)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
