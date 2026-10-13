using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.projectiles.melee.other_melee;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using jarkos.items.usetiles;
using jarkos.items.materials;
namespace jarkos.items.weapons.melee.other_melee
{
	public class BloodBrotherHammer : ModItem
    {
        private int swingCounter = 0;
        public override void SetDefaults()
        {
            Item.width = 62;
            Item.height = 62;
            Item.damage = 320;
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 34;
            Item.useAnimation = 34;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 9.5f;
            Item.UseSound = null;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<BloodyHammer_proj>();
            Item.shootSpeed = 12f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = false;

            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(0, 1, 10, 0);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            swingCounter++;
            float swingDir = swingCounter % 2 == 0 ? 1f : -1f;
            Projectile.NewProjectile(source, player.MountedCenter, velocity, type, damage, knockback, player.whoAmI, ai0: 0f, ai1: swingDir);
            return false;
        }

        public override void AddRecipes()
		{
    		CreateRecipe()
        	.AddIngredient(ItemID.MoltenHamaxe, 1)
			.AddIngredient(ModContent.ItemType<BloodyAlloy>(), 12)
            .AddIngredient(ModContent.ItemType<PulsatingFragment>(), 2)
            .AddTile(TileID.Anvils)
        	.Register();
		}
    }
}
