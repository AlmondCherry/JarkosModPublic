using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using jarkos.items.usetiles;

namespace jarkos.items.accs.summon
{
	public class RadiantGenerator : ModItem
	{
		public override void SetStaticDefaults()
		{
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 10));
			ItemID.Sets.AnimatesAsSoul[Item.type] = true;
		}
		public override void SetDefaults()
		{
			Item.width = 40;
			Item.height = 64;
			Item.accessory = true;
			Item.rare = ItemRarityID.Yellow;
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			var modPl = player.GetModPlayer<Jarkos_player>();
			modPl.Radiant_Generator = true;
			player.maxMinions += 1;
		}
		public override bool CanEquipAccessory(Player player, int slot, bool modded)
		{
			var jarkosplayer = player.GetModPlayer<Jarkos_player>();
			
			if (!jarkosplayer.nature_dar && !jarkosplayer.Evil_Gods_Parchment_equip)
			{
				return true;
			}
			return false;
		}
		public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ModContent.ItemType<GalvanicAlloy>(), 2)
			.AddIngredient(ItemID.Ectoplasm, 8)
			.AddTile(TileID.MythrilAnvil)
			.Register();
		}
	}
}
