using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.projectiles.melee.unique_melee;
using jarkos.items.weapons.melee.true_melee;
using jarkos.items.usetiles;
using jarkos.items.materials;

namespace jarkos.items.weapons.melee.unique_melee
{
	public class Sunfate : ModItem
	{
		public int attackType = 0;
		public int comboExpireTimer = 0;
		public override void SetDefaults()
		{
			Item.damage = 38;
			Item.DamageType = DamageClass.Melee;
			Item.autoReuse = true;
			Item.knockBack = 2;
			Item.useTime = 7;
			Item.useAnimation = 7;
			Item.width = 50;
			Item.height = 50;
			Item.scale = 1f;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
      		Item.noUseGraphic = true;
			Item.shoot = ModContent.ProjectileType<Cu>();
			Item.shootSpeed = 20f;

			Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(0, 1, 20, 0);
		}
        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                if (player.HasBuff(ModContent.BuffType<jarkos.buffs.buff.SunfateLaserCooldown>()))
                {
                    return false;
                }
            }
            return true;
        }

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (player.altFunctionUse == 2)
            {
                Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<SunfateLaserChanneled>(), damage * 2, knockback, player.whoAmI);

                player.AddBuff(ModContent.BuffType<jarkos.buffs.buff.SunfateLaserCooldown>(), 3600);
                return false;
            }

            Projectile.NewProjectile(source, position, velocity * 0, type, damage, knockback, Main.myPlayer, attackType);
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
			.AddIngredient(ModContent.ItemType<IgnisiumSword>(), 1)
			.AddIngredient(ItemID.HellstoneBar, 10)
			.AddIngredient(ModContent.ItemType<BloodyAlloy>(), 4)
			.AddIngredient(ModContent.ItemType<FragmentOfFear>(), 6)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
