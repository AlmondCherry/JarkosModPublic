using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;

namespace jarkos.items.materials
{
	public class BulbSunEnergy : ModItem
	{
		public override void SetStaticDefaults()
		{
			Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(3, 6));
			ItemID.Sets.AnimatesAsSoul[Item.type] = true;
		}
        public override void SetDefaults()
		{
            Item.width = 16;
            Item.height = 60;
			Item.maxStack = Item.CommonMaxStack;
			Item.material = true;
			Item.value = Item.buyPrice(gold: 5);
		}
    }
}
