using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using jarkos.projectiles.ranged.firearm;
namespace jarkos.items.weapons.ranged.firing
{
	public class Liquid_magma : ModItem
	{
		public override void SetDefaults()
		{
			Item.useTime = 3;
			Item.DamageType = DamageClass.Ranged;
            Item.width = 60;
            Item.height = 28;
            Item.useStyle = ItemUseStyleID.Shoot;
			Item.useAnimation = 9;
			Item.reuseDelay = 3;
            Item.damage = 24;
            Item.autoReuse = true;
			Item.scale = 1f;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<Liquid_magmaproj>();
			Item.shootSpeed = 18f;

			Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(0, 1, 0, 0);
        }
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			velocity = velocity.RotatedByRandom(MathHelper.ToRadians(10));
		}
        public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ItemID.LavaBucket, 2)
			.AddIngredient(ItemID.Hellstone, 10)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
