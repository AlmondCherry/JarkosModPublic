using Terraria;
using Terraria.ModLoader;

namespace jarkos.items.materials
{
	public class RemainsOfMoth : ModItem
	{
        public override void SetDefaults()
		{
            Item.width = 32;
            Item.height = 32;
			Item.maxStack = Item.CommonMaxStack;
			Item.material = true;
			Item.value = Item.buyPrice(silver: 5);
		}
    }
}
