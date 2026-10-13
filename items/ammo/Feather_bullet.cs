using jarkos.items.materials;
using jarkos.projectiles.bullet;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.ammo
{
	public class Feather_bullet : ModItem
	{
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;
		}
		public override void SetDefaults()
		{
			Item.width = 16;
			Item.height = 44;
			Item.damage = 4;
			Item.DamageType = DamageClass.Ranged;
			Item.maxStack = Item.CommonMaxStack;
			Item.consumable = true;
			Item.knockBack = 1f;
			Item.shoot = ModContent.ProjectileType<Feather_bullet_proj>();
			Item.shootSpeed = 5f;
            Item.ammo = AmmoID.Bullet;
		}
		public override void AddRecipes()
		{
			CreateRecipe(100)
				.AddIngredient(ModContent.ItemType<FeatherBlack>(), 1)
				.AddIngredient(ItemID.MusketBall, 100)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
