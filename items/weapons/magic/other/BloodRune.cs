using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.projectiles.magic.other;
using jarkos.items.weapons.magic.scrolls;
using jarkos.items.materials;

namespace jarkos.items.weapons.magic.other
{
	public class BloodRune : ModItem
	{
		public override void SetDefaults()
		{
			Item.damage = 42;
			Item.DamageType = DamageClass.Magic;
			Item.crit = 16;
			Item.width = 38;
			Item.height = 38;
			Item.useAnimation = 23;
			Item.useTime = 23;
			Item.scale = 1f;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 2;
			Item.mana = 7;
			Item.autoReuse = true;
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.shoot = ModContent.ProjectileType<Blood_sp>();
			Item.shootSpeed = 14f;
			Item.rare = ModContent.RarityType<jarkos.rare.HolyRarity>();

			Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 40, 0);
		}
		public override void AddRecipes() {
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<BloodScroll>(), 200)
			.AddIngredient(ItemID.CrimtaneBar, 12)
			.AddIngredient(ModContent.ItemType<BloodDroplet>(), 4)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
