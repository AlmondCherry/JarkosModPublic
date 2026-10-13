using System;
using jarkos.tiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.core.PlayerInteract
{
	public class ManaPlatePlayer : ModPlayer
	{
		public bool StandingOnManaPlate = false;
		public int ManaTimer = 0;

		public override void ResetEffects()
		{
			StandingOnManaPlate = false;
		}

		public override void PostUpdate()
		{
			CheckStandingOnManaPlate();

			if (StandingOnManaPlate)
			{
				if (Main.rand.NextBool(4))
				{
					Vector2 dustPos = new Vector2(Player.position.X + Main.rand.NextFloat(Player.width), Player.position.Y + Player.height - 4f);
					Dust d = Dust.NewDustPerfect(dustPos, DustID.ManaRegeneration, new Vector2(0f, -1.2f), Scale: 1.1f);
					d.noGravity = true;
				}

				bool bossAlive = IsAnyBossAlive();
				int interval = bossAlive ? 180 : 60;

				ManaTimer++;
				if (ManaTimer >= interval)
				{
					ManaTimer = 0;
					if (Player.statMana < Player.statManaMax2)
					{
						int restoreAmount = 20;
						Player.statMana = Math.Min(Player.statManaMax2, Player.statMana + restoreAmount);
						Player.ManaEffect(restoreAmount);
						if (Player.whoAmI == Main.myPlayer)
						{
							SoundEngine.PlaySound(SoundID.MaxMana with { Volume = 0.4f, Pitch = 0.2f });
						}
					}
				}
			}
			else
			{
				ManaTimer = 0;
			}
		}

		private void CheckStandingOnManaPlate()
		{
			int minTileX = (int)(Player.position.X / 16f);
			int maxTileX = (int)((Player.position.X + Player.width) / 16f);
			int plateType = ModContent.TileType<ManaPlateTile>();

			int[] yOffsets = new int[] {
				(int)((Player.position.Y + Player.height + 2f) / 16f),
				(int)((Player.position.Y + Player.height - 2f) / 16f)
			};

			foreach (int tileY in yOffsets)
			{
				for (int x = minTileX; x <= maxTileX; x++)
				{
					if (x >= 0 && x < Main.maxTilesX && tileY >= 0 && tileY < Main.maxTilesY)
					{
						Tile tile = Main.tile[x, tileY];
						if (tile.HasTile && tile.TileType == plateType)
						{
							StandingOnManaPlate = true;
							return;
						}
					}
				}
			}
		}

		private bool IsAnyBossAlive()
		{
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC npc = Main.npc[i];
				if (npc.active && (npc.boss || npc.type == NPCID.EaterofWorldsHead))
				{
					return true;
				}
			}
			return false;
		}
	}
}
