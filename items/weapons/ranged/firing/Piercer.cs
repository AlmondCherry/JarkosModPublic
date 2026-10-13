using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Jarkos.projectiles.mecha;
using jarkos.items.materials;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
namespace Jarkos.items.weapons.ranged.firing
{
	public class Piercer : ModItem
	{
		public override void SetDefaults()
		{
			Item.DamageType = DamageClass.Ranged;
			Item.UseSound = null;
			Item.useTime = 30;
			Item.useAnimation = 30;
            Item.damage = 50;
			Item.scale = 0.65f;
            Item.autoReuse = true;
			Item.shoot = ModContent.ProjectileType<Piercer_Holdout>();
			Item.shootSpeed = 38f;
			Item.channel = true;
            Item.noUseGraphic = true;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;

			Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 10, 0);
        }
		public override bool AltFunctionUse(Player player) {
			return true;
		}
		public override bool CanUseItem(Player player) {
            return player.ownedProjectileCounts[Item.shoot] < 1;
		}
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float altFire = player.altFunctionUse == 2 ? 1f : 0f;
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, altFire);
            return false;
        }
		public override void AddRecipes() {
			CreateRecipe(1)
			.AddRecipeGroup(nameof(ItemID.IronBar), 16)
			.AddIngredient(ItemID.Diamond, 2)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
