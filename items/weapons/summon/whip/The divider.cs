using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.projectiles.summon.whip;
using jarkos.items.usetiles;
using jarkos.items.materials;

namespace jarkos.items.weapons.summon.whip
{
	public class The_divider : ModItem
	{
		public override void SetDefaults()
        {
            Item.damage = 55;
            Item.DamageType = DamageClass.SummonMeleeSpeed;
            Item.width = 30;
            Item.height = 30;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 3f;
            Item.shoot = ModContent.ProjectileType<Whip_divider_proj>();
            Item.shootSpeed = 5f;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = false;
            Item.UseSound = SoundID.Item152;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 1, 20, 0);
        }
        public override bool MeleePrefix() => true;
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<GalvanicAlloy>(), 4)
                .AddIngredient(ModContent.ItemType<Whip_of_energy>(), 1)
                .AddIngredient(ModContent.ItemType<ElementalPowder>(), 2)
                .AddTile(TileID.MythrilAnvil)
                .Register();
        }
	}
}
