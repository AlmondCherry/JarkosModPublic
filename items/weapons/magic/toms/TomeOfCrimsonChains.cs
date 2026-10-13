using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.projectiles.magic.toms;
using jarkos.items.usetiles;

namespace Jarkos.items.weapons.magic.toms
{
	public class TomeOfCrimsonChains : ModItem
	{
		public override void SetDefaults()
        {
            Item.damage = 34;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 9;
            Item.width = 28;
            Item.height = 32;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.channel = true;
            Item.knockBack = 3f;
            Item.UseSound = SoundID.Item103 with { Pitch = 0.2f, Volume = 0.7f };
            Item.shoot = ModContent.ProjectileType<BloodCloudProj>();
            Item.shootSpeed = 16f;
            Item.autoReuse = false;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 1, 10, 0);
        }
        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[Item.shoot] < 1;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<BloodCloudProj>(), damage, knockback, player.whoAmI);
            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.SpellTome, 1)
                .AddIngredient(ModContent.ItemType<BloodyAlloy>(), 6)
                .AddIngredient(ItemID.Cloud, 50)
                .AddIngredient(ItemID.FallenStar, 4)
                .AddTile(TileID.Bookcases)
                .Register();
        }
    }
}
