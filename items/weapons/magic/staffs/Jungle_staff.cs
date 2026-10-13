using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.items.weapons.magic.staffs
{
    public class Jungle_staff : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.staff[Item.type] = true;
        }
        public override void SetDefaults()
        {
            Item.useTime = 2;
            Item.DamageType = DamageClass.Magic;
            Item.width = 39;
            Item.height = 41;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = 4;
            Item.reuseDelay = 26;
            Item.damage = 8;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.scale = 1f;
            Item.mana = 4;
            Item.UseSound = SoundID.Item43;
            Item.autoReuse = false;
            Item.shoot = ProjectileID.Leaf;
            Item.shootSpeed = 18f;

            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 0, 10, 0);
        }
        public override void AddRecipes() {
			CreateRecipe(1)
			.AddIngredient(ItemID.Vine, 3)
			.AddIngredient(ItemID.JungleSpores, 12)
			.AddTile(TileID.Anvils)
			.Register();
		}
    }
}
