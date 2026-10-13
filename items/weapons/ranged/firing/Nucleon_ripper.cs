using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using jarkos.projectiles.mecha;
using Terraria.Audio;
using Jarkos.items.materials;
using Jarkos.items.weapons.ranged.firing;
namespace jarkos.items.weapons.ranged.firing
{
	public class Nucleon_ripper : ModItem
	{
        public override void SetDefaults()
        {
            Item.DamageType = DamageClass.Ranged;
            Item.damage = 274;
            Item.width = 40;
            Item.height = 60;
            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 3;
            Item.noMelee = true;
            Item.shootSpeed = 24;
            Item.shoot = ModContent.ProjectileType<Nucleon_ripper_Holdout>();
            Item.autoReuse = true;
            Item.channel = true;
            Item.noUseGraphic = true;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 0, 30, 0);
        }
        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-35f, -20f);
        }
		public override void AddRecipes()
        {
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<Plasma_wave>(), 1)
            .AddIngredient(ModContent.ItemType<DiffusalBar>(), 4)
            .AddIngredient(ItemID.Glass, 80)
            .AddIngredient(ItemID.Wire, 30)
            .AddTile(TileID.MythrilAnvil)
            .Register();
        }
	}
}
