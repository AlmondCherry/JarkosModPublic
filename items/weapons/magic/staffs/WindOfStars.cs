using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Jarkos.projectiles.magic.staffs;
using jarkos.items.materials;

namespace jarkos.items.weapons.magic.staffs
{
    public class WindOfStars : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.staff[Item.type] = true;
        }
        public override void SetDefaults()
        {
            Item.damage = 58;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 16;
            Item.width = 50;
            Item.height = 50;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 4.5f;
            Item.UseSound = SoundID.Item9;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<Hellish_star>();
            Item.shootSpeed = 12f;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 1, 10, 0);
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.SpellTome, 1);
            recipe.AddIngredient(ModContent.ItemType<ElementalPowder>(), 2);
            recipe.AddTile(TileID.Bookcases);
            recipe.Register();
        }
    }
}
