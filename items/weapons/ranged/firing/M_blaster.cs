using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Jarkos.projectiles.mecha;
using jarkos.items.usetiles;
using Jarkos.items.materials;
using jarkos.items.materials;

namespace Jarkos.items.weapons.ranged.firing
{
	public class M_blaster : ModItem
	{
		public override void SetDefaults()
        {
            Item.damage = 42;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 52;
            Item.height = 28;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.scale = 0.5f;
            Item.knockBack = 1.5f;
            Item.UseSound = SoundID.Item75;
            Item.autoReuse = true;
            Item.channel = true;
            Item.noUseGraphic = true;
            Item.shoot = ModContent.ProjectileType<M_blaster_Holdout>();
            Item.shootSpeed = 16f;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 1, 0, 0);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            return true;
        }
        public override void AddRecipes() {
		CreateRecipe(1)
		.AddIngredient(ItemID.SpaceGun, 1)
		.AddIngredient(ModContent.ItemType<GalvanicAlloy>(), 8)
		.AddIngredient(ModContent.ItemType<ThickSilk>(), 2)
		.AddTile(TileID.MythrilAnvil)
		.Register();
		}
	}
}
