using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.usable
{
	public class Bugs_bag : ModItem
	{
		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 10;
		}
		public override void SetDefaults() 
		{
			Item.width = 38;
			Item.height = 36;
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
            		player.QuickSpawnItem(EntitySource, ItemID.LadyBug, 1);
					player.QuickSpawnItem(EntitySource, ItemID.Maggot, 1);
					player.QuickSpawnItem(EntitySource, ItemID.Sluggy, 1);
					player.QuickSpawnItem(EntitySource, ItemID.Worm, 1);
					player.QuickSpawnItem(EntitySource, ItemID.WaterStrider, 1);
					player.QuickSpawnItem(EntitySource, ItemID.Firefly, 1);
					player.QuickSpawnItem(EntitySource, ItemID.Lavafly, 1);
					player.QuickSpawnItem(EntitySource, ItemID.UlyssesButterfly, 1);
					player.QuickSpawnItem(EntitySource, ItemID.JuliaButterfly, 1);
					player.QuickSpawnItem(EntitySource, ItemID.RedAdmiralButterfly, 1);
				}
				if (Main.rand.NextFloat() < .6000f)
				{
					player.QuickSpawnItem(EntitySource, ItemID.BlackDragonfly, 1);
					player.QuickSpawnItem(EntitySource, ItemID.BlueDragonfly, 1);
					player.QuickSpawnItem(EntitySource, ItemID.GreenDragonfly, 1);
					player.QuickSpawnItem(EntitySource, ItemID.OrangeDragonfly, 1);
					player.QuickSpawnItem(EntitySource, ItemID.RedDragonfly, 1);
					player.QuickSpawnItem(EntitySource, ItemID.YellowDragonfly, 1);
            	}
				if (Main.rand.NextFloat() < .4500f)
				{
            		player.QuickSpawnItem(EntitySource, ItemID.LightningBug, 1);
					player.QuickSpawnItem(EntitySource, ItemID.EnchantedNightcrawler, 1);
					player.QuickSpawnItem(EntitySource, ItemID.Buggy, 1);
					player.QuickSpawnItem(EntitySource, ItemID.MagmaSnail, 1);
					player.QuickSpawnItem(EntitySource, ItemID.PurpleEmperorButterfly, 1);
					player.QuickSpawnItem(EntitySource, ItemID.TreeNymphButterfly, 1);
            	}
				if (Main.rand.NextFloat() < .100f)
				{
            		player.QuickSpawnItem(EntitySource, ItemID.GoldButterfly, 1);
					player.QuickSpawnItem(EntitySource, ItemID.GoldDragonfly, 1);
					player.QuickSpawnItem(EntitySource, ItemID.GoldGrasshopper, 1);
					player.QuickSpawnItem(EntitySource, ItemID.GoldLadyBug, 1);
					player.QuickSpawnItem(EntitySource, ItemID.GoldWaterStrider, 1);
					player.QuickSpawnItem(EntitySource, ItemID.GoldWorm, 1);
            	}
				if (Main.rand.NextFloat() < .9500f)
				{
					player.QuickSpawnItem(EntitySource, ItemID.BlackScorpion, 1);
					player.QuickSpawnItem(EntitySource, ItemID.Grasshopper, 1);
					player.QuickSpawnItem(EntitySource, ItemID.Grubby, 1);
					player.QuickSpawnItem(EntitySource, ItemID.SulphurButterfly, 1);
					player.QuickSpawnItem(EntitySource, ItemID.MonarchButterfly, 1);
					player.QuickSpawnItem(EntitySource, ItemID.ZebraSwallowtailButterfly, 1);
					player.QuickSpawnItem(EntitySource, ItemID.HellButterfly, 1);
            	}
				
           		player.QuickSpawnItem(EntitySource, ItemID.Stinkbug, 2);
				player.QuickSpawnItem(EntitySource, ItemID.Snail, 1);
				player.QuickSpawnItem(EntitySource, ItemID.Scorpion, 1);
			}
		}
	}
