using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using jarkos.items.materials;
using Terraria.DataStructures;

namespace jarkos.items.weapons.magic.scrolls
{
	public class SporeScroll : ModItem
	{
		public override void SetDefaults()
		{
			Item.damage = 39;
            Item.maxStack = 9999;
			Item.DamageType = DamageClass.Magic;
			Item.width = 32;
			Item.height = 32;
			Item.useTime = 3;
			Item.useAnimation = 12;
			Item.reuseDelay = 32;
			Item.scale = 1f;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 2;
			Item.noMelee = true;
      		Item.noUseGraphic = true;
			Item.consumable = true;
			Item.shoot = ProjectileID.SporeCloud;
			Item.shootSpeed = 8f;

			Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 0, 50);
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
			var velocity1 = velocity.RotatedByRandom(MathHelper.ToRadians(8));
			var velocity2 = velocity.RotatedByRandom(MathHelper.ToRadians(-9));
			var velocity3 = velocity.RotatedByRandom(MathHelper.ToRadians(17));
			Projectile.NewProjectile(source, position, velocity1 * 1.05f, type, damage, knockback);
			Projectile.NewProjectile(source, position, velocity2 * 1.25f, type, damage, knockback);
			Projectile.NewProjectile(source, position, velocity3 * 1.1f, type, damage, knockback);
			return base.Shoot(player, source, position, velocity, type, damage, knockback);
		}
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            velocity = velocity.RotatedByRandom(MathHelper.ToRadians(8)) * 2.25f;
        }
        public override void AddRecipes() {
			CreateRecipe(10)
			.AddIngredient(ModContent.ItemType<FragmentOfRage>(), 1)
			.Register();
		}
	}
}
