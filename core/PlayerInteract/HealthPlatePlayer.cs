using jarkos.tiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.core.PlayerInteract
{
	public class HealthPlatePlayer : ModPlayer
	{
		public bool StandingOnHealthPlate = false;

		public override void ResetEffects()
		{
			StandingOnHealthPlate = false;
		}

		public override void PostUpdate()
		{
			CheckStandingOnHealthPlate();

			if (StandingOnHealthPlate)
			{
				if (Main.rand.NextBool(4))
				{
					Vector2 dustPos = new Vector2(Player.position.X + Main.rand.NextFloat(Player.width), Player.position.Y + Player.height - 4f);
					Dust d = Dust.NewDustPerfect(dustPos, DustID.RedTorch, new Vector2(0f, -1.2f), Scale: 1.1f);
					d.noGravity = true;
				}
			}
		}
		private void CheckStandingOnHealthPlate()
		{
			int minTileX = (int)(Player.position.X / 16f);
			int maxTileX = (int)((Player.position.X + Player.width) / 16f);
			int plateType = ModContent.TileType<HealthPlateTile>();

			int[] yOffsets = new int[]
			{
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
							StandingOnHealthPlate = true;
							return;
						}
					}
				}
			}
		}

		public override void UpdateLifeRegen()
		{
			if (!StandingOnHealthPlate) return;

			bool bossAlive = IsAnyBossAlive();

			float efficiencyMult = bossAlive ? 1.375f : 55.75f;
			int flatRegen = bossAlive ? 35 : 70;

			if (Player.lifeRegen > 0)
			{
				Player.lifeRegen = (int)(Player.lifeRegen * efficiencyMult);
			}

			Player.lifeRegen += flatRegen;
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
