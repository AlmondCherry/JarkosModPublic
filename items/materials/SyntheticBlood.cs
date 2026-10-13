using Terraria;
using Terraria.ModLoader;

namespace jarkos.items.materials
{
	public class SyntheticBlood : ModItem
	{
		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults() 
		{
			Item.width = 24;
			Item.height = 34;
			Item.maxStack = Item.CommonMaxStack;
			Item.value = Item.buyPrice(silver: 1);
		}
		public override void PostUpdate()
        {
            Lighting.AddLight(Item.Center, 1f, 1f, 1f);
        }
	}
}
