using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using jarkos.projectiles.summon.whip;
using jarkos.items.materials;

namespace jarkos.items.weapons.summon.whip
{
	public class Leaf_whip : ModItem
	{
		public override void SetStaticDefaults() {
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}
		public override void SetDefaults() {
			Item.DamageType = DamageClass.SummonMeleeSpeed;
			Item.damage = 6;
			Item.knockBack = 1;
			Item.shoot = ModContent.ProjectileType<Leaf_whip_proj>();
			Item.shootSpeed = 4;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 18;
			Item.useAnimation = 18;
			Item.UseSound = SoundID.Item152;
			Item.channel = true;
			Item.noMelee = true;
			Item.noUseGraphic = true;

			Item.rare = ItemRarityID.White;
            Item.value = Item.sellPrice(0, 0, 0, 50);
		}
		public override void AddRecipes() {
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<FragmentOfNature>(), 10)
			.AddIngredient(ItemID.Wood, 40)
			.AddIngredient(ItemID.Gel, 4)
			.AddTile(TileID.WorkBenches)
			.Register();
		}
	}
}
