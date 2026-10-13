using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Jarkos.items.materials;
using jarkos.projectiles.mecha;
using jarkos.items.usetiles;

namespace Jarkos.items.weapons.ranged.firing
{
	public class Electric_arrestor : ModItem
	{
		public float radians = 0;
		public override void SetDefaults()
		{
			Item.DamageType = DamageClass.Ranged;
			Item.useTime = 24;
			Item.useAnimation = 24;
			Item.width = 58;
			Item.height = 30;
            Item.damage = 225;
			Item.scale = 1f;
            Item.autoReuse = true;
			Item.shoot = ProjectileType<Electric_arrestor_Holdout>();
			Item.shootSpeed = 8f;
			Item.noMelee = true;
			Item.useStyle = ItemUseStyleID.Shoot;
            Item.channel = true;
            Item.noUseGraphic = true;

			Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 1, 0, 0);
        }
		public override void AddRecipes() {
		CreateRecipe(1)
		.AddIngredient(ModContent.ItemType<Fulgur_shot>(), 1)
		.AddIngredient(ModContent.ItemType<GalvanicAlloy>(), 4)
		.AddTile(TileID.MythrilAnvil)
		.Register();
		}
	}
}
