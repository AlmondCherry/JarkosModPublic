using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using jarkos.projectiles.ranged.bow;

namespace jarkos.items.weapons.ranged.bows
{
	public class FrostWeakBow : ModItem
	{
		public override void SetDefaults()
		{
			Item.useTime = 25;
			Item.DamageType = DamageClass.Ranged;
            Item.width = 24;
            Item.height = 48;
            Item.useStyle = ItemUseStyleID.Shoot;
			Item.useAnimation = 25;
            Item.damage = 18;
            Item.autoReuse = true;
            Item.noMelee = true;
			Item.scale = 1f;
			Item.shoot = ModContent.ProjectileType<FrostWeakBowHoldout>();
			Item.shootSpeed = 12f;
			Item.useAmmo = AmmoID.Arrow;
			Item.channel = true;
			Item.noUseGraphic = true;

			Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 20, 0);
        }
		public override bool CanUseItem(Player player)
		{
			return player.ownedProjectileCounts[Item.shoot] <= 0;
		}
		public override bool CanConsumeAmmo(Item ammo, Player player)
		{
			return player.ownedProjectileCounts[Item.shoot] > 0;
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			type = ModContent.ProjectileType<FrostWeakBowHoldout>();
		}
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ItemID.WoodenBow, 1)
				.AddIngredient(ItemID.SnowBlock, 150)
				.AddIngredient(ItemID.IceBlock, 50)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
