using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.projectiles.magic.other;
using jarkos.items.materials;

namespace jarkos.items.weapons.magic.other
{
	public class StarKatana : ModItem
	{
		public override void SetDefaults()
		{
			Item.damage = 21;
			Item.DamageType = DamageClass.Magic;
			Item.crit = 4;
			Item.width = 48;
			Item.height = 54;
			Item.useTime = 17;
			Item.useAnimation = 17;
			Item.scale = 1f;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.knockBack = 2;
			Item.autoReuse = true;
			Item.mana = 8;
			Item.shootSpeed = 16f;
			Item.shoot = ModContent.ProjectileType<Swing_Sk>();
			Item.noMelee = true;
			Item.shootsEveryUse = true;
			Item.autoReuse = true;

			Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 40, 0);
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			float adjustedItemScale = player.GetAdjustedItemScale(Item);
			Projectile.NewProjectile(source, player.MountedCenter, new Vector2(player.direction, 0f), type, damage, knockback, player.whoAmI, player.direction * player.gravDir, player.itemAnimationMax, adjustedItemScale);
			NetMessage.SendData(MessageID.PlayerControls, -1, -1, null, player.whoAmI);
			Projectile.NewProjectile(null, position, velocity / 1f, type = ModContent.ProjectileType<Star01proj>(), damage, knockback, player.whoAmI);
			return base.Shoot(player, source, position, velocity, type, damage, knockback);
		}
        public override void AddRecipes()
		{
			CreateRecipe()
			.AddIngredient(ItemID.Katana, 1)
			.AddIngredient(ItemID.FallenStar, 6)
			.AddIngredient(ModContent.ItemType<EnchantedFragment>(), 8)
			.AddIngredient(ModContent.ItemType<FragmentOfFear>(), 4)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
