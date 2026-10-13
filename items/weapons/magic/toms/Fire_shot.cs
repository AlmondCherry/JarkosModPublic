using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.projectiles.magic.toms;
using Terraria.Audio;

namespace jarkos.items.weapons.magic.toms
{
	public class Fire_shot : ModItem
	{
		public override void SetStaticDefaults()
        {
            Item.staff[Item.type] = true;
        }
        public override void SetDefaults()
        {
            Item.damage = 65;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 4;
            Item.width = 52;
            Item.height = 52;
            Item.useTime = 12;
            Item.useAnimation = 48;
            Item.reuseDelay = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 6f;
            Item.UseSound = SoundID.Item73;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<Fire_shot_proj>();
            Item.shootSpeed = 14f;

            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(0, 0, 30, 0);
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.Book, 1);
            recipe.AddIngredient(ItemID.HellstoneBar, 12);
            recipe.AddIngredient(ItemID.Fireblossom, 4);
            recipe.AddTile(TileID.Bookcases);
            recipe.Register();
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int shotIndex = 4 - (player.itemAnimation / Item.useTime);
            if (shotIndex < 0) shotIndex = 0;
            if (shotIndex > 3) shotIndex = 3;
            float angleOffset = 0f;
            if (shotIndex == 0) angleOffset = 25f;
            else if (shotIndex == 1) angleOffset = 8f;
            else if (shotIndex == 2) angleOffset = -8f;
            else if (shotIndex == 3) angleOffset = -25f;
            SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/hotshot_shot") with { Pitch = Main.rand.NextFloat(-0.25f, 0.25f) }, position);
            Vector2 perturbedSpeed = velocity.RotatedBy(MathHelper.ToRadians(angleOffset));
            Projectile.NewProjectile(source, position, perturbedSpeed, type, damage, knockback, player.whoAmI);
            return false;
        }
    }
}
