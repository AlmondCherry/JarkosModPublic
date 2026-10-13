using Jarkos.items.materials;
using Jarkos.projectiles.mecha;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Jarkos.items.weapons.ranged.firing
{
    public class Dark_matter_gun : ModItem
    {
        public override void SetDefaults()
        {
            Item.damage = 45;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 60;
            Item.height = 24;
            Item.useTime = 5;
            Item.useAnimation = 5;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 2f;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<Dark_matter_gun_Holdout>();
            Item.shootSpeed = 18f;
            Item.noUseGraphic = true;
            Item.channel = true;

            Item.rare = ItemRarityID.Red;
            Item.value = Item.sellPrice(0, 10, 0, 0);
        }
        public override void AddRecipes()
		{
			CreateRecipe(1)
            .AddIngredient(ModContent.ItemType<BlockingPrism>(), 6)
			.AddIngredient(ModContent.ItemType<Diffusal_shot>(), 1)
			.AddTile(TileID.LunarCraftingStation)
			.Register();
		}
    }
}
