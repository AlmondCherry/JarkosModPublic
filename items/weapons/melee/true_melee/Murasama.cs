using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.projectiles.melee.true_melee;
using jarkos.items.materials;
using jarkos.items.usetiles;

namespace jarkos.items.weapons.melee.true_melee
{
	public class Murasama : ModItem
	{
		public override void SetDefaults()
		{
			Item.damage = 3609;
			Item.DamageType = DamageClass.Melee;
			Item.width = 156;
			Item.height = 324;
			Item.useTime = 2;
			Item.crit = 30;
			Item.useAnimation = 2;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 0;
			Item.autoReuse = true;
			Item.rare = ModContent.RarityType<jarkos.rare.SpeedRarity>();
			Item.shootSpeed = 30;
			Item.shoot = ModContent.ProjectileType<Murasama_slash>();
			Item.noMelee = true;
            Item.value = Item.sellPrice(1, 0, 0, 0);
			Item.noUseGraphic = true;
			Item.channel = true;
		}
		public override void AddRecipes()
		{
    		CreateRecipe()
        	.AddIngredient(ModContent.ItemType<Yamato>(), 1)
			.AddIngredient(ModContent.ItemType<NitoziumPlate>(), 2)
			.AddIngredient(ModContent.ItemType<AntiqueBar>(), 12)
			.AddTile(TileID.LunarCraftingStation)
        	.Register();
		}
    }
}
