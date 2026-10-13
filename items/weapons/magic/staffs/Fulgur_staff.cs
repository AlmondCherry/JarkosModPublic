using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.projectiles.magic.staffs;
using jarkos.items.usetiles;

namespace jarkos.items.weapons.magic.staffs
{
    public class Fulgur_staff : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.staff[Item.type] = true;
        }
        public override void SetDefaults()
        {
            Item.damage = 11;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 8;
            Item.width = 42;
            Item.height = 42;
            Item.useTime = 5;
            Item.useAnimation = 5;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 2.5f;
            Item.UseSound = SoundID.Item72;
            Item.autoReuse = true;
            Item.channel = true;
            Item.noUseGraphic = false;
            Item.shoot = ModContent.ProjectileType<Fulgur_staff_proj>();
            Item.shootSpeed = 1f;

            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 40, 0);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.owner == player.whoAmI && proj.type == type)
                    return false;
            }
            Vector2 direction = velocity;
            if (direction == Vector2.Zero) direction = new Vector2(player.direction, 0f);
            else direction.Normalize();

            Vector2 staffTip = player.Center + direction * 50f;
            Projectile.NewProjectile(source, staffTip, direction, type, damage, knockback, player.whoAmI);
            return false;
        }
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                Vector2 direction = Vector2.Normalize(Main.MouseWorld - player.Center);
                if (!direction.HasNaNs())
                {
                    position = player.Center + direction * 50f;
                    velocity = direction;
                }
            }
        }
        public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ModContent.ItemType<FulgurBar>(), 8)
				.AddIngredient(ItemID.Sapphire, 4)
				.AddTile(TileID.Anvils)
				.Register();
		}
    }
}
