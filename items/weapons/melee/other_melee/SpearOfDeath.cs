using jarkos.projectiles.melee.other_melee;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.weapons.melee.other_melee
{
    public class SpearOfDeath : ModItem
    {
        private int swingVariation = 0;

        public override void SetStaticDefaults()
        {
            ItemID.Sets.SkipsInitialUseSound[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.scale = 0.9f;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = 24;
            Item.useAnimation = 24;
            Item.autoReuse = true;
            Item.UseSound = null;
            Item.damage = 65;
            Item.knockBack = 5f;
            Item.crit = 6;
            Item.DamageType = DamageClass.Melee;
            Item.shoot = ModContent.ProjectileType<Spear_death_proj>();
            Item.shootSpeed = 1f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = false;

            Item.value = Item.sellPrice(0, 1, 20, 0);
            Item.rare = ItemRarityID.Pink;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int variation = swingVariation;
            swingVariation = (swingVariation + 1) % 2;

            Vector2 aim = (Main.MouseWorld - player.Center).SafeNormalize(new Vector2(player.direction, 0f));
            if (player.whoAmI == Main.myPlayer)
            {
                Projectile.NewProjectile(source, player.Center, aim, type, damage, knockback, player.whoAmI, variation);
            }
            return false;
        }
    }
}
