using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Jarkos.projectiles.mecha;
using Jarkos.items.usetiles;
using jarkos.items.materials;
namespace Jarkos.items.weapons.ranged.firing
{
	public class Scarlet_traveler : ModItem
	{
        public override void SetDefaults()
        {
            Item.width = 54;
            Item.height = 22;
            Item.damage = 19;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 28;
            Item.useAnimation = 28;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 3f;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<Scarlet_traveler_Holdout>();
            Item.shootSpeed = 14f;
            Item.noUseGraphic = true;
            Item.channel = true;

            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 20, 0);
        }
        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[Item.shoot] < 1 && player.GetModPlayer<jarkos.systems.PurgatoryPlayer>().ScarletTravelerCooldown == 0;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.GetModPlayer<jarkos.systems.PurgatoryPlayer>().ScarletTravelerCooldown = 28;
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }
        public override void AddRecipes() 
        {
            CreateRecipe(1)
            .AddIngredient(ModContent.ItemType<SteelBar>(), 6)
            .AddRecipeGroup(nameof(ItemID.TissueSample), 18)
            .AddIngredient(ItemID.Ruby, 2)
            .AddTile(TileID.Anvils)
            .Register();
		}
	}
}
