using jarkos.GameplayChanges;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace jarkos.items.accs.special
{
	public class MinerAccessory : ModItem
	{
		public override void SetDefaults()
		{
			Item.width = 40;
			Item.height = 48;
			Item.accessory = true;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 50);
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.luck += 0.50f;
			Item item = player.inventory[player.selectedItem];

      		if ((item.hammer > 0 
			|| item.axe > 0 
			|| item.pick > 0 
			|| item.createTile >= TileID.Dirt 
			|| item.createWall >= 0) 
			&& !item.channel 
			&& (Main.tile[Player.tileTargetX, Player.tileTargetY].TileType != TileID.Extractinator))

        	player.itemTime = 1;
      		Player.tileRangeX = 20;
      		Player.tileRangeY = 20;

			var veinPlayer = player.GetModPlayer<VeinMinerPlayer>();
			if (veinPlayer != null)
			{
				veinPlayer.minerAccessoryEquipped = true;
			}
		}
        public override void AddRecipes()
		{
			CreateRecipe(1)
			.AddIngredient(ItemID.MiningPotion, 2)
			.AddIngredient(ItemID.AncientChisel, 1)
			.AddTile(TileID.Anvils)
			.Register();
		}
	}
}
