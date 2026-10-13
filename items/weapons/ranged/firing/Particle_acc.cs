using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.projectiles.mecha;
using jarkos.items.materials;
namespace Jarkos.items.weapons.ranged.firing
{
	public class Particle_acc : ModItem
	{
        public override void SetDefaults()
        {
            Item.damage = 42;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 54;
            Item.height = 28;
            Item.useTime = 8;
            Item.useAnimation = 8;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 0.5f;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<Particle_acc_Holdout>();
            Item.shootSpeed = 25f;
            Item.channel = true;
            Item.noUseGraphic = true;

            Item.rare = ItemRarityID.Lime;
            Item.value = Item.sellPrice(0, 5, 0, 0);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<Particle_acc_Holdout>(), damage, knockback, player.whoAmI, type);
            return false;
        }
        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-35, 2);
        }
    	public override void AddRecipes() 
        {
            CreateRecipe(1)
            .AddIngredient(ItemType<FortifiedPlate>(), 8)
            .AddIngredient(ItemType<BulbSunEnergy>(), 2)
            .AddIngredient(ItemType<ThickSilk>(), 2)
            .AddTile(TileID.MythrilAnvil)
            .Register();
		}
    }
}
