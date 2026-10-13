using jarkos.items.bags;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using jarkos.items.materials;
using jarkos.items.weapons.ranged.firing;
using jarkos.items.weapons.melee.other_melee;
using jarkos.items.weapons.magic.staffs;
using Terraria.GameContent.Bestiary;
using ReLogic.Content;
using Jarkos.items.usetiles;
using jarkos.particles;
using System.IO;
using Terraria.DataStructures;
using System.Collections.Generic;
using Terraria.UI;

namespace jarkos.npc.godofdeath
{
	public class GodOfDeathScreenFadeSystem : ModSystem
	{
		public static float ScreenWhiteAlpha = 0f;

		public override void PostUpdateEverything()
		{
			if (!NPC.AnyNPCs(ModContent.NPCType<God_of_death>()) && ScreenWhiteAlpha > 0f)
			{
				ScreenWhiteAlpha = Math.Max(0f, ScreenWhiteAlpha - 0.035f);
			}
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			if (ScreenWhiteAlpha > 0f)
			{
				int mouseIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
				if (mouseIndex != -1)
				{
					layers.Insert(mouseIndex, new LegacyGameInterfaceLayer("Jarkos: GodOfDeathDeathFade", delegate
					{
						Main.spriteBatch.End();
						Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
						Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), Color.White * MathHelper.Clamp(ScreenWhiteAlpha, 0f, 1f));
						Main.spriteBatch.End();
						Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
						return true;
					}, InterfaceScaleType.UI));
				}
			}
		}
	}

	[AutoloadBossHead]
	public class God_of_death : ModNPC
	{
		private int phase = 1;
		private float transitionTimer = 0f;
		private Vector2 dashTargetPos;
		private Vector2 dashStartPos;

		private bool isDying = false;
		private float deathTimer = 0f;

		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[NPC.type] = 3;
			NPCID.Sets.TrailCacheLength[NPC.type] = 10;
			NPCID.Sets.TrailingMode[NPC.type] = 0;
		}

		public override void SetDefaults()
		{
			NPC.aiStyle = -1;
			NPC.width = 70;
			NPC.height = 100;
			NPC.damage = 25;
			NPC.scale = 1.1f;
			NPC.defense = 12;
			NPC.lifeMax = 6500;
			NPC.HitSound = SoundID.NPCHit5;
			NPC.DeathSound = SoundID.NPCDeath6;
			NPC.knockBackResist = 0f;
			NPC.lavaImmune = true;
			NPC.noTileCollide = true;
			NPC.noGravity = true;
			NPC.boss = true;
			NPC.value = Item.buyPrice(0, 8, 0, 0);

			if (Main.netMode != NetmodeID.Server)
			{
				Music = MusicLoader.GetMusicSlot(Mod, "Music/DeathGodTheme");
			}
		}

		public override bool CheckDead()
		{
			if (!isDying)
			{
				isDying = true;
				deathTimer = 0f;
				NPC.life = 1;
				NPC.dontTakeDamage = true;
				NPC.netUpdate = true;
				ClearAllActiveAttacks();

				if (Main.netMode != NetmodeID.Server)
				{
					SoundEngine.PlaySound(new SoundStyle("jarkos/sounds/GoD_DeathImpact") { Volume = 1.2f }, NPC.Center);
				}

				return false;
			}
			return deathTimer >= 300f;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
				new FlavorTextBestiaryInfoElement("...")
			});
		}

		private enum BossState
		{
			TelegraphedSpears = 0,
			EaseOutDashClones = 1,
			LongDashMinionOrbs = 2,
			ShadowBrushStrokes = 3,
			Phase2_TelegraphedSpearCircle = 4,
			Phase2_DashAndOffscreenClones = 5,
			Phase2_TrackingSpearRings = 6,
			Phase2_InkDashSeries = 7
		}

		private BossState State
		{
			get => (BossState)NPC.ai[0];
			set => NPC.ai[0] = (float)value;
		}

		private ref float StateTimer => ref NPC.ai[1];

		private bool CheckPlayerAliveAndTarget()
		{
			if (NPC.target < 0 || NPC.target >= Main.maxPlayers || !Main.player[NPC.target].active || Main.player[NPC.target].dead)
			{
				NPC.TargetClosest(false);
			}

			Player targetPlayer = Main.player[NPC.target];
			if (targetPlayer == null || !targetPlayer.active || targetPlayer.dead)
			{
				bool anyAlive = false;
				for (int i = 0; i < Main.maxPlayers; i++)
				{
					Player p = Main.player[i];
					if (p != null && p.active && !p.dead)
					{
						NPC.target = i;
						anyAlive = true;
						break;
					}
				}

				if (!anyAlive)
				{
					NPC.velocity.Y -= 0.5f;
					NPC.velocity.X *= 0.95f;
					NPC.EncourageDespawn(10);
					if (NPC.timeLeft > 10)
					{
						NPC.timeLeft = 10;
					}

					if (NPC.position.Y < 0 || NPC.timeLeft <= 1)
					{
						NPC.active = false;
						NPC.netUpdate = true;
					}
					return false;
				}
			}

			NPC.timeLeft = 3000;
			return true;
		}

		public override void AI()
		{
			Lighting.AddLight(NPC.Center, 0.4f, 0.2f, 0.5f);

			if (isDying)
			{
				deathTimer++;
				NPC.life = 1;
				NPC.velocity = Vector2.Zero;
				NPC.dontTakeDamage = true;
				ClearAllActiveAttacks();

				if (Main.netMode != NetmodeID.Server && Main.musicFade != null)
				{
					for (int m = 0; m < Main.musicFade.Length; m++)
					{
						Main.musicFade[m] = Math.Min(Main.musicFade[m], 0.35f);
					}
				}

				if (deathTimer >= 210f)
				{
					GodOfDeathScreenFadeSystem.ScreenWhiteAlpha = (deathTimer - 210f) / 90f;
				}
				else
				{
					GodOfDeathScreenFadeSystem.ScreenWhiteAlpha = 0f;
				}

				if (deathTimer >= 300f)
				{
					if (Main.netMode != NetmodeID.Server)
					{
						for (int i = 0; i < 60; i++)
						{
							float angle = i * (MathHelper.TwoPi / 60f);
							Vector2 vel = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * Main.rand.NextFloat(8f, 10f);
							Dust d = Dust.NewDustPerfect(NPC.Center, DustID.Wraith, vel, Scale: Main.rand.NextFloat(1.6f, 3.0f));
							d.noGravity = true;
						}

						for (int i = 0; i < 40; i++)
						{
							float angle = i * (MathHelper.TwoPi / 40f) + Main.rand.NextFloat(-0.08f, 0.08f);
							Vector2 sparkVel = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * Main.rand.NextFloat(10f, 12f);
							Color sparkColor = Color.Lerp(Color.DarkGray, new Color(45, 0, 75), Main.rand.NextFloat());
							new Spark(NPC.Center, sparkVel, sparkColor, 90, Main.rand.NextFloat(1.3f, 2.8f), 1f, 0f).Spawn();
						}

						for (int i = 0; i < 40; i++)
						{
							float angle = i * (MathHelper.TwoPi / 40f) + Main.rand.NextFloat(-0.08f, 0.08f);
							Vector2 gravitVel = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * Main.rand.NextFloat(8f, 10f);
							Color gravitColor = Color.Lerp(Color.DarkGray, new Color(35, 10, 55), Main.rand.NextFloat());
							new Spark_gravit(NPC.Center, gravitVel, gravitColor, 90, Main.rand.NextFloat(1.3f, 2.8f), 1f, 0f).Spawn();
						}

						for (int i = 0; i < 30; i++)
						{
							Vector2 shardVel = Main.rand.NextVector2Circular(8f, 10f);
							new VoidShardParticle(NPC.Center, shardVel, Color.DarkGray, 65, 1.8f, 1.0f).Spawn();
						}
					}

					if (Main.netMode != NetmodeID.MultiplayerClient)
					{
						NPC.dontTakeDamage = false;
						NPC.life = 0;
						NPC.HitEffect(0, 1337.0);
						NPC.NPCLoot();
					}
					NPC.active = false;
				}
				return;
			}

			if (!CheckPlayerAliveAndTarget())
			{
				return;
			}

			Player player = Main.player[NPC.target];

			float healthPercent = NPC.life / (float)NPC.lifeMax;
			if (healthPercent > 0.5f)
			{
				phase = 1;
			}
			else
			{
				if (phase == 1)
				{
					phase = 2;
					transitionTimer = 120f;
					NPC.velocity = Vector2.Zero;
					ClearAllActiveAttacks();
					NPC.netUpdate = true;
					if (Main.netMode != NetmodeID.Server)
					{
						SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
					}
				}
			}

			if (transitionTimer > 0)
			{
				transitionTimer--;
				NPC.velocity = Vector2.Zero;
				if (transitionTimer == 0 && phase == 2)
				{
					ResetState(BossState.Phase2_TelegraphedSpearCircle);
				}
				if (Main.netMode != NetmodeID.Server)
				{
					for (int i = 0; i < 5; i++)
					{
						Vector2 speed = Main.rand.NextVector2Circular(7f, 7f);
						new VoidShardParticle(NPC.Center, speed, Color.Black, 45, 1.3f, 0.9f).Spawn();
					}
				}
				return;
			}

			NPC.spriteDirection = player.Center.X > NPC.Center.X ? 1 : -1;
			StateTimer++;
			ExecuteAttack(player);
		}

		private void ClearAllActiveAttacks()
		{
			int spearType = ModContent.ProjectileType<GodOfDeathSpear>();
			int ringSpearType = ModContent.ProjectileType<GodOfDeathRingSpear>();
			int cloneType = ModContent.ProjectileType<GodOfDeathShadowClone>();
			int brushType = ModContent.ProjectileType<GodOfDeathBrushProj>();
			int blobType = ModContent.ProjectileType<GodOfDeathInkBlob>();
			int spikeType = ModContent.ProjectileType<GodOfDeathSpikeProj>();

			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile proj = Main.projectile[i];
				if (proj.active && (proj.type == spearType || proj.type == ringSpearType || proj.type == cloneType || proj.type == brushType || proj.type == blobType || proj.type == spikeType))
				{
					proj.Kill();
				}
			}

			int minionType = ModContent.NPCType<GodOfDeathMinionOrb>();
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC minion = Main.npc[i];
				if (minion.active && minion.type == minionType)
				{
					minion.active = false;
				}
			}
		}

		private void PlayDashSound(Vector2 position)
		{
			if (Main.netMode == NetmodeID.Server)
				return;

			string[] dashSounds = new string[]
			{
				"jarkos/sounds/GoD_dashone",
				"jarkos/sounds/GoD_dashtwo",
				"jarkos/sounds/GoD_dashthree"
			};

			string chosenSoundPath = dashSounds[Main.rand.Next(dashSounds.Length)];
			float randomPitch = Main.rand.NextFloat(-0.25f, 0.25f);

			SoundStyle soundStyle = new SoundStyle(chosenSoundPath)
			{
				Pitch = randomPitch,
				Volume = 1.0f
			};

			SoundEngine.PlaySound(soundStyle, position);
		}

		private void ResetState(BossState newState)
		{
			State = newState;
			StateTimer = 0f;
			NPC.netUpdate = true;
		}

		private void ExecuteAttack(Player player)
		{
			switch (State)
			{
				case BossState.TelegraphedSpears:
				{
					Vector2 hoverTarget = player.Center + new Vector2(0f, -280f + MathF.Sin(Main.GameUpdateCount * 0.05f) * 40f);
					Vector2 toHover = hoverTarget - NPC.Center;
					NPC.velocity = Vector2.Lerp(NPC.velocity, toHover * 0.05f, 0.1f);

					if (StateTimer < 240 && StateTimer % 10 == 0)
					{
						int spearIndex = (int)(StateTimer / 10) % 8;
						int cycle = (int)(StateTimer / 80);

						float angle = spearIndex * (MathHelper.TwoPi / 8f) + cycle * 0.25f;
						Vector2 spawnPos = player.Center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 880f;

						if (Main.netMode != NetmodeID.MultiplayerClient)
						{
							int proj = Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, Vector2.Zero, ModContent.ProjectileType<GodOfDeathSpear>(), 28, 1f, Main.myPlayer, 0f, player.whoAmI);
							if (proj >= 0 && proj < Main.maxProjectiles)
							{
								Main.projectile[proj].netUpdate = true;
							}
						}
						if (Main.netMode != NetmodeID.Server)
						{
							SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0.4f, Volume = 0.5f }, spawnPos);
						}
					}

					if (StateTimer >= 300)
					{
						SelectNextState();
					}
					break;
				}

				case BossState.EaseOutDashClones:
				{
					if (StateTimer <= 30)
					{
						Vector2 prepPos = player.Center + new Vector2(player.direction * -320f, -200f);
						NPC.velocity = Vector2.Lerp(NPC.velocity, (prepPos - NPC.Center) * 0.1f, 0.15f);

						if (StateTimer == 30)
						{
							dashTargetPos = player.Center;
							dashStartPos = NPC.Center;
							PlayDashSound(NPC.Center);
						}
					}
					else if (StateTimer > 30 && StateTimer <= 80)
					{
						float t = (StateTimer - 30f) / 50f;
						float easeOut = 1f - (1f - t) * (1f - t);
						float speed = MathHelper.Lerp(36f, 3f, easeOut);

						Vector2 dashDir = (dashTargetPos - dashStartPos).SafeNormalize(Vector2.UnitX);
						NPC.velocity = dashDir * speed;

						if (StateTimer % 8 == 0)
						{
							if (Main.netMode != NetmodeID.MultiplayerClient)
							{
								int proj = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<GodOfDeathShadowClone>(), 32, 1f, Main.myPlayer);
								if (proj >= 0 && proj < Main.maxProjectiles)
								{
									Main.projectile[proj].netUpdate = true;
								}
							}
						}
					}
					else if (StateTimer > 80 && StateTimer <= 105)
					{
						Vector2 hoverTarget = player.Center + new Vector2(player.direction * -260f, -180f);
						NPC.velocity = Vector2.Lerp(NPC.velocity, (hoverTarget - NPC.Center) * 0.05f, 0.08f);
					}

					if (StateTimer >= 105)
					{
						SelectNextState();
					}
					break;
				}

				case BossState.LongDashMinionOrbs:
				{
					if (StateTimer <= 30)
					{
						Vector2 prepPos = player.Center + new Vector2(player.direction * -400f, -220f);
						NPC.velocity = Vector2.Lerp(NPC.velocity, (prepPos - NPC.Center) * 0.1f, 0.15f);

						if (StateTimer == 30)
						{
							dashTargetPos = player.Center + new Vector2(player.direction * 600f, 0f);
							dashStartPos = NPC.Center;
							PlayDashSound(NPC.Center);
						}
					}
					else if (StateTimer > 30 && StateTimer <= 130)
					{
						float t = (StateTimer - 30f) / 100f;
						float speed = MathHelper.Lerp(34f, 8f, t);

						Vector2 dashDir = (dashTargetPos - dashStartPos).SafeNormalize(Vector2.UnitX);
						NPC.velocity = dashDir * speed;

						if (StateTimer % 20 == 0)
						{
							if (Main.netMode == NetmodeID.SinglePlayer || Main.netMode == NetmodeID.Server)
							{
								NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<GodOfDeathMinionOrb>());
							}
						}
					}
					else if (StateTimer > 130 && StateTimer < 480)
					{
						Vector2 hoverTarget = player.Center + new Vector2(0f, -320f + MathF.Sin(Main.GameUpdateCount * 0.04f) * 50f);
						Vector2 toHover = hoverTarget - NPC.Center;
						NPC.velocity = Vector2.Lerp(NPC.velocity, toHover * 0.04f, 0.1f);
					}

					if (StateTimer >= 480)
					{
						SelectNextState();
					}
					break;
				}

				case BossState.ShadowBrushStrokes:
				{
					Vector2 hoverTarget = player.Center + new Vector2(player.direction * -300f, -240f);
					Vector2 toHover = hoverTarget - NPC.Center;
					NPC.velocity = Vector2.Lerp(NPC.velocity, toHover * 0.06f, 0.12f);

					if (StateTimer < 180 && StateTimer % 45 == 0)
					{
						Vector2 spawnPos = player.Center + Main.rand.NextVector2CircularEdge(360f, 360f);
						Vector2 targetDir = (player.Center - spawnPos).SafeNormalize(Vector2.UnitY);
						float targetAngle = targetDir.ToRotation();

						if (Main.netMode != NetmodeID.MultiplayerClient)
						{
							int proj = Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, Vector2.Zero, ModContent.ProjectileType<GodOfDeathBrushProj>(), 34, 1f, Main.myPlayer, 0f, targetAngle);
							if (proj >= 0 && proj < Main.maxProjectiles)
							{
								Main.projectile[proj].netUpdate = true;
							}
						}
						if (Main.netMode != NetmodeID.Server)
						{
							SoundEngine.PlaySound(SoundID.Item71 with { Pitch = 0.2f, Volume = 0.8f }, spawnPos);
						}
					}

					if (StateTimer >= 220)
					{
						SelectNextState();
					}
					break;
				}

				case BossState.Phase2_TelegraphedSpearCircle:
				{
					Vector2 hoverTarget = player.Center + new Vector2(0f, -300f + MathF.Sin(Main.GameUpdateCount * 0.06f) * 50f);
					Vector2 toHover = hoverTarget - NPC.Center;
					NPC.velocity = Vector2.Lerp(NPC.velocity, toHover * 0.06f, 0.1f);

					if (StateTimer < 192 && StateTimer % 6 == 0)
					{
						int spearIndex = (int)(StateTimer / 6) % 16;
						float angle = spearIndex * (MathHelper.TwoPi / 16f);
						Vector2 spawnPos = player.Center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 880f;

						if (Main.netMode != NetmodeID.MultiplayerClient)
						{
							int proj = Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, Vector2.Zero, ModContent.ProjectileType<GodOfDeathSpear>(), 32, 1f, Main.myPlayer, 0f, player.whoAmI);
							if (proj >= 0 && proj < Main.maxProjectiles)
							{
								Main.projectile[proj].netUpdate = true;
							}
						}
						if (Main.netMode != NetmodeID.Server)
						{
							SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0.5f, Volume = 0.6f }, spawnPos);
						}
					}

					if (StateTimer >= 260)
					{
						SelectNextState();
					}
					break;
				}

				case BossState.Phase2_DashAndOffscreenClones:
				{
					if (StateTimer <= 30)
					{
						Vector2 prepPos = player.Center + new Vector2(player.direction * -340f, -220f);
						NPC.velocity = Vector2.Lerp(NPC.velocity, (prepPos - NPC.Center) * 0.1f, 0.15f);

						if (StateTimer == 30)
						{
							dashTargetPos = player.Center;
							dashStartPos = NPC.Center;
							PlayDashSound(NPC.Center);
						}
					}
					else if (StateTimer > 30 && StateTimer <= 80)
					{
						float t = (StateTimer - 30f) / 50f;
						float easeOut = 1f - (1f - t) * (1f - t);
						float speed = MathHelper.Lerp(38f, 4f, easeOut);

						Vector2 dashDir = (dashTargetPos - dashStartPos).SafeNormalize(Vector2.UnitX);
						NPC.velocity = dashDir * speed;

						if (StateTimer % 8 == 0)
						{
							if (Main.netMode != NetmodeID.MultiplayerClient)
							{
								int proj = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<GodOfDeathShadowClone>(), 34, 1f, Main.myPlayer, 0f);
								if (proj >= 0 && proj < Main.maxProjectiles)
								{
									Main.projectile[proj].netUpdate = true;
								}
							}
						}
					}
					else if (StateTimer > 80 && StateTimer <= 135)
					{
						Vector2 hoverTarget = player.Center + new Vector2(0f, -260f + MathF.Sin(Main.GameUpdateCount * 0.05f) * 30f);
						NPC.velocity = Vector2.Lerp(NPC.velocity, (hoverTarget - NPC.Center) * 0.05f, 0.08f);
					}

					if (StateTimer >= 30 && StateTimer <= 120 && StateTimer % 9 == 0)
					{
						float offscreenAngle = Main.rand.NextFloat(MathHelper.TwoPi);
						Vector2 spawnPos = player.Center + new Vector2(MathF.Cos(offscreenAngle), MathF.Sin(offscreenAngle)) * 880f;

						if (Main.netMode != NetmodeID.MultiplayerClient)
						{
							int proj = Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, Vector2.Zero, ModContent.ProjectileType<GodOfDeathShadowClone>(), 34, 1f, Main.myPlayer, 1f);
							if (proj >= 0 && proj < Main.maxProjectiles)
							{
								Main.projectile[proj].netUpdate = true;
							}
						}
					}

					if (StateTimer >= 135)
					{
						SelectNextState();
					}
					break;
				}

				case BossState.Phase2_TrackingSpearRings:
				{
					Vector2 hoverTarget = player.Center + new Vector2(0f, -280f + MathF.Sin(Main.GameUpdateCount * 0.05f) * 40f);
					Vector2 toHover = hoverTarget - NPC.Center;
					NPC.velocity = Vector2.Lerp(NPC.velocity, toHover * 0.04f, 0.1f);

					if (StateTimer < 375 && StateTimer % 75 == 0)
					{
						int ringIndex = (int)(StateTimer / 75);
						if (ringIndex < 5)
						{
							float baseAngle = Main.rand.NextFloat(MathHelper.TwoPi);
							for (int i = 0; i < 6; i++)
							{
								float angleOffset = baseAngle + i * (MathHelper.TwoPi / 6f);
								if (Main.netMode != NetmodeID.MultiplayerClient)
								{
									int proj = Projectile.NewProjectile(NPC.GetSource_FromAI(), player.Center, Vector2.Zero, ModContent.ProjectileType<GodOfDeathRingSpear>(), 32, 1f, Main.myPlayer, angleOffset, player.whoAmI);
									if (proj >= 0 && proj < Main.maxProjectiles)
									{
										Main.projectile[proj].netUpdate = true;
									}
								}
							}
							if (Main.netMode != NetmodeID.Server)
							{
								SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0.4f, Volume = 0.8f }, player.Center);
							}
						}
					}

					if (StateTimer >= 450)
					{
						SelectNextState();
					}
					break;
				}

				case BossState.Phase2_InkDashSeries:
				{
					float cycleDuration = 55f;
					float subTimer = StateTimer % cycleDuration;
					int dashIndex = (int)(StateTimer / cycleDuration);

					if (dashIndex < 4)
					{
						if (subTimer <= 12f)
						{
							NPC.velocity = Vector2.Lerp(NPC.velocity, Vector2.Zero, 0.22f);

							if (subTimer == 12f)
							{
								dashTargetPos = player.Center;
								dashStartPos = NPC.Center;
								PlayDashSound(NPC.Center);
							}
						}
						else if (subTimer > 12f && subTimer <= 38f)
						{
							float t = (subTimer - 12f) / 26f;
							float easeOut = 1f - (1f - t) * (1f - t);
							float speed = MathHelper.Lerp(42f, 7f, easeOut);

							Vector2 dashDir = (dashTargetPos - dashStartPos).SafeNormalize(Vector2.UnitX);
							NPC.velocity = dashDir * speed;

							if (subTimer % 3 == 0)
							{
								if (Main.netMode != NetmodeID.MultiplayerClient)
								{
									Vector2 blobVel = dashDir.RotatedByRandom(1.2f) * Main.rand.NextFloat(3f, 10f);
									int proj = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, blobVel, ModContent.ProjectileType<GodOfDeathInkBlob>(), 26, 1f, Main.myPlayer);
									if (proj >= 0 && proj < Main.maxProjectiles)
									{
										Main.projectile[proj].netUpdate = true;
									}
								}
							}
						}
						else if (subTimer > 38f)
						{
							Vector2 hoverTarget = player.Center + new Vector2(player.direction * -240f, -160f);
							NPC.velocity = Vector2.Lerp(NPC.velocity, (hoverTarget - NPC.Center) * 0.05f, 0.08f);
						}
					}

					if (StateTimer >= 220)
					{
						SelectNextState();
					}
					break;
				}
			}
		}

		private void SelectNextState()
		{
			BossState currentState = State;
			BossState nextState = BossState.TelegraphedSpears;
			if (phase == 1)
			{
				switch (currentState)
				{
					case BossState.TelegraphedSpears:
						nextState = BossState.EaseOutDashClones;
						break;
					case BossState.EaseOutDashClones:
						nextState = BossState.LongDashMinionOrbs;
						break;
					case BossState.LongDashMinionOrbs:
						nextState = BossState.ShadowBrushStrokes;
						break;
					case BossState.ShadowBrushStrokes:
						nextState = BossState.TelegraphedSpears;
						break;
					default:
						nextState = BossState.TelegraphedSpears;
						break;
				}
			}
			else
			{
				switch (currentState)
				{
					case BossState.Phase2_TelegraphedSpearCircle:
						nextState = BossState.Phase2_DashAndOffscreenClones;
						break;
					case BossState.Phase2_DashAndOffscreenClones:
						nextState = BossState.Phase2_TrackingSpearRings;
						break;
					case BossState.Phase2_TrackingSpearRings:
						nextState = BossState.Phase2_InkDashSeries;
						break;
					case BossState.Phase2_InkDashSeries:
						nextState = BossState.Phase2_TelegraphedSpearCircle;
						break;
					default:
						nextState = BossState.Phase2_TelegraphedSpearCircle;
						break;
				}
			}
			ResetState(nextState);
		}

		public override bool CheckActive()
		{
			return false;
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(phase);
			writer.Write(transitionTimer);
			writer.WriteVector2(dashTargetPos);
			writer.WriteVector2(dashStartPos);
			writer.Write(isDying);
			writer.Write(deathTimer);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			phase = reader.ReadInt32();
			transitionTimer = reader.ReadSingle();
			dashTargetPos = reader.ReadVector2();
			dashStartPos = reader.ReadVector2();
			isDying = reader.ReadBoolean();
			deathTimer = reader.ReadSingle();
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot)
		{
			npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<Godofdeath_bag>()));
			npcLoot.Add(ItemDropRule.MasterModeCommonDrop(ModContent.ItemType<GoDRelicItem>()));
			if (!Main.expertMode)
			{
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FeatherBlack>(), 1, 4, 12));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Plague_slimemachine>(), 20, 1, 1));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<KnifeSacrifice>(), 4, 1, 1));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Plague_magestaff>(), 4, 1, 1));
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<SpearOfDeath>(), 4, 1, 1));
			}
		}

		public override void BossLoot(ref string name, ref int potionType)
		{
			potionType = ItemID.HealingPotion;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D texture = TextureAssets.Npc[NPC.type].Value;
			int frameHeight = texture.Height / Main.npcFrameCount[NPC.type];

			Rectangle sourceRect = new Rectangle(0, NPC.frame.Y, texture.Width, frameHeight);
			Vector2 origin = sourceRect.Size() / 2f;

			SpriteEffects effects = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

			if (isDying)
			{
				float progress = deathTimer / 300f;
				float shakeAmount = progress * 24f;

				Vector2 shake = Main.rand.NextVector2Circular(shakeAmount, shakeAmount);
				Vector2 drawCenter = NPC.Center + shake - screenPos;

				Color darkTint = Color.Lerp(drawColor, Color.Black, progress);

				for (int i = 0; i < NPC.oldPos.Length; i++)
				{
					Vector2 oldCenter = NPC.oldPos[i] + NPC.Size / 2f + shake;

					float trailProgress = (i + 1) / (float)(NPC.oldPos.Length + 1);
					float trailAlpha = (1f - trailProgress) * 0.45f * (1f - progress * 0.7f);

					Color trailColor = Color.Lerp(new Color(110, 30, 180), Color.Black, progress) * trailAlpha;
					spriteBatch.Draw(texture, oldCenter - screenPos, sourceRect, trailColor, NPC.rotation, origin, NPC.scale, effects, 0f);
				}

				spriteBatch.Draw(texture, drawCenter, sourceRect, darkTint, NPC.rotation, origin, NPC.scale, effects, 0f);

				Color auraColor = new Color(70, 10, 100) * ((1f - progress) * 0.7f);
				spriteBatch.Draw(texture, drawCenter, sourceRect, auraColor, NPC.rotation, origin, NPC.scale * (1f + progress * 0.08f), effects, 0f);

				return false;
			}

			for (int i = 0; i < NPC.oldPos.Length; i++)
			{
				Vector2 oldCenter = NPC.oldPos[i] + NPC.Size / 2f;

				float trailProgress = (i + 1) / (float)(NPC.oldPos.Length + 1);
				float trailAlpha = (1f - trailProgress) * 0.45f * ((255 - NPC.alpha) / 255f);

				Color trailColor = new Color(110, 30, 180) * trailAlpha;
				float scale = NPC.scale * (1f - trailProgress * 0.04f);

				spriteBatch.Draw(texture, oldCenter - screenPos, sourceRect, trailColor, NPC.rotation, origin, scale, effects, 0f);
			}

			Color mainColor = drawColor * ((255 - NPC.alpha) / 255f);
			spriteBatch.Draw(texture, NPC.Center - screenPos, sourceRect, mainColor, NPC.rotation, origin, NPC.scale, effects, 0f);

			return false;
		}

		public override void FindFrame(int frameHeight)
		{
			int startFrame = 0;
			int finalFrame = 2;
			int frameSpeed = 5;
			NPC.frameCounter += 0.5f;

			if (NPC.frameCounter > frameSpeed)
			{
				NPC.frameCounter = 0;
				NPC.frame.Y += frameHeight;
				if (NPC.frame.Y > finalFrame * frameHeight)
				{
					NPC.frame.Y = startFrame * frameHeight;
				}
			}
		}
	}

	#region Minion NPC
	public class GodOfDeathMinionOrb : ModNPC
	{
		public override string Texture => "jarkos/npc/godofdeath/GodOfDeathOrb";

		public override void SetDefaults()
		{
			NPC.netAlways = true;
			NPC.width = 40;
			NPC.height = 40;
			NPC.damage = 22;
			NPC.defense = 5;
			NPC.lifeMax = 250;
			NPC.HitSound = SoundID.NPCHit5;
			NPC.DeathSound = SoundID.NPCDeath6;
			NPC.knockBackResist = 0f;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.value = 0;
		}

		public override void AI()
		{
			NPC.ai[0]++;
			float timer = NPC.ai[0];

			Player player = Main.player[Player.FindClosest(NPC.Center, 1, 1)];
			if (player != null && player.active && !player.dead)
			{
				Vector2 toPlayer = (player.Center - NPC.Center).SafeNormalize(Vector2.Zero);
				NPC.velocity = Vector2.Lerp(NPC.velocity, toPlayer * 1.8f, 0.05f);
			}

			if (timer > 420f)
			{
				float swell = (timer - 420f) / 180f;
				NPC.scale = 1.0f + swell * 0.8f;
			}
			else
			{
				NPC.scale = 1.0f;
			}

			if (Main.rand.NextBool(2))
			{
				Dust d = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Wraith, 0f, 0f, 100, default, 1.2f * NPC.scale);
				d.velocity *= 0.3f;
				d.noGravity = true;
			}

			if (timer % 30 == 0)
			{
				bool verticalCycle = ((int)(timer / 30)) % 2 == 1;

				if (Main.netMode != NetmodeID.Server)
				{
					SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0.5f, Volume = 0.4f }, NPC.Center);
				}

				if (Main.netMode == NetmodeID.SinglePlayer || Main.netMode == NetmodeID.Server)
				{
					if (verticalCycle)
					{
						Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0f, -6.5f), ModContent.ProjectileType<GodOfDeathSpikeProj>(), 20, 1f, Main.myPlayer);
						Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(0f, 6.5f), ModContent.ProjectileType<GodOfDeathSpikeProj>(), 20, 1f, Main.myPlayer);
					}
					else
					{
						Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(-6.5f, 0f), ModContent.ProjectileType<GodOfDeathSpikeProj>(), 20, 1f, Main.myPlayer);
						Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, new Vector2(6.5f, 0f), ModContent.ProjectileType<GodOfDeathSpikeProj>(), 20, 1f, Main.myPlayer);
					}
				}
			}

			if (timer >= 600f)
			{
				ExplodeAndShootSpikes();
				NPC.life = 0;
				NPC.active = false;
			}
		}

		private void ExplodeAndShootSpikes()
		{
			if (Main.netMode != NetmodeID.Server)
			{
				SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.3f, Volume = 0.9f }, NPC.Center);

				for (int i = 0; i < 15; i++)
				{
					Vector2 speed = Main.rand.NextVector2Circular(6f, 6f);
					Dust d = Dust.NewDustPerfect(NPC.Center, DustID.Wraith, speed, Scale: 1.5f);
					d.noGravity = true;
				}
			}

			if (Main.netMode == NetmodeID.SinglePlayer || Main.netMode == NetmodeID.Server)
			{
				for (int i = 0; i < 8; i++)
				{
					float angle = i * (MathHelper.TwoPi / 8f);
					Vector2 spikeVel = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 7.5f;
					Projectile.NewProjectile(NPC.GetSource_Death(), NPC.Center, spikeVel, ModContent.ProjectileType<GodOfDeathSpikeProj>(), 26, 1f, Main.myPlayer);
				}
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Texture2D texture = TextureAssets.Npc[NPC.type].Value;
			Vector2 origin = texture.Size() / 2f;

			float pulse = 1f + MathF.Sin(Main.GameUpdateCount * 0.15f) * 0.08f;
			float finalScale = NPC.scale * pulse;

			Color auraColor = new Color(140, 20, 200) * 0.65f;
			spriteBatch.Draw(texture, NPC.Center - screenPos, null, auraColor, 0f, origin, finalScale * 1.18f, SpriteEffects.None, 0f);
			spriteBatch.Draw(texture, NPC.Center - screenPos, null, Color.Black, 0f, origin, finalScale, SpriteEffects.None, 0f);

			return false;
		}
	}
	#endregion

	#region Projectiles
	public class GodOfDeathSpear : ModProjectile
	{
		public override string Texture => "jarkos/projectiles/melee/other_melee/Spear_death_proj";

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}

		public override void SetDefaults()
		{
			Projectile.netImportant = true;
			Projectile.width = 24;
			Projectile.height = 24;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 300;
			Projectile.penetrate = -1;
		}

		private ref float Timer => ref Projectile.ai[0];
		private ref float TargetPlayerIndex => ref Projectile.ai[1];

		public override void AI()
		{
			Player target = Main.player[(int)TargetPlayerIndex];

			if (Timer < 45f)
			{
				Projectile.velocity = Vector2.Zero;

				if (target != null && target.active)
				{
					Vector2 targetDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
					Projectile.rotation = targetDir.ToRotation() + MathHelper.PiOver4;
				}

				if (Timer % 3 == 0)
				{
					Vector2 aimDir = (Projectile.rotation - MathHelper.PiOver4).ToRotationVector2();
					float dist = Main.rand.NextFloat(60f, 600f);
					Vector2 dustPos = Projectile.Center + aimDir * dist;
					Dust d = Dust.NewDustDirect(dustPos, 0, 0, DustID.Wraith, 0f, 0f, 100, default, 1.2f);
					d.velocity = -aimDir * 2f;
					d.noGravity = true;
				}
			}
			else if (Timer == 45f)
			{
				Vector2 aimDir = (Projectile.rotation - MathHelper.PiOver4).ToRotationVector2();
				Projectile.velocity = aimDir * 24f;

				if (Main.netMode != NetmodeID.Server)
				{
					SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0.2f, Volume = 0.8f }, Projectile.Center);
				}
			}
			else
			{
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

				if (Main.rand.NextBool(2))
				{
					Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Wraith, 0f, 0f, 100, default, 1.2f);
					d.velocity *= 0.2f;
					d.noGravity = true;
				}
			}

			Timer++;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Vector2 origin = texture.Size() / 2f;

			if (Timer < 45f)
			{
				float aimProgress = Timer / 45f;
				float lineAlpha = MathF.Sin(aimProgress * MathF.PI) * 0.85f;

				Vector2 aimDir = (Projectile.rotation - MathHelper.PiOver4).ToRotationVector2();
				Vector2 lineStart = Projectile.Center;
				Vector2 lineEnd = Projectile.Center + aimDir * 1800f;

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

				Color laserColor = new Color(160, 40, 240) * lineAlpha;
				Terraria.Utils.DrawLine(Main.spriteBatch, lineStart, lineEnd, laserColor, laserColor, 4);
				Terraria.Utils.DrawLine(Main.spriteBatch, lineStart, lineEnd, Color.White * lineAlpha, Color.White * lineAlpha, 1);

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			}

			if (Timer >= 45f)
			{
				for (int i = 0; i < Projectile.oldPos.Length; i++)
				{
					Vector2 oldPos = Projectile.oldPos[i] + Projectile.Size / 2f;
					float opacity = 1f - (i / (float)Projectile.oldPos.Length);
					Color color = Color.Purple * opacity * 0.5f;
					Main.EntitySpriteDraw(texture, oldPos - Main.screenPosition, null, color, Projectile.rotation, origin, Projectile.scale * (1f - i * 0.05f), SpriteEffects.None, 0);
				}
			}

			Color drawColor = Timer < 45f ? Color.MediumPurple : Color.Black;
			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, drawColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);

			return false;
		}
	}

	public class GodOfDeathRingSpear : ModProjectile
	{
		public override string Texture => "jarkos/projectiles/melee/other_melee/Spear_death_proj";

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}

		public override void SetDefaults()
		{
			Projectile.netImportant = true;
			Projectile.width = 24;
			Projectile.height = 24;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 260;
			Projectile.penetrate = -1;
		}

		private ref float AngleOffset => ref Projectile.ai[0];
		private ref float TargetPlayerIndex => ref Projectile.ai[1];
		private ref float Timer => ref Projectile.localAI[0];
		private ref float Launched => ref Projectile.localAI[1];

		public override void AI()
		{
			Timer++;

			Player target = Main.player[(int)TargetPlayerIndex];

			if (Timer <= 60f)
			{
				Projectile.velocity = Vector2.Zero;

				if (target != null && target.active && !target.dead)
				{
					float progress = Timer / 60f;
					float easeOut = 1f - MathF.Pow(1f - progress, 3);
					float currentRotation = AngleOffset + easeOut * 2.2f;

					Vector2 offset = currentRotation.ToRotationVector2() * 260f;
					Projectile.Center = target.Center + offset;

					Vector2 inwardDir = -offset.SafeNormalize(Vector2.UnitY);
					Projectile.rotation = inwardDir.ToRotation() + MathHelper.PiOver4;
				}

				if (Timer % 3 == 0)
				{
					Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Wraith, 0f, 0f, 100, default, 1.2f);
					d.velocity *= 0.2f;
					d.noGravity = true;
				}
			}
			else
			{
				if (Launched == 0f)
				{
					Launched = 1f;
					Vector2 flyDir = (Projectile.rotation - MathHelper.PiOver4).ToRotationVector2();
					Projectile.velocity = flyDir * 22f;

					if (Main.netMode != NetmodeID.Server)
					{
						SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0.3f, Volume = 0.7f }, Projectile.Center);
					}
				}

				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;

				if (Main.rand.NextBool(2))
				{
					Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Wraith, 0f, 0f, 100, default, 1.2f);
					d.velocity *= 0.2f;
					d.noGravity = true;
				}
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Vector2 origin = texture.Size() / 2f;

			if (Timer <= 60f)
			{
				float progress = Timer / 60f;
				float alpha = MathF.Sin(progress * MathF.PI) * 0.85f;

				Vector2 aimDir = (Projectile.rotation - MathHelper.PiOver4).ToRotationVector2();
				Vector2 lineStart = Projectile.Center;
				Vector2 lineEnd = Projectile.Center + aimDir * 1200f;

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

				Color laserColor = new Color(160, 40, 240) * alpha;
				Terraria.Utils.DrawLine(Main.spriteBatch, lineStart, lineEnd, laserColor, laserColor, 4);
				Terraria.Utils.DrawLine(Main.spriteBatch, lineStart, lineEnd, Color.White * alpha, Color.White * alpha, 1);

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			}

			if (Timer > 60f)
			{
				for (int i = 0; i < Projectile.oldPos.Length; i++)
				{
					Vector2 oldPos = Projectile.oldPos[i] + Projectile.Size / 2f;
					float opacity = 1f - (i / (float)Projectile.oldPos.Length);
					Color color = Color.Purple * opacity * 0.5f;
					Main.EntitySpriteDraw(texture, oldPos - Main.screenPosition, null, color, Projectile.rotation, origin, Projectile.scale * (1f - i * 0.05f), SpriteEffects.None, 0);
				}
			}

			Color drawColor = Timer <= 60f ? Color.MediumPurple : Color.Black;
			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, drawColor, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);

			return false;
		}
	}

	public class GodOfDeathShadowClone : ModProjectile
	{
		public override string Texture => "jarkos/npc/godofdeath/God_of_death";

		public override void SetStaticDefaults()
		{
			Main.projFrames[Projectile.type] = 3;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}

		public override void SetDefaults()
		{
			Projectile.netImportant = true;
			Projectile.width = 70;
			Projectile.height = 100;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 180;
			Projectile.penetrate = -1;
		}

		private ref float IsOffscreenClone => ref Projectile.ai[0];
		private ref float Timer => ref Projectile.localAI[0];
		private ref float MoveLocked => ref Projectile.localAI[1];

		public override void AI()
		{
			Timer++;

			Player player = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];

			if (Main.rand.NextBool(3))
			{
				Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Wraith, 0f, 0f, 100, default, 1.3f);
				d.velocity *= 0.3f;
				d.noGravity = true;
			}

			if (IsOffscreenClone == 1f)
			{
				if (MoveLocked == 0f && player != null && player.active && !player.dead)
				{
					MoveLocked = 1f;
					Vector2 flyDir = (player.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
					Projectile.velocity = flyDir * 18f;
				}

				if (player != null && player.active && !player.dead)
				{
					float dist = Vector2.Distance(Projectile.Center, player.Center);
					float maxDist = 420f;
					float minDist = 50f;

					if (dist < maxDist)
					{
						float slowFactor = MathHelper.Clamp((dist - minDist) / (maxDist - minDist), 0.2f, 1.0f);
						float targetSpeed = 18f * slowFactor;
						if (Projectile.velocity != Vector2.Zero)
						{
							Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * targetSpeed;
						}
					}
				}
			}
			else
			{
				if (Timer <= 18f)
				{
					if (player != null && player.active && !player.dead)
					{
						Vector2 toPlayer = player.Center - Projectile.Center;
						if (toPlayer != Vector2.Zero)
						{
							Projectile.velocity = Vector2.Lerp(Projectile.velocity, toPlayer.SafeNormalize(Vector2.Zero) * 7f, 0.25f);
						}
					}
				}
				else if (Timer > 18f)
				{
					if (MoveLocked == 0f)
					{
						MoveLocked = 1f;
						Vector2 flyDir = Projectile.velocity != Vector2.Zero ? Projectile.velocity.SafeNormalize(Vector2.UnitX) : Vector2.UnitX;
						if (player != null && player.active && !player.dead)
						{
							flyDir = (player.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
						}
						Projectile.velocity = flyDir * 19f;
					}

					if (player != null && player.active && !player.dead)
					{
						float dist = Vector2.Distance(Projectile.Center, player.Center);
						float maxDist = 420f;
						float minDist = 50f;

						if (dist < maxDist)
						{
							float slowFactor = MathHelper.Clamp((dist - minDist) / (maxDist - minDist), 0.15f, 1.0f);
							float targetSpeed = 19f * slowFactor;
							if (Projectile.velocity != Vector2.Zero)
							{
								Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * targetSpeed;
							}
						}
					}
				}
			}

			Projectile.frameCounter++;
			if (Projectile.frameCounter >= 5)
			{
				Projectile.frameCounter = 0;
				Projectile.frame = (Projectile.frame + 1) % 3;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Npc[ModContent.NPCType<God_of_death>()].Value;
			int frameHeight = texture.Height / Main.projFrames[Projectile.type];
			Rectangle sourceRect = new Rectangle(0, Projectile.frame * frameHeight, texture.Width, frameHeight);
			Vector2 origin = sourceRect.Size() / 2f;

			float opacity = 1f;
			if (Timer < 10f)
				opacity = Timer / 10f;
			else if (Projectile.timeLeft < 50)
				opacity = Projectile.timeLeft / 50f;

			for (int i = 0; i < Projectile.oldPos.Length; i++)
			{
				Vector2 oldPos = Projectile.oldPos[i] + Projectile.Size / 2f;
				float trailProgress = (i + 1) / (float)(Projectile.oldPos.Length + 1);
				float trailOpacity = (1f - trailProgress) * opacity * 0.65f;
				Color trailColor = Color.Black * trailOpacity;
				float trailScale = Projectile.scale * (1f - trailProgress * 0.05f);

				Main.EntitySpriteDraw(texture, oldPos - Main.screenPosition, sourceRect, trailColor, Projectile.rotation, origin, trailScale, SpriteEffects.None, 0);
			}

			Color cloneColor = new Color(100, 30, 160) * (opacity * 0.75f);

			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, sourceRect, cloneColor, Projectile.rotation, origin, Projectile.scale * 1.05f, SpriteEffects.None, 0);
			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, sourceRect, Color.Black * opacity, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);

			return false;
		}
	}

	public class GodOfDeathSpikeProj : ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_0";

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}

		public override void SetDefaults()
		{
			Projectile.netImportant = true;
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 240;
			Projectile.penetrate = -1;
		}

		public override void AI()
		{
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

			if (Main.rand.NextBool(3))
			{
				Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Wraith, 0f, 0f, 100, default, 1.1f);
				d.velocity *= 0.2f;
				d.noGravity = true;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Extra[ExtrasID.ThePerfectGlow].Value;
			Vector2 origin = texture.Size() / 2f;

			for (int i = 0; i < Projectile.oldPos.Length; i++)
			{
				Vector2 oldPos = Projectile.oldPos[i] + Projectile.Size / 2f;
				float opacity = 1f - (i / (float)Projectile.oldPos.Length);
				Color trailColor = new Color(120, 20, 180) * opacity * 0.5f;
				Main.EntitySpriteDraw(texture, oldPos - Main.screenPosition, null, trailColor, Projectile.rotation, origin, new Vector2(0.2f, 0.6f) * (1f - i * 0.08f), SpriteEffects.None, 0);
			}

			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Color.Black, Projectile.rotation, origin, new Vector2(0.25f, 0.7f), SpriteEffects.None, 0);
			return false;
		}
	}

	public class GodOfDeathBrushProj : ModProjectile
	{
		public override string Texture => "jarkos/items/weapons/melee/unique_melee/Dante_sword";

		private static Asset<Effect> riftShader;
		private static bool shaderTried;

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 16;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
		}

		public override void SetDefaults()
		{
			Projectile.netImportant = true;
			Projectile.width = 100;
			Projectile.height = 100;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 65;
			Projectile.penetrate = -1;
		}

		private ref float Timer => ref Projectile.ai[0];
		private ref float TargetAngle => ref Projectile.ai[1];

		public override void AI()
		{
			Timer++;

			float startAngle = TargetAngle - 2.1f;
			float endAngle = TargetAngle + 2.1f;

			if (Timer <= 8f)
			{
				Projectile.rotation = startAngle;
				Projectile.alpha = (int)MathHelper.Lerp(255f, 0f, Timer / 8f);
			}
			else if (Timer > 8f && Timer <= 48f)
			{
				float progress = (Timer - 8f) / 40f;
				float easeOut = 1f - MathF.Pow(1f - progress, 3);
				Projectile.rotation = MathHelper.Lerp(startAngle, endAngle, easeOut);

				Vector2 tipDir = Projectile.rotation.ToRotationVector2();
				Vector2 tipPos = Projectile.Center + tipDir * 120f;

				if (Timer % 3 == 0)
				{
					if (Main.netMode != NetmodeID.MultiplayerClient)
					{
						int blobCount = Main.rand.Next(2, 5);
						for (int b = 0; b < blobCount; b++)
						{
							Vector2 blobVel = tipDir.RotatedByRandom(1.4f) * Main.rand.NextFloat(4f, 15f);
							int proj = Projectile.NewProjectile(Projectile.GetSource_FromAI(), tipPos, blobVel, ModContent.ProjectileType<GodOfDeathInkBlob>(), 24, 1f, Main.myPlayer);
							if (proj >= 0 && proj < Main.maxProjectiles)
							{
								Main.projectile[proj].netUpdate = true;
							}
						}
					}
				}

				for (int i = 0; i < 3; i++)
				{
					Dust d = Dust.NewDustPerfect(tipPos + Main.rand.NextVector2Circular(10f, 10f), DustID.Wraith, tipDir * 3.5f, Scale: 1.8f);
					d.noGravity = true;
				}
			}
			else
			{
				Projectile.alpha = (int)MathHelper.Lerp(0f, 255f, (Timer - 48f) / 17f);
			}
		}

		private static bool TryGetShader(out Effect fx)
		{
			if (!shaderTried)
			{
				shaderTried = true;
				try
				{
					riftShader = ModContent.Request<Effect>("jarkos/Shaders/RiftSlash", AssetRequestMode.ImmediateLoad);
				}
				catch { riftShader = null; }
			}
			fx = riftShader != null ? riftShader.Value : null;
			return fx != null;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D bladeTex = TextureAssets.Projectile[Projectile.type].Value;
			Vector2 origin = new Vector2(6f, bladeTex.Height - 6f);
			float opacity = (255 - Projectile.alpha) / 255f;
			float bladeLen = 140f;
			float texDiag = MathF.Sqrt(bladeTex.Width * bladeTex.Width + bladeTex.Height * bladeTex.Height);
			float scale = bladeLen / texDiag * 1.1f;

			if (TryGetShader(out Effect fx))
			{
				Texture2D noise = ModContent.Request<Texture2D>("jarkos/projectiles/melee/true_melee/RiftNoise").Value;
				float quadHalf = bladeLen * 1.6f;
				float quadScale = quadHalf * 2f / noise.Width;
				Vector2 lead = Projectile.rotation.ToRotationVector2();

				fx.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
				fx.Parameters["uLead"]?.SetValue(new Vector3(lead.X, lead.Y, 1f));
				fx.Parameters["uSpan"]?.SetValue(3.8f);
				fx.Parameters["uR0"]?.SetValue(1.1f);
				fx.Parameters["uR1"]?.SetValue(0.7f);
				fx.Parameters["uInner"]?.SetValue(30f / quadHalf);
				fx.Parameters["uStretch"]?.SetValue(1.2f);
				fx.Parameters["uFinisher"]?.SetValue(1f);
				fx.Parameters["uFade"]?.SetValue(opacity * 0.95f);
				fx.Parameters["uColorEdge"]?.SetValue(new Vector3(0.5f, 0.1f, 0.8f));
				fx.Parameters["uColorMid"]?.SetValue(new Vector3(0.3f, 0.05f, 0.4f));
				fx.Parameters["uColorCore"]?.SetValue(new Vector3(0.05f, 0.0f, 0.1f));
				fx.Parameters["uNoiseTexture"]?.SetValue(noise);

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, fx, Main.GameViewMatrix.TransformationMatrix);
				Main.spriteBatch.Draw(noise, Projectile.Center - Main.screenPosition, null, Color.White, 0f, noise.Size() * 0.5f, quadScale, SpriteEffects.None, 0f);
				Main.spriteBatch.End();
				Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			}

			for (int i = 0; i < Projectile.oldPos.Length; i++)
			{
				Vector2 oldCenter = Projectile.oldPos[i] + Projectile.Size / 2f;
				float progress = (i + 1) / (float)(Projectile.oldPos.Length + 1);
				float trailAlpha = (1f - progress) * opacity * 0.7f;
				Color trailColor = new Color(80, 15, 140) * trailAlpha;

				Main.EntitySpriteDraw(bladeTex, oldCenter - Main.screenPosition, null, trailColor, Projectile.oldRot[i] + MathHelper.PiOver4, origin, scale * (1.05f - progress * 0.1f), SpriteEffects.None, 0);
			}

			Color drawColor = Color.Lerp(Color.Black, new Color(70, 20, 110), 0.3f) * opacity;
			Main.EntitySpriteDraw(bladeTex, Projectile.Center - Main.screenPosition, null, drawColor, Projectile.rotation + MathHelper.PiOver4, origin, scale, SpriteEffects.None, 0);

			return false;
		}
	}

	public class GodOfDeathInkBlob : ModProjectile
	{
		public override string Texture => "jarkos/npc/godofdeath/BrushCrop";

		public override void SetDefaults()
		{
			Projectile.netImportant = true;
			Projectile.width = 22;
			Projectile.height = 22;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 140;
			Projectile.penetrate = -1;
		}

		private ref float Timer => ref Projectile.localAI[0];
		private ref float TargetFallSpeed => ref Projectile.localAI[1];

		public override void OnSpawn(IEntitySource source)
		{
			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				Projectile.scale = Main.rand.NextFloat(0.5f, 2.2f);
				TargetFallSpeed = Main.rand.NextFloat(0.8f, 4.8f);
				Projectile.netUpdate = true;
			}
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(TargetFallSpeed);
			writer.Write(Projectile.scale);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			TargetFallSpeed = reader.ReadSingle();
			Projectile.scale = reader.ReadSingle();
		}

		public override void AI()
		{
			Timer++;
			Projectile.velocity.X *= 0.98f;
			Projectile.velocity.Y = MathHelper.Lerp(Projectile.velocity.Y, TargetFallSpeed, 0.04f);

			if (Main.rand.NextBool(4))
			{
				Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Wraith, 0f, 1f, 100, default, 1.1f * Projectile.scale);
				d.velocity *= 0.2f;
				d.noGravity = true;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			Vector2 origin = texture.Size() / 2f;

			float opacity = 1f;
			if (Timer < 10f)
				opacity = Timer / 10f;
				
			else if (Projectile.timeLeft < 40)
				opacity = Projectile.timeLeft / 40f;

			float blobScale = Projectile.scale * 0.75f;
			Color inkGlow = new Color(140, 20, 210) * (opacity * 0.65f);

			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, inkGlow, 0f, origin, blobScale * 1.15f, SpriteEffects.None, 0);
			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, Color.Black * opacity, 0f, origin, blobScale, SpriteEffects.None, 0);

			return false;
		}
	}
	#endregion
}
