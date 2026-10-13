using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.projectiles.ranged.other;
using jarkos.projectiles;

namespace jarkos.items.weapons.magic.other
{
	public class BeeRune : ModItem
	{
		public override void SetDefaults()
		{
			Item.damage = 7;
			Item.DamageType = DamageClass.Magic;
			Item.crit = 4;
			Item.width = 48;
			Item.height = 28;
			Item.useAnimation = 29;
			Item.useTime = 29;
			Item.scale = 1f;
			Item.mana = 20;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.autoReuse = true;
			Item.noMelee = true;
			Item.shoot = ModContent.ProjectileType<Empty_proj>();
			Item.shootSpeed = 2f;

			Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 30, 0);
		}
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
			for(int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f) + Vector2.One.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-360, 360))) * 8f;
				Projectile.NewProjectile(null, player.Center, vel, ModContent.ProjectileType<Bee_rocket>(), Item.damage, 0);
            }
            return true;
        }
		public override void AddRecipes() {
			CreateRecipe(1)
			.AddIngredient(ItemID.BeeGun, 1)
			.AddIngredient(ItemID.BeeWax, 16)
			.AddIngredient(ItemID.Grenade, 80)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
