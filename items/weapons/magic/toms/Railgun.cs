using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Jarkos.projectiles.mecha;
using jarkos.items.materials;
using jarkos.items.usetiles;

namespace Jarkos.items.weapons.magic.toms
{
	public class Railgun : ModItem
	{
        public override void SetDefaults()
        {
            Item.width = 62;
            Item.height = 22;
            Item.damage = 275;
            Item.DamageType = DamageClass.Magic;
            Item.useTime = 45;
            Item.useAnimation = 45;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 8f;
            Item.UseSound = null;
            Item.autoReuse = false;
            Item.channel = true;
            Item.shoot = ModContent.ProjectileType<Railgun_charge>();
            Item.shootSpeed = 30f;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 1, 80, 0);
        }
        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-45, 0);
        }
		public override void AddRecipes() {
		CreateRecipe(1)
        .AddIngredient(ItemID.HallowedBar, 12)
		.AddIngredient(ModContent.ItemType<GalvanicAlloy>(), 6)
		.AddIngredient(ModContent.ItemType<PulsatingFragment>(), 2)
		.AddTile(TileID.MythrilAnvil)
		.Register();
		}
	}
}
