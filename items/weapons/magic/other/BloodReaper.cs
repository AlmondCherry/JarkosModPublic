using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.projectiles.magic.other;
namespace jarkos.items.weapons.magic.other
{
	public class BloodReaper : ModItem
	{
		public override void SetDefaults()
        {
            Item.damage = 62;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 18;
            Item.width = 42;
            Item.height = 42;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.knockBack = 5f;
            Item.UseSound = SoundID.Item1;
            Item.shoot = ModContent.ProjectileType<BloodReaperSwing>();
            Item.shootSpeed = 10f;
            Item.autoReuse = true;

            Item.rare = ItemRarityID.Yellow;
            Item.value = Item.sellPrice(0, 5, 40, 0);
        }
        private int swingDir = 1;
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            swingDir = -swingDir;

            int rTime = Main.rand.Next(18, 31);

            Item.useAnimation = rTime;
            Item.useTime = rTime + Main.rand.Next(3, 12);
            
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<BloodReaperSwing>(), damage, knockback, player.whoAmI, rTime, swingDir);
            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.SoulofNight, 12)
                .AddIngredient(ItemID.Ichor, 20)
                .AddIngredient(ItemID.DeathSickle, 1)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
	}
}
