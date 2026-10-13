using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using jarkos.projectiles.summon.whip;
using jarkos.items.materials;

namespace jarkos.items.weapons.summon.whip
{
	public class Scourge_of_Dark_Matter : ModItem
	{
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}
		public override void SetDefaults()
		{
			Item.DamageType = DamageClass.SummonMeleeSpeed;
			Item.damage = 96;
			Item.knockBack = 2;
			Item.shoot = ModContent.ProjectileType<Scourge_of_Dark_Matter_proj>();
			Item.shootSpeed = 4;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 34;
			Item.useAnimation = 34;
			Item.UseSound = SoundID.Item152;
			Item.channel = true;
			Item.noMelee = true;
			Item.noUseGraphic = true;

			Item.rare = ItemRarityID.Red;
            Item.value = Item.sellPrice(0, 4, 10, 0);
		}
		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<Whip_of_perfection>(), 1)
			.AddIngredient(ModContent.ItemType<HeavenlyPowder>(), 3)
			.AddTile(TileID.LunarCraftingStation)
			.Register();
		}
	}
}
