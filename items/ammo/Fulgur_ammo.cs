using jarkos.items.usetiles;
using jarkos.projectiles.bullet;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.ammo
{
	public class Fulgur_ammo : ModItem
	{
		public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 99;
        }
        public override void SetDefaults()
        {
            Item.damage = 12;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 8;
            Item.height = 8;
            Item.maxStack = 9999;
            Item.consumable = true;
            Item.knockBack = 1.5f;
            Item.value = Item.sellPrice(copper: 8);
            Item.rare = ItemRarityID.Orange;
            Item.shoot = ModContent.ProjectileType<Bullet_death>();
            Item.shootSpeed = 5f;
            Item.ammo = AmmoID.Bullet;
        }
		public override void AddRecipes()
        {
			CreateRecipe(75)
				.AddIngredient(ModContent.ItemType<FulgurBar>(), 1)
				.AddIngredient(ItemID.MusketBall, 75)
				.AddTile(TileID.LunarCraftingStation)
				.Register();
		}
	}
}
