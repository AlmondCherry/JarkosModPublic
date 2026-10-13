using jarkos.items.materials;
using jarkos.projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.usable
{
	public class FeatherHook : ModItem
	{

		public override void SetDefaults()
		{
			Item.CloneDefaults(ItemID.GrapplingHook);
			Item.width = 28;
			Item.height = 28;
			Item.useTime = 16;
			Item.useAnimation = 16;
			Item.useTurn = true;
			Item.UseSound = SoundID.Item1;
			Item.noMelee = true;
			Item.shoot = ModContent.ProjectileType<FeatherHookProjectile>();
			Item.shootSpeed = 26f;
			Item.channel = true;
			Item.value = Item.sellPrice(0, 1, 50, 0);
			Item.rare = ItemRarityID.Orange;
			Item.maxStack = 1;
		}

		public override bool CanUseItem(Player player)
		{
			int hookType = ModContent.ProjectileType<FeatherHookProjectile>();
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile p = Main.projectile[i];
				if (p.active && p.type == hookType && p.owner == player.whoAmI)
					return false;
			}
			return true;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			Projectile.NewProjectile(source, player.MountedCenter, velocity, type, 0, 0f, player.whoAmI);
			return false;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.Feather, 5)
				.AddIngredient(ItemID.Hook, 1)
				.AddTile(TileID.Anvils)
				.Register();

			CreateRecipe()
				.AddIngredient(ModContent.ItemType<FeatherBlack>(), 5)
				.AddIngredient(ItemID.Hook, 1)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
