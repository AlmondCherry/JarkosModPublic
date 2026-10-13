using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Jarkos.projectiles.magic.staffs;

namespace jarkos.items.weapons.magic.staffs
{
    public class Plague_magestaff : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.staff[Item.type] = true;
        }
        public override void SetDefaults()
        {
            Item.damage = 72;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 14;
            Item.width = 54;
            Item.height = 54;
            Item.useTime = 28;
            Item.useAnimation = 28;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 4f;
            Item.UseSound = SoundID.Item103;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<Plague_mage_proj>();
            Item.shootSpeed = 12f;

            Item.value = Item.sellPrice(0, 1, 20, 0);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i < 3; i++)
            {
                float phase = MathHelper.TwoPi / 3f * i;
                Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, ai0: phase);
            }
            return false;
        }
    }
}
