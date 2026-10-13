using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Jarkos.projectiles.mecha;
using Jarkos.items.materials;
using jarkos.items.materials;
namespace Jarkos.items.weapons.ranged.firing
{
	public class Rollback : ModItem
	{
        public override void SetDefaults()
        {
            Item.width = 74;
            Item.height = 26;
            Item.damage = 210;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.knockBack = 6f;
            Item.shoot = ModContent.ProjectileType<Rollback_Holdout>();
            Item.shootSpeed = 16f;
            Item.UseSound = null;
            Item.autoReuse = true;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 2, 0, 0);
        }
        public override bool AltFunctionUse(Player player)
        {
            return true;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<Rollback_Holdout>(), damage, knockback, player.whoAmI);
            return false;
        }
        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-35, 3);
        }
		public override void AddRecipes()
        {
            CreateRecipe(1)
            .AddIngredient(ItemID.SoulofSight, 10)
            .AddIngredient(ItemID.HallowedBar, 8)
            .AddIngredient(ModContent.ItemType<ThickSilk>(), 2)
            .AddTile(TileID.MythrilAnvil)
            .Register();
		}
	}
}
