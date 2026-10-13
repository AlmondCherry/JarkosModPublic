using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using jarkos.projectiles.bullet;
using jarkos.items.materials;

namespace jarkos.items.weapons.ranged.bows
{
	public class Hopeless_fate : ModItem
	{
		public override void SetDefaults()
		{
			Item.useTime = 8;
			Item.DamageType = DamageClass.Ranged;
            Item.width = 22;
            Item.height = 44;
            Item.useStyle = ItemUseStyleID.Shoot;
			Item.useAnimation = 16;
			Item.reuseDelay = 8;
            Item.damage = 24;
            Item.autoReuse = true;
            Item.noMelee = true;
			Item.scale = 1f;
			Item.noMelee = true;
			Item.channel = true;
            Item.shoot = ProjectileID.PurificationPowder;
			Item.shootSpeed = 26f;
			Item.useAmmo = AmmoID.Arrow;
			Item.UseSound = SoundID.Item5;

            Item.value = Item.sellPrice(0, 0, 20, 0);
        }
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
			if (type == ProjectileID.WoodenArrowFriendly)
			{
				type = ModContent.ProjectileType<Black_feather_arrow>();
			}
        }
		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<FeatherBlack>(), 6)
			.AddIngredient(ItemID.WoodenBow, 1)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
