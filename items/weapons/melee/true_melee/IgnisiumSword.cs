using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.projectiles.melee.true_melee;
using jarkos.items.usetiles;

namespace jarkos.items.weapons.melee.true_melee
{
	public class IgnisiumSword : ModItem
	{
		public override void SetDefaults()
		{
			Item.damage = 50;
			Item.DamageType = DamageClass.Melee;
			Item.width = 46;
			Item.height = 52;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 4.5f;
			Item.autoReuse = true;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.shoot = ModContent.ProjectileType<IgnisiumSwordSwing>();
			Item.shootSpeed = 1f;

			Item.rare = ItemRarityID.Orange;
			Item.value = Item.sellPrice(0, 0, 40, 0);
		}

		public override bool CanUseItem(Player player)
		{
			return player.ownedProjectileCounts[Item.shoot] < 1;
		}

		public override void AddRecipes() {
			CreateRecipe(1)
				.AddIngredient(ModContent.ItemType<IgnisiumBar>(), 8)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
