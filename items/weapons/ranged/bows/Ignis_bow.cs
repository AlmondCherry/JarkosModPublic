using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using jarkos.items.usetiles;

namespace jarkos.items.weapons.ranged.bows
{
	public class Ignis_bow : ModItem
	{
		public override void SetDefaults()
		{
			Item.useTime = 19;
			Item.DamageType = DamageClass.Ranged;
            Item.width = 24;
            Item.height = 48;
            Item.useStyle = ItemUseStyleID.Shoot;
			Item.useAnimation = 19;
            Item.damage = 10;
            Item.autoReuse = true;
            Item.noMelee = true;
			Item.scale = 1f;
			Item.noMelee = true;
			Item.channel = true;
            Item.shoot = ProjectileID.PurificationPowder;
			Item.shootSpeed = 26f;
			Item.useAmmo = AmmoID.Arrow;
			Item.UseSound = SoundID.Item5;

			Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 40, 0);
        }
		public override bool CanConsumeAmmo(Item ammo, Player player) {
			return Main.rand.NextFloat() >= 0.70f;
		}
		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			if (type == ProjectileID.WoodenArrowFriendly) {
				type = ModContent.ProjectileType<jarkos.projectiles.bullet.Arrow_otch>();
			}
		}
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ModContent.ItemType<IgnisiumBar>(), 8)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}
