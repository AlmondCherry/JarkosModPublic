using jarkos.projectiles.melee.other_melee;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.weapons.melee.other_melee
{
    public class TireIron : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 17;
            Item.DamageType = DamageClass.Melee;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.knockBack = 4f;
            Item.UseSound = SoundID.Item1;
            Item.shoot = ModContent.ProjectileType<Lomprojectile>();
            Item.shootSpeed = 14f;
            Item.channel = false;
            Item.autoReuse = false;

            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 5, 0);
        }
        public override bool CanUseItem(Player player)
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.owner == player.whoAmI && proj.type == ModContent.ProjectileType<Lomprojectile>())
                {
                    return false;
                }
            }
            return true;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<Lomprojectile>(), damage, knockback, player.whoAmI);
            return false;
        }
        public override void AddRecipes()
		{
    		CreateRecipe()
            .AddRecipeGroup(nameof(ItemID.GoldBar), 8)
            .AddRecipeGroup(nameof(ItemID.IronBar), 8)
            .AddIngredient(ItemID.Chain, 20)
            .AddTile(TileID.Anvils)
        	.Register();
		}
    }
}
