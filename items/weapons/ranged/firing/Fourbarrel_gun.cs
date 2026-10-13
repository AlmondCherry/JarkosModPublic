using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Jarkos.projectiles.mecha;
using Jarkos.items.materials;
namespace Jarkos.items.weapons.ranged.firing
{
	public class Fourbarrel_gun : ModItem
	{
		public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 24;
            Item.damage = 32;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 6;
            Item.useAnimation = 6;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 1.5f;
            Item.UseSound = null;
            Item.autoReuse = true;
            Item.channel = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<Fourbarrel_Holdout>();
            Item.shootSpeed = 18f;

            Item.rare = ItemRarityID.Red;
            Item.value = Item.sellPrice(0, 5, 0, 0);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            return true;
        }
        public override void AddRecipes() 
        {
            CreateRecipe(1)
            .AddIngredient(ModContent.ItemType<Threebarrel_atomgun>(), 1)
            .AddIngredient(ModContent.ItemType<DiffusalBar>(), 4)
            .AddIngredient(ItemID.FragmentVortex, 12)
            .AddTile(TileID.MythrilAnvil)
            .Register();
		}
	}
}
