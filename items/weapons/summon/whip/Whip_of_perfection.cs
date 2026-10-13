using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using jarkos.projectiles.summon.whip;
using Jarkos.items.materials;

namespace jarkos.items.weapons.summon.whip
{
	public class Whip_of_perfection : ModItem
	{
		public override void SetStaticDefaults() 
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}
		public override void SetDefaults() 
		{
			Item.DamageType = DamageClass.SummonMeleeSpeed;
			Item.damage = 76;
			Item.knockBack = 2;
			Item.shoot = ModContent.ProjectileType<Whip_of_perfection_proj>();
			Item.shootSpeed = 4;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 24;
			Item.useAnimation = 24;
			Item.UseSound = SoundID.Item152;
			Item.channel = true;
			Item.noMelee = true;
			Item.noUseGraphic = true;

			Item.rare = ItemRarityID.Lime;
            Item.value = Item.sellPrice(0, 3, 0, 0);
		}
		public override void AddRecipes() 
		{
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<Diffusal_whip>(), 1)
			.AddIngredient(ItemID.ChlorophyteBar, 10)
			.AddIngredient(ModContent.ItemType<ResidualShadow>(), 12)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
}
