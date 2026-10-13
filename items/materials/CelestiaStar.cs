using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using jarkos.rare;

namespace jarkos.items.materials
{
	public class CelestiaStar : ModItem
	{
		public override void SetStaticDefaults()
		{
			Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(3, 7));
			ItemID.Sets.AnimatesAsSoul[Item.type] = true;
		}
        public override void SetDefaults()
		{
            Item.width = 30;
            Item.height = 30;
			Item.maxStack = Item.CommonMaxStack;
			Item.material = true;
			Item.rare = ModContent.RarityType<CosmicRarity>();
			Item.value = Item.buyPrice(platinum: 10);
		}
    }
}
