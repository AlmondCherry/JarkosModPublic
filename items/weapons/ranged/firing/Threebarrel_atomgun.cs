using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Jarkos.projectiles.mecha;
using Jarkos.items.materials;
namespace Jarkos.items.weapons.ranged.firing
{
	public class Threebarrel_atomgun : ModItem
	{
		public override void SetDefaults()
        {
            Item.damage = 49;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 60;
            Item.height = 26;
            Item.useTime = 12;
            Item.useAnimation = 12;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 1.5f;
            Item.UseSound = null;
            Item.autoReuse = true;
            Item.channel = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<Threebarrel_Holdout>();
            Item.shootSpeed = 18f;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 2, 0, 0);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            return true;
        }
		public override void AddRecipes() {
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<Twobarrel_atomgun>(), 1)
			.AddIngredient(ModContent.ItemType<DiffusalBar>(), 4)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
}
