using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;

namespace jarkos.items.weapons.ranged.firing
{
	public class Antique_beam_rifle : ModItem
	{
		public override void SetDefaults()
		{
			Item.useTime = 24;
			Item.DamageType = DamageClass.Ranged;
            Item.width = 84;
            Item.height = 44;
            Item.useStyle = ItemUseStyleID.Shoot;
			Item.useAnimation = 24;
            Item.damage = 398;
            Item.autoReuse = true;
            Item.noMelee = true;
			Item.scale = 1f;
			Item.UseSound = null;
			Item.channel = true;
			Item.noUseGraphic = true;
			Item.useAmmo = AmmoID.Bullet;
            Item.shoot = ModContent.ProjectileType<jarkos.projectiles.mecha.Antique_beam_rifle_Holdout>();
			Item.shootSpeed = 18f;

			Item.rare = ModContent.RarityType<jarkos.rare.SpeedRarity>();
            Item.value = Item.sellPrice(2, 0, 0, 0);
        }
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) 
		{
			Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<jarkos.projectiles.mecha.Antique_beam_rifle_Holdout>(), damage, knockback, player.whoAmI);
			return false;
		}
	}
}
