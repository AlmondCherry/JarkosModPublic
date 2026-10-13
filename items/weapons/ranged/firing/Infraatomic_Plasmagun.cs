using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Jarkos.projectiles.mecha;
using Jarkos.items.materials;
using jarkos.items.materials;
namespace Jarkos.items.weapons.ranged.firing
{
	public class Infraatomic_Plasmagun : ModItem
	{
        public override void SetDefaults()
        {
            Item.damage = 55;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 48;
            Item.height = 28;
            Item.useTime = 8;
            Item.useAnimation = 8;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 2f;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<Infraatomic_Plasmagun_Holdout>();
            Item.shootSpeed = 18f;
            Item.noUseGraphic = true;
            Item.channel = true;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 1, 0, 0);
        }
		public override void AddRecipes() {
			CreateRecipe(1)
			.AddIngredient(ItemID.HallowedBar, 6)
			.AddIngredient(ModContent.ItemType<ThickSilk>(), 2)
            .AddIngredient(ItemID.SoulofFright, 8)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
}
