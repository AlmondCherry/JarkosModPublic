using jarkos.items.usetiles;
using jarkos.projectiles.bullet;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.ammo
{
	public class Deatharrow : ModItem
	{
		public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 99;
        }
        public override void SetDefaults()
        {
            Item.damage = 14;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 14;
            Item.height = 32;
            Item.maxStack = 9999;
            Item.consumable = true;
            Item.knockBack = 2.5f;
            Item.value = Item.sellPrice(copper: 15);
            Item.rare = ItemRarityID.Cyan;
            Item.shoot = ModContent.ProjectileType<Arrow_death>();
            Item.shootSpeed = 4f;
            Item.ammo = AmmoID.Arrow;
        }
		public override void AddRecipes()
        {
			CreateRecipe(125)
				.AddIngredient(ModContent.ItemType<FulgurBar>(), 1)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
