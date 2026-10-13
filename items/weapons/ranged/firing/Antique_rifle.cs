using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.projectiles.mecha;

namespace jarkos.items.weapons.ranged.firing
{
	public class Antique_rifle : ModItem
	{
		public float shoots = 0f;
		public override void SetDefaults()
		{
			Item.useTime = 4;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 84;
			Item.height = 44;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useAnimation = 4;
			Item.reuseDelay = 0;
			Item.damage = 204;
			Item.noMelee = true;
			Item.scale = 1f;
			Item.UseSound = SoundID.Item41;
			Item.autoReuse = true;
			Item.channel = true;
			Item.noUseGraphic = true;
			Item.shoot = ModContent.ProjectileType<Antique_rifle_Holdout>();
			Item.shootSpeed = 20f;
			Item.useAmmo = AmmoID.Bullet;

			Item.rare = ModContent.RarityType<jarkos.rare.SpeedRarity>();
            Item.value = Item.sellPrice(2, 0, 0, 0);
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<Antique_rifle_Holdout>(), damage, knockback, player.whoAmI);
			return false;
		}
	}
}
