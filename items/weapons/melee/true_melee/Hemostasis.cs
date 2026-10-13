using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.projectiles.melee.true_melee;
using jarkos.items.usetiles;
using jarkos.items.materials;

namespace jarkos.items.weapons.melee.true_melee
{
	public class Hemostasis : ModItem
	{
		public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = 10;
            Item.useAnimation = 10;
            Item.noMelee = true;
            Item.damage = 80;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 10;
            Item.knockBack = 2f;
            Item.shoot = ModContent.ProjectileType<Hemostasis_drain>();
            Item.shootSpeed = 1f;
            Item.channel = true;
            Item.noUseGraphic = true;
            Item.autoReuse = false;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 1, 30, 0);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, player.Center, Vector2.Zero, type, damage, knockback, player.whoAmI);
            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<BloodyAlloy>(), 12)
                .AddIngredient(ModContent.ItemType<ThickSilk>(), 8)
                .AddIngredient(ModContent.ItemType<ElementalPowder>(), 2)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
	}
}
