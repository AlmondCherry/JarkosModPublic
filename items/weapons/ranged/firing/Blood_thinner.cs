using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.projectiles.mecha;
namespace jarkos.items.weapons.ranged.firing
{
	public class Blood_thinner : ModItem
	{
		public override void SetDefaults()
        {
            Item.damage = 35;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.knockBack = 3f;
            Item.autoReuse = false;
            Item.shoot = ModContent.ProjectileType<Blood_thinner_Holdout>();
            Item.shootSpeed = 0f;
            Item.channel = true;
            Item.UseSound = null;

            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 10, 0);
        }
        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-45, 0);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, player.Center, Vector2.Zero, ModContent.ProjectileType<Blood_thinner_Holdout>(), damage, knockback, player.whoAmI);
            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.CrimtaneBar, 15)
                .AddIngredient(ItemID.TissueSample, 10)
                .AddTile(TileID.Anvils)
                .Register();
        }
	}
    public class BloodDissolverPlayer : ModPlayer
    {
        public int screenShakeTimer = 0;
        public override void PostUpdate()
        {
            if (screenShakeTimer > 0)
            {
                screenShakeTimer--;
            }
        }
        public override void ModifyScreenPosition()
        {
            if (screenShakeTimer > 0)
            {
                float intensity = screenShakeTimer * 1.5f;
                Main.screenPosition += new Vector2(Main.rand.NextFloat(-intensity, intensity), Main.rand.NextFloat(-intensity, intensity));
            }
        }
    }
}
