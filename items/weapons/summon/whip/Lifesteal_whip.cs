using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using jarkos.projectiles.summon.whip;
using jarkos.items.materials;

namespace jarkos.items.weapons.summon.whip
{
	public class Lifesteal_whip : ModItem
	{
		public override void SetStaticDefaults() {
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}
		public override void SetDefaults() {
			Item.DamageType = DamageClass.SummonMeleeSpeed;
			Item.damage = 24;
			Item.knockBack = 4;
			Item.shoot = ModContent.ProjectileType<Lifesteal_whip_proj>();
			Item.shootSpeed = 4;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 26;
			Item.useAnimation = 26;
			Item.UseSound = SoundID.Item152;
			Item.channel = true;
			Item.noMelee = true;
			Item.noUseGraphic = true;

			Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 30, 0);
		}
		public override void AddRecipes() {
			CreateRecipe(1)
			.AddIngredient(ItemID.BlandWhip, 1)
			.AddIngredient(ModContent.ItemType<SyntheticBlood>(), 20)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
