using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Jarkos.projectiles.mecha;
using Jarkos.items.materials;
using jarkos.items.usetiles;
using jarkos.items.materials;
using Jarkos.items.usetiles;
namespace Jarkos.items.weapons.ranged.firing
{
	public class Fulgur_shot : ModItem
	{
		public override void SetDefaults()
        {
            Item.width = 52;
            Item.height = 28;
            Item.damage = 17;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 4f;
            Item.UseSound = null;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<Fulgur_shot_Holdout>();
            Item.shootSpeed = 12f;
            Item.channel = true;
            Item.noUseGraphic = true;

            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 40, 0);
        }
        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-6, 0);
        }
		public override void AddRecipes() {
		CreateRecipe(1)
		.AddIngredient(ModContent.ItemType<SteelBar>(), 3)
		.AddIngredient(ModContent.ItemType<FulgurBar>(), 8)
		.AddIngredient(ModContent.ItemType<PulsatingFragment>(), 2)
		.AddTile(TileID.Anvils)
		.Register();
		}
	}
}
