using jarkos.rare;
using Terraria;
using Terraria.ModLoader;

namespace jarkos.items.materials
{
    public class Vileplasm : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 22;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(0, 0, 15, 0);
            Item.rare = ModContent.RarityType<StormRarity>();
        }
    }
}
