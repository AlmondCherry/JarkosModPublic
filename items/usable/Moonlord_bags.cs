using jarkos.items.materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.items.usable
{
	public class Moonlord_bags : ModItem
	{
		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 10;
		}
		public override void SetDefaults()
		{
			Item.width = 70;
			Item.height = 54;
			Item.maxStack = 9999;
		}
		public override bool CanRightClick()
		{
			return true;
		}
		public override void RightClick(Player player)
		{
				var EntitySource = player.GetSource_OpenItem(Type);

            	player.QuickSpawnItem(EntitySource, ItemID.FragmentNebula, Main.rand.Next(12, 36));
				player.QuickSpawnItem(EntitySource, ItemID.FragmentSolar, Main.rand.Next(12, 36));
				player.QuickSpawnItem(EntitySource, ItemID.FragmentVortex, Main.rand.Next(12, 36));
				player.QuickSpawnItem(EntitySource, ItemID.FragmentStardust, Main.rand.Next(12, 36));

				if(NPC.downedMoonlord)
				{
					player.QuickSpawnItem(EntitySource, ItemID.LunarOre, Main.rand.Next(70, 420));
					player.QuickSpawnItem(EntitySource, ModContent.ItemType<ElementalFluid>(), Main.rand.Next(1, 10));
				}
			}
		}
	}
