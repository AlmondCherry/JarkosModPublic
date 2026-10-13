using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.items.materials;
using Jarkos.items.weapons.ranged.firing;

namespace jarkos.items.weapons.ranged.firing
{
	public class Cafas : ModItem
	{
		public override void SetStaticDefaults()
		{
			Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(4, 8));
			ItemID.Sets.AnimatesAsSoul[Item.type] = true;
			ItemID.Sets.ItemNoGravity[Item.type] = true;
		}
		public override void SetDefaults()
		{
			Item.damage = 107;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 58;
			Item.height = 56;
			Item.useTime = 6;
			Item.useAnimation = 6;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.autoReuse = true;
            Item.channel = true;
			Item.shoot = ModContent.ProjectileType<jarkos.projectiles.ranged.firearm.Cafas_holdout>();
			Item.shootSpeed = 30f;

			Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 2, 0, 0);
		}
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<jarkos.projectiles.ranged.firearm.Cafas_holdout>(), damage, knockback, player.whoAmI);
            return false;
        }
        public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<Electric_arrestor>(), 1)
			.AddIngredient(ModContent.ItemType<FortifiedPlate>(), 8)
			.AddIngredient(ModContent.ItemType<ThickSilk>(), 2)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
    }
}
