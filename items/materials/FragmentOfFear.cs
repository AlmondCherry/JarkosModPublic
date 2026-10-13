using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;

namespace jarkos.items.materials
{
	public class FragmentOfFear : ModItem
	{
		public override void SetStaticDefaults()
		{
			Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(6, 6));
			ItemID.Sets.AnimatesAsSoul[Item.type] = true;
		}
        public override void SetDefaults()
		{
            Item.width = 40;
            Item.height = 40;
			Item.maxStack = Item.CommonMaxStack;
			Item.material = true;
			Item.value = Item.buyPrice(silver: 20);
		}
		public override void PostUpdate()
        {
            Lighting.AddLight(Item.Center, 1f, 1f, 1f);
        }
    }
}
