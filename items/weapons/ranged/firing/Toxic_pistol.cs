using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.projectiles.ranged.firearm;
using jarkos.projectiles;
using Jarkos.projectiles.ranged.firearm;
namespace jarkos.items.weapons.ranged.firing
{
	public class Toxic_pistol : ModItem
	{
		private float shoot_type = 0f;
		public override void SetDefaults()
		{
			Item.useTime = 24;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 84;
			Item.height = 44;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useAnimation = 24;
			Item.damage = 46;
			Item.autoReuse = true;
			Item.noMelee = true;
			Item.scale = 1f;
			Item.UseSound = SoundID.Item41;
			Item.autoReuse = true;
			Item.shoot = ProjectileType<Empty_proj>();
			Item.shootSpeed = 18f;

			Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 20, 0);
		}
		public override Vector2? HoldoutOffset()
		{
			return new Vector2(-5f, 2f);
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			shoot_type++;
			if (shoot_type > 3)
			{
				shoot_type = 0;
			}
			if (shoot_type == 0 && player.ownedProjectileCounts[ModContent.ProjectileType<Toxic_pistol_proj>()] != 1)
			{
				Item.noUseGraphic = false;
				Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<Toxic_bolt>(), damage + (damage * 20 / 100), knockback);
			}
			if (shoot_type == 1 && player.ownedProjectileCounts[ModContent.ProjectileType<Toxic_pistol_proj>()] != 1)
			{
				Item.noUseGraphic = false;
				Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<Toxic_bolt>(), damage + (damage * 20 / 100), knockback);
			}
			if (shoot_type == 2 && player.ownedProjectileCounts[ModContent.ProjectileType<Toxic_pistol_proj>()] != 1)
			{
				Item.noUseGraphic = false;
				Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<Toxic_bolt>(), damage + (damage * 20 / 100), knockback);
			}
			if (shoot_type == 3 && player.ownedProjectileCounts[ModContent.ProjectileType<Toxic_pistol_proj>()] < 1)
			{
				Item.noUseGraphic = true;
				Projectile.NewProjectile(source, position, velocity * 2.5f, ModContent.ProjectileType<Toxic_pistol_proj>(), damage + (damage * Main.rand.Next(250, 420) / 100), knockback);
			}
			return base.Shoot(player, source, position, velocity, type, damage, knockback);
		}
        public override bool CanUseItem(Player player)
        {
			if (player.ownedProjectileCounts[ModContent.ProjectileType<Toxic_pistol_proj>()] == 1)
			{
				return false;
			}
			return true;
        }
		public override void AddRecipes()
        {
            CreateRecipe()
				.AddIngredient(ItemID.FlintlockPistol, 1)
                .AddRecipeGroup(nameof(ItemID.Vertebrae), 8)
                .AddRecipeGroup(nameof(ItemID.TissueSample), 25)
                .AddIngredient(ItemID.DirtBlock, 250)
                .AddTile(TileID.Anvils)
                .Register();
        }
	}
}
