using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.projectiles.mecha;
using jarkos.projectiles.ranged.firearm;
using jarkos.projectiles.bullet;
namespace jarkos.items.weapons.ranged.firing
{
	public class Jupiter_storm : ModItem
	{
		public override void SetDefaults()
		{
			Item.useTime = 4;
			Item.DamageType = DamageClass.Ranged;
            Item.width = 180;
            Item.height = 68;
            Item.useStyle = ItemUseStyleID.Shoot;
			Item.useAnimation = 4;
            Item.damage = 54;
            Item.autoReuse = true;
            Item.noMelee = true;
			Item.scale = 1f;
			Item.UseSound = null;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<Jupiter_storm_Holdout>();
			Item.shootSpeed = 26f;
            Item.useAmmo = AmmoID.Bullet;
            Item.channel = true;
            Item.noUseGraphic = true;

			Item.rare = ItemRarityID.Lime;
            Item.value = Item.sellPrice(0, 5, 0, 0);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<Jupiter_storm_Holdout>(), damage, knockback, player.whoAmI, type);
            return false;
        }
		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ItemID.Megashark, 1)
			.AddIngredient(ItemID.VenusMagnum, 1)
			.AddIngredient(ItemID.Tsunami, 1)
			.AddIngredient(ItemID.ChlorophyteBar, 24)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
}
