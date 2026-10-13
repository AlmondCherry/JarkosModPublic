using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.materials
{
	public class EnchantedFragment : ModItem
	{
		public override void SetStaticDefaults()
		{
			ItemID.Sets.ItemIconPulse[Item.type] = true;
			ItemID.Sets.ItemNoGravity[Item.type] = true;
			Item.ResearchUnlockCount = 25;
		}
		public override void SetDefaults()
		{
			Item.width = 38;
			Item.height = 38;
			Item.maxStack = Item.CommonMaxStack;
		}
		public override void PostUpdate()
        {
            Lighting.AddLight(Item.Center, 0.95f, 0.2f, 1f);
        }
	}
}
