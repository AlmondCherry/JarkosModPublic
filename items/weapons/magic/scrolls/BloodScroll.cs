using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using jarkos.projectiles.magic.scrolls;
using jarkos.items.materials;
using Terraria.DataStructures;

namespace jarkos.items.weapons.magic.scrolls
{
	public class BloodScroll : ModItem
	{
		public override void SetDefaults()
		{
			Item.damage = 39;
            Item.maxStack = 9999;
			Item.DamageType = DamageClass.Magic;
			Item.width = 32;
			Item.height = 32;
			Item.useTime = 15;
			Item.useAnimation = 15;
			Item.scale = 1f;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 2;
			Item.noMelee = true;
      		Item.noUseGraphic = true;
			Item.consumable = true;
			Item.shoot = ModContent.ProjectileType<Blood_arrow>();
			Item.shootSpeed = 14f;

			Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 0, 50);
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
			var velocity1 = velocity.RotatedBy(MathHelper.ToRadians(8));
			var velocity2 = velocity.RotatedBy(MathHelper.ToRadians(-8));
			Projectile.NewProjectile(source, position, velocity1, type, damage, knockback);
			Projectile.NewProjectile(source, position, velocity2, type, damage, knockback);
			return base.Shoot(player, source, position, velocity, type, damage, knockback);
		}
		public override void AddRecipes() {
			CreateRecipe(10)
			.AddIngredient(ModContent.ItemType<FragmentOfRage>(), 1)
			.Register();
		}
	}
}
