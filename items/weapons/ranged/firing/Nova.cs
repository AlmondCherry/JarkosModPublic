using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.projectiles.mecha;
using Jarkos.items.materials;
using jarkos.items.materials;
using jarkos.items.usetiles;
using Terraria.Audio;
namespace jarkos.items.weapons.ranged.firing
{
	public class Nova : ModItem
	{
		public override void SetDefaults()
        {
            Item.damage = 1005;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 72;
            Item.useAnimation = 72;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.knockBack = 6.5f;
            Item.UseSound = null;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<Nova_Holdout>();
            Item.shootSpeed = 18f;

            Item.rare = ItemRarityID.Red;
            Item.value = Item.sellPrice(0, 10, 0, 0);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<Nova_Holdout>(), damage, knockback, player.whoAmI);
            return false;
        }
        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-30, 0);
        }
		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<CrystalClearAlloy>(), 4)
			.AddIngredient(ModContent.ItemType<DiffusalBar>(), 4)
			.AddIngredient(ModContent.ItemType<ThickSilk>(), 2)
			.AddTile(TileID.LunarCraftingStation)
			.Register();
		}
	}
}
