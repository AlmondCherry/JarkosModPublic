using Terraria;
using Terraria.ModLoader;
namespace Jarkos.items.materials
{
	public class DeliciousBait : ModItem
	{
        public override void SetDefaults()
		{
            Item.width = 24;
            Item.height = 26;
			Item.maxStack = Item.CommonMaxStack;
			Item.material = true;
		}
	}
}
