using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.items.usetiles;
using jarkos.items.materials;
using jarkos.projectiles.ranged.bows;

namespace jarkos.items.weapons.ranged.bows
{
	public class Anticvar_bow : ModItem
	{
		public override void SetDefaults()
		{
			Item.useTime = 19;
			Item.DamageType = DamageClass.Ranged;
            Item.width = 24;
            Item.height = 48;
            Item.useStyle = ItemUseStyleID.Shoot;
			Item.useAnimation = 19;
            Item.damage = 98;
            Item.autoReuse = true;
			Item.rare = ModContent.RarityType<jarkos.rare.SpeedRarity>();
			Item.scale = 1f;
			Item.noMelee = true;
			Item.channel = true;
            Item.shoot = ModContent.ProjectileType<Anticvar_arrow_proj>();
			Item.shootSpeed = 26f;
			Item.useAmmo = AmmoID.Arrow;
			Item.UseSound = SoundID.Item5;

			Item.rare = ModContent.RarityType<jarkos.rare.SpeedRarity>();
            Item.value = Item.sellPrice(1, 0, 0, 0);
        }
		public override bool CanConsumeAmmo(Item ammo, Player player)
		{
			return Main.rand.NextFloat() >= 0.70f;
		}
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) 
		{
			int projType = ModContent.ProjectileType<Anticvar_arrow_proj>();
			var velocity1 = velocity.RotatedBy(MathHelper.ToRadians(5));
			var velocity2 = velocity.RotatedBy(MathHelper.ToRadians(-5));

			Projectile.NewProjectile(source, position, velocity1, projType, damage, knockback, player.whoAmI, 0, 0);
			Projectile.NewProjectile(source, position, velocity2, projType, damage, knockback, player.whoAmI, 0, 0);
			return false;
		}
		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<AntiqueBar>(), 6)
			.AddIngredient(ModContent.ItemType<AncientFragment>(), 3)
			.AddIngredient(ItemID.Phantasm, 1)
			.AddIngredient(ItemID.Tsunami, 1)
			.AddTile(TileID.LunarCraftingStation)
			.Register();
		}
	}
}
