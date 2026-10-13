using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using jarkos.projectiles.summon.whip;
using jarkos.items.usetiles;

namespace jarkos.items.weapons.summon.whip
{
	public class Whip_of_energy : ModItem
	{
		public override void SetStaticDefaults() {
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}
		public override void SetDefaults() {
			Item.DamageType = DamageClass.SummonMeleeSpeed;
			Item.damage = 17;
			Item.knockBack = 4;
			Item.shoot = ModContent.ProjectileType<Whip_of_energy_proj>();
			Item.shootSpeed = 4;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 28;
			Item.useAnimation = 28;
			Item.UseSound = SoundID.Item152;
			Item.channel = true;
			Item.noMelee = true;
			Item.noUseGraphic = true;

			Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 20, 0);
		}
		public override void AddRecipes() {
			CreateRecipe(1)
			.AddIngredient(ItemID.Rope, 50)
			.AddIngredient(ItemID.Silk, 8)
			.AddIngredient(ModContent.ItemType<FulgurBar>(), 8)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
