using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.projectiles.melee.true_melee;
using jarkos.projectiles.melee.unique_melee;
using jarkos.items.usetiles;

namespace jarkos.items.weapons.melee.true_melee
{
	public class Uranium_sword : ModItem
	{
		public override void SetDefaults()
		{
			Item.damage = 56;
			Item.DamageType = DamageClass.Melee;
			Item.width = 60;
			Item.height = 60;
			Item.useTime = 35;
			Item.useAnimation = 35;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 6f;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.shoot = ModContent.ProjectileType<Galvanic_sword_swing>();
			Item.shootSpeed = 1f;
			Item.channel = true;
			Item.shootsEveryUse = true;

			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.sellPrice(0, 1, 80, 0);
		}

		public override bool CanUseItem(Player player)
		{
			var modPlayer = player.GetModPlayer<UraniumSwordPlayer>();
			bool isNextSpecial = ((modPlayer.attackCount + 1) % 5 == 0);
			if (isNextSpecial)
			{
				Item.useTime = 120;
				Item.useAnimation = 120;
			}
			else
			{
				Item.useTime = 36;
				Item.useAnimation = 36;
			}
			return true;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			var modPlayer = player.GetModPlayer<UraniumSwordPlayer>();
			modPlayer.attackCount++;

			bool isSpecial = (modPlayer.attackCount % 5 == 0);

			if (isSpecial)
			{
				Vector2 dir = velocity.SafeNormalize(Vector2.UnitX * player.direction);
				Projectile.NewProjectile(
					source,
					player.MountedCenter,
					dir,
					ModContent.ProjectileType<Uranium_sword_laser>(),
					(int)(damage * 2.5f),
					knockback * 1.5f,
					player.whoAmI
				);
			}
			else
			{
				int angDir = (modPlayer.attackCount % 2 == 0) ? -1 : 1;
				Vector2 dir = velocity.SafeNormalize(Vector2.UnitX * player.direction);
				Projectile.NewProjectile(
					source,
					player.MountedCenter,
					dir,
					ModContent.ProjectileType<Galvanic_sword_swing>(),
					damage,
					knockback,
					player.whoAmI,
					0,
					angDir
				);
			}
			return false;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
			.AddIngredient(ModContent.ItemType<Lazatep_sword>(), 1)
			.AddIngredient(ModContent.ItemType<GalvanicAlloy>(), 6)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}

	public class UraniumSwordPlayer : ModPlayer
	{
		public int attackCount;
	}
}
