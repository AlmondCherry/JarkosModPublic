using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.projectiles.melee.true_melee;
using jarkos.items.weapons.melee.unique_melee;
using jarkos.items.usetiles;

namespace jarkos.items.weapons.melee.true_melee
{
	public class Yamato : ModItem
	{
		public override void SetDefaults()
		{
			Item.damage = 2956;
			Item.DamageType = DamageClass.Melee;
			Item.width = 226;
			Item.height = 220;
			Item.useTime = 12;
			Item.useAnimation = 12;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 0;
			Item.autoReuse = true;
			Item.shootSpeed = 14;
			Item.shoot = ModContent.ProjectileType<Yamato_slash>();
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.channel = true;

			Item.rare = ItemRarityID.Red;
            Item.value = Item.sellPrice(5, 50, 50, 0);
		}
		public override void AddRecipes()
		{
    		CreateRecipe()
        	.AddIngredient(ModContent.ItemType<Dante_sword>(), 1)
			.AddIngredient(ModContent.ItemType<CrystalClearAlloy>(), 2)
			.AddTile(TileID.MythrilAnvil)
        	.Register();
		}
	}
}
