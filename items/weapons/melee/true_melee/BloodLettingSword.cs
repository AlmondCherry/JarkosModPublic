using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.projectiles.melee.true_melee;
using jarkos.items.usetiles;
using jarkos.items.materials;

namespace jarkos.items.weapons.melee.true_melee
{
	public class BloodLettingSword : ModItem
	{
		public override void SetDefaults()
        {
            Item.width = 60;
            Item.height = 60;
            Item.damage = 34;
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 42;
            Item.useAnimation = 42;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 5f;
            Item.UseSound = null;
            Item.autoReuse = true;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.shootSpeed = 1f;
            Item.shoot = ModContent.ProjectileType<Swing_bloodletting_sword>();
            Item.channel = false;

            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 30, 0);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, player.Center, Vector2.Zero, type, damage, knockback, player.whoAmI);
            return false;
        }
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            Lighting.AddLight(player.Center, 0.8f, 0.1f, 0.1f);
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<BloodyAlloy>(), 10);
            recipe.AddIngredient(ModContent.ItemType<FragmentOfFear>(), 12);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
	}
}
