using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.items.weapons.magic.scrolls;
using jarkos.projectiles.magic.other;
using jarkos.projectiles;

namespace jarkos.items.weapons.magic.other
{
	public class FireballRune : ModItem
	{
		public override void SetDefaults()
		{
			Item.damage = 11;
			Item.DamageType = DamageClass.Magic;
			Item.channel = true;
			Item.crit = 6;
			Item.width = 16;
			Item.height = 18;
			Item.useAnimation = 36;
			Item.useTime = 36;
			Item.scale = 1f;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 2;
			Item.mana = 10;
			Item.autoReuse = true;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.shoot = ModContent.ProjectileType<Empty_proj>();
			Item.shootSpeed = 2f;

			Item.rare = ModContent.RarityType<jarkos.rare.MagmaRarity>();
            Item.value = Item.sellPrice(0, 0, 40, 0);
		}
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
			for(int i = 0; i < 6; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(4f, 4f) + Vector2.One.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-360, 360))) * 8f;
				Projectile.NewProjectile(source, player.Center, vel, ModContent.ProjectileType<Hellstar3>(), Item.damage, 0, player.whoAmI);
            }
            return false;
        }
		public override void AddRecipes() {
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<FireballScroll>(), 200)
			.AddIngredient(ItemID.Gel, 20)
			.AddIngredient(ItemID.FallenStar, 2)
			.Register();
		}
	}
}
