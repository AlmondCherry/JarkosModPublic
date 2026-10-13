using jarkos.projectiles.melee.other_melee;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.items.weapons.melee.other_melee
{
    public class BloodLettingBoomerang : ModItem
    {
        public override void SetStaticDefaults()
        {
            Terraria.GameContent.Creative.CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }
        public override void SetDefaults()
        {
            Item.damage = 14;
            Item.DamageType = DamageClass.Melee;
            Item.width = 16;
            Item.height = 55;
            Item.useTime = 10;
            Item.useAnimation = 10;
            Item.reuseDelay = 20;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 3f;
            Item.shootSpeed = 20;
            Item.shoot = ModContent.ProjectileType<Bloodletting_blade_proj>();
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;

            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 30, 0);
        }
        public override void AddRecipes()
		{
    		CreateRecipe()
            .AddRecipeGroup(nameof(ItemID.IronBar), 4)
        	.AddRecipeGroup(nameof(ItemID.TissueSample), 16)
            .AddIngredient(ItemID.CrimtaneBar, 8)
            .AddTile(TileID.Anvils)
        	.Register();
		}
    }
}
