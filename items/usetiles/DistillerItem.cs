using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using jarkos.tiles;

namespace jarkos.items.usetiles
{
    public class DistillerItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.maxStack = 99;
            Item.useTurn = true;
            Item.autoReuse = false;
            Item.useAnimation = 60;
            Item.useTime = 60;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<DistillerTile>();
            Item.rare = ItemRarityID.Cyan;
            Item.value = Item.buyPrice(silver: 50);
        }
    }
}
