using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.projectiles.bullet;
using jarkos.items.usetiles;
namespace jarkos.items.weapons.ranged.bows
{
	public class Chrono_bow : ModItem
	{
		public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 46;
            Item.damage = 38;
            Item.knockBack = 1.5f;
            Item.DamageType = DamageClass.Ranged;
            Item.useAmmo = AmmoID.Arrow;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = 14;
            Item.useAnimation = 14;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.shoot = ProjectileID.PurificationPowder;
            Item.shootSpeed = 18f;
            Item.UseSound = SoundID.Item5 with
            {
                Volume = 0.6f, Pitch = 0.4f
            };

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 1, 0, 0);
        }
        public override bool MeleePrefix() => false;
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<Laser_beam_arrow>(), damage, knockback, player.whoAmI);
            return false;
        }
        public override Vector2? HoldoutOffset() => new Vector2(-2, 0);
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<GalvanicAlloy>(), 6)
                .AddIngredient(ItemID.SoulofLight, 4)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
    }
}
