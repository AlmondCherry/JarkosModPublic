using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using jarkos.projectiles.summon.whip;
using jarkos.items.materials;
using Jarkos.items.materials;

namespace jarkos.items.weapons.summon.whip
{
	public class Diffusal_whip : ModItem
	{
		public override void SetStaticDefaults() 
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}
		public override void SetDefaults() 
		{
			Item.DamageType = DamageClass.SummonMeleeSpeed;
			Item.damage = 47;
			Item.knockBack = 2;
			Item.shoot = ModContent.ProjectileType<Diffusal_whip_proj>();
			Item.shootSpeed = 4;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 32;
			Item.useAnimation = 32;
			Item.UseSound = SoundID.Item152;
			Item.channel = true;
			Item.noMelee = true;
			Item.noUseGraphic = true;

			Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 1, 30, 0);
		}
		public override void AddRecipes() {
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<DiffusalBar>(), 8)
			.AddIngredient(ItemID.HallowedBar, 8)
			.AddIngredient(ModContent.ItemType<ElementalPowder>(), 2)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
