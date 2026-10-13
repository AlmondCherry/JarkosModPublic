using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using jarkos.projectiles.ranged.firearm;
using Terraria.DataStructures;
namespace jarkos.items.weapons.ranged.firing
{
	public class Sniper_horseman : ModItem
	{
		public override void SetDefaults()
		{
			Item.useTime = 42;
			Item.DamageType = DamageClass.Ranged;
            Item.width = 114;
            Item.height = 32;
            Item.useStyle = ItemUseStyleID.Shoot;
			Item.useAnimation = 42;
            Item.damage = 189;
			Item.knockBack = 2f;
            Item.autoReuse = true;
            Item.noMelee = true;
			Item.scale = 1f;
			Item.UseSound = null;
            Item.shoot = ModContent.ProjectileType<Sniper_horseman_Holdout>();
			Item.shootSpeed = 18f;
            Item.useAmmo = AmmoID.Bullet;
            Item.channel = true;
            Item.noUseGraphic = true;

			Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 1, 0, 0);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<Sniper_horseman_Holdout>(), damage, knockback, player.whoAmI, type);
            return false;
        }
		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ItemID.SoulofMight, 8)
			.AddIngredient(ItemID.HallowedBar, 10)
			.AddIngredient(ItemID.Musket, 1)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
}
