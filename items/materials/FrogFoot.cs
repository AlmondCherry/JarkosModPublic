using Terraria;
using Terraria.ModLoader;

namespace Jarkos.items.materials
{
	public class FrogFoot : ModItem
	{
        public override void SetDefaults()
		{
            Item.width = 40;
            Item.height = 40;
			Item.maxStack = Item.CommonMaxStack;
			Item.material = true;
			Item.value = Item.buyPrice(silver: 5);
		}
	}
}
