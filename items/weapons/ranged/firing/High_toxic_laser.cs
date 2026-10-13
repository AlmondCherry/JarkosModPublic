using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Jarkos.projectiles.mecha;
using Jarkos.items.materials;
using jarkos.items.materials;
using Jarkos.items.usetiles;
namespace Jarkos.items.weapons.ranged.firing
{
	public class High_toxic_laser : ModItem
	{
		public override void SetDefaults()
        {
            Item.damage = 48;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 62;
            Item.height = 26;
            Item.useTime = 21;
            Item.useAnimation = 21;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.knockBack = 2.5f;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<High_toxic_laser_Holdout>();
            Item.shootSpeed = 10f;
            Item.UseSound = null;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 1, 0, 0);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<High_toxic_laser_Holdout>(), damage, knockback, player.whoAmI);
            return false;
        }
        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-45f, 0f);
        }
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            position += velocity.SafeNormalize(Vector2.Zero) * 45f;
        }
		public override void AddRecipes() {
		CreateRecipe(1)
		.AddIngredient(ModContent.ItemType<SteelBar>(), 4)
		.AddIngredient(ItemID.JungleSpores, 12)
        .AddIngredient(ItemID.SoulofNight, 6)
		.AddTile(TileID.MythrilAnvil)
		.Register();
		}
	}
}
