using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using jarkos.projectiles.mecha;
using Terraria.DataStructures;
namespace jarkos.items.weapons.ranged.firing
{
	public class Plague_slimemachine : ModItem
	{
		public override void SetDefaults()
		{
			Item.useTime = 42;
			Item.DamageType = DamageClass.Ranged;
            Item.width = 84;
            Item.height = 44;
            Item.useStyle = ItemUseStyleID.Shoot;
			Item.useAnimation = 42;
            Item.damage = 126;
            Item.autoReuse = true;
            Item.noMelee = true;
			Item.scale = 1f;
			Item.UseSound = null;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<Plague_slimemachine_Holdout>();
			Item.shootSpeed = 18f;
            Item.channel = true;
            Item.noUseGraphic = true;

            Item.value = Item.sellPrice(0, 0, 30, 0);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<Plague_slimemachine_Holdout>(), damage, knockback, player.whoAmI);
            return false;
        }
        public override Vector2? HoldoutOffset() {
			return new Vector2(-5f, 2f);
		}
	}
}
