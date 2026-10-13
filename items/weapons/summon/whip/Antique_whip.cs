using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using jarkos.projectiles.summon.whip;
using jarkos.items.materials;
using jarkos.items.usetiles;
using jarkos.rare;

namespace jarkos.items.weapons.summon.whip
{
	public class Antique_whip : ModItem
	{
		public override void SetStaticDefaults() {
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}
		public override void SetDefaults() {
			Item.DamageType = DamageClass.SummonMeleeSpeed;
			Item.damage = 460;
			Item.knockBack = 5;
			Item.shoot = ModContent.ProjectileType<Antique_whip_proj>();
			Item.shootSpeed = 4;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 40;
			Item.rare = ModContent.RarityType<SpeedRarity>();
			Item.useAnimation = 40;
			Item.UseSound = SoundID.Item152;
			Item.channel = true;
			Item.noMelee = true;
			Item.noUseGraphic = true;

            Item.value = Item.sellPrice(0, 40, 0, 0);
		}
		public override void AddRecipes() {
			CreateRecipe(1)
			.AddIngredient(ItemID.RainbowWhip, 1)
			.AddIngredient(ModContent.ItemType<AntiqueBar>(), 8)
			.AddIngredient(ModContent.ItemType<AncientFragment>(), 6)
			.AddIngredient(ModContent.ItemType<ElementalFluid>(), 4)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
