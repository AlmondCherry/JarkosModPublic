using jarkos.items.usetiles;
using jarkos.projectiles.bullet;
using jarkos.rare;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.ammo
{
	public class Anticvar_ammo : ModItem
	{
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;
		}
		public override void SetDefaults()
		{
			Item.width = 16;
			Item.height = 44;
			Item.damage = 79;
			Item.DamageType = DamageClass.Ranged;
			Item.maxStack = Item.CommonMaxStack;
			Item.rare = ModContent.RarityType<SpeedRarity>();
			Item.consumable = true;
			Item.knockBack = 1f;
			Item.shoot = ModContent.ProjectileType<Anticvarammo>();
            Item.ammo = AmmoID.Bullet;
		}
		public override void AddRecipes()
		{
			CreateRecipe(75)
				.AddIngredient(ModContent.ItemType<AntiqueBar>(), 1)
				.AddIngredient(ItemID.MusketBall, 75)
				.AddTile(TileID.LunarCraftingStation)
				.Register();
		}
	}
}
