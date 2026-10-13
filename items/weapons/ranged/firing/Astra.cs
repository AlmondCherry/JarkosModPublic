using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.projectiles.mecha;
using jarkos.items.materials;

namespace jarkos.items.weapons.ranged.firing
{
	public class Astra : ModItem
	{
        public override void SetDefaults()
        {
            Item.width = 72;
            Item.height = 28;
            Item.damage = 2500;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 132;
            Item.useAnimation = 132;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 15f;
            Item.UseSound = null;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<Astra_Holdout>();
            Item.shootSpeed = 1f;
            Item.channel = true;
            Item.noUseGraphic = true;

            Item.rare = ItemRarityID.Lime;
            Item.value = Item.sellPrice(0, 5, 0, 0);
        }
		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ItemID.SniperRifle, 1)
			.AddIngredient(ModContent.ItemType<BulbSunEnergy>(), 2)
			.AddTile(TileID.LunarCraftingStation)
			.Register();
		}
	}
}
