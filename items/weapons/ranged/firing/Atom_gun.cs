using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Jarkos.projectiles.mecha;
using jarkos.items.materials;
namespace Jarkos.items.weapons.ranged.firing
{
	public class Atom_gun : ModItem
	{
		public override void SetDefaults()
        {
            Item.damage = 12;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 62;
            Item.height = 22;
            Item.useTime = 7;
            Item.useAnimation = 7;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.knockBack = 2f;
            Item.UseSound = null;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<Atom_gun_Holdout>();
            Item.shootSpeed = 14f;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 1, 0, 0);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<Atom_gun_Holdout>(), damage, knockback, player.whoAmI);
            return false;
        }
		public override Vector2? HoldoutOffset()
        {
            return new Vector2(-45, 0);
        }
		public override void AddRecipes() 
        {
            CreateRecipe(1)
            .AddIngredient(ItemID.Minishark, 1)
            .AddIngredient(ModContent.ItemType<Laser_two>(), 1)
            .AddRecipeGroup(nameof(ItemID.CobaltBar), 8)
            .AddTile(TileID.MythrilAnvil)
            .Register();
		}
	}
}
