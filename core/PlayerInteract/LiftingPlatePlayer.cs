using jarkos.tiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace jarkos.core.PlayerInteract
{
	public class LiftingPlatePlayer : ModPlayer
	{
		public bool StandingOnLiftingPlate = false;
		public int StepTimer = 0;
		public bool HasTriggeredLaunch = false;
		public int LevitationTimer = 0;

		public override void ResetEffects()
		{
			StandingOnLiftingPlate = false;
		}

		public override void PostUpdate()
		{
			CheckStandingOnLiftingPlate();

			if (StandingOnLiftingPlate)
			{
				if (!HasTriggeredLaunch)
				{
					StepTimer++;
					if (Main.rand.NextBool(2))
					{
						Vector2 dustPos = new Vector2(Player.position.X + Main.rand.NextFloat(Player.width), Player.position.Y + Player.height - 4f);
						Dust d = Dust.NewDustPerfect(dustPos, DustID.YellowTorch, new Vector2(0f, -2f), Scale: 1.1f);
						d.noGravity = true;
					}

					if (StepTimer >= 9)
					{
						HasTriggeredLaunch = true;
						Player.velocity.Y = -18f;
						LevitationTimer = 240;

						if (Main.netMode != NetmodeID.Server)
						{
							SoundEngine.PlaySound(SoundID.Item24 with { Pitch = 0.5f }, Player.Center);
							for (int i = 0; i < 20; i++)
							{
								Vector2 vel = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-6f, -12f));
								Dust d = Dust.NewDustPerfect(Player.Bottom, DustID.Cloud, vel, Scale: Main.rand.NextFloat(1f, 1.5f));
								d.noGravity = false;
							}
						}
					}
				}
			}
			else
			{
				StepTimer = 0;
				HasTriggeredLaunch = false;
			}

			if (LevitationTimer > 0)
			{
				LevitationTimer--;

				if (Player.velocity.Y > 0f)
				{
					if (Player.velocity.Y > 2.5f)
					{
						Player.velocity.Y = MathHelper.Lerp(Player.velocity.Y, 2.5f, 0.15f);
					}
					Player.slowFall = true;
					Player.fallStart = (int)(Player.position.Y / 16f);

					if (Main.rand.NextBool(3))
					{
						Vector2 dustPos = new Vector2(Player.position.X + Main.rand.NextFloat(Player.width), Player.position.Y + Player.height);
						Dust d = Dust.NewDustPerfect(dustPos, DustID.YellowTorch, new Vector2(0f, 1f), Scale: 0.9f);
						d.noGravity = true;
					}
				}

				if (Player.velocity.Y == 0f && Player.oldVelocity.Y > 0f)
				{
					LevitationTimer = 0;
				}
			}
		}

		private void CheckStandingOnLiftingPlate()
		{
			int minTileX = (int)(Player.position.X / 16f);
			int maxTileX = (int)((Player.position.X + Player.width) / 16f);
			int plateType = ModContent.TileType<LiftingPlateTile>();

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
							StandingOnLiftingPlate = true;
							return;
						}
					}
				}
			}
		}
	}
}
