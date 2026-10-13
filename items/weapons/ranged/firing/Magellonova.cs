using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.projectiles.mecha;
using Terraria.Audio;
using jarkos.items.materials;
using jarkos.items.usetiles;
namespace jarkos.items.weapons.ranged.firing
{
	public class Magellonova : ModItem
	{
		public override void SetDefaults()
        {
            Item.width = 70;
            Item.height = 24;
            Item.damage = 1550;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 90;
            Item.useAnimation = 90;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.rare = ModContent.RarityType<jarkos.rare.SpeedRarity>();
            Item.channel = true;
            Item.knockBack = 8f;
            Item.UseSound = null;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<Magellonova_Holdout>();
            Item.shootSpeed = 20f;
            Item.crit = 25;

            Item.rare = ItemRarityID.Red;
            Item.value = Item.sellPrice(0, 50, 0, 0);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<Magellonova_Holdout>(), damage, knockback, player.whoAmI);
            return false;
        }
        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-55, 0);
        }
		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<Nova>(), 1)
			.AddIngredient(ModContent.ItemType<NitoziumPlate>(), 2)
			.AddIngredient(ModContent.ItemType<AntiqueBar>(), 12)
			.AddTile(TileID.LunarCraftingStation)
			.Register();
		}
	}
}
