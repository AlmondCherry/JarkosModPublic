using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.projectiles.melee.unique_melee;
using jarkos.items.materials;

namespace jarkos.items.weapons.melee.true_melee
{
	public class AsteroidSword : ModItem
	{
		public int attackType = 0;
		public int comboExpireTimer = 0;
		public override void SetDefaults()
		{
			Item.damage = 56;
			Item.crit = 4;
			Item.DamageType = DamageClass.Melee;
			Item.autoReuse = true;
			Item.knockBack = 9;
			Item.useTime = 7;
			Item.useAnimation = 7;
			Item.width = 50;
			Item.height = 50;
			Item.scale = 1f;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.shoot = ModContent.ProjectileType<Asteroid_sword_swing>();
			Item.shootSpeed = 20f;

			Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(0, 1, 0, 0);
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			Projectile.NewProjectile(source, position, velocity * 0, type, damage, knockback, player.whoAmI, attackType);
			attackType = (attackType + 1) % 2;
			comboExpireTimer = 0;
			return false;
		}
		public override void UpdateInventory(Player player) {
			if (comboExpireTimer++ >= 120)
				attackType = 0;
		}
		public override void AddRecipes()
		{
    		CreateRecipe()
        	.AddIngredient(ItemID.HellstoneBar, 12)
			.AddIngredient(ItemID.MeteoriteBar, 8)
			.AddIngredient(ModContent.ItemType<FragmentOfRage>(), 2)
			.AddIngredient(ItemID.Obsidian, 18)
			.AddTile(TileID.Anvils)
        	.Register();
		}
	}
}
