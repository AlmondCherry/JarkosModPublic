using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Jarkos.projectiles.magic.staffs;
using Terraria.DataStructures;

namespace jarkos.items.weapons.magic.staffs
{
    public class Hypothermia : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.staff[Item.type] = true;
        }
        public override void SetDefaults()
        {
            Item.useTime = 2;
            Item.DamageType = DamageClass.Magic;
            Item.width = 66;
            Item.height = 66;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 2;
            Item.reuseDelay = 26;
            Item.damage = 125;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.scale = 1f;
            Item.mana = 4;
            Item.UseSound = SoundID.Item43;
            Item.autoReuse = false;
            Item.shoot = ModContent.ProjectileType<Hypothermia_projectile>();
            Item.shootSpeed = 16f;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 1, 20, 0);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                Vector2 velocity1 = Vector2.Normalize(Main.MouseWorld - player.Center) * 16f;
                Vector2 position1 = player.Center - velocity1.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-45f, 45f))) * 6;
                Vector2 to_mouse1 = Vector2.Normalize(Main.MouseWorld - position1) * 16f;
                Projectile.NewProjectile(source, position1, to_mouse1, type, damage, knockback, player.whoAmI);
            }
            return false;
        }
    }
}
