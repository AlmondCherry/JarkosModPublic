using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.usable
{
	public class Fruit_bag : ModItem
	{
		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 10;
		}
		public override void SetDefaults()
		{
			Item.width = 38;
			Item.height = 42;
			Item.maxStack = 9999;
		}
		public override bool CanRightClick()
		{
			return true;
		}
		public override void RightClick(Player player)
		{
				var EntitySource = player.GetSource_OpenItem(Type);
            	if (Main.rand.NextFloat() < .9000f)
				{
            		player.QuickSpawnItem(EntitySource, ItemID.Cherry, 2);
					player.QuickSpawnItem(EntitySource, ItemID.Plum, 2);
					player.QuickSpawnItem(EntitySource, ItemID.Pineapple, 1);
            	}
				if (Main.rand.NextFloat() < .6000f)
				{
            		player.QuickSpawnItem(EntitySource, ItemID.Mango, 2);
					player.QuickSpawnItem(EntitySource, ItemID.Banana, 2);
					player.QuickSpawnItem(EntitySource, ItemID.Coconut, 1);
            	}
				if (Main.rand.NextFloat() < .3000f)
				{
            		player.QuickSpawnItem(EntitySource, ItemID.Elderberry, 2);
					player.QuickSpawnItem(EntitySource, ItemID.BlackCurrant, 2);
					player.QuickSpawnItem(EntitySource, ItemID.Rambutan, 1);
            	}
				if (Main.rand.NextFloat() < .1500f)
				{
            		player.QuickSpawnItem(EntitySource, ItemID.BloodOrange, 2);
					player.QuickSpawnItem(EntitySource, ItemID.Dragonfruit, 2);
					player.QuickSpawnItem(EntitySource, ItemID.Starfruit, 1);
            	}
				
           		player.QuickSpawnItem(EntitySource, ItemID.Apple, 2);
				player.QuickSpawnItem(EntitySource, ItemID.Lemon, 2);
				player.QuickSpawnItem(EntitySource, ItemID.Apricot, 1);
				player.QuickSpawnItem(EntitySource, ItemID.Grapefruit, 1);
				player.QuickSpawnItem(EntitySource, ItemID.Peach, 1);
			}
		}
	}
