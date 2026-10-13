using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.projectiles.ranged.bow;

namespace jarkos.items.weapons.ranged.bows
{
	public class Bamboo_bow : ModItem
	{
		public override void SetDefaults()
		{
			Item.useTime = 25;
			Item.DamageType = DamageClass.Ranged;
            Item.width = 24;
            Item.height = 48;
            Item.useStyle = ItemUseStyleID.Shoot;
			Item.useAnimation = 25;
            Item.damage = 22;
            Item.autoReuse = true;
            Item.noMelee = true;
			Item.scale = 1f;
			Item.shoot = ProjectileID.WoodenArrowFriendly;
			Item.shootSpeed = 10f;
			Item.useAmmo = AmmoID.Arrow;
			Item.UseSound = SoundID.Item5;

			Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 10, 0);
        }
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) 
		{
			if (Main.rand.NextBool(4)) 
			{
				type = ModContent.ProjectileType<Bamboo_arrow_proj>();
			}

			Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
			return false;
		}
		public override void AddRecipes() 
		{
			CreateRecipe()
				.AddIngredient(ItemID.BambooBlock, 15)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}
}
