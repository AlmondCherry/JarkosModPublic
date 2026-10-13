using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.projectiles.melee.other_melee;
using jarkos.items.materials;

namespace jarkos.items.weapons.melee.true_melee
{
	public class Grantores : ModItem
	{
		public override void SetDefaults()
        {
            Item.damage = 37;
            Item.DamageType = DamageClass.Melee;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 5f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<Grantores_proj>();
            Item.shootSpeed = 14f;
            Item.noMelee = true;
            Item.noUseGraphic = true;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 1, 50, 0);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0f, 0f);
            return false;
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddRecipeGroup(nameof(ItemID.CobaltBar), 12);
            recipe.AddIngredient(ModContent.ItemType<SemiorganicStone>(), 2);
            recipe.AddIngredient(ModContent.ItemType<PulsatingFragment>(), 4);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
}
