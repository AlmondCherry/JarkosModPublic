using jarkos.items.usetiles;
using jarkos.projectiles.bullet;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.ammo
{
	public class Noxtilium_ammo : ModItem
	{
		public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 99;
        }
        public override void SetDefaults()
        {
            Item.damage = 14;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 8;
            Item.height = 8;
            Item.maxStack = 9999;
            Item.consumable = true;
            Item.knockBack = 2f;
            Item.value = Item.sellPrice(copper: 8);
            Item.rare = ItemRarityID.LightPurple;
            Item.shoot = ModContent.ProjectileType<Noxtilium_bullet_proj>();
            Item.shootSpeed = 5f;
            Item.ammo = AmmoID.Bullet;
        }
		public override void AddRecipes()
        {
			CreateRecipe(75)
				.AddIngredient(ModContent.ItemType<NoxtiliumBar>(), 1)
				.AddIngredient(ItemID.MusketBall, 75)
				.AddTile(TileID.LunarCraftingStation)
				.Register();
		}
	}
}
