using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;

namespace jarkos.items.materials
{
	public class FragmentOfNature : ModItem
	{
		public override void SetStaticDefaults()
		{
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 6));
			ItemID.Sets.AnimatesAsSoul[Item.type] = true;
			ItemID.Sets.ItemIconPulse[Item.type] = true;
		}
        public override void SetDefaults()
		{
            Item.width = 30;
            Item.height = 48;
			Item.maxStack = Item.CommonMaxStack;
			Item.material = true;
			Item.value = Item.buyPrice(silver: 3);
		}
		public override void PostUpdate()
        {
            Lighting.AddLight(Item.Center, 1f, 1f, 1f);
        }
	}
}
