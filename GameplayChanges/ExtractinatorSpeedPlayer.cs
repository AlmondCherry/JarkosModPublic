using System;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.GameplayChanges
{
	public class ExtractinatorSpeedPlayer : ModPlayer
	{
		private static readonly MethodInfo _extractinatorUseMethod = typeof(Player).GetMethod("ExtractinatorUse", BindingFlags.NonPublic | BindingFlags.Instance);

		public static void CallExtractinatorUse(Player player, int extractType, int extractinatorBlockType)
		{
			_extractinatorUseMethod?.Invoke(player, new object[] { extractType, extractinatorBlockType });
		}

		public override void PostUpdate()
		{
			bool instantStack = Jarkos_config.Instance?.ExtractStackInstantly ?? true;
			bool fastSpeed = Jarkos_config.Instance?.FastExtractinator ?? true;

			if (!instantStack && !fastSpeed)
				return;

			if (Player.tileTargetX < 0 || Player.tileTargetX >= Main.maxTilesX || Player.tileTargetY < 0 || Player.tileTargetY >= Main.maxTilesY)
				return;

			Tile tile = Main.tile[Player.tileTargetX, Player.tileTargetY];
			if (tile != null && tile.HasTile && (tile.TileType == TileID.Extractinator || tile.TileType == TileID.ChlorophyteExtractinator))
			{
				Item heldItem = Player.inventory[Player.selectedItem];
				if (heldItem != null && !heldItem.IsAir && IsExtractable(heldItem))
				{
					if (Player.itemTime > 0)
					{
						if (instantStack)
						{
							int extractType = ItemID.Sets.ExtractinatorMode[heldItem.type];
							if (extractType < 0) extractType = heldItem.type;

							int count = heldItem.stack;
							heldItem.TurnToAir();

							for (int k = 0; k < count; k++)
							{
								CallExtractinatorUse(Player, extractType, tile.TileType);
							}

							Player.itemTime = 12;
							Player.itemAnimation = 12;
						}
						else if (fastSpeed)
						{
							if (Player.itemTime > 1)
							{
								Player.itemTime = Math.Max(1, Player.itemTime / 10);
							}
							if (Player.itemAnimation > 1)
							{
								Player.itemAnimation = Math.Max(1, Player.itemAnimation / 10);
							}
						}
					}
				}
			}
		}

		public static bool IsExtractable(Item item)
		{
			if (item == null || item.IsAir) return false;

			if (item.type < ItemID.Sets.ExtractinatorMode.Length && ItemID.Sets.ExtractinatorMode[item.type] >= 0)
				return true;

			switch (item.type)
			{
				case ItemID.SiltBlock:
				case ItemID.SlushBlock:
				case ItemID.DesertFossil:
					return true;
				default:
					return false;
			}
		}
	}

	public class ExtractinatorGlobalTile : GlobalTile
	{
		public override void RightClick(int i, int j, int type)
		{
			if (type != TileID.Extractinator && type != TileID.ChlorophyteExtractinator)
				return;

			if (Jarkos_config.Instance != null && !Jarkos_config.Instance.ExtractStackInstantly)
				return;

			Player player = Main.LocalPlayer;
			if (player == null || !player.active || player.dead)
				return;

			Item heldItem = player.inventory[player.selectedItem];
			if (heldItem == null || heldItem.IsAir || !ExtractinatorSpeedPlayer.IsExtractable(heldItem))
				return;

			int extractType = ItemID.Sets.ExtractinatorMode[heldItem.type];
			if (extractType < 0) extractType = heldItem.type;

			int count = heldItem.stack;
			heldItem.TurnToAir();

			for (int k = 0; k < count; k++)
			{
				ExtractinatorSpeedPlayer.CallExtractinatorUse(player, extractType, type);
			}

			player.itemTime = 12;
			player.itemAnimation = 12;
		}
	}
}
