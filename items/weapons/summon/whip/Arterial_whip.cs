using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using jarkos.projectiles.summon.whip;
using jarkos.items.usetiles;

namespace jarkos.items.weapons.summon.whip
{
	public class Arterial_whip : ModItem
	{
		public override void SetStaticDefaults() {
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}
		public override void SetDefaults() {
			Item.DamageType = DamageClass.SummonMeleeSpeed;
			Item.damage = 24;
			Item.knockBack = 4;
			Item.shoot = ModContent.ProjectileType<Arterial_whip_proj>();
			Item.shootSpeed = 4;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 26;
			Item.useAnimation = 26;
			Item.UseSound = SoundID.Item152;
			Item.channel = true;
			Item.noMelee = true;
			Item.noUseGraphic = true;

			Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 1, 10, 0);
		}
		public override void AddRecipes() {
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<Lifesteal_whip>(), 1)
			.AddIngredient(ModContent.ItemType<BloodyAlloy>(), 8)
			.AddIngredient(ItemID.SoulofNight, 4)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
