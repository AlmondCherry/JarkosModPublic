using jarkos.projectiles.bullet;
using Jarkos.items.usetiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.ammo
{
	public class Mecha_arrow : ModItem
	{
		public override void SetDefaults()
        {
            Item.damage = 25;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 14;
            Item.height = 32;
            Item.maxStack = 9999;
            Item.consumable = true;
            Item.knockBack = 4f;
            Item.value = Item.sellPrice(copper: 20);
            Item.rare = ItemRarityID.Green;
            Item.shoot = ModContent.ProjectileType<Rocket_arrow>();
            Item.shootSpeed = 2f;
            Item.ammo = AmmoID.Arrow;
        }
		public override void AddRecipes()
        {
			CreateRecipe(125)
				.AddIngredient(ModContent.ItemType<SteelBar>(), 1)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
