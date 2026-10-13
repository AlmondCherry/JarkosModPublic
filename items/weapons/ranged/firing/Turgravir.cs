using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using jarkos.items.materials;
using jarkos.projectiles.mecha;
using Terraria.DataStructures;
namespace jarkos.items.weapons.ranged.firing
{
	public class Turgravir : ModItem
	{
		public override void SetDefaults()
        {
            Item.width = 54;
            Item.height = 22;
            Item.damage = 113;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 29;
            Item.useAnimation = 29;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 2f;
            Item.UseSound = null;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<Turgravir_Holdout>();
            Item.shootSpeed = 18f;
            Item.useAmmo = AmmoID.Bullet;
            Item.channel = true;
            Item.noUseGraphic = true;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 2, 0, 0);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<Turgravir_Holdout>(), damage, knockback, player.whoAmI, type);
            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<FortifiedPlate>(), 8)
			.AddIngredient(ModContent.ItemType<ChitinBeetle>(), 6)
			.AddIngredient(ItemID.HallowedBar, 8)
			.AddTile(TileID.MythrilAnvil)
			.Register();
        }
    }
}
