using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.projectiles.magic.toms;
using jarkos.items.usetiles;

namespace Jarkos.items.weapons.magic.toms
{
	public class GrimoireOfClots : ModItem
	{
		public override void SetDefaults()
        {
            Item.damage = 40;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 12;
            Item.width = 28;
            Item.height = 32;
            Item.useTime = 25;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 5f;
            Item.UseSound = SoundID.Item21 with { Pitch = -0.3f };
            Item.shoot = ModContent.ProjectileType<BloodClot_tome_proj>();
            Item.shootSpeed = 9f;
            Item.autoReuse = true;

            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 20, 0);
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Book, 1)
                .AddIngredient(ModContent.ItemType<BloodyAlloy>(), 8)
                .AddRecipeGroup(nameof(ItemID.TissueSample), 12)
                .AddTile(TileID.Bookcases)
                .Register();
        }
    }
}
