using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using jarkos.projectiles.melee.other_melee;
using Microsoft.Xna.Framework;

namespace jarkos.items.weapons.melee.other_melee
{
	public class Fusion_fist : ModItem
	{
		public override void SetDefaults()
        {
            Item.damage = 85;
            Item.DamageType = DamageClass.Melee;
            Item.width = 44;
            Item.height = 44;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 9f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<Fusion_fist_proj>();
            Item.shootSpeed = 18f;
            Item.noMelee = true;
            Item.noUseGraphic = true;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 1, 10, 0);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 dir = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
            float dist = Vector2.Distance(player.Center, Main.MouseWorld);
            float range = MathHelper.Clamp(dist, 200f, 600f);

            Projectile.NewProjectile(source, player.Center, dir, type, damage, knockback, player.whoAmI, 0f, range);
            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ItemID.HellstoneBar, 20)
            .AddIngredient(ItemID.Chain, 15)
            .AddIngredient(ItemID.SoulofMight, 10)
            .AddTile(TileID.MythrilAnvil)
            .Register();
        }
    }
}
