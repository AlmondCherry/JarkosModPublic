using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.projectiles.ranged.firearm;
using jarkos.projectiles.mecha;
using Terraria.Audio;
namespace jarkos.items.weapons.ranged.firing
{
	public class Ultimatum : ModItem
	{
		private int shotCounter = 0;
        public override void SetDefaults()
        {
            Item.damage = 55;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 60;
            Item.height = 24;
            Item.useTime = 12;
            Item.useAnimation = 12;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 4f;
            Item.UseSound = SoundID.Item91;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<Ultimatum_Holdout>();
            Item.shootSpeed = 18f;
            Item.useAmmo = AmmoID.Bullet;
            Item.channel = true;
            Item.noUseGraphic = true;

            Item.rare = ItemRarityID.Red;
            Item.value = Item.sellPrice(0, 10, 0, 0);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<Ultimatum_Holdout>(), damage, knockback, player.whoAmI, type);
            return false;
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.LunarBar, 12);
            recipe.AddIngredient(ItemID.FragmentVortex, 18);
            recipe.AddIngredient(ItemID.IllegalGunParts, 1);
            recipe.AddTile(TileID.LunarCraftingStation);
            recipe.Register();
        }
	}
}
