using jarkos.items.usetiles;
using jarkos.projectiles.bullet;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.ammo
{
	public class Arrowotch : ModItem
	{
		public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 99;
        }
        public override void SetDefaults()
        {
            Item.damage = 16;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 14;
            Item.height = 32;
            Item.maxStack = 9999;
            Item.consumable = true;
            Item.knockBack = 2f;
            Item.value = Item.sellPrice(copper: 12);
            Item.rare = ItemRarityID.Orange;
            Item.shoot = ModContent.ProjectileType<Arrow_otch>();
            Item.shootSpeed = 3.5f;
            Item.ammo = AmmoID.Arrow;
        }
		public override void AddRecipes()
        {
			CreateRecipe(125)
				.AddIngredient(ModContent.ItemType<IgnisiumBar>(), 1)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
