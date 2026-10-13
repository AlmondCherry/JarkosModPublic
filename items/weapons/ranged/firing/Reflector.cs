using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.projectiles.mecha;
using Jarkos.items.materials;
using jarkos.items.materials;
using Jarkos.items.usetiles;
namespace Jarkos.items.weapons.ranged.firing
{
	public class Reflector : ModItem
	{
		public override void SetDefaults()
        {
            Item.width = 54;
            Item.height = 22;
            Item.damage = 22;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 3f;
            Item.UseSound = SoundID.Item157;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<Reflector_Holdout>();
            Item.shootSpeed = 18f;
            Item.noMelee = true;
            Item.channel = true;
            Item.noUseGraphic = true;

            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 15, 0);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<Reflector_Holdout>(), damage, knockback, player.whoAmI, type);
            return false;
        }
        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-38, 0);
        }
		public override void AddRecipes() {
            CreateRecipe(1)
            .AddIngredient(ModContent.ItemType<SteelBar>(), 4)
            .AddIngredient(ModContent.ItemType<Battery>(), 2)
            .AddRecipeGroup(nameof(ItemID.TissueSample), 16)
            .AddTile(TileID.Anvils)
            .Register();
		}
	}
}
