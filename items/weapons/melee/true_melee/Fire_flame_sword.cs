using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.projectiles.melee.true_melee;
using jarkos.items.materials;

namespace jarkos.items.weapons.melee.true_melee
{
	public class Fire_flame_sword : ModItem
	{
        public override void SetDefaults()
        {
            Item.width = 62;
            Item.height = 64;
            Item.damage = 32;
            Item.DamageType = DamageClass.Melee;
            Item.knockBack = 6.5f;
            Item.crit = 6;
            Item.useTime = 28;
            Item.useAnimation = 28;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(gold: 1, silver: 50);
            Item.UseSound = null;
            Item.shoot = ModContent.ProjectileType<FireSwordSwing>();
            Item.shootSpeed = 1f;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int comboSwings = 0;
            if (Main.rand.NextBool(10))
                comboSwings = Main.rand.Next(3, 7);

            Projectile.NewProjectile(source, player.MountedCenter, velocity.SafeNormalize(Vector2.UnitX), type, damage, knockback, player.whoAmI, ai0: 0f, ai1: comboSwings);
            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.HellstoneBar, 12)
                .AddIngredient(ItemID.Obsidian, 40)
                .AddIngredient(ModContent.ItemType<FragmentOfFear>(), 8)
                .AddTile(TileID.Hellforge)
                .Register();
        }
    }
}
