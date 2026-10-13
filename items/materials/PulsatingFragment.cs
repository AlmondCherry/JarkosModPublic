using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;

namespace jarkos.items.materials
{
	public class PulsatingFragment : ModItem
	{
		public override void SetStaticDefaults()
		{
			Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(4, 5));
			ItemID.Sets.AnimatesAsSoul[Item.type] = true;
			ItemID.Sets.ItemNoGravity[Item.type] = true;
		}
        public override void SetDefaults()
		{
            Item.width = 16;
            Item.height = 16;
			Item.maxStack = Item.CommonMaxStack;
			Item.material = true;
			Item.value = Item.buyPrice(silver: 80);
		}
		public override void PostUpdate()
        {
            Lighting.AddLight(Item.Center, 1f, 0.8f, 0.35f);
        }
    }
}
