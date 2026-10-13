using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;

namespace jarkos.items.accs.fishing
{
	public class OtherworldlyPlea : ModItem
	{
		public override void SetStaticDefaults()
		{
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(4, 5));
			ItemID.Sets.AnimatesAsSoul[Item.type] = true;
		}
		public override void SetDefaults()
		{
			Item.width = 50;
			Item.height = 84;
			Item.accessory = true;
			Item.rare = ItemRarityID.Green;
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			float missing_hp_ratio = 1f - (player.statLife / (float)player.statLifeMax2);
			float final_def_bonus = 20f * missing_hp_ratio;
			player.statDefense += (int)final_def_bonus;

			if(player.statLife < player.statLifeMax2 / 2)
			{
				player.endurance += 0.20f;
			}
		}
	}
}
