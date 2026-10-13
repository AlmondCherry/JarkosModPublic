using jarkos.projectiles.bullet;
using Jarkos.items.usetiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.ammo
{
	public class Mecha_ammo : ModItem
	{
		public override void SetDefaults()
        {
            Item.damage = 12;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 8;
            Item.height = 14;
            Item.maxStack = 9999;
            Item.consumable = true;
            Item.knockBack = 2f;
            Item.value = Item.sellPrice(copper: 25);
            Item.rare = ItemRarityID.Orange;
            Item.shoot = ModContent.ProjectileType<Rocket_bullet>();
            Item.shootSpeed = 12f;
            Item.ammo = AmmoID.Bullet;
        }
		public override void AddRecipes()
        {
			CreateRecipe(75)
				.AddIngredient(ModContent.ItemType<SteelBar>(), 1)
				.AddIngredient(ItemID.MusketBall, 75)
				.AddTile(TileID.LunarCraftingStation)
				.Register();
		}
	}
}
