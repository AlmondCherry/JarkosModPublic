using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.projectiles;

namespace jarkos.items.bags
{
    public class Celestia_bag : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 52;
            Item.height = 42;
            Item.maxStack = 20;
            Item.useAnimation = 45;
            Item.useTime = 45;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.noUseGraphic = true;
            Item.expert = true;
            Item.shoot = ModContent.ProjectileType<Celestia_bag_proj>();
			Item.shootSpeed = 20f;
        }
    }
}
