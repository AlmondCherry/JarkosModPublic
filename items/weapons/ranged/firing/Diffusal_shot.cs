using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Jarkos.projectiles.mecha;

namespace Jarkos.items.weapons.ranged.firing
{
	public class Diffusal_shot : ModItem
	{
		public override void SetDefaults()
        {
            Item.width = 54;
            Item.height = 28;
            Item.damage = 34;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 24;
            Item.useAnimation = 24;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.knockBack = 6f;
            Item.UseSound = null;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<Diffusal_shot_Holdout>();
            Item.shootSpeed = 16f;

            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(0, 1, 0, 0);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<Diffusal_shot_Holdout>(), damage, knockback, player.whoAmI);
            return false;
        }
        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-50f, 2f);
        }
		public override void AddRecipes()
        {
            CreateRecipe(1)
            .AddIngredient(ModContent.ItemType<Ignis_shot>(), 1)
            .AddIngredient(ModContent.ItemType<Fulgur_shot>(), 1)
            .AddIngredient(ItemID.HellstoneBar, 6)
            .AddTile(TileID.Anvils)
            .Register();
		}
	}
}
