using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.items.usetiles;

namespace jarkos.items.weapons.melee.other_melee
{
    public class NoxtiliumKnife : ModItem
    {
        public override void SetDefaults() {
            Item.width = 32;
            Item.height = 32;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useAnimation = 15;
            Item.useTime = 15;
            Item.useTurn = true;
            Item.damage = 32;
            Item.DamageType = DamageClass.Melee;
            Item.knockBack = 3f;
            Item.autoReuse = true;
            Item.UseSound = SoundID.Item1;

            Item.value = Item.sellPrice(0, 1, 0, 0);
            Item.rare = ItemRarityID.LightRed;
        }
        public override void AddRecipes() {
            CreateRecipe()
            .AddIngredient(ModContent.ItemType<NoxtiliumBar>(), 6)
            .AddTile(TileID.Anvils)
            .Register();
        }
    }
}
