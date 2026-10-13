using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Jarkos.projectiles.mecha;
using Jarkos.items.materials;
using jarkos.items.materials;
namespace Jarkos.items.weapons.ranged.firing
{
	public class Nailgun01 : ModItem
	{
		public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 24;
            Item.damage = 42;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 1.5f;
            Item.UseSound = null;
            Item.autoReuse = true;
            Item.channel = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<Nailgun01_Holdout>();
            Item.shootSpeed = 16f;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 1, 0, 0);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            return true;
        }
		public override Vector2? HoldoutOffset() {
			return new Vector2(-20f, 0f);
		}
		public override void AddRecipes() 
        {
            CreateRecipe(1)
            .AddIngredient(ItemID.HallowedBar, 6)
            .AddIngredient(ModContent.ItemType<ThickSilk>(), 2)
            .AddIngredient(ItemID.SoulofSight, 8)
            .AddTile(TileID.MythrilAnvil)
            .Register();
		}
	}
}
