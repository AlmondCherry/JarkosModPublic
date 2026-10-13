using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.projectiles.bullet;

namespace jarkos.items.weapons.ranged.bows
{
	public class Silent_destroyer : ModItem
	{
		private int consecutiveShots = 0;
        public override void SetDefaults()
        {
            Item.damage = 72;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 28;
            Item.height = 64;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 2.5f;
            Item.UseSound = null;
            Item.autoReuse = true;
            Item.shoot = ProjectileID.WoodenArrowFriendly;
            Item.shootSpeed = 16f;
            Item.useAmmo = AmmoID.Arrow;

            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ModContent.RarityType<jarkos.rare.StormRarity>();
        }
        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            type = ModContent.ProjectileType<Silent_arrow>();
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            consecutiveShots++;
            int mainProj = Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<Silent_arrow>(), damage, knockback, player.whoAmI, ai0: 0f);
            if (consecutiveShots >= 3)
            {
                consecutiveShots = 0;
                for (int i = -1; i <= 1; i += 2)
                {
                    Vector2 spreadVelocity = velocity.RotatedBy(MathHelper.ToRadians(12f * i)) * 0.85f;
                    Projectile.NewProjectile(source, position, spreadVelocity, ModContent.ProjectileType<Silent_arrow>(), (int)(damage * 0.6f), knockback * 0.5f, player.whoAmI, ai0: 1f);
                }
                for (int i = 0; i < 8; i++)
                {
                    Dust dust = Dust.NewDustDirect(position, 1, 1, DustID.Shadowflame);
                    dust.velocity = velocity.RotatedByRandom(0.5f) * 0.3f;
                    dust.noGravity = true;
                    dust.scale = 1.2f;
                }
            }
            return false;
        }
    }
}
