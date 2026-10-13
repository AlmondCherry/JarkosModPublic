using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.materials
{
	public class BloodDroplet : ModItem
	{
		public override void SetStaticDefaults()
		{
			Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(4, 7));
			ItemID.Sets.AnimatesAsSoul[Item.type] = true;
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults()
		{
			Item.width = 14;
			Item.height = 15;
			Item.maxStack = Item.CommonMaxStack;
			Item.material = true;
			Item.value = Item.buyPrice(silver: 5);
		}
		public override void PostUpdate()
        {
            Lighting.AddLight(Item.Center, 1f, 1f, 1f);
        }
	}
}
