using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using jarkos.projectiles.magic.toms;
using jarkos.items.materials;
using Jarkos.items.materials;
using Terraria.DataStructures;

namespace jarkos.items.weapons.magic.toms
{
	public class Galaxity : ModItem
	{
		public override void SetDefaults()
		{
			Item.useTime = 23;
			Item.useAnimation = 23;
            Item.damage = 164;
			Item.scale = 1f;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.UseSound = SoundID.Item9;
            Item.autoReuse = true;
            Item.mana = 14;
			Item.noMelee = true;
            Item.DamageType = DamageClass.Magic;
			Item.shoot = ProjectileType<Purple_galaxy>();
			Item.shootSpeed = 24f;

			Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 1, 30, 0);
        }
        public override void AddRecipes() 
		{
			CreateRecipe(1)
            .AddIngredient(ItemID.SpellTome, 1)
			.AddIngredient(ModContent.ItemType<EnchantedFragment>(), 5)
			.AddIngredient(ModContent.ItemType<ResidualShadow>(), 8)
			.AddIngredient(ItemID.SoulofSight, 12)
			.AddTile(TileID.Bookcases)
			.Register();
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
			Projectile.NewProjectile(null, position, velocity / 1.25f, type = ModContent.ProjectileType<Cyan_galaxy>(), damage / 2, knockback, player.whoAmI);
			return true;
		}
    }
}
