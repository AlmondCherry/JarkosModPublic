using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.projectiles.melee.true_melee;

namespace jarkos.items.weapons.melee.true_melee
{
	public class Doppelganger : ModItem
	{
        public override void SetDefaults()
        {
            Item.width = 72;
            Item.height = 80;
            Item.damage = 14;
            Item.DamageType = DamageClass.Melee;
            Item.knockBack = 5f;
            Item.crit = 4;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = true;
            Item.UseSound = null;
            Item.shoot = ModContent.ProjectileType<DopengalgerSwing>();
            Item.shootSpeed = 1f;

            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 5, 0);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float swingDir = player.GetModPlayer<DopengalgerPlayer>().NextSwingDirection();

            Projectile.NewProjectile(source, player.MountedCenter, velocity, type, damage, knockback, player.whoAmI, ai0: swingDir);
            
            int emberCount = Main.rand.Next(2, 4);
            Vector2 dir = velocity.SafeNormalize(Vector2.UnitX * player.direction);

            for (int i = 0; i < emberCount; i++)
            {
                Vector2 emberVel = dir.RotatedByRandom(MathHelper.ToRadians(20f)) * Main.rand.NextFloat(7f, 9f);
                emberVel.Y -= Main.rand.NextFloat(1f, 2.5f);
                Projectile.NewProjectile(source, player.MountedCenter + dir * 40f, emberVel, ModContent.ProjectileType<CosmicEmber>(), (int)(damage * 0.5f), 1.5f, player.whoAmI);
            }
            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.FallenStar, 5)
                .AddIngredient(ItemID.IronBar, 8)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
    public class DopengalgerPlayer : ModPlayer
    {
        private bool swingUp;
        public float NextSwingDirection()
        {
            swingUp = !swingUp;
            return swingUp ? 1f : 0f;
        }
	}
}
