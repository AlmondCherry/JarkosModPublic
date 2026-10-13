using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.projectiles.magic.staffs;

namespace jarkos.items.weapons.magic.staffs
{
    public class Rainfire_staff : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.staff[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.DamageType = DamageClass.Magic;
            Item.width = 66;
            Item.height = 66;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.damage = 68;
            Item.noMelee = true;
            Item.scale = 1f;
            Item.mana = 5;
            Item.noUseGraphic = true;
            Item.UseSound = SoundID.Item43;
            Item.autoReuse = true;
            Item.channel = true;
            Item.shoot = ModContent.ProjectileType<Rainfire_staff_swing>();
            Item.shootSpeed = 16f;

            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(0, 1, 40, 0);
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
            .AddIngredient(ItemID.Flamelash, 1)
            .AddRecipeGroup(nameof(ItemID.TitaniumBar), 12)
            .AddIngredient(ItemID.SoulofLight, 4)
            .AddIngredient(ItemID.LightShard, 1)
            .AddTile(TileID.MythrilAnvil)
            .Register();
        }
    }
}
